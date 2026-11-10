
using System;
using System.Collections.Generic;
using System.Linq;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.Views
{
    public partial class CreateTaskPage : ContentPage, IQueryAttributable
    {
        private readonly ApiService _apiService;
        private long _projectId;
        private bool _isSaving;
        private bool _membersLoaded;
        private bool _isLoadingMembers;

        private List<ProjectMemberModel> _projectMembers = new();

        public CreateTaskPage(ApiService apiService)
        {
            InitializeComponent();

            _apiService = apiService;

            // Set the earliest selectable due date to today.
            DueDatePicker.MinimumDate = DateTime.Today;
            DueDatePicker.Date = DateTime.Today;

            // Members must load before a task can be assigned.
            AssignedToPicker.IsEnabled = false;
        }

        // Receives the project ID passed from the Task Board.
        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("projectId", out var projectIdValue)
                && long.TryParse(projectIdValue?.ToString(), out long projectId))
            {
                _projectId = projectId;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_membersLoaded && !_isLoadingMembers)
            {
                await LoadProjectMembersAsync();
            }
        }

        // Load the members belonging to this project.
        private async Task LoadProjectMembersAsync()
        {
            if (_projectId <= 0)
            {
                await DisplayAlertAsync(
                    "Project unavailable",
                    "The project ID is missing. Return to the Task Board and try again.",
                    "OK");

                return;
            }

            _isLoadingMembers = true;
            AssignedToPicker.IsEnabled = false;
            AssignedToPicker.Title = "Loading project members...";

            try
            {
                var members =
                    await _apiService.GetProjectMembersAsync(_projectId);

                _projectMembers = members?
                    .Where(member =>
                        member.Student != null &&
                        member.Student.UserId > 0)
                    .ToList() ?? new List<ProjectMemberModel>();

                if (_projectMembers.Count == 0)
                {
                    AssignedToPicker.Title = "No project members found";

                    await DisplayAlertAsync(
                        "No members found",
                        "There are no available project members to assign this task to.",
                        "OK");

                    return;
                }

                // Display members by their full names.
                AssignedToPicker.ItemsSource = _projectMembers;
                AssignedToPicker.ItemDisplayBinding =
                    new Binding(nameof(ProjectMemberModel.FullName));

                AssignedToPicker.Title = "Select a project member";
                AssignedToPicker.IsEnabled = true;

                // Automatically select the logged-in student.
                var currentMember = _projectMembers.FirstOrDefault(
                    member => member.Student.UserId == _apiService.CurrentUserId);

                if (currentMember != null)
                {
                    AssignedToPicker.SelectedItem = currentMember;
                }

                _membersLoaded = true;
            }
            catch (Exception ex)
            {
                AssignedToPicker.Title = "Unable to load members";

                await DisplayAlertAsync(
                    "Unable to load project members",
                    $"Project members could not be loaded. Please check the backend connection and try again.\n\nDetails: {ex.Message}",
                    "OK");
            }
            finally
            {
                _isLoadingMembers = false;
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

            if (AssignedToPicker.SelectedItem is not ProjectMemberModel selectedMember
                || selectedMember.Student == null
                || selectedMember.Student.UserId <= 0)
            {
                await DisplayAlertAsync(
                    "Missing assignment",
                    "Please select a project member to assign this task to.",
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

            if (!_membersLoaded)
            {
                await DisplayAlertAsync(
                    "Members unavailable",
                    "Project members have not loaded. Please try again.",
                    "OK");
                return;
            }

            _isSaving = true;
            SaveTaskButton.IsEnabled = false;
            SaveTaskButton.Text = "Saving...";

            try
            {
                var request = new CreateTaskRequest
                {
                    TaskTitle = title,
                    TaskDescription = description,
                    Priority = priority,
                    DueDate = DueDatePicker.Date,

                    // Send the actual student ID, not the membership ID.
                    AssignedTo = selectedMember.Student.UserId
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
