using QRCoder;

namespace QR_generator.Services;

public interface IQrCodeService
{
    byte[] Generate(string text);
}

public class QrCodeService : IQrCodeService
{
    public byte[] Generate(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(20); // 20 pixels per module
    }
}