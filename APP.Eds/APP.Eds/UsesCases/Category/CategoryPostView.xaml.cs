using APP.Eds.Services.Category;
using APP.Eds.Components.PopUp;
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
            string newText = Regex.Replace(e.NewTextValue, @"[^a-zA-Z·ÈÌÛ˙¡…Õ”⁄¸‹Ò—\s-]", "");

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

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_categoryService.Description))
            {
                await CustomAlert.ShowErrorAsync("La descripciÛn de la categorÌa es obligatoria para el registro", "DescripciÛn Requerida");
                return;
            }

            if (_categoryService.Description.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("La descripciÛn debe tener al menos 3 caracteres para ser v·lida", "DescripciÛn Muy Corta");
                return;
            }

            if (_categoryService.Description.Length > 50)
            {
                await CustomAlert.ShowErrorAsync("La descripciÛn no puede exceder 50 caracteres", "DescripciÛn Muy Larga");
                return;
            }

            // Clean up and validate format
            string originalDescription = _categoryService.Description;
            _categoryService.Description = _categoryService.Description.Trim();
            
            if (originalDescription != _categoryService.Description)
            {
                await CustomAlert.ShowInfoAsync("Los espacios extra han sido removidos autom·ticamente", "DescripciÛn Limpiada");
            }

            // Check for duplicates or invalid patterns
            if (_categoryService.Description.Contains("  "))
            {
                await CustomAlert.ShowWarningAsync("Se detectaron espacios dobles en la descripciÛn. Se corregir·n autom·ticamente.", "Espacios Detectados");
                _categoryService.Description = Regex.Replace(_categoryService.Description, @"\s+", " ");
            }

            LoadingOverlay.ShowLoading();
            await _categoryService.SaveCategoryDataAsync();
            
            await CustomAlert.ShowSuccessAsync($"La categorÌa '{_categoryService.Description}' ha sido creada exitosamente", "CategorÌa Creada");

            // Clear form field after successful submission
            Description = string.Empty;
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar la categorÌa:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

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