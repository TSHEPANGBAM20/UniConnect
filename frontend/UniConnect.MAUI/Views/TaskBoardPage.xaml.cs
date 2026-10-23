
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class TaskBoardPage : ContentPage, IQueryAttributable
    {
        private readonly TaskBoardViewModel _viewModel;
        private bool _drawerOpen;
        private bool _drawerAnimating;
        private bool _initialLoadCompleted;
        private bool _refreshTasksOnAppearing;

        public TaskBoardPage(TaskBoardViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Receives the selected project from ProjectsPage.
            if (query.TryGetValue("SelectedProject", out var selectedProject)
                && selectedProject is ProjectModel project)
            {
                _viewModel.ProjectId = project.ProjectId;
                _initialLoadCompleted = false;
            }

            // Also supports navigation that passes only a project ID.
            if (query.TryGetValue("projectId", out var projectIdValue))
            {
                if (long.TryParse(projectIdValue?.ToString(), out long projectId))
                {
                    _viewModel.ProjectId = projectId;
                    _initialLoadCompleted = false;
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load initially, or reload after returning from task creation.
            if (_initialLoadCompleted && !_refreshTasksOnAppearing)
                return;

            _initialLoadCompleted = true;
            _refreshTasksOnAppearing = false;

            await _viewModel.LoadTasksAsync();
        }

        private async void OnDoneClicked(object? sender, EventArgs e)
        {
            if (sender is Button button &&
                button.BindingContext is TaskModel task)
            {
                //Updates this task's status to DONE.
                
                await _viewModel.MarkDoneAsync(task);
            }
        }

        private async void OnMenuTapped(object? sender, TappedEventArgs e)
        {
            await OpenDrawerAsync();
        }

        private async void OnCloseDrawerClicked(object? sender, EventArgs e)
        {
            await CloseDrawerAsync();
        }

        private async void OnDrawerBackdropTapped(object? sender, TappedEventArgs e)
        {
            await CloseDrawerAsync();
        }

        private async Task OpenDrawerAsync()
        {
            if (_drawerOpen || _drawerAnimating)
                return;

            _drawerAnimating = true;

            try
            {
                ProjectDrawerOverlay.IsVisible = true;
                ProjectDrawer.TranslationX = -320;
                DrawerBackdrop.Opacity = 0;

                await Task.WhenAll(
                    ProjectDrawer.TranslateToAsync(0, 0, 280, Easing.CubicOut),
                    DrawerBackdrop.FadeToAsync(0.35, 220, Easing.CubicOut));

                _drawerOpen = true;
            }
            finally
            {
                _drawerAnimating = false;
            }
        }

        private async Task CloseDrawerAsync()
        {
            if (!_drawerOpen || _drawerAnimating)
                return;

            _drawerAnimating = true;

            try
            {
                await Task.WhenAll(
                    ProjectDrawer.TranslateToAsync(-320, 0, 220, Easing.CubicIn),
                    DrawerBackdrop.FadeToAsync(0, 180, Easing.CubicIn));

                ProjectDrawerOverlay.IsVisible = false;
                _drawerOpen = false;
            }
            finally
            {
                _drawerAnimating = false;
            }
        }


                private async void OnProjectMenuItemClicked(object? sender, EventArgs e){
                    if (sender is not Button button ||button.CommandParameter is not string choice)
                    return;
                    await CloseDrawerAsync();

string projectId = Uri.EscapeDataString(
    _viewModel.ProjectId.ToString());

switch (choice)
{
    case "Tasks":
        // Already on the Task Board.
        break;

        case "Project Details":
        if (_viewModel.ProjectId <= 0)
                    {
                      await DisplayAlertAsync(
            "Coming soon",
            "Project Details is not available yet.",
            "OK"); 
            return;
            }
            //passess projectId to Specific project details
            await Shell.Current.GoToAsync(
            $"{nameof(ProjectDetailsPage)}?projectId={projectId}");
             break;

        
        

    case "Project Chat":
        if (_viewModel.ProjectId <= 0)
        {
            await DisplayAlertAsync(
                "Project unavailable",
                "The selected project ID is missing. Please return to Projects and open the project again.",
                "OK");
            return;
        }

        // Passes the selected project ID to ChatPage.
        await Shell.Current.GoToAsync(
            $"{nameof(ChatPage)}?projectId={projectId}");
        break;
}


}


        private async void OnAddTaskTapped(object? sender, TappedEventArgs e)
        {
            if (_viewModel.ProjectId <= 0)
            {
                await DisplayAlertAsync(
                    "Project unavailable",
                    "The selected project ID is missing. Return to Projects and open the project again.",
                    "OK");
                return;
            }

            // CreateTaskPage receives the selected project ID.
        
            _refreshTasksOnAppearing = true;

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(CreateTaskPage)}?projectId={_viewModel.ProjectId}");
            }
            catch (Exception ex)
            {
                _refreshTasksOnAppearing = false;

                await DisplayAlertAsync(
                    "Unable to open task form",
                    $"The Add New Task page could not be opened.\n\n{ex.Message}",
                    "OK");
            }
        }

        private async void OnTabSelected(object? sender, string tab)
        {
            await TabNavigator.GoToTabAsync(this, tab, current: "");
        }

        private async void OnMenuItemSelected(object? sender, string item)
        {
            await TabNavigator.HandleMenuItemAsync(this, item);
        }
    }
}
