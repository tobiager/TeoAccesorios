using System;
using System.Collections.Generic;
using System.Linq;

namespace TeoAccesorios.Desktop.Models;
public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public string Canal { get; set; } = "Instagram";
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? DireccionEnvio { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();
    public bool Anulada { get; set; } = false;
    public decimal Total => Detalles.Sum(d => d.Subtotal);
    
}
