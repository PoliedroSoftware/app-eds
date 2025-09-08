using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Inventory
{
    public class Compartment : INotifyPropertyChanged
    {
        private bool isExpanded;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("idCompartment")]
        public int IdCompartment { get; set; }

        [JsonPropertyName("compartment")]
        public int CompartmentNumber { get; set; }

        [JsonPropertyName("idProduct")]
        public int IdProduct { get; set; }

        [JsonPropertyName("product")]
        public string Product { get; set; } = string.Empty;

        [JsonPropertyName("productType")]
        public string ProductType { get; set; } = string.Empty;

        [JsonPropertyName("stock")]
        public double Stock { get; set; }

        /// <summary>
        /// Nombre del producto con especificación del tipo de combustible
        /// Para gasolina: muestra "Gasolina Corriente", "Gasolina Extra", etc.
        /// Para ACPM: muestra solo "ACPM"
        /// </summary>
        [JsonIgnore]
        public string ProductWithType
        {
            get
            {
                try
                {
                    // Si el producto está vacío, retornar valor por defecto
                    if (string.IsNullOrWhiteSpace(Product))
                        return "Sin producto";

                    // Normalizar nombres para comparación
                    var product = Product.Trim();
                    var productType = ProductType?.Trim() ?? "";

                    // Si es ACPM, mostrar solo ACPM
                    if (product.ToLowerInvariant().Contains("acpm") || 
                        productType.ToLowerInvariant().Contains("acpm"))
                    {
                        return "ACPM";
                    }

                    // Si es gasolina, agregar el tipo específico
                    if (product.ToLowerInvariant().Contains("gasolina"))
                    {
                        // Extraer tipo específico del ProductType si está disponible
                        if (!string.IsNullOrWhiteSpace(productType))
                        {
                            var typeLower = productType.ToLowerInvariant();
                            
                            if (typeLower.Contains("corriente"))
                                return "Gasolina Corriente";
                            else if (typeLower.Contains("extra") || typeLower.Contains("premium"))
                                return "Gasolina Extra";
                            else if (typeLower.Contains("super") || typeLower.Contains("suprema"))
                                return "Gasolina Super";
                            else
                            {
                                // Si el productType tiene información útil, usarla
                                return $"Gasolina {productType}";
                            }
                        }
                        
                        // Si el Product ya incluye el tipo, usarlo directamente
                        if (product.Length > "Gasolina".Length)
                        {
                            return product;
                        }
                        
                        // Por defecto, si solo dice "Gasolina", asumir Corriente
                        return "Gasolina Corriente";
                    }

                    // Para otros productos, mostrar el nombre tal como viene
                    return product;
                }
                catch
                {
                    return Product ?? "Sin producto";
                }
            }
        }

        /// <summary>
        /// Descripción corta del tipo de combustible para mostrar en UI compacta
        /// </summary>
        [JsonIgnore]
        public string FuelTypeShort
        {
            get
            {
                try
                {
                    var productWithType = ProductWithType.ToLowerInvariant();
                    
                    if (productWithType.Contains("acpm"))
                        return "ACPM";
                    else if (productWithType.Contains("corriente"))
                        return "G. Corriente";
                    else if (productWithType.Contains("extra"))
                        return "G. Extra";
                    else if (productWithType.Contains("super"))
                        return "G. Super";
                    else if (productWithType.Contains("gasolina"))
                        return "Gasolina";
                    else
                        return ProductWithType.Length > 10 ? ProductWithType.Substring(0, 10) + "..." : ProductWithType;
                }
                catch
                {
                    return "N/A";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
