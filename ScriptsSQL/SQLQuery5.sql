
-- CARGA INICIAL DE PRODUCTOS BYTESTORE
-- No duplica productos que ya existan con el mismo nombre.

INSERT INTO Productos
    (Nombre, Descripcion, Precio, Stock, Activo, CategoriaId, MarcaId)
SELECT
    datos.Nombre,
    datos.Descripcion,
    datos.Precio,
    datos.Stock,
    1,
    c.Id,
    m.Id
FROM (VALUES
    -- PROCESADORES (3 nuevos + Ryzen existente)
    ('AMD Ryzen 5 5600', 'Procesador AMD de 6 nucleos y 12 hilos', 180000.00, 12, 'Procesadores', 'AMD'),
    ('Intel Core i5 12400F', 'Procesador Intel de 12va generacion', 210000.00, 8, 'Procesadores', 'Intel'),
    ('Intel Core i7 12700K', 'Procesador Intel de alto rendimiento', 390000.00, 5, 'Procesadores', 'Intel'),

    -- MOTHERBOARDS
    ('ASUS Prime B550M-A', 'Motherboard AM4 con chipset B550', 145000.00, 10, 'Motherboards', 'ASUS'),
    ('MSI B550 Gaming Plus', 'Motherboard AM4 para gaming', 185000.00, 7, 'Motherboards', 'MSI'),
    ('Gigabyte B760M DS3H', 'Motherboard Intel LGA1700', 170000.00, 9, 'Motherboards', 'Gigabyte'),
    ('ASUS TUF Gaming B650', 'Motherboard AM5 con soporte DDR5', 285000.00, 5, 'Motherboards', 'ASUS'),

    -- TARJETAS GRAFICAS
    ('MSI GeForce RTX 4060', 'Tarjeta grafica NVIDIA de 8 GB', 490000.00, 6, 'Tarjetas graficas', 'MSI'),
    ('ASUS GeForce RTX 4070', 'Tarjeta grafica NVIDIA de 12 GB', 850000.00, 4, 'Tarjetas graficas', 'ASUS'),
    ('Gigabyte Radeon RX 7600', 'Tarjeta grafica AMD de 8 GB', 430000.00, 8, 'Tarjetas graficas', 'Gigabyte'),
    ('MSI Radeon RX 6600', 'Tarjeta grafica AMD de 8 GB', 350000.00, 10, 'Tarjetas graficas', 'MSI'),

    -- MEMORIAS RAM
    ('Kingston Fury Beast 16GB DDR4', 'Memoria RAM DDR4 3200 MHz', 65000.00, 20, 'Memorias RAM', 'Kingston'),
    ('Corsair Vengeance 16GB DDR4', 'Memoria RAM DDR4 3200 MHz', 75000.00, 15, 'Memorias RAM', 'Corsair'),
    ('Kingston Fury Beast 32GB DDR5', 'Memoria RAM DDR5 5600 MHz', 155000.00, 12, 'Memorias RAM', 'Kingston'),
    ('Corsair Vengeance 32GB DDR5', 'Memoria RAM DDR5 6000 MHz', 185000.00, 10, 'Memorias RAM', 'Corsair'),

    -- ALMACENAMIENTO
    ('Kingston NV2 500GB', 'SSD NVMe M.2 de 500 GB', 65000.00, 20, 'Almacenamiento', 'Kingston'),
    ('Kingston NV2 1TB', 'SSD NVMe M.2 de 1 TB', 105000.00, 15, 'Almacenamiento', 'Kingston'),
    ('Samsung 990 PRO 1TB', 'SSD NVMe de alto rendimiento', 185000.00, 8, 'Almacenamiento', 'Samsung'),
    ('WD Blue SN580 1TB', 'SSD NVMe M.2 de 1 TB', 115000.00, 12, 'Almacenamiento', 'Western Digital'),

    -- FUENTES
    ('Corsair CX550', 'Fuente de alimentacion de 550 W', 95000.00, 10, 'Fuentes de alimentacion', 'Corsair'),
    ('Cooler Master MWE 650', 'Fuente de alimentacion de 650 W', 115000.00, 9, 'Fuentes de alimentacion', 'Cooler Master'),
    ('Seasonic Focus GX 750', 'Fuente modular de 750 W', 195000.00, 6, 'Fuentes de alimentacion', 'Seasonic'),
    ('Corsair RM850e', 'Fuente modular de 850 W', 225000.00, 5, 'Fuentes de alimentacion', 'Corsair'),

    -- REFRIGERACION
    ('DeepCool AK400', 'Cooler de aire para procesador', 55000.00, 12, 'Refrigeracion', 'DeepCool'),
    ('Cooler Master Hyper 212', 'Cooler de aire para CPU', 65000.00, 10, 'Refrigeracion', 'Cooler Master'),
    ('DeepCool AK620', 'Cooler de doble torre para CPU', 115000.00, 7, 'Refrigeracion', 'DeepCool'),
    ('NZXT Kraken 240', 'Refrigeracion liquida de 240 mm', 245000.00, 4, 'Refrigeracion', 'NZXT'),

    -- GABINETES
    ('Corsair 4000D Airflow', 'Gabinete ATX con alto flujo de aire', 155000.00, 8, 'Gabinetes', 'Corsair'),
    ('NZXT H5 Flow', 'Gabinete ATX con panel lateral de vidrio', 165000.00, 7, 'Gabinetes', 'NZXT'),
    ('Cooler Master TD500 Mesh', 'Gabinete ATX con ventiladores RGB', 195000.00, 6, 'Gabinetes', 'Cooler Master'),
    ('DeepCool CH370', 'Gabinete compacto Micro-ATX', 95000.00, 10, 'Gabinetes', 'DeepCool'),

    -- MOUSE
    ('Logitech G203', 'Mouse gaming con sensor optico', 35000.00, 20, 'Mouse', 'Logitech'),
    ('Logitech G305', 'Mouse gaming inalambrico', 65000.00, 15, 'Mouse', 'Logitech'),
    ('Razer DeathAdder V2', 'Mouse gaming ergonomico', 75000.00, 12, 'Mouse', 'Razer'),
    ('Redragon Cobra M711', 'Mouse gaming con iluminacion RGB', 29000.00, 18, 'Mouse', 'Redragon'),

    -- TECLADOS
    ('Redragon Kumara K552', 'Teclado mecanico compacto', 65000.00, 15, 'Teclados', 'Redragon'),
    ('Logitech G213', 'Teclado gaming con iluminacion RGB', 85000.00, 12, 'Teclados', 'Logitech'),
    ('Razer BlackWidow V3', 'Teclado mecanico gaming', 165000.00, 8, 'Teclados', 'Razer'),
    ('Corsair K70 RGB', 'Teclado mecanico de alto rendimiento', 195000.00, 6, 'Teclados', 'Corsair'),

    -- MONITORES
    ('LG UltraGear 24GN60R', 'Monitor gaming Full HD de 144 Hz', 285000.00, 8, 'Monitores', 'LG'),
    ('Samsung Odyssey G4 25', 'Monitor gaming Full HD de 240 Hz', 395000.00, 5, 'Monitores', 'Samsung'),
    ('ASUS TUF VG27AQ', 'Monitor gaming QHD de 165 Hz', 485000.00, 6, 'Monitores', 'ASUS'),
    ('Acer Nitro VG240Y', 'Monitor gaming Full HD de 23.8 pulgadas', 245000.00, 9, 'Monitores', 'Acer')
) AS datos (Nombre, Descripcion, Precio, Stock, Categoria, Marca)
INNER JOIN Categorias c ON c.Nombre = datos.Categoria
INNER JOIN Marcas m ON m.Nombre = datos.Marca
WHERE NOT EXISTS (
    SELECT 1
    FROM Productos p
    WHERE p.Nombre = datos.Nombre
);

-- VERIFICAR CANTIDAD TOTAL
SELECT COUNT(*) AS TotalProductos
FROM Productos;

-- VERIFICAR PRODUCTOS POR CATEGORIA
SELECT c.Nombre AS Categoria, COUNT(p.Id) AS Cantidad
FROM Categorias c
LEFT JOIN Productos p ON p.CategoriaId = c.Id
GROUP BY c.Nombre
ORDER BY c.Nombre;
