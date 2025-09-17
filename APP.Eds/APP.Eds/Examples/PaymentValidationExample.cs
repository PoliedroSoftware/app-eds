using APP.Eds.Services.TypeOfCollection;

namespace APP.Eds.Examples
{
    /// <summary>
    /// Ejemplo de uso del sistema de validación de formas de pago
    /// </summary>
    public class PaymentValidationExample
    {
        private TypeOfCollectionService _paymentService;

        public PaymentValidationExample()
        {
            _paymentService = new TypeOfCollectionService();
        }

        /// <summary>
        /// Ejemplo de cómo configurar y validar pagos
        /// </summary>
        public async Task ConfigureAndValidatePaymentsExample()
        {
            try
            {
                // 1. Configurar el total de ventas (esto vendría de tu servicio de ventas)
                decimal totalVentas = 150000m; // $150,000
                _paymentService.SetSalesTotal(totalVentas);

                // 2. Agregar métodos de pago con montos específicos
                var efectivo = new PaymentMethodItem
                {
                    Id = 1,
                    Name = "Efectivo Pesos",
                    Type = "Efectivo",
                    Provider = "N/A",
                    Status = "Activo",
                    Icon = "??"
                };
                await _paymentService.AddPaymentMethodWithAmountAsync(efectivo, 80000m);

                var tarjeta = new PaymentMethodItem
                {
                    Id = 2,
                    Name = "Tarjeta Visa",
                    Type = "Tarjeta de Credito",
                    Provider = "Visa",
                    Status = "Activo",
                    Icon = "??"
                };
                await _paymentService.AddPaymentMethodWithAmountAsync(tarjeta, 70000m);

                // 3. La validación se ejecutará automáticamente
                // También puedes ejecutarla manualmente:
                await _paymentService.TriggerPaymentValidationAsync();

                // 4. Verificar el estado
                if (_paymentService.IsPaymentComplete)
                {
                    Console.WriteLine("? Todos los pagos han sido registrados");
                    Console.WriteLine($"Total ventas: ${_paymentService.TotalSalesAmount:N2}");
                    Console.WriteLine($"Total pagos: ${_paymentService.TotalPaymentMethodsAmount:N2}");
                }
                else
                {
                    Console.WriteLine($"? Faltan pagos por registrar: ${_paymentService.RemainingAmount:N2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el ejemplo: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo de integración con el servicio de Court
        /// </summary>
        public async Task IntegrateWithCourtServiceExample()
        {
            try
            {
                // 1. Obtener total de ventas desde el servicio de Court
                // var courtService = new CourtService();
                // decimal totalVentas = courtService.GetTotalAmount();

                // Para este ejemplo, usamos un valor simulado
                decimal totalVentas = 250000m;

                // 2. Configurar el total en el servicio de pagos
                _paymentService.SetSalesTotal(totalVentas);

                // 3. Los métodos de pago se agregarían desde la UI
                // Cada vez que el usuario agrega un método, se valida automáticamente

                // 4. Al final del proceso de cierre, validar manualmente
                await _paymentService.TriggerPaymentValidationAsync();

                // 5. Si los pagos están completos, mostrar el popup
                if (_paymentService.IsPaymentComplete)
                {
                    await _paymentService.ShowPaymentMethodsPopupAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la integración: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo de manejo de diferentes escenarios
        /// </summary>
        public async Task HandleDifferentScenariosExample()
        {
            // Escenario 1: Pagos exactos
            _paymentService.SetSalesTotal(100000m);
            await _paymentService.AddPaymentMethodWithAmountAsync(
                new PaymentMethodItem { Name = "Efectivo", Status = "Activo" }, 100000m);
            // Resultado: Mostrará mensaje de "Pagos Completos" y popup

            // Escenario 2: Pagos excedentes
            _paymentService.SetSalesTotal(100000m);
            await _paymentService.AddPaymentMethodWithAmountAsync(
                new PaymentMethodItem { Name = "Efectivo", Status = "Activo" }, 120000m);
            // Resultado: Mostrará mensaje de "Exceso en Pagos"

            // Escenario 3: Pagos pendientes
            _paymentService.SetSalesTotal(100000m);
            await _paymentService.AddPaymentMethodWithAmountAsync(
                new PaymentMethodItem { Name = "Efectivo", Status = "Activo" }, 80000m);
            // Resultado: Mostrará mensaje de "Pagos Pendientes"
        }
    }
}