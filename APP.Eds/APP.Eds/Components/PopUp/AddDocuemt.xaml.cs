using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace APP.Eds.Components.PopUp
{
    public partial class AddDocuemt : Popup, INotifyPropertyChanged
    {
        private readonly CourtService _courtService;

        // Contexto opcional del corte
        private readonly int? _courtId;
        private readonly string _courtIdFieldName;

        // Cache de la última selección durante la sesión (persiste aunque cierres el popup)
        private static readonly ObservableCollection<SelectedFileItem> _lastSelection = new();

        // Modelo de archivo seleccionado (multi-archivo)
        public class SelectedFileItem
        {
            public FileResult? File { get; init; } // <- nullable para poder cachear sin necesidad del FileResult
            public string Name => File?.FileName ?? _name ?? string.Empty;
            public long SizeBytes { get; init; }
            public string Base64 { get; init; } = string.Empty;

            // Para conservar nombre cuando File==null (desde cache)
            public string? _name { get; init; }

            public string SizeDisplay => FormatSize(SizeBytes);

            public static string FormatSize(long size)
            {
                if (size < 1024) return $"{size} B";
                var kb = size / 1024d;
                if (kb < 1024) return $"{kb:0.#} KB";
                var mb = kb / 1024d;
                return $"{mb:0.##} MB";
            }
        }

        public ObservableCollection<SelectedFileItem> SelectedFiles { get; } = new();

        // Compat
        private string _selectedFileName;
        private string _fileBase64;

        public string SelectedFileName
        {
            get => _selectedFileName;
            private set { _selectedFileName = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFileSelected)); }
        }

        public string FileBase64
        {
            get => _fileBase64;
            private set { _fileBase64 = value; OnPropertyChanged(); }
        }

        public bool HasFiles => SelectedFiles.Count > 0;
        public bool IsFileSelected => HasFiles; // compat
        public string TotalSizeDisplay => SelectedFiles.Any()
            ? SelectedFileItem.FormatSize(SelectedFiles.Sum(f => f.SizeBytes))
            : "0 B";

        public string SummaryText => $"{SelectedFiles.Count} archivo(s) seleccionados";

        // Firma compatible hacia atrás
        public AddDocuemt(CourtService courtService, int? courtId = null, string courtIdFieldName = "courtId")
        {
            InitializeComponent();
            _courtService = courtService;
            _courtId = courtId;
            _courtIdFieldName = courtIdFieldName;

            BindingContext = this;

            // Restaurar selección previa si el popup se reabre
            if (_lastSelection.Any())
            {
                foreach (var item in _lastSelection)
                    SelectedFiles.Add(item);

                // Refrescar resumen
                OnPropertyChanged(nameof(HasFiles));
                OnPropertyChanged(nameof(TotalSizeDisplay));
                OnPropertyChanged(nameof(SummaryText));

                // Mantener compat (último archivo)
                var last = SelectedFiles.LastOrDefault();
                if (last is not null)
                {
                    SelectedFileName = last.Name;
                    FileBase64 = last.Base64;
                }
            }
        }

        private void PersistSelectionToCache()
        {
            _lastSelection.Clear();
            foreach (var f in SelectedFiles)
            {
                _lastSelection.Add(new SelectedFileItem
                {
                    File = null,            // no es necesario para re-enviar, ya tenemos Base64
                    _name = f.Name,
                    SizeBytes = f.SizeBytes,
                    Base64 = f.Base64
                });
            }
        }

        private void OnCloseTapped(object sender, EventArgs e)
        {
            try
            {
                // Guardar selección para que persista si el usuario cierra con la X
                PersistSelectionToCache();
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddDocument popup disposed: {ex.Message}");
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
                        { DevicePlatform.Android, new[] { "application/pdf", "image/jpeg", "image/png", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain" } },
                        { DevicePlatform.WinUI, new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" } },
                        { DevicePlatform.Tizen, new[] { "*/*" } },
                        { DevicePlatform.macOS, new[] { "pdf", "jpg", "jpeg", "png", "doc", "docx", "txt" } },
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Seleccione uno o más documentos",
                    FileTypes = customFileType
                };

                var results = await FilePicker.Default.PickMultipleAsync(options);
                if (results == null) return;

                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" };
                const long maxPerFile = 5L * 1024 * 1024; // 5 MB

                int added = 0;
                foreach (var result in results)
                {
                    var ext = Path.GetExtension(result.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        await CustomAlert.ShowWarningAsync(
                            $"El tipo de archivo '{ext}' no está permitido.\n\nTipos permitidos: {string.Join(", ", allowedExtensions)}",
                            "Tipo de Archivo No Permitido");
                        continue;
                    }

                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    var fileSize = bytes.LongLength;

                    if (fileSize > maxPerFile)
                    {
                        await CustomAlert.ShowWarningAsync(
                            $"El archivo '{result.FileName}' ({fileSize / 1024d / 1024d:0.##} MB) excede el límite de 5 MB.",
                            "Archivo Muy Grande");
                        continue;
                    }

                    if (SelectedFiles.Any(f => string.Equals(f.Name, result.FileName, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    SelectedFiles.Add(new SelectedFileItem
                    {
                        File = result,
                        _name = result.FileName,
                        SizeBytes = fileSize,
                        Base64 = Convert.ToBase64String(bytes)
                    });
                    added++;
                }

                if (added > 0)
                {
                    var last = SelectedFiles.Last();
                    SelectedFileName = last.Name;
                    FileBase64 = last.Base64;

                    OnPropertyChanged(nameof(HasFiles));
                    OnPropertyChanged(nameof(TotalSizeDisplay));
                    OnPropertyChanged(nameof(SummaryText));

                    // Persistir inmediatamente para que, si cierra, la selección permanezca
                    PersistSelectionToCache();
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudo seleccionar archivos:\n\n{ex.Message}", "Error de Archivo");
            }
        }

        private async void OnRemoveFileClicked(object sender, EventArgs e)
        {
            try
            {
                if ((sender as Button)?.CommandParameter is SelectedFileItem item)
                {
                    SelectedFiles.Remove(item);

                    OnPropertyChanged(nameof(HasFiles));
                    OnPropertyChanged(nameof(TotalSizeDisplay));
                    OnPropertyChanged(nameof(SummaryText));

                    // Actualizar cache al quitar
                    PersistSelectionToCache();
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudo quitar el archivo:\n\n{ex.Message}", "Error");
            }
        }

        // Enviar lote
        private async void Add_Dispenser(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Text = "Agregando...";
                }

                if (!SelectedFiles.Any())
                {
                    await CustomAlert.ShowWarningAsync("Debe seleccionar al menos un archivo antes de continuar", "Archivo Requerido");
                    return;
                }

                var filesBase64 = SelectedFiles.Select(f => f.Base64).ToList();
                var nombresDocuments = SelectedFiles.Select(f => f.Name).ToList();

                _courtService.AddDocumentsFromPopup(filesBase64, nombresDocuments);

                var uploadedCount = filesBase64.Count;

                try { _courtService.LastUploadedDocumentsCount = uploadedCount; } catch { }

                await CustomAlert.ShowSuccessAsync(
                    $"{uploadedCount} documento(s) agregados exitosamente al cierre de turno",
                    "Documentos Agregados");

                // Envío completado: limpiar selección y cache
                SelectedFiles.Clear();
                _lastSelection.Clear();
                OnPropertyChanged(nameof(HasFiles));
                OnPropertyChanged(nameof(TotalSizeDisplay));
                OnPropertyChanged(nameof(SummaryText));

                Close(uploadedCount);
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al guardar los documentos:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
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
                await Task.Delay(100);
                // Al cerrar manualmente (sin enviar) se mantiene _lastSelection
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddDocument popup disposed during close: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing document popup: {ex.Message}");
            }
        }

        // Nuevo: eliminar todos los archivos seleccionados
        private async void OnClearAllFilesClicked(object sender, EventArgs e)
        {
            try
            {
                if (!SelectedFiles.Any())
                    return;

                // Confirmación simple (puedes cambiar a tu CustomAlert si tienes confirmación allí)
                var confirm = await Application.Current.MainPage.DisplayAlert(
                    "Eliminar todo",
                    "¿Desea quitar todos los archivos seleccionados?",
                    "Sí", "No");

                if (!confirm) return;

                SelectedFiles.Clear();
                // limpiar cache persistente si se está usando
                try { _lastSelection.Clear(); } catch { }

                // limpiar compat
                SelectedFileName = string.Empty;
                FileBase64 = string.Empty;

                OnPropertyChanged(nameof(HasFiles));
                OnPropertyChanged(nameof(TotalSizeDisplay));
                OnPropertyChanged(nameof(SummaryText));
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudieron quitar los archivos:\n\n{ex.Message}", "Error");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}