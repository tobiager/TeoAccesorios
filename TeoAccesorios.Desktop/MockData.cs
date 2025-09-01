using System;
using System.Collections.Generic;
using TeoAccesorios.Desktop.Models;

namespace TeoAccesorios.Desktop;
using TeoAccesorios.Desktop.Models;

public static class MockData
{
    public static List<Categoria> Categorias = new()
    {
        new Categoria{ Id=1, Nombre="Carteras", Descripcion="Carteras de cuero y eco cuero" },
        new Categoria{ Id=2, Nombre="Mochilas", Descripcion="Mochilas urbanas y de viaje" },
        new Categoria{ Id=3, Nombre="Billeteras", Descripcion="Billeteras y tarjeteros" },
        new Categoria{ Id=4, Nombre="Cinturones", Descripcion="Cinturones de cuero" },
        new Categoria{ Id=5, Nombre="Riñoneras", Descripcion="Riñoneras y bandoleras" },
    };

    public static List<Producto> Productos = new()
    {
        new Producto{ Id=1, Nombre="Cartera Soho", CategoriaId=1, CategoriaNombre="Carteras", Precio=85000m, Stock=6, StockMinimo=3, Descripcion="Cartera mediana, cuero vacuno, color suela." },
        new Producto{ Id=2, Nombre="Mochila Urban Pro", CategoriaId=2, CategoriaNombre="Mochilas", Precio=120000m, Stock=4, StockMinimo=2, Descripcion="Mochila grande con cierre reforzado." },
        new Producto{ Id=3, Nombre="Billetera Slim", CategoriaId=3, CategoriaNombre="Billeteras", Precio=35000m, Stock=20, StockMinimo=5, Descripcion="Billetera minimalista con RFID." },
        new Producto{ Id=4, Nombre="Cinturón Clásico", CategoriaId=4, CategoriaNombre="Cinturones", Precio=30000m, Stock=9, StockMinimo=4, Descripcion="Cinturón 100% cuero, hebilla níquel." },
        new Producto{ Id=5, Nombre="Riñonera Street", CategoriaId=5, CategoriaNombre="Riñoneras", Precio=52000m, Stock=7, StockMinimo=3, Descripcion="Riñonera cruzada, varios bolsillos." },
    };

    public static List<Cliente> Clientes = new()
    {
        new Cliente{ Id=1, Nombre="Juan Pérez", Email="juan@example.com", Telefono="+54 9 379 555-1234", Direccion="Junín 123", Localidad="Corrientes", Provincia="Corrientes", Activo=true},
        new Cliente{ Id=2, Nombre="María González", Email="maria@example.com", Telefono="+54 9 379 555-5678", Direccion="San Martín 456", Localidad="Resistencia", Provincia="Chaco", Activo=true},
    };

    public static List<Pedido> Pedidos = new()
    {
        new Pedido{
            Id=1001, ClienteNombre="Juan Pérez", Fecha=DateTime.Today.AddDays(-2), Estado="Pendiente",
            Items=new(){
                new PedidoItem{ ProductoId=1, ProductoNombre="Funda Silicone iPhone", Cantidad=2, PrecioUnitario=12000m },
                new PedidoItem{ ProductoId=3, ProductoNombre="Cable USB-C 1m", Cantidad=1, PrecioUnitario=7000m }
            }
        },
        new Pedido{
            Id=1002, ClienteNombre="María González", Fecha=DateTime.Today.AddDays(-1), Estado="Enviado",
            Items=new(){ new PedidoItem{ ProductoId=4, ProductoNombre="Auriculares BT Over-Ear", Cantidad=1, PrecioUnitario=55000m } }
        }
    };
    public static List<Usuario> Usuarios = new()
    {
        new Usuario{ Id=1, NombreUsuario="admin", Rol="Admin", Activo=true },
        new Usuario{ Id=2, NombreUsuario="vendedor1", Rol="Vendedor", Activo=true },
        new Usuario{ Id=3, NombreUsuario="vendedor2", Rol="Vendedor", Activo=true },
    };

    public static List<Venta> Ventas = new()
    {
        new Venta{
            Id=2001, Fecha=DateTime.Today.AddDays(-1), Vendedor="vendedor1", ClienteId=1, ClienteNombre="Juan Pérez", DireccionEnvio="Junín 123",
            Detalles=new(){
                new DetalleVenta{ Id=1, ProductoId=2, ProductoNombre="Cargador 20W USB-C", Cantidad=1, PrecioUnitario=18000m },
                new DetalleVenta{ Id=2, ProductoId=3, ProductoNombre="Cable USB-C 1m", Cantidad=2, PrecioUnitario=7000m }
            }
        }
    };

}
