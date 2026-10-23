using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class ProjectsPage : ContentPage
    {
        private readonly ProjectsViewModel _viewModel;

        public ProjectsPage(ProjectsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProjectsAsync();//triggers page

        }

        private async void OnNewProjectClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewProjectPage));
        }

        // Join an existing project with its 6-digit code, then open that project.
        private async void OnJoinWithCodeClicked(object? sender, EventArgs e)
        {
            string? code = await this.DisplayPromptAsync(
                "Join a project",
                "Enter the 6-digit project code your Team Lead shared with you.",
                "Join", "Cancel",
                placeholder: "123456", maxLength: 6, keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(code)) return; // cancelled

            var (projectId, error) = await _viewModel.JoinByCodeAsync(code.Trim());
            if (projectId == null)
            {
                await this.DisplayAlertAsync("Couldn't join", error ?? "Something went wrong.", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(TaskBoardPage)}?projectId={projectId}");
        }

        private async void OnProjectTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject view && view.BindingContext is ProjectModel project)
                await Shell.Current.GoToAsync($"{nameof(TaskBoardPage)}?projectId={project.ProjectId}");
        }

        private async void OnTabSelected(object? sender, string tab)
            => await TabNavigator.GoToTabAsync(this, tab, current: "Projects");

        private async void OnMenuItemSelected(object? sender, string item)
            => await TabNavigator.HandleMenuItemAsync(this, item);
    }
}
