using System;

namespace System.Text
{
    public static class StringExtension
    {
        public static string DecodeBase64(this string base64Encoded)
        {
            var bytes = Convert.FromBase64String(base64Encoded);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string EncodeBase64(this string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }
    }
}
