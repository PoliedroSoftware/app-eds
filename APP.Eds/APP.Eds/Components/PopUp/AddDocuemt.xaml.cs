using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.Components.PopUp
{
    public partial class AddDocuemt : Popup, INotifyPropertyChanged
    {
        private readonly CourtService _courtService;

        // ======== NUEVO: caché estática para conservar la selección entre aperturas ========
        private static readonly ObservableCollection<SelectedFileItem> SelectionCache = new();

        // Modelo de archivo seleccionado
        public class SelectedFileItem
        {
            public string Name { get; set; }
            public long Size { get; set; }
            public string SizeText => $"{Size / 1024.0 / 1024.0:0.##} MB";
            public string Base64 { get; set; }
            public string Extension { get; set; }
            public string Icon =>
                Extension switch
                {
                    ".pdf" => "📕",
                    ".jpg" or ".jpeg" or ".png" => "🖼️",
                    ".doc" or ".docx" => "📘",
                    ".txt" => "📄",
                    _ => "📎"
                };
        }

        // Archivos seleccionados (previos al envío)
        public ObservableCollection<SelectedFileItem> SelectedFiles { get; } = new();

        public bool HasSelectedFiles => SelectedFiles.Count > 0;

        private string _addButtonText = "Agregar (0)";
        public string AddButtonText
        {
            get => _addButtonText;
            set { _addButtonText = value; OnPropertyChanged(); }
        }

        public AddDocuemt(CourtService courtService)
        {
            InitializeComponent();
            _courtService = courtService;
            BindingContext = this;

            // Cargar desde caché al abrir el popup
            if (SelectionCache.Count > 0)
            {
                foreach (var item in SelectionCache)
                    SelectedFiles.Add(new SelectedFileItem
                    {
                        Name = item.Name,
                        Size = item.Size,
                        Base64 = item.Base64,
                        Extension = item.Extension
                    });
            }

            SelectedFiles.CollectionChanged += (_, __) =>
            {
                // sincroniza la caché en cada cambio
                SyncCacheFromSelected();
                OnPropertyChanged(nameof(HasSelectedFiles));
                AddButtonText = $"Agregar ({SelectedFiles.Count})";
            };
        }

        // Cerrar por “X”: solo cerrar, no alterar servicio ni la caché
        private void OnCloseTapped(object sender, EventArgs e)
        {
            try { Close(); } catch { }
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

                var options = new PickOptions()
                {
                    PickerTitle = "Seleccione uno o varios archivos",
                    FileTypes = customFileType,
                };

                var results = await FilePicker.Default.PickMultipleAsync(options);
                if (results is null) return;

                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" };

                foreach (var r in results)
                {
                    var ext = Path.GetExtension(r.FileName)?.ToLowerInvariant() ?? "";
                    if (!allowedExtensions.Contains(ext)) continue;

                    var fileSize = new FileInfo(r.FullPath).Length;
                    if (fileSize > 5 * 1024 * 1024) continue; // 5MB por archivo

                    // Evitar duplicados por nombre dentro de la selección del popup
                    if (SelectedFiles.Any(f => string.Equals(f.Name, r.FileName, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    string base64 = await Task.Run(async () =>
                    {
                        using var stream = await r.OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        return Convert.ToBase64String(ms.ToArray());
                    });

                    SelectedFiles.Add(new SelectedFileItem
                    {
                        Name = r.FileName,
                        Size = fileSize,
                        Base64 = base64,
                        Extension = ext
                    });
                }

                // Sin confirmaciones aquí: la lista previa del popup es suficiente
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudieron seleccionar archivos:\n\n{ex.Message}", "Error de Archivo");
            }
        }

        private void OnRemoveItemClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is SelectedFileItem item)
            {
                SelectedFiles.Remove(item);
            }
        }

        private async void OnClearSelectedClicked(object sender, EventArgs e)
        {
            if (SelectedFiles.Count == 0) return;
            SelectedFiles.Clear();
            await Task.Yield();
        }

        // Confirmar y agregar todos al servicio (una sola confirmación final)
        private async void Add_Dispenser(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;

            try
            {
                if (SelectedFiles.Count == 0)
                {
                    await CustomAlert.ShowWarningAsync("Debe seleccionar al menos un archivo.", "Adjuntar documentos");
                    return;
                }

                var filesBase64 = SelectedFiles.Select(f => f.Base64).ToList();
                var names = SelectedFiles.Select(f => f.Name).ToList();

                _courtService.AddDocumentsFromPopup(filesBase64, names);

                // Limpiar selección una vez que quedaron agregados al corte
                SelectedFiles.Clear();
                SelectionCache.Clear();

                // Mensaje único al finalizar
                // (Si no quieres ningún mensaje aquí, puedes eliminar este bloque)
                // var totalMB = filesBase64.Sum(b64 => GetApproxBytesFromBase64(b64)) / 1024.0 / 1024.0;
                // await CustomAlert.ShowSuccessAsync($"Se agregaron {names.Count} archivo(s) ({totalMB:0.##} MB) al cierre.","Documentos agregados");

                await CloseAsync();
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al agregar documentos:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                if (button != null) button.IsEnabled = true;
            }
        }

        private static long GetApproxBytesFromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return 0;
            int padding = base64.EndsWith("==") ? 2 : base64.EndsWith("=") ? 1 : 0;
            return (long)((base64.Length * 3) / 4) - padding;
        }

        private void SyncCacheFromSelected()
        {
            SelectionCache.Clear();
            foreach (var item in SelectedFiles)
                SelectionCache.Add(item);
        }

        private async Task CloseAsync()
        {
            try { await Task.Delay(50); Close(); } catch { }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}