using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System.Security.Cryptography
{
    public class TextCypher
    {
        const int SymmetricKeySize = 256;
        RSASignaturePadding SignaturePadding => RSASignaturePadding.Pkcs1;
        HashAlgorithmName HashAlgorithmName => HashAlgorithmName.SHA256;
        SymmetricAlgorithm CreateCypher() => Aes.Create(); //SymmetricAlgorithm.Create("AES") throws a NotSupportedException on dotnet core 2.0
        HashAlgorithm CreateHash() => SHA256.Create(); //HashAlgorithm.Create("SHA256") throws a NotSupportedException on dotnet core 2.0
        RSA CreateRSA() => RSA.Create();

        public TextCypher() { }

        /// <summary>
        /// Convert plaintext into base64 encoded cyphertext using the secret symmetric key.
        /// </summary>
        /// <param name="key">secret symmetric key.</param>
        /// <param name="plaintext">Plaintext.</param>
        /// <returns>Base64 encoded ciphertext.</returns>
        public string EncryptMessage(byte[] key, string plaintext)
        {
            using var cipher = CreateCypher();
            cipher.KeySize = SymmetricKeySize;
            //"AES" algorithm defaults to AES-256 and generates a new IV when it is created.
            var iv = cipher.IV; //capture IV to incorporate in the message.
            using var memoryStream = new MemoryStream();
            memoryStream.Write(iv, 0, iv.Length); //prepend stream with unencrypted 16-byte initialization vector.
            using var encryptor = cipher.CreateEncryptor(key, iv);
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using var cryptoWriter = new StreamWriter(cryptoStream);
            cryptoWriter.Write(plaintext);
            cryptoWriter.Flush();
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        /// <summary>
        /// Convert a base64 cyphertext encrypted with the provided key into plaintext.
        /// </summary>
        /// <param name="key">secret symmetric key</param>
        /// <param name="cyphertext">Cyptertext</param>
        /// <returns>Plaintext.</returns>
        public string DecryptMessage(byte[] key, byte[] cyphertext)
        {
            using var cipher = CreateCypher();
            cipher.KeySize = SymmetricKeySize;
            using var memoryStream = new MemoryStream(cyphertext);
            var iv = new byte[cipher.IV.Length]; //initilization vector size from cipher implementation
            memoryStream.Read(iv, 0, iv.Length); //read initialization vector as unencrypted bytes from the front of the stream
            using var decryptor = cipher.CreateDecryptor(key, iv);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var cryptoReader = new StreamReader(cryptoStream);
            return cryptoReader.ReadToEnd();
        }
        /// <summary>
        /// Convert a base64 cyphertext encrypted with the provided key into plaintext.
        /// </summary>
        /// <param name="key">secret symmetric key</param>
        /// <param name="cyphertext">Cyptertext as base64-encoded string</param>
        /// <returns>Plaintext.</returns>
        public string DecryptMessage(byte[] key, string cyphertext)
        {
            return DecryptMessage(key, Convert.FromBase64String(cyphertext));
        }

        public string SignMessage(RSAParameters rsaKeys, string plaintext)
        {

            using var rsa = CreateRSA();
            using var hash = CreateHash();
            byte[] data, hashResult, signature, signedData;
            rsa.ImportParameters(rsaKeys);
            data = Encoding.UTF8.GetBytes(plaintext);
            hashResult = hash.ComputeHash(data);
            signature = rsa.SignHash(hashResult, HashAlgorithmName, SignaturePadding);
            signedData = signature.Concat(data).ToArray();
            return Convert.ToBase64String(signedData);
        }

        public bool VerifyMessage(RSAParameters rsaKeys, string base64signed, out string plaintext)
        {
            using var rsa = CreateRSA();
            using var hash = CreateHash();
            byte[] signedData, data, signature, hashResult;
            rsa.ImportParameters(rsaKeys);
            signedData = Convert.FromBase64String(base64signed);
            //RSA Signature size is equal to the Modulus of the key.
            signature = signedData.Take(rsaKeys.Modulus?.Length ?? 0).ToArray();
            data = signedData.Skip(rsaKeys.Modulus?.Length ?? 0).ToArray();
            hashResult = hash.ComputeHash(data);
            plaintext = Encoding.UTF8.GetString(data);
            return rsa.VerifyHash(hashResult, signature, HashAlgorithmName, SignaturePadding);
        }
    }
}