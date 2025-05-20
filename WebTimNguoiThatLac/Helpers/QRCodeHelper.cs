using QRCoder;


namespace WebTimNguoiThatLac.Helpers
{

    public class QRCodeHelper
    {
        public static byte[] GenerateQRCode(string text)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20); // Số 20 là độ lớn pixel mỗi ô vuông
        }
    }

}
