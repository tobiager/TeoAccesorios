````markdown
name=README.md
url=https://github.com/tobiager/TeoAccesorios/blob/main/README.md
<p align="center">
  <img src="https://raw.githubusercontent.com/tobiager/UNNE-LSI/main/assets/facena.png" alt="Logo de FaCENA" width="100">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white"/>
  <img src="https://img.shields.io/badge/TSQL-0078D4?style=for-the-badge&logo=microsoft-sql-server&logoColor=white"/>
  <img src="https://img.shields.io/badge/WinForms-0078D4?style=for-the-badge&logo=windows&logoColor=white"/>
  <img src="https://img.shields.io/badge/Visual%20Studio-5C2D91?style=for-the-badge&logo=visual-studio&logoColor=white"/>
  <img src="https://img.shields.io/badge/Estado-en%20desarrollo-yellow?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/Cursada-2025-blue?style=for-the-badge"/>
</p>

# TeoAccesorios — Desktop App (WinForms, .NET 8)

Aplicación de escritorio en C# WinForms para la gestión integral de una marroquinería, conectada a SQL Server.
Administra clientes, productos, usuarios y ventas, con reportes y exportación multi-formato.

Repositorio: https://github.com/tobiager/TeoAccesorios
Última actualización conocida: 2025-10-27
Estado: en desarrollo
Lenguajes principales: C# (488,804 bytes), TSQL (41,839 bytes)

---

## Índice

- [Resumen](#resumen)
- [Características principales](#características-principales)
- [Arquitectura y tecnologías](#arquitectura-y-tecnologías)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Base de datos](#base-de-datos)
- [Instalación y ejecución](#instalación-y-ejecución)
- [Configuración de entorno](#configuración-de-entorno)
- [Flujo de uso y pantallas principales](#flujo-de-uso-y-pantallas-principales)
- [Roles y permisos](#roles-y-permisos)
- [Reportes y exportación](#reportes-y-exportación)
- [Seguridad y buenas prácticas](#seguridad-y-buenas-prácticas)
- [Testing y Calidad](#testing-y-calidad)
- [Roadmap (actualizado)](#roadmap-actualizado)
- [Historial de cambios](#historial-de-cambios)
- [Problemas conocidos / Troubleshooting](#problemas-conocidos--troubleshooting)
- [Contribuir](#contribuir)
- [Autores y contacto](#autores-y-contacto)

---

## Resumen

TeoAccesorios es una aplicación de escritorio desarrollada para facilitar la gestión operativa y administrativa de una marroquinería: ventas, stock, clientes, empleados y reportes. Está pensada para entornos pequeños/medianos que usan Windows y SQL Server.

## Características principales

- ABM de clientes, productos, usuarios/empleados, provincias y localidades.
- Gestión de ventas: creación, detalle, anulación y restauración (según rol).
- Panel principal (Dashboard) con KPIs: ingresos, cantidad de ventas, productos más vendidos y clientes frecuentes.
- Exportación de reportes en PDF (QuestPDF) y Excel (ClosedXML).
- Preferencias de UI: columnas visibles en grillas persistentes por usuario.
- Impresión de comprobantes con logo y formato listo para impresión.
- Búsqueda modal avanzada para seleccionar clientes y productos al crear una venta.

## Arquitectura y tecnologías

- .NET 8.0
- WinForms (desktop)
- C# (estructura por capas: Dominio, Datos, Infra, UI)
- SQL Server (script de creación y datos de ejemplo en `DataBase/TeoAccesorios.sql`)
- Patron Repository para acceso a datos
- Bibliotecas principales: QuestPDF (PDF), ClosedXML (Excel)
- IDE recomendado: Visual Studio 2022

## Estructura del repositorio

```
TeoAccesorios.Desktop/
├── Datos/              # Acceso a BD (Db, Repository)
├── Dominio/            # Modelos de negocio (Cliente, Producto, Venta, etc.)
├── Infra/              # Infraestructura (Auth, GridHelper, Validaciones)
├── Properties/         # Recursos autogenerados
├── Recursos/           # Archivos de recursos (resx, imágenes)
└── UI/                 # Formularios WinForms
    ├── Categorias/
    ├── Clientes/
    ├── Common/         # Dashboard, Login, Main, Reportes
    ├── Productos/
    ├── Provincias/
    ├── Subcategorias/
    ├── Usuarios/
    └── Ventas/
```

## Base de datos

- Archivo: `DataBase/TeoAccesorios.sql`.
- Contiene: definiciones de tablas, relaciones, índices y datos de ejemplo (categorías, clientes, usuarios, productos, ventas, provincias y localidades).
- Nota: el script incluye datos de prueba para facilitar la puesta en marcha; revísalo antes de usar en un entorno de producción.

Cadena de conexión por defecto (archivo `Db.cs`):

```csharp
public static readonly string ConnectionString =
    "Server=localhost;Database=TeoAccesorios;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";
```

Recomendación: en entornos productivos configure autenticación SQL y encripte parámetros sensibles. No deje `Trusted_Connection=True` si usa credenciales remotas.

## Instalación y ejecución

1. Clonar el repositorio:
   git clone https://github.com/tobiager/TeoAccesorios.git
2. Abrir el script `DataBase/TeoAccesorios.sql` en SQL Server Management Studio y ejecutarlo para crear la base de datos con datos de ejemplo.
3. Abrir `TeoAccesorios-Desktop.sln` en Visual Studio 2022.
4. Asegurarse de tener instalado .NET 8 SDK en el equipo.
5. Restaurar paquetes NuGet desde Visual Studio.
6. Compilar la solución y ejecutar (F5) en modo Debug.

## Configuración de entorno

- Base de datos: modifique `Db.cs` si su servidor SQL usa otro nombre/credenciales.
- Archivos y recursos: las imágenes del UI se encuentran en `Recursos/assets`.
- Variables que puede ajustar:
  - Timeout de conexión
  - Rutas de exportación por defecto

## Flujo de uso y pantallas principales

- Login: autenticación básica (modo demo acepta `admin`/`admin123` si se usó el script con datos de ejemplo).
- Dashboard: KPIs, últimos movimientos y alertas de stock.
- Clientes: alta/edición/baja/restauración y búsqueda.
- Productos: ABM de productos con control de stock y categorias/subcategorias.
- Ventas: creación de ventas, selección de cliente y productos desde modales, emisión de comprobante y anulación.
- Reportes: seleccionar rango, aplicar filtros y exportar a PDF/XLSX.

## Roles y permisos

- Administrador: acceso completo, puede gestionar usuarios y ver todos los reportes.
- Vendedor: registrar ventas, ver sus ventas, anular ventas del día (según reglas de negocio).
- Gerente: (si está implementado) acceso a reportes y métricas agregadas.

## Reportes y exportación

- Implementación basada en `ReportService` que proyecta datos a `ReportSnapshot`.
- Exportadores implementan `IReportExporter`:
  - `PdfReportExporter` → QuestPDF
  - `ExcelReportExporter` → ClosedXML
- Los reportes incluyen cabecera con logo, periodo analizado, tabla de ventas y totales.

## Seguridad y buenas prácticas

- Actualmente las contraseñas pueden estar en texto (ver `Usuarios` table seed). Priorizar la implementación de hashing (bcrypt/Argon2) para producción.
- No incluir credenciales en el repo. Use secret managers o variables de entorno.
- Validar permisos en la capa de negocio además de la UI.

## Testing y Calidad

- No hay tests automatizados incluidos hoy. Recomendación:
  - Agregar pruebas unitarias para servicios (ReportService, VentaService, Repository mocks)
  - Añadir pruebas de integración para el acceso a BD en un entorno de CI con DB efímera

## Roadmap (actualizado)

Estado: muchos ítems planificados ya fueron implementados y quedan pendientes mejoras.

- [x] Base de datos de localidades y provincias (cargada)
- [x] Gestión de ventas anuladas/inactivas (vista separada y reglas implementadas)
- [x] Exportación PDF y Excel (QuestPDF, ClosedXML)
- [x] Preferencias de columnas en grillas
- [x] Datos de ejemplo para arranque rápido
- [ ] Seguridad de contraseñas: hashear contraseñas y flujo de recuperación
- [ ] Mejoras en búsqueda: optimizar selectores modales y autocompletado
- [ ] Reportes y métricas: gráficos interactivos en el Dashboard
- [ ] Tests automatizados (unit + integración)

> Si una tarea aparece marcada como completada pero observas un comportamiento distinto en tu entorno, por favor abre una issue describiendo el problema y pasos para reproducir.

## Historial de cambios (resumen)

- 2025-10-27: Actualización del README; sincronización del roadmap con implementación real.
- (ver commits en el repo para historial completo)

## Problemas conocidos / Troubleshooting

- Error de conexión a SQL Server:
  - Verifique la cadena en `Db.cs` y que SQL Server acepte conexiones locales.
  - Si usa autenticación SQL, cambie la cadena y pruebe con SSMS.
- Dependencias NuGet faltantes:
  - Restaurar paquetes desde Visual Studio o ejecutar `dotnet restore`.
- Fuentes o imágenes faltantes en la UI:
  - Compruebe que `Recursos/assets` exista en el workspace; si faltan, la aplicación carga placeholders.

## Contribuir

- Abrir issues para bugs o mejoras.
- Hacer forks y pull requests contra la rama `main`.
- Sugerencia para PRs: describir el cambio, tests realizados y capturas si aplica.

## Licencia

- No hay licencia explícita en el repo. Si quieres usar este código en producción o distribuirlo, agrega un LICENSE apropiado (MIT, Apache-2.0, etc.).

## Autores y contacto

Proyecto desarrollado por Tobias Orban y Ivana Azcona (UNNE - FaCENA, 2025).

---

````