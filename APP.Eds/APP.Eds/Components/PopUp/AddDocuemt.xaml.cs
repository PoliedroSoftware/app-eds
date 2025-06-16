using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp
{
    public partial class AddDocuemt : Popup
    {
        private readonly CourtService _courtService;

        public string SelectedFileName { get; private set; }
        public string FileBase64 { get; private set; }
        public bool IsFileSelected => !string.IsNullOrEmpty(SelectedFileName);

        public AddDocuemt(CourtService courtService)
        {
            InitializeComponent();
            _courtService = courtService;
            BindingContext = this;

        }

        private void OnCloseTapped(object sender, EventArgs e)
        {
            Close();
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    if (result.FileName.Length > 5 * 1024 * 1024)
                    {
                        await Application.Current.MainPage.DisplayAlert("Advertencia", "El archivo es demasiado grande. El tamaño máximo permitido es 5 MB.", "OK");
                        return;
                    }

                    SelectedFileName = result.FileName;

                    FileBase64 = await Task.Run(async () =>
                    {
                        using var stream = await result.OpenReadAsync();
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        return Convert.ToBase64String(memoryStream.ToArray());
                    });

                    OnPropertyChanged(nameof(SelectedFileName));
                    OnPropertyChanged(nameof(IsFileSelected));                    
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo seleccionar el archivo: {ex.Message}", "OK");
            }
        }

        private async void Add_Dispenser(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(FileBase64))
                {
                    // Crear listas para los documentos y nombres
                    List<string> filesBase64 = new List<string> { FileBase64 };
                    List<string> nombresDocuments = new List<string> { SelectedFileName };

                    // Llamar al método que maneja múltiples documentos
                     _courtService.AddDocumentsFromPopup(filesBase64, nombresDocuments);
                    // close
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Advertencia", "Por favor, selecciona un archivo.", "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al guardar el documento: {ex.Message}", "Ok");
            }
            Close();
        }
    }
}