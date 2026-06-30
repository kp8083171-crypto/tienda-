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
            Console.WriteLine(" [6] Generar Comprobante de Venta");
            Console.WriteLine("==================================================================");
            Console.Write("Seleccione una opción (1-6): ");

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
                    case 6:
                        MenuComprobante();
                        break;
                    default:
                        Console.WriteLine("Opción inválida. Intente del 1 al 6.");
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

        if (lista.Count == 0)
        {
            Console.WriteLine("ERROR: No hay productos registrados en el inventario.");
            return;
        }

        // --- VALIDACIÓN CON REINTENTO: CÓDIGO DE PRODUCTO ---
        // Se permite reintentar hasta encontrar un producto válido o cancelar con 0
        string[] productoSeleccionado = null;

        while (productoSeleccionado == null)
        {
            Console.Write("Ingrese el código del producto a vender (0 para cancelar): ");
            string codigo = Console.ReadLine();

            if (codigo == "0")
            {
                Console.WriteLine("Venta cancelada.");
                return;
            }

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
                Console.WriteLine(">> NOTIFICACIÓN: El código ingresado no existe. Por favor, ingrese un código válido.\n");
            }
        }

        Console.WriteLine($"Producto: {productoSeleccionado[1]} | Precio: S/.{productoSeleccionado[3]} | Stock Actual: {productoSeleccionado[4]}");

        int stockActual = int.Parse(productoSeleccionado[4]);

        // --- VALIDACIÓN CON REINTENTO: CANTIDAD A VENDER ---
        int cantidad = 0;
        bool cantidadValida = false;

        while (!cantidadValida)
        {
            Console.Write("Cantidad a vender (0 para cancelar): ");
            string entradaCantidad = Console.ReadLine();

            if (entradaCantidad == "0")
            {
                Console.WriteLine("Venta cancelada.");
                return;
            }

            if (!int.TryParse(entradaCantidad, out cantidad))
            {
                Console.WriteLine(">> NOTIFICACIÓN: Debe ingresar un número entero válido.\n");
                continue;
            }

            if (cantidad < 0)
            {
                Console.WriteLine(">> NOTIFICACIÓN: La cantidad no puede ser negativa. Ingrese un valor válido.\n");
                continue;
            }

            if (cantidad == 0)
            {
                Console.WriteLine(">> NOTIFICACIÓN: La cantidad debe ser mayor a cero. Ingrese un valor válido.\n");
                continue;
            }

            if (cantidad > stockActual)
            {
                Console.WriteLine($">> NOTIFICACIÓN: Stock insuficiente (disponible: {stockActual}). Ingrese una cantidad válida.\n");
                continue;
            }

            cantidadValida = true;
        }

        double precio = double.Parse(productoSeleccionado[3]);
        double totalPagar = precio * cantidad;

        // --- SELECCIÓN DE TIPO DE PAGO (con validación de reintento) ---
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
            else Console.WriteLine(">> NOTIFICACIÓN: Opción no válida. Ingrese 1 o 2.\n");
        }

        double importeRecibido = totalPagar;
        double vuelto = 0;

        // --- IMPORTE RECIBIDO Y VUELTO (con validación de reintento) ---
        if (tipoPago == "Efectivo")
        {
            bool importeValido = false;
            while (!importeValido)
            {
                Console.Write($"Total a pagar: S/.{totalPagar:F2} | Ingrese Pago del Cliente: S/. ");
                string entradaImporte = Console.ReadLine();

                if (!double.TryParse(entradaImporte, out importeRecibido))
                {
                    Console.WriteLine(">> NOTIFICACIÓN: Debe ingresar un monto numérico válido.\n");
                    continue;
                }

                if (importeRecibido < 0)
                {
                    Console.WriteLine(">> NOTIFICACIÓN: El monto no puede ser negativo. Ingrese un valor válido.\n");
                    continue;
                }

                if (importeRecibido < totalPagar)
                {
                    Console.WriteLine(">> NOTIFICACIÓN: El monto ingresado es insuficiente. Ingrese un valor válido.\n");
                    continue;
                }

                vuelto = importeRecibido - totalPagar;
                importeValido = true;
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
    static int PedirEntero(string mensaje)
    {
        int valor;
        while (true)
        {
            Console.Write(mensaje);
            string input = Console.ReadLine();

            if (int.TryParse(input, out valor))
                return valor;

            Console.WriteLine("Valor inválido. Intente nuevamente.");
        }
    }
    static void GenerarReporte()

    {
        Console.Clear();
        Console.WriteLine("==================================================================");
        Console.WriteLine("              REPORTE DE VENTAS E INGRESOS");
        Console.WriteLine("==================================================================");
        Console.WriteLine(" [1] Reporte general de ventas totales");
        Console.WriteLine(" [2] Reporte de ventas con fechas específicas");
        Console.WriteLine(" [3] Volver al Menú Principal");
        Console.WriteLine("==================================================================");
        Console.Write("Opción: ");

        string opcionReporte = Console.ReadLine();

        if (opcionReporte == "3")
            return;

        if (opcionReporte != "1" && opcionReporte != "2")
        {
            Console.WriteLine("Opción inválida.");
            return;
        }

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

        DateTime fechaInicio = DateTime.MinValue;
        DateTime fechaFin = DateTime.MaxValue;

        // --- OPCIÓN 2: pedir rango de fechas ---
        if (opcionReporte == "2")
        {
            Console.WriteLine("\n=== FECHA INICIO ===");

            while (true)
            {
                Console.Write("Día: ");
                bool diaValido = int.TryParse(Console.ReadLine(), out int dia);

                Console.Write("Mes: ");
                bool mesValido = int.TryParse(Console.ReadLine(), out int mes);

                Console.Write("Año: ");
                bool anioValido = int.TryParse(Console.ReadLine(), out int anio);

                if (diaValido && mesValido && anioValido &&
                    DateTime.TryParse($"{anio}-{mes}-{dia}", out fechaInicio))
                {
                    break;
                }

                Console.WriteLine("Fecha inválida. Intente nuevamente.\n");
            }

            Console.WriteLine("\n=== FECHA FIN ===");

            while (true)
            {
                Console.Write("Día: ");
                bool diaValido = int.TryParse(Console.ReadLine(), out int dia);

                Console.Write("Mes: ");
                bool mesValido = int.TryParse(Console.ReadLine(), out int mes);

                Console.Write("Año: ");
                bool anioValido = int.TryParse(Console.ReadLine(), out int anio);

                if (diaValido && mesValido && anioValido &&
                    DateTime.TryParse($"{anio}-{mes}-{dia}", out fechaFin))
                {
                    break;
                }

                Console.WriteLine("Fecha inválida. Intente nuevamente.\n");
            }

            fechaInicio = fechaInicio.Date;
            fechaFin = fechaFin.Date.AddDays(1).AddSeconds(-1);
        }

        // --- FILTRO TIPO DE PAGO ---
        Console.WriteLine();
        Console.WriteLine("Filtrar por tipo de pago:");
        Console.WriteLine(" [1] Efectivo");
        Console.WriteLine(" [2] Yape/Plin");
        Console.WriteLine(" [3] Todos");
        Console.Write("Opción: ");

        string opcionPago = Console.ReadLine();
        string tipoBuscado = opcionPago == "1" ? "Efectivo"
                           : opcionPago == "2" ? "Yape/Plin"
                           : "TODOS";

        // --- RESUMEN GENERAL ---
        int ventasTotales = 0;
        double gananciaTotal = 0;

        foreach (string linea in lineas)
        {
            string[] datos = linea.Split(',');

            if (datos.Length != 8)
                continue;

            ventasTotales++;
            gananciaTotal += double.Parse(datos[4]);
        }

        Console.WriteLine();
        Console.WriteLine("===== RESUMEN GENERAL =====");
        Console.WriteLine($"Total de ventas registradas: {ventasTotales}");
        Console.WriteLine($"Ganancia total acumulada:    S/.{gananciaTotal:F2}");
        Console.WriteLine();

        // --- RESULTADOS FILTRADOS ---
        Console.WriteLine("===== RESULTADOS FILTRADOS =====");
        Console.WriteLine();

        int ventasFiltradas = 0;
        double ingresosFiltrados = 0;

        foreach (string linea in lineas)
        {
            string[] datos = linea.Split(',');

            if (datos.Length != 8)
                continue;

            DateTime fechaVenta = DateTime.Parse(datos[0]);
            string nombre = datos[2];
            int cantidad = int.Parse(datos[3]);
            double total = double.Parse(datos[4]);
            string tipoPago = datos[5];
            double importeRecibido = double.Parse(datos[6]);
            double vuelto = double.Parse(datos[7]);

            bool cumplePago = (tipoBuscado == "TODOS" || tipoPago == tipoBuscado);
            bool cumpleFecha = (fechaVenta >= fechaInicio && fechaVenta <= fechaFin);

            if (!cumplePago || !cumpleFecha)
                continue;

            ventasFiltradas++;
            ingresosFiltrados += total;

            Console.WriteLine($"Fecha:          {fechaVenta:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"Producto:       {nombre}");
            Console.WriteLine($"Cantidad:       {cantidad}");
            Console.WriteLine($"Total:          S/.{total:F2}");
            Console.WriteLine($"Pago con:       {tipoPago}");
            Console.WriteLine($"Monto recibido: S/.{importeRecibido:F2}");
            Console.WriteLine($"Vuelto:         S/.{vuelto:F2}");
            Console.WriteLine("--------------------------------");
        }

        if (ventasFiltradas == 0)
        {
            Console.WriteLine("No se encontraron ventas con los filtros seleccionados.");
        }

        Console.WriteLine();
        Console.WriteLine("===== RESUMEN FILTRADO =====");
        Console.WriteLine($"Ventas encontradas: {ventasFiltradas}");
        Console.WriteLine($"Ingresos:           S/.{ingresosFiltrados:F2}");
    }
static void MenuComprobante()
    {
        string subOp = "";
        while (subOp != "3")
        {
            Console.Clear();
            Console.WriteLine("=== GESTIÓN DE COMPROBANTES - BODEGA MIGUELITO ===");
            Console.WriteLine("[1] Generar boleta de última venta");
            Console.WriteLine("[2] Generar boleta por fecha específica");
            Console.WriteLine("[3] Volver al menú principal");
            Console.Write("Seleccione: ");
            subOp = Console.ReadLine();

            if (subOp == "1") GenerarUltimaBoleta();
            else if (subOp == "2") GenerarBoletaPorFecha();
        }
    }

    static void GenerarUltimaBoleta()
    {
        if (!File.Exists(rutaVentas)) return;
        string[] lineas = File.ReadAllLines(rutaVentas);
        if (lineas.Length == 0) { Console.WriteLine("No hay ventas."); Console.ReadKey(); return; }
        
        string[] datos = lineas[lineas.Length - 1].Split(',');
        ImprimirBoletaCompleta(datos);
        
        Console.WriteLine("\nPresione cualquier tecla para volver...");
        Console.ReadKey();
    }

    static void GenerarBoletaPorFecha()
    {
        Console.Write("Ingrese fecha (yyyy-mm-dd): ");
        string fecha = Console.ReadLine();
        string[] todas = File.ReadAllLines(rutaVentas);
        
        Console.Clear();
        List<string[]> encontrados = new List<string[]>();
        for (int i = 0; i < todas.Length; i++)
        {
            if (todas[i].StartsWith(fecha))
            {
                encontrados.Add(todas[i].Split(','));
                Console.WriteLine($"[{encontrados.Count}] {todas[i]}");
            }
        }

        if (encontrados.Count > 0)
        {
            Console.Write("\nIngrese el número de venta: ");
            if (int.TryParse(Console.ReadLine(), out int sel) && sel > 0 && sel <= encontrados.Count)
            {
                ImprimirBoletaCompleta(encontrados[sel - 1]);
                Console.WriteLine("\nPresione cualquier tecla para volver...");
                Console.ReadKey();
            }
        }
        else { Console.WriteLine("No se encontraron registros."); Console.ReadKey(); }
    }

    static void ImprimirBoletaCompleta(string[] d)
{
    Console.Clear();

    Console.WriteLine("           BODEGA \"MIGUELITO\"");
    Console.WriteLine("            RUC : 20557410008");
    Console.WriteLine("========================================");

    // Fecha y hora
    if (d.Length >= 1)
        Console.WriteLine($"Fecha/Hora    : {d[0]}");

    // Producto
    if (d.Length >= 3)
        Console.WriteLine($"Producto      : {d[2]}");

    // Cantidad
    if (d.Length >= 4)
        Console.WriteLine($"Cantidad      : {d[3]}");

    // Precio unitario y total
    if (d.Length >= 5)
    {
        double total = Convert.ToDouble(d[4]);
        int cantidad = Convert.ToInt32(d[3]);
        double precioUnitario = total / cantidad;

        Console.WriteLine($"Precio unit.  : S/. {precioUnitario:F2}");
        Console.WriteLine($"TOTAL         : S/. {total:F2}");
    }

    // Cliente
    Console.WriteLine("Cliente       : Cliente general");

    Console.WriteLine("========================================");

    // Tipo de pago
    if (d.Length >= 6)
    {
        Console.WriteLine($"Tipo de pago  : {d[5]}");

        if (d[5].Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Pago recibido : S/. {Convert.ToDouble(d[6]):F2}");
            Console.WriteLine($"Vuelto        : S/. {Convert.ToDouble(d[7]):F2}");
        }
        else
        {
            // Para Yape o Plin
            Console.WriteLine($"Monto pagado  : S/. {Convert.ToDouble(d[6]):F2}");
        }
    }

    Console.WriteLine("========================================");
    Console.WriteLine("      ¡GRACIAS POR SU COMPRA!");
}
}
