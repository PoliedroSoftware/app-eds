using Microsoft.Maui.Controls;

namespace APP.Eds.UsesCases.Behaviors
{
    public class LettersOnlyBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;
                string oldText = e.OldTextValue;

                if (string.IsNullOrEmpty(newText))
                {
                    return;
                }

                // Permitir letras (incluyendo acentos), números, espacios y caracteres especiales comunes (/, #, -, etc.)
                bool isValid = System.Text.RegularExpressions.Regex.IsMatch(newText, @"^[a-zA-Z0-9\u00C0-\u00FF\s/#\-._&()]+$");

                if (!isValid)
                {
                    entry.Text = oldText;
                }
            }
        }
    }
}