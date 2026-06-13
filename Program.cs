using System;
using System.Collections.Generic;
using System.IO; // Capa nativa para el manejo y persistencia de archivos (.txt)

class Program
{
    // Rutas de almacenamiento persistente en el disco duro (Base de datos plana)
    static string rutaInventario = "inventario.csv";
    static string rutaVentas = "ventas.txt";

    static void Main(string[] args)
    {
        // Asegurar la existencia física de los archivos desde el arranque
        if (!File.Exists(rutaInventario)) File.Create(rutaInventario).Close();
        if (!File.Exists(rutaVentas)) File.Create(rutaVentas).Close();

        int opcion = 0;
        do
        {
            Console.Clear();
            Console.WriteLine("==================================================================");
            Console.WriteLine("           SISTEMA DE CONTROL DE VENTAS E INVENTARIO");
            Console.WriteLine("                       \"BODEGA AUTOMÁTICA\"");
            Console.WriteLine("==================================================================");
            Console.WriteLine(" [1] Registrar Nueva Venta");
            Console.WriteLine(" [2] Consultar Stock de un Producto");
            Console.WriteLine(" [3] Agregar Nuevo Producto al Inventario");
            Console.WriteLine(" [4] Generar Reporte de Ventas e Ingresos");
            Console.WriteLine(" [5] Salir del Sistema");
            Console.WriteLine("==================================================================");
            Console.Write("Seleccione una opción (1-5): ");

            if (int.TryParse(Console.ReadLine(), out opcion))
            {
                Console.Clear();
                switch (opcion)
                {
                    case 1:
                        RegistrarVenta(); // Trazabilidad: Opción 1 del Menú
                        break;
                    case 2:
                        VerStock();       // Trazabilidad: Opción 2 del Menú
                        break;
                    case 3:
                        AgregarProducto(); // Trazabilidad: Opción 3 del Menú
                        break;
                    case 4:
                        GenerarReporte(); // Trazabilidad: Opción 4 del Menú
                        break;
                    case 5:
                        Console.WriteLine("Cerrando flujos y asegurando archivos... ¡Hasta luego!");
                        break;
                    default:
                        Console.WriteLine("Opción inválida. Intente del 1 al 5.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Por favor, ingrese un número válido.");
            }

            if (opcion != 5)
            {
                Console.WriteLine("\nPresione cualquier tecla para regresar al menú principal...");
                Console.ReadKey();
            }

        } while (opcion != 5);
    }


    // LEER CSV → LISTA

    static List<string[]> LeerInventario()
    {
        var lista = new List<string[]>();

        if (!File.Exists(rutaInventario))
            return lista;

        string[] lineas = File.ReadAllLines(rutaInventario);

        foreach (string linea in lineas)
        {
            string[] datos = linea.Split(',');

            if (datos.Length == 5)
                lista.Add(datos);
        }

        return lista;
    }

    //GUARDAR LISTA

    static void GuardarInventario(List<string[]> lista)
    {
        var lineas = new List<string>();

        foreach (var p in lista)
        {
            lineas.Add($"{p[0]},{p[1]},{p[2]},{p[3]},{p[4]}");
        }

        File.WriteAllLines(rutaInventario, lineas);
    }


    // Firmas de los métodos requeridos para la implementación del código del Rol 2
    static void RegistrarVenta() { /* Lógica de lectura/escritura simultánea */ }

    static void VerStock()
    {
        var lista = LeerInventario();

        Console.WriteLine("=== CONSULTAR STOCK ===");
        Console.WriteLine("[1] Buscar producto por código");
        Console.WriteLine("[2] Mostrar TODO el inventario");
        Console.Write("Seleccione: ");

        string opcion = Console.ReadLine();

        if (opcion == "1")
        {
            Console.Write("Ingrese código: ");
            string codigo = Console.ReadLine();

            bool encontrado = false;

            foreach (var p in lista)
            {
                if (p[0].Equals(codigo, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"\nCódigo: {p[0]}");
                    Console.WriteLine($"Nombre: {p[1]}");
                    Console.WriteLine($"Categoría: {p[2]}");
                    Console.WriteLine($"Precio: {p[3]}");
                    Console.WriteLine($"Stock: {p[4]}");
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
                Console.WriteLine("Producto no encontrado.");
        }
        else if (opcion == "2")
        {
            Console.WriteLine("\n=== INVENTARIO COMPLETO ===");

            if (lista.Count == 0)
            {
                Console.WriteLine("No hay productos.");
                return;
            }

            foreach (var p in lista)
            {
                Console.WriteLine($"{p[0]} | {p[1]} | {p[2]} | S/.{p[3]} | Stock: {p[4]}");
            }
        }
        else
        {
            Console.WriteLine("Opción inválida.");
        }
    }

    static void AgregarProducto()
    {
        var lista = LeerInventario();

        Console.WriteLine("=== AGREGAR PRODUCTO ===");

        Console.Write("Código: ");
        string codigo = Console.ReadLine();

        foreach (var p in lista)
        {
            if (p[0].Equals(codigo, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("ERROR: Código ya existe.");
                return;
            }
        }

        Console.Write("Nombre: ");
        string nombre = Console.ReadLine();

        Console.Write("Categoría: ");
        string categoria = Console.ReadLine();

        Console.Write("Precio: ");
        if (!double.TryParse(Console.ReadLine(), out double precio))
        {
            Console.WriteLine("Precio inválido.");
            return;
        }

        Console.Write("Stock: ");
        if (!int.TryParse(Console.ReadLine(), out int stock))
        {
            Console.WriteLine("Stock inválido.");
            return;
        }

        lista.Add(new string[]
        {
            codigo, nombre, categoria,
            precio.ToString(), stock.ToString()
        });

        GuardarInventario(lista);

        Console.WriteLine("Producto agregado correctamente.");
    }

    static void GenerarReporte() { /* Lógica de procesamiento y conteo */ }
}
//ACTUALIZACIÓN 13/06/26