using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Inventory
{
    public class Tank : INotifyPropertyChanged
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

        [JsonPropertyName("idTank")]
        public int IdTank { get; set; }

        [JsonPropertyName("tank")]
        public string TankNumber { get; set; } = string.Empty;

        [JsonPropertyName("tankCapacity")]
        public int TankCapacity { get; set; }

        [JsonPropertyName("compartments")]
        public List<Compartment> Compartments { get; set; } = new List<Compartment>();

        /// <summary>
        /// Stock total actual del tanque (suma de todos los compartimentos)
        /// </summary>
        [JsonIgnore]
        public double CurrentStock
        {
            get
            {
                try
                {
                    return Compartments?.Sum(c => c.Stock) ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Stock total formateado para mostrar en la UI
        /// </summary>
        [JsonIgnore]
        public string CurrentStockText
        {
            get
            {
                try
                {
                    return $"{CurrentStock:N0}G";
                }
                catch
                {
                    return "0G";
                }
            }
        }

        /// <summary>
        /// Porcentaje de llenado del tanque basado en el stock actual vs capacidad máxima
        /// </summary>
        [JsonIgnore]
        public double FillPercentage
        {
            get
            {
                try
                {
                    if (TankCapacity <= 0) return 0;
                    return (CurrentStock / TankCapacity) * 100;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        /// <summary>
        /// Método público para notificar cambios en propiedades (usado por ViewModel)
        /// </summary>
        public void NotifyPropertyChanged(string propertyName) =>
            OnPropertyChanged(propertyName);
    }
}
