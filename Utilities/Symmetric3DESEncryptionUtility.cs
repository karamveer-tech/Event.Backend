using System.Security.Cryptography;
using System.Text;

namespace eventManager.Utilities
{
    public class Symmetric3DESEncryptionUtility
    {
        private static readonly string SECURITY_KEY = "ki90P6bdGP4hZhx21PmpsaA01w";

        /// <summary>
        /// Returns the encryoted value of the parameter using 3DES
        /// symmetric cryptography algorithms.
        /// </summary>
        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            // Get the key from config file
            // todo: add exception handling if the configuration is NOT available
            // string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));
            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                keyArray = SHA256.HashData(UTF8Encoding.UTF8.GetBytes(SECURITY_KEY));
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(SECURITY_KEY);
            }

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = keyArray;
            aesAlg.IV = new byte[16];  // 128-bit IV (default to all zeroes for simplicity, but you should use a random IV)

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            using ICryptoTransform encryptor = aesAlg.CreateEncryptor();
            byte[] resultArray = PerformCryptography(toEncryptArray, encryptor);
            return Convert.ToBase64String(resultArray);
        }

        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            if (string.IsNullOrEmpty(cipherString)) return string.Empty;

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            // If hashing is required, use SHA-256 instead of MD5 (more secure)
            if (useHashing)
            {
                using SHA256 sha256 = SHA256.Create();
                keyArray = sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(SECURITY_KEY));
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(SECURITY_KEY);
            }

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = keyArray;
            aesAlg.IV = new byte[16];  // Use the same IV for decryption (in real applications, you should store and retrieve the IV properly)

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            byte[] resultArray = PerformCryptography(toEncryptArray, decryptor);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        // Helper method to perform encryption or decryption
        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
            }
            return memoryStream.ToArray();
        }
    }
}
