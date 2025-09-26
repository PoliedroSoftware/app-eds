using APP.Eds.Services.Court;
using APP.Eds.Models.Court;

namespace APP.Eds.Examples
{
    /// <summary>
    /// Ejemplo de uso de la nueva validación de métodos de pago en el envío de cortes
    /// </summary>
    public class CourtPaymentValidationExample
    {
        private CourtService _courtService;

        public CourtPaymentValidationExample()
        {
            _courtService = CourtService.Instance;
        }

        /// <summary>
        /// Ejemplo de caso exitoso donde los métodos de pago coinciden con el total de ventas
        /// </summary>
        public async Task ExampleSuccessfulPaymentValidation()
        {
            try
            {
                // 1. Configurar ventas simuladas (esto normalmente viene de los dispensadores)
                _courtService.AmountResults.Clear();
                _courtService.AmountResults.Add(50000); // Venta dispensador 1
                _courtService.AmountResults.Add(75000); // Venta dispensador 2
                // Total de ventas: $125,000

                // 2. Configurar métodos de pago que suman exactamente el total de ventas
                _courtService.CourtTypeOfCollections = new System.Collections.ObjectModel.ObservableCollection<CourtTypeOfCollection>
                {
                    new CourtTypeOfCollection
                    {
                        IdTypeOfCollection = 1,
                        TypeOfCollectionName = "Efectivo",
                        Amount = 80000, // $80,000 en efectivo
                        Description = "Pago en efectivo"
                    },
                    new CourtTypeOfCollection
                    {
                        IdTypeOfCollection = 2,
                        TypeOfCollectionName = "Tarjeta de Crédito",
                        Amount = 45000, // $45,000 en tarjeta
                        Description = "Pago con tarjeta Visa"
                    }
                };
                // Total métodos de pago: $125,000 (coincide exactamente)

                // 3. Intentar enviar el corte
                Console.WriteLine("=== Ejemplo: Validación Exitosa ===");
                Console.WriteLine($"Total de ventas: ${_courtService.GetTotalAmount():N2}");
                Console.WriteLine($"Total métodos de pago: ${_courtService.GetTotalTypeOfCollection():N2}");
                Console.WriteLine("Iniciando envío del corte...");

                await _courtService.SendCourtDataAsync();

                if (_courtService.LastSendWasSuccessful)
                {
                    Console.WriteLine("✅ Corte enviado exitosamente");
                    Console.WriteLine("La validación de pagos fue exitosa - los totales coinciden");
                }
                else
                {
                    Console.WriteLine("❌ El envío falló por otras razones");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo exitoso: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo donde los métodos de pago son menores al total de ventas
        /// </summary>
        public async Task ExampleInsufficientPaymentValidation()
        {
            try
            {
                // 1. Configurar ventas simuladas
                _courtService.AmountResults.Clear();
                _courtService.AmountResults.Add(100000); // Total: $100,000

                // 2. Configurar métodos de pago INSUFICIENTES
                _courtService.CourtTypeOfCollections = new System.Collections.ObjectModel.ObservableCollection<CourtTypeOfCollection>
                {
                    new CourtTypeOfCollection
                    {
                        IdTypeOfCollection = 1,
                        TypeOfCollectionName = "Efectivo",
                        Amount = 60000, // Solo $60,000
                        Description = "Pago parcial en efectivo"
                    }
                };
                // Total métodos de pago: $60,000 (falta $40,000)

                Console.WriteLine("\n=== Ejemplo: Pagos Insuficientes ===");
                Console.WriteLine($"Total de ventas: ${_courtService.GetTotalAmount():N2}");
                Console.WriteLine($"Total métodos de pago: ${_courtService.GetTotalTypeOfCollection():N2}");
                Console.WriteLine($"Faltante: ${_courtService.GetTotalAmount() - _courtService.GetTotalTypeOfCollection():N2}");
                Console.WriteLine("Iniciando envío del corte...");

                await _courtService.SendCourtDataAsync();

                if (!_courtService.LastSendWasSuccessful)
                {
                    Console.WriteLine("❌ El envío fue rechazado correctamente");
                    Console.WriteLine("La validación detectó que faltan métodos de pago");
                }
                else
                {
                    Console.WriteLine("⚠️ El envío no debería haber sido exitoso");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo de pagos insuficientes: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo donde los métodos de pago exceden el total de ventas
        /// </summary>
        public async Task ExampleExcessivePaymentValidation()
        {
            try
            {
                // 1. Configurar ventas simuladas
                _courtService.AmountResults.Clear();
                _courtService.AmountResults.Add(80000); // Total: $80,000

                // 2. Configurar métodos de pago EXCESIVOS
                _courtService.CourtTypeOfCollections = new System.Collections.ObjectModel.ObservableCollection<CourtTypeOfCollection>
                {
                    new CourtTypeOfCollection
                    {
                        IdTypeOfCollection = 1,
                        TypeOfCollectionName = "Efectivo",
                        Amount = 70000,
                        Description = "Pago en efectivo"
                    },
                    new CourtTypeOfCollection
                    {
                        IdTypeOfCollection = 2,
                        TypeOfCollectionName = "Tarjeta de Débito",
                        Amount = 30000,
                        Description = "Pago con tarjeta de débito"
                    }
                };
                // Total métodos de pago: $100,000 (excede por $20,000)

                Console.WriteLine("\n=== Ejemplo: Pagos Excesivos ===");
                Console.WriteLine($"Total de ventas: ${_courtService.GetTotalAmount():N2}");
                Console.WriteLine($"Total métodos de pago: ${_courtService.GetTotalTypeOfCollection():N2}");
                Console.WriteLine($"Excedente: ${_courtService.GetTotalTypeOfCollection() - _courtService.GetTotalAmount():N2}");
                Console.WriteLine("Iniciando envío del corte...");

                await _courtService.SendCourtDataAsync();

                if (!_courtService.LastSendWasSuccessful)
                {
                    Console.WriteLine("❌ El envío fue rechazado correctamente");
                    Console.WriteLine("La validación detectó un exceso en los métodos de pago");
                }
                else
                {
                    Console.WriteLine("⚠️ El envío no debería haber sido exitoso");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo de pagos excesivos: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo de caso especial: sin ventas registradas
        /// </summary>
        public async Task ExampleNoSalesValidation()
        {
            try
            {
                // 1. Sin ventas (caso especial)
                _courtService.AmountResults.Clear(); // Total: $0

                // 2. Sin métodos de pago
                _courtService.CourtTypeOfCollections = new System.Collections.ObjectModel.ObservableCollection<CourtTypeOfCollection>();

                Console.WriteLine("\n=== Ejemplo: Sin Ventas ===");
                Console.WriteLine($"Total de ventas: ${_courtService.GetTotalAmount():N2}");
                Console.WriteLine($"Total métodos de pago: ${_courtService.GetTotalTypeOfCollection():N2}");
                Console.WriteLine("Iniciando envío del corte...");

                await _courtService.SendCourtDataAsync();

                if (!_courtService.LastSendWasSuccessful)
                {
                    Console.WriteLine("❌ El envío fue rechazado");
                    Console.WriteLine("Se requiere al menos un método de pago cuando hay ventas");
                }
                else
                {
                    Console.WriteLine("✅ El envío fue exitoso para caso sin ventas");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo sin ventas: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejecuta todos los ejemplos de validación
        /// </summary>
        public async Task RunAllValidationExamples()
        {
            Console.WriteLine("🔍 EJEMPLOS DE VALIDACIÓN DE MÉTODOS DE PAGO EN ENVÍO DE CORTES");
            Console.WriteLine("================================================================");

            await ExampleSuccessfulPaymentValidation();
            await ExampleInsufficientPaymentValidation();
            await ExampleExcessivePaymentValidation();
            await ExampleNoSalesValidation();

            Console.WriteLine("\n📋 RESUMEN DE VALIDACIONES IMPLEMENTADAS:");
            Console.WriteLine("- ✅ Verificación de igualdad entre ventas y métodos de pago");
            Console.WriteLine("- ✅ Detección de pagos insuficientes");
            Console.WriteLine("- ✅ Detección de pagos excesivos");
            Console.WriteLine("- ✅ Validación de métodos de pago obligatorios con ventas");
            Console.WriteLine("- ✅ Mensajes de error específicos y detallados");
            Console.WriteLine("- ✅ Bloqueo de envío cuando la validación falla");
        }

        /// <summary>
        /// Método de utilidad para configurar un escenario específico de prueba
        /// </summary>
        public void SetupTestScenario(double totalSales, List<(string paymentMethod, double amount, string description)> payments)
        {
            try
            {
                // Configurar ventas
                _courtService.AmountResults.Clear();
                _courtService.AmountResults.Add(totalSales);

                // Configurar métodos de pago
                _courtService.CourtTypeOfCollections = new System.Collections.ObjectModel.ObservableCollection<CourtTypeOfCollection>();

                int id = 1;
                foreach (var (paymentMethod, amount, description) in payments)
                {
                    _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                    {
                        IdTypeOfCollection = id++,
                        TypeOfCollectionName = paymentMethod,
                        Amount = amount,
                        Description = description
                    });
                }

                Console.WriteLine($"Escenario configurado:");
                Console.WriteLine($"- Total ventas: ${totalSales:N2}");
                Console.WriteLine($"- Total métodos pago: ${_courtService.GetTotalTypeOfCollection():N2}");
                Console.WriteLine($"- Métodos registrados: {payments.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando escenario de prueba: {ex.Message}");
            }
        }
    }
}