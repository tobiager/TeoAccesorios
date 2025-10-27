using System;
using System.Security.Cryptography;
using System.Text;

namespace TeoAccesorios.Desktop.Infra.Auth
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashea una contrase�a usando SHA-256 (compatible con SQL Server HASHBYTES)
        /// </summary>
        public static byte[] HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contrase�a no puede estar vac�a", nameof(password));
            
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        
        /// <summary>
        /// Convierte un hash a representaci�n hexadecimal (para debugging)
        /// </summary>
        public static string HashToHex(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}