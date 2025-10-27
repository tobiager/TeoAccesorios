-- ========================================
-- Script de migración para agregar rol Gerente
-- TeoAccesorios - Aplicar a base de datos existente
-- Fecha: 2025-10-27
-- ========================================

USE [TeoAccesorios]
GO

-- ========================================
-- PASO 1: Verificar estructura de tabla Usuarios
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'Rol')
BEGIN
    PRINT 'Error: La columna Rol no existe en la tabla Usuarios'
    RETURN
END
GO

-- ========================================
-- PASO 2: Crear usuario Gerente si no existe
-- ========================================
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'gerente')
BEGIN
    -- Obtener el siguiente ID disponible
    DECLARE @NextId INT
    SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM dbo.Usuarios
    
    SET IDENTITY_INSERT [dbo].[Usuarios] ON
    
    INSERT INTO [dbo].[Usuarios] ([Id], [NombreUsuario], [Rol], [Activo], [correo], [contrasenia])
    VALUES (@NextId, N'gerente', N'Gerente', 1, N'gerente@teoaccesorios.com', N'gerente123')
    
    SET IDENTITY_INSERT [dbo].[Usuarios] OFF
    
    PRINT 'Usuario Gerente creado exitosamente con ID: ' + CAST(@NextId AS VARCHAR(10))
END
ELSE
BEGIN
    -- Si ya existe, actualizar su rol a Gerente
    UPDATE dbo.Usuarios 
    SET Rol = 'Gerente', 
        Activo = 1,
        correo = N'gerente@teoaccesorios.com'
    WHERE NombreUsuario = 'gerente'
    
    PRINT 'Usuario Gerente actualizado exitosamente'
END
GO

-- ========================================
-- PASO 3: Actualizar usuarios Admin existentes (opcional)
-- ========================================
-- Si quieres mantener tu usuario admin actual, descomenta esto:
/*
UPDATE dbo.Usuarios 
SET Rol = 'Admin' 
WHERE NombreUsuario = 'admin' AND Rol != 'Gerente'

PRINT 'Usuarios Admin actualizados'
*/
GO

-- ========================================
-- PASO 4: Crear trigger para proteger al Gerente
-- ========================================
IF OBJECT_ID('trg_ProtegerGerente', 'TR') IS NOT NULL
    DROP TRIGGER trg_ProtegerGerente
GO

CREATE TRIGGER trg_ProtegerGerente
ON dbo.Usuarios
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar si se intenta eliminar al Gerente
    IF EXISTS (SELECT 1 FROM deleted WHERE Rol = 'Gerente')
    BEGIN
        RAISERROR('No se puede eliminar al usuario Gerente.', 16, 1)
        ROLLBACK TRANSACTION
        RETURN
    END
    
    -- Si no es Gerente, permitir la eliminación
    DELETE FROM dbo.Usuarios 
    WHERE Id IN (SELECT Id FROM deleted WHERE Rol != 'Gerente')
END
GO

PRINT 'Trigger de protección creado exitosamente'
GO

-- ========================================
-- PASO 5: Crear constraint para prevenir desactivación del Gerente
-- ========================================
IF OBJECT_ID('chk_GerenteActivo', 'C') IS NOT NULL
    ALTER TABLE dbo.Usuarios DROP CONSTRAINT chk_GerenteActivo
GO

ALTER TABLE dbo.Usuarios
ADD CONSTRAINT chk_GerenteActivo 
CHECK (NOT (Rol = 'Gerente' AND Activo = 0))
GO

PRINT 'Constraint de activación creado exitosamente'
GO

-- ========================================
-- PASO 6: Verificar resultados
-- ========================================
SELECT 
    Id,
    NombreUsuario,
    Rol,
    Activo,
    correo
FROM dbo.Usuarios
ORDER BY 
    CASE Rol 
        WHEN 'Gerente' THEN 1 
        WHEN 'Admin' THEN 2 
        WHEN 'Vendedor' THEN 3 
        ELSE 4 
    END,
    NombreUsuario
GO

PRINT '========================================='
PRINT 'Migración completada exitosamente'
PRINT '========================================='
PRINT 'Usuario Gerente: gerente / gerente123'
PRINT 'Recuerda actualizar tu aplicación .NET'
PRINT '========================================='
GO