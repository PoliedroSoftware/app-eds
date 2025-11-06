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

        // Límites de tamaño de archivos (configurables)
        private const long MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB por archivo
        private const long MAX_TOTAL_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB total
        private const double MAX_FILE_SIZE_MB = MAX_FILE_SIZE_BYTES / (1024.0 * 1024.0);
        private const double MAX_TOTAL_SIZE_MB = MAX_TOTAL_SIZE_BYTES / (1024.0 * 1024.0);

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

                var skippedFiles = new List<string>();
                var oversizedFiles = new List<(string Name, long Size)>();

                foreach (var r in results)
                {
                    var ext = Path.GetExtension(r.FileName)?.ToLowerInvariant() ?? "";
                    
                    // Verificar extensión permitida
                    if (!allowedExtensions.Contains(ext))
                    {
                        skippedFiles.Add($"{r.FileName} (formato no permitido)");
                        continue;
                    }

                    var fileSize = new FileInfo(r.FullPath).Length;
                    
                    // Verificar tamaño del archivo individual
                    if (fileSize > MAX_FILE_SIZE_BYTES)
                    {
                        oversizedFiles.Add((r.FileName, fileSize));
                        continue;
                    }

                    // Evitar duplicados por nombre dentro de la selección del popup
                    if (SelectedFiles.Any(f => string.Equals(f.Name, r.FileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        skippedFiles.Add($"{r.FileName} (ya agregado)");
                        continue;
                    }

                    // Verificar que no se exceda el tamaño total permitido
                    var currentTotalSize = SelectedFiles.Sum(f => f.Size);
                    if (currentTotalSize + fileSize > MAX_TOTAL_SIZE_BYTES)
                    {
                        var remainingMB = (MAX_TOTAL_SIZE_BYTES - currentTotalSize) / (1024.0 * 1024.0);
                        await CustomAlert.ShowWarningAsync(
                            $"⚠️ Límite de Tamaño Total Alcanzado\n\n" +
                            $"No se puede agregar '{r.FileName}' porque excedería el límite total permitido.\n\n" +
                            $"• Tamaño actual: {currentTotalSize / (1024.0 * 1024.0):0.##} MB\n" +
                            $"• Tamaño del archivo: {fileSize / (1024.0 * 1024.0):0.##} MB\n" +
                            $"• Límite total: {MAX_TOTAL_SIZE_MB:0.##} MB\n" +
                            $"• Espacio disponible: {remainingMB:0.##} MB\n\n" +
                            $"💡 Sugerencia: Elimine algunos archivos o comprima las imágenes antes de agregar más.",
                            "Límite de Tamaño");
                        break;
                    }

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

                // Mostrar advertencias sobre archivos que exceden el tamaño permitido
                if (oversizedFiles.Any())
                {
                    var fileList = string.Join("\n", oversizedFiles.Select(f => 
                        $"  • {f.Name} ({f.Size / (1024.0 * 1024.0):0.##} MB)"));
                    
                    await CustomAlert.ShowWarningAsync(
                        $"⚠️ Archivos Demasiado Grandes\n\n" +
                        $"Los siguientes archivos exceden el límite de {MAX_FILE_SIZE_MB:0.##} MB por archivo y no fueron agregados:\n\n" +
                        $"{fileList}\n\n" +
                        $"💡 Sugerencias:\n" +
                        $"• Para imágenes: use una resolución menor o comprima el archivo\n" +
                        $"• Para PDFs: reduzca la calidad o divida en archivos más pequeños\n" +
                        $"• Para documentos: guarde en formato comprimido",
                        "Tamaño de Archivo Excedido");
                }
                
                // Mostrar información sobre archivos omitidos por otras razones
                if (skippedFiles.Any() && !oversizedFiles.Any())
                {
                    var fileList = string.Join("\n", skippedFiles.Select(f => $"  • {f}"));
                    await CustomAlert.ShowWarningAsync(
                        $"Algunos archivos no fueron agregados:\n\n{fileList}",
                        "Archivos Omitidos");
                }
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

                // Validación final: verificar tamaño total antes de enviar
                var totalSize = SelectedFiles.Sum(f => f.Size);
                var totalSizeMB = totalSize / (1024.0 * 1024.0);

                if (totalSize > MAX_TOTAL_SIZE_BYTES)
                {
                    await CustomAlert.ShowErrorAsync(
                        $"⚠️ El Tamaño Total Excede el Límite Permitido\n\n" +
                        $"No se pueden enviar los archivos porque el tamaño total supera el límite máximo.\n\n" +
                        $"• Tamaño total actual: {totalSizeMB:0.##} MB\n" +
                        $"• Límite máximo: {MAX_TOTAL_SIZE_MB:0.##} MB\n" +
                        $"• Debe reducir: {(totalSizeMB - MAX_TOTAL_SIZE_MB):0.##} MB\n\n" +
                        $"💡 Opciones:\n" +
                        $"• Elimine algunos archivos de la lista\n" +
                        $"• Comprima las imágenes o archivos PDF\n" +
                        $"• Divida los documentos en envíos más pequeños\n\n" +
                        $"Use el botón 'Limpiar Todo' para empezar de nuevo o elimine archivos individuales.",
                        "Tamaño Excedido");
                    return;
                }

                // Verificar que cada archivo individual no exceda el límite
                var oversizedFile = SelectedFiles.FirstOrDefault(f => f.Size > MAX_FILE_SIZE_BYTES);
                if (oversizedFile != null)
                {
                    await CustomAlert.ShowErrorAsync(
                        $"⚠️ Archivo Individual Demasiado Grande\n\n" +
                        $"El archivo '{oversizedFile.Name}' excede el límite permitido.\n\n" +
                        $"• Tamaño del archivo: {oversizedFile.Size / (1024.0 * 1024.0):0.##} MB\n" +
                        $"• Límite por archivo: {MAX_FILE_SIZE_MB:0.##} MB\n\n" +
                        $"Por favor, elimine este archivo o cargue una versión más pequeña.",
                        "Archivo Muy Grande");
                    return;
                }

                var filesBase64 = SelectedFiles.Select(f => f.Base64).ToList();
                var names = SelectedFiles.Select(f => f.Name).ToList();

                _courtService.AddDocumentsFromPopup(filesBase64, names);

                // Limpiar selección una vez que quedaron agregados al corte
                SelectedFiles.Clear();
                SelectionCache.Clear();

                // Mensaje de confirmación con información del tamaño
                await CustomAlert.ShowSuccessAsync(
                    $"✅ Documentos Agregados Exitosamente\n\n" +
                    $"• Archivos agregados: {names.Count}\n" +
                    $"• Tamaño total: {totalSizeMB:0.##} MB\n\n" +
                    $"Los documentos se enviarán cuando complete el cierre de turno.",
                    "Documentos Adjuntos");

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