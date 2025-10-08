using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace APP.Eds.Models.Inventory
{
    public class Compartment : INotifyPropertyChanged
    {

        [JsonPropertyName("idProductType")] 
        public int? IdProductType
        {
            get => _idProductType;
            set
            {
                _idProductType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductWithType));
                OnPropertyChanged(nameof(FuelTypeShort));
            }
        }
        private int? _idProductType;

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

        private static readonly Dictionary<int, (int TypeId, string TypeDesc, string Family)> ProductCatalog =
            new()
            {
        { 21, (1, "Corriente", "Gasolina") },
        { 23, (9, "Extra",     "Gasolina") },
        { 22, (2, "Diésel",    "ACPM")     },
            };

        [JsonIgnore]
        public string ProductWithType
        {
            get
            {
                var family = GetFamilyName(Product, IdProductType);

                if (IdProductType.HasValue)
                {
                    var desc = GetFuelTypeDescription(IdProductType.Value, ProductType);
                    return string.IsNullOrWhiteSpace(desc)
                        ? family
                        : $"{family} {desc}";
                }

                if (ProductCatalog.TryGetValue(IdProduct, out var info))
                {
                    return $"{info.Family} {info.TypeDesc}";
                }

                var pt = ProductType?.Trim() ?? string.Empty;
                var combined = $"{family} {pt}".ToLowerInvariant();

                if (combined.Contains("acpm") || combined.Contains("diésel") || combined.Contains("diesel")) return "ACPM";
                if (combined.Contains("corriente") || combined.Contains("regular") || combined.Contains("común")) return "Gasolina Corriente";
                if (combined.Contains("extra") || combined.Contains("premium") || combined.Contains("plus")) return "Gasolina Extra";
                if (combined.Contains("super")) return "Gasolina Super";
                if (combined.Contains("gasolina")) return "Gasolina";

                return family;
            }
        }

        private static string GetFamilyName(string? product, int? idProductType)
        {
            if (idProductType is 1 or 9) return "Gasolina"; 
            if (idProductType is 2) return "ACPM";          
            var p = product?.ToLowerInvariant() ?? "";
            if (p.Contains("acpm") || p.Contains("diésel") || p.Contains("diesel")) return "ACPM";
            if (p.Contains("gas")) return "Gasolina";
            return string.IsNullOrWhiteSpace(product) ? "Sin producto" : product!.Trim();
        }

        private static string GetFuelTypeDescription(int id, string? productTypeFromApi)
        {
            return id switch
            {
                1 => "Corriente",
                9 => "Extra",
                2 => "Diésel", 
                _ => string.IsNullOrWhiteSpace(productTypeFromApi) ? "" : productTypeFromApi.Trim()
            };
        }


        [JsonIgnore]
        public string ProductTypeDisplay
        {
            get
            {
                if (IdProductType.HasValue)
                {
                    var family = GetFamilyName(Product, IdProductType);
                    var desc = GetFuelTypeDescription(IdProductType.Value, ProductType);

                    if (string.Equals(family, "Gasolina", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(desc))
                        return $"Combustible {desc}";

                    if (string.Equals(family, "ACPM", StringComparison.OrdinalIgnoreCase))
                        return "Diésel"; 

                    return desc ?? string.Empty;
                }

                if (ProductCatalog.TryGetValue(IdProduct, out var info))
                {
                    if (string.Equals(info.Family, "Gasolina", StringComparison.OrdinalIgnoreCase))
                        return $"Combustible {info.TypeDesc}";

                    if (string.Equals(info.Family, "ACPM", StringComparison.OrdinalIgnoreCase))
                        return "Diésel";

                    return info.TypeDesc;
                }

                return string.IsNullOrWhiteSpace(ProductType) ? string.Empty : ProductType.Trim();
            }
        }

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
