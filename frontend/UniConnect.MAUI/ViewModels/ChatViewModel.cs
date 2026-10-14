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
        private bool _isBusy;
        private bool _isSending;
        private string _draftMessage = string.Empty;

        public int ProjectId { get; set; }

        public ObservableCollection<MessageModel> Messages { get; } = new();

        public string DraftMessage
        {
            get => _draftMessage;
            set
            {
                if (_draftMessage == value)
                    return;

                _draftMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        public bool IsSending
        {
            get => _isSending;
            private set
            {
                if (_isSending == value)
                    return;

                _isSending = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        public bool CanSend =>
            !IsBusy &&
            !IsSending &&
            !string.IsNullOrWhiteSpace(DraftMessage);

        public ChatViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadMessagesAsync()
        {
            if (ProjectId == 0 || IsBusy)
                return;

            IsBusy = true;

            try
            {
                var messages = await _apiService.GetMessagesAsync(ProjectId);

                Messages.Clear();

                foreach (var message in messages)
                    Messages.Add(message);
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
            if (!CanSend || ProjectId == 0)
                return;

            var text = DraftMessage.Trim();

            IsSending = true;

            try
            {
                await _apiService.SendMessageAsync(ProjectId, text);

                DraftMessage = string.Empty;

                await LoadMessagesAsync();
            }
            finally
            {
                IsSending = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}