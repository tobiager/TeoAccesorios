/* ================================================================
   Inserci�n de 240 localidades (10 por cada una de las 24 provincias)
   ================================================================ */
USE [TeoAccesorios];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

PRINT 'Iniciando inserci�n de 240 localidades (10 por provincia)...';

WITH LocalidadesAInsertar (Localidad, ProvinciaNombre) AS (
    /* ===================== Buenos Aires ===================== */
    SELECT N'La Plata', N'Buenos Aires' UNION ALL
    SELECT N'Mar del Plata', N'Buenos Aires' UNION ALL
    SELECT N'Lomas de Zamora', N'Buenos Aires' UNION ALL
    SELECT N'Quilmes', N'Buenos Aires' UNION ALL
    SELECT N'Bah�a Blanca', N'Buenos Aires' UNION ALL
    SELECT N'Tandil', N'Buenos Aires' UNION ALL
    SELECT N'Pergamino', N'Buenos Aires' UNION ALL
    SELECT N'Jun�n', N'Buenos Aires' UNION ALL
    SELECT N'Mor�n', N'Buenos Aires' UNION ALL
    SELECT N'San Isidro', N'Buenos Aires' UNION ALL

    /* ========== Ciudad Aut�noma de Buenos Aires (CABA) ========== */
    SELECT N'Palermo', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Recoleta', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Belgrano', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Caballito', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Flores', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Mataderos', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Villa Urquiza', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'N��ez', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'San Telmo', N'Ciudad Aut�noma de Buenos Aires' UNION ALL
    SELECT N'Balvanera', N'Ciudad Aut�noma de Buenos Aires' UNION ALL

    /* ===================== Catamarca ===================== */
    SELECT N'San Fernando del Valle de Catamarca', N'Catamarca' UNION ALL
    SELECT N'Valle Viejo', N'Catamarca' UNION ALL
    SELECT N'Fray Mamerto Esqui�', N'Catamarca' UNION ALL
    SELECT N'Bel�n', N'Catamarca' UNION ALL
    SELECT N'Andalgal�', N'Catamarca' UNION ALL
    SELECT N'Tinogasta', N'Catamarca' UNION ALL
    SELECT N'Recreo', N'Catamarca' UNION ALL
    SELECT N'Santa Mar�a', N'Catamarca' UNION ALL
    SELECT N'Fiambal�', N'Catamarca' UNION ALL
    SELECT N'Pom�n', N'Catamarca' UNION ALL

    /* ===================== Chaco ===================== */
    SELECT N'Resistencia', N'Chaco' UNION ALL
    SELECT N'Presidencia Roque S�enz Pe�a', N'Chaco' UNION ALL
    SELECT N'Barranqueras', N'Chaco' UNION ALL
    SELECT N'Fontana', N'Chaco' UNION ALL
    SELECT N'Villa �ngela', N'Chaco' UNION ALL
    SELECT N'Charata', N'Chaco' UNION ALL
    SELECT N'Las Bre�as', N'Chaco' UNION ALL
    SELECT N'General Jos� de San Mart�n', N'Chaco' UNION ALL
    SELECT N'Juan Jos� Castelli', N'Chaco' UNION ALL
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

    /* ===================== C�rdoba ===================== */
    SELECT N'C�rdoba', N'C�rdoba' UNION ALL
    SELECT N'R�o Cuarto', N'C�rdoba' UNION ALL
    SELECT N'Villa Carlos Paz', N'C�rdoba' UNION ALL
    SELECT N'Alta Gracia', N'C�rdoba' UNION ALL
    SELECT N'San Francisco', N'C�rdoba' UNION ALL
    SELECT N'Villa Mar�a', N'C�rdoba' UNION ALL
    SELECT N'R�o Tercero', N'C�rdoba' UNION ALL
    SELECT N'La Falda', N'C�rdoba' UNION ALL
    SELECT N'Jes�s Mar�a', N'C�rdoba' UNION ALL
    SELECT N'Cosqu�n', N'C�rdoba' UNION ALL

    /* ===================== Corrientes ===================== */
    SELECT N'Corrientes', N'Corrientes' UNION ALL
    SELECT N'Goya', N'Corrientes' UNION ALL
    SELECT N'Paso de los Libres', N'Corrientes' UNION ALL
    SELECT N'Curuz� Cuati�', N'Corrientes' UNION ALL
    SELECT N'Mercedes', N'Corrientes' UNION ALL
    SELECT N'Bella Vista', N'Corrientes' UNION ALL
    SELECT N'Esquina', N'Corrientes' UNION ALL
    SELECT N'Santo Tom�', N'Corrientes' UNION ALL
    SELECT N'Gobernador Virasoro', N'Corrientes' UNION ALL
    SELECT N'Ituzaing�', N'Corrientes' UNION ALL

    /* ===================== Entre R�os ===================== */
    SELECT N'Paran�', N'Entre R�os' UNION ALL
    SELECT N'Concordia', N'Entre R�os' UNION ALL
    SELECT N'Gualeguaych�', N'Entre R�os' UNION ALL
    SELECT N'Concepci�n del Uruguay', N'Entre R�os' UNION ALL
    SELECT N'Gualeguay', N'Entre R�os' UNION ALL
    SELECT N'Villaguay', N'Entre R�os' UNION ALL
    SELECT N'Victoria', N'Entre R�os' UNION ALL
    SELECT N'Chajar�', N'Entre R�os' UNION ALL
    SELECT N'La Paz', N'Entre R�os' UNION ALL
    SELECT N'Diamante', N'Entre R�os' UNION ALL

    /* ===================== Formosa ===================== */
    SELECT N'Formosa', N'Formosa' UNION ALL
    SELECT N'Clorinda', N'Formosa' UNION ALL
    SELECT N'Piran�', N'Formosa' UNION ALL
    SELECT N'El Colorado', N'Formosa' UNION ALL
    SELECT N'Ibarreta', N'Formosa' UNION ALL
    SELECT N'Laguna Blanca', N'Formosa' UNION ALL
    SELECT N'Ingeniero Ju�rez', N'Formosa' UNION ALL
    SELECT N'Las Lomitas', N'Formosa' UNION ALL
    SELECT N'Comandante Fontana', N'Formosa' UNION ALL
    SELECT N'Buena Vista', N'Formosa' UNION ALL

    /* ===================== Jujuy ===================== */
    SELECT N'San Salvador de Jujuy', N'Jujuy' UNION ALL
    SELECT N'Palpal�', N'Jujuy' UNION ALL
    SELECT N'Perico', N'Jujuy' UNION ALL
    SELECT N'Libertador General San Mart�n', N'Jujuy' UNION ALL
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
    SELECT N'Realic�', N'La Pampa' UNION ALL
    SELECT N'Eduardo Castex', N'La Pampa' UNION ALL
    SELECT N'General Acha', N'La Pampa' UNION ALL
    SELECT N'Veinticinco de Mayo', N'La Pampa' UNION ALL
    SELECT N'Macach�n', N'La Pampa' UNION ALL
    SELECT N'Winifreda', N'La Pampa' UNION ALL
    SELECT N'Catril�', N'La Pampa' UNION ALL

    /* ===================== La Rioja ===================== */
    SELECT N'La Rioja', N'La Rioja' UNION ALL
    SELECT N'Chilecito', N'La Rioja' UNION ALL
    SELECT N'Aimogasta', N'La Rioja' UNION ALL
    SELECT N'Chamical', N'La Rioja' UNION ALL
    SELECT N'Chepes', N'La Rioja' UNION ALL
    SELECT N'Villa Uni�n', N'La Rioja' UNION ALL
    SELECT N'Nonogasta', N'La Rioja' UNION ALL
    SELECT N'Ulapes', N'La Rioja' UNION ALL
    SELECT N'Anillaco', N'La Rioja' UNION ALL
    SELECT N'Famatina', N'La Rioja' UNION ALL

    /* ===================== Mendoza ===================== */
    SELECT N'Mendoza', N'Mendoza' UNION ALL
    SELECT N'Godoy Cruz', N'Mendoza' UNION ALL
    SELECT N'Guaymall�n', N'Mendoza' UNION ALL
    SELECT N'Luj�n de Cuyo', N'Mendoza' UNION ALL
    SELECT N'Las Heras', N'Mendoza' UNION ALL
    SELECT N'San Mart�n', N'Mendoza' UNION ALL
    SELECT N'San Rafael', N'Mendoza' UNION ALL
    SELECT N'Tunuy�n', N'Mendoza' UNION ALL
    SELECT N'Tupungato', N'Mendoza' UNION ALL
    SELECT N'Malarg�e', N'Mendoza' UNION ALL

    /* ===================== Misiones ===================== */
    SELECT N'Posadas', N'Misiones' UNION ALL
    SELECT N'Puerto Iguaz�', N'Misiones' UNION ALL
    SELECT N'Eldorado', N'Misiones' UNION ALL
    SELECT N'Ober�', N'Misiones' UNION ALL
    SELECT N'Ap�stoles', N'Misiones' UNION ALL
    SELECT N'San Vicente', N'Misiones' UNION ALL
    SELECT N'Jard�n Am�rica', N'Misiones' UNION ALL
    SELECT N'Puerto Rico', N'Misiones' UNION ALL
    SELECT N'Candelaria', N'Misiones' UNION ALL
    SELECT N'Leandro N. Alem', N'Misiones' UNION ALL

    /* ===================== Neuqu�n ===================== */
    SELECT N'Neuqu�n', N'Neuqu�n' UNION ALL
    SELECT N'Plottier', N'Neuqu�n' UNION ALL
    SELECT N'Centenario', N'Neuqu�n' UNION ALL
    SELECT N'Cutral C�', N'Neuqu�n' UNION ALL
    SELECT N'Plaza Huincul', N'Neuqu�n' UNION ALL
    SELECT N'San Mart�n de los Andes', N'Neuqu�n' UNION ALL
    SELECT N'Jun�n de los Andes', N'Neuqu�n' UNION ALL
    SELECT N'Zapala', N'Neuqu�n' UNION ALL
    SELECT N'Chos Malal', N'Neuqu�n' UNION ALL
    SELECT N'Rinc�n de los Sauces', N'Neuqu�n' UNION ALL

    /* ===================== R�o Negro ===================== */
    SELECT N'Viedma', N'R�o Negro' UNION ALL
    SELECT N'General Roca', N'R�o Negro' UNION ALL
    SELECT N'Cipolletti', N'R�o Negro' UNION ALL
    SELECT N'San Carlos de Bariloche', N'R�o Negro' UNION ALL
    SELECT N'Allen', N'R�o Negro' UNION ALL
    SELECT N'Villa Regina', N'R�o Negro' UNION ALL
    SELECT N'Choele Choel', N'R�o Negro' UNION ALL
    SELECT N'San Antonio Oeste', N'R�o Negro' UNION ALL
    SELECT N'El Bols�n', N'R�o Negro' UNION ALL
    SELECT N'R�o Colorado', N'R�o Negro' UNION ALL

    /* ===================== Salta ===================== */
    SELECT N'Salta', N'Salta' UNION ALL
    SELECT N'San Ram�n de la Nueva Or�n', N'Salta' UNION ALL
    SELECT N'Tartagal', N'Salta' UNION ALL
    SELECT N'General G�emes', N'Salta' UNION ALL
    SELECT N'Rosario de la Frontera', N'Salta' UNION ALL
    SELECT N'Joaqu�n V. Gonz�lez', N'Salta' UNION ALL
    SELECT N'Cafayate', N'Salta' UNION ALL
    SELECT N'Met�n', N'Salta' UNION ALL
    SELECT N'Cerrillos', N'Salta' UNION ALL
    SELECT N'Coronel Moldes', N'Salta' UNION ALL

    /* ===================== San Juan ===================== */
    SELECT N'San Juan', N'San Juan' UNION ALL
    SELECT N'Rawson', N'San Juan' UNION ALL
    SELECT N'Chimbas', N'San Juan' UNION ALL
    SELECT N'Santa Luc�a', N'San Juan' UNION ALL
    SELECT N'Pocito', N'San Juan' UNION ALL
    SELECT N'Rivadavia', N'San Juan' UNION ALL
    SELECT N'Albard�n', N'San Juan' UNION ALL
    SELECT N'Caucete', N'San Juan' UNION ALL
    SELECT N'J�chal', N'San Juan' UNION ALL
    SELECT N'Sarmiento', N'San Juan' UNION ALL

    /* ===================== San Luis ===================== */
    SELECT N'San Luis', N'San Luis' UNION ALL
    SELECT N'Villa Mercedes', N'San Luis' UNION ALL
    SELECT N'Merlo', N'San Luis' UNION ALL
    SELECT N'La Punta', N'San Luis' UNION ALL
    SELECT N'Justo Daract', N'San Luis' UNION ALL
    SELECT N'Juana Koslay', N'San Luis' UNION ALL
    SELECT N'Tilisarao', N'San Luis' UNION ALL
    SELECT N'Concar�n', N'San Luis' UNION ALL
    SELECT N'Buena Esperanza', N'San Luis' UNION ALL
    SELECT N'Quines', N'San Luis' UNION ALL

    /* ===================== Santa Cruz ===================== */
    SELECT N'R�o Gallegos', N'Santa Cruz' UNION ALL
    SELECT N'Caleta Olivia', N'Santa Cruz' UNION ALL
    SELECT N'El Calafate', N'Santa Cruz' UNION ALL
    SELECT N'Pico Truncado', N'Santa Cruz' UNION ALL
    SELECT N'Puerto Deseado', N'Santa Cruz' UNION ALL
    SELECT N'Las Heras', N'Santa Cruz' UNION ALL
    SELECT N'Perito Moreno', N'Santa Cruz' UNION ALL
    SELECT N'Gobernador Gregores', N'Santa Cruz' UNION ALL
    SELECT N'Puerto San Juli�n', N'Santa Cruz' UNION ALL
    SELECT N'R�o Turbio', N'Santa Cruz' UNION ALL

    /* ===================== Santa Fe ===================== */
    SELECT N'Santa Fe', N'Santa Fe' UNION ALL
    SELECT N'Rosario', N'Santa Fe' UNION ALL
    SELECT N'Venado Tuerto', N'Santa Fe' UNION ALL
    SELECT N'Rafaela', N'Santa Fe' UNION ALL
    SELECT N'Reconquista', N'Santa Fe' UNION ALL
    SELECT N'Esperanza', N'Santa Fe' UNION ALL
    SELECT N'San Justo', N'Santa Fe' UNION ALL
    SELECT N'Ca�ada de G�mez', N'Santa Fe' UNION ALL
    SELECT N'Villa Constituci�n', N'Santa Fe' UNION ALL
    SELECT N'G�lvez', N'Santa Fe' UNION ALL

    /* ===================== Santiago del Estero ===================== */
    SELECT N'Santiago del Estero', N'Santiago del Estero' UNION ALL
    SELECT N'La Banda', N'Santiago del Estero' UNION ALL
    SELECT N'Termas de R�o Hondo', N'Santiago del Estero' UNION ALL
    SELECT N'Fr�as', N'Santiago del Estero' UNION ALL
    SELECT N'A�atuya', N'Santiago del Estero' UNION ALL
    SELECT N'Quimil�', N'Santiago del Estero' UNION ALL
    SELECT N'Fern�ndez', N'Santiago del Estero' UNION ALL
    SELECT N'Loreto', N'Santiago del Estero' UNION ALL
    SELECT N'Campo Gallo', N'Santiago del Estero' UNION ALL
    SELECT N'Monte Quemado', N'Santiago del Estero' UNION ALL

    /* ===================== Tierra del Fuego ===================== */
    /* Si tu tabla usa el nombre largo, c�mbialo por:
       'Tierra del Fuego, Ant�rtida e Islas del Atl�ntico Sur' */
    SELECT N'Ushuaia', N'Tierra del Fuego' UNION ALL
    SELECT N'R�o Grande', N'Tierra del Fuego' UNION ALL
    SELECT N'Tolhuin', N'Tierra del Fuego' UNION ALL
    SELECT N'Lago Escondido', N'Tierra del Fuego' UNION ALL
    SELECT N'Puerto Almanza', N'Tierra del Fuego' UNION ALL
    SELECT N'San Sebasti�n', N'Tierra del Fuego' UNION ALL
    SELECT N'Cabo San Pablo', N'Tierra del Fuego' UNION ALL
    SELECT N'Estancia Harberton', N'Tierra del Fuego' UNION ALL
    SELECT N'Rancho Hambre', N'Tierra del Fuego' UNION ALL
    SELECT N'Las Cotorras', N'Tierra del Fuego' UNION ALL

    /* ===================== Tucum�n ===================== */
    SELECT N'San Miguel de Tucum�n', N'Tucum�n' UNION ALL
    SELECT N'Yerba Buena', N'Tucum�n' UNION ALL
    SELECT N'Taf� Viejo', N'Tucum�n' UNION ALL
    SELECT N'Concepci�n', N'Tucum�n' UNION ALL
    SELECT N'Aguilares', N'Tucum�n' UNION ALL
    SELECT N'Monteros', N'Tucum�n' UNION ALL
    SELECT N'Famaill�', N'Tucum�n' UNION ALL
    SELECT N'Las Talitas', N'Tucum�n' UNION ALL
    SELECT N'Simoca', N'Tucum�n' UNION ALL
    SELECT N'Lules', N'Tucum�n'
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
PRINT 'Inserci�n finalizada. Se agregaron ' + CAST(@FilasAfectadas AS NVARCHAR(10)) + ' localidades nuevas.';

COMMIT TRAN;
GO
