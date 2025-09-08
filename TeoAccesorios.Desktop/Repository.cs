using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop
{
    public static class Repository
    {
        public static List<Venta> Ventas => ListarVentas(true);
        public static List<Producto> Productos => ListarProductos(true);
        public static List<Cliente> Clientes => ListarClientes(true);

        // ========= CLIENTES =========
        public static List<Cliente> ListarClientes(bool incluirInactivos = false)
        {
            var dt = Db.Query(@"
                SELECT  id_cliente      AS Id,
                        (nombre + ' ' + apellido) AS Nombre,
                        email           AS Email,
                        telefono        AS Telefono,
                        direccion       AS Direccion,
                        1               AS Activo
                FROM dbo.cliente");

            var list = dt.AsEnumerable().Select(r => new Cliente
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string>("Nombre") ?? "",
                Email = r.Field<string?>("Email") ?? "",
                Telefono = r.Field<string?>("Telefono") ?? "",
                Direccion = r.Field<string?>("Direccion") ?? "",
                Localidad = "",
                Provincia = "",
                Activo = r.Field<int>("Activo") == 1
            }).ToList();

            return incluirInactivos ? list : list.Where(c => c.Activo).ToList();
        }

        public static int InsertarCliente(Cliente c)
        {
            const string sql = @"
                INSERT INTO dbo.cliente (nombre, apellido, email, telefono, direccion)
                VALUES (@nom, '', @mail, @tel, @dir);
                SELECT SCOPE_IDENTITY();";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@nom", c.Nombre ?? "");
            cmd.Parameters.AddWithValue("@mail", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dir", (object?)c.Direccion ?? DBNull.Value);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ActualizarCliente(Cliente c)
        {
            const string sql = @"
                UPDATE dbo.cliente
                SET nombre=@nom, email=@mail, telefono=@tel, direccion=@dir
                WHERE id_cliente=@id;";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.Parameters.AddWithValue("@nom", c.Nombre ?? "");
            cmd.Parameters.AddWithValue("@mail", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dir", (object?)c.Direccion ?? DBNull.Value);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        // ========= USUARIOS =========
        public static List<Usuario> ListarUsuarios()
        {
            var dt = Db.Query(@"
                SELECT  id_usuario     AS Id,
                        nombreUsuario  AS NombreUsuario,
                        correo         AS Correo,
                        contrasenia    AS Contrasenia,
                        rol            AS Rol,
                        activo         AS Activo
                FROM dbo.Usuarios"); // <— tu tabla real

            return dt.AsEnumerable().Select(r => new Usuario
            {
                Id = r.Field<int>("Id"),
                NombreUsuario = r.Field<string>("NombreUsuario") ?? "",
                Correo = r.Field<string?>("Correo") ?? "",
                Contrasenia = r.Field<string?>("Contrasenia") ?? "",
                Rol = r.Field<string?>("Rol") ?? "",
                Activo = r.Field<bool>("Activo")
            }).ToList();
        }

        public static List<Usuario> Usuarios => ListarUsuarios();

        // ========= CATEGORÍAS / PRODUCTOS =========
        public static List<Categoria> ListarCategorias()
        {
            var dt = Db.Query("SELECT id_categoria AS Id, nombre AS Nombre, descripcion AS Descripcion, activo AS Activo FROM dbo.categoria");
            return dt.AsEnumerable().Select(r => new Categoria
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string>("Nombre") ?? "",
                Descripcion = r.Field<string?>("Descripcion"),
                Activo = r.Field<bool>("Activo")
            }).ToList();
        }

        public static List<Producto> ListarProductos(bool incluirInactivos = false)
        {
            var dt = Db.Query(@"
                SELECT p.id_producto          AS Id,
                       p.nombre               AS Nombre,
                       p.descripcion          AS Descripcion,
                       p.precio               AS Precio,
                       p.stock                AS Stock,
                       p.stockMinimo          AS StockMinimo,
                       p.activo               AS Activo,
                       s.id_subcategoria      AS SubcategoriaId,
                       s.descripcion          AS SubcategoriaDesc,
                       c.id_categoria         AS CategoriaId,
                       c.nombre               AS CategoriaNombre
                FROM dbo.producto p
                JOIN dbo.subcategoria s ON s.id_subcategoria = p.id_subcategoria
                JOIN dbo.categoria    c ON c.id_categoria    = s.id_categoria");

            var list = dt.AsEnumerable().Select(r => new Producto
            {
                Id = r.Field<int>("Id"),
                Nombre = r.Field<string>("Nombre") ?? "",
                Descripcion = r.Field<string?>("Descripcion"),
                Precio = r.Field<decimal>("Precio"),
                Stock = r.Field<int>("Stock"),
                StockMinimo = r.Field<int>("StockMinimo"),
                Activo = r.Field<bool>("Activo"),
                SubcategoriaId = r.Field<int>("SubcategoriaId"),
                SubcategoriaNombre = r.Field<string>("SubcategoriaDesc") ?? "",
                CategoriaId = r.Field<int>("CategoriaId"),
                CategoriaNombre = r.Field<string>("CategoriaNombre") ?? ""
            }).ToList();

            return incluirInactivos ? list : list.Where(p => p.Activo).ToList();
        }

        public static int InsertarProducto(Producto p)
        {
            const string sql = @"
                INSERT INTO dbo.producto (nombre, descripcion, precio, stock, stockMinimo, activo, id_subcategoria, fechaAlta)
                VALUES (@nom, @desc, @precio, @stock, @min, 1, @sub, GETDATE());
                SELECT SCOPE_IDENTITY();";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@nom", p.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
            cmd.Parameters.AddWithValue("@min", p.StockMinimo);
            cmd.Parameters.AddWithValue("@sub", p.SubcategoriaId);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ActualizarProducto(Producto p)
        {
            const string sql = @"
                UPDATE dbo.producto
                SET nombre=@nom, descripcion=@desc, precio=@precio,
                    stock=@stock, stockMinimo=@min, id_subcategoria=@sub
                WHERE id_producto=@id;";

            using var cn = new SqlConnection(Db.ConnectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@nom", p.Nombre ?? "");
            cmd.Parameters.AddWithValue("@desc", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
            cmd.Parameters.AddWithValue("@min", p.StockMinimo);
            cmd.Parameters.AddWithValue("@sub", p.SubcategoriaId);
            cn.Open();
            cmd.ExecuteNonQuery();
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
                        ISNULL(v.Anulada,0) AS Anulada,
                        (c.nombre + ' ' + c.apellido) AS ClienteNombre
                FROM dbo.Ventas v
                LEFT JOIN dbo.cliente c ON c.id_cliente = v.ClienteId");

            var ventas = dt.AsEnumerable().Select(r => new Venta
            {
                Id = r.Field<int>("Id"),
                FechaVenta = r.Field<DateTime>("Fecha"),
                ClienteId = r.Field<int>("ClienteId"),
                ClienteNombre = r.Field<string?>("ClienteNombre") ?? "",
                Vendedor = r.Field<string?>("Vendedor") ?? "",
                Canal = r.Field<string?>("Canal") ?? "",
                DireccionEnvio = r.Field<string?>("DireccionEnvio") ?? "",
                Anulada = r.Field<int>("Anulada") == 1
            }).ToList();

            var dtDet = Db.Query(@"
                SELECT  d.id_detalle      AS Id,
                        d.id_venta        AS VentaId,
                        d.id_producto     AS ProductoId,
                        p.nombre          AS ProductoNombre,
                        d.cantidad        AS Cantidad,
                        d.precioUnitario  AS PrecioUnitario,
                        d.subtotal        AS Subtotal
                FROM dbo.DetalleVenta d
                JOIN dbo.producto p ON p.id_producto = d.id_producto");

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
                    VALUES (@fecha, @vend, @canal, @cli, @dir, 0);
                    SELECT SCOPE_IDENTITY();", cn, tx);

                cmdCab.Parameters.AddWithValue("@fecha", v.FechaVenta);
                cmdCab.Parameters.AddWithValue("@vend", v.Vendedor ?? "");
                cmdCab.Parameters.AddWithValue("@canal", v.Canal ?? "");
                cmdCab.Parameters.AddWithValue("@cli", v.ClienteId);
                cmdCab.Parameters.AddWithValue("@dir", (object?)v.DireccionEnvio ?? DBNull.Value);

                var idVenta = Convert.ToInt32(cmdCab.ExecuteScalar());

                foreach (var d in v.Detalles)
                {
                    var cmdDet = new SqlCommand(@"
                        INSERT INTO dbo.DetalleVenta (id_venta, id_producto, cantidad, precioUnitario, subtotal)
                        VALUES (@v,@p,@c,@pu,@st);", cn, tx);

                    cmdDet.Parameters.AddWithValue("@v", idVenta);
                    cmdDet.Parameters.AddWithValue("@p", d.ProductoId);
                    cmdDet.Parameters.AddWithValue("@c", d.Cantidad);
                    cmdDet.Parameters.AddWithValue("@pu", d.PrecioUnitario);
                    cmdDet.Parameters.AddWithValue("@st", d.Subtotal);
                    cmdDet.ExecuteNonQuery();

                    var cmdStock = new SqlCommand(
                        "UPDATE dbo.producto SET stock = stock - @c WHERE id_producto=@p;", cn, tx);
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
                var cmd = new SqlCommand(
                    "UPDATE dbo.Ventas SET Anulada=@a WHERE Id=@id;", cn, tx);
                cmd.Parameters.AddWithValue("@a", anulada ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", idVenta);
                cmd.ExecuteNonQuery();

                var dtDet = new DataTable();
                using (var da = new SqlDataAdapter(
                    "SELECT id_producto, cantidad FROM dbo.DetalleVenta WHERE id_venta=@id", cn))
                {
                    da.SelectCommand.Transaction = tx;
                    da.SelectCommand.Parameters.AddWithValue("@id", idVenta);
                    da.Fill(dtDet);
                }

                foreach (DataRow r in dtDet.Rows)
                {
                    int prod = (int)r["id_producto"];
                    int cant = (int)r["cantidad"];

                    var cmdStock = new SqlCommand(
                        anulada
                            ? "UPDATE dbo.producto SET stock = stock + @c WHERE id_producto=@p;"
                            : "UPDATE dbo.producto SET stock = stock - @c WHERE id_producto=@p;",
                        cn, tx);
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
    }
}
