using MauiButton = Microsoft.Maui.Controls.Button;

namespace APP.Eds.Controls
{
    public class HoverButton : MauiButton
    {
        // Propiedades para los colores personalizables
        public Color NormalColor { get; set; } = Colors.Green;
        public Color HoverColor { get; set; } = Colors.DarkGreen;
        public Color PressedColor { get; set; } = Colors.White;

        public HoverButton()
        {
            BackgroundColor = NormalColor;

            Pressed += OnPressed;
            Released += OnReleased;

#if WINDOWS
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += OnPointerEntered;
            pointerGesture.PointerExited += OnPointerExited;
            GestureRecognizers.Add(pointerGesture);
#endif

        }

        private async void OnPressed(object sender, EventArgs e)
        {
            _ = AnimateColorAsync(PressedColor);
        }

        private async void OnReleased(object sender, EventArgs e)
        {
            _ = AnimateColorAsync(NormalColor);
        }

#if WINDOWS
        private async void OnPointerEntered(object sender, EventArgs e)
        {
            _ = AnimateColorAsync(HoverColor);

        }

        private async void OnPointerExited(object sender, EventArgs e)
        {
            _ = AnimateColorAsync(NormalColor);
        }
#endif

        private async Task AnimateColorAsync(Color targetColor)
        {
            var startColor = BackgroundColor;

            const int steps = 10;
            const int totalDuration = 150; // ms
            int delay = totalDuration / steps;

            for (int i = 1; i <= steps; i++)
            {
                var interpolated = InterpolateColor(startColor, targetColor, (double)i / steps);
                BackgroundColor = interpolated;
                await Task.Delay(delay);
            }
        }

        private Color InterpolateColor(Color from, Color to, double t)
        {

            return Color.FromRgba(
                from.Red + (to.Red - from.Red) * t,
                from.Green + (to.Green - from.Green) * t,
                from.Blue + (to.Blue - from.Blue) * t,
                from.Alpha + (to.Alpha - from.Alpha) * t);
        }
    }

    public class HoverButtonPopup : MauiButton
    {
        //public Color NormalColorPopup { get; set; } = Colors.Gray;
        private Color? _originalBackgroundColor;
        public Color PressedColorPopup { get; set; } = Colors.Black;
        public Color HoverColorPopup { get; set; } = Colors.Black;
        public HoverButtonPopup()
        {
            //BackgroundColor = NormalColorPopup;

            Pressed += OnPressedPopup;
            Released += OnReleasedPopup;
#if WINDOWS
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += OnPointerEntered;
            pointerGesture.PointerExited += OnPointerExited;
            GestureRecognizers.Add(pointerGesture);
#endif
        }

        private async void OnReleasedPopup(object sender, EventArgs e)
        {
            _ = AnimateColorAsync(_originalBackgroundColor);
        }

        private async void OnPressedPopup(object sender, EventArgs e)
        {
            SaveOriginalColor();
            _ = AnimateColorAsync(PressedColorPopup);
        }

#if WINDOWS
        private async void OnPointerEntered(object sender, EventArgs e)
        {
            _ = AnimateColorAsync(HoverColorPopup);
        }

        private async void OnPointerExited(object sender, EventArgs e)
        {
            SaveOriginalColor();
            _ = AnimateColorAsync(_originalBackgroundColor);
        }
#endif

        private void SaveOriginalColor()
        {
            if (_originalBackgroundColor == null)
                _originalBackgroundColor = BackgroundColor;
        }

        private async Task AnimateColorAsync(Color targetColor)
        {
            var startColor = BackgroundColor;

            const int steps = 10;
            const int totalDuration = 150; // ms
            int delay = totalDuration / steps;

            for (int i = 1; i <= steps; i++)
            {
                var interpolated = InterpolateColor(startColor, targetColor, (double)i / steps);
                BackgroundColor = interpolated;
                await Task.Delay(delay);
            }
        }

        private Color InterpolateColor(Color from, Color to, double t)
        {
            if (from.IsDefault()) from = Colors.Transparent;
            if (to.IsDefault()) to = Colors.Transparent;

            return Color.FromRgba(
                from.Red + (to.Red - from.Red) * t,
                from.Green + (to.Green - from.Green) * t,
                from.Blue + (to.Blue - from.Blue) * t,
                from.Alpha + (to.Alpha - from.Alpha) * t);
        }
    }
}