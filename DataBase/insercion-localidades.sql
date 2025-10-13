/* ================================================================
   Inserción de 240 localidades (10 por cada una de las 24 provincias)
   ================================================================ */
USE [TeoAccesorios];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

PRINT 'Iniciando inserción de 240 localidades (10 por provincia)...';

WITH LocalidadesAInsertar (Localidad, ProvinciaNombre) AS (
    /* ===================== Buenos Aires ===================== */
    SELECT N'La Plata', N'Buenos Aires' UNION ALL
    SELECT N'Mar del Plata', N'Buenos Aires' UNION ALL
    SELECT N'Lomas de Zamora', N'Buenos Aires' UNION ALL
    SELECT N'Quilmes', N'Buenos Aires' UNION ALL
    SELECT N'Bahía Blanca', N'Buenos Aires' UNION ALL
    SELECT N'Tandil', N'Buenos Aires' UNION ALL
    SELECT N'Pergamino', N'Buenos Aires' UNION ALL
    SELECT N'Junín', N'Buenos Aires' UNION ALL
    SELECT N'Morón', N'Buenos Aires' UNION ALL
    SELECT N'San Isidro', N'Buenos Aires' UNION ALL

    /* ========== Ciudad Autónoma de Buenos Aires (CABA) ========== */
    SELECT N'Palermo', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Recoleta', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Belgrano', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Caballito', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Flores', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Mataderos', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Villa Urquiza', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Núñez', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'San Telmo', N'Ciudad Autónoma de Buenos Aires' UNION ALL
    SELECT N'Balvanera', N'Ciudad Autónoma de Buenos Aires' UNION ALL

    /* ===================== Catamarca ===================== */
    SELECT N'San Fernando del Valle de Catamarca', N'Catamarca' UNION ALL
    SELECT N'Valle Viejo', N'Catamarca' UNION ALL
    SELECT N'Fray Mamerto Esquiú', N'Catamarca' UNION ALL
    SELECT N'Belén', N'Catamarca' UNION ALL
    SELECT N'Andalgalá', N'Catamarca' UNION ALL
    SELECT N'Tinogasta', N'Catamarca' UNION ALL
    SELECT N'Recreo', N'Catamarca' UNION ALL
    SELECT N'Santa María', N'Catamarca' UNION ALL
    SELECT N'Fiambalá', N'Catamarca' UNION ALL
    SELECT N'Pomán', N'Catamarca' UNION ALL

    /* ===================== Chaco ===================== */
    SELECT N'Resistencia', N'Chaco' UNION ALL
    SELECT N'Presidencia Roque Sáenz Peña', N'Chaco' UNION ALL
    SELECT N'Barranqueras', N'Chaco' UNION ALL
    SELECT N'Fontana', N'Chaco' UNION ALL
    SELECT N'Villa Ángela', N'Chaco' UNION ALL
    SELECT N'Charata', N'Chaco' UNION ALL
    SELECT N'Las Breñas', N'Chaco' UNION ALL
    SELECT N'General José de San Martín', N'Chaco' UNION ALL
    SELECT N'Juan José Castelli', N'Chaco' UNION ALL
    SELECT N'Machagai', N'Chaco' UNION ALL

    /* ===================== Chubut ===================== */
    SELECT N'Rawson', N'Chubut' UNION ALL
    SELECT N'Trelew', N'Chubut' UNION ALL
    SELECT N'Puerto Madryn', N'Chubut' UNION ALL
    SELECT N'Comodoro Rivadavia', N'Chubut' UNION ALL
    SELECT N'Esquel', N'Chubut' UNION ALL
    SELECT N'Sarmiento', N'Chubut' UNION ALL
    SELECT N'Gaiman', N'Chubut' UNION ALL
    SELECT N'Dolavon', N'Chubut' UNION ALL
    SELECT N'Lago Puelo', N'Chubut' UNION ALL
    SELECT N'El Hoyo', N'Chubut' UNION ALL

    /* ===================== Córdoba ===================== */
    SELECT N'Córdoba', N'Córdoba' UNION ALL
    SELECT N'Río Cuarto', N'Córdoba' UNION ALL
    SELECT N'Villa Carlos Paz', N'Córdoba' UNION ALL
    SELECT N'Alta Gracia', N'Córdoba' UNION ALL
    SELECT N'San Francisco', N'Córdoba' UNION ALL
    SELECT N'Villa María', N'Córdoba' UNION ALL
    SELECT N'Río Tercero', N'Córdoba' UNION ALL
    SELECT N'La Falda', N'Córdoba' UNION ALL
    SELECT N'Jesús María', N'Córdoba' UNION ALL
    SELECT N'Cosquín', N'Córdoba' UNION ALL

    /* ===================== Corrientes ===================== */
    SELECT N'Corrientes', N'Corrientes' UNION ALL
    SELECT N'Goya', N'Corrientes' UNION ALL
    SELECT N'Paso de los Libres', N'Corrientes' UNION ALL
    SELECT N'Curuzú Cuatiá', N'Corrientes' UNION ALL
    SELECT N'Mercedes', N'Corrientes' UNION ALL
    SELECT N'Bella Vista', N'Corrientes' UNION ALL
    SELECT N'Esquina', N'Corrientes' UNION ALL
    SELECT N'Santo Tomé', N'Corrientes' UNION ALL
    SELECT N'Gobernador Virasoro', N'Corrientes' UNION ALL
    SELECT N'Ituzaingó', N'Corrientes' UNION ALL

    /* ===================== Entre Ríos ===================== */
    SELECT N'Paraná', N'Entre Ríos' UNION ALL
    SELECT N'Concordia', N'Entre Ríos' UNION ALL
    SELECT N'Gualeguaychú', N'Entre Ríos' UNION ALL
    SELECT N'Concepción del Uruguay', N'Entre Ríos' UNION ALL
    SELECT N'Gualeguay', N'Entre Ríos' UNION ALL
    SELECT N'Villaguay', N'Entre Ríos' UNION ALL
    SELECT N'Victoria', N'Entre Ríos' UNION ALL
    SELECT N'Chajarí', N'Entre Ríos' UNION ALL
    SELECT N'La Paz', N'Entre Ríos' UNION ALL
    SELECT N'Diamante', N'Entre Ríos' UNION ALL

    /* ===================== Formosa ===================== */
    SELECT N'Formosa', N'Formosa' UNION ALL
    SELECT N'Clorinda', N'Formosa' UNION ALL
    SELECT N'Pirané', N'Formosa' UNION ALL
    SELECT N'El Colorado', N'Formosa' UNION ALL
    SELECT N'Ibarreta', N'Formosa' UNION ALL
    SELECT N'Laguna Blanca', N'Formosa' UNION ALL
    SELECT N'Ingeniero Juárez', N'Formosa' UNION ALL
    SELECT N'Las Lomitas', N'Formosa' UNION ALL
    SELECT N'Comandante Fontana', N'Formosa' UNION ALL
    SELECT N'Buena Vista', N'Formosa' UNION ALL

    /* ===================== Jujuy ===================== */
    SELECT N'San Salvador de Jujuy', N'Jujuy' UNION ALL
    SELECT N'Palpalá', N'Jujuy' UNION ALL
    SELECT N'Perico', N'Jujuy' UNION ALL
    SELECT N'Libertador General San Martín', N'Jujuy' UNION ALL
    SELECT N'San Pedro de Jujuy', N'Jujuy' UNION ALL
    SELECT N'Tilcara', N'Jujuy' UNION ALL
    SELECT N'Humahuaca', N'Jujuy' UNION ALL
    SELECT N'La Quiaca', N'Jujuy' UNION ALL
    SELECT N'Monterrico', N'Jujuy' UNION ALL
    SELECT N'Yuto', N'Jujuy' UNION ALL

    /* ===================== La Pampa ===================== */
    SELECT N'Santa Rosa', N'La Pampa' UNION ALL
    SELECT N'General Pico', N'La Pampa' UNION ALL
    SELECT N'Toay', N'La Pampa' UNION ALL
    SELECT N'Realicó', N'La Pampa' UNION ALL
    SELECT N'Eduardo Castex', N'La Pampa' UNION ALL
    SELECT N'General Acha', N'La Pampa' UNION ALL
    SELECT N'Veinticinco de Mayo', N'La Pampa' UNION ALL
    SELECT N'Macachín', N'La Pampa' UNION ALL
    SELECT N'Winifreda', N'La Pampa' UNION ALL
    SELECT N'Catriló', N'La Pampa' UNION ALL

    /* ===================== La Rioja ===================== */
    SELECT N'La Rioja', N'La Rioja' UNION ALL
    SELECT N'Chilecito', N'La Rioja' UNION ALL
    SELECT N'Aimogasta', N'La Rioja' UNION ALL
    SELECT N'Chamical', N'La Rioja' UNION ALL
    SELECT N'Chepes', N'La Rioja' UNION ALL
    SELECT N'Villa Unión', N'La Rioja' UNION ALL
    SELECT N'Nonogasta', N'La Rioja' UNION ALL
    SELECT N'Ulapes', N'La Rioja' UNION ALL
    SELECT N'Anillaco', N'La Rioja' UNION ALL
    SELECT N'Famatina', N'La Rioja' UNION ALL

    /* ===================== Mendoza ===================== */
    SELECT N'Mendoza', N'Mendoza' UNION ALL
    SELECT N'Godoy Cruz', N'Mendoza' UNION ALL
    SELECT N'Guaymallén', N'Mendoza' UNION ALL
    SELECT N'Luján de Cuyo', N'Mendoza' UNION ALL
    SELECT N'Las Heras', N'Mendoza' UNION ALL
    SELECT N'San Martín', N'Mendoza' UNION ALL
    SELECT N'San Rafael', N'Mendoza' UNION ALL
    SELECT N'Tunuyán', N'Mendoza' UNION ALL
    SELECT N'Tupungato', N'Mendoza' UNION ALL
    SELECT N'Malargüe', N'Mendoza' UNION ALL

    /* ===================== Misiones ===================== */
    SELECT N'Posadas', N'Misiones' UNION ALL
    SELECT N'Puerto Iguazú', N'Misiones' UNION ALL
    SELECT N'Eldorado', N'Misiones' UNION ALL
    SELECT N'Oberá', N'Misiones' UNION ALL
    SELECT N'Apóstoles', N'Misiones' UNION ALL
    SELECT N'San Vicente', N'Misiones' UNION ALL
    SELECT N'Jardín América', N'Misiones' UNION ALL
    SELECT N'Puerto Rico', N'Misiones' UNION ALL
    SELECT N'Candelaria', N'Misiones' UNION ALL
    SELECT N'Leandro N. Alem', N'Misiones' UNION ALL

    /* ===================== Neuquén ===================== */
    SELECT N'Neuquén', N'Neuquén' UNION ALL
    SELECT N'Plottier', N'Neuquén' UNION ALL
    SELECT N'Centenario', N'Neuquén' UNION ALL
    SELECT N'Cutral Có', N'Neuquén' UNION ALL
    SELECT N'Plaza Huincul', N'Neuquén' UNION ALL
    SELECT N'San Martín de los Andes', N'Neuquén' UNION ALL
    SELECT N'Junín de los Andes', N'Neuquén' UNION ALL
    SELECT N'Zapala', N'Neuquén' UNION ALL
    SELECT N'Chos Malal', N'Neuquén' UNION ALL
    SELECT N'Rincón de los Sauces', N'Neuquén' UNION ALL

    /* ===================== Río Negro ===================== */
    SELECT N'Viedma', N'Río Negro' UNION ALL
    SELECT N'General Roca', N'Río Negro' UNION ALL
    SELECT N'Cipolletti', N'Río Negro' UNION ALL
    SELECT N'San Carlos de Bariloche', N'Río Negro' UNION ALL
    SELECT N'Allen', N'Río Negro' UNION ALL
    SELECT N'Villa Regina', N'Río Negro' UNION ALL
    SELECT N'Choele Choel', N'Río Negro' UNION ALL
    SELECT N'San Antonio Oeste', N'Río Negro' UNION ALL
    SELECT N'El Bolsón', N'Río Negro' UNION ALL
    SELECT N'Río Colorado', N'Río Negro' UNION ALL

    /* ===================== Salta ===================== */
    SELECT N'Salta', N'Salta' UNION ALL
    SELECT N'San Ramón de la Nueva Orán', N'Salta' UNION ALL
    SELECT N'Tartagal', N'Salta' UNION ALL
    SELECT N'General Güemes', N'Salta' UNION ALL
    SELECT N'Rosario de la Frontera', N'Salta' UNION ALL
    SELECT N'Joaquín V. González', N'Salta' UNION ALL
    SELECT N'Cafayate', N'Salta' UNION ALL
    SELECT N'Metán', N'Salta' UNION ALL
    SELECT N'Cerrillos', N'Salta' UNION ALL
    SELECT N'Coronel Moldes', N'Salta' UNION ALL

    /* ===================== San Juan ===================== */
    SELECT N'San Juan', N'San Juan' UNION ALL
    SELECT N'Rawson', N'San Juan' UNION ALL
    SELECT N'Chimbas', N'San Juan' UNION ALL
    SELECT N'Santa Lucía', N'San Juan' UNION ALL
    SELECT N'Pocito', N'San Juan' UNION ALL
    SELECT N'Rivadavia', N'San Juan' UNION ALL
    SELECT N'Albardón', N'San Juan' UNION ALL
    SELECT N'Caucete', N'San Juan' UNION ALL
    SELECT N'Jáchal', N'San Juan' UNION ALL
    SELECT N'Sarmiento', N'San Juan' UNION ALL

    /* ===================== San Luis ===================== */
    SELECT N'San Luis', N'San Luis' UNION ALL
    SELECT N'Villa Mercedes', N'San Luis' UNION ALL
    SELECT N'Merlo', N'San Luis' UNION ALL
    SELECT N'La Punta', N'San Luis' UNION ALL
    SELECT N'Justo Daract', N'San Luis' UNION ALL
    SELECT N'Juana Koslay', N'San Luis' UNION ALL
    SELECT N'Tilisarao', N'San Luis' UNION ALL
    SELECT N'Concarán', N'San Luis' UNION ALL
    SELECT N'Buena Esperanza', N'San Luis' UNION ALL
    SELECT N'Quines', N'San Luis' UNION ALL

    /* ===================== Santa Cruz ===================== */
    SELECT N'Río Gallegos', N'Santa Cruz' UNION ALL
    SELECT N'Caleta Olivia', N'Santa Cruz' UNION ALL
    SELECT N'El Calafate', N'Santa Cruz' UNION ALL
    SELECT N'Pico Truncado', N'Santa Cruz' UNION ALL
    SELECT N'Puerto Deseado', N'Santa Cruz' UNION ALL
    SELECT N'Las Heras', N'Santa Cruz' UNION ALL
    SELECT N'Perito Moreno', N'Santa Cruz' UNION ALL
    SELECT N'Gobernador Gregores', N'Santa Cruz' UNION ALL
    SELECT N'Puerto San Julián', N'Santa Cruz' UNION ALL
    SELECT N'Río Turbio', N'Santa Cruz' UNION ALL

    /* ===================== Santa Fe ===================== */
    SELECT N'Santa Fe', N'Santa Fe' UNION ALL
    SELECT N'Rosario', N'Santa Fe' UNION ALL
    SELECT N'Venado Tuerto', N'Santa Fe' UNION ALL
    SELECT N'Rafaela', N'Santa Fe' UNION ALL
    SELECT N'Reconquista', N'Santa Fe' UNION ALL
    SELECT N'Esperanza', N'Santa Fe' UNION ALL
    SELECT N'San Justo', N'Santa Fe' UNION ALL
    SELECT N'Cañada de Gómez', N'Santa Fe' UNION ALL
    SELECT N'Villa Constitución', N'Santa Fe' UNION ALL
    SELECT N'Gálvez', N'Santa Fe' UNION ALL

    /* ===================== Santiago del Estero ===================== */
    SELECT N'Santiago del Estero', N'Santiago del Estero' UNION ALL
    SELECT N'La Banda', N'Santiago del Estero' UNION ALL
    SELECT N'Termas de Río Hondo', N'Santiago del Estero' UNION ALL
    SELECT N'Frías', N'Santiago del Estero' UNION ALL
    SELECT N'Añatuya', N'Santiago del Estero' UNION ALL
    SELECT N'Quimilí', N'Santiago del Estero' UNION ALL
    SELECT N'Fernández', N'Santiago del Estero' UNION ALL
    SELECT N'Loreto', N'Santiago del Estero' UNION ALL
    SELECT N'Campo Gallo', N'Santiago del Estero' UNION ALL
    SELECT N'Monte Quemado', N'Santiago del Estero' UNION ALL

    /* ===================== Tierra del Fuego ===================== */
    /* Si tu tabla usa el nombre largo, cámbialo por:
       'Tierra del Fuego, Antártida e Islas del Atlántico Sur' */
    SELECT N'Ushuaia', N'Tierra del Fuego' UNION ALL
    SELECT N'Río Grande', N'Tierra del Fuego' UNION ALL
    SELECT N'Tolhuin', N'Tierra del Fuego' UNION ALL
    SELECT N'Lago Escondido', N'Tierra del Fuego' UNION ALL
    SELECT N'Puerto Almanza', N'Tierra del Fuego' UNION ALL
    SELECT N'San Sebastián', N'Tierra del Fuego' UNION ALL
    SELECT N'Cabo San Pablo', N'Tierra del Fuego' UNION ALL
    SELECT N'Estancia Harberton', N'Tierra del Fuego' UNION ALL
    SELECT N'Rancho Hambre', N'Tierra del Fuego' UNION ALL
    SELECT N'Las Cotorras', N'Tierra del Fuego' UNION ALL

    /* ===================== Tucumán ===================== */
    SELECT N'San Miguel de Tucumán', N'Tucumán' UNION ALL
    SELECT N'Yerba Buena', N'Tucumán' UNION ALL
    SELECT N'Tafí Viejo', N'Tucumán' UNION ALL
    SELECT N'Concepción', N'Tucumán' UNION ALL
    SELECT N'Aguilares', N'Tucumán' UNION ALL
    SELECT N'Monteros', N'Tucumán' UNION ALL
    SELECT N'Famaillá', N'Tucumán' UNION ALL
    SELECT N'Las Talitas', N'Tucumán' UNION ALL
    SELECT N'Simoca', N'Tucumán' UNION ALL
    SELECT N'Lules', N'Tucumán'
)
INSERT INTO dbo.Localidades (Nombre, ProvinciaId)
SELECT 
    L.Localidad,
    P.Id
FROM LocalidadesAInsertar AS L
JOIN dbo.Provincias AS P
  ON P.Nombre = L.ProvinciaNombre
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Localidades X
    WHERE X.Nombre = L.Localidad
      AND X.ProvinciaId = P.Id
);

DECLARE @FilasAfectadas INT = @@ROWCOUNT;
PRINT 'Inserción finalizada. Se agregaron ' + CAST(@FilasAfectadas AS NVARCHAR(10)) + ' localidades nuevas.';

COMMIT TRAN;
GO
