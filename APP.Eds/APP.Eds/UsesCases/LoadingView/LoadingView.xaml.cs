using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.UsesCases.LoadingView
{
    public partial class LoadingView : ContentView
    {
        public LoadingView()
        {
            InitializeComponent();
        }


        public void ShowLoading()
        {
            LoadingOverlay.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoadingLabel.IsVisible = true;
        }

        public void HideLoading()
        {
            LoadingOverlay.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoadingLabel.IsVisible = false;
        }
    }
}
