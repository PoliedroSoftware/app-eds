using System.ComponentModel;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Compartiment
{
    public class CompartimentResponse : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        [JsonPropertyName("idCompartiment")]
        public int IdCompartment { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("nominal")]
        public double Nominal { get; set; }

        [JsonPropertyName("operative")]
        public double Operative { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("idTank")]
        public int IdTank { get; set; }

        [JsonPropertyName("idProduct")]
        public int IdProduct { get; set; }

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set
            {
                if (_productName == value) return; _productName = value;
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(DisplayCompartiment));
            }
        }

        public string DisplayCompartiment =>
            $"Compartimento: {Number}\n" +
            $"Capacidad nominal: {Nominal}\n" +
            $"Capacidad operativa: {Operative}\n" +
            $"Altura: {Height}\n" +
            $"Producto: {(string.IsNullOrWhiteSpace(ProductName) ? "-" : ProductName)}\n" +
            "-------------------------------------------------";
    }
}
