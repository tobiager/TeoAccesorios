using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TeoAccesorios.Desktop
{
    public static class Db
    {
        
        public static readonly string ConnectionString =
            "Server=localhost;Database=TeoAccesorios;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

        public static DataTable Query(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var da = new SqlDataAdapter(sql, cn);
            var dt = new DataTable();
            try
            {
                da.Fill(dt);
            }
            catch (SqlException ex)
            {
                
                throw new Exception($"SQL error: {ex.Message}\n\nSQL:\n{sql}", ex);
            }
            return dt;
        }


        public static int Exec(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        
        public static int Exec(string sql, params SqlParameter[] ps)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            if (ps is { Length: > 0 }) cmd.Parameters.AddRange(ps);
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static T Scalar<T>(string sql)
        {
            using var cn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cn.Open();
            var val = cmd.ExecuteScalar();
            if (val == null || val is DBNull) return default!;
            return (T)Convert.ChangeType(val, typeof(T));
        }
    }
}
