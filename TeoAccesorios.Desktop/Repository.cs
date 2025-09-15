using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    // ====== DTOs para la vista inferior (stats) ======
    namespace Models
    {
        public class CategoriaStat
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
            public int Productos { get; set; }
            public int ProductosActivos { get; set; }
            public int StockTotal { get; set; }
            public int BajoMinimo { get; set; }
        }

        public class SubcategoriaStat
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
            public int CategoriaId { get; set; }
            public string CategoriaNombre { get; set; } = "";
            public int Productos { get; set; }
            public int ProductosActivos { get; set; }
            public int StockTotal { get; set; }
            public int BajoMinimo { get; set; }
        }
    }

    public static class Repository
    {
        public static List<Venta> Ventas => ListarVentas(true);
        public static List<Producto> Productos => ListarProductos(true);
        public static List<Cliente> Clientes => ListarClientes(true);

        // ========= CLIENTES =========
        public static List<Cliente> ListarClientes(bool incluirInactivos = false)
        {
            var dt = Db.Query(@"
                SELECT  c.Id,
                        LTRIM(RTRIM(c.Nombre)) AS Nombre,
                        c.Email,
                        c.Telefono,
                        c.Direccion,
                        c.Localidad,
                        c.Provincia,
                        c.Activo
                FROM dbo.Clientes c;",
                Array.Empty<SqlParameter>());

            var list = dt.AsEnumerable().Select(r => new Cliente
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string?>("Nombre") ?? "",
                Email = r.Field<string?>("Email") ?? "",
                Telefono = r.Field<string?>("Telefono") ?? "",
                Direccion = r.Field<string?>("Direccion") ?? "",
                Localidad = r.Field<string?>("Localidad") ?? "",
                Provincia = r.Field<string?>("Provincia") ?? "",
                Activo = r.Field<bool>("Activo")
            }).ToList();

            return incluirInactivos ? list : list.Where(c => c.Activo).ToList();
        }

        public static int InsertarCliente(Cliente c)
        {
            const string sql = @"
                INSERT INTO dbo.Clientes
                    (Nombre, Email, Telefono, Direccion, Localidad, Provincia, Activo)
                OUTPUT INSERTED.Id
                VALUES (@nom, @mail, @tel, @dir, @loc, @prov, 1);";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@nom", c.Nombre ?? "");
            cmd.Parameters.AddWithValue("@mail", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dir", (object?)c.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@loc", (object?)c.Localidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@prov", (object?)c.Provincia ?? DBNull.Value);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ActualizarCliente(Cliente c)
        {
            const string sql = @"
                UPDATE dbo.Clientes
                   SET Nombre=@nom,
                       Email=@mail,
                       Telefono=@tel,
                       Direccion=@dir,
                       Localidad=@loc,
                       Provincia=@prov,
                       Activo=@act
                 WHERE Id=@id;";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.Parameters.AddWithValue("@nom", c.Nombre ?? "");
            cmd.Parameters.AddWithValue("@mail", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dir", (object?)c.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@loc", (object?)c.Localidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@prov", (object?)c.Provincia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@act", c.Activo);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        // Soft delete
        public static void EliminarCliente(int id)
        {
            Db.Exec("UPDATE dbo.Clientes SET Activo=0 WHERE Id=@id",
                new SqlParameter("@id", id));
        }

        // ========= USUARIOS =========
        public static List<Usuario> ListarUsuarios()
        {
            var dt = Db.Query(@"
                SELECT  u.Id              AS Id,
                        u.NombreUsuario   AS NombreUsuario,
                        u.correo          AS Correo,
                        u.contrasenia     AS Contrasenia,
                        u.Rol             AS Rol,
                        u.Activo          AS Activo
                FROM dbo.Usuarios u;",
                Array.Empty<SqlParameter>());

            return dt.AsEnumerable().Select(r => new Usuario
            {
                Id = r.Field<int>("Id"),
                NombreUsuario = r.Field<string?>("NombreUsuario") ?? "",
                Correo = r.Field<string?>("Correo") ?? "",
                Contrasenia = r.Field<string?>("Contrasenia") ?? "",
                Rol = r.Field<string?>("Rol") ?? "",
                Activo = r.Field<bool>("Activo")
            }).ToList();
        }
        public static List<Usuario> Usuarios => ListarUsuarios();

        // ========= CATEGORÍAS =========
        public static List<Categoria> ListarCategorias(bool incluirInactivas = true)
        {
            var dt = Db.Query(@"
                SELECT Id, Nombre, Descripcion, Activo
                FROM dbo.Categorias
                ORDER BY Nombre;",
                Array.Empty<SqlParameter>());

            var list = dt.AsEnumerable().Select(r => new Categoria
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string?>("Nombre") ?? "",
                Descripcion = r.Field<string?>("Descripcion"),
                Activo = r.Field<bool>("Activo")
            }).ToList();

            return incluirInactivas ? list : list.Where(x => x.Activo).ToList();
        }

        // ========= SUBCATEGORÍAS =========
        public static List<Subcategoria> ListarSubcategorias(int? categoriaId = null, bool incluirInactivas = true)
        {
            var dt = Db.Query(@"
            SELECT  s.Id, s.Nombre, s.Descripcion, s.CategoriaId, s.Activo,
                    c.Nombre AS CategoriaNombre
            FROM dbo.Subcategorias s
            JOIN dbo.Categorias c ON c.Id = s.CategoriaId
            WHERE (@cat IS NULL OR s.CategoriaId = @cat)
              AND (@all = 1 OR s.Activo = 1)
            ORDER BY c.Nombre, s.Nombre;",
                new SqlParameter("@cat", (object?)categoriaId ?? DBNull.Value),
                new SqlParameter("@all", incluirInactivas ? 1 : 0));

            return dt.AsEnumerable().Select(r => new Subcategoria
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string?>("Nombre") ?? "",
                Descripcion = r.Field<string?>("Descripcion"),
                CategoriaId = r.Field<int>("CategoriaId"),
                CategoriaNombre = r.Field<string?>("CategoriaNombre") ?? "",
                Activo = r.Field<bool>("Activo")
            }).ToList();
        }

        // ===== CATEGORÍAS: CRUD =====
        public static int InsertarCategoria(Categoria c)
        {
            const string sql = @"
                INSERT INTO dbo.Categorias (Nombre, Descripcion, Activo)
                OUTPUT INSERTED.Id
                VALUES (@nom, @desc, @act);";
            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@nom", c.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)c.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@act", c.Activo);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ActualizarCategoria(Categoria c)
        {
            const string sql = @"
                UPDATE dbo.Categorias
                   SET Nombre=@nom, Descripcion=@desc, Activo=@act
                 WHERE Id=@id;";
            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.Parameters.AddWithValue("@nom", c.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)c.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@act", c.Activo);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        public static void SetCategoriaActiva(int id, bool activa)
        {
            Db.Exec("UPDATE dbo.Categorias SET Activo=@a WHERE Id=@id",
                new SqlParameter("@a", activa ? 1 : 0),
                new SqlParameter("@id", id));
        }

        // ===== SUBCATEGORÍAS: CRUD =====
        public static int InsertarSubcategoria(Subcategoria s)
        {
            const string sql = @"
                INSERT INTO dbo.Subcategorias (Nombre, Descripcion, CategoriaId, Activo)
                OUTPUT INSERTED.Id
                VALUES (@nom, @desc, @cat, @act);";
            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@nom", s.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)s.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cat", s.CategoriaId);
            cmd.Parameters.AddWithValue("@act", s.Activo);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ActualizarSubcategoria(Subcategoria s)
        {
            const string sql = @"
                UPDATE dbo.Subcategorias
                   SET Nombre=@nom, Descripcion=@desc, CategoriaId=@cat, Activo=@act
                 WHERE Id=@id;";
            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", s.Id);
            cmd.Parameters.AddWithValue("@nom", s.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)s.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cat", s.CategoriaId);
            cmd.Parameters.AddWithValue("@act", s.Activo);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        public static void SetSubcategoriaActiva(int id, bool activa)
        {
            Db.Exec("UPDATE dbo.Subcategorias SET Activo=@a WHERE Id=@id",
                new SqlParameter("@a", activa ? 1 : 0),
                new SqlParameter("@id", id));
        }

        // ===== VALIDACIÓN PARA BLOQUEAR LA BAJA (soft delete seguro) =====
        public static int ContarProductosPorCategoria(int categoriaId)
        {
            var dt = Db.Query(@"
                SELECT COUNT(*) AS Cnt
                FROM dbo.Productos
                WHERE CategoriaId = @id;",
                new SqlParameter("@id", categoriaId));
            return dt.AsEnumerable().Select(r => r.Field<int>("Cnt")).FirstOrDefault();
        }

        public static int ContarProductosPorSubcategoria(int subcategoriaId)
        {
            var dt = Db.Query(@"
                SELECT COUNT(*) AS Cnt
                FROM dbo.Productos
                WHERE SubcategoriaId = @id;",
                new SqlParameter("@id", subcategoriaId));
            return dt.AsEnumerable().Select(r => r.Field<int>("Cnt")).FirstOrDefault();
        }

        public static bool TryDesactivarCategoria(int id, out int productosAsociados)
        {
            productosAsociados = ContarProductosPorCategoria(id);
            if (productosAsociados > 0) return false;   
            SetCategoriaActiva(id, false);              
            return true;
        }

        public static bool TryDesactivarSubcategoria(int id, out int productosAsociados)
        {
            productosAsociados = ContarProductosPorSubcategoria(id);
            if (productosAsociados > 0) return false;   
            SetSubcategoriaActiva(id, false);          
            return true;
        }


        // ========= PRODUCTOS =========
        public static List<Producto> ListarProductos(bool incluirInactivos = false)
        {
            var dt = Db.Query(@"
                SELECT  p.Id, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.StockMinimo, p.Activo,
                        p.CategoriaId, c.Nombre AS CategoriaNombre,
                        p.SubcategoriaId, s.Nombre AS SubcategoriaNombre
                FROM dbo.Productos p
                LEFT JOIN dbo.Categorias    c ON c.Id = p.CategoriaId
                LEFT JOIN dbo.Subcategorias s ON s.Id = p.SubcategoriaId;",
                Array.Empty<SqlParameter>());

            var list = dt.AsEnumerable().Select(r => new Producto
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string?>("Nombre") ?? "",
                Descripcion = r.Field<string?>("Descripcion"),
                Precio = r.Field<decimal>("Precio"),
                Stock = r.Field<int>("Stock"),
                StockMinimo = r.Field<int>("StockMinimo"),
                Activo = r.Field<bool>("Activo"),
                CategoriaId = r.Field<int?>("CategoriaId") ?? 0,
                CategoriaNombre = r.Field<string?>("CategoriaNombre") ?? "",
                SubcategoriaId = r.Field<int?>("SubcategoriaId"),
                SubcategoriaNombre = r.Field<string?>("SubcategoriaNombre") ?? ""
            }).ToList();

            return incluirInactivos ? list : list.Where(p => p.Activo).ToList();
        }

        public static int InsertarProducto(Producto p)
        {
            const string sql = @"
                INSERT INTO dbo.Productos
                    (Nombre, Descripcion, Precio, Stock, StockMinimo, CategoriaId, SubcategoriaId, Activo)
                OUTPUT INSERTED.Id
                VALUES (@nom, @desc, @precio, @stock, @min, @cat, @sub, 1);";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@nom", p.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
            cmd.Parameters.AddWithValue("@min", p.StockMinimo);
            cmd.Parameters.AddWithValue("@cat", p.CategoriaId);
            cmd.Parameters.AddWithValue("@sub", (object?)p.SubcategoriaId ?? DBNull.Value);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ActualizarProducto(Producto p)
        {
            const string sql = @"
                UPDATE dbo.Productos
                   SET Nombre=@nom,
                       Descripcion=@desc,
                       Precio=@precio,
                       Stock=@stock,
                       StockMinimo=@min,
                       CategoriaId=@cat,
                       SubcategoriaId=@sub,
                       Activo=@act
                 WHERE Id=@id;";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@nom", p.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
            cmd.Parameters.AddWithValue("@min", p.StockMinimo);
            cmd.Parameters.AddWithValue("@cat", p.CategoriaId);
            cmd.Parameters.AddWithValue("@sub", (object?)p.SubcategoriaId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@act", p.Activo);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        // Soft delete
        public static void EliminarProducto(int id)
        {
            Db.Exec("UPDATE dbo.Productos SET Activo=0 WHERE Id=@id",
                new SqlParameter("@id", id));
        }

        // ========= STATS (para la vista de abajo) =========
        public static List<Models.CategoriaStat> GetCategoriaStats(bool incluirInactivos = true)
        {
            var dt = Db.Query(@"
                SELECT  c.Id,
                        c.Nombre,
                        COUNT(p.Id)                                           AS Productos,
                        SUM(CASE WHEN p.Activo = 1 THEN 1 ELSE 0 END)         AS ProductosActivos,
                        COALESCE(SUM(p.Stock), 0)                             AS StockTotal,
                        SUM(CASE WHEN p.Activo = 1 AND p.Stock < p.StockMinimo THEN 1 ELSE 0 END) AS BajoMinimo
                FROM dbo.Categorias c
                LEFT JOIN dbo.Productos p ON p.CategoriaId = c.Id
                GROUP BY c.Id, c.Nombre
                ORDER BY c.Nombre;",
                Array.Empty<SqlParameter>());

            var list = dt.AsEnumerable().Select(r => new Models.CategoriaStat
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string?>("Nombre") ?? "",
                Productos = r.Field<int>("Productos"),
                ProductosActivos = r.Field<int>("ProductosActivos"),
                StockTotal = r.Field<int>("StockTotal"),
                BajoMinimo = r.Field<int>("BajoMinimo")
            }).ToList();

            if (!incluirInactivos) list = list.Where(x => x.ProductosActivos > 0).ToList();
            return list;
        }

        public static List<Models.SubcategoriaStat> GetSubcategoriaStats(int? categoriaId = null, bool incluirInactivos = true)
        {
            var dt = Db.Query(@"
                SELECT  s.Id,
                        s.Nombre,
                        s.CategoriaId,
                        c.Nombre AS CategoriaNombre,
                        COUNT(p.Id)                                           AS Productos,
                        SUM(CASE WHEN p.Activo = 1 THEN 1 ELSE 0 END)         AS ProductosActivos,
                        COALESCE(SUM(p.Stock), 0)                             AS StockTotal,
                        SUM(CASE WHEN p.Activo = 1 AND p.Stock < p.StockMinimo THEN 1 ELSE 0 END) AS BajoMinimo
                FROM dbo.Subcategorias s
                JOIN dbo.Categorias c ON c.Id = s.CategoriaId
                LEFT JOIN dbo.Productos p ON p.SubcategoriaId = s.Id
                WHERE (@cat IS NULL OR s.CategoriaId = @cat)
                GROUP BY s.Id, s.Nombre, s.CategoriaId, c.Nombre
                ORDER BY c.Nombre, s.Nombre;",
                new SqlParameter("@cat", (object?)categoriaId ?? DBNull.Value));

            var list = dt.AsEnumerable().Select(r => new Models.SubcategoriaStat
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string?>("Nombre") ?? "",
                CategoriaId = r.Field<int>("CategoriaId"),
                CategoriaNombre = r.Field<string?>("CategoriaNombre") ?? "",
                Productos = r.Field<int>("Productos"),
                ProductosActivos = r.Field<int>("ProductosActivos"),
                StockTotal = r.Field<int>("StockTotal"),
                BajoMinimo = r.Field<int>("BajoMinimo")
            }).ToList();

            if (!incluirInactivos) list = list.Where(x => x.ProductosActivos > 0).ToList();
            return list;
        }

        // ========= VENTAS =========
        public static List<Venta> ListarVentas(bool incluirAnuladas = false)
        {
            var dt = Db.Query(@"
                SELECT  v.Id,
                        v.Fecha,
                        v.ClienteId,
                        v.Vendedor,
                        v.Canal,
                        v.DireccionEnvio,
                        CAST(ISNULL(v.Anulada, 0) AS bit) AS Anulada,
                        c.Nombre AS ClienteNombre
                FROM dbo.Ventas v
                LEFT JOIN dbo.Clientes c ON c.Id = v.ClienteId;",
                Array.Empty<SqlParameter>());

            var ventas = dt.AsEnumerable().Select(r => new Venta
            {
                Id = r.Field<int>("Id"),
                FechaVenta = r.Field<DateTime>("Fecha"),
                ClienteId = r.Field<int>("ClienteId"),
                ClienteNombre = r.Field<string?>("ClienteNombre") ?? "",
                Vendedor = r.Field<string?>("Vendedor") ?? "",
                Canal = r.Field<string?>("Canal") ?? "",
                DireccionEnvio = r.Field<string?>("DireccionEnvio") ?? "",
                Anulada = r.Field<bool>("Anulada")
            }).ToList();

            var dtDet = Db.Query(@"
                SELECT  Id,
                        VentaId,
                        ProductoId,
                        ProductoNombre,
                        Cantidad,
                        PrecioUnitario,
                        (Cantidad * PrecioUnitario) AS Subtotal
                FROM dbo.DetalleVenta;",
                Array.Empty<SqlParameter>());

            var detalles = dtDet.AsEnumerable().Select(r => new DetalleVenta
            {
                Id = r.Field<int>("Id"),
                VentaId = r.Field<int>("VentaId"),
                ProductoId = r.Field<int>("ProductoId"),
                ProductoNombre = r.Field<string?>("ProductoNombre") ?? "",
                Cantidad = r.Field<int>("Cantidad"),
                PrecioUnitario = r.Field<decimal>("PrecioUnitario"),
                Subtotal = r.Field<decimal>("Subtotal")
            }).ToList();

            foreach (var v in ventas)
            {
                v.Detalles = detalles.Where(d => d.VentaId == v.Id).ToList();
                v.Total = v.Detalles.Sum(d => d.Subtotal);
            }

            if (!incluirAnuladas) ventas = ventas.Where(v => !v.Anulada).ToList();
            return ventas.OrderByDescending(v => v.FechaVenta).ToList();
        }

        public static int InsertarVenta(Venta v)
        {
            using var cn = new SqlConnection(Db.ConnectionString);
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                var cmdCab = new SqlCommand(@"
                    INSERT INTO dbo.Ventas (Fecha, Vendedor, Canal, ClienteId, DireccionEnvio, Anulada)
                    OUTPUT INSERTED.Id
                    VALUES (@fecha, @vend, @canal, @cli, @dir, 0);", cn, tx);

                cmdCab.Parameters.AddWithValue("@fecha", v.FechaVenta);
                cmdCab.Parameters.AddWithValue("@vend", v.Vendedor ?? "");
                cmdCab.Parameters.AddWithValue("@canal", v.Canal ?? "");
                cmdCab.Parameters.AddWithValue("@cli", v.ClienteId);
                cmdCab.Parameters.AddWithValue("@dir", (object?)v.DireccionEnvio ?? DBNull.Value);

                var idVenta = Convert.ToInt32(cmdCab.ExecuteScalar());

                foreach (var d in v.Detalles)
                {
                    var cmdDet = new SqlCommand(@"
                        INSERT INTO dbo.DetalleVenta (VentaId, ProductoId, ProductoNombre, Cantidad, PrecioUnitario)
                        VALUES (@v, @p, @pn, @c, @pu);", cn, tx);
                    cmdDet.Parameters.AddWithValue("@v", idVenta);
                    cmdDet.Parameters.AddWithValue("@p", d.ProductoId);
                    cmdDet.Parameters.AddWithValue("@pn", d.ProductoNombre ?? "");
                    cmdDet.Parameters.AddWithValue("@c", d.Cantidad);
                    cmdDet.Parameters.AddWithValue("@pu", d.PrecioUnitario);
                    cmdDet.ExecuteNonQuery();

                    var cmdStock = new SqlCommand(
                        "UPDATE dbo.Productos SET Stock = Stock - @c WHERE Id=@p;", cn, tx);
                    cmdStock.Parameters.AddWithValue("@c", d.Cantidad);
                    cmdStock.Parameters.AddWithValue("@p", d.ProductoId);
                    cmdStock.ExecuteNonQuery();
                }

                tx.Commit();
                return idVenta;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public static void SetVentaAnulada(int idVenta, bool anulada)
        {
            using var cn = new SqlConnection(Db.ConnectionString);
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                new SqlCommand("UPDATE dbo.Ventas SET Anulada=@a WHERE Id=@id;", cn, tx)
                {
                    Parameters =
                    {
                        new("@a", anulada ? 1 : 0),
                        new("@id", idVenta)
                    }
                }.ExecuteNonQuery();

                var dtDet = new DataTable();
                using (var da = new SqlDataAdapter(
                    "SELECT ProductoId, Cantidad FROM dbo.DetalleVenta WHERE VentaId=@id", cn))
                {
                    da.SelectCommand.Transaction = tx;
                    da.SelectCommand.Parameters.AddWithValue("@id", idVenta);
                    da.Fill(dtDet);
                }

                foreach (DataRow r in dtDet.Rows)
                {
                    int prod = (int)r["ProductoId"];
                    int cant = (int)r["Cantidad"];

                    var sql = anulada
                        ? "UPDATE dbo.Productos SET Stock = Stock + @c WHERE Id=@p;"
                        : "UPDATE dbo.Productos SET Stock = Stock - @c WHERE Id=@p;";
                    var cmdStock = new SqlCommand(sql, cn, tx);
                    cmdStock.Parameters.AddWithValue("@c", cant);
                    cmdStock.Parameters.AddWithValue("@p", prod);
                    cmdStock.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public static void EliminarVentaTotal(int idVenta)
        {
            using var cn = new SqlConnection(Db.ConnectionString);
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                var dtDet = new DataTable();
                using (var da = new SqlDataAdapter(
                    "SELECT ProductoId, Cantidad FROM dbo.DetalleVenta WHERE VentaId=@id", cn))
                {
                    da.SelectCommand.Transaction = tx;
                    da.SelectCommand.Parameters.AddWithValue("@id", idVenta);
                    da.Fill(dtDet);
                }

                foreach (DataRow r in dtDet.Rows)
                {
                    int prod = (int)r["ProductoId"];
                    int cant = (int)r["Cantidad"];
                    var cmd = new SqlCommand(
                        "UPDATE dbo.Productos SET Stock = Stock + @c WHERE Id=@p;", cn, tx);
                    cmd.Parameters.AddWithValue("@c", cant);
                    cmd.Parameters.AddWithValue("@p", prod);
                    cmd.ExecuteNonQuery();
                }

                new SqlCommand("DELETE FROM dbo.DetalleVenta WHERE VentaId=@id;", cn, tx)
                { Parameters = { new("@id", idVenta) } }.ExecuteNonQuery();

                new SqlCommand("DELETE FROM dbo.Ventas WHERE Id=@id;", cn, tx)
                { Parameters = { new("@id", idVenta) } }.ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
