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
                        GestionProductos(); // Trazabilidad: Opción 3 del Menú
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


    // Lógica de lectura/escritura simultánea incorporada con éxito
    static void RegistrarVenta() 
    {
        Console.WriteLine("=== REGISTRAR NUEVA VENTA ===");
        var lista = LeerInventario();

        Console.Write("Ingrese el código del producto a vender: ");
        string codigo = Console.ReadLine();

        string[] productoSeleccionado = null;

        // Buscar el producto en el inventario cargado
        foreach (var p in lista)
        {
            if (p[0].Equals(codigo, StringComparison.OrdinalIgnoreCase))
            {
                productoSeleccionado = p;
                break;
            }
        }

        if (productoSeleccionado == null)
        {
            Console.WriteLine("ERROR: El producto no existe en el inventario.");
            return;
        }

        // Mostrar detalles actuales del producto seleccionado
        Console.WriteLine($"Producto: {productoSeleccionado[1]} | Precio: S/.{productoSeleccionado[3]} | Stock Actual: {productoSeleccionado[4]}");
        
        Console.Write("Cantidad a vender: ");
        if (!int.TryParse(Console.ReadLine(), out int cantidad) || cantidad <= 0)
        {
            Console.WriteLine("ERROR: Cantidad inválida.");
            return;
        }

        int stockActual = int.Parse(productoSeleccionado[4]);

        // Validar si hay suficientes existencias en bodega
        if (cantidad > stockActual)
        {
            Console.WriteLine("ERROR: Stock insuficiente para procesar esta venta.");
            return;
        }

        // Calcular los valores comerciales de la transacción
        double precio = double.Parse(productoSeleccionado[3]);
        double totalPagar = precio * cantidad;

        // Actualizar el stock reduciéndolo en memoria
        int nuevoStock = stockActual - cantidad;
        productoSeleccionado[4] = nuevoStock.ToString();

        // Guardar el inventario con los stocks actualizados en el archivo plano
        GuardarInventario(lista);

        // Estructurar el formato de salida para el archivo histórico de ventas
        // Formato: Fecha_Hora,Codigo,Nombre,Cantidad,Total
        string registroVenta = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{productoSeleccionado[0]},{productoSeleccionado[1]},{cantidad},{totalPagar}";
        
        // Escribir la venta al final del archivo sin alterar registros previos
        File.AppendAllLines(rutaVentas, new List<string> { registroVenta });

        Console.WriteLine("\n=================================");
        Console.WriteLine("    VENTA PROCESADA CON ÉXITO");
        Console.WriteLine($" Total a cobrar: S/.{totalPagar:F2}");
        Console.WriteLine("=================================");
    }

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

static void GestionProductos()
{
    Console.WriteLine("=== GESTIÓN DE PRODUCTOS ===");
    Console.WriteLine("[1] Agregar producto");
    Console.WriteLine("[2] Editar producto");
    Console.WriteLine("[3] Eliminar producto");
    Console.Write("Seleccione: ");

    string opcion = Console.ReadLine();

    if (opcion == "1")
    {
        AgregarProducto();
    }
    else if (opcion == "2")
    {
        EditarProducto();
    }
    else if (opcion == "3")
    {
        EliminarProducto();
    }
    else
    {
        Console.WriteLine("Opción inválida.");
    }
}

    static void AgregarProducto()
    {
        var lista = LeerInventario();

        Console.WriteLine("=== PRODUCTOS DISPONIBLES ===");

        foreach (var p in lista)
        {
            Console.WriteLine($"{p[0]} | {p[1]} | {p[2]} | S/.{p[3]} | Stock: {p[4]}");
        }

        Console.WriteLine();

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

static void EditarProducto()
{
    var lista = LeerInventario();

    Console.WriteLine("=== PRODUCTOS DISPONIBLES ===");

    foreach (var p in lista)
    {
        Console.WriteLine($"{p[0]} | {p[1]} | {p[2]} | S/.{p[3]} | Stock: {p[4]}");
    }

    Console.WriteLine("=== EDITAR PRODUCTO ===");

    Console.Write("Ingrese el código del producto: ");
    string codigo = Console.ReadLine();

    string[] producto = null;

    foreach (var p in lista)
    {
        if (p[0].Equals(codigo, StringComparison.OrdinalIgnoreCase))
        {
            producto = p;
            break;
        }
    }

    if (producto == null)
    {
        Console.WriteLine("Producto no encontrado.");
        return;
    }

    Console.WriteLine("\nDatos actuales:");
    Console.WriteLine($"Código: {producto[0]}");
    Console.WriteLine($"Nombre: {producto[1]}");
    Console.WriteLine($"Categoría: {producto[2]}");
    Console.WriteLine($"Precio: {producto[3]}");
    Console.WriteLine($"Stock: {producto[4]}");

    Console.Write("\nNuevo código: ");
    string nuevoCodigo = Console.ReadLine();

    Console.Write("\nNuevo nombre: ");
    string nuevoNombre = Console.ReadLine();

    Console.Write("Nueva categoría: ");
    string nuevaCategoria = Console.ReadLine();

    Console.Write("Nuevo precio: ");
    if (!double.TryParse(Console.ReadLine(), out double nuevoPrecio))
    {
        Console.WriteLine("Precio inválido.");
        return;
    }

    Console.Write("Nuevo stock: ");
    if (!int.TryParse(Console.ReadLine(), out int nuevoStock))
    {
        Console.WriteLine("Stock inválido.");
        return;
    }

    producto[0] = nuevoCodigo;
    producto[1] = nuevoNombre;
    producto[2] = nuevaCategoria;
    producto[3] = nuevoPrecio.ToString();
    producto[4] = nuevoStock.ToString();

    GuardarInventario(lista);

    Console.WriteLine("Producto actualizado correctamente.");
}

static void EliminarProducto()
{
    var lista = LeerInventario();

    Console.WriteLine("=== PRODUCTOS DISPONIBLES ===");

    foreach (var p in lista)
    {
        Console.WriteLine($"{p[0]} | {p[1]} | {p[2]} | S/.{p[3]} | Stock: {p[4]}");
    }

    Console.WriteLine();

    Console.WriteLine("=== ELIMINAR PRODUCTO ===");

    Console.Write("Ingrese el código del producto a eliminar: ");
    string codigo = Console.ReadLine();

    string[] producto = null;

    foreach (var p in lista)
    {
        if (p[0].Equals(codigo, StringComparison.OrdinalIgnoreCase))
        {
            producto = p;
            break;
        }
    }

    if (producto == null)
    {
        Console.WriteLine("Producto no encontrado.");
        return;
    }

    Console.WriteLine($"\nCódigo: {producto[0]}");
    Console.WriteLine($"Nombre: {producto[1]}");

    Console.Write("\n¿Desea eliminar este producto? (S/N): ");
    string respuesta = Console.ReadLine();

    if (respuesta.ToUpper() == "S")
    {
        lista.Remove(producto);

        GuardarInventario(lista);

        Console.WriteLine("Producto eliminado correctamente.");
    }
    else
    {
        Console.WriteLine("Operación cancelada.");
    }
}

    static void GenerarReporte()
{
    Console.WriteLine("=== REPORTE DE VENTAS E INGRESOS ===");

    if (!File.Exists(rutaVentas))
    {
        Console.WriteLine("No existe el archivo de ventas.");
        return;
    }

    string[] lineas = File.ReadAllLines(rutaVentas);

    if (lineas.Length == 0)
    {
        Console.WriteLine("No hay ventas registradas.");
        return;
    }

    int totalVentas = 0;
    int totalProductosVendidos = 0;
    double ingresosTotales = 0;

    Console.WriteLine("\n===== DETALLE DE VENTAS =====\n");

    foreach (string linea in lineas)
    {
        string[] datos = linea.Split(',');

        if (datos.Length != 5)
            continue;

        string fecha = datos[0];
        string codigo = datos[1];
        string nombre = datos[2];
        int cantidad = int.Parse(datos[3]);
        double total = double.Parse(datos[4]);

        totalVentas++;
        totalProductosVendidos += cantidad;
        ingresosTotales += total;

        Console.WriteLine($"Fecha: {fecha}");
        Console.WriteLine($"Código: {codigo}");
        Console.WriteLine($"Producto: {nombre}");
        Console.WriteLine($"Cantidad Vendida: {cantidad}");
        Console.WriteLine($"Total Venta: S/. {total:F2}");
        Console.WriteLine("----------------------------------");
    }

    Console.WriteLine("\n===== RESUMEN GENERAL =====");
    Console.WriteLine($"Número de ventas realizadas: {totalVentas}");
    Console.WriteLine($"Total de productos vendidos: {totalProductosVendidos}");
    Console.WriteLine($"Ingresos totales: S/. {ingresosTotales:F2}");
}
}