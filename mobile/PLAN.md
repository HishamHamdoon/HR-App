# Flutter Mobile App — Implementation Plan

Mobile client for HR-App, consuming the existing `Emp.Api` REST API.

## Contract realities

Every claim below was read directly from the source and re-verified on 2026-07-17. Do not add an item
here from a summary, a search hit, or a secondhand report without opening the surrounding code — an
earlier revision of this plan asserted two bugs that did not exist (see "Withdrawn claims") because a
single line was read out of context.

- **`AddControllers()` has no JSON configuration** (`Emp.Api/Program.cs:88`), so the API serializes with
  System.Text.Json defaults — camelCase. Flutter models bind `isSuccess`, `filePath`, `employeeName`.
  Worth pinning explicitly rather than depending on a default.
- **Codegen from Swagger won't work.** `ResponseDto.Result` is `object?`, and the highest-value endpoints
  (`balance/{employeeId}`, `notifications/mine`, `dashboard-counts`) return anonymous types. Swagger emits
  these as untyped blobs, so `openapi-generator` / `swagger_parser` would produce `dynamic` everywhere.
  Models are hand-written.
- **Leave attachments are publicly readable and enumerable.** `Program.cs:185` `UseStaticFiles()` serves
  `wwwroot/uploads/leaves/`, and `LeavesController.cs:274` names each file after the leave ID
  (`1.pdf`, `1035.pdf`, `1036.pdf` are all present on disk). Anyone who can reach the API can walk
  `/uploads/leaves/{n}.pdf` and harvest every attachment in the system — including the medical
  certificates the app's own `IsAttachmentRequired` flag exists to collect. No authentication involved.
  The GUID-named files from the unused `IFileService` path are at least unguessable; the ID-named ones
  are not. This is the most serious defect found. See **A3**.
- **"My payslips" is not currently possible.** `SalaryController.cs:12` and `PayrollsController.cs:13`
  are both class-level `[Authorize(Roles = "Admin")]`. An employee cannot read their own salary. The
  feature needs new endpoints or it drops from scope.

### Withdrawn claims

Asserted in the first revision, disproved on inspection. Recorded so they are not "rediscovered":

- **`PATCH api/Leaves/{id}/decision` defaults to rejection** — **false.** `LeavesController.cs:602-608`
  already rejects any `status` that isn't exactly `Approved` or `Rejected` before the ternary at line 637
  is reached. Line 637 was read without its guard.
- **Duplicate `[HttpPatch("terminate/{id}")]` at `EmployeeController.cs:701`** — **false.** Line 485 is
  `//`-commented and line 701 sits inside a `/* */` block spanning lines 700-759. Only line 510 is live;
  there is no ambiguous-match risk.

## Scope

Employee + manager self-service. Admin CRUD stays on EMP.Web.

| Area | Screens | Existing endpoints |
|---|---|---|
| Auth | Login, forced password change, logout | `POST api/Auth/login`, `POST api/Auth/change-password` |
| Profile | View, edit (phone/address only) | `GET api/Employee/{id}`, `PUT api/Employee/my-profile` |
| Leave | Balance, my history, request w/ attachment, detail | `GET api/Leaves/balance/{id}`, `get-leaves-by-employeeId/{id}`, `POST api/Leaves`, `GET api/LeaveTypes` |
| Manager | Team leaves, approve/reject, my team | `get-leaves-by-managerId/{id}`, `PATCH api/Leaves/{id}/decision`, `GET api/Employee/by-manager/{id}` |
| Notifications | List, badge, mark all read | `GET api/Notifications/mine`, `POST mark-all-read` |
| Payslips | My salary + payroll history | **none — must be built** |
| Settings | Theme, language (en/ar), calendar (Gregorian/Hijri) | `POST api/Auth/preferences` |

Manager UI gates on the `IsManager` JWT claim, matching how the API derives it (owns ≥1 department) —
not on a role, since only `Admin` and `Employee` exist.

## Workstream A — API changes

Prerequisites. The Flutter app can't be correct without them.

**A1. — withdrawn.** The decision endpoint already validates its input; see "Withdrawn claims" above.
Label retained so A2–A7 references stay stable.

**A2. Stop storing absolute URLs in `Leave.FilePath`.** `LeavesController.cs:281` builds the URL from
`HttpContext.Request.Host`, so a leave filed from the web on `localhost:7031` stores a `localhost` URL
that no phone can resolve. Store the relative path (`/uploads/leaves/{file}`); let each client resolve
against its own base URL. Needs a migration to rewrite existing rows, and the
`"https://placeholde.co/600x400"` fallback (typo'd domain) should become `null`.

**A3. Attachments must be auth-protected (highest severity).** Not merely "anyone with the URL" —
filenames are the leave ID, so the whole set is enumerable by anyone who can reach the API (see Contract
realities). Add `GET api/Leaves/{id}/attachment` that checks the caller is the owner, the approver, or
Admin, and stop serving `uploads/leaves` via `UseStaticFiles`. Mobile uses the endpoint exclusively.
Coupled to A2 and to EMP.Web, whose Razor views read `FilePath` directly — all three move together.

**A4. Employee payslip endpoints.** `GET api/Salary/mine` and `GET api/Payrolls/mine` scoped to the
caller's `EmployeeId` claim. Without these, payslips leave scope.

**A5. Refresh tokens.** The JWT is hard-coded to 7 days (`AddDays(7)`, with `ExpiryMinutes` commented
out). On mobile that means a silent logout every 7 days and a stolen token valid for a week. Add a
refresh token table + `POST api/Auth/refresh`; shorten access tokens to ~30–60 min. The largest API item
— if deferred, the app ships with 7-day sessions and forced re-login, which is tolerable for a pilot but
not for release.

**A6. Push notifications.** Notifications are in-app rows only. Add a device-token table,
`POST api/Notifications/register-device`, and fire FCM/APNs alongside the existing `Notifications.Add(...)`
calls in `LeavesController`. Also: `Notification.Url` stores web routes like `/Leaves/TeamLeaves` —
mobile needs a route-mapping layer or, better, structured `{type, entityId}` fields.

**A7. Housekeeping.** Delete `POST api/Leaves/TestUpload` (live debug endpoint, `LeavesController.cs:762`);
pin `AddControllers().AddJsonOptions(camelCase)` (`Program.cs:88` currently relies on the default); fix
`Cors:AllowedOrigins` — `appsettings.json:22-23` allows `7163`/`5163` but EMP.Web runs on `7026`/`5261`,
so the list matches nothing — and add mobile origins.

## Workstream B — Flutter app

**Stack:** Flutter 3.x, Riverpod (state), Dio (HTTP + interceptors), `flutter_secure_storage` (token —
Keychain/Keystore, never SharedPreferences), `go_router`, `freezed` + `json_serializable` (hand-written
models), `easy_localization` or ARB/`intl`.

**Layout** (`/mobile`, feature-first):

```
lib/
  core/        api_client, interceptors, response_envelope, error, storage, router, theme
  features/    auth/ profile/ leave/ team/ notifications/ payslip/ settings/
                 └─ data/ (dto, repository)  domain/  presentation/
  l10n/        app_en.arb, app_ar.arb
```

**`ResponseDto` envelope.** Every call unwraps a generic `ApiResponse<T>` with
`{result, isSuccess, message}`. Critical wrinkle: the API is **inconsistent** — `AuthController.Register`
returns `BadRequest(ex.Message)` (raw string), while others return a `ResponseDto`. Some endpoints return
HTTP 200 with `isSuccess: false`. So the envelope parser must handle: 200+`isSuccess:true`,
200+`isSuccess:false` (business failure — surface `message`), non-200+JSON envelope, and non-200+raw
string. Getting this one class right removes most error-handling pain downstream.

**Auth interceptor.** Attach bearer, decode claims (`EmployeeId`, `IsManager`, `MustChangePassword`,
`PreferredTheme`, `Calendar`, `Lang`), on 401 refresh-and-retry once (post-A5) with a mutex so concurrent
401s don't stampede. `MustChangePassword == "true"` → `go_router` redirect that pins the user to the
change-password screen, mirroring `LicenseEnforcementMiddleware`.

**Client-side leave validation** mirrors the server so users get instant feedback — end ≥ start; half-day
must start and end the same day; per-type `MinDays`/`MaxDays`; attachment required when
`IsAttachmentRequired`. The server stays the authority; the client only avoids a round-trip. Request
submits as `multipart/form-data` (`CreateLeaveDto` takes `IFormFile? Attachment`). Note `ManagerId` in the
DTO is ignored — the server resolves the approver by walking the department tree.

**Localization** is not an afterthought: EMP.Web already ships en + ar with RTL and Hijri. Ship
`app_en.arb`/`app_ar.arb` from day one, honor the `Lang`/`Calendar` claims, and use `Directionality` +
logical padding (`EdgeInsetsDirectional`) throughout. Retrofitting RTL is far more expensive than building
with it.

## Phases

1. **API prerequisites** — A3 + A2 first (one change: authorized attachment endpoint, relative paths,
   migration for existing rows, EMP.Web views updated, static serving removed). This closes a live data
   exposure and is not API-only — it touches EMP.Web. Then A7's independent leftovers.
2. **Flutter foundation** — project scaffold, Dio + envelope + interceptors, secure storage, router, theme,
   l10n skeleton, CI job in `.github/workflows/ci.yml`.
3. **Auth + profile** — login, forced password change, profile view/edit. First end-to-end vertical slice;
   proves the envelope and claims work.
4. **Leave (employee)** — balance, history, request with attachment. The core value.
5. **Manager** — team leaves, approve/reject (depends on A1), my team.
6. **Notifications** — in-app list + badge first; push (A6) after.
7. **Payslips** — depends on A4.
8. **Refresh tokens** (A5) — API + client, before any real rollout.
9. **Hardening** — certificate pinning, jailbreak/root posture, release signing, store metadata.

Phases 3–7 each land as a working app you can put on a device.

## Risks

The two that will bite hardest:

- **The untyped `object? Result` envelope** means every contract change is a silent runtime break in
  Flutter with no compile-time signal. Mitigate with a contract test suite hitting a running API.
- **`https://localhost:7031` is unreachable from a physical device or the Android emulator** (which needs
  `10.0.2.2`). Make the base URL a `--dart-define` build config with dev/staging/prod flavors from the start.

Also worth knowing: `PayrollJobService : BackgroundService` is **never registered** — there's no
`AddHostedService` call anywhere in the repo, so monthly payroll auto-generation has never actually run.
That doesn't block mobile, but if payslips are in scope, the data they read may be sparser than expected.
