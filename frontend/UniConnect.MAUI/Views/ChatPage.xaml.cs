
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    // This file controls the behaviour of the UniConnect chat screen, including loading messages
    // when the page opens, passing the user's message to the ChatViewModel when they send it,
    // and displaying alerts when messages cannot be loaded or sent. The ChatViewModel handles
    // the underlying message operations, including any communication with the backend.
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;

        // This constructor loads the chat screen's XAML layout and receives the ChatViewModel,
        // which manages the chat data and message operations. It connects the view model to the
        // page so the interface can display and interact with the data it provides.
        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        // This method runs when the chat screen appears and asks the ChatViewModel to load the
        // conversation's messages. If loading fails, it displays an alert so the user knows why
        // the chat may be unavailable.
        //
        // BACKEND NOTE: Check that LoadMessagesAsync() retrieves the correct conversation's
        // message history. The request needs to identify the relevant project or chat, and the
        // response must provide the message details expected by the frontend. Agree on the API
        // endpoint, request parameters and response format with the frontend implementation.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.LoadMessagesAsync();

            if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
            {
                await DisplayAlertAsync(
                    "Chat unavailable",
                    _viewModel.ErrorMessage,
                    "OK");
            }
        }

        // This method reads the message from the input field, checks that it is not empty,
        // and asks the ChatViewModel to send it. When sending succeeds, it clears the input;
        // if an error occurs, it displays an alert to let the user know the message was not sent.
        //
        // BACKEND NOTE: The message-sending operation must eventually submit the message to the
        // backend with the correct conversation or project identifier and authenticated sender.
        // Agree on the endpoint, required request fields and success/error response with the
        // frontend implementation. Do not treat a message as successfully sent unless the
        // backend operation confirms success.
        private async void OnSendClicked(object? sender, EventArgs e)
        {
            _viewModel.DraftMessage = MessageEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_viewModel.DraftMessage))
                return;

            try
            {
                await _viewModel.SendAsync();

                if (string.IsNullOrEmpty(_viewModel.DraftMessage))
                    MessageEntry.Text = string.Empty;

                if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
                {
                    await DisplayAlertAsync(
                        "Chat update",
                        _viewModel.ErrorMessage,
                        "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlertAsync(
                    "Message not sent",
                    _viewModel.ErrorMessage is not null
                        ? _viewModel.ErrorMessage
                        : "Couldn't send the message. Check the backend connection and try again.",
                    "OK");
            }
        }
    }
}
