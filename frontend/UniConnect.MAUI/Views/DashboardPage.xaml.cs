using System.Diagnostics;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class DashboardPage : ContentPage, IQueryAttributable
    {
        private readonly DashboardViewModel _viewModel;

        public string Preview { get; set; } = "false";

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
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

            Debug.WriteLine($"DASHBOARD DIAGNOSTIC: Preview = {Preview}");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Debug.WriteLine(
                $"DASHBOARD DIAGNOSTIC: OnAppearing, Preview = {Preview}");

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

        private async void OnNewProjectClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewProjectPage));
        }

        private async void OnProjectTapped(object sender, TappedEventArgs e)
        {
            if (sender is BindableObject view &&
                view.BindingContext is ProjectModel project)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(TaskBoardPage)}?projectId={project.ProjectId}");
            }
        }

        private async void OnTabSelected(object? sender, string tab)
            => await TabNavigator.GoToTabAsync(this, tab, current: "Home");

        private async void OnMenuItemSelected(object? sender, string item)
        {
            if (item == "Logout")
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            if (item == "MyAccount")
            {
               await Shell.Current.GoToAsync("//MyAccountPage");
               return ;
            }
            

            
            
            this.DisplayAlertAsync(
                "Coming soon",
                $"{item} isn't built yet.",
                "OK");
        }
    }
}