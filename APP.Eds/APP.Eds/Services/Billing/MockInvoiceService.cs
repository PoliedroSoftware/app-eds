using APP.Eds.Models.Billing;
using System.Collections.ObjectModel;

namespace APP.Eds.Services.Billing;

/// <summary>
/// Servicio mock para generar facturas de ejemplo con el PDF incluido en el proyecto
/// </summary>
public class MockInvoiceService
{
    private static readonly Random _random = new();
    private static ObservableCollection<ElectronicInvoiceModel> _mockInvoices;

    // ✨ NEW: Sample islander names for mock data
    private static readonly string[] IslanderNames = new[]
    {
        "Juan Carlos Rodriguez",
        "Maria Elena Gutierrez",
        "Carlos Alberto Mendez",
        "Ana Lucia Torres",
        "Pedro Antonio Martinez",
        "Sofia Fernanda Garcia",
        "Luis Miguel Hernandez",
        "Carmen Rosa Diaz"
    };

    // ✨ NEW: Sample EDS names for mock data
    private static readonly string[] EdsNames = new[]
    {
        "EDS Principal Centro",
        "EDS Norte Autopista",
        "EDS Sur Terminal",
        "EDS Oriente Plaza",
        "EDS Occidente Centro",
        "EDS Aeropuerto",
        "EDS Industrial",
        "EDS La Estación"
    };

    /// <summary>
    /// Obtiene una colección de facturas mock de ejemplo
    /// </summary>
    public static ObservableCollection<ElectronicInvoiceModel> GetMockInvoices()
    {
        if (_mockInvoices != null)
            return _mockInvoices;

        _mockInvoices = new ObservableCollection<ElectronicInvoiceModel>();

        // Generar 10 facturas de ejemplo
        for (int i = 1; i <= 10; i++)
        {
            _mockInvoices.Add(GenerateMockInvoice(i));
        }

        return _mockInvoices;
    }

    /// <summary>
    /// Genera una factura mock individual
    /// </summary>
    private static ElectronicInvoiceModel GenerateMockInvoice(int index)
    {
        var daysAgo = _random.Next(0, 30);
        var date = DateTime.Now.AddDays(-daysAgo);
        var invoiceNumber = $"{date:yyyyMMdd}{_random.Next(1000, 9999)}";

        var subtotal = _random.Next(50000, 500000);
        var taxRate = 0.19;
        var taxAmount = subtotal * taxRate;
        var total = subtotal + taxAmount;

        var clients = new[]
        {
  new { Name = "ABC DISTRIBUIDORA S.A.S", DocNumber = "900123456", DocType = "NIT", Email = "contabilidad@abcdistribuidora.com" },
 new { Name = "TRANSPORTES DEL SUR LTDA", DocNumber = "800234567", DocType = "NIT", Email = "facturacion@transportesdelsur.com" },
            new { Name = "COMERCIALIZADORA ANDINA S.A", DocNumber = "900345678", DocType = "NIT", Email = "cuentasporcobrar@andina.co" },
          new { Name = "JUAN CARLOS RODRIGUEZ", DocNumber = "79456123", DocType = "CC", Email = "jcrodriguez@email.com" },
            new { Name = "MARIA FERNANDA GARCIA", DocNumber = "52789456", DocType = "CC", Email = "mfgarcia@email.com" },
   new { Name = "INVERSIONES EL TRIUNFO S.A.S", DocNumber = "900456789", DocType = "NIT", Email = "admin@eltriunfo.com" },
  new { Name = "SERVICIOS INTEGRALES DEL CARIBE", DocNumber = "900567890", DocType = "NIT", Email = "contabilidad@sicaribe.com" },
     new { Name = "PEDRO ANTONIO MARTINEZ", DocNumber = "1015678901", DocType = "CC", Email = "pamartinez@email.com" },
            new { Name = "LOGISTICA Y TRANSPORTE NACIONAL", DocNumber = "900678901", DocType = "NIT", Email = "facturas@logtrans.com" },
    new { Name = "ANA LUCIA TORRES", DocNumber = "41234567", DocType = "CC", Email = "altorres@email.com" }
      };

        var client = clients[index % clients.Length];
        var paymentMethods = new[] { "Efectivo", "Tarjeta", "Transferencia" };
        var statuses = new[] { "Emitida", "Emitida", "Emitida", "Emitida", "Emitida" }; // Mayoría emitidas
        
        // ✨ NEW: Generate consistent IDs for islanders and EDS
        var islanderIndex = _random.Next(IslanderNames.Length);
        var edsIndex = _random.Next(EdsNames.Length);

        return new ElectronicInvoiceModel
        {
            InvoiceHash = GenerateMockCUDE(),
            InvoiceNumber = invoiceNumber,
            Prefix = "SETT",
            Date = date,
            ClientName = client.Name,
            ClientDocumentNumber = client.DocNumber,
            TotalAmount = total,
            TaxAmount = taxAmount,
            Subtotal = subtotal,
            Status = statuses[_random.Next(statuses.Length)],
            PaymentMethod = paymentMethods[_random.Next(paymentMethods.Length)],
            Email = client.Email,
            QRCode = GenerateMockQRCode(),
            TechProviderFootNote = "Factura Electrónica generada por Poliedro Software - Sistema EDS",
            // ✨ NEW: Add mock islander and EDS information with IDs
            IslanderId = $"ISL{islanderIndex + 1:D3}",
            IslanderName = IslanderNames[islanderIndex],
            EdsId = $"EDS{edsIndex + 1:D3}",
            EdsName = EdsNames[edsIndex]
        };
    }

    /// <summary>
    /// Genera un CUDE mock de 64 caracteres (simulando SHA-256)
    /// </summary>
    private static string GenerateMockCUDE()
    {
        const string chars = "ABCDEF0123456789";
        return new string(Enumerable.Repeat(chars, 64)
         .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Genera un código QR mock base64
    /// </summary>
    private static string GenerateMockQRCode()
    {
        // Simular un QR code base64 pequeño (en producción vendría del servicio real)
        return "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    }

    /// <summary>
    /// Obtiene el PDF de ejemplo almacenado en el proyecto
    /// </summary>
    public static async Task<byte[]> GetMockPdfAsync()
    {
        try
        {
            // Intentar cargar el PDF desde Resources/Raw
            using var stream = await FileSystem.OpenAppPackageFileAsync("factura.pdf");
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error cargando PDF mock: {ex.Message}");

            // Si no se puede cargar, generar un PDF mínimo válido
            return GenerateMinimalPdf();
        }
    }

    /// <summary>
    /// Genera un PDF mínimo válido si no se puede cargar el de ejemplo
    /// </summary>
    private static byte[] GenerateMinimalPdf()
    {
        // PDF mínimo válido (1 página en blanco con un texto)
        var pdfContent = @"%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /Resources 4 0 R /MediaBox [0 0 612 792] /Contents 5 0 R >>
endobj
4 0 obj
<< /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >>
endobj
5 0 obj
<< /Length 44 >>
stream
BT
/F1 12 Tf
100 700 Td
(Factura Electrónica) Tj
ET
endstream
endobj
xref
0 6
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000214 00000 n 
0000000303 00000 n 
trailer
<< /Size 6 /Root 1 0 R >>
startxref
395
%%EOF";

        return System.Text.Encoding.ASCII.GetBytes(pdfContent);
    }

    /// <summary>
    /// Abre el PDF mock en el visor del sistema
    /// </summary>
    public static async Task<bool> OpenMockPdfAsync(string invoiceNumber)
    {
        try
        {
            var pdfBytes = await GetMockPdfAsync();
            var fileName = $"Factura_Mock_{invoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error abriendo PDF mock: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error al Abrir PDF",
                       $"No se pudo abrir el PDF de ejemplo.\n\nError: {ex.Message}",
            "OK");
            return false;
        }
    }

    /// <summary>
    /// Comparte el PDF mock
    /// </summary>
    public static async Task<bool> ShareMockPdfAsync(string invoiceNumber)
    {
        try
        {
            var pdfBytes = await GetMockPdfAsync();
            var fileName = $"Factura_Mock_{invoiceNumber}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = $"Factura Electrónica Mock {invoiceNumber}",
                File = new ShareFile(filePath)
            });

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error compartiendo PDF mock: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
              "Error al Compartir PDF",
          $"No se pudo compartir el PDF de ejemplo.\n\nError: {ex.Message}",
             "OK");
            return false;
        }
    }

    /// <summary>
    /// Agrega una nueva factura mock al inicio del historial
    /// </summary>
    public static void AddMockInvoice()
    {
        if (_mockInvoices == null)
            _mockInvoices = new ObservableCollection<ElectronicInvoiceModel>();

        var newInvoice = GenerateMockInvoice(_mockInvoices.Count + 1);
        newInvoice.Date = DateTime.Now; // Factura actual

        _mockInvoices.Insert(0, newInvoice);
    }

    /// <summary>
    /// Limpia todas las facturas mock
    /// </summary>
    public static void ClearMockInvoices()
    {
        _mockInvoices?.Clear();
    }
}
