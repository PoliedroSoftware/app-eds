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

                // Permitir solo letras y espacios
                bool isValid = System.Text.RegularExpressions.Regex.IsMatch(newText, @"^[a-zA-Z\s]*$");

                if (!isValid)
                {
                    entry.Text = oldText;
                }
            }
        }
    }
}