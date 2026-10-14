namespace APP.Eds.Controls;

/// <summary>Stacks existing form cells on narrow screens without replacing their bindings.</summary>
public sealed class ResponsiveFormGrid : Grid
{
    private List<(View View, int Row, int Column, int RowSpan, int ColumnSpan)>? _cells;
    private List<ColumnDefinition>? _columns;
    private List<RowDefinition>? _rows;
    private bool _stacked;
    public ResponsiveFormGrid()
    {
        Loaded += (_, _) => Reflow();
        SizeChanged += (_, _) => Reflow();
    }
    private void Reflow()
    {
        if (Width <= 0 || Children.Count == 0) return;
        if (_cells == null)
        {
            _columns = ColumnDefinitions.ToList();
            _rows = RowDefinitions.ToList();
            _cells = Children.OfType<View>().Select(view => (view, GetRow((BindableObject)view), GetColumn((BindableObject)view), GetRowSpan((BindableObject)view), GetColumnSpan((BindableObject)view))).OrderBy(cell => cell.Item2).ThenBy(cell => cell.Item3).ToList();
        }
        var stack = Width < Math.Max(480, (_columns?.Count ?? 1) * 240);
        if (stack == _stacked) return;
        _stacked = stack;
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();
        if (stack)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            for (var i = 0; i < _cells.Count; i++)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var view = _cells[i].View;
                SetRow((BindableObject)view, i); SetColumn((BindableObject)view, 0); SetRowSpan((BindableObject)view, 1); SetColumnSpan((BindableObject)view, 1);
            }
        }
        else
        {
            foreach (var column in _columns!) ColumnDefinitions.Add(column);
            foreach (var row in _rows!) RowDefinitions.Add(row);
            foreach (var cell in _cells)
            {
                SetRow((BindableObject)cell.View, cell.Row); SetColumn((BindableObject)cell.View, cell.Column);
                SetRowSpan((BindableObject)cell.View, cell.RowSpan); SetColumnSpan((BindableObject)cell.View, cell.ColumnSpan);
            }
        }
    }
}
