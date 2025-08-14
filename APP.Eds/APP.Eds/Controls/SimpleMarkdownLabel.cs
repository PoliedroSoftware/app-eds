using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace APP.Eds.Controls
{
    public class SimpleMarkdownLabel : StackLayout
    {
        public static readonly BindableProperty MarkdownTextProperty = BindableProperty.Create(
            nameof(MarkdownText),
            typeof(string),
            typeof(SimpleMarkdownLabel),
            string.Empty,
            propertyChanged: OnMarkdownTextChanged);

        public string MarkdownText
        {
            get => (string)GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        public SimpleMarkdownLabel()
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 8;
        }

        private static void OnMarkdownTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SimpleMarkdownLabel markdownLabel && newValue is string markdownText)
            {
                markdownLabel.RenderMarkdown(markdownText);
            }
        }

        private void RenderMarkdown(string markdown)
        {
            Children.Clear();

            if (string.IsNullOrEmpty(markdown))
                return;

            var lines = markdown.Split('\n');
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    // Agregar espacio entre párrafos
                    Children.Add(new BoxView { HeightRequest = 4, BackgroundColor = Colors.Transparent });
                    continue;
                }

                if (trimmedLine.StartsWith("## "))
                {
                    // Título de nivel 2
                    AddHeading(trimmedLine.Substring(3));
                }
                else if (trimmedLine.StartsWith("- "))
                {
                    // Lista
                    AddListItem(trimmedLine.Substring(2));
                }
                else if (Regex.IsMatch(trimmedLine, @"^\*\*.*\*\*"))
                {
                    // Texto en negrita
                    AddBoldText(trimmedLine);
                }
                else
                {
                    // Texto normal
                    AddParagraph(trimmedLine);
                }
            }
        }

        private void AddHeading(string text)
        {
            var label = new Label
            {
                Text = text,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1F2937"),
                Margin = new Thickness(0, 8, 0, 4)
            };
            Children.Add(label);
        }

        private void AddParagraph(string text)
        {
            // Procesar texto con formato inline (código, negrita)
            text = ProcessInlineFormatting(text);
            
            var label = new Label
            {
                Text = text,
                FontSize = 14,
                TextColor = Color.FromArgb("#374151"),
                Margin = new Thickness(0, 0, 0, 4),
                LineBreakMode = LineBreakMode.WordWrap
            };
            Children.Add(label);
        }

        private void AddListItem(string text)
        {
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(16, 0, 0, 4)
            };

            var bullet = new Label
            {
                Text = "•",
                FontSize = 14,
                TextColor = Color.FromArgb("#6B7280"),
                VerticalOptions = LayoutOptions.Start
            };

            var content = new Label
            {
                Text = ProcessInlineFormatting(text),
                FontSize = 14,
                TextColor = Color.FromArgb("#374151"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Start
            };

            stackLayout.Children.Add(bullet);
            stackLayout.Children.Add(content);
            Children.Add(stackLayout);
        }

        private void AddBoldText(string text)
        {
            // Remover los asteriscos y crear texto en negrita
            var cleanText = text.Replace("**", "");
            var label = new Label
            {
                Text = cleanText,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1F2937"),
                Margin = new Thickness(0, 0, 0, 4)
            };
            Children.Add(label);
        }

        private string ProcessInlineFormatting(string text)
        {
            // Procesar código inline (texto entre ` `)
            text = Regex.Replace(text, @"`([^`]+)`", "$1");
            
            // Procesar negrita inline (texto entre ** **)
            text = Regex.Replace(text, @"\*\*([^*]+)\*\*", "$1");
            
            return text;
        }
    }
}