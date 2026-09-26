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
            await _viewModel.SendAsync();
            MessageEntry.Text = string.Empty;
        }
    }
}
