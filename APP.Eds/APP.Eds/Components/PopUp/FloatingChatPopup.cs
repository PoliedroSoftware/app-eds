using APP.Eds.Services.Copilot;
using APP.Eds.Controls;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace APP.Eds.Components.PopUp
{
    public class FloatingChatPopup : Popup
    {
        private readonly CopilotService _copilotService;
        private readonly string _currentStepContext;
        private readonly VerticalStackLayout _messagesContainer;
        private readonly ScrollView _chatScrollView;
        private Entry _messageEntry;
        private readonly HorizontalStackLayout _loadingIndicator;
        private readonly Grid _inputGrid;
        private Button _sendButton;
        private readonly Frame _quickActionsFrame;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public FloatingChatPopup(CopilotService copilotService, string currentStepContext = "")
        {
            _copilotService = copilotService;
            _currentStepContext = currentStepContext;

         // Crear contenedor de mensajes con mejor configuración
   _messagesContainer = new VerticalStackLayout
  {
          Spacing =15,
  Padding = new Thickness(10,10)
            };

       _chatScrollView = new ScrollView
       {
    BackgroundColor = Color.FromArgb("#F8F9FA"),
           Padding = new Thickness(10,10),
         Content = _messagesContainer,
     VerticalScrollBarVisibility = ScrollBarVisibility.Always,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Never
    };

   // Loading indicator mejorado
    _loadingIndicator = CreateLoadingIndicator();

            // Quick actions frame
      _quickActionsFrame = CreateQuickActionsFrame();

       // Input area mejorada
CreateInputArea(out _messageEntry, out _inputGrid, out _sendButton);

     // Header mejorado con gradiente
      var headerGrid = CreateModernHeader();

     // Context badge si existe
  var contextBadge = CreateContextBadge();

 // Main content grid
      var contentGrid = new Grid
 {
   RowDefinitions =
  {
 new RowDefinition { Height = GridLength.Auto }, // Header
      new RowDefinition { Height = GridLength.Auto }, // Context badge
          new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Chat (expandible)
 new RowDefinition { Height = GridLength.Auto }, // Quick actions
        new RowDefinition { Height = GridLength.Auto }  // Input
        }
      };
       
         contentGrid.Add(headerGrid, 0);
    contentGrid.Add(contextBadge, 1);
   contentGrid.Add(_chatScrollView, 2);
   contentGrid.Add(_quickActionsFrame, 3);
  contentGrid.Add(_inputGrid, 4);

   // Main frame con mejor dimensionamiento
var mainFrame = new Frame
  {
    BackgroundColor = Colors.White,
      CornerRadius = 24,
       Padding = 0,
         HasShadow = true,
   BorderColor = Colors.Transparent,
         WidthRequest = 360,
      HeightRequest = 600,
       Content = contentGrid
        };

    Content = mainFrame;
     Size = new Size(380, 620);

        // Agregar mensaje de bienvenida mejorado
    AddWelcomeMessage();

 // Animar entrada del popup
  AnimateEntry();
    }

        private Frame CreateContextBadge()
   {
      if (string.IsNullOrEmpty(_currentStepContext))
        {
    return new Frame { IsVisible = false };
        }

         var badge = new Frame
            {
   BackgroundColor = Color.FromArgb("#FFF3E0"),
   BorderColor = Color.FromArgb("#FFB74D"),
    CornerRadius = 8,
     Padding = new Thickness(12, 8),
      Margin = new Thickness(15, 10, 15, 0),
     HasShadow = false
   };

        var stack = new HorizontalStackLayout { Spacing = 8 };
        
   stack.Children.Add(new Label
            {
  Text = "??",
    FontSize = 14,
          VerticalOptions = LayoutOptions.Center
            });

            stack.Children.Add(new Label
 {
                Text = $"Contexto: {_currentStepContext}",
          FontSize = 12,
                FontAttributes = FontAttributes.Bold,
          TextColor = Color.FromArgb("#E65100"),
    VerticalOptions = LayoutOptions.Center
            });

            badge.Content = stack;
            return badge;
        }

        private Frame CreateQuickActionsFrame()
        {
       var frame = new Frame
       {
     BackgroundColor = Colors.White,
         Padding = new Thickness(15, 10),
     BorderColor = Color.FromArgb("#E0E0E0"),
    HasShadow = false,
                CornerRadius = 0
  };

var stack = new VerticalStackLayout { Spacing = 8 };

      var titleLabel = new Label
    {
         Text = "?? Preguntas sugeridas:",
                FontSize = 12,
  FontAttributes = FontAttributes.Bold,
    TextColor = Color.FromArgb("#757575"),
           Margin = new Thickness(0, 0, 0, 5)
     };
   stack.Children.Add(titleLabel);

            var questionsLayout = new FlexLayout
 {
        Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
          Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
  JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
    AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start
            };

            var suggestedQuestions = new[]
      {
        "¿Cómo configuro un tanque?",
     "¿Qué son los compartimientos?",
    "¿Cómo agrego isleros?",
   "¿Cuál es el siguiente paso?"
            };

foreach (var question in suggestedQuestions)
      {
        var button = new Button
           {
      Text = question,
FontSize = 11,
         Padding = new Thickness(12, 6),
       Margin = new Thickness(0, 0, 5, 5),
      BackgroundColor = Color.FromArgb("#F5F5F5"),
       TextColor = Color.FromArgb("#6A1B9A"),
     CornerRadius = 15,
                BorderColor = Color.FromArgb("#E0E0E0"),
        BorderWidth = 1
    };
  button.Clicked += async (s, e) => await SendQuickQuestion(question);
          questionsLayout.Children.Add(button);
            }

            stack.Children.Add(questionsLayout);
  frame.Content = stack;
         return frame;
        }

        private async Task SendQuickQuestion(string question)
    {
     if (_messageEntry != null)
            {
     _messageEntry.Text = question;
       }
            await Task.Delay(100);
 await OnSendMessageAsync();
        }

 private async void AnimateEntry()
        {
            if (Content is Frame frame)
   {
       frame.Opacity = 0;
        frame.Scale = 0.8;
      frame.TranslationY = 50;

       await Task.WhenAll(
     frame.FadeTo(1, 300, Easing.CubicOut),
         frame.ScaleTo(1, 300, Easing.SpringOut),
   frame.TranslateTo(0, 0, 300, Easing.CubicOut)
                );
   }
        }

        private HorizontalStackLayout CreateLoadingIndicator()
        {
         var loadingFrame = new Frame
            {
   BackgroundColor = Color.FromArgb("#EDE7F6"),
                CornerRadius = 20,
        Padding = new Thickness(15, 10),
     HasShadow = false,
        HorizontalOptions = LayoutOptions.Start,
   Margin = new Thickness(15, 10),
                IsVisible = false
            };

          var innerStack = new HorizontalStackLayout { Spacing = 10 };

       // ActivityIndicator con color del tema
       innerStack.Children.Add(new ActivityIndicator
            {
      IsRunning = true,
          Color = Color.FromArgb("#6A1B9A"),
                WidthRequest = 20,
  HeightRequest = 20
 });

   // Animación de puntos suspensivos
     var typingLabel = new Label
 {
     Text = "El asistente está escribiendo",
            FontSize = 13,
         TextColor = Color.FromArgb("#6A1B9A"),
                FontAttributes = FontAttributes.Italic,
             VerticalOptions = LayoutOptions.Center
      };
      innerStack.Children.Add(typingLabel);

    // Agregar puntos animados
       var dotsLabel = new Label
  {
             Text = "...",
                FontSize = 13,
          TextColor = Color.FromArgb("#6A1B9A"),
     VerticalOptions = LayoutOptions.Center
    };
        innerStack.Children.Add(dotsLabel);

   // Animar los puntos
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
{
    if (!loadingFrame.IsVisible) return true;
     
          var currentText = dotsLabel.Text;
        dotsLabel.Text = currentText.Length >= 3 ? "." : currentText + ".";
             return true;
    });

            loadingFrame.Content = innerStack;

    var container = new HorizontalStackLayout
        {
 IsVisible = false,
         HorizontalOptions = LayoutOptions.Start
     };
    container.Children.Add(loadingFrame);

            return container;
}

        private void CreateInputArea(out Entry messageEntry, out Grid inputGrid, out Button sendButton)
      {
      messageEntry = new Entry
      {
            Placeholder = "Escribe tu pregunta aquí...",
     PlaceholderColor = Color.FromArgb("#9E9E9E"),
        TextColor = Color.FromArgb("#212121"),
    FontSize = 14,
  BackgroundColor = Colors.Transparent,
                VerticalOptions = LayoutOptions.Center,
  MaxLength = 500
            };
    messageEntry.Completed += OnSendMessage;

 var inputFrame = new Frame
    {
                BackgroundColor = Color.FromArgb("#F5F5F5"),
       CornerRadius = 24,
       Padding = new Thickness(16, 10),
            BorderColor = Color.FromArgb("#E0E0E0"),
           HeightRequest = 48,
          Content = messageEntry
    };

      sendButton = new Button
    {
         Text = "?",
         BackgroundColor = Color.FromArgb("#6A1B9A"),
        TextColor = Colors.White,
 FontSize = 20,
     FontAttributes = FontAttributes.Bold,
CornerRadius = 24,
          WidthRequest = 48,
             HeightRequest = 48,
       Padding = 0
            };
 sendButton.Clicked += OnSendMessage;

         inputGrid = new Grid
            {
        BackgroundColor = Colors.White,
     Padding = new Thickness(15, 12),
     ColumnDefinitions =
                {
              new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
  new ColumnDefinition { Width = GridLength.Auto }
          },
            ColumnSpacing = 10
    };

     inputGrid.Add(inputFrame, 0);
         inputGrid.Add(sendButton, 1);

            _messageEntry = messageEntry;
            _sendButton = sendButton;
        }

        private Grid CreateModernHeader()
        {
      var headerGrid = new Grid
      {
            Padding = new Thickness(20, 15),
    ColumnDefinitions =
       {
        new ColumnDefinition { Width = GridLength.Auto },
        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
             new ColumnDefinition { Width = GridLength.Auto }
              },
    BackgroundColor = Color.FromArgb("#6A1B9A")
      };

            // Gradiente simulado con overlay
        var gradientOverlay = new BoxView
            {
             BackgroundColor = Color.FromArgb("#20FFFFFF"),
           Opacity = 0.1
      };
            headerGrid.Add(gradientOverlay, 0);
            Grid.SetColumnSpan(gradientOverlay, 3);

     // Avatar del asistente
         var avatarFrame = new Frame
{
  WidthRequest = 48,
       HeightRequest = 48,
      CornerRadius = 24,
    Padding = 0,
          BackgroundColor = Color.FromArgb("#8E24AA"),
   HasShadow = false,
            BorderColor = Color.FromArgb("#FFFFFF"),
   Content = new Label
          {
      Text = "??",
     FontSize = 28,
    HorizontalOptions = LayoutOptions.Center,
    VerticalOptions = LayoutOptions.Center
  }
            };

   // Información del asistente
   var titleStack = new VerticalStackLayout
     {
          Margin = new Thickness(12, 0, 0, 0),
                VerticalOptions = LayoutOptions.Center,
     Spacing = 2
            };

    titleStack.Children.Add(new Label
        {
       Text = "Asistente IA EDS",
         FontSize = 17,
     FontAttributes = FontAttributes.Bold,
  TextColor = Colors.White
  });

      // Estado online con indicador
  var statusStack = new HorizontalStackLayout { Spacing = 6 };

       statusStack.Children.Add(new BoxView
      {
     WidthRequest = 8,
          HeightRequest = 8,
    CornerRadius = 4,
    BackgroundColor = Color.FromArgb("#4CAF50"),
         VerticalOptions = LayoutOptions.Center
            });

     statusStack.Children.Add(new Label
  {
       Text = "En línea • Respuesta rápida",
       FontSize = 11,
 TextColor = Color.FromArgb("#E1BEE7"),
                VerticalOptions = LayoutOptions.Center
          });

 titleStack.Children.Add(statusStack);

     // Botón de cerrar mejorado
            var closeButton = new Button
 {
             Text = "?",
                BackgroundColor = Color.FromArgb("#20FFFFFF"),
       TextColor = Colors.White,
      FontSize = 22,
 FontAttributes = FontAttributes.Bold,
           WidthRequest = 40,
                HeightRequest = 40,
           CornerRadius = 20,
           Padding = 0
        };
closeButton.Clicked += (s, e) => Close();

      headerGrid.Add(avatarFrame, 0);
            headerGrid.Add(titleStack, 1);
       headerGrid.Add(closeButton, 2);

            return headerGrid;
        }

        private void AddWelcomeMessage()
        {
            var welcomeFrame = CreateMessageFrame(false);
            var messageLayout = new VerticalStackLayout { Spacing = 10 };

            // Header del mensaje con avatar
            var headerLayout = CreateMessageHeader(false, "Asistente IA");
            messageLayout.Children.Add(headerLayout);

            // Mensaje de bienvenida mejorado - FIXED: Better structure
            var welcomeStack = new VerticalStackLayout { Spacing = 8 };
            
            // Greeting
            welcomeStack.Children.Add(new Label
            {
                Text = "?? ¡Hola! Bienvenido",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1F2937"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            // Introduction
            var introLabel = new Label
            {
                FontSize = 14,
                TextColor = Color.FromArgb("#374151"),
                LineBreakMode = LineBreakMode.WordWrap,
                LineHeight = 1.4
            };
            
            var introSpan = new FormattedString();
            introSpan.Spans.Add(new Span { Text = "Soy tu " });
            introSpan.Spans.Add(new Span 
            { 
                Text = "asistente inteligente de configuración EDS", 
                FontAttributes = FontAttributes.Bold, 
                TextColor = Color.FromArgb("#6A1B9A") 
            });
            introSpan.Spans.Add(new Span { Text = ". Estoy aquí para ayudarte en cada paso del proceso." });
            introLabel.FormattedText = introSpan;
            welcomeStack.Children.Add(introLabel);

            // Capabilities title
            welcomeStack.Children.Add(new Label
            {
                Text = "?? Puedo ayudarte con:",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1F2937"),
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 8, 0, 4)
            });

            // Capabilities list
            var capabilities = new[]
            {
                "Configuración de infraestructura",
                "Gestión de tanques y dispensadores",
                "Asignación de isleros",
                "Resolución de dudas del sistema"
            };

            foreach (var capability in capabilities)
            {
                var capabilityStack = new HorizontalStackLayout { Spacing = 8 };
                capabilityStack.Children.Add(new Label
                {
                    Text = "•",
                    FontSize = 13,
                    TextColor = Color.FromArgb("#6A1B9A"),
                    VerticalOptions = LayoutOptions.Start
                });
                capabilityStack.Children.Add(new Label
                {
                    Text = capability,
                    FontSize = 13,
                    TextColor = Color.FromArgb("#374151"),
                    LineBreakMode = LineBreakMode.WordWrap,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                });
                welcomeStack.Children.Add(capabilityStack);
            }

            // Call to action
            welcomeStack.Children.Add(new Label
            {
                Text = "Haz una pregunta o usa las sugerencias abajo ??",
                FontSize = 12,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#757575"),
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 8, 0, 0)
            });

            messageLayout.Children.Add(welcomeStack);

            // Timestamp
            var timeLabel = CreateTimestamp();
            messageLayout.Children.Add(timeLabel);

            welcomeFrame.Content = messageLayout;
            _messagesContainer.Children.Add(welcomeFrame);
        }

        private Frame CreateMessageFrame(bool isUser)
        {
            return new Frame
            {
                BackgroundColor = isUser ? Color.FromArgb("#6A1B9A") : Colors.White,
                CornerRadius = 20,
                Padding = new Thickness(16, 12),
                BorderColor = isUser ? Colors.Transparent : Color.FromArgb("#E0E0E0"),
                HasShadow = !isUser,
                HorizontalOptions = isUser ? LayoutOptions.End : LayoutOptions.Start,
                MaximumWidthRequest = 280,
                Margin = new Thickness(0, 5),
                WidthRequest = -1 // Let it size to content
            };
        }

        private HorizontalStackLayout CreateMessageHeader(bool isUser, string name)
      {
     var headerLayout = new HorizontalStackLayout
            {
          Spacing = 8,
                HorizontalOptions = isUser ? LayoutOptions.End : LayoutOptions.Start
            };

      if (isUser)
  {
       headerLayout.Children.Add(new Label
              {
        Text = name,
      FontSize = 12,
       FontAttributes = FontAttributes.Bold,
             TextColor = Color.FromArgb("#E1BEE7"),
     VerticalOptions = LayoutOptions.Center
  });

                headerLayout.Children.Add(new Label
   {
  Text = "??",
    FontSize = 16,
     VerticalOptions = LayoutOptions.Center
            });
         }
         else
     {
        var smallAvatar = new Frame
              {
         WidthRequest = 24,
               HeightRequest = 24,
        CornerRadius = 12,
       Padding = 0,
                BackgroundColor = Color.FromArgb("#EDE7F6"),
          HasShadow = false,
   Content = new Label
         {
            Text = "??",
         FontSize = 14,
             HorizontalOptions = LayoutOptions.Center,
     VerticalOptions = LayoutOptions.Center
         }
    };

          headerLayout.Children.Add(smallAvatar);
        headerLayout.Children.Add(new Label
      {
      Text = name,
        FontSize = 12,
           FontAttributes = FontAttributes.Bold,
          TextColor = Color.FromArgb("#6A1B9A"),
         VerticalOptions = LayoutOptions.Center
 });
         }

 return headerLayout;
 }

        private Label CreateTimestamp()
     {
      return new Label
            {
          Text = DateTime.Now.ToString("HH:mm"),
FontSize = 10,
         TextColor = Color.FromArgb("#9E9E9E"),
       HorizontalOptions = LayoutOptions.End,
       Margin = new Thickness(0, 5, 0, 0)
            };
        }

    private async void OnSendMessage(object sender, EventArgs e)
        {
      await OnSendMessageAsync();
        }

        private async Task OnSendMessageAsync()
        {
          var message = _messageEntry?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(message))
    return;

        // Limpiar el input
     if (_messageEntry != null)
   _messageEntry.Text = string.Empty;

            // Agregar mensaje del usuario
         AddUserMessage(message);

     // Ocultar quick actions y mostrar loading
      _quickActionsFrame.IsVisible = false;
            SetLoading(true);

      try
            {
        // Obtener respuesta de la IA
    var response = await _copilotService.GetHelpAsync(message, _currentStepContext);

       // Agregar respuesta del asistente
       AddAssistantMessage(response);
     }
         catch (Exception ex)
 {
                AddAssistantMessage($"?? Lo siento, ocurrió un error:\n\n{ex.Message}\n\nPor favor, intenta de nuevo.");
        }
      finally
   {
         SetLoading(false);
  }

     // Scroll al final
await ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
     var messageFrame = CreateMessageFrame(true);
    var messageLayout = new VerticalStackLayout { Spacing = 6 };

         // Header del usuario
            var headerLayout = CreateMessageHeader(true, "Tú");
            messageLayout.Children.Add(headerLayout);

       // Mensaje
 messageLayout.Children.Add(new Label
            {
         Text = message,
        FontSize = 14,
         TextColor = Colors.White,
           LineBreakMode = LineBreakMode.WordWrap,
       LineHeight = 1.3
       });

 // Timestamp
     var timeLabel = new Label
      {
      Text = DateTime.Now.ToString("HH:mm"),
          FontSize = 10,
       TextColor = Color.FromArgb("#E1BEE7"),
          HorizontalOptions = LayoutOptions.End
      };
 messageLayout.Children.Add(timeLabel);

    messageFrame.Content = messageLayout;
            _messagesContainer.Children.Add(messageFrame);

            // Animar entrada del mensaje
    AnimateMessageEntry(messageFrame);
        }

        private void AddAssistantMessage(string message)
        {
            var messageFrame = CreateMessageFrame(false);
            var messageLayout = new VerticalStackLayout { Spacing = 8 };

      // Header del asistente
            var headerLayout = CreateMessageHeader(false, "Asistente IA");
    messageLayout.Children.Add(headerLayout);

   // Mensaje con markdown
      try
            {
           var markdown = string.IsNullOrWhiteSpace(_copilotService.MarkdownResponse)
 ? message
 : _copilotService.MarkdownResponse;

 var markdownLabel = new SimpleMarkdownLabel
 {
  MarkdownText = markdown
 };
    messageLayout.Children.Add(markdownLabel);
   }
  catch
    {
     messageLayout.Children.Add(new Label
        {
         Text = message,
   FontSize = 14,
    TextColor = Color.FromArgb("#424242"),
   LineBreakMode = LineBreakMode.WordWrap,
         LineHeight = 1.4
      });
   }

          // Action buttons (copy, feedback)
            var actionButtons = CreateMessageActionButtons(message);
            messageLayout.Children.Add(actionButtons);

          // Timestamp
        var timeLabel = CreateTimestamp();
            timeLabel.TextColor = Color.FromArgb("#9E9E9E");
    messageLayout.Children.Add(timeLabel);

            messageFrame.Content = messageLayout;
     _messagesContainer.Children.Add(messageFrame);

      // Animar entrada del mensaje
            AnimateMessageEntry(messageFrame);
        }

        private HorizontalStackLayout CreateMessageActionButtons(string messageText)
        {
 var stack = new HorizontalStackLayout
            {
       Spacing = 10,
      Margin = new Thickness(0, 5, 0, 0)
        };

       // Botón copiar
   var copyButton = new Button
            {
  Text = "??",
        FontSize = 14,
         Padding = new Thickness(8, 4),
                BackgroundColor = Color.FromArgb("#F5F5F5"),
       TextColor = Color.FromArgb("#6A1B9A"),
  CornerRadius = 12,
                BorderWidth = 0
            };
            copyButton.Clicked += async (s, e) =>
            {
    await Clipboard.SetTextAsync(messageText);
    copyButton.Text = "? Copiado";
     await Task.Delay(1500);
           copyButton.Text = "??";
 };

            // Botón feedback positivo
    var likeButton = new Button
            {
        Text = "??",
    FontSize = 14,
      Padding = new Thickness(8, 4),
      BackgroundColor = Color.FromArgb("#F5F5F5"),
        TextColor = Color.FromArgb("#757575"),
      CornerRadius = 12,
  BorderWidth = 0
            };
  likeButton.Clicked += (s, e) =>
      {
  likeButton.BackgroundColor = Color.FromArgb("#E8F5E9");
      likeButton.TextColor = Color.FromArgb("#4CAF50");
   };

       // Botón feedback negativo
      var dislikeButton = new Button
      {
        Text = "??",
    FontSize = 14,
      Padding = new Thickness(8, 4),
         BackgroundColor = Color.FromArgb("#F5F5F5"),
    TextColor = Color.FromArgb("#757575"),
     CornerRadius = 12,
     BorderWidth = 0
            };
            dislikeButton.Clicked += (s, e) =>
  {
         dislikeButton.BackgroundColor = Color.FromArgb("#FFEBEE");
       dislikeButton.TextColor = Color.FromArgb("#F44336");
   };

     stack.Children.Add(copyButton);
            stack.Children.Add(likeButton);
            stack.Children.Add(dislikeButton);

            return stack;
   }

        private async void AnimateMessageEntry(Frame messageFrame)
        {
messageFrame.Opacity = 0;
 messageFrame.TranslationY = 20;

          await Task.WhenAll(
     messageFrame.FadeTo(1, 250, Easing.CubicOut),
          messageFrame.TranslateTo(0, 0, 250, Easing.CubicOut)
        );
        }

        private void SetLoading(bool isLoading)
        {
       if (_loadingIndicator != null)
 {
_loadingIndicator.IsVisible = isLoading;
     
        // Si está cargando, agregar al contenedor si no está ya
       if (isLoading && !_messagesContainer.Children.Contains(_loadingIndicator))
       {
       _messagesContainer.Children.Add(_loadingIndicator);
         }
  // Si terminó de cargar, remover del contenedor
     else if (!isLoading && _messagesContainer.Children.Contains(_loadingIndicator))
                {
     _messagesContainer.Children.Remove(_loadingIndicator);
            }
      }

   if (_sendButton != null)
    _sendButton.IsEnabled = !isLoading;

   if (_messageEntry != null)
   _messageEntry.IsEnabled = !isLoading;
}

        private async Task ScrollToBottom()
      {
 await Task.Delay(150);

       if (_chatScrollView != null && _messagesContainer != null)
      {
  await _chatScrollView.ScrollToAsync(0, _messagesContainer.Height, true);
            }
    }
    }

    public class ChatMessage
    {
        public string Text { get; set; }
   public bool IsUser { get; set; }
   public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
