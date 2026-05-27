using System.Text;
using Emp.Models.Licensing;

// Offline vendor tool: mints a signed license key that the product verifies with the
// same Licensing:Secret. Run this where the secret is held — never ship it to customers.
//
// Usage:
//   licensegen --secret <SECRET> [--type Yearly|Trial] [--months 12] [--expiry yyyy-MM-dd] [--verify <key>]
// The secret may also come from the LICENSING_SECRET environment variable.

var options = ParseArgs(args);

var secret = options.GetValueOrDefault("secret")
             ?? Environment.GetEnvironmentVariable("LICENSING_SECRET");

if (string.IsNullOrWhiteSpace(secret))
{
    Console.Error.WriteLine("Error: a secret is required (--secret <value> or LICENSING_SECRET env var).");
    PrintUsage();
    return 1;
}

var secretBytes = Encoding.UTF8.GetBytes(secret);

// Verify mode: check an existing key and print what it grants.
if (options.TryGetValue("verify", out var keyToVerify) && !string.IsNullOrWhiteSpace(keyToVerify))
{
    if (LicenseKeyCodec.TryVerify(keyToVerify, secretBytes, out var vType, out var vExpiry))
    {
        var valid = vExpiry.Date >= DateTime.Now.Date;
        Console.WriteLine($"VALID signature");
        Console.WriteLine($"  Type    : {vType}");
        Console.WriteLine($"  Expires : {vExpiry:yyyy-MM-dd}");
        Console.WriteLine($"  Status  : {(valid ? "active" : "EXPIRED")}");
        return valid ? 0 : 2;
    }
    Console.Error.WriteLine("INVALID: signature does not match this secret (or key is malformed).");
    return 1;
}

// Generate mode.
var type = options.GetValueOrDefault("type") ?? "Yearly";
type = type.Equals("Trial", StringComparison.OrdinalIgnoreCase) ? "Trial" : "Yearly";

DateTime expiry;
if (options.TryGetValue("expiry", out var expiryArg) && DateTime.TryParse(expiryArg, out var parsedExpiry))
{
    expiry = parsedExpiry.Date;
}
else
{
    var months = 12;
    if (options.TryGetValue("months", out var monthsArg) && int.TryParse(monthsArg, out var parsedMonths) && parsedMonths > 0)
    {
        months = parsedMonths;
    }
    expiry = DateTime.Now.Date.AddMonths(months);
}

var key = LicenseKeyCodec.Generate(type, expiry, secretBytes);

Console.WriteLine($"Type    : {type}");
Console.WriteLine($"Expires : {expiry:yyyy-MM-dd}");
Console.WriteLine();
Console.WriteLine("License key:");
Console.WriteLine(key);
return 0;

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--")) continue;
        var name = args[i][2..];
        var value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : "true";
        result[name] = value;
    }
    return result;
}

static void PrintUsage()
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  licensegen --secret <SECRET> [--type Yearly|Trial] [--months 12]");
    Console.Error.WriteLine("  licensegen --secret <SECRET> --expiry 2027-05-26");
    Console.Error.WriteLine("  licensegen --secret <SECRET> --verify <existing-key>");
    Console.Error.WriteLine();
    Console.Error.WriteLine("The secret may also be supplied via the LICENSING_SECRET environment variable.");
}
