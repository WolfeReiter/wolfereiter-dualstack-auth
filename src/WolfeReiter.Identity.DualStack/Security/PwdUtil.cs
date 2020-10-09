using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace WolfeReiter.Identity.DualStack.Security
{
    /// <summary>
    /// PBKDF2 (password-based key derivation function 2) password hash and verify implementation.
    /// </summary>
    public static class PwdUtil
    {
        static RandomNumberGenerator Rng => RandomNumberGenerator.Create();
        public static byte[] NewSalt()
        {
            var buff = new byte[256 / 8];
            Rng.GetBytes(buff);
            return buff;
        }

        public static byte[] Hash(string pwd, byte[] salt)
        {
            Debug.Assert(pwd != null);
            Debug.Assert(salt != null);

            byte[] pwdBytes = Encoding.Default.GetBytes(pwd);
            return Hash(pwdBytes, salt);
        } 

        public static byte[] Hash(byte[] pwd, byte[] salt)
        {
            Debug.Assert(pwd != null);
            Debug.Assert(salt != null);

            var pbkdf2 = new Rfc2898DeriveBytes(
                password: pwd,
                salt: salt, 
                iterations: 65536, 
                hashAlgorithm: HashAlgorithmName.SHA256);

            return pbkdf2.GetBytes(256 / 8);                
        }

        public static bool Verify(string pwd, byte[] hash, byte[] salt)
        {
            Debug.Assert(pwd != null);
            Debug.Assert(hash != null);
            Debug.Assert(salt != null);

            var testHash = Hash(pwd, salt);
            return testHash.Length == hash.Length && testHash.SequenceEqual(hash);
        }
    }
}