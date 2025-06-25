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

            string newText = Regex.Replace(e.NewTextValue, @"[^a-zA-Z\s]", "");

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
            LoadingOverlay.ShowLoading();
            await _categoryService.SaveCategoryDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Description = string.Empty;
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