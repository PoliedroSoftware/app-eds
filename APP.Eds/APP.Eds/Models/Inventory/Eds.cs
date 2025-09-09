using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Inventory
{
    public class Eds : INotifyPropertyChanged
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

        [JsonPropertyName("idEds")]
        public int IdEds { get; set; }

        [JsonPropertyName("eds")]
        public string EdsName { get; set; } = string.Empty;

        [JsonPropertyName("tanks")]
        public List<Tank> Tanks { get; set; } = new List<Tank>();

        /// <summary>
        /// Stock total de la EDS (suma de todos los compartimentos de todos los tanques)
        /// </summary>
        [JsonIgnore]
        public double TotalStock
        {
            get
            {
                try
                {
                    return Tanks?.Sum(tank => tank.CurrentStock) ?? 0;
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
        public string TotalStockText
        {
            get
            {
                try
                {
                    return $"{TotalStock:N0}G";
                }
                catch
                {
                    return "0G";
                }
            }
        }

        /// <summary>
        /// Número total de tanques en esta EDS
        /// </summary>
        [JsonIgnore]
        public int TotalTanks => Tanks?.Count ?? 0;

        /// <summary>
        /// Número total de compartimentos en esta EDS
        /// </summary>
        [JsonIgnore]
        public int TotalCompartments
        {
            get
            {
                try
                {
                    return Tanks?.Sum(t => t.Compartments?.Count ?? 0) ?? 0;
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
