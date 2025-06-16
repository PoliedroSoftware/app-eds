using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Inventory
{
    public class Business : INotifyPropertyChanged
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

        [JsonPropertyName("idBusiness")]
        public int IdBusiness { get; set; }

        [JsonPropertyName("business")]
        public string BusinessName { get; set; }

        [JsonPropertyName("eds")]
        public List<Eds> Eds { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
