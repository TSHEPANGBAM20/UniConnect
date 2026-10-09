using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;

        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadMessagesAsync();
        }

       private async void OnSendClicked(object sender, EventArgs e)
{
    _viewModel.DraftMessage = MessageEntry.Text;

    try
    {
        await _viewModel.SendAsync();

        // Clear the input only after a successful send.
        MessageEntry.Text = string.Empty;
    }
    catch (Exception)
    {
        await DisplayAlertAsync(
            "Message not sent",
            "Your message could not be sent. Please check your connection and try again.",
            "OK");
    }
}
}
}
