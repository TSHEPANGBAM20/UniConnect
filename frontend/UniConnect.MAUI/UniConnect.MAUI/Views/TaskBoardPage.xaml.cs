using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;
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

        private async void OnDoneClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskModel task)
                await _viewModel.MarkDoneAsync(task);
        }

        // Menu icon (user story: menu -> Chat / Project details)
        private async void OnMenuTapped(object? sender, TappedEventArgs e)
        {
            string choice = await this.DisplayActionSheetAsync("Project", "Cancel", null, "Chat", "Project details");

            if (choice == "Chat")
                await Shell.Current.GoToAsync($"{nameof(ChatPage)}?projectId={_viewModel.ProjectId}");
            else if (choice == "Project details")
                await this.DisplayAlertAsync("Coming soon", "Project details isn't built yet.", "OK");
        }

        private async void OnAddTaskTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewTaskPage));
        }

        // current is empty on purpose: tapping Projects here returns to the project list
        private async void OnTabSelected(object? sender, string tab)
            => await TabNavigator.GoToTabAsync(this, tab, current: "");

        private async void OnMenuItemSelected(object? sender, string item)
            => await TabNavigator.HandleMenuItemAsync(this, item);
    }
}
