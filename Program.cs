using System;
using System.IO; // Capa nativa para el manejo y persistencia de archivos (.txt)

class Program
{
    // Rutas de almacenamiento persistente en el disco duro (Base de datos plana)
    static string rutaInventario = "inventario.txt";
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

    // Firmas de los métodos requeridos para la implementación del código del Rol 2
    static void RegistrarVenta() { /* Lógica de lectura/escritura simultánea */ }
    static void VerStock() { /* Lógica con StreamReader */ }
    static void AgregarProducto() { /* Lógica con StreamWriter en modo append */ }
    static void GenerarReporte() { /* Lógica de procesamiento y conteo */ }
}