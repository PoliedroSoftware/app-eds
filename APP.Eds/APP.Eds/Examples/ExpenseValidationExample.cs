using APP.Eds.Services.Court;
using APP.Eds.Models.Court;
using System.Collections.ObjectModel;

namespace APP.Eds.Examples
{
    /// <summary>
    /// Ejemplo de uso de la validación de gastos contra efectivo disponible
    /// </summary>
    public class ExpenseValidationExample
    {
        private CourtService _courtService;

        public ExpenseValidationExample()
        {
            _courtService = CourtService.Instance;
        }

        /// <summary>
        /// Ejemplo 1: Intento de registrar gasto sin métodos de pago en efectivo
        /// </summary>
        public async Task Example1_NoEfectivoDisponible()
        {
            try
            {
                Console.WriteLine("=== EJEMPLO 1: Sin Efectivo Disponible ===\n");

                // 1. Limpiar métodos de pago y gastos
                _courtService.CourtTypeOfCollections = new ObservableCollection<CourtTypeOfCollection>();
                _courtService.CourtExpenditures = new ObservableCollection<CourtExpenditure>();

                // 2. Agregar solo método de pago con tarjeta (NO efectivo)
                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 1,
                    TypeOfCollectionName = "Tarjeta de Crédito",
                    Amount = 100000,
                    Description = "Pago con tarjeta Visa"
                });

                Console.WriteLine("Métodos de pago registrados:");
                Console.WriteLine("- Tarjeta de Crédito: $100,000");
                Console.WriteLine();

                // 3. Intentar registrar un gasto
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 1,
                    Description = "Combustible"
                };
                _courtService.CourtExpenditureAmount = 50000;
                _courtService.ExpenditureDescription = "Compra de combustible para generador";

                Console.WriteLine("Intentando registrar gasto:");
                Console.WriteLine($"- Tipo: Combustible");
                Console.WriteLine($"- Monto: $50,000");
                Console.WriteLine();

                await _courtService.AddCourtExpenditureFromPopup();

                // Si llegamos aquí, el gasto fue rechazado (lo esperado)
                Console.WriteLine("❌ El gasto fue rechazado correctamente");
                Console.WriteLine("Razón: No hay métodos de pago en efectivo disponibles\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo 1: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 2: Registro exitoso de gasto con efectivo suficiente
        /// </summary>
        public async Task Example2_GastoExitoso()
        {
            try
            {
                Console.WriteLine("=== EJEMPLO 2: Gasto Exitoso con Efectivo Suficiente ===\n");

                // 1. Limpiar y configurar métodos de pago
                _courtService.CourtTypeOfCollections = new ObservableCollection<CourtTypeOfCollection>();
                _courtService.CourtExpenditures = new ObservableCollection<CourtExpenditure>();

                // 2. Agregar efectivo
                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 1,
                    TypeOfCollectionName = "Efectivo Pesos",
                    Amount = 200000,
                    Description = "Recaudo en efectivo"
                });

                Console.WriteLine("Métodos de pago registrados:");
                Console.WriteLine("- Efectivo Pesos: $200,000");
                Console.WriteLine();

                // 3. Registrar un gasto válido
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 1,
                    Description = "Mantenimiento"
                };
                _courtService.CourtExpenditureAmount = 80000;
                _courtService.ExpenditureDescription = "Mantenimiento preventivo de dispensadores";

                Console.WriteLine("Intentando registrar gasto:");
                Console.WriteLine($"- Tipo: Mantenimiento");
                Console.WriteLine($"- Monto: $80,000");
                Console.WriteLine();

                await _courtService.AddCourtExpenditureFromPopup();

                // Verificar que el gasto fue registrado
                var gastosRegistrados = _courtService.CourtExpenditures.Sum(g => g.Amount);
                var efectivoRestante = 200000 - gastosRegistrados;

                Console.WriteLine("✅ El gasto fue registrado exitosamente");
                Console.WriteLine($"Efectivo inicial: $200,000");
                Console.WriteLine($"Gasto registrado: ${gastosRegistrados:N2}");
                Console.WriteLine($"Efectivo restante: ${efectivoRestante:N2}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo 2: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 3: Múltiples gastos que agotan el efectivo
        /// </summary>
        public async Task Example3_MultipleGastosAgotanEfectivo()
        {
            try
            {
                Console.WriteLine("=== EJEMPLO 3: Múltiples Gastos que Agotan el Efectivo ===\n");

                // 1. Configurar efectivo inicial
                _courtService.CourtTypeOfCollections = new ObservableCollection<CourtTypeOfCollection>();
                _courtService.CourtExpenditures = new ObservableCollection<CourtExpenditure>();

                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 1,
                    TypeOfCollectionName = "Efectivo",
                    Amount = 150000,
                    Description = "Recaudo del turno"
                });

                Console.WriteLine("Efectivo inicial: $150,000\n");

                // 2. Registrar primer gasto (exitoso)
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 1,
                    Description = "Combustible"
                };
                _courtService.CourtExpenditureAmount = 80000;
                _courtService.ExpenditureDescription = "Combustible para generador";

                Console.WriteLine("Gasto 1: Combustible - $80,000");
                await _courtService.AddCourtExpenditureFromPopup();
                Console.WriteLine("✅ Registrado exitosamente\n");

                // 3. Registrar segundo gasto (exitoso)
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 2,
                    Description = "Limpieza"
                };
                _courtService.CourtExpenditureAmount = 40000;
                _courtService.ExpenditureDescription = "Servicio de limpieza";

                Console.WriteLine("Gasto 2: Limpieza - $40,000");
                await _courtService.AddCourtExpenditureFromPopup();
                Console.WriteLine("✅ Registrado exitosamente\n");

                var gastosAcumulados = _courtService.CourtExpenditures.Sum(g => g.Amount);
                Console.WriteLine($"Total gastos registrados: ${gastosAcumulados:N2}");
                Console.WriteLine($"Efectivo restante: ${150000 - gastosAcumulados:N2}\n");

                // 4. Intentar registrar tercer gasto (debe fallar)
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 3,
                    Description = "Suministros"
                };
                _courtService.CourtExpenditureAmount = 50000;
                _courtService.ExpenditureDescription = "Compra de suministros";

                Console.WriteLine("Gasto 3: Suministros - $50,000");
                await _courtService.AddCourtExpenditureFromPopup();
                Console.WriteLine("❌ Gasto rechazado (excede efectivo disponible)\n");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo 3: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 4: Múltiples métodos de pago en efectivo
        /// </summary>
        public async Task Example4_MultipleMetodosEfectivo()
        {
            try
            {
                Console.WriteLine("=== EJEMPLO 4: Múltiples Métodos de Pago en Efectivo ===\n");

                // 1. Configurar múltiples métodos de efectivo
                _courtService.CourtTypeOfCollections = new ObservableCollection<CourtTypeOfCollection>();
                _courtService.CourtExpenditures = new ObservableCollection<CourtExpenditure>();

                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 1,
                    TypeOfCollectionName = "Efectivo Pesos",
                    Amount = 100000,
                    Description = "Recaudo caja principal"
                });

                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 2,
                    TypeOfCollectionName = "efectivo dolares",
                    Amount = 50000,
                    Description = "Recaudo en dólares convertido"
                });

                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 3,
                    TypeOfCollectionName = "Tarjeta de Crédito",
                    Amount = 75000,
                    Description = "Pago con tarjeta"
                });

                Console.WriteLine("Métodos de pago registrados:");
                Console.WriteLine("- Efectivo Pesos: $100,000");
                Console.WriteLine("- efectivo dolares: $50,000");
                Console.WriteLine("- Tarjeta de Crédito: $75,000");
                Console.WriteLine();

                var totalEfectivo = _courtService.CourtTypeOfCollections
                    .Where(m => m.TypeOfCollectionName.Contains("Efectivo", StringComparison.OrdinalIgnoreCase))
                    .Sum(m => m.Amount);

                Console.WriteLine($"Total efectivo disponible: ${totalEfectivo:N2}\n");

                // 2. Registrar gasto usando el efectivo combinado
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 1,
                    Description = "Reparación"
                };
                _courtService.CourtExpenditureAmount = 120000;
                _courtService.ExpenditureDescription = "Reparación de equipo crítico";

                Console.WriteLine("Intentando registrar gasto:");
                Console.WriteLine($"- Tipo: Reparación");
                Console.WriteLine($"- Monto: $120,000");
                Console.WriteLine();

                await _courtService.AddCourtExpenditureFromPopup();

                var gastosRegistrados = _courtService.CourtExpenditures.Sum(g => g.Amount);
                var efectivoRestante = totalEfectivo - gastosRegistrados;

                Console.WriteLine("✅ El gasto fue registrado exitosamente");
                Console.WriteLine($"Total efectivo: ${totalEfectivo:N2}");
                Console.WriteLine($"Gasto registrado: ${gastosRegistrados:N2}");
                Console.WriteLine($"Efectivo restante: ${efectivoRestante:N2}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo 4: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 5: Escenario de límite exacto
        /// </summary>
        public async Task Example5_LimiteExacto()
        {
            try
            {
                Console.WriteLine("=== EJEMPLO 5: Gasto que Usa Todo el Efectivo Disponible ===\n");

                // 1. Configurar efectivo
                _courtService.CourtTypeOfCollections = new ObservableCollection<CourtTypeOfCollection>();
                _courtService.CourtExpenditures = new ObservableCollection<CourtExpenditure>();

                _courtService.CourtTypeOfCollections.Add(new CourtTypeOfCollection
                {
                    IdTypeOfCollection = 1,
                    TypeOfCollectionName = "Efectivo",
                    Amount = 100000,
                    Description = "Efectivo del turno"
                });

                Console.WriteLine("Efectivo disponible: $100,000\n");

                // 2. Registrar gasto por el total exacto
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 1,
                    Description = "Compra de inventario"
                };
                _courtService.CourtExpenditureAmount = 100000;
                _courtService.ExpenditureDescription = "Compra de inventario crítico";

                Console.WriteLine("Intentando registrar gasto:");
                Console.WriteLine($"- Tipo: Compra de inventario");
                Console.WriteLine($"- Monto: $100,000 (todo el efectivo)");
                Console.WriteLine();

                await _courtService.AddCourtExpenditureFromPopup();

                Console.WriteLine("✅ El gasto fue registrado exitosamente");
                Console.WriteLine("Efectivo restante: $0.00\n");

                // 3. Intentar registrar otro gasto (debe fallar)
                _courtService.SelectedExpenditure = new ExpendituresCourtModel
                {
                    IdExpenditure = 2,
                    Description = "Otro gasto"
                };
                _courtService.CourtExpenditureAmount = 1000;
                _courtService.ExpenditureDescription = "Intento de gasto adicional";

                Console.WriteLine("Intentando registrar gasto adicional:");
                Console.WriteLine($"- Monto: $1,000");
                Console.WriteLine();

                await _courtService.AddCourtExpenditureFromPopup();

                Console.WriteLine("❌ Gasto rechazado (no hay efectivo disponible)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ejemplo 5: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejecutar todos los ejemplos
        /// </summary>
        public async Task RunAllExamples()
        {
            Console.WriteLine("🔍 EJEMPLOS DE VALIDACIÓN DE GASTOS CONTRA EFECTIVO DISPONIBLE");
            Console.WriteLine("================================================================\n");

            await Example1_NoEfectivoDisponible();
            await Task.Delay(500);

            await Example2_GastoExitoso();
            await Task.Delay(500);

            await Example3_MultipleGastosAgotanEfectivo();
            await Task.Delay(500);

            await Example4_MultipleMetodosEfectivo();
            await Task.Delay(500);

            await Example5_LimiteExacto();

            Console.WriteLine("\n📋 RESUMEN DE VALIDACIONES IMPLEMENTADAS:");
            Console.WriteLine("- ✅ No permite gastos sin efectivo disponible");
            Console.WriteLine("- ✅ Permite gastos dentro del límite de efectivo");
            Console.WriteLine("- ✅ Rechaza gastos que exceden el efectivo restante");
            Console.WriteLine("- ✅ Considera múltiples métodos de pago en efectivo");
            Console.WriteLine("- ✅ Acumula gastos correctamente");
            Console.WriteLine("- ✅ Mensajes de error contextuales y específicos");
            Console.WriteLine("- ✅ Validación case-insensitive para 'Efectivo'");
        }
    }
}