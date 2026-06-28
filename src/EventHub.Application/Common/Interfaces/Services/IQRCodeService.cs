namespace EventHub.Application.Common.Interfaces.Services;

public interface IQRCodeService
{
    string GenerateQrData(Guid ticketId, Guid eventId, Guid userId);
    bool ValidateQrData(string qrData);
    (Guid ticketId, Guid eventId, Guid userId)? DecodeQrData(string qrData);
}