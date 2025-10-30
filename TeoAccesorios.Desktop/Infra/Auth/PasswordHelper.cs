using System;
using System.Security.Cryptography;
using System.Text;

namespace TeoAccesorios.Desktop.Infra.Auth
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashea una contrase�a usando SHA-256 (compatible con SQL Server HASHBYTES).
        /// No lanza excepci�n si password es null o vac�o: devuelve array vac�o.
        /// </summary>
        public static byte[] HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                // No lanzar excepci�n; devolver array vac�o para indicar fallo.
                return Array.Empty<byte>();
            }

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Intenta hashear la contrase�a sin lanzar excepci�n en caso de entrada inv�lida.
        /// Devuelve false y un array vac�o + mensaje si password es null o vac�o.
        /// </summary>
        public static bool TryHashPassword(string password, out byte[] hash, out string message)
        {
            if (string.IsNullOrEmpty(password))
            {
                hash = Array.Empty<byte>();
                message = "Debe ingresar usuario/contrase�a";
                return false;
            }

            using var sha256 = SHA256.Create();
            hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Verifica que el hash proporcionado corresponda con la contrase�a.
        /// Usa comparaci�n en tiempo fijo para evitar fugas por temporizaci�n.
        /// </summary>
        public static bool VerifyPassword(byte[] hash, string password)
        {
            if (hash is null) throw new ArgumentNullException(nameof(hash));
            if (password is null) throw new ArgumentNullException(nameof(password));

            using var sha256 = SHA256.Create();
            var computed = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }

        /// <summary>
        /// Convierte un hash a representaci�n hexadecimal (para debugging).
        /// </summary>
        public static string HashToHex(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}