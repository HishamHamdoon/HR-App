import 'dart:convert';

/// The envelope almost every Emp.Api endpoint returns: `{ result, isSuccess, message }`.
///
/// `result` is `object?` on the server, so it arrives untyped and each caller decodes
/// its own shape.
class ApiEnvelope {
  const ApiEnvelope({required this.isSuccess, this.result, this.message = ''});

  final bool isSuccess;
  final Object? result;
  final String message;
}

/// Thrown for any call that did not come back as a successful envelope.
class ApiException implements Exception {
  const ApiException(this.message, {this.statusCode});

  final String message;
  final int? statusCode;

  @override
  String toString() => 'ApiException(${statusCode ?? '-'}): $message';
}

/// Decodes an Emp.Api response body.
///
/// The API is not consistent about how it reports failure, so this has to cope with
/// four shapes:
///
///  * 2xx + `{isSuccess: true}`  — success; `result` holds the payload.
///  * 2xx + `{isSuccess: false}` — a business failure that still returned HTTP 200.
///    Most controllers do this: they catch, set `IsSuccess = false`, and return the
///    envelope. Treating 200 as success would silently swallow these.
///  * non-2xx + envelope JSON    — failure carrying `message`.
///  * non-2xx + a bare string    — failure where the body *is* the message.
///    `AuthController.Register` returns `BadRequest(ex.Message)`, not an envelope.
///
/// A body that is neither (a raw `bool` from `PayrollsController.PaySalary`, an empty
/// 204, an HTML error page) falls back to the HTTP status.
ApiEnvelope parseEnvelope(int? statusCode, Object? data) {
  final decoded = data is String ? _tryDecodeJson(data) : data;

  if (decoded is Map && decoded.containsKey('isSuccess')) {
    return ApiEnvelope(
      isSuccess: decoded['isSuccess'] == true,
      result: decoded['result'],
      message: decoded['message'] is String ? decoded['message'] as String : '',
    );
  }

  final ok = statusCode != null && statusCode >= 200 && statusCode < 300;
  final text = decoded is String ? decoded : '';

  return ApiEnvelope(
    isSuccess: ok,
    result: ok ? decoded : null,
    message: ok
        ? ''
        : text.trim().isNotEmpty
        ? text.trim()
        : 'Request failed${statusCode == null ? '' : ' ($statusCode)'}.',
  );
}

Object? _tryDecodeJson(String raw) {
  if (raw.isEmpty) return null;
  try {
    return jsonDecode(raw);
  } on FormatException {
    // A bare, unquoted string body — the Register error path.
    return raw;
  }
}
