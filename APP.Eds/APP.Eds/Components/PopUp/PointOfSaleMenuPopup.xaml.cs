using APP.Eds.UsesCases.PointOfSale;
using APP.Eds.UsesCases.Billing;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Views.Popups
{
    public partial class PointOfSaleMenuPopup : Popup
    {
 public PointOfSaleMenuPopup()
        {
            InitializeComponent();
  _ = AnimateEntry();
        }

        private async Task AnimateEntry()
  {
            await Task.Delay(50);
            
            Frame.Scale = 0.8;
   Frame.Opacity = 0;
   Frame.TranslationY = 0;
     Frame.TranslationX = 0;
            Frame.Rotation = 0;
            
            await Task.WhenAll(
         Frame.ScaleTo(1, 400, Easing.SpringOut),
                Frame.FadeTo(1, 300, Easing.CubicOut)
            );
        }

      private async void OnCloseTapped(object sender, EventArgs e)
        {
   try
            {
          await AnimateExit();
                
       try
      {
      Close();
     }
          catch (ObjectDisposedException ex)
   {
    System.Diagnostics.Debug.WriteLine($"PointOfSaleMenuPopup was already disposed during close: {ex.Message}");
       }
}
            catch (Exception ex)
       {
          System.Diagnostics.Debug.WriteLine($"Error in PointOfSaleMenuPopup OnCloseTapped: {ex.Message}");
}
        }

 private async void OnBillingTapped(object sender, EventArgs e)
 {
          try
        {
                try
                {
#if ANDROID || IOS
    HapticFeedback.Perform(HapticFeedbackType.Click);
#endif
     }
          catch { }
     
         if (sender is Border border)
                {
        _ = Task.Run(async () =>
          {
       await MainThread.InvokeOnMainThreadAsync(async () =>
            {
      await Task.WhenAll(
        border.ScaleTo(0.95, 80, Easing.CubicOut),
   border.FadeTo(0.7, 80)
      );
      await Task.WhenAll(
   border.ScaleTo(1.02, 100, Easing.SpringOut),
       border.FadeTo(1, 100)
     );
   await border.ScaleTo(1, 80, Easing.CubicOut);
  });
 });
       }

      await Task.Delay(200);
      await AnimateExit();
         
     try
      {
          Close();
     }
          catch (ObjectDisposedException ex)
              {
            System.Diagnostics.Debug.WriteLine($"PointOfSaleMenuPopup was already disposed: {ex.Message}");
             }

       if (Application.Current?.MainPage is NavigationPage navPage)
  {
          await navPage.PushAsync(new PointOfSaleView());
       }
      }
   catch (Exception ex)
   {
           System.Diagnostics.Debug.WriteLine($"Error in PointOfSaleMenuPopup OnBillingTapped: {ex.Message}");
            }
        }

        private async void OnInvoiceHistoryTapped(object sender, EventArgs e)
        {
            try
    {
        try
      {
#if ANDROID || IOS
         HapticFeedback.Perform(HapticFeedbackType.Click);
#endif
   }
          catch { }
 
                if (sender is Border border)
       {
       _ = Task.Run(async () =>
             {
                await MainThread.InvokeOnMainThreadAsync(async () =>
   {
        await Task.WhenAll(
          border.ScaleTo(0.95, 80, Easing.CubicOut),
          border.FadeTo(0.7, 80)
     );
    await Task.WhenAll(
         border.ScaleTo(1.02, 100, Easing.SpringOut),
     border.FadeTo(1, 100)
     );
         await border.ScaleTo(1, 80, Easing.CubicOut);
        });
        });
 }

    await Task.Delay(200);
           await AnimateExit();
                
    try
        {
       Close();
   }
catch (ObjectDisposedException ex)
      {
        System.Diagnostics.Debug.WriteLine($"PointOfSaleMenuPopup was already disposed: {ex.Message}");
}

                if (Application.Current?.MainPage is NavigationPage navPage)
   {
             await navPage.PushAsync(new InvoiceHistoryView());
        }
      }
            catch (Exception ex)
    {
    System.Diagnostics.Debug.WriteLine($"Error in PointOfSaleMenuPopup OnInvoiceHistoryTapped: {ex.Message}");
    }
        }

        private async Task AnimateExit()
    {
         await Task.WhenAll(
      Frame.ScaleTo(0.85, 200, Easing.CubicIn),
        Frame.FadeTo(0, 150, Easing.CubicIn)
     );
        }
    }
}
