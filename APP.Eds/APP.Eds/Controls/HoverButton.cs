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
        public Color NormalColor { get; set; } = Colors.Green;
        public Color HoverColor { get; set; } = Colors.DarkGreen;
        public Color PressedColor { get; set; } = Colors.LimeGreen;


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

    }
}


