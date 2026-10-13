using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    [QueryProperty(nameof(ProjectId), "projectId")]
    public class TaskBoardViewModel : INotifyPropertyChanged
    {
        // DEV SWITCH: the backend has no task endpoints yet. While true, a failed call
        // falls back to sample data (and "Done" works locally) so the UI can be tested.
        // Set to false once the API works.
        private const bool UseSampleDataOnFailure = true;

        private readonly ApiService _apiService;

        public int ProjectId { get; set; }

        public ObservableCollection<TaskModel> Tasks { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public TaskBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadTasksAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            List<TaskModel> tasks;
            try
            {
                if (ProjectId == 0) throw new InvalidOperationException("No project selected");
                tasks = await _apiService.GetTasksAsync(ProjectId);
            }
            catch (Exception)
            {
                tasks = UseSampleDataOnFailure ? SampleTasks() : new List<TaskModel>();
                if (!UseSampleDataOnFailure)
                    ErrorMessage = "Couldn't load tasks. Check your connection and try again.";
            }

            Tasks.Clear();
            foreach (var t in tasks)
                Tasks.Add(t);

            IsBusy = false;
        }

        public async Task MarkDoneAsync(TaskModel task)
        {
            ErrorMessage = string.Empty;
            try
            {
                await _apiService.UpdateTaskStatusAsync(task.TaskId, "DONE");
            }
            catch (Exception)
            {
                if (!UseSampleDataOnFailure)
                {
                    ErrorMessage = "Couldn't update the task. Try again.";
                    return;
                }
                // sample mode: pretend the server accepted it
            }

            task.Status = "DONE";

            // TaskModel doesn't raise change notifications, so re-insert the item
            // to make the card redraw as completed (button hidden, tick shown).
            int index = Tasks.IndexOf(task);
            if (index >= 0) Tasks[index] = task;
        }

        private static List<TaskModel> SampleTasks() => new()
        {
            new TaskModel { TaskId = 1, ProjectId = 1, TaskTitle = "Design the login screen", Status = "NOT_STARTED", AssignedToName = "Tshepang", DueDate = DateTime.Today.AddDays(3) },
            new TaskModel { TaskId = 2, ProjectId = 1, TaskTitle = "Set up the MySQL database", Status = "IN_PROGRESS", AssignedToName = "Koketso", DueDate = DateTime.Today.AddDays(5) },
            new TaskModel { TaskId = 3, ProjectId = 1, TaskTitle = "Write the API contract", Status = "DONE", AssignedToName = "Molebogeng", DueDate = DateTime.Today.AddDays(-1) },
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
