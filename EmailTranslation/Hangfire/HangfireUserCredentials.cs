using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EmailTranslation.Hangfire
{
    public class HangfireUserCredentials
    {
        public string Username { get; set; }
        public byte[] PasswordSha1Hash { get; set; }


        public string Password {
            set {
                using (var cryptoProvider = SHA1.Create())
                {
                    PasswordSha1Hash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(value));
                }
            }
        }

        public bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException("login");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("password");

            if (username == Username)
            {
                using (var cryptoProvider = SHA1.Create())
                {
                    byte[] passwordHash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return StructuralComparisons.StructuralEqualityComparer.Equals(passwordHash, PasswordSha1Hash);
                }
            }
            else
                return false;
        }
    }
}
