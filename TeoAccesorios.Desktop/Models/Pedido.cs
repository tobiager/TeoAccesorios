using System;
using System.Collections.Generic;
using System.Linq;

namespace TeoAccesorios.Desktop.Models;
public class Pedido
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public List<PedidoItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
}
