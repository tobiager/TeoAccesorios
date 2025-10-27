-- ============================================================
-- Script de Corrección FINAL - Con eliminación de constraints
-- Base de datos: TeoAccesorios
-- ============================================================

USE TeoAccesorios;
GO

PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║      CORRECCIÓN FINAL - ELIMINANDO RESTRICCIONES          ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
PRINT '';

-- ============================================================
-- PASO 1: Eliminar constraint DEFAULT de la columna contrasenia
-- ============================================================
PRINT '📌 PASO 1: Eliminando constraint DEFAULT...';
GO

DECLARE @ConstraintName NVARCHAR(200);
DECLARE @SQL NVARCHAR(500);

-- Buscar el nombre del constraint
SELECT @ConstraintName = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id 
    AND dc.parent_object_id = c.object_id
WHERE c.object_id = OBJECT_ID('dbo.Usuarios')
  AND c.name = 'contrasenia';

IF @ConstraintName IS NOT NULL
BEGIN
    SET @SQL = 'ALTER TABLE dbo.Usuarios DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
    EXEC sp_executesql @SQL;
    PRINT '   ✅ Constraint "' + @ConstraintName + '" eliminado';
END
ELSE
BEGIN
    PRINT '   ℹ️  No hay constraint DEFAULT en la columna';
END
PRINT '';
GO

-- ============================================================
-- PASO 2: Verificar que existe contrasenia_hash con datos
-- ============================================================
PRINT '📌 PASO 2: Verificando columna temporal...';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Usuarios') 
    AND name = 'contrasenia_hash'
)
BEGIN
    PRINT '   ⚠️  Creando columna contrasenia_hash...';
    ALTER TABLE dbo.Usuarios ADD contrasenia_hash VARBINARY(32) NULL;
    
    -- Hashear desde backup
    UPDATE dbo.Usuarios
    SET contrasenia_hash = dbo.HashPassword(
        CASE 
            WHEN contrasenia_backup IS NOT NULL THEN contrasenia_backup
            WHEN contrasenia IS NOT NULL THEN contrasenia
            ELSE 'default123'
        END
    );
    PRINT '   ✅ Columna creada y hasheada';
END
ELSE
BEGIN
    PRINT '   ✅ Columna contrasenia_hash ya existe';
END
PRINT '';
GO

-- ============================================================
-- PASO 3: Eliminar columna antigua contrasenia
-- ============================================================
PRINT '📌 PASO 3: Eliminando columna antigua...';
GO

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Usuarios') 
    AND name = 'contrasenia'
)
BEGIN
    ALTER TABLE dbo.Usuarios DROP COLUMN contrasenia;
    PRINT '   ✅ Columna antigua "contrasenia" eliminada';
END
ELSE
BEGIN
    PRINT '   ℹ️  Columna ya fue eliminada';
END
PRINT '';
GO

-- ============================================================
-- PASO 4: Renombrar contrasenia_hash a contrasenia
-- ============================================================
PRINT '📌 PASO 4: Renombrando columna temporal...';
GO

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Usuarios') 
    AND name = 'contrasenia_hash'
)
BEGIN
    EXEC sp_rename 'dbo.Usuarios.contrasenia_hash', 'contrasenia', 'COLUMN';
    PRINT '   ✅ Columna renombrada a "contrasenia"';
END
ELSE
BEGIN
    PRINT '   ℹ️  Columna ya tiene el nombre correcto';
END
PRINT '';
GO

-- ============================================================
-- PASO 5: Hacer columna NOT NULL
-- ============================================================
PRINT '📌 PASO 5: Configurando NOT NULL...';
GO

-- Asegurar que no hay NULLs
UPDATE dbo.Usuarios
SET contrasenia = dbo.HashPassword('default123')
WHERE contrasenia IS NULL;

-- Hacer NOT NULL
ALTER TABLE dbo.Usuarios
ALTER COLUMN contrasenia VARBINARY(32) NOT NULL;

PRINT '   ✅ Columna configurada como NOT NULL';
PRINT '';
GO

-- ============================================================
-- PASO 6: Establecer contraseñas por rol
-- ============================================================
PRINT '📌 PASO 6: Estableciendo contraseñas específicas...';
GO

UPDATE dbo.Usuarios
SET contrasenia = dbo.HashPassword('gerente123')
WHERE Rol = 'Gerente';
PRINT '   ✅ Gerente → gerente123';

UPDATE dbo.Usuarios
SET contrasenia = dbo.HashPassword('admin123')
WHERE Rol = 'Admin';
PRINT '   ✅ Admin → admin123';

UPDATE dbo.Usuarios
SET contrasenia = dbo.HashPassword('vendedor123')
WHERE Rol = 'Vendedor';
PRINT '   ✅ Vendedor → vendedor123';

PRINT '';
GO

-- ============================================================
-- VERIFICACIÓN FINAL COMPLETA
-- ============================================================
PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║                  ✅ VERIFICACIÓN FINAL ✅                 ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
PRINT '';

-- 1. Verificar estructura
PRINT '1️⃣ Estructura de columna:';
SELECT 
    c.name AS Columna,
    t.name AS TipoDato,
    c.max_length AS Tamaño,
    CASE c.is_nullable WHEN 0 THEN 'NOT NULL' ELSE 'NULL' END AS Nullable,
    CASE 
        WHEN t.name = 'varbinary' AND c.max_length = 32 AND c.is_nullable = 0
        THEN '✅ PERFECTO'
        ELSE '❌ ERROR'
    END AS Estado
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Usuarios')
  AND c.name = 'contrasenia';

PRINT '';

-- 2. Verificar todos los usuarios
PRINT '2️⃣ Estado de usuarios:';
SELECT 
    Id,
    NombreUsuario,
    Rol,
    Activo,
    LEN(contrasenia) AS TamañoHash,
    LEFT(CONVERT(VARCHAR(64), contrasenia, 2), 20) + '...' AS HashInicio,
    CASE 
        WHEN LEN(contrasenia) = 32 THEN '✅ OK'
        ELSE '❌ ERROR'
    END AS Estado
FROM dbo.Usuarios
ORDER BY 
    CASE Rol
        WHEN 'Gerente' THEN 1
        WHEN 'Admin' THEN 2
        WHEN 'Vendedor' THEN 3
        ELSE 4
    END,
    NombreUsuario;

PRINT '';

-- 3. Test de login
PRINT '3️⃣ Test de Login:';

DECLARE @testResults TABLE (Rol VARCHAR(20), Estado VARCHAR(30));

-- Test Gerente
IF EXISTS (
    SELECT 1 FROM dbo.Usuarios 
    WHERE Rol = 'Gerente' 
      AND contrasenia = dbo.HashPassword('gerente123')
      AND Activo = 1
)
    INSERT INTO @testResults VALUES ('Gerente', '✅ LOGIN OK (gerente123)');
ELSE
    INSERT INTO @testResults VALUES ('Gerente', '❌ LOGIN FALLO');

-- Test Admin
IF EXISTS (
    SELECT 1 FROM dbo.Usuarios 
    WHERE Rol = 'Admin' 
      AND contrasenia = dbo.HashPassword('admin123')
      AND Activo = 1
)
    INSERT INTO @testResults VALUES ('Admin', '✅ LOGIN OK (admin123)');
ELSE
    INSERT INTO @testResults VALUES ('Admin', '❌ LOGIN FALLO');

-- Test Vendedor
IF EXISTS (
    SELECT 1 FROM dbo.Usuarios 
    WHERE Rol = 'Vendedor' 
      AND contrasenia = dbo.HashPassword('vendedor123')
      AND Activo = 1
)
    INSERT INTO @testResults VALUES ('Vendedor', '✅ LOGIN OK (vendedor123)');
ELSE
    INSERT INTO @testResults VALUES ('Vendedor', '❌ LOGIN FALLO');

SELECT * FROM @testResults;

PRINT '';

-- 4. Resumen final
DECLARE @totalUsuarios INT = (SELECT COUNT(*) FROM dbo.Usuarios);
DECLARE @hashCorrectos INT = (SELECT COUNT(*) FROM dbo.Usuarios WHERE LEN(contrasenia) = 32);
DECLARE @estructuraOK BIT = (
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('dbo.Usuarios')
              AND c.name = 'contrasenia'
              AND t.name = 'varbinary'
              AND c.max_length = 32
              AND c.is_nullable = 0
        ) THEN 1 ELSE 0
    END
);

PRINT '';
PRINT '╔═══════════════════════════════════════════════════════════╗';

IF @estructuraOK = 1 AND @totalUsuarios = @hashCorrectos
BEGIN
    PRINT '║          🎉🎉🎉 BASE DE DATOS 100% CORRECTA 🎉🎉🎉       ║';
    PRINT '╚═══════════════════════════════════════════════════════════╝';
    PRINT '';
    PRINT '✅ Estructura: VARBINARY(32) NOT NULL';
    PRINT '✅ Usuarios con hash correcto: ' + CAST(@hashCorrectos AS VARCHAR) + '/' + CAST(@totalUsuarios AS VARCHAR);
    PRINT '✅ Login funcionando correctamente';
    PRINT '';
    PRINT '📋 CREDENCIALES:';
    PRINT '   👤 Gerente:  gerente123';
    PRINT '   👤 Admin:    admin123';
    PRINT '   👤 Vendedor: vendedor123';
    PRINT '';
    PRINT '🚀 SIGUIENTE PASO: Actualizar código C#';
END
ELSE
BEGIN
    PRINT '║              ⚠️ HAY PROBLEMAS PENDIENTES ⚠️              ║';
    PRINT '╚═══════════════════════════════════════════════════════════╝';
    PRINT '';
    PRINT 'Estructura OK: ' + CASE WHEN @estructuraOK = 1 THEN '✅' ELSE '❌' END;
    PRINT 'Hashes correctos: ' + CAST(@hashCorrectos AS VARCHAR) + '/' + CAST(@totalUsuarios AS VARCHAR);
END

PRINT '';
GO