using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiButton = Microsoft.Maui.Controls.Button;
using System.Runtime.InteropServices;
using MauiApp = Microsoft.Maui.Controls.Application;

#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Platform;
using WinRT.Interop;
using Microsoft.UI.Xaml;
#endif

namespace APP.Eds.Controls
{
    public class HoverButton : MauiButton
    {
        // Propiedades para los colores personalizables
        public Color NormalColor { get; set; } = Colors.Green;
        public Color HoverColor { get; set; } = Colors.DarkGreen;
        public Color PressedColor { get; set; } = Colors.LimeGreen;

#if WINDOWS
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        private const int IDC_ARROW = 32512;
        private const int IDC_HAND = 32649;
#endif

        public HoverButton()
        {
            BackgroundColor = NormalColor;

            Pressed += OnPressed;
            Released += OnReleased;

            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += OnPointerEntered;
            pointerGesture.PointerExited += OnPointerExited;
            GestureRecognizers.Add(pointerGesture);
        }

        private async void OnPressed(object sender, EventArgs e)
        {
            await AnimateColorAsync(PressedColor);
        }

        private async void OnReleased(object sender, EventArgs e)
        {
            await AnimateColorAsync(NormalColor);
        }

        private async void OnPointerEntered(object sender, EventArgs e)
        {
            await AnimateColorAsync(HoverColor);
#if WINDOWS
            ChangeCursorToHand(true);
#endif
        }

        private async void OnPointerExited(object sender, EventArgs e)
        {
            await AnimateColorAsync(NormalColor);
#if WINDOWS
            ChangeCursorToHand(false);
#endif
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
            return Color.FromRgba(
                from.Red + (to.Red - from.Red) * t,
                from.Green + (to.Green - from.Green) * t,
                from.Blue + (to.Blue - from.Blue) * t,
                from.Alpha + (to.Alpha - from.Alpha) * t);
        }

#if WINDOWS
        private void ChangeCursorToHand (bool isHand)
        {
             var mauiWindow = MauiApp.Current.Windows.FirstOrDefault();
             if (mauiWindow?.Handler?.PlatformView is Microsoft.UI.Xaml.Window window)
             {
                IntPtr hwnd = WindowNative.GetWindowHandle(window);
                IntPtr hCursor = LoadCursor(IntPtr.Zero, isHand ? IDC_HAND : IDC_ARROW);
                SetCursor(hCursor);
             }
}
#endif
    }
}


