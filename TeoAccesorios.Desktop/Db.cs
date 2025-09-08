using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace TeoAccesorios.Desktop
{
    public static class Db
    {
        // Ajustá si hace falta
        public static readonly string ConnectionString =
            "Server=localhost;Database=TeoAccesorios;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

        // -------- SELECT (SIN parámetros) -> PROHIBIDO --------
        [Obsolete("Usar Query(sql, params SqlParameter[]) parametrizado", error: true)]
        public static DataTable Query(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var da = new SqlDataAdapter(sql, cn);
            var dt = new DataTable();
            try { da.Fill(dt); }
            catch (SqlException ex) { throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}", ex); }
            return dt;
        }

        // -------- SELECT (CON parámetros) --------
        public static DataTable Query(string sql, params SqlParameter[] ps)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            if (ps is { Length: > 0 }) cmd.Parameters.AddRange(ps);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();

            try
            {
                cn.Open();
                da.Fill(dt);
            }
            catch (SqlException ex)
            {
                var pars = string.Join(", ",
                    cmd.Parameters.Cast<SqlParameter>()
                        .Select(p => $"{p.ParameterName}={(p.Value is null or DBNull ? "NULL" : p.Value)}"));

                throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}\n\nParams: {pars}", ex);
            }
            return dt;
        }

        // -------- INSERT/UPDATE/DELETE (SIN parámetros) --------
        public static int Exec(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            try
            {
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}", ex);
            }
        }

        // -------- INSERT/UPDATE/DELETE (CON parámetros) --------
        public static int Exec(string sql, params SqlParameter[] ps)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            if (ps is { Length: > 0 }) cmd.Parameters.AddRange(ps);

            try
            {
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                var pars = string.Join(", ",
                    cmd.Parameters.Cast<SqlParameter>()
                        .Select(p => $"{p.ParameterName}={(p.Value is null or DBNull ? "NULL" : p.Value)}"));

                throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}\n\nParams: {pars}", ex);
            }
        }

        // -------- ESCALAR (SIN parámetros) --------
        public static T Scalar<T>(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            try
            {
                cn.Open();
                var val = cmd.ExecuteScalar();
                if (val == null || val is DBNull) return default!;
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch (SqlException ex)
            {
                throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}", ex);
            }
        }

        // -------- ESCALAR (CON parámetros) --------
        public static T Scalar<T>(string sql, params SqlParameter[] ps)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            if (ps is { Length: > 0 }) cmd.Parameters.AddRange(ps);

            try
            {
                cn.Open();
                var val = cmd.ExecuteScalar();
                if (val == null || val is DBNull) return default!;
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch (SqlException ex)
            {
                var pars = string.Join(", ",
                    cmd.Parameters.Cast<SqlParameter>()
                        .Select(p => $"{p.ParameterName}={(p.Value is null or DBNull ? "NULL" : p.Value)}"));

                throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}\n\nParams: {pars}", ex);
            }
        }

        // -------- Helper para crear parámetros --------
        public static SqlParameter P(string name, SqlDbType type, int size, object? value)
        {
            var p = new SqlParameter(name, type) { Value = value ?? DBNull.Value };
            if (size > 0) p.Size = size;
            return p;
        }
        public static SqlParameter P(string name, SqlDbType type, object? value) => P(name, type, 0, value);
    }
}
