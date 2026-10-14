using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class TaskBoardPage : ContentPage
    {
        private readonly TaskBoardViewModel _viewModel;

        public TaskBoardPage(TaskBoardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadTasksAsync();
        }

        private async void OnChatClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?projectId={_viewModel.ProjectId}");
        }

        
        private async void OnTabSelected(object? sender, string tab)
        {
            await Shell.Current.GoToAsync("//DashboardPage");
        }

        private async void OnMenuItemSelected(object? sender, string item)
        {
            await Shell.Current.GoToAsync("//DashboardPage");
        }
    }
}
