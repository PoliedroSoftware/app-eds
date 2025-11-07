using APP.Eds.Helpers;
using APP.Eds.Models.Billing;
using APP.Eds.Models.Client;
using APP.Eds.Models.PointOfSale;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APP.Eds.Services.Billing;

public class ElectronicBillingService
{
    private readonly string? _authToken;

    public static bool UseMockData { get; set; } = true;

    private static string BillingApiUrl => Configuration.BillingApiUrl;
    private static string PdfApiUrl => Configuration.PdfApiUrl;

    private static ObservableCollection<ElectronicInvoiceModel> _invoiceHistory = new();
    public ObservableCollection<ElectronicInvoiceModel> InvoiceHistory
    {
        get
        {
            
            if (UseMockData && !_invoiceHistory.Any())
            {
                var mockInvoices = MockInvoiceService.GetMockInvoices();
                foreach (var invoice in mockInvoices)
                {
                    _invoiceHistory.Add(invoice);
                }
            }
            return _invoiceHistory;
        }
    }

    public ElectronicBillingService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    }

    public async Task<BillingResult> GenerateElectronicInvoiceAsync(
        SaleModel sale,
       ClientLegalModel client,
        string whatsappNumber = null)
    {
        try
        {

            if (sale == null)
                return new BillingResult { Success = false, Message = "La venta es requerida" };

            if (client == null)
                return new BillingResult { Success = false, Message = "El cliente es requerido" };

            if (!sale.Items.Any())
                return new BillingResult { Success = false, Message = "La venta debe tener al menos un producto" };

            var billingRequest = BuildBillingRequest(sale, client, whatsappNumber);

            // ✨ NEW: Get current islander and EDS information from Preferences
            var islanderName = Preferences.Get("userName", "Usuario no identificado");
            var edsName = Preferences.Get("edsName", "EDS no identificada");

            System.Diagnostics.Debug.WriteLine($"📋 Información de facturación:");
            System.Diagnostics.Debug.WriteLine($"   - Islandero: {islanderName}");
            System.Diagnostics.Debug.WriteLine($"   - EDS: {edsName}");

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(60);

            var json = JsonSerializer.Serialize(new List<BillingRequest> { billingRequest }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            System.Diagnostics.Debug.WriteLine($"JSON Request: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(BillingApiUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = ParseBillingResponse(responseContent);
                if (apiResponse?.Success == true && apiResponse.Data?.Cude != null)
                {

                    var invoice = new ElectronicInvoiceModel
                    {
                        InvoiceHash = apiResponse.Data.Cude,
                        InvoiceNumber = billingRequest.Number,
                        Prefix = billingRequest.Prefix,
                        Date = DateTime.Now,
                        ClientName = client.Name,
                        ClientDocumentNumber = client.DocumentNumber,
                        TotalAmount = sale.Total,
                        TaxAmount = sale.Tax,
                        Subtotal = sale.SubTotal,
                        Status = "Emitida",
                        PaymentMethod = sale.PaymentMethod == PaymentMethod.Cash ? "Efectivo" : "Tarjeta",
                        Email = client.Email ?? "",
                        // ✨ NEW: Add islander and EDS information
                        IslanderName = islanderName,
                        EdsName = edsName
                    };

                    _invoiceHistory.Insert(0, invoice);

                    System.Diagnostics.Debug.WriteLine($"✅ Factura guardada en historial con CUDE: {apiResponse.Data.Cude}");
                    System.Diagnostics.Debug.WriteLine($"   - Generada por: {islanderName}");
                    System.Diagnostics.Debug.WriteLine($"   - En EDS: {edsName}");

                    return new BillingResult
                    {
                        Success = true,
                        Message = "Factura electrónica generada exitosamente",
                        InvoiceNumber = billingRequest.Number,
                        ResponseData = responseContent,
                        InvoiceHash = apiResponse.Data.Cude,
                        QRCode = apiResponse.Data.QRCode,
                        TechProviderFootNote = apiResponse.Data.TechProviderDefaultFootNote
                    };
                }
                else
                {
                    var errorMessage = $"La factura fue procesada pero no se recibió el CUDE. Respuesta: {responseContent}";
                    System.Diagnostics.Debug.WriteLine($"⚠️ {errorMessage}");

                    return new BillingResult
                    {
                        Success = false,
                        Message = "La factura fue procesada pero no se pudo obtener el CUDE de validación",
                        ResponseData = responseContent
                    };
                }
            }
            else
            {
                var errorMessage = $"Error al generar factura: {response.StatusCode} - {responseContent}";
                System.Diagnostics.Debug.WriteLine(errorMessage);

                return new BillingResult
                {
                    Success = false,
                    Message = errorMessage,
                    ResponseData = responseContent
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            var errorMessage = $"Error de conexión con el servicio de facturación: {httpEx.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);

            return new BillingResult
            {
                Success = false,
                Message = errorMessage
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            var errorMessage = $"Timeout al conectar con el servicio de facturación: {timeoutEx.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);

            return new BillingResult
            {
                Success = false,
                Message = "El servicio de facturación no responde. Por favor, intente nuevamente."
            };
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error inesperado al generar factura: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            return new BillingResult
            {
                Success = false,
                Message = errorMessage
            };
        }
    }

    public async Task<byte[]> DownloadInvoicePdfAsync(string invoiceHash)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(invoiceHash))
            {
                throw new ArgumentException("El CUDE/CUFE de la factura es requerido");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Environment", Configuration.BillingApiEnvironment);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.BillingApiToken);
            var url = $"{PdfApiUrl}/{invoiceHash}";
            System.Diagnostics.Debug.WriteLine($"📄 Descargando PDF con CUDE: {invoiceHash}");
            System.Diagnostics.Debug.WriteLine($"📄 URL completa: {url}");

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Error al descargar PDF: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"❌ Respuesta: {errorContent}");
                throw new HttpRequestException($"Error al descargar PDF: {response.StatusCode} - {errorContent}");
            }

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            System.Diagnostics.Debug.WriteLine($"✅ PDF descargado exitosamente: {pdfBytes.Length} bytes");

            return pdfBytes;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error descargando PDF: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<bool> OpenInvoicePdfAsync(string invoiceHash, string invoiceNumber)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📄 Abriendo PDF de factura {invoiceNumber} con CUDE: {invoiceHash}");

            var pdfBytes = await DownloadInvoicePdfAsync(invoiceHash);
            var fileName = $"Factura_{invoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            System.Diagnostics.Debug.WriteLine($"💾 Guardando PDF en: {filePath}");
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            System.Diagnostics.Debug.WriteLine($"🚀 Abriendo PDF en visor del sistema...");
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });

            System.Diagnostics.Debug.WriteLine($"✅ PDF abierto exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error abriendo PDF de factura {invoiceNumber}: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
               "Error al Abrir PDF",
                $"No se pudo abrir el PDF de la factura {invoiceNumber}.\n\n" +
                $"Error: {ex.Message}\n\n" +
                $"Verifique su conexión a internet e intente nuevamente.",
                "OK");
            return false;
        }
    }

    public async Task<bool> ShareInvoicePdfAsync(string invoiceHash, string invoiceNumber)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📤 Compartiendo PDF de factura {invoiceNumber} con CUDE: {invoiceHash}");

            var pdfBytes = await DownloadInvoicePdfAsync(invoiceHash);
            var fileName = $"Factura_{invoiceNumber}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            System.Diagnostics.Debug.WriteLine($"💾 Guardando PDF en: {filePath}");
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            System.Diagnostics.Debug.WriteLine($"🚀 Abriendo sheet de compartir...");
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = $"Factura Electrónica {invoiceNumber}",
                File = new ShareFile(filePath)
            });

            System.Diagnostics.Debug.WriteLine($"✅ PDF compartido exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error compartiendo PDF de factura {invoiceNumber}: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error al Compartir PDF",
                $"No se pudo compartir el PDF de la factura {invoiceNumber}.\n\n" +
                $"Error: {ex.Message}\n\n" +
                $"Verifique su conexión a internet e intente nuevamente.",
                "OK");
            return false;
        }
    }

    private BillingApiResponse ParseBillingResponse(string responseContent)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Parseando respuesta de facturación...");
            System.Diagnostics.Debug.WriteLine($"📄 Response: {responseContent}");

            var response = JsonSerializer.Deserialize<BillingApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Respuesta parseada correctamente");
                System.Diagnostics.Debug.WriteLine($"   - Success: {response.Success}");
                System.Diagnostics.Debug.WriteLine($"   - Code: {response.Code}");
                System.Diagnostics.Debug.WriteLine($"   - Info: {response.Info}");

                if (response.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"- CUDE: {response.Data.Cude}");
                    System.Diagnostics.Debug.WriteLine($"   - QRCode length: {response.Data.QRCode?.Length ?? 0}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Data es null en la respuesta");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Respuesta deserializada es null");
            }

            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error parseando respuesta: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    private BillingRequest BuildBillingRequest(SaleModel sale, ClientLegalModel client, string whatsappNumber)
    {
        var now = DateTime.Now;
        var invoiceNumber = GenerateInvoiceNumber();
        double subtotal = sale.SubTotal;
        double taxAmount = sale.Tax;
        double total = sale.Total;
        return new BillingRequest
        {
            Date = now.ToString("yyyy-MM-dd"),
            Time = now.ToString("HH:mm:ss.fffZ"),
            SendToEmail = client.Email ?? "",
            SoftwareManufacturer = new SoftwareManufacturer
            {
                OwnerName = "Poliedro Software",
                SoftwareName = "EDS",
                CompanyName = "Poliedro Software S.A.S"
            },
            PayPointInfo = new PayPointInfo
            {
                Code = "FE",
                Address = "Punto de Venta Principal",
                CashierName = Preferences.Get("userName", "Cajero"),
                PayPointType = "Principal",
                SaleCode = sale.SaleId.ToString()
            },
            Number = invoiceNumber,
            Prefix = "SETT",
            TransactionDate = now.ToString("yyyy-MM-dd"),
            OrderReference = new OrderReference
            {
                IdOrder = sale.SaleId.ToString()
            },
            SendEmail = !string.IsNullOrWhiteSpace(client.Email),
            CustomerEntity = new CustomerEntity
            {
                IdentificationNumber = client.DocumentNumber,
                Dv = client.VerificationDigit > 0 ? client.VerificationDigit.ToString() : "0",
                Name = client.Name,
                Phone = whatsappNumber ?? "",
                Email = client.Email ?? "",
                Address = "",
                City = "",
                State = "",
                Country = "Colombia",
                TypeDocumentIdentificationId = client.DocumentTypeId,
                TypeOrganizationId = client.DocumentTypeId == 1 ? 1 : 2,
                TypeLiabilityId = client.VatResponsibleParty ? 1 : 2,
                MunicipalityId = 0,
                MunicipalityCode = "",
                TypeRegimeId = client.SimpleTaxRegime ? 2 : 1,
                MultipleResolution = "",
                ApiKey = "",
                Profit = 0,
                MerchantRegistration = ""
            },
            PaymentEntity = new PaymentEntity
            {
                PaymentFormId = sale.PaymentMethod == PaymentMethod.Cash ? 1 : 2,
                PaymentMethodId = sale.PaymentMethod == PaymentMethod.Cash ? 10 : 48,
                PaymentDueDate = now.ToString("yyyy-MM-dd"),
                DurationMeasure = "0"
            },
            ItemElectronicEntity = [.. sale.Items.Select(item => new ItemElectronicEntity
            {
                Description = item.ProductName,
                Code = item.ProductName.Replace(" ", "").ToUpper(),
                InvoicedQuantity = item.Quantity,
                BaseQuantity = 1,
                UnitPrice = item.UnitPrice,
                PriceAmount = item.UnitPrice,
                LineExtensionAmount = item.TotalPrice,
                Subtotal = item.TotalPrice,
                UnitMeasureId = 94,
                TypeItemIdentificationId = 4,
                Transaccion = 1,
                FreeOfChargeIndicator = false,
                Percent = 0,
                TaxAmount = 0,
                Notes = "",
                TaxTotals = [],
                WithHoldingTaxTotal = [],
                AllowanceCharges = []
            })],
            Resolution = "18760000001",
            ResolutionText = "Resolución DIAN Número 18760000001",
            HeadNote = "",
            FootNote = "Gracias por su compra",
            Notes = "",
            AllowanceTotal = 0,
            InvoiceBaseTotal = subtotal,
            InvoiceTaxExclusiveTotal = subtotal,
            InvoiceTaxInclusiveTotal = total,
            TotalToPay = total,
            FinalTotalToPay = total,
            AllTaxTotalEntity = [],
            AllHoldingsTaxTotalEntity = []
        };
    }

    /// <summary>
    /// Genera un número de factura único
    /// </summary>
    private string GenerateInvoiceNumber()
    {
        var now = DateTime.Now;
        return $"{now:yyyyMMdd}{now:HHmmss}";
    }

    /// <summary>
    /// Genera una nota crédito para anular una factura electrónica
    /// </summary>
    public async Task<BillingResult> GenerateCreditNoteAsync(ElectronicInvoiceModel invoice, string reason)
    {
        try
        {
            if (invoice == null)
                return new BillingResult { Success = false, Message = "La factura es requerida" };

            if (string.IsNullOrWhiteSpace(invoice.InvoiceHash))
                return new BillingResult { Success = false, Message = "La factura no tiene un CUDE válido" };

            if (string.IsNullOrWhiteSpace(reason))
                return new BillingResult { Success = false, Message = "El motivo es requerido" };

            System.Diagnostics.Debug.WriteLine($"📝 Generando nota crédito para factura {invoice.FullInvoiceNumber}");
            System.Diagnostics.Debug.WriteLine($"   - CUDE original: {invoice.InvoiceHash}");
            System.Diagnostics.Debug.WriteLine($"   - Motivo: {reason}");

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            httpClient.DefaultRequestHeaders.Add("X-Environment", Configuration.BillingApiEnvironment);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.BillingApiToken);

            var creditNoteRequest = BuildCreditNoteRequest(invoice, reason);

            var json = JsonSerializer.Serialize(new List<object> { creditNoteRequest }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            System.Diagnostics.Debug.WriteLine($"JSON Credit Note Request: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // TODO: Actualizar con la URL correcta del endpoint de notas crédito
            var creditNoteApiUrl = Configuration.BillingApiUrl.Replace("/billing", "/credit-note");
            var response = await httpClient.PostAsync(creditNoteApiUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = ParseBillingResponse(responseContent);
                if (apiResponse?.Success == true && apiResponse.Data?.Cude != null)
                {
                    // Actualizar estado de la factura original
                    invoice.Status = "Anulada";
                    OnPropertyChanged(nameof(InvoiceHistory));

                    System.Diagnostics.Debug.WriteLine($"✅ Nota crédito generada exitosamente");
                    System.Diagnostics.Debug.WriteLine($"   - CUFE nota crédito: {apiResponse.Data.Cude}");

                    var creditNoteNumber = GenerateInvoiceNumber();
                    
                    return new BillingResult
                    {
                        Success = true,
                        Message = "Nota crédito generada exitosamente",
                        InvoiceNumber = creditNoteNumber,
                        CreditNoteNumber = $"NC-{creditNoteNumber}",
                        ResponseData = responseContent,
                        InvoiceHash = apiResponse.Data.Cude,
                        QRCode = apiResponse.Data.QRCode,
                        TechProviderFootNote = apiResponse.Data.TechProviderDefaultFootNote
                    };
                }
                else
                {
                    var errorMessage = $"La nota crédito fue procesada pero no se recibió el CUFE. Respuesta: {responseContent}";
                    System.Diagnostics.Debug.WriteLine($"⚠️ {errorMessage}");

                    return new BillingResult
                    {
                        Success = false,
                        Message = "La nota crédito fue procesada pero no se pudo obtener el CUFE de validación",
                        ResponseData = responseContent
                    };
                }
            }
            else
            {
                var errorMessage = $"Error al generar nota crédito: {response.StatusCode} - {responseContent}";
                System.Diagnostics.Debug.WriteLine(errorMessage);

                return new BillingResult
                {
                    Success = false,
                    Message = errorMessage,
                    ResponseData = responseContent
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            var errorMessage = $"Error de conexión con el servicio de facturación: {httpEx.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);

            return new BillingResult
            {
                Success = false,
                Message = errorMessage
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            var errorMessage = $"Timeout al conectar con el servicio de facturación: {timeoutEx.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);

            return new BillingResult
            {
                Success = false,
                Message = "El servicio de facturación no responde. Por favor, intente nuevamente."
            };
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error inesperado al generar nota crédito: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            return new BillingResult
            {
                Success = false,
                Message = errorMessage
            };
        }
    }

    private object BuildCreditNoteRequest(ElectronicInvoiceModel invoice, string reason)
    {
        var now = DateTime.Now;
        var creditNoteNumber = GenerateInvoiceNumber();

        // Estructura básica de nota crédito
        // TODO: Ajustar según la estructura real requerida por la API
        return new
        {
            date = now.ToString("yyyy-MM-dd"),
            time = now.ToString("HH:mm:ss.fffZ"),
            number = creditNoteNumber,
            prefix = "NC",
            referencedInvoiceNumber = invoice.InvoiceNumber,
            referencedInvoicePrefix = invoice.Prefix,
            referencedInvoiceCude = invoice.InvoiceHash,
            discrepancyResponseCode = "1", // 1 = Anulación
            discrepancyDescription = reason,
            billingReference = new
            {
                number = invoice.InvoiceNumber,
                uuid = invoice.InvoiceHash,
                issueDate = invoice.Date.ToString("yyyy-MM-dd")
            },
            customerEntity = new
            {
                identificationNumber = invoice.ClientDocumentNumber,
                name = invoice.ClientName,
                email = invoice.Email
            },
            totalAmount = invoice.TotalAmount,
            taxAmount = invoice.TaxAmount,
            subtotal = invoice.Subtotal
        };
    }

    // Necesitamos un evento PropertyChanged para notificar cambios
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ...existing code...
}

public class BillingResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string InvoiceNumber { get; set; }
    public string CreditNoteNumber { get; set; } // ✨ NEW
    public string ResponseData { get; set; }
    public string InvoiceHash { get; set; }
    public string QRCode { get; set; }
    public string TechProviderFootNote { get; set; }
}

public class BillingApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("data")]
    public BillingDataResponse Data { get; set; }

    [JsonPropertyName("ER")]
    public string ER { get; set; }
}

public class BillingDataResponse
{
    [JsonPropertyName("cude")]
    public string Cude { get; set; }

    [JsonPropertyName("QRCode")]
    public string QRCode { get; set; }

    [JsonPropertyName("techProviderDefaultFootNote")]
    public string TechProviderDefaultFootNote { get; set; }
}
