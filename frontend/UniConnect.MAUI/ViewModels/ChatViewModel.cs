
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    // Sends new messages and keeps the screen updated when something changes.
    [QueryProperty(nameof(ProjectId), "projectId")]
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isBusy;
        private bool _isSending;
        private string _draftMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private long _projectId;

        // ID of the project whose chat the user is opening.
        public long ProjectId
        {
            get => _projectId;
            set
            {
                if (_projectId == value)
                    return;

                _projectId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        // Stores the messages shown in the chat.
        public ObservableCollection<MessageModel> Messages { get; } = new();

        // Stores the message the user is typing.
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

        // Stores an error message if loading or sending fails.
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage == value)
                    return;

                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // Tracks whether messages are loading.
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        // Tracks whether a message is being sent.
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

        // Checks whether the user can send a message.
        public bool CanSend =>
            !IsBusy &&
            !IsSending &&
            ProjectId > 0 &&
            !string.IsNullOrWhiteSpace(DraftMessage);

        public ChatViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Assigns bubble colours according to the sender.
        // The same sender always receives the same colour.
        private void AssignBubbleColours(List<MessageModel> messages)
        {
            var senderColours = new Dictionary<long, int>();

            // The logged-in user's messages are always black.
            senderColours[_apiService.CurrentUserId] = 0;

            int nextColour = 1;

            foreach (var message in messages)
            {
                message.IsOwnMessage =
                    message.SenderId == _apiService.CurrentUserId;

                // Check whether this sender already has a colour.
                if (!senderColours.ContainsKey(message.SenderId))
                {
                    // Assign the next participant colour.
                    senderColours[message.SenderId] = nextColour;

                    // Alternate between white and grey for other participants.
                    nextColour = nextColour == 1 ? 2 : 1;
                }

                message.BubbleColorIndex =
                    senderColours[message.SenderId];
            }
        }

        // Loads messages for the selected project.
        public async Task LoadMessagesAsync()
        {
            if (ProjectId <= 0 || IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var messages =
                    await _apiService.GetMessagesAsync(ProjectId);

                AssignBubbleColours(messages);

                Messages.Clear();

                foreach (var message in messages)
                    Messages.Add(message);
            }
            catch (Exception)
            {
                ErrorMessage =
                    "Couldn't load messages. Check the backend connection and try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Sends the message typed by the user.
        public async Task SendAsync()
        {
            if (!CanSend)
                return;

            var text = DraftMessage.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            IsSending = true;
            ErrorMessage = string.Empty;

            try
            {
                await _apiService.SendMessageAsync(ProjectId, text);

                // Clear the input after a successful send.
                DraftMessage = string.Empty;

                // Reload messages to display the latest version.
                await RefreshMessagesAfterSendAsync();
            }
            catch (Exception)
            {
                ErrorMessage =
                    "Couldn't send the message. Check the backend connection and try again.";

                throw;
            }
            finally
            {
                IsSending = false;
            }
        }

        // Reloads messages after sending.
        private async Task RefreshMessagesAfterSendAsync()
        {
            try
            {
                var messages =
                    await _apiService.GetMessagesAsync(ProjectId);

                AssignBubbleColours(messages);

                Messages.Clear();

                foreach (var message in messages)
                    Messages.Add(message);
            }
            catch (Exception)
            {
                ErrorMessage =
                    "The message was submitted, but the chat couldn't refresh.";
            }
        }

        // Notifies the screen when a property changes.
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}
