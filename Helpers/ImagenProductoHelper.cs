namespace ByteStore.Helpers
{
    public static class ImagenProductoHelper
    {
        private static readonly Dictionary<string, int> Imagenes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // PROCESADORES
                { "Procesador AMD Ryzen 7 5700X", 1 },
                { "AMD Ryzen 5 5600", 2 },
                { "Intel Core i5 12400F", 3 },
                { "Intel Core i7 12700K", 4 },

                // MOTHERBOARDS
                { "ASUS Prime B550M-A", 5 },
                { "MSI B550 Gaming Plus", 6 },
                { "Gigabyte B760M DS3H", 7 },
                { "ASUS TUF Gaming B650", 8 },

                // TARJETAS GRAFICAS
                { "MSI GeForce RTX 4060", 9 },
                { "ASUS GeForce RTX 4070", 10 },
                { "Gigabyte Radeon RX 7600", 11 },
                { "MSI Radeon RX 6600", 12 },

                // MEMORIAS RAM
                { "Kingston Fury Beast 16GB DDR4", 13 },
                { "Corsair Vengeance 16GB DDR4", 14 },
                { "Kingston Fury Beast 32GB DDR5", 15 },
                { "Corsair Vengeance 32GB DDR5", 16 },

                // ALMACENAMIENTO
                { "Kingston NV2 500GB", 17 },
                { "Kingston NV2 1TB", 18 },
                { "Samsung 990 PRO 1TB", 19 },
                { "WD Blue SN580 1TB", 20 },

                // FUENTES
                { "Corsair CX550", 21 },
                { "Cooler Master MWE 650", 22 },
                { "Seasonic Focus GX 750", 23 },
                { "Corsair RM850e", 24 },

                // REFRIGERACION
                { "DeepCool AK400", 25 },
                { "Cooler Master Hyper 212", 26 },
                { "DeepCool AK620", 27 },
                { "NZXT Kraken 240", 28 },

                // GABINETES
                { "Corsair 4000D Airflow", 29 },
                { "NZXT H5 Flow", 30 },
                { "Cooler Master TD500 Mesh", 31 },
                { "DeepCool CH370", 32 },

                // MOUSE
                { "Logitech G203", 33 },
                { "Logitech G305", 34 },
                { "Razer DeathAdder V2", 35 },
                { "Redragon Cobra M711", 36 },

                // TECLADOS
                { "Redragon Kumara K552", 37 },
                { "Logitech G213", 38 },
                { "Razer BlackWidow V3", 39 },
                { "Corsair K70 RGB", 40 },

                // MONITORES
                { "LG UltraGear 24GN60R", 41 },
                { "Samsung Odyssey G4 25", 42 },
                { "ASUS TUF VG27AQ", 43 },
                { "Acer Nitro VG240Y", 44 }
            };

        public static string? ObtenerRuta(string nombre)
        {
            return Imagenes.TryGetValue(nombre, out int numero)
                ? $"~/images/productos/producto-{numero}.jfif"
                : null;
        }
    }
}
