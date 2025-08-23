using APP.Eds.Services.Category;
using System.Text.RegularExpressions;

namespace APP.Eds.UsesCases.Category;

public partial class CategoryPostView : ContentPage
{
    private CategoryService _categoryService;
    
    public CategoryPostView()
	{
		InitializeComponent();
        _categoryService = new CategoryService();
        BindingContext = _categoryService;
    }

    private void CategoryEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Only allow letters, spaces, and some common characters for category names
            string newText = Regex.Replace(e.NewTextValue, @"[^a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s-]", "");

            if (newText != e.NewTextValue)
            {
                entry.Text = newText;
                entry.CursorPosition = newText.Length;
            }
        }
    }
    
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Enviando...";
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_categoryService.Description))
            {
                await DisplayAlert("Error", "Por favor ingrese la descripciÛn de la categorÌa", "OK");
                return;
            }

            if (_categoryService.Description.Length < 3)
            {
                await DisplayAlert("Error", "La descripciÛn de la categorÌa debe tener al menos 3 caracteres", "OK");
                return;
            }

            if (_categoryService.Description.Length > 50)
            {
                await DisplayAlert("Error", "La descripciÛn de la categorÌa no puede exceder 50 caracteres", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _categoryService.SaveCategoryDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar la categorÌa: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form field after successful submission
            Description = string.Empty;

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = _categoryService.SendData; // Restore original text from translations
            }
        }
    }

    public string Description
    {
        get => _categoryService.Description;
        set
        {
            _categoryService.Description = value;
            OnPropertyChanged();
        }
    }

}