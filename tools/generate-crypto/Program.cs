using System;
using System.Text;
using System.Security.Cryptography;

namespace generate_crypto
{
    class Program
    {
        static void Main(string[] args)
        {
            string rsaPair, aesKey;
            var rsa = RSA.Create();
            
            rsaPair = rsa.ToXmlString(true).EncodeBase64();

            var aes = Aes.Create();
            aes.KeySize=256;
            aes.GenerateKey();
            aesKey = Convert.ToBase64String(aes.Key);

            Console.WriteLine($"\"Cryptography\": {{ \n\t\"RsaKeyPair\": \"{rsaPair}\", \n\t\"AesKey\": \"{aesKey}\"\n}}");
        }
    }

    public static class StringExtension
    {
        public static string EncodeBase64(this string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }
    }
}
