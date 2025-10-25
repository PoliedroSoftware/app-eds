using System.ComponentModel;
using APP.Eds.Models.Eds;
using APP.Eds.Services.RegisterShift;
using System.Diagnostics;

namespace APP.Eds.UsesCases.RegisterShift;

public partial class RegisterShiftView : ContentPage, INotifyPropertyChanged
{
    private RegisterShiftService _service;

    public RegisterShiftView()
    {
        InitializeComponent();

        _service = RegisterShiftService.Instance;
        BindingContext = _service;

        ConfigureDatePickerSafely();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            _service?.RefreshEdsCommand?.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing EDS list: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
    }

    private void ConfigureDatePickerSafely()
    {
        try
        {
            var picker = this.FindByName<DatePicker>("datePicker");
            if (picker != null)
            {
                picker.MinimumDate = new DateTime(1900, 1, 1);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error configuring DatePicker: {ex.Message}");
        }
    }

    // Event handler referenced in XAML for the EDS Picker
    private void OnEdsSelected(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Picker picker) return;
            if (picker.SelectedItem is EdsResponse selected)
            {
                // The SelectedItem is already bound to _service.SelectedEds in XAML,
                // but we ensure the Id is set in case of manual updates
                _service.SelectedEds = selected;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling EDS selection: {ex.Message}");
        }
    }
}