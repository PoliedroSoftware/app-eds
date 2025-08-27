using APP.Eds.Services.Court;
using APP.Eds.Components.PopUp;
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
                    // Check file size (5 MB limit)
                    var fileSize = new FileInfo(result.FullPath).Length;
                    if (fileSize > 5 * 1024 * 1024)
                    {
                        await CustomAlert.ShowWarningAsync(
                            $"El archivo seleccionado ({fileSize / 1024 / 1024:F1} MB) excede el límite permitido de 5 MB.\n\nPor favor, seleccione un archivo más pequeño o comprima el actual.", 
                            "Archivo Muy Grande");
                        return;
                    }

                    // Check file type (optional - add allowed extensions)
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" };
                    var fileExtension = Path.GetExtension(result.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        await CustomAlert.ShowWarningAsync(
                            $"El tipo de archivo '{fileExtension}' no está permitido.\n\nTipos permitidos: {string.Join(", ", allowedExtensions)}", 
                            "Tipo de Archivo No Permitido");
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
                    
                    await CustomAlert.ShowSuccessAsync($"Archivo '{SelectedFileName}' seleccionado correctamente", "Archivo Cargado");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudo seleccionar el archivo:\n\n{ex.Message}", "Error de Archivo");
            }
        }

        private async void Add_Dispenser(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(FileBase64))
                {
                    List<string> filesBase64 = new List<string> { FileBase64 };
                    List<string> nombresDocuments = new List<string> { SelectedFileName };

                    _courtService.AddDocumentsFromPopup(filesBase64, nombresDocuments);
                    
                    await CustomAlert.ShowSuccessAsync($"El documento '{SelectedFileName}' ha sido agregado exitosamente al cierre de turno", "Documento Agregado");
                }
                else
                {
                    await CustomAlert.ShowWarningAsync("Debe seleccionar un archivo antes de continuar", "Archivo Requerido");
                    return;
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al guardar el documento:\n\n{ex.Message}", "Error del Sistema");
                return;
            }
            
            await CloseAsync();
        }

        private async Task CloseAsync()
        {
            try
            {
                await Task.Delay(100); // Small delay for smooth animation
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing document popup: {ex.Message}");
            }
        }
    }
}