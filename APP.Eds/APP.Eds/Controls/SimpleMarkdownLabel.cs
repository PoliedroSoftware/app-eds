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
 Spacing =8;
 Padding = new Thickness(4);
 HorizontalOptions = LayoutOptions.FillAndExpand;
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

 if (string.IsNullOrWhiteSpace(markdown))
 return;

 var lines = markdown.Split('\n');
 bool inCodeBlock = false;
 var codeBlockLines = new List<string>();

 foreach (var line in lines)
 {
 var trimmedLine = line.TrimEnd();

 // Handle code blocks
 if (trimmedLine.StartsWith("```") )
 {
 if (inCodeBlock)
 {
 // End code block
 AddCodeBlock(string.Join("\n", codeBlockLines));
 codeBlockLines.Clear();
 inCodeBlock = false;
 }
 else
 {
 // Start code block
 inCodeBlock = true;
 }
 continue;
 }

 if (inCodeBlock)
 {
 codeBlockLines.Add(line);
 continue;
 }

 if (string.IsNullOrWhiteSpace(trimmedLine))
 {
 Children.Add(new BoxView { HeightRequest =6, BackgroundColor = Colors.Transparent });
 continue;
 }

 // Headers (H1, H2, H3)
 if (trimmedLine.StartsWith("### "))
 {
 AddHeading(trimmedLine.Substring(4),14, Color.FromArgb("#4B5563"));
 }
 else if (trimmedLine.StartsWith("## "))
 {
 AddHeading(trimmedLine.Substring(3),17, Color.FromArgb("#1F2937"));
 }
 else if (trimmedLine.StartsWith("# "))
 {
 AddHeading(trimmedLine.Substring(2),20, Color.FromArgb("#111827"));
 }
 // Lists
 else if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("• "))
 {
 AddListItem(trimmedLine.Substring(2));
 }
 else if (Regex.IsMatch(trimmedLine, @"^\d+\.\s"))
 {
 var match = Regex.Match(trimmedLine, @"^(\d+)\.\s(.*)");
 if (match.Success)
 {
 AddNumberedListItem(match.Groups[1].Value, match.Groups[2].Value);
 }
 }
 // Blockquotes
 else if (trimmedLine.StartsWith("> "))
 {
 AddBlockquote(trimmedLine.Substring(2));
 }
 // Horizontal rule
 else if (trimmedLine == "---" || trimmedLine == "***")
 {
 AddHorizontalRule();
 }
 // Bold-only line
 else if (Regex.IsMatch(trimmedLine, @"^\*\*.*\*\*$"))
 {
 AddBoldText(trimmedLine);
 }
 // Regular paragraph
 else
 {
 AddParagraph(trimmedLine);
 }
 }
 }

 private void AddHeading(string text, double fontSize, Color color)
 {
 var label = new Label
 {
 Text = text,
 FontSize = fontSize,
 FontAttributes = FontAttributes.Bold,
 TextColor = color,
 Margin = new Thickness(0,10,0,6),
 LineBreakMode = LineBreakMode.WordWrap
 };
 Children.Add(label);

 // Add subtle underline for H1 and H2
 if (fontSize >=17)
 {
 Children.Add(new BoxView
 {
 BackgroundColor = Color.FromArgb("#E5E7EB"),
 HeightRequest =2,
 Margin = new Thickness(0,0,0,6),
 HorizontalOptions = LayoutOptions.FillAndExpand
 });
 }
 }

 private void AddParagraph(string text)
 {
 var formattedLabel = CreateFormattedLabel(text);
 formattedLabel.Margin = new Thickness(0,0,0,6);
 Children.Add(formattedLabel);
 }

 private void AddListItem(string text)
 {
 var stackLayout = new StackLayout
 {
 Orientation = StackOrientation.Horizontal,
 Spacing =10,
 Margin = new Thickness(16,0,0,4)
 };

 var bullet = new Label
 {
 Text = "•",
 FontSize =12,
 TextColor = Color.FromArgb("#6A1B9A"),
 VerticalOptions = LayoutOptions.Start,
 Margin = new Thickness(0,3,0,0)
 };

 var content = CreateFormattedLabel(text);
 content.VerticalOptions = LayoutOptions.Start;

 stackLayout.Children.Add(bullet);
 stackLayout.Children.Add(content);
 Children.Add(stackLayout);
 }

 private void AddNumberedListItem(string number, string text)
 {
 var stackLayout = new StackLayout
 {
 Orientation = StackOrientation.Horizontal,
 Spacing =10,
 Margin = new Thickness(16,0,0,4)
 };

 var numberLabel = new Label
 {
 Text = $"{number}.",
 FontSize =14,
 FontAttributes = FontAttributes.Bold,
 TextColor = Color.FromArgb("#6A1B9A"),
 VerticalOptions = LayoutOptions.Start,
 MinimumWidthRequest =25
 };

 var content = CreateFormattedLabel(text);
 content.VerticalOptions = LayoutOptions.Start;

 stackLayout.Children.Add(numberLabel);
 stackLayout.Children.Add(content);
 Children.Add(stackLayout);
 }

 private void AddBlockquote(string text)
 {
 var frame = new Frame
 {
 BackgroundColor = Color.FromArgb("#F3F4F6"),
 BorderColor = Color.FromArgb("#6A1B9A"),
 CornerRadius =8,
 Padding = new Thickness(12,8),
 Margin = new Thickness(8,0,0,6),
 HasShadow = false
 };

 var stack = new StackLayout
 {
 Orientation = StackOrientation.Horizontal,
 Spacing =10
 };

 var quoteIcon = new Label
 {
 Text = "?",
 FontSize =16,
 VerticalOptions = LayoutOptions.Start
 };

 var content = CreateFormattedLabel(text);
 content.VerticalOptions = LayoutOptions.Center;

 stack.Children.Add(quoteIcon);
 stack.Children.Add(content);
 frame.Content = stack;
 Children.Add(frame);
 }

 private void AddCodeBlock(string code)
 {
 var frame = new Frame
 {
 BackgroundColor = Color.FromArgb("#1F2937"),
 CornerRadius =8,
 Padding = new Thickness(12),
 Margin = new Thickness(0,6,0,6),
 HasShadow = false,
 BorderColor = Color.FromArgb("#374151")
 };

 var label = new Label
 {
 Text = code,
 FontSize =12,
 FontFamily = "Courier New",
 TextColor = Color.FromArgb("#10B981"),
 LineBreakMode = LineBreakMode.WordWrap
 };

 frame.Content = label;
 Children.Add(frame);
 }

 private void AddHorizontalRule()
 {
 Children.Add(new BoxView
 {
 BackgroundColor = Color.FromArgb("#D1D5DB"),
 HeightRequest =1,
 Margin = new Thickness(0,10,0,10),
 HorizontalOptions = LayoutOptions.FillAndExpand
 });
 }

 private void AddBoldText(string text)
 {
 var cleanText = text.Replace("**"," ").Trim();
 var label = new Label
 {
 Text = cleanText,
 FontSize =14,
 FontAttributes = FontAttributes.Bold,
 TextColor = Color.FromArgb("#1F2937"),
 Margin = new Thickness(0,0,0,4),
 LineBreakMode = LineBreakMode.WordWrap
 };
 Children.Add(label);
 }

 private Label CreateFormattedLabel(string text)
 {
 var formattedString = new FormattedString();
 var parts = SplitMarkdownText(text);

 foreach (var (content, isBold, isCode, isItalic) in parts)
 {
 var span = new Span { Text = content };

 if (isCode)
 {
 span.FontFamily = "Courier New";
 span.BackgroundColor = Color.FromArgb("#F3F4F6");
 span.TextColor = Color.FromArgb("#6A1B9A");
 span.FontSize =13;
 }
 else if (isBold)
 {
 span.FontAttributes = FontAttributes.Bold;
 span.TextColor = Color.FromArgb("#1F2937");
 }
 else if (isItalic)
 {
 span.FontAttributes = FontAttributes.Italic;
 span.TextColor = Color.FromArgb("#4B5563");
 }
 else
 {
 span.TextColor = Color.FromArgb("#374151");
 }

 span.FontSize =14;
 formattedString.Spans.Add(span);
 }

 return new Label
 {
 FormattedText = formattedString,
 LineBreakMode = LineBreakMode.WordWrap,
 LineHeight =1.4
 };
 }

 private List<(string content, bool isBold, bool isCode, bool isItalic)> SplitMarkdownText(string text)
 {
 var parts = new List<(string, bool, bool, bool)>();
 var pattern = @"(`[^`]+`|\*\*[^*]+\*\*|\*[^*]+\*|[^`*]+)";
 var matches = Regex.Matches(text, pattern);

 foreach (Match match in matches)
 {
 var value = match.Value;

 if (value.StartsWith("`") && value.EndsWith("`"))
 {
 // Code
 parts.Add((value.Trim('`'), false, true, false));
 }
 else if (value.StartsWith("**") && value.EndsWith("**"))
 {
 // Bold
 parts.Add((value.Trim('*'), true, false, false));
 }
 else if (value.StartsWith("*") && value.EndsWith("*") && !value.StartsWith("**"))
 {
 // Italic
 parts.Add((value.Trim('*'), false, false, true));
 }
 else
 {
 // Regular text
 parts.Add((value, false, false, false));
 }
 }

 return parts;
 }
 }
}