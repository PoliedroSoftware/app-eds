// Se añadió un comentario para forzar la recompilación.
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

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

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            Entry entry = (Entry)sender;
            string newText = Regex.Replace(args.NewTextValue, @"[^a-zA-Z\s]", "");
            if (entry.Text != newText)
            {
                entry.Text = newText;
            }
        }
    }
}