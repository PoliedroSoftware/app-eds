using APP.Eds.Services.Court;
using APP.Eds.Components.PopUp;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.Components.PopUp
{
    public partial class AddDocuemt : Popup, INotifyPropertyChanged
    {
        private readonly CourtService _courtService;
        private string _selectedFileName;
        private string _fileBase64;

        public string SelectedFileName 
        { 
            get => _selectedFileName;
            private set
            {
                _selectedFileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFileSelected));
            }
        }

        public string FileBase64 
        { 
            get => _fileBase64;
            private set
            {
                _fileBase64 = value;
                OnPropertyChanged();
            }
        }

        public bool IsFileSelected => !string.IsNullOrEmpty(SelectedFileName);

        public AddDocuemt(CourtService courtService)
        {
            InitializeComponent();
            _courtService = courtService;
            BindingContext = this;
        }

        private void OnCloseTapped(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddDocument popup was already disposed during close: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing AddDocument popup: {ex.Message}");
            }
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.pdf", "public.jpeg", "public.png", "com.microsoft.word.doc" } },
                        { DevicePlatform.Android, new[] { "application/pdf", "image/jpeg", "image/png", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
                        { DevicePlatform.WinUI, new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" } },
                        { DevicePlatform.Tizen, new[] { "*/*" } },
                        { DevicePlatform.macOS, new[] { "pdf", "jpg", "jpeg", "png", "doc", "docx", "txt" } },
                    });

                var options = new PickOptions()
                {
                    PickerTitle = "Seleccione un documento",
                    FileTypes = customFileType,
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    // Check file size (5 MB limit)
                    var fileSize = new FileInfo(result.FullPath).Length;
                    if (fileSize > 5 * 1024 * 1024)
                    {
                        await CustomAlert.ShowWarningAsync(
                            $"El archivo seleccionado ({fileSize / 1024.0 / 1024.0:F1} MB) excede el límite permitido de 5 MB.\n\nPor favor, seleccione un archivo más pequeño o comprima el actual.", 
                            "Archivo Muy Grande");
                        return;
                    }

                    // Check file type
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
                // Disable button to prevent multiple submissions
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Text = "Agregando...";
                }

                if (!string.IsNullOrEmpty(FileBase64))
                {
                    List<string> filesBase64 = new List<string> { FileBase64 };
                    List<string> nombresDocuments = new List<string> { SelectedFileName };

                    _courtService.AddDocumentsFromPopup(filesBase64, nombresDocuments);
                    
                    await CustomAlert.ShowSuccessAsync($"El documento '{SelectedFileName}' ha sido agregado exitosamente al cierre de turno", "Documento Agregado");
                    
                    await CloseAsync();
                }
                else
                {
                    await CustomAlert.ShowWarningAsync("Debe seleccionar un archivo antes de continuar", "Archivo Requerido");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al guardar el documento:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                // Re-enable button
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = "Agregar";
                }
            }
        }

        private async Task CloseAsync()
        {
            try
            {
                await Task.Delay(100); // Small delay for smooth animation
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddDocument popup was already disposed during close: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing document popup: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}