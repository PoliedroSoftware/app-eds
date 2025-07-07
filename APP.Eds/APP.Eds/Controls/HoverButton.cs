using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiButton = Microsoft.Maui.Controls.Button;
using System.Runtime.InteropServices;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace APP.Eds.Controls
{
    public class HoverButton : MauiButton
    {
        // Propiedades para los colores personalizables
        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }


        public HoverButton()
        {
            Loaded += OnLoaded;

            Pressed += OnPressed;
            Released += OnReleased;

#if WINDOWS
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += OnPointerEntered;
            pointerGesture.PointerExited += OnPointerExited;
            GestureRecognizers.Add(pointerGesture);
#endif
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            if (NormalColor == default || NormalColor == Colors.Transparent)
                NormalColor = this.BackgroundColor != default ? this.BackgroundColor : Colors.Transparent;
                NormalColor = this.TextColor != default ? this.TextColor : Colors.Transparent;

            if (HoverColor == default || HoverColor == Colors.Transparent)
                HoverColor = AdjustBrightness(NormalColor, 1.1);

            if (PressedColor == default || PressedColor == Colors.Transparent)
                PressedColor = AdjustBrightness(NormalColor, 0.9);

            this.BackgroundColor = NormalColor;
        }

        private async void OnPressed(object sender, EventArgs e)
        {
            _= AnimateColorAsync(PressedColor);
        }

        private async void OnReleased(object sender, EventArgs e)
        {
            _= AnimateColorAsync(NormalColor);
        }

#if WINDOWS
        private async void OnPointerEntered(object sender, EventArgs e)
        {
            _= AnimateColorAsync(HoverColor);

        }

        private async void OnPointerExited(object sender, EventArgs e)
        {
            _= AnimateColorAsync(NormalColor);
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

        private Color AdjustBrightness(Color color, double factor)
        {
            return Color.FromRgba(
                Math.Min(color.Red * factor, 1.0),
                Math.Min(color.Green * factor, 1.0),
                Math.Min(color.Blue * factor, 1.0),
                color.Alpha);
        }

    }
}


