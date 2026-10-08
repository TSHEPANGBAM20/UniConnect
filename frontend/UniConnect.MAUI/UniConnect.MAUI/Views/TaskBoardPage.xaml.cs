using UniConnect.MAUI.Models;
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

        private async void OnChatClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(
                $"{nameof(ChatPage)}?projectId={_viewModel.ProjectId}");
        }

        private async void OnMarkDoneClicked(object sender, EventArgs e)
        {
            if (sender is Button button &&
                button.BindingContext is TaskModel task)
            {
                try
                {
                    await _viewModel.MarkDoneAsync(task);

                    // Hide the button after the task is successfully marked as done.
                    button.IsVisible = false;
                }
                catch (Exception)
                {
                    await DisplayAlertAsync(
                        "Error",
                        "Could not mark the task as done. Please try again.",
                        "OK");
                }
            }
        }
    }
}
