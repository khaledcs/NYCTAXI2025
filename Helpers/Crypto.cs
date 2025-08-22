using System;
using System.Text;

namespace NYC_Taxi_System.Helpers
{
    /// <summary>
    /// Handles cryptographic hashing of a string value
    /// </summary>
    public static class Crypto
    {
        /// <summary>
        /// Method for hashing
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String Hash(string value)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value))
                );
        }
    }
}