using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WolfeReiter.Identity.DualStack.Security
{
    public class CryptoService
    {
        IConfiguration Configuration { get; }
        byte[] AesKey { get; }
        RSAParameters RsaKeyPair { get; }
        TextCypher Cypher { get; }
        public CryptoService(IConfiguration configuration) 
        {
            Configuration = configuration; 
            AesKey        = Convert.FromBase64String(Configuration.GetValue<string>("Cryptography:AesKey"));
            RsaKeyPair    = Configuration.GetValue<string>("Cryptography:RsaKeyPair").DecodeBase64().RSAParametersFromXml();
            Cypher        = new TextCypher();
        }
        
        public string Encrypt(string plaintext)
        {
            var signed    = Cypher.SignMessage(RsaKeyPair, plaintext);
            var encrypted = Cypher.EncryptMessage(AesKey, signed);
            return encrypted;
        }
        public bool DecryptAndVerify(string cyphertext, out string plaintext)
        {
            var decrypted = Cypher.DecryptMessage(AesKey, cyphertext);
            return Cypher.VerifyMessage(RsaKeyPair, decrypted, out plaintext);
        }
    }
}