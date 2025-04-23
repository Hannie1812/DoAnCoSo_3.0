namespace WebTimNguoiThatLac.BoTro
{
    public class Filter
    {
        private static readonly string strCheck = "áàạảãâấầậẩẫăắằặẳẵÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴéèẹẻẽêếềệểễÉÈẸẺẼÊẾỀỆỂỄ" +
            "óòọỏõôốồộổỗơớờợởỡÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠúùụủũưứừựửữÚÙỤỦŨƯỨỪỰỬỮíìịỉĩÍÌỊỈĨđĐýỳỵỷỹÝỲỴỶỸ~!@#$%^&*()-[{]}|\\/'\"\\.,><;:";

        private static readonly string[] VietNamChar = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

        public static string FilterChar(string str)
        {
            str = str.Trim();
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            str = str.Replace(" ", "-");
            str = str.Replace("--", "-");
            str = str.Replace("?", "");
            str = str.Replace("&", "");
            str = str.Replace(",", "");
            str = str.Replace(":", "");
            str = str.Replace("!", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            str = str.Replace("%", "");
            str = str.Replace("#", "");
            str = str.Replace("$", "");
            str = str.Replace("*", "");
            str = str.Replace("`", "");
            str = str.Replace("~", "");
            str = str.Replace("@", "");
            str = str.Replace("^", "");
            str = str.Replace(".", "");
            str = str.Replace("/", "");
            str = str.Replace(">", "");
            str = str.Replace("<", "");
            str = str.Replace("[", "");
            str = str.Replace("]", "");
            str = str.Replace(";", "");
            str = str.Replace("+", "");
            return str.ToLower();
        }

        public static string ChuyenCoDauThanhKhongDau(string str)
        {
            str = str.Trim();
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            //str = str.Replace(" ", "-");
            str = str.Replace("--", "-");
            str = str.Replace("?", "");
            str = str.Replace("&", "");
            str = str.Replace(",", "");
            str = str.Replace(":", "");
            str = str.Replace("!", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            str = str.Replace("%", "");
            str = str.Replace("#", "");
            str = str.Replace("$", "");
            str = str.Replace("*", "");
            str = str.Replace("`", "");
            str = str.Replace("~", "");
            str = str.Replace("@", "");
            str = str.Replace("^", "");
            str = str.Replace(".", "");
            str = str.Replace("/", "");
            str = str.Replace(">", "");
            str = str.Replace("<", "");
            str = str.Replace("[", "");
            str = str.Replace("]", "");
            str = str.Replace(";", "");
            str = str.Replace("+", "");
            return str.ToLower();
        }

        // Mã hóa CCCD bằng AES
        public static string EncryptCCCD(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;
            var key = "WebTimNguoiThatLacCCCDKey1234"; // 24 ký tự cho AES-192
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // IV mặc định là 0
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    using (var sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    var encrypted = ms.ToArray();
                    return System.Convert.ToBase64String(encrypted);
                }
            }
        }

        // Giải mã CCCD bằng AES
        public static string DecryptCCCD(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return null;
            var key = "WebTimNguoiThatLacCCCDKey1234";
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var buffer = System.Convert.FromBase64String(cipherText);
                using (var ms = new System.IO.MemoryStream(buffer))
                using (var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                using (var sr = new System.IO.StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

}
