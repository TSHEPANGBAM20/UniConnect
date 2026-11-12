
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public ObservableCollection<ProjectModel> Projects { get; } = new();

        public ObservableCollection<NotificationModel> Notifications { get; } = new();

        public string WelcomeMessage =>
            $"Welcome Back, {_apiService.CurrentUserName ?? "User"} {_apiService.CurrentUserSurname ?? string.Empty}".TrimEnd();

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmptyProjects));
                OnPropertyChanged(nameof(ShowEmptyNotifications));
            }
        }

        private bool _hasProjects;

        public bool HasProjects
        {
            get => _hasProjects;
            set
            {
                _hasProjects = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmptyProjects));
            }
        }

        public bool ShowEmptyProjects => !HasProjects && !IsBusy;

        private bool _hasNotifications;

        public bool HasNotifications
        {
            get => _hasNotifications;
            set
            {
                _hasNotifications = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmptyNotifications));
            }
        }

        public bool ShowEmptyNotifications => !HasNotifications && !IsBusy;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Loads projects and combines task, project and chat notifications.
        public async Task LoadProjectsAsync()
        {
            IsBusy = true;

            OnPropertyChanged(nameof(WelcomeMessage));

            try
            {
                Projects.Clear();

                try
                {
                    // BACKEND:
                    // GET projects belonging to the logged-in user.

                    var projects = await _apiService.GetProjectsAsync(
                        _apiService.CurrentUserId);

                    foreach (var project in projects ?? new List<ProjectModel>())
                    {
                        Projects.Add(project);
                    }

                    HasProjects = Projects.Count > 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PROJECT LOADING ERROR: {ex.Message}");
                    HasProjects = false;
                }

                await LoadDeadlineNotificationsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Combines backend task notifications with frontend project
        // and incoming chat notifications.
        private async Task LoadDeadlineNotificationsAsync()
        {
            Notifications.Clear();

            var combinedNotifications = new List<NotificationModel>();

            // 1. Retrieve task deadline notifications from the backend.
            try
            {
                if (_apiService.CurrentUserId > 0)
                {
                    // BACKEND:
                    // GET /tasks/notifications/{studentId}
                    // Expected: OVERDUE, DUE_TODAY and DUE_TOMORROW.

                    var taskNotifications =
                        await _apiService.GetNotificationsAsync();

                    if (taskNotifications != null)
                    {
                        combinedNotifications.AddRange(taskNotifications);
                    }

                    Debug.WriteLine(
                        $"TASK NOTIFICATIONS: {taskNotifications?.Count ?? 0} retrieved.");
                }
                else
                {
                    Debug.WriteLine("No logged-in student ID found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TASK NOTIFICATION ERROR: {ex.Message}");
            }

            // 2. Generate project deadline notifications.
            combinedNotifications.AddRange(
                BuildProjectDeadlineNotifications());

            // 3. Check for new incoming chat messages.
            await LoadChatNotificationsAsync(combinedNotifications);

            // 4. Sort notifications by date, newest first.
            foreach (var notification in combinedNotifications
                         .OrderByDescending(n => n.DueDate))
            {
                Notifications.Add(notification);
            }

            HasNotifications = Notifications.Count > 0;

            Debug.WriteLine(
                $"TOTAL NOTIFICATIONS: {Notifications.Count}");
        }

        // Creates notifications for projects due yesterday or earlier,
        // today, or tomorrow.
        private List<NotificationModel> BuildProjectDeadlineNotifications()
        {
            var projectNotifications = new List<NotificationModel>();

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly tomorrow = today.AddDays(1);

            foreach (var project in Projects)
            {
                string status = project.ProjectStatus?.Trim() ?? string.Empty;

                if (status.Equals("DONE", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string notificationType;
                string title;
                string message;

                if (project.EndDate < today)
                {
                    notificationType = "PROJECT_OVERDUE";
                    title = "Project overdue";
                    message = $"{project.ProjectName} has passed its deadline.";
                }
                else if (project.EndDate == today)
                {
                    notificationType = "PROJECT_DUE_TODAY";
                    title = "Project due today";
                    message = $"{project.ProjectName} is due today.";
                }
                else if (project.EndDate == tomorrow)
                {
                    notificationType = "PROJECT_DUE_TOMORROW";
                    title = "Project due tomorrow";
                    message = $"{project.ProjectName} is due tomorrow.";
                }
                else
                {
                    continue;
                }

                projectNotifications.Add(new NotificationModel
                {
                    NotificationId = -project.ProjectId,
                    ProjectId = project.ProjectId,
                    ProjectName = project.ProjectName,
                    Title = title,
                    Message = message,
                    DueDate = project.EndDate.ToDateTime(TimeOnly.MinValue),
                    IsRead = false,
                    NotificationType = notificationType,
                    RelatedType = "PROJECT"
                });
            }

            Debug.WriteLine(
                $"PROJECT NOTIFICATIONS: {projectNotifications.Count} generated.");

            return projectNotifications;
        }

        // Checks each project for incoming messages.
        private async Task LoadChatNotificationsAsync(
            List<NotificationModel> combinedNotifications)
        {
            if (_apiService.CurrentUserId <= 0)
                return;

            foreach (var project in Projects)
            {
                if (project.ProjectId <= 0)
                    continue;

                try
                {
                    // BACKEND:
                    // POST /chat/chatMessages?projectId={projectId}
                    //
                    // Retrieves messages belonging to this project.

                    var messages = await _apiService.GetMessagesAsync(
                        project.ProjectId);

                    var incomingMessages = messages
                        .Where(m => m.SenderId != _apiService.CurrentUserId)
                        .OrderBy(m => m.MessageId)
                        .ToList();

                    long latestIncomingMessageId =
                        incomingMessages.LastOrDefault()?.MessageId ?? 0;

                    string lastSeenKey = GetLastSeenMessageKey(
                        _apiService.CurrentUserId,
                        project.ProjectId);

                    string pendingKey = GetPendingMessageKey(
                        _apiService.CurrentUserId,
                        project.ProjectId);

                    long lastSeenMessageId =
                        Preferences.Default.Get(lastSeenKey, -1L);

                    long pendingMessageId =
                        Preferences.Default.Get(pendingKey, 0L);

                    // First check: treat existing messages as old.
                    // Do not notify the user about messages already in the chat.
                    if (lastSeenMessageId < 0)
                    {
                        Preferences.Default.Set(
                            lastSeenKey,
                            latestIncomingMessageId);

                        Debug.WriteLine(
                            $"CHAT BASELINE: Project {project.ProjectId}, message {latestIncomingMessageId}");

                        continue;
                    }

                    // A higher message ID means a newer incoming message.
                    if (latestIncomingMessageId > lastSeenMessageId)
                    {
                        pendingMessageId = latestIncomingMessageId;

                        Preferences.Default.Set(
                            pendingKey,
                            pendingMessageId);

                        Preferences.Default.Set(
                            lastSeenKey,
                            latestIncomingMessageId);

                        Debug.WriteLine(
                            $"NEW CHAT MESSAGE: Project {project.ProjectId}");
                    }

                    // Keep displaying a pending notification until the
                    // user opens that project's chat.
                    if (pendingMessageId > 0)
                    {
                        var pendingMessage = incomingMessages
                            .FirstOrDefault(m =>
                                m.MessageId == pendingMessageId);

                        combinedNotifications.Add(new NotificationModel
                        {
                            NotificationId = -pendingMessageId,
                            MessageId = pendingMessageId,
                            ProjectId = project.ProjectId,
                            ProjectName = project.ProjectName,
                            Title = "New message",
                            Message = "You have a new message.",
                            DueDate = pendingMessage?.TimeSent ?? DateTime.Now,
                            IsRead = false,
                            NotificationType = "CHAT_MESSAGE",
                            RelatedType = "CHAT"
                        });
                    }
                }
                catch (Exception ex)
                {
                    // A chat failure must not prevent other notifications
                    // or projects from loading.
                    Debug.WriteLine(
                        $"CHAT NOTIFICATION ERROR - Project {project.ProjectId}: {ex.Message}");
                }
            }
        }

        // Creates unique device storage keys for each user and project.
        private static string GetLastSeenMessageKey(
            long userId,
            long projectId)
        {
            return $"ChatLastSeen_{userId}_{projectId}";
        }

        private static string GetPendingMessageKey(
            long userId,
            long projectId)
        {
            return $"ChatPending_{userId}_{projectId}";
        }

        // Called when the user opens a project's chat.
        // Clears its pending notification and updates the last-seen ID.
        public static void MarkChatNotificationsAsRead(
            long userId,
            long projectId,
            long latestIncomingMessageId)
        {
            if (userId <= 0 || projectId <= 0)
                return;

            string lastSeenKey = GetLastSeenMessageKey(userId, projectId);
            string pendingKey = GetPendingMessageKey(userId, projectId);

            long currentLastSeen =
                Preferences.Default.Get(lastSeenKey, -1L);

            Preferences.Default.Set(
                lastSeenKey,
                Math.Max(currentLastSeen, latestIncomingMessageId));

            Preferences.Default.Set(pendingKey, 0L);
        }

        // Loads temporary dashboard preview projects.
        public void LoadPreviewProjects()
        {
            IsBusy = true;

            try
            {
                Projects.Clear();
                Notifications.Clear();

                HasNotifications = false;

                // PREVIEW DATA:
                // These projects are temporary sample data.

                Projects.Add(new ProjectModel
                {
                    ProjectId = 1,
                    ProjectName = "UniConnect",
                    ProjectDescription = "University team collaboration platform",
                    StartDate = new DateOnly(2026, 10, 1),
                    EndDate = new DateOnly(2026, 11, 30)
                });

                Projects.Add(new ProjectModel
                {
                    ProjectId = 2,
                    ProjectName = "Handy Market",
                    ProjectDescription = "Marketplace for local service providers",
                    StartDate = new DateOnly(2026, 9, 15),
                    EndDate = new DateOnly(2026, 12, 15),
                    ProjectStatus = "IN PROGRESS"
                });

                Projects.Add(new ProjectModel
                {
                    ProjectId = 3,
                    ProjectName = "Student Portal",
                    ProjectDescription = "A platform for student resources",
                    StartDate = new DateOnly(2026, 10, 5),
                    EndDate = new DateOnly(2026, 12, 1),
                    ProjectStatus = "IN PROGRESS"
                });

                foreach (var notification in BuildProjectDeadlineNotifications())
                {
                    Notifications.Add(notification);
                }

                HasProjects = Projects.Count > 0;
                HasNotifications = Notifications.Count > 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
