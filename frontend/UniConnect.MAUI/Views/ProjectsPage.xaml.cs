
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class ProjectsPage : ContentPage, IQueryAttributable
    {
        private readonly ProjectsViewModel _viewModel;
        private bool _openJoinPrompt;

        public ProjectsPage(ProjectsViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _openJoinPrompt =
                query.TryGetValue("openJoinPrompt", out var value) &&
                string.Equals(
                    value?.ToString(),
                    "true",
                    StringComparison.OrdinalIgnoreCase);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.LoadProjectsAsync();

            if (_openJoinPrompt)
            {
                // Prevent the prompt from opening more than once.
                _openJoinPrompt = false;

                await OpenJoinWithCodePromptAsync();
            }
        }

        private async void OnNewProjectClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewProjectPage));
        }

        public async Task OpenJoinWithCodePromptAsync()
        {
            string? code = await this.DisplayPromptAsync(
                "Join a project",
                "Enter the 6-digit project code your Team Lead shared with you.",
                "Join",
                "Cancel",
                placeholder: "123456",
                maxLength: 6,
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(code))
                return;

            var (projectId, error) =
                await _viewModel.JoinByCodeAsync(code.Trim());

            if (projectId == null)
            {
                await this.DisplayAlertAsync(
                    "Couldn't join",
                    error ?? "Something went wrong.",
                    "OK");

                return;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(TaskBoardPage)}?projectId={projectId}");
        }

        private async void OnJoinWithCodeClicked(object? sender, EventArgs e)
        {
            await OpenJoinWithCodePromptAsync();
        }

        private async void OnProjectTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject view &&
                view.BindingContext is ProjectModel project)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(TaskBoardPage)}?projectId={project.ProjectId}");
            }
        }

        private async void OnTabSelected(object? sender, string tab)
            => await TabNavigator.GoToTabAsync(
                this, tab, current: "Projects");

        private async void OnMenuItemSelected(object? sender, string item)
            => await TabNavigator.HandleMenuItemAsync(this, item);
    }
}
