using System.Collections.Generic;
using System.ComponentModel;
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
        public string TankNumber { get; set; }

        [JsonPropertyName("tankCapacity")]
        public int TankCapacity { get; set; }

        [JsonPropertyName("compartments")]
        public List<Compartment> Compartments { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
