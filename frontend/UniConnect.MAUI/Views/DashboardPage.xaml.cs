
using System.Diagnostics;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class DashboardPage : ContentPage, IQueryAttributable
    {
        private readonly DashboardViewModel _viewModel;
        private readonly ApiService _apiService;

        public string Preview { get; set; } = "false";

        public DashboardPage(
            DashboardViewModel viewModel,
            ApiService apiService)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _apiService = apiService;

            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Preview = query.TryGetValue("preview", out var value)
                && string.Equals(
                    value?.ToString(),
                    "true",
                    StringComparison.OrdinalIgnoreCase)
                ? "true"
                : "false";

            Debug.WriteLine(
                $"DASHBOARD DIAGNOSTIC: Preview = {Preview}");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Debug.WriteLine(
                $"DASHBOARD DIAGNOSTIC: OnAppearing, Preview = {Preview}");

            try
            {
                if (Preview == "true")
                {
                    _viewModel.LoadPreviewProjects();

                    Debug.WriteLine(
                        $"DASHBOARD DIAGNOSTIC: Preview project count = {_viewModel.Projects.Count}");
                }
                else
                {
                    await _viewModel.LoadProjectsAsync();

                    Debug.WriteLine(
                        $"DASHBOARD DIAGNOSTIC: API project count = {_viewModel.Projects.Count}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"DASHBOARD LOADING ERROR: {ex.Message}");
            }
        }

        // Opens the New Project page.
        private async void OnNewProjectClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewProjectPage));
        }

        // Opens Projects with the Join prompt.
        private async void OnJoinWithCodeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(
                "///ProjectsPage?openJoinPrompt=true");
        }

        // Opens the Projects page and displays all projects.
        private async void OnViewAllProjectsTapped(
            object sender,
            TappedEventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("///ProjectsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"VIEW ALL PROJECTS ERROR: {ex.Message}");

                await DisplayAlertAsync(
                    "Navigation error",
                    "Unable to open all projects. Please try again.",
                    "OK");
            }
        }

        // Opens the selected project.
        private async void OnProjectTapped(
            object sender,
            TappedEventArgs e)
        {
            if (sender is BindableObject view &&
                view.BindingContext is ProjectModel project)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(TaskBoardPage)}?projectId={project.ProjectId}");
            }
        }

        // Opens the relevant project chat from a notification.
        private async void OnNotificationTapped(
            object sender,
            TappedEventArgs e)
        {
            if (sender is not BindableObject view ||
                view.BindingContext is not NotificationModel notification)
            {
                return;
            }

            if (notification.NotificationType != "CHAT_MESSAGE" ||
                notification.ProjectId <= 0)
            {
                return;
            }

            if (_apiService.CurrentUserId <= 0)
            {
                await DisplayAlertAsync(
                    "Login required",
                    "Please log in again to open this chat.",
                    "OK");

                return;
            }

            try
            {
                DashboardViewModel.MarkChatNotificationsAsRead(
                    _apiService.CurrentUserId,
                    notification.ProjectId,
                    notification.MessageId);

                notification.IsRead = true;

                await Shell.Current.GoToAsync(
                    $"{nameof(ChatPage)}?projectId={notification.ProjectId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"CHAT NOTIFICATION NAVIGATION ERROR: {ex.Message}");

                await DisplayAlertAsync(
                    "Unable to open chat",
                    "The chat could not be opened. Please try again.",
                    "OK");
            }
        }

        // Bottom navigation.
        private async void OnTabSelected(object? sender, string tab)
        {
            await TabNavigator.GoToTabAsync(
                this,
                tab,
                current: "Home");
        }

        // Menu navigation.
        private async void OnMenuItemSelected(
            object? sender,
            string item)
        {
            if (item == "Logout")
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            if (item == "MyAccount")
            {
                await Shell.Current.GoToAsync("//MyAccountPage");
                return;
            }

            await DisplayAlertAsync(
                "Coming soon",
                $"{item} isn't built yet.",
                "OK");
        }
    }
}
