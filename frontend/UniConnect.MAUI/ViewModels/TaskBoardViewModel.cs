
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
        private static readonly bool UseSampleDataOnFailure = true;

        private readonly ApiService _apiService;

        public long ProjectId { get; set; }

        public ObservableCollection<TaskModel> Tasks { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isTeamLeader;
        public bool IsTeamLeader
        {
            get => _isTeamLeader;
            private set
            {
                _isTeamLeader = value;
                OnPropertyChanged();
            }
        }

        public TaskBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadTasksAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (ProjectId <= 0)
                    throw new InvalidOperationException(
                        "No project selected.");

                // Load project members.
                var members =
                    await _apiService.GetProjectMembersAsync(ProjectId);

                // Identify the logged-in user's project role.
                IsTeamLeader = members.Any(member =>
                    member.Student != null &&
                    member.Student.UserId == _apiService.CurrentUserId &&
                    RoleNames.IsLeader(member.Role));

                // Load project tasks.
                var tasks =
                    await _apiService.GetTasksAsync(ProjectId);

                // Match each assigned student ID to their project member.
                foreach (var task in tasks)
                {
                    if (task.AssignedTo.HasValue)
                    {
                        var assignedMember = members.FirstOrDefault(member =>
                            member.Student != null &&
                            member.Student.UserId == task.AssignedTo.Value);

                        if (assignedMember?.Student != null)
                        {
                            task.AssignedToName =
                                $"{assignedMember.Student.Name} {assignedMember.Student.Surname}".Trim();
                        }
                        else
                        {
                            task.AssignedToName = "Unknown member";
                        }
                    }
                    else
                    {
                        task.AssignedToName = "Not assigned";
                    }
                }

                // Update the displayed task collection.
                Tasks.Clear();

                foreach (var task in tasks)
                    Tasks.Add(task);
            }
            catch (Exception)
            {
                IsTeamLeader = false;

                if (UseSampleDataOnFailure)
                {
                    Tasks.Clear();

                    foreach (var task in SampleTasks())
                        Tasks.Add(task);
                }
                else
                {
                    Tasks.Clear();
                    ErrorMessage =
                        "Couldn't load tasks or project permissions. Check your connection and try again.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task MarkDoneAsync(TaskModel task)
        {
            ErrorMessage = string.Empty;

            try
            {
                await _apiService.UpdateTaskStatusAsync(
                    task.TaskId, "DONE");
            }
            catch (Exception)
            {
                if (!UseSampleDataOnFailure)
                {
                    ErrorMessage =
                        "Couldn't update the task. Try again.";
                    return;
                }

                // Sample mode: simulate a successful update.
            }

            task.Status = "DONE";

            int index = Tasks.IndexOf(task);

            if (index >= 0)
                Tasks[index] = task;
        }

        private static List<TaskModel> SampleTasks() => new()
        {
            new TaskModel
            {
                TaskId = 1,
                Project = new ProjectModel { ProjectId = 1 },
                TaskTitle = "Design the login screen",
                TaskDescription = "Create the login page layout and validation.",
                Priority = "HIGH",
                Status = "NOT_STARTED",
                AssignedToName = "Tshepang",
                DueDate = DateTime.Today.AddDays(3)
            },
            new TaskModel
            {
                TaskId = 2,
                Project = new ProjectModel { ProjectId = 1 },
                TaskTitle = "Set up the MySQL database",
                TaskDescription = "Configure the database connection.",
                Priority = "MEDIUM",
                Status = "IN_PROGRESS",
                AssignedToName = "Koketso",
                DueDate = DateTime.Today.AddDays(5)
            },
            new TaskModel
            {
                TaskId = 3,
                Project = new ProjectModel { ProjectId = 1 },
                TaskTitle = "Write the API contract",
                TaskDescription = "Document the backend API endpoints.",
                Priority = "LOW",
                Status = "DONE",
                AssignedToName = "Molebogeng",
                DueDate = DateTime.Today.AddDays(-1)
            }
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(name));
        }
    }
}
