<p align="center">
  <img src="https://raw.githubusercontent.com/tobiager/UNNE-LSI/main/assets/facena.png" alt="Logo de FaCENA" width="100">
</p>



<p align="center">
  <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white"/>
  <img src="https://img.shields.io/badge/WinForms-0078D4?style=for-the-badge&logo=windows&logoColor=white"/>
  <img src="https://img.shields.io/badge/Visual%20Studio-5C2D91?style=for-the-badge&logo=visual-studio&logoColor=white"/>
  <img src="https://img.shields.io/badge/Estado-en%20desarrollo-yellow?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/Cursada-2025-blue?style=for-the-badge"/>
</p>

#  TeoAccesorios — Desktop App (WinForms, .NET 8)

Aplicación de escritorio desarrollada en **C# con WinForms** para gestionar las ventas de una marroquinería.  
El sistema está pensado como demo funcional con **mock data en memoria**, para mostrar pantallas y flujos al docente mientras se define el DER final.

---

## 🚀 Cómo abrir

1. Clonar el repo o descargar el ZIP.
2. Abrir `TeoAccesorios-Desktop.sln` en **Visual Studio 2022**.
3. Ejecutar en modo **Debug** (F5).

---

## 🧭 Módulos incluidos

-  **Login** (demo, acepta cualquier usuario/contraseña)
-  **Dashboard** (KPIs + últimas ventas + stock bajo)
-  **Clientes** (alta, edición, eliminación y restauración)
-  **Productos** (filtro por texto/categoría, ABM solo para Admin)
-  **Empleados/Usuarios** (solo visible para Admin)
-  **Ventas**
  - Nueva venta (cliente + productos + dirección de envío)
  - Listado de ventas con detalle al hacer doble clic
  - Anulación / restauración con reglas por rol
-  **Reportes**
  - Rango semanal, mensual o personalizado
  - KPIs: ingresos totales, cantidad de ventas, clientes únicos, productos vendidos
  - Exportación en CSV / TSV / JSON

---

##  Roles de usuario

- **Administrador**
  - Gestiona clientes, productos y empleados
  - Accede a reportes completos
  - Puede eliminar/restaurar registros

- **Vendedor**
  - Puede registrar ventas
  - Solo ve sus propias ventas
  - Puede anular/restaurar solo ventas del día
  - Acceso de solo lectura en productos

---

## 📸 Capturas (demo)

> Agregá imágenes del login, dashboard y reportes acá para que quede fachero.

---

## 🏗️ Estado actual

✔️ Pantallas funcionales en WinForms con mock data  
✔️ Navegación integrada en una sola ventana (sidebar fijo + panel central)  
✔️ Roles diferenciados (Admin / Vendedor)  
⚡ Pendiente: conexión a base de datos real

---

<p align="center"><b>❤️ Proyecto desarrollado por Tobias Orban y Ivana Azcona (UNNE - FaCENA, 2025)</b></p>
