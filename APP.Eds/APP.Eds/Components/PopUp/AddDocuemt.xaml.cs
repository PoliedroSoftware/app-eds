using System.Collections.ObjectModel; // ObservableCollection
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using APP.Eds.Services.Files;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using APP.Eds.Models.Court;
using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

#if WINDOWS
using Windows.Storage.Pickers;
using Windows.Storage; // StorageFile + OpenStreamForReadAsync()
using WinRT.Interop;
#endif

namespace APP.Eds.Components.PopUp;

public partial class AddDocuemt : Popup
{
    private readonly CourtService _courtService;

    public class SelectedFileItem
    {
        public FileResult? File { get; init; }
#if WINDOWS
        public StorageFile? WinFile { get; init; }
#endif
        public required string Name { get; init; }
        public long Size { get; init; }

        public string? FullPath =>
            File?.FullPath
#if WINDOWS
            ?? WinFile?.Path
#endif
            ;

        public string SizeText => Size < 1024 ? $"{Size} B" :
                                  Size < 1024 * 1024 ? $"{Size / 1024.0:F1} KB" :
                                  $"{Size / 1024.0 / 1024.0:F2} MB";

        public async Task<Stream> OpenReadAsync()
        {
#if WINDOWS
            if (WinFile is not null)
                return await WinFile.OpenStreamForReadAsync();
#endif
            if (File is not null)
                return await File.OpenReadAsync();

            throw new InvalidOperationException("No hay origen de archivo para abrir.");
        }
    }

    public ObservableCollection<SelectedFileItem> SelectedFiles { get; } = new();

    public int SelectedCount => SelectedFiles.Count;
    public string TotalSizeText
    {
        get
        {
            var bytes = SelectedFiles.Sum(f => f.Size);
            if (bytes == 0) return string.Empty;
            return $"• Total: { (bytes < 1024 ? $"{bytes} B" :
                                bytes < 1024 * 1024 ? $"{bytes / 1024.0:F1} KB" :
                                $"{bytes / 1024.0 / 1024.0:F2} MB") }";
        }
    }
    public string SummaryText => SelectedCount > 0 ? "Se subirán en un solo paquete (1 petición HTTP)" : string.Empty;

    public AddDocuemt(CourtService service)
    {
        InitializeComponent();
        _courtService = service;
        BindingContext = this;
    }

    private void RefreshBindings()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(TotalSizeText));
        OnPropertyChanged(nameof(SummaryText));
    }

    // Comparador básico para evitar duplicados al acumular selección
    private static bool AreSame(SelectedFileItem a, SelectedFileItem b)
    {
        var ap = a.FullPath;
        var bp = b.FullPath;

        if (!string.IsNullOrWhiteSpace(ap) && !string.IsNullOrWhiteSpace(bp))
            return string.Equals(ap, bp, StringComparison.OrdinalIgnoreCase);

        return string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase) && a.Size == b.Size;
    }

    // Picker unificado. En Windows usa WinUI FileOpenPicker anclado a la ventana.
    private async Task<IEnumerable<SelectedFileItem>> PickFilesAsync()
    {
#if WINDOWS
        var mauiWindow = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (mauiWindow is null)
            return Enumerable.Empty<SelectedFileItem>();

        var hWnd = WindowNative.GetWindowHandle(mauiWindow); // ancla al HWND de la app

        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Downloads,
            ViewMode = PickerViewMode.List
        };
        InitializeWithWindow.Initialize(picker, hWnd);

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".pdf");

        var files = await picker.PickMultipleFilesAsync();
        if (files is null || files.Count == 0)
            return Enumerable.Empty<SelectedFileItem>();

        var list = new List<SelectedFileItem>(files.Count);
        foreach (var file in files)
        {
            long size = 0;
            try
            {
                var props = await file.GetBasicPropertiesAsync();
                size = (long)props.Size;
            }
            catch { /* deja size=0 si no es posible leerlo */ }

            list.Add(new SelectedFileItem
            {
                WinFile = file,          // Guardar StorageFile (no construir FileResult)
                Name = file.Name,
                Size = size
            });
        }
        return list;
#else
        var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
        {
            PickerTitle = "Selecciona uno o varios documentos",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "image/jpeg", "image/png", "application/pdf" } },
                { DevicePlatform.iOS, new[] { "public.jpeg", "public.png", "com.adobe.pdf" } },
                { DevicePlatform.MacCatalyst, new[] { "public.jpeg", "public.png", "com.adobe.pdf" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".pdf" } },
            })
        });

        if (results is null) return Enumerable.Empty<SelectedFileItem>();

        var accepted = results.Where(r =>
            r.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            r.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            r.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            r.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

        var list = new List<SelectedFileItem>();
        foreach (var f in accepted)
        {
            long size = 0;
            try { using var s = await f.OpenReadAsync(); size = s.Length; } catch { }
            list.Add(new SelectedFileItem { File = f, Name = f.FileName, Size = size });
        }
        return list;
#endif
    }

    private bool _isPicking;

    private async void OnPickMultipleFiles(object sender, EventArgs e)
    {
        try
        {
            if (_isPicking) return;               // evita doble apertura
            _isPicking = true;

            var picked = await PickFilesAsync();

            // Acumular en lugar de reemplazar; evitar duplicados
            foreach (var item in picked)
                if (!SelectedFiles.Any(x => AreSame(x, item)))
                    SelectedFiles.Add(item);

            RefreshBindings();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error seleccionando archivos:\n\n{ex.Message}", "Error");
        }
        finally
        {
            _isPicking = false;
        }
    }

    private async void OnUpload(object sender, EventArgs e)
    {
        if (SelectedFiles.Count == 0)
        {
            await CustomAlert.ShowWarningAsync("Seleccione al menos un archivo.", "Archivos requeridos");
            return;
        }

        try
        {
            if (sender is Button b) { b.IsEnabled = false; b.Text = "Subiendo..."; }

            var token = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            var upload = new FileUploadService(token);

            var docs = new List<CourtDocument>(SelectedFiles.Count);
            foreach (var item in SelectedFiles)
            {
                using var s = await item.OpenReadAsync(); // <-- unificado
                using var ms = new MemoryStream();
                await s.CopyToAsync(ms);
                docs.Add(new CourtDocument { DocumentName = item.Name, Descripcion = Convert.ToBase64String(ms.ToArray()) });
            }

            int? courtId = null;
            var result = await upload.UploadDocumentsBatchAsync(docs, courtId);

            if (result.Success)
            {
                await CustomAlert.ShowSuccessAsync(result.Message, "Archivos subidos");
                Close();
            }
            else
            {
                var det = result.FailedUploads.FirstOrDefault()?.Message ?? "Error desconocido";
                await CustomAlert.ShowErrorAsync($"{result.Message}\n\n{det}", "Error al subir");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al subir archivos:\n\n{ex.Message}", "Error del sistema");
        }
        finally
        {
            if (sender is Button b) { b.IsEnabled = true; b.Text = "Subir"; }
        }
    }

    private void OnCloseTapped(object sender, EventArgs e) { try { Close(); } catch { } }
}