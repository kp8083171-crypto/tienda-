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

        Console.WriteLine($"Producto: {productoSeleccionado[1]} | Precio: S/.{productoSeleccionado[3]} | Stock Actual: {productoSeleccionado[4]}");
        
        Console.Write("Cantidad a vender: ");
        if (!int.TryParse(Console.ReadLine(), out int cantidad) || cantidad <= 0)
        {
            Console.WriteLine("ERROR: Cantidad inválida.");
            return;
        }

        int stockActual = int.Parse(productoSeleccionado[4]);

        if (cantidad > stockActual)
        {
            Console.WriteLine("ERROR: Stock insuficiente para procesar esta venta.");
            return;
        }

        double precio = double.Parse(productoSeleccionado[3]);
        double totalPagar = precio * cantidad;

        // --- NUEVA LÓGICA: SELECCIÓN DE TIPO DE PAGO ---
        string tipoPago = "";
        while (tipoPago == "")
        {
            Console.WriteLine("\nSeleccione Tipo de Pago:");
            Console.WriteLine("[1] Efectivo");
            Console.WriteLine("[2] Yape / Plin");
            Console.Write("Opción: ");
            string opPago = Console.ReadLine();

            if (opPago == "1") tipoPago = "Efectivo";
            else if (opPago == "2") tipoPago = "Yape/Plin";
            else Console.WriteLine("Opción no válida. Intente de nuevo.");
        }

        double importeRecibido = totalPagar;
        double vuelto = 0;

        // --- NUEVA LÓGICA: IMPORTE RECIBIDO Y VUELTO ---
        if (tipoPago == "Efectivo")
        {
            bool importeValido = false;
            while (!importeValido)
            {
                Console.Write($"Total a pagar: S/.{totalPagar:F2} | Ingrese Pago del Cliente: S/. ");
                if (double.TryParse(Console.ReadLine(), out importeRecibido) && importeRecibido >= totalPagar)
                {
                    vuelto = importeRecibido - totalPagar;
                    importeValido = true;
                }
                else
                {
                    Console.WriteLine("ERROR: El monto ingresado es insuficiente o inválido.");
                }
            }
        }
        else
        {
            // Para Yape/Plin el importe recibido es exactamente el total a pagar y el vuelto es 0
            importeRecibido = totalPagar;
            vuelto = 0;
            Console.WriteLine($"\nPago con {tipoPago} confirmado por S/.{totalPagar:F2}");
        }

        // --- PERSISTENCIA Y ACTUALIZACIÓN ---
        int nuevoStock = stockActual - cantidad;
        productoSeleccionado[4] = nuevoStock.ToString();

        GuardarInventario(lista);

        // Se agregan las nuevas columnas al registro: TipoPago, ImporteRecibido, Vuelto
        string registroVenta = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{productoSeleccionado[0]},{productoSeleccionado[1]},{cantidad},{totalPagar},{tipoPago},{importeRecibido},{vuelto}";
        
        File.AppendAllLines(rutaVentas, new List<string> { registroVenta });

        Console.WriteLine("\n=================================");
        Console.WriteLine("    VENTA PROCESADA CON ÉXITO");
        Console.WriteLine($" Pago con:       {tipoPago}");
        Console.WriteLine($" Total a cobrar: S/.{totalPagar:F2}");
        Console.WriteLine($" Recibido:       S/.{importeRecibido:F2}");
        Console.WriteLine($" Vuelto:         S/.{vuelto:F2}");
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

int ventasTotales = 0;
double gananciaTotal = 0;

// Primero calculamos resumen general
foreach (string linea in lineas)
{
    string[] datos = linea.Split(',');

    if (datos.Length != 8)
        continue;

    int cantidad = int.Parse(datos[3]);
    double total = double.Parse(datos[4]);

    ventasTotales++;
    gananciaTotal += total;
}

Console.WriteLine("\n===== RESUMEN GENERAL =====");
Console.WriteLine($"Total de ventas realizadas: {ventasTotales}");
Console.WriteLine($"Ganancia total: S/. {gananciaTotal:F2}");

Console.WriteLine("\nSeleccione tipo de pago para filtrar:");
Console.WriteLine("[1] Efectivo");
Console.WriteLine("[2] Yape / Plin");
Console.Write("Opción: ");

string opcion = Console.ReadLine();
string tipoBuscado = "";

if (opcion == "1")
    tipoBuscado = "Efectivo";
else if (opcion == "2")
    tipoBuscado = "Yape/Plin";
else
{
    Console.WriteLine("Opción inválida.");
    return;
}

int ventasFiltradas = 0;
double ingresosFiltrados = 0;

Console.WriteLine($"\n=== VENTAS POR {tipoBuscado.ToUpper()} ===\n");

foreach (string linea in lineas)
{
    string[] datos = linea.Split(',');

    if (datos.Length != 8)
        continue;

    string fecha = datos[0];
    string codigo = datos[1];
    string nombre = datos[2];
    int cantidad = int.Parse(datos[3]);
    double total = double.Parse(datos[4]);
    string tipoPago = datos[5];

    if (tipoPago == tipoBuscado)
    {
        ventasFiltradas++;
        ingresosFiltrados += total;

        Console.WriteLine($"Fecha: {fecha}");
        Console.WriteLine($"Código: {codigo}");
        Console.WriteLine($"Producto: {nombre}");
        Console.WriteLine($"Cantidad: {cantidad}");
        Console.WriteLine($"Total: S/. {total:F2}");
        Console.WriteLine("--------------------------------");
    }
}

Console.WriteLine("\n===== RESUMEN FILTRADO =====");
Console.WriteLine($"Ventas por {tipoBuscado}: {ventasFiltradas}");
Console.WriteLine($"Ingresos por {tipoBuscado}: S/. {ingresosFiltrados:F2}");
}

}