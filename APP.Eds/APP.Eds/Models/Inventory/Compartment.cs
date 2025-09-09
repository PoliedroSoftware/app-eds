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
        public string Product 
        { 
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductWithType));
                OnPropertyChanged(nameof(FuelTypeShort));
            }
        }
        private string _product = string.Empty;

        [JsonPropertyName("productType")]
        public string ProductType 
        { 
            get => _productType;
            set
            {
                _productType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductWithType));
                OnPropertyChanged(nameof(FuelTypeShort));
            }
        }
        private string _productType = string.Empty;

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
                    // Debug: Log para ver los datos que llegan
                    System.Diagnostics.Debug.WriteLine($"ProductWithType - Product: '{Product}', ProductType: '{ProductType}'");

                    // Si el producto está vacío, retornar valor por defecto
                    if (string.IsNullOrWhiteSpace(Product))
                        return "Sin producto";

                    // Normalizar nombres para comparación
                    var product = Product.Trim();
                    var productType = ProductType?.Trim() ?? "";

                    // Combinar ambos campos para análisis más completo
                    var combinedText = $"{product} {productType}".ToLowerInvariant();

                    // Si es ACPM, mostrar solo ACPM
                    if (combinedText.Contains("acpm") || combinedText.Contains("diésel") || combinedText.Contains("diesel"))
                    {
                        return "ACPM";
                    }

                    // Si es gasolina, determinar el tipo específico
                    if (combinedText.Contains("gasolina") || combinedText.Contains("gas") || combinedText.Contains("nafta"))
                    {
                        // Detectar tipo específico de gasolina
                        if (combinedText.Contains("corriente") || combinedText.Contains("regular") || combinedText.Contains("común"))
                        {
                            return "Gasolina Corriente";
                        }
                        else if (combinedText.Contains("extra") || combinedText.Contains("premium") || combinedText.Contains("plus"))
                        {
                            return "Gasolina Extra";
                        }
                        else if (combinedText.Contains("super") || combinedText.Contains("suprema") || combinedText.Contains("supreme"))
                        {
                            return "Gasolina Super";
                        }
                        else if (combinedText.Contains("95") || combinedText.Contains("octanos 95"))
                        {
                            return "Gasolina 95";
                        }
                        else if (combinedText.Contains("91") || combinedText.Contains("octanos 91"))
                        {
                            return "Gasolina 91";
                        }
                        
                        // Si el producto ya incluye información de tipo en el nombre
                        if (product.ToLowerInvariant() != "gasolina" && product.ToLowerInvariant().Contains("gasolina"))
                        {
                            return product; // Usar el nombre completo que viene del servidor
                        }
                        
                        // Si ProductType tiene información adicional útil
                        if (!string.IsNullOrWhiteSpace(productType) && 
                            !productType.ToLowerInvariant().Contains("combustible") &&
                            !productType.ToLowerInvariant().Contains("liquid"))
                        {
                            return $"Gasolina {productType}";
                        }
                        
                        // Por defecto, si solo dice "Gasolina", marcar como tipo desconocido
                        return "Gasolina";
                    }

                    // Para otros productos, mostrar el nombre tal como viene
                    return product;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en ProductWithType: {ex.Message}");
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
