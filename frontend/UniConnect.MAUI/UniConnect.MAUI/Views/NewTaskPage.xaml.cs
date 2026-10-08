using UniConnect.MAUI.Models;

namespace UniConnect.MAUI.Views
{
    public partial class NewTaskPage : ContentPage
    {
        public NewTaskPage()
        {
            InitializeComponent();

            // Default values for a new task.
            StartDatePicker.Date = DateTime.Today;
            DueDatePicker.Date = DateTime.Today.AddDays(7);

            PriorityPicker.SelectedIndex = 1; // Medium
            StatusPicker.SelectedIndex = 0;   // Not Started
        }

        private async void OnCreateTaskClicked(object sender, EventArgs e)
        {
            // Basic validation.
            if (string.IsNullOrWhiteSpace(TaskTitleEntry.Text))
            {
                await DisplayAlertAsync(
                    "Missing Information",
                    "Please enter a task title.",
                    "OK");

                return;
            }

            if (DueDatePicker.Date < StartDatePicker.Date)
            {
                await DisplayAlertAsync(
                    "Invalid Dates",
                    "The due date cannot be before the start date.",
                    "OK");

                return;
            }

            // Convert the user-friendly progress text
            // to the status values currently used by the app.
            string status = StatusPicker.SelectedItem?.ToString() switch
            {
                "Not Started" => "NOT_STARTED",
                "In Progress" => "IN_PROGRESS",
                "Completed" => "DONE",
                _ => "NOT_STARTED"
            };

            // Build the task request.
            // This will later be sent through ApiService
            // once the backend Task endpoint is confirmed.
            var taskRequest = new CreateTaskRequest
            {
                TaskTitle = TaskTitleEntry.Text.Trim(),
                TaskDescription = TaskDescriptionEditor.Text?.Trim(),
                Priority = PriorityPicker.SelectedItem?.ToString() ?? "Medium",
                StartDate = StartDatePicker.Date,
                DueDate = DueDatePicker.Date,

                // The actual member ID will be connected
                // when the project members are loaded from the backend.
                AssignedTo = null
            };

            // Keep the selected status ready for the backend integration.
            // The request currently does not contain Status because
            // the backend CreateTask contract has not been confirmed yet.
            _ = status;

            await DisplayAlertAsync(
                "Task Ready",
                "The task form is complete. Backend creation will be connected once the Task API is confirmed.",
                "OK");
        }
    }
}
