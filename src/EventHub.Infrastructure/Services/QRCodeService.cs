using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EventHub.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace EventHub.Infrastructure.Services;

public sealed class QRCodeSettings
{
    public string Secret { get; set; } = string.Empty;
}

public sealed class QRCodeService : IQRCodeService
{
    private readonly byte[] _secret;

    public QRCodeService(IOptions<QRCodeSettings> options)
    {
        _secret = Encoding.UTF8.GetBytes(options.Value.Secret);
    }

    public string GenerateQrData(Guid ticketId, Guid eventId, Guid userId)
    {
        var payload = new QrPayload(ticketId, eventId, userId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        var signature = ComputeSignature(json);
        return $"{Convert.ToBase64String(json)}.{Convert.ToBase64String(signature)}";
    }

    public bool ValidateQrData(string qrData) => DecodeQrData(qrData) is not null;

    public (Guid ticketId, Guid eventId, Guid userId)? DecodeQrData(string qrData)
    {
        if (string.IsNullOrEmpty(qrData)) return null;
        var parts = qrData.Split('.', 2);
        if (parts.Length != 2) return null;

        byte[] payloadBytes, signatureBytes;
        try
        {
            payloadBytes = Convert.FromBase64String(parts[0]);
            signatureBytes = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return null;
        }

        var expected = ComputeSignature(payloadBytes);
        if (!CryptographicOperations.FixedTimeEquals(expected, signatureBytes))
            return null;

        try
        {
            var payload = JsonSerializer.Deserialize<QrPayload>(payloadBytes);
            if (payload is null) return null;
            return (payload.TicketId, payload.EventId, payload.UserId);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private byte[] ComputeSignature(byte[] payload)
    {
        using var hmac = new HMACSHA256(_secret);
        return hmac.ComputeHash(payload);
    }

    private sealed record QrPayload(Guid TicketId, Guid EventId, Guid UserId, long CreatedAtUnix);
}
