using APP.Eds.Services.HoseHistory;

namespace APP.Eds.UsesCases.HoseHistory;

public partial class HoseHistoryPostView : ContentPage
{
    private HoseHistoryService _hosehistoryService;

    public HoseHistoryPostView()
    {
        InitializeComponent();
        _hosehistoryService = new HoseHistoryService();
        BindingContext = _hosehistoryService;
    }

}
