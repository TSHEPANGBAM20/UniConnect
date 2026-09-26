using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    [QueryProperty(nameof(ProjectId), "projectId")]
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public int ProjectId { get; set; }

        public ObservableCollection<MessageModel> Messages { get; } = new();

        public string DraftMessage { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ChatViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadMessagesAsync()
        {
            if (ProjectId == 0) return;

            IsBusy = true;
            try
            {
                var messages = await _apiService.GetMessagesAsync(ProjectId);
                Messages.Clear();
                foreach (var m in messages)
                    Messages.Add(m);
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

        public async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(DraftMessage)) return;

            var text = DraftMessage;
            DraftMessage = string.Empty;
            OnPropertyChanged(nameof(DraftMessage));

            await _apiService.SendMessageAsync(ProjectId, text);
            await LoadMessagesAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
