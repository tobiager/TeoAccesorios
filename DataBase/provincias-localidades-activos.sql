-- Provincias.Activo
IF COL_LENGTH('dbo.Provincias', 'Activo') IS NULL
BEGIN
    ALTER TABLE dbo.Provincias
      ADD Activo BIT NOT NULL CONSTRAINT DF_Provincias_Activo DEFAULT (1);
END

-- Localidades.Activo
IF COL_LENGTH('dbo.Localidades', 'Activo') IS NULL
BEGIN
    ALTER TABLE dbo.Localidades
      ADD Activo BIT NOT NULL CONSTRAINT DF_Localidades_Activo DEFAULT (1);
END
