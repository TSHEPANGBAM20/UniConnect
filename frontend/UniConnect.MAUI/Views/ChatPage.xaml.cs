
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    //controls chat screens
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;

        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

    
        //messages appear here 
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

        //send Message
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
