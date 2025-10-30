using System;
using System.Security.Cryptography;
using System.Text;

namespace TeoAccesorios.Desktop.Infra.Auth
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashea una contraseña usando SHA-256 (compatible con SQL Server HASHBYTES).
        /// No lanza excepción si password es null o vacío: devuelve array vacío.
        /// </summary>
        public static byte[] HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                // No lanzar excepción; devolver array vacío para indicar fallo.
                return Array.Empty<byte>();
            }

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Intenta hashear la contraseña sin lanzar excepción en caso de entrada inválida.
        /// Devuelve false y un array vacío + mensaje si password es null o vacío.
        /// </summary>
        public static bool TryHashPassword(string password, out byte[] hash, out string message)
        {
            if (string.IsNullOrEmpty(password))
            {
                hash = Array.Empty<byte>();
                message = "Debe ingresar usuario/contraseña";
                return false;
            }

            using var sha256 = SHA256.Create();
            hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Verifica que el hash proporcionado corresponda con la contraseña.
        /// Usa comparación en tiempo fijo para evitar fugas por temporización.
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
        /// Convierte un hash a representación hexadecimal (para debugging).
        /// </summary>
        public static string HashToHex(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}