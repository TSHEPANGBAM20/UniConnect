
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    // This ViewModel handles the chat screen. It loads messages for a project,
    // sends new messages and keeps the screen updated when something changes.
    [QueryProperty(nameof(ProjectId), "projectId")]
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isBusy;
        private bool _isSending;
        private string _draftMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private long _projectId;

        // This is the ID of the project whose chat the user is opening.
        // It is passed to this page when the user navigates to the chat screen.
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

        // This collection stores the messages shown in the chat.
        // When messages are added or removed, the screen can update automatically.
        public ObservableCollection<MessageModel> Messages { get; } = new();

        // This stores the message the user is busy typing.
        // When it changes, CanSend is checked again to see if the send button should be enabled.
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

        // This stores an error message if loading or sending messages does not work.
        // The ChatPage uses it to show the user what went wrong.
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

        // IsBusy is used while the chat messages are loading.
        // It also helps stop the user from trying to send a message at the same time.
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

        // IsSending tracks whether a message is currently being sent.
        // This helps prevent the user from submitting the same message repeatedly.
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

        // The user can only send a message if a project is selected, the message is not empty,
        // and the app is not already loading messages or sending another message.
        public bool CanSend =>
            !IsBusy &&
            !IsSending &&
            ProjectId > 0 &&
            !string.IsNullOrWhiteSpace(DraftMessage);

        // ApiService is used to communicate with the backend, keeping the API requests
        // separate from the code that controls what happens on the chat screen.
        public ChatViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // This loads the messages for the selected project and displays them in the chat.
        // If something goes wrong, an error is saved and the loading state is reset.
        //
        // BACKEND NOTE: GetMessagesAsync(ProjectId) must retrieve messages for the correct
        // project. We need to agree on the API route and response format, including the message
        // details needed by MessageModel and how the messages will be ordered.
        public async Task LoadMessagesAsync()
        {
            if (ProjectId <= 0 || IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var messages = await _apiService.GetMessagesAsync(ProjectId);

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

        // This sends the text the user typed for the selected project's chat.
        // The message is trimmed first, and the input is only cleared after the send request
        // succeeds. The chat is then refreshed to show the latest messages.
        //
        // BACKEND NOTE: SendMessageAsync(ProjectId, text) needs to send the message to the
        // correct project. The backend should identify the sender from the logged-in user,
        // validate the message and return a response that lets the frontend know if sending
        // succeeded or failed. We still need to agree on the request format and API route.
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

                // Clear the input only after the send request succeeds.
                DraftMessage = string.Empty;

                // Reload the chat so the latest messages can be displayed.
                // If refreshing fails, it does not necessarily mean sending failed.
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

        // This reloads the messages after sending so the chat can show the latest version
        // returned by the backend. A refresh error is handled separately because the message
        // might already have been sent successfully.
        //
        // BACKEND NOTE: Make sure the message appears in the history returned by
        // GetMessagesAsync(ProjectId) after it has been saved. Real-time updates are not
        // implemented here; the current approach is to request the messages again.
        private async Task RefreshMessagesAfterSendAsync()
        {
            try
            {
                var messages = await _apiService.GetMessagesAsync(ProjectId);

                Messages.Clear();

                foreach (var message in messages)
                    Messages.Add(message);
            }
            catch (Exception)
            {
                // Sending may have worked even if reloading the messages failed.
                // The chat can try loading the messages again later.
                ErrorMessage =
                    "The message was submitted, but the chat couldn't refresh.";
            }
        }

        // This notifies the screen when a property changes so that the displayed information
        // stays up to date without needing to reopen the page.
        public event PropertyChangedEventHandler? PropertyChanged;

        // This raises the notification for whichever property has changed.
        // CallerMemberName automatically gets the property name when it is called.
        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}
