using APP.Eds.Services.Billing;

namespace APP.Eds.Examples;

/// <summary>
/// Ejemplo de cómo usar el sistema de facturas mock para desarrollo y testing
/// </summary>
public class MockInvoiceExample
{
    /// <summary>
    /// Ejemplo 1: Activar modo mock y ver facturas
    /// </summary>
    public static async Task Example1_ActivateMockMode()
    {
        Console.WriteLine("=== EJEMPLO 1: Activar Modo Mock ===\n");

        // ✅ Activar modo mock
        ElectronicBillingService.UseMockData = true;
        Console.WriteLine("✅ Modo mock activado");

        // Obtener facturas mock
        var mockInvoices = MockInvoiceService.GetMockInvoices();
        Console.WriteLine($"📄 Se cargaron {mockInvoices.Count} facturas mock\n");

        // Mostrar resumen de las primeras 3 facturas
        foreach (var invoice in mockInvoices.Take(3))
        {
            Console.WriteLine($"Factura: {invoice.FullInvoiceNumber}");
            Console.WriteLine($"  Cliente: {invoice.ClientName}");
            Console.WriteLine($"Documento: {invoice.ClientDocumentNumber}");
            Console.WriteLine($"  Total: {invoice.TotalAmountFormatted}");
            Console.WriteLine($"  Fecha: {invoice.DateFormatted}");
            Console.WriteLine($"  Estado: {invoice.StatusIcon} {invoice.Status}");
            Console.WriteLine($"  CUDE: {invoice.CudeInfo}");
            Console.WriteLine();
        }

        Console.WriteLine($"... y {mockInvoices.Count - 3} facturas más\n");
    }

    /// <summary>
    /// Ejemplo 2: Abrir y compartir PDFs mock
    /// </summary>
    public static async Task Example2_OpenAndSharePdf()
    {
        Console.WriteLine("=== EJEMPLO 2: Abrir y Compartir PDF Mock ===\n");

        var mockInvoices = MockInvoiceService.GetMockInvoices();
        var firstInvoice = mockInvoices.FirstOrDefault();

        if (firstInvoice != null)
        {
            Console.WriteLine($"📄 Trabajando con factura: {firstInvoice.FullInvoiceNumber}\n");

            // Abrir PDF mock
            Console.WriteLine("🚀 Abriendo PDF mock...");
            bool openSuccess = await MockInvoiceService.OpenMockPdfAsync(firstInvoice.FullInvoiceNumber);
            Console.WriteLine(openSuccess ? "✅ PDF abierto exitosamente" : "❌ Error al abrir PDF");
            Console.WriteLine();

            // Compartir PDF mock
            Console.WriteLine("📤 Compartiendo PDF mock...");
            bool shareSuccess = await MockInvoiceService.ShareMockPdfAsync(firstInvoice.FullInvoiceNumber);
            Console.WriteLine(shareSuccess ? "✅ PDF compartido exitosamente" : "❌ Error al compartir PDF");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Ejemplo 3: Agregar nuevas facturas mock
    /// </summary>
    public static void Example3_AddMockInvoices()
    {
        Console.WriteLine("=== EJEMPLO 3: Agregar Nuevas Facturas Mock ===\n");

        var initialCount = MockInvoiceService.GetMockInvoices().Count;
        Console.WriteLine($"📊 Facturas iniciales: {initialCount}\n");

        // Agregar 3 nuevas facturas
        for (int i = 1; i <= 3; i++)
        {
            MockInvoiceService.AddMockInvoice();
            Console.WriteLine($"✅ Factura mock #{i} agregada");
        }

        var finalCount = MockInvoiceService.GetMockInvoices().Count;
        Console.WriteLine($"\n📊 Total facturas: {finalCount}");
        Console.WriteLine($"✨ Se agregaron {finalCount - initialCount} nuevas facturas\n");
    }

    /// <summary>
    /// Ejemplo 4: Limpiar facturas mock
    /// </summary>
    public static void Example4_ClearMockInvoices()
    {
        Console.WriteLine("=== EJEMPLO 4: Limpiar Facturas Mock ===\n");

        var initialCount = MockInvoiceService.GetMockInvoices().Count;
        Console.WriteLine($"📊 Facturas antes de limpiar: {initialCount}\n");

        MockInvoiceService.ClearMockInvoices();
        Console.WriteLine("🗑️ Facturas mock limpiadas");

        var finalCount = MockInvoiceService.GetMockInvoices().Count;
        Console.WriteLine($"📊 Facturas después de limpiar: {finalCount}\n");
    }

    /// <summary>
    /// Ejemplo 5: Usar mock service con ElectronicBillingService
    /// </summary>
    public static async Task Example5_UseWithBillingService()
    {
        Console.WriteLine("=== EJEMPLO 5: Integración con ElectronicBillingService ===\n");

        // Activar modo mock
        ElectronicBillingService.UseMockData = true;
        Console.WriteLine("✅ Modo mock activado en ElectronicBillingService\n");

        // Crear instancia del servicio
        var billingService = new ElectronicBillingService();

        // Obtener historial (cargará facturas mock automáticamente)
        var invoiceHistory = billingService.InvoiceHistory;
        Console.WriteLine($"📄 Historial cargado: {invoiceHistory.Count} facturas\n");

        // Mostrar estadísticas
        var totalAmount = invoiceHistory.Sum(i => i.TotalAmount);
        var avgAmount = invoiceHistory.Average(i => i.TotalAmount);
        var maxAmount = invoiceHistory.Max(i => i.TotalAmount);
        var minAmount = invoiceHistory.Min(i => i.TotalAmount);

        Console.WriteLine("📊 Estadísticas del Historial:");
        Console.WriteLine($"  💰 Total facturado: ${totalAmount:N2}");
        Console.WriteLine($"  📈 Promedio por factura: ${avgAmount:N2}");
        Console.WriteLine($"  ⬆️ Factura máxima: ${maxAmount:N2}");
        Console.WriteLine($"  ⬇️ Factura mínima: ${minAmount:N2}");
        Console.WriteLine();

        // Agrupar por método de pago
        var byPaymentMethod = invoiceHistory
    .GroupBy(i => i.PaymentMethod)
        .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(i => i.TotalAmount) });

        Console.WriteLine("💳 Facturación por Método de Pago:");
        foreach (var group in byPaymentMethod)
        {
            Console.WriteLine($"  {group.Method}: {group.Count} facturas = ${group.Total:N2}");
        }
        Console.WriteLine();

        // Facturas recientes (últimos 7 días)
        var recentInvoices = invoiceHistory
          .Where(i => i.Date >= DateTime.Now.AddDays(-7))
           .OrderByDescending(i => i.Date);

        Console.WriteLine("🕒 Facturas Recientes (últimos 7 días):");
        foreach (var invoice in recentInvoices)
        {
            Console.WriteLine($"  {invoice.DateFormatted} - {invoice.FullInvoiceNumber} - {invoice.TotalAmountFormatted}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Ejemplo 6: Cambiar entre modo mock y modo real
    /// </summary>
    public static void Example6_ToggleMockMode()
    {
        Console.WriteLine("=== EJEMPLO 6: Alternar entre Modo Mock y Modo Real ===\n");

        // Verificar modo actual
        Console.WriteLine($"📍 Modo actual: {(ElectronicBillingService.UseMockData ? "MOCK" : "REAL")}\n");

        // Modo MOCK (para desarrollo/testing)
        Console.WriteLine("🔧 Activando modo MOCK...");
        ElectronicBillingService.UseMockData = true;
        Console.WriteLine($"✅ Modo actual: {(ElectronicBillingService.UseMockData ? "MOCK" : "REAL")}");
        Console.WriteLine("   - Facturas de ejemplo cargadas");
        Console.WriteLine("   - PDFs de ejemplo disponibles");
        Console.WriteLine("   - No se consumen créditos reales\n");

        // Modo REAL (para producción)
        Console.WriteLine("🚀 Activando modo REAL...");
        ElectronicBillingService.UseMockData = false;
        Console.WriteLine($"✅ Modo actual: {(ElectronicBillingService.UseMockData ? "MOCK" : "REAL")}");
        Console.WriteLine(" - Conexión al servicio de facturación real");
        Console.WriteLine("   - Facturas reales generadas");
        Console.WriteLine("   - PDFs desde el servicio de facturación\n");
    }

    /// <summary>
    /// Ejemplo 7: Simular flujo completo de facturación mock
    /// </summary>
    public static async Task Example7_CompleteWorkflow()
    {
        Console.WriteLine("=== EJEMPLO 7: Flujo Completo de Facturación Mock ===\n");

        // 1. Activar modo mock
        ElectronicBillingService.UseMockData = true;
        Console.WriteLine("1️⃣ Modo mock activado\n");

        // 2. Cargar facturas existentes
        var billingService = new ElectronicBillingService();
        Console.WriteLine($"2️⃣ Historial cargado: {billingService.InvoiceHistory.Count} facturas\n");

        // 3. Agregar nueva factura (simular venta)
        Console.WriteLine("3️⃣ Registrando nueva venta...");
        MockInvoiceService.AddMockInvoice();
        Console.WriteLine($"   ✅ Nueva factura agregada al historial\n");

        // 4. Obtener la factura recién creada
        var latestInvoice = billingService.InvoiceHistory.FirstOrDefault();
        if (latestInvoice != null)
        {
            Console.WriteLine("4️⃣ Detalles de la factura generada:");
            Console.WriteLine($"   📄 Número: {latestInvoice.FullInvoiceNumber}");
            Console.WriteLine($" 👤 Cliente: {latestInvoice.ClientName}");
            Console.WriteLine($"   📝 Documento: {latestInvoice.ClientDocumentNumber}");
            Console.WriteLine($"   💰 Total: {latestInvoice.TotalAmountFormatted}");
            Console.WriteLine($"   📅 Fecha: {latestInvoice.DateFormatted}");
            Console.WriteLine($"   💳 Pago: {latestInvoice.PaymentMethodIcon} {latestInvoice.PaymentMethod}");
            Console.WriteLine($"   {latestInvoice.StatusIcon} Estado: {latestInvoice.Status}\n");

            // 5. Visualizar PDF
            Console.WriteLine("5️⃣ Visualizando PDF de la factura...");
            await MockInvoiceService.OpenMockPdfAsync(latestInvoice.FullInvoiceNumber);
            Console.WriteLine("   ✅ PDF abierto\n");

            // 6. Compartir factura
            Console.WriteLine("6️⃣ Compartiendo factura...");
            await MockInvoiceService.ShareMockPdfAsync(latestInvoice.FullInvoiceNumber);
            Console.WriteLine("   ✅ Factura compartida\n");
        }

        Console.WriteLine("✨ Flujo completo ejecutado exitosamente\n");
    }

    /// <summary>
    /// Ejecuta todos los ejemplos
    /// </summary>
    public static async Task RunAllExamples()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   📄 SISTEMA DE FACTURAS MOCK - EJEMPLOS DE USO 📄      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

        await Example1_ActivateMockMode();
        await Task.Delay(1000);

        await Example2_OpenAndSharePdf();
        await Task.Delay(1000);

        Example3_AddMockInvoices();
        await Task.Delay(1000);

        Example4_ClearMockInvoices();
        await Task.Delay(1000);

        await Example5_UseWithBillingService();
        await Task.Delay(1000);

        Example6_ToggleMockMode();
        await Task.Delay(1000);

        await Example7_CompleteWorkflow();

        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ✅ TODOS LOS EJEMPLOS EJECUTADOS          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
    }
}
