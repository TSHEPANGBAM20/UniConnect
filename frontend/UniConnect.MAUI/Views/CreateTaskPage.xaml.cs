
using System;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.Views
{
    public partial class CreateTaskPage : ContentPage, IQueryAttributable
    {
        private readonly ApiService _apiService;
        private long _projectId;
        private bool _isSaving;

        public CreateTaskPage(ApiService apiService)
        {
            InitializeComponent();

            _apiService = apiService;

            // Set the earliest selectable due date to today.
            DueDatePicker.MinimumDate = DateTime.Today;
            DueDatePicker.Date = DateTime.Today;
        }

        // Receives the project ID passed from the Task Board.
        public void ApplyQueryAttributes(
            System.Collections.Generic.IDictionary<string, object> query)
        {
            if (query.TryGetValue("projectId", out var projectIdValue)
                && long.TryParse(projectIdValue?.ToString(), out long projectId))
            {
                _projectId = projectId;
            }
        }

        private async void OnSaveTaskClicked(object? sender, EventArgs e)
        {
            if (_isSaving)
                return;

            string title = TaskTitleEntry.Text?.Trim() ?? string.Empty;
            string description = TaskDescriptionEditor.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlertAsync(
                    "Missing title",
                    "Please enter a task title.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                await DisplayAlertAsync(
                    "Missing description",
                    "Please enter a task description.",
                    "OK");
                return;
            }

            if (PriorityPicker.SelectedItem is not string priority)
            {
                await DisplayAlertAsync(
                    "Missing priority",
                    "Please select a task priority.",
                    "OK");
                return;
            }

            if (_projectId <= 0)
            {
                await DisplayAlertAsync(
                    "Project unavailable",
                    "The project ID is missing. Return to the Task Board and try again.",
                    "OK");
                return;
            }

            _isSaving = true;
            SaveTaskButton.IsEnabled = false;
            SaveTaskButton.Text = "Saving...";

            try
            {
               
                //  return the created task or an error.
                var request = new CreateTaskRequest
                {
                    TaskTitle = title,
                    TaskDescription = description,
                    Priority = priority,
                    DueDate = DueDatePicker.Date
                };

                await _apiService.CreateTaskAsync(_projectId, request);

                await DisplayAlertAsync(
                    "Task saved",
                    "The task was saved successfully.",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Unable to save task",
                    $"The task could not be saved. Please check the backend connection and try again.\n\nDetails: {ex.Message}",
                    "OK");
            }
            finally
            {
                _isSaving = false;
                SaveTaskButton.IsEnabled = true;
                SaveTaskButton.Text = "Save Task";
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
