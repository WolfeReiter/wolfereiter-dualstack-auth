using System;
using WolfeReiter.Identity.DualStack.Security;

namespace pbkdf2
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run password");
                return;
            }

            string pwd = args[0];
            var salt = PwdUtil.NewSalt();
            var hash = PwdUtil.Hash(pwd, salt);
            Console.WriteLine($"Hash: {Convert.ToBase64String(hash)}");
            Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");
        }
    }
}
