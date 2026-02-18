using System.Security.Cryptography;
using System.Text;

namespace Shared.HMAC;

public class HmacService
{
    private readonly string _secretKey;
    private const string SignatureHeader = "X-HMAC-Signature";
    private const string TimestampHeader = "X-HMAC-Timestamp";
    private const int MaxTimestampAgeSeconds = 300; // 5 minutes

    public HmacService(string secretKey)
    {
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
    }

    public string ComputeSignature(string requestBody, string timestamp)
    {
        var message = $"{timestamp}:{requestBody}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    public bool ValidateSignature(string requestBody, string signature, string timestamp)
    {
        // Check timestamp to prevent replay attacks
        if (!IsTimestampValid(timestamp))
            return false;

        var expectedSignature = ComputeSignature(requestBody, timestamp);
        return signature == expectedSignature;
    }

    public string GetTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }

    private bool IsTimestampValid(string timestamp)
    {
        if (!long.TryParse(timestamp, out var timestampSeconds))
            return false;

        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var age = Math.Abs(currentTimestamp - timestampSeconds);
        return age <= MaxTimestampAgeSeconds;
    }

    public static class Headers
    {
        public const string Signature = SignatureHeader;
        public const string Timestamp = TimestampHeader;
    }
}
