using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls;
#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Runtime.InteropServices;
#endif

namespace APP.Eds.UsesCases.Behaviors
{
    internal class HoverEffectBehavior: Behavior<Button>
    {
        public Color NormalColor { get; set; } = Colors.Green;
        public Color HoverColor { get; set; } = Colors.DarkGreen;
        public Color PressedColor { get; set; } = Colors.LimeGreen;

        private Button _button;

#if WINDOWS
        // Importación de funciones nativas Win32
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        private const int IDC_ARROW = 32512;
        private const int IDC_HAND = 32649;
#endif

        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);
            _button = bindable;
            _button.BackgroundColor = NormalColor;

            _button.Pressed += OnPressed;
            _button.Released += OnReleased;

#if WINDOWS
          bindable.Loaded += OnButtonLoaded;
#endif

        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);
            _button.Pressed -= OnPressed;
            _button.Released -= OnReleased;

#if WINDOWS
            bindable.Loaded -= OnButtonLoaded;
#endif
        }

        private void OnPressed(object sender, EventArgs e)
        {
            _button.BackgroundColor = PressedColor;
        }

        private void OnReleased(object sender, EventArgs e)
        {
            _button.BackgroundColor = NormalColor;
        }

#if WINDOWS
           private void OnButtonLoaded(object sender, EventArgs e)
           {
                var handler = _button.Handler as ButtonHandler;
                var platformView = handler?.PlatformView;
                
                if (platformView != null)
                {
                     platformView.PointerEntered += PlatformView_PointerEntered;
                     platformView.PointerExited += PlatformView_PointerExited;
            }
        }

        private void PlatformView_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _button.BackgroundColor = HoverColor;

            // Cambiar el cursor a mano:
            IntPtr hCursor = LoadCursor(IntPtr.Zero, IDC_HAND);
            SetCursor(hCursor);
        }

        private void PlatformView_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _button.BackgroundColor = NormalColor;

            // Restaurar el cursor:
            IntPtr hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
            SetCursor(hCursor);
        }
#endif
    }
}
