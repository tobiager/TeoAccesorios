using System;
using System.Collections.Generic;
namespace TeoAccesorios.Desktop.Models;
public class Venta
{
    public int Id { get; set; }
    public DateTime FechaVenta { get; set; }
    public int UsuarioId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";
    public decimal Total { get; set; }
    public bool Anulada { get; set; } = false;
    public DateTime Fecha => FechaVenta;
    public string Vendedor { get; set; } = "";
    public string Canal { get; set; } = "";
    public string DireccionEnvio { get; set; } = "";
    public List<DetalleVenta> Detalles { get; set; } = new();
}