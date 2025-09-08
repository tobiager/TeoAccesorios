# TeoAccesorios — Desktop Frontend (WinForms, .NET 8)

Pantallas de **escritorio** listas para mostrar en Visual Studio 2022 (sin backend ni base de datos). Usa **mock data** en memoria.

## 🚀 Cómo abrir
1. Descomprimir el ZIP.
2. Abrir `TeoAccesorios-Desktop.sln` en **Visual Studio 2022**.
3. Ejecutar en **Debug** (F5).

## 🧭 Módulos incluidos
- **Inicio** (MainForm) con accesos rápidos
- **Productos** (filtro por texto/categoría)
- **Categorías**
- **Clientes**
- **Pedidos** (Doble clic → Detalle)
- **Carrito / Checkout** (flujo demo)
- **Dashboard** (métricas simples)

> Ideal para presentar avances de UI mientras definen el DER.

## 🔐 Login de demo
- Usuario: cualquier texto
- Rol: si el usuario contiene "admin" → Admin; caso contrario → Vendedor.
- El Dashboard solo aparece para **Admin**.

- **Login** ahora se centra automáticamente y permite entrar con cualquier usuario/contraseña (demo total).

## 🧾 Ventas
- **Nueva Venta**: seleccioná Cliente, Producto, Cantidad y Dirección de envío; se agrega al historial.
- **Dashboard**: muestra Ventas/Ingresos de hoy + accesos rápidos y últimas 10 ventas.
