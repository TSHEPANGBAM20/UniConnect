using UniConnect.MAUI.Models;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProjectsAsync();
        }

        private async void OnNewProjectClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewProjectPage));
        }

        private async void OnProjectTapped(object sender, TappedEventArgs e)
        {
            if (sender is TapGestureRecognizer tap && tap.BindingContext is ProjectModel project)
            {
                await Shell.Current.GoToAsync($"{nameof(TaskBoardPage)}?projectId={project.ProjectId}");
            }
        }
    }
}
