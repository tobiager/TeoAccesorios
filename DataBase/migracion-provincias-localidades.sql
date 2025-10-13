/* ================================================================
   TeoAccesorios – Provincias/Localidades + migración y FKs
   Ejecutar con un login que tenga permisos DDL.
================================================================ */
USE [TeoAccesorios];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ---------------------------
   Helpers de existencia
--------------------------- */
IF OBJECT_ID('dbo.Provincias','U') IS NULL
BEGIN
    PRINT '1. Creando tabla Provincias...';
    CREATE TABLE dbo.Provincias(
        Id     INT IDENTITY(1,1) NOT NULL
      , Nombre NVARCHAR(100)     NOT NULL
      , CONSTRAINT PK_Provincias PRIMARY KEY CLUSTERED(Id)
      , CONSTRAINT UQ_Provincias_Nombre UNIQUE(Nombre)
    );
END
GO

IF OBJECT_ID('dbo.Localidades','U') IS NULL
BEGIN
    PRINT '2. Creando tabla Localidades...';
    CREATE TABLE dbo.Localidades(
        Id          INT IDENTITY(1,1) NOT NULL
      , Nombre      NVARCHAR(100)     NOT NULL
      , ProvinciaId INT               NOT NULL
      , CONSTRAINT PK_Localidades PRIMARY KEY CLUSTERED(Id)
      , CONSTRAINT FK_Localidades_Provincias
            FOREIGN KEY (ProvinciaId) REFERENCES dbo.Provincias(Id)
      , CONSTRAINT UQ_Localidades_NombreProvincia UNIQUE(Nombre, ProvinciaId)
    );
END
GO

/* -----------------------------------------
   3) Poblar Provincias (si está vacío)
----------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Provincias)
BEGIN
    PRINT '3. Poblando tabla Provincias con datos de Argentina...';
    SET IDENTITY_INSERT dbo.Provincias ON;
    INSERT INTO dbo.Provincias (Id, Nombre) VALUES
    (1,  N'Buenos Aires'),
    (2,  N'Catamarca'),
    (3,  N'Chaco'),
    (4,  N'Chubut'),
    (5,  N'Ciudad Autónoma de Buenos Aires'),
    (6,  N'Córdoba'),
    (7,  N'Corrientes'),
    (8,  N'Entre Ríos'),
    (9,  N'Formosa'),
    (10, N'Jujuy'),
    (11, N'La Pampa'),
    (12, N'La Rioja'),
    (13, N'Mendoza'),
    (14, N'Misiones'),
    (15, N'Neuquén'),
    (16, N'Río Negro'),
    (17, N'Salta'),
    (18, N'San Juan'),
    (19, N'San Luis'),
    (20, N'Santa Cruz'),
    (21, N'Santa Fe'),
    (22, N'Santiago del Estero'),
    (23, N'Tierra del Fuego'),
    (24, N'Tucumán');
    SET IDENTITY_INSERT dbo.Provincias OFF;
END
GO

/* ================================================================
   4) Migrar datos desde Clientes (texto -> FK)
================================================================ */
IF OBJECT_ID('dbo.Clientes','U') IS NULL
BEGIN
    RAISERROR('La tabla dbo.Clientes no existe en TeoAccesorios.', 16, 1);
    RETURN;
END

BEGIN TRAN;

PRINT '4. Insertando Provincias/Localidades faltantes desde Clientes...';

-- Provincias que no existan (desde Clientes.Provincia)
INSERT INTO dbo.Provincias(Nombre)
SELECT DISTINCT LTRIM(RTRIM(c.Provincia))
FROM dbo.Clientes c
WHERE c.Provincia IS NOT NULL AND LTRIM(RTRIM(c.Provincia)) <> N''
  AND NOT EXISTS (SELECT 1 FROM dbo.Provincias p
                  WHERE p.Nombre = LTRIM(RTRIM(c.Provincia)));

-- Localidades que no existan (pareadas por provincia)
INSERT INTO dbo.Localidades(Nombre, ProvinciaId)
SELECT DISTINCT LTRIM(RTRIM(c.Localidad)) AS Nombre, p.Id
FROM dbo.Clientes c
JOIN dbo.Provincias p
  ON p.Nombre = LTRIM(RTRIM(c.Provincia))
WHERE c.Localidad IS NOT NULL AND LTRIM(RTRIM(c.Localidad)) <> N''
  AND NOT EXISTS (SELECT 1
                  FROM dbo.Localidades l
                  WHERE l.Nombre = LTRIM(RTRIM(c.Localidad))
                    AND l.ProvinciaId = p.Id);

-- 4.1) Asegurar columna nueva en Clientes
IF COL_LENGTH('dbo.Clientes','LocalidadId') IS NULL
BEGIN
    PRINT '5. Agregando columna Clientes.LocalidadId...';
    ALTER TABLE dbo.Clientes ADD LocalidadId INT NULL;
END

-- 4.2) Completar LocalidadId en Clientes según textos actuales
PRINT '6. Actualizando Clientes.LocalidadId...';
UPDATE c
SET c.LocalidadId = l.Id
FROM dbo.Clientes c
JOIN dbo.Provincias p
  ON p.Nombre = LTRIM(RTRIM(c.Provincia))
JOIN dbo.Localidades l
  ON l.Nombre = LTRIM(RTRIM(c.Localidad)) AND l.ProvinciaId = p.Id
WHERE c.LocalidadId IS NULL;

-- 4.3) FK en Clientes -> Localidades
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Clientes_Localidades' AND parent_object_id = OBJECT_ID('dbo.Clientes')
)
BEGIN
    PRINT '7. Creando FK_Clientes_Localidades...';
    ALTER TABLE dbo.Clientes
      ADD CONSTRAINT FK_Clientes_Localidades
          FOREIGN KEY (LocalidadId) REFERENCES dbo.Localidades(Id);
END

-- 4.4) Dropear columnas viejas (si existen)
IF COL_LENGTH('dbo.Clientes','Localidad') IS NOT NULL
BEGIN
    PRINT '8. Eliminando columna Clientes.Localidad (texto)...';
    ALTER TABLE dbo.Clientes DROP COLUMN Localidad;
END

IF COL_LENGTH('dbo.Clientes','Provincia') IS NOT NULL
BEGIN
    PRINT '9. Eliminando columna Clientes.Provincia (texto)...';
    ALTER TABLE dbo.Clientes DROP COLUMN Provincia;
END

COMMIT TRAN;
PRINT 'Clientes migrado a FK de Localidades.';
GO

/* ================================================================
   5) Ventas: permitir dirección/localidad propias por venta,
      o tomar por defecto las del cliente.
================================================================ */

IF OBJECT_ID('dbo.Ventas','U') IS NULL
BEGIN
    RAISERROR('La tabla dbo.Ventas no existe en TeoAccesorios.', 16, 1);
    RETURN;
END

BEGIN TRAN;

-- 5.1) Asegurar columnas en Ventas
IF COL_LENGTH('dbo.Ventas','DireccionVenta') IS NULL
BEGIN
    PRINT '10. Agregando Ventas.DireccionVenta (NULLable)...';
    ALTER TABLE dbo.Ventas ADD DireccionVenta NVARCHAR(200) NULL;
END

-- Preferimos el nombre LocalidadIdVenta (coincide con tu diagrama).
IF COL_LENGTH('dbo.Ventas','LocalidadIdVenta') IS NULL
BEGIN
    IF COL_LENGTH('dbo.Ventas','LocalidadId') IS NOT NULL
    BEGIN
        PRINT '11. Renombrando Ventas.LocalidadId -> LocalidadIdVenta...';
        EXEC sp_rename 'dbo.Ventas.LocalidadId', 'LocalidadIdVenta', 'COLUMN';
    END
    ELSE
    BEGIN
        PRINT '11. Agregando Ventas.LocalidadIdVenta...';
        ALTER TABLE dbo.Ventas ADD LocalidadIdVenta INT NULL;
    END
END

-- ProvinciaIdVenta es opcional; si existe, la completamos
IF COL_LENGTH('dbo.Ventas','ProvinciaIdVenta') IS NULL
BEGIN
    PRINT '12. (Opcional) Creando Ventas.ProvinciaIdVenta...';
    ALTER TABLE dbo.Ventas ADD ProvinciaIdVenta INT NULL;
END

-- 5.2) FK Ventas -> Localidades
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Ventas_Localidades' AND parent_object_id = OBJECT_ID('dbo.Ventas')
)
BEGIN
    PRINT '13. Creando FK_Ventas_Localidades...';
    ALTER TABLE dbo.Ventas
      ADD CONSTRAINT FK_Ventas_Localidades
          FOREIGN KEY (LocalidadIdVenta) REFERENCES dbo.Localidades(Id);
END

-- 5.3) Completar por defecto desde el Cliente si está NULL
PRINT '14. Completando datos de venta desde el Cliente cuando están NULL...';
UPDATE v
SET  v.DireccionVenta   = ISNULL(v.DireccionVenta,  c.Direccion),
     v.LocalidadIdVenta = ISNULL(v.LocalidadIdVenta, c.LocalidadId),
     v.ProvinciaIdVenta = ISNULL(v.ProvinciaIdVenta, l.ProvinciaId)
FROM dbo.Ventas v
JOIN dbo.Clientes c   ON c.Id = v.ClienteId
LEFT JOIN dbo.Localidades l ON l.Id = c.LocalidadId;

COMMIT TRAN;

PRINT '¡Script completado con éxito!';
GO
