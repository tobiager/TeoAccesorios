-- ============================================================
-- Script de Diagnóstico - Login con Hashing
-- ============================================================

USE TeoAccesorios;
GO

PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║              DIAGNÓSTICO DE LOGIN                         ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
PRINT '';

-- ============================================================
-- TEST 1: Verificar usuario admin
-- ============================================================
PRINT '1️⃣ Datos del usuario admin:';
SELECT 
    Id,
    NombreUsuario,
    Rol,
    Activo,
    LEN(contrasenia) AS TamañoHash,
    CONVERT(VARCHAR(64), contrasenia, 2) AS HashCompleto
FROM dbo.Usuarios
WHERE NombreUsuario = 'admin';

PRINT '';

-- ============================================================
-- TEST 2: Hash esperado vs hash real
-- ============================================================
PRINT '2️⃣ Comparación de hashes:';

DECLARE @passwordTest NVARCHAR(50) = 'admin123';
DECLARE @hashSQL VARBINARY(32) = dbo.HashPassword(@passwordTest);
DECLARE @hashReal VARBINARY(32) = (SELECT contrasenia FROM dbo.Usuarios WHERE NombreUsuario = 'admin');

SELECT 
    'Hash generado por SQL' AS Origen,
    CONVERT(VARCHAR(64), @hashSQL, 2) AS Hash
UNION ALL
SELECT 
    'Hash en base de datos',
    CONVERT(VARCHAR(64), @hashReal, 2)
UNION ALL
SELECT
    '¿Coinciden?',
    CASE WHEN @hashSQL = @hashReal THEN '✅ SÍ' ELSE '❌ NO' END;

PRINT '';

-- ============================================================
-- TEST 3: Simular login como lo haría C#
-- ============================================================
PRINT '3️⃣ Simulando login desde C#:';

DECLARE @usuario NVARCHAR(50) = 'admin';
DECLARE @password NVARCHAR(50) = 'admin123';
DECLARE @passwordHashCSharp VARBINARY(32);

-- Simular el hash que debería generar C# (SHA256)
SET @passwordHashCSharp = HASHBYTES('SHA2_256', @password);

PRINT 'Usuario intentando login: ' + @usuario;
PRINT 'Contraseña: ' + @password;
PRINT 'Hash que C# debería generar: ' + CONVERT(VARCHAR(64), @passwordHashCSharp, 2);
PRINT '';

-- Intentar login
IF EXISTS (
    SELECT 1 
    FROM dbo.Usuarios 
    WHERE nombreUsuario = @usuario 
      AND contrasenia = @passwordHashCSharp
      AND activo = 1
)
BEGIN
    SELECT '✅ LOGIN EXITOSO' AS Resultado;
    SELECT 
        Id,
        NombreUsuario AS Usuario,
        Rol,
        'Login correcto con admin123' AS Mensaje
    FROM dbo.Usuarios
    WHERE NombreUsuario = @usuario;
END
ELSE
BEGIN
    SELECT '❌ LOGIN FALLÓ' AS Resultado;
    
    -- Verificar cada condición
    PRINT '';
    PRINT '🔍 Análisis detallado:';
    
    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombreUsuario = @usuario)
        PRINT '   ❌ El usuario NO existe';
    ELSE
        PRINT '   ✅ El usuario existe';
    
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombreUsuario = @usuario AND activo = 0)
        PRINT '   ❌ El usuario está INACTIVO';
    ELSE
        PRINT '   ✅ El usuario está activo';
    
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombreUsuario = @usuario AND contrasenia != @passwordHashCSharp)
        PRINT '   ❌ La contraseña NO coincide (problema de hash)';
    ELSE
        PRINT '   ✅ La contraseña coincide';
END

PRINT '';

-- ============================================================
-- TEST 4: Probar otros usuarios
-- ============================================================
PRINT '4️⃣ Test de todos los usuarios activos:';

SELECT 
    NombreUsuario,
    Rol,
    Activo,
    CASE 
        WHEN Rol = 'Gerente' THEN 
            CASE WHEN contrasenia = HASHBYTES('SHA2_256', 'gerente123') THEN '✅ gerente123 OK' ELSE '❌ gerente123 FALLO' END
        WHEN Rol = 'Admin' THEN 
            CASE WHEN contrasenia = HASHBYTES('SHA2_256', 'admin123') THEN '✅ admin123 OK' ELSE '❌ admin123 FALLO' END
        WHEN Rol = 'Vendedor' THEN 
            CASE WHEN contrasenia = HASHBYTES('SHA2_256', 'vendedor123') THEN '✅ vendedor123 OK' ELSE '❌ vendedor123 FALLO' END
        ELSE '⚠️ Rol desconocido'
    END AS EstadoPassword
FROM dbo.Usuarios
WHERE Activo = 1
ORDER BY 
    CASE Rol
        WHEN 'Gerente' THEN 1
        WHEN 'Admin' THEN 2
        WHEN 'Vendedor' THEN 3
        ELSE 4
    END;

PRINT '';

-- ============================================================
-- TEST 5: Verificar encoding de caracteres
-- ============================================================
PRINT '5️⃣ Verificación de encoding:';

-- Probar diferentes encodings de la misma contraseña
DECLARE @testPassword NVARCHAR(50) = 'admin123';

SELECT 
    'SHA2_256 directo' AS Método,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @testPassword), 2) AS Hash
UNION ALL
SELECT 
    'Con CAST a VARCHAR',
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(@testPassword AS VARCHAR(50))), 2)
UNION ALL
SELECT 
    'Con CAST a NVARCHAR',
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(@testPassword AS NVARCHAR(50))), 2);

PRINT '';
PRINT '╔═══════════════════════════════════════════════════════════╗';
PRINT '║                    CONCLUSIÓN                             ║';
PRINT '╚═══════════════════════════════════════════════════════════╝';
GO