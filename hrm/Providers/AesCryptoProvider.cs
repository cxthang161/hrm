using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace hrm.Providers
{
    public class AesCryptoProvider : Controller
    {
        private readonly IConfiguration _configuration;

        public AesCryptoProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Encrypt(string list)
        {
            List<string> permissionList = string.IsNullOrEmpty(list)
                ? new List<string>()
                : list.Split(',').Select(p => p.Trim()).ToList();

            string json = JsonConvert.SerializeObject(permissionList);

            string secretKey = _configuration["Cryptoraphy:SecretKey"];
            string salt = _configuration["Cryptoraphy:Salt"];

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] ivBytes = Encoding.UTF8.GetBytes(salt);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            string secretKey = _configuration["Cryptoraphy:SecretKey"];
            string salt = _configuration["Cryptoraphy:Salt"];

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] ivBytes = Encoding.UTF8.GetBytes(salt);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
