
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    // Controls the chat screen.
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;

        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        // Loads messages and displays the latest message when opening chat.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.LoadMessagesAsync();

            await Task.Delay(100);

            ScrollToLatestMessage();

            if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
            {
                await DisplayAlertAsync(
                    "Chat unavailable",
                    _viewModel.ErrorMessage,
                    "OK");
            }
        }

        // Scrolls to the newest message.
        private void ScrollToLatestMessage()
        {
            if (_viewModel.Messages.Count == 0)
                return;

            var latestMessage = _viewModel.Messages[
                _viewModel.Messages.Count - 1];

            MessagesCollectionView.ScrollTo(
                latestMessage,
                position: ScrollToPosition.End,
                animate: false);
        }

        // Sends a message and scrolls to it.
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

                await Task.Delay(100);

                ScrollToLatestMessage();

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
