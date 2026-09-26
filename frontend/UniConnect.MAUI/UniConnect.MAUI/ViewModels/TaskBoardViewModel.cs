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
        private readonly ApiService _apiService;

        public int ProjectId { get; set; }

        public ObservableCollection<TaskModel> Tasks { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public TaskBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadTasksAsync()
        {
            if (ProjectId == 0) return;

            IsBusy = true;
            try
            {
                var tasks = await _apiService.GetTasksAsync(ProjectId);
                Tasks.Clear();
                foreach (var t in tasks)
                    Tasks.Add(t);
            }
            catch (Exception)
            {
                // TODO: friendly error banner
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task MarkDoneAsync(TaskModel task)
        {
            await _apiService.UpdateTaskStatusAsync(task.TaskId, "DONE");
            task.Status = "DONE";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
