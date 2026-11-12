
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    [QueryProperty(nameof(ProjectId), "projectId")]
    public class ProjectDetailsViewModel : INotifyPropertyChanged
    {
        private static readonly bool UseSampleDataOnFailure = false;
        private const bool SampleViewerIsLeader = true;

        private readonly ApiService _api;

        public long ProjectId { get; set; }

        public ObservableCollection<ProjectMemberModel> Members { get; } = new();
        public ObservableCollection<ProjectFileModel> Files { get; } = new();

        private const long MaxFileBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".ppt", ".pptx",
                ".xls", ".xlsx", ".txt", ".zip",
                ".png", ".jpg", ".jpeg"
            };

        private ProjectDetailsModel? _details;

        public ProjectDetailsModel? Details
        {
            get => _details;
        }

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

        // Project details displayed on the page

        public string ProjectName => Details?.ProjectName ?? string.Empty;

        public string Description =>
            string.IsNullOrWhiteSpace(Details?.ProjectDescription)
                ? "No description."
                : Details!.ProjectDescription!;

        public string StartText => FormatDate(Details?.StartDate);

        public string EndText => FormatDate(Details?.EndDate);

        public string StatusText => Details?.Status switch
        {
            "INPROGRESS" => "In progress",
            "COMPLETED" => "Completed",
            null or "" => "-",
            var status => status
        };

        public string FileText =>
            Files.Count == 0 ? "No file attached" :
            (Files[0].FileName ?? "Project file");

        public string ProjectFileDisplayName =>
            Files.Count == 0 ? string.Empty :
            (Files[0].FileName ?? "project-file");

        public string CodeText => Details?.ProjectCode?.ToString() ?? "------";

        public bool HasCode => Details?.ProjectCode != null;

        public string RoleText => RoleNames.Display(Details?.MyRole);

        // Permissions

        public bool IsLeader => RoleNames.IsLeader(Details?.MyRole);

        public bool IsNotLeader => !IsLeader;

        // File information

        public string FileCountText =>
            Files.Count == 1 ? "1 file" : $"{Files.Count} files";

        public bool HasNoFiles => Files.Count == 0;

        // Member information

        public string MemberCountText =>
            Members.Count == 1 ? "1 member" : $"{Members.Count} members";

        private long MyId =>
            UseSampleDataOnFailure ? 2 : _api.CurrentUserId;

        // Edit project details

        private bool _isEditing;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsViewingDetails));
            }
        }

        public bool IsViewingDetails => !IsEditing;

        private string _editProjectName = string.Empty;

        public string EditProjectName
        {
            get => _editProjectName;
            set
            {
                _editProjectName = value;
                OnPropertyChanged();
            }
        }

        private string _editDescription = string.Empty;

        public string EditDescription
        {
            get => _editDescription;
            set
            {
                _editDescription = value;
                OnPropertyChanged();
            }
        }

        private DateTime _editStartDate = DateTime.Today;

        public DateTime EditStartDate
        {
            get => _editStartDate;
            set
            {
                _editStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _editEndDate = DateTime.Today;

        public DateTime EditEndDate
        {
            get => _editEndDate;
            set
            {
                _editEndDate = value;
                OnPropertyChanged();
            }
        }

        public List<string> StatusOptions { get; } =
            new() { "INPROGRESS", "COMPLETED" };

        private string _editStatus = "INPROGRESS";

        public string EditStatus
        {
            get => _editStatus;
            set
            {
                _editStatus = value;
                OnPropertyChanged();
            }
        }

        public string EditStatusText =>
            EditStatus == "COMPLETED" ? "Completed" : "In progress";

        public ProjectDetailsViewModel(ApiService api)
        {
            _api = api;
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            ProjectDetailsModel details;
            List<ProjectMemberModel> members;
            List<ProjectFileModel> files;

            try
            {
                if (ProjectId == 0)
                    throw new InvalidOperationException("No project selected.");

                details = await _api.GetProjectDetailsAsync(ProjectId);
                members = await _api.GetProjectMembersAsync(ProjectId);
                files = await _api.GetProjectFilesAsync(ProjectId);
            }
            catch (Exception)
            {
                if (!UseSampleDataOnFailure)
                {
                    ErrorMessage =
                        "Couldn't load project details. Check your connection and try again.";
                    IsBusy = false;
                    return;
                }

                (details, members) = SampleData();
                files = SampleFiles();
            }

            if (string.IsNullOrEmpty(details.MyRole))
            {
                details.MyRole = members
                    .FirstOrDefault(m =>
                        m.Student.UserId == _api.CurrentUserId)?.Role;
            }

            bool leader = RoleNames.IsLeader(details.MyRole);

            foreach (var member in members)
                member.CanRemove = leader && !member.IsLeader;

            Members.Clear();

            foreach (var member in members)
                Members.Add(member);

            foreach (var file in files)
                file.CanDelete = leader || file.UploadedById == MyId;

            Files.Clear();

            foreach (var file in files)
                Files.Add(file);

            _details = details;

            IsEditing = false;
            Refresh();

            IsBusy = false;
        }

        // Start editing. Only the Team Lead can edit.

        public string? StartEditing()
        {
            if (!IsLeader)
                return "Only the Team Lead can edit project details.";

            if (Details == null)
                return "Project details have not loaded.";

            EditProjectName = Details.ProjectName ?? string.Empty;
            EditDescription = Details.ProjectDescription ?? string.Empty;
            EditStartDate = Details.StartDate?.Date ?? DateTime.Today;
            EditEndDate = Details.EndDate?.Date ?? EditStartDate;
            EditStatus = Details.Status ?? "INPROGRESS";

            ErrorMessage = string.Empty;
            IsEditing = true;

            return null;
        }

        // Cancel editing without saving changes.

        public void CancelEditing()
        {
            IsEditing = false;
            ErrorMessage = string.Empty;
            Refresh();
        }

        // Save edited project details.

        public async Task<string?> SaveEditingAsync()
        {
            if (!IsLeader)
                return "Only the Team Lead can edit project details.";

            if (Details == null)
                return "Project details have not loaded.";

            EditProjectName = EditProjectName.Trim();
            EditDescription = EditDescription.Trim();

            if (string.IsNullOrWhiteSpace(EditProjectName))
                return "Enter a project name.";

            if (EditProjectName.Length > 100)
                return "The project name cannot exceed 100 characters.";

            if (string.IsNullOrWhiteSpace(EditDescription))
                return "Enter a project description.";

            if (EditEndDate.Date < EditStartDate.Date)
                return "The due date cannot be before the start date.";

            if (!StatusOptions.Contains(EditStatus))
                return "Select a valid project status.";

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var updatedProject = new ProjectDetailsModel
                {
                    ProjectId = Details.ProjectId,
                    ProjectName = EditProjectName,
                    ProjectDescription = EditDescription,
                    StartDate = EditStartDate.Date,
                    EndDate = EditEndDate.Date,
                    Status = EditStatus,
                    ProjectCode = Details.ProjectCode,
                    ProjectFileName = Details.ProjectFileName,
                    MyRole = Details.MyRole
                };

                var savedProject = await _api.UpdateProjectAsync(
                    ProjectId,
                    updatedProject);

                savedProject.MyRole = Details.MyRole;
                _details = savedProject;

                IsEditing = false;
                Refresh();

                return null;
            }
            catch (Exception ex)
            {
                return FriendlyError(
                    ex,
                    "Couldn't save the project details. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<string?> UploadFileAsync(FileResult file)
        {
            string extension = Path.GetExtension(file.FileName);

            if (!AllowedExtensions.Contains(extension))
                return "That file type isn't allowed. Use a PDF, Word, Excel, PowerPoint, text, zip or picture file.";

            try
            {
                await using var stream = await file.OpenReadAsync();

                long size = stream.CanSeek ? stream.Length : 0;

                if (size > MaxFileBytes)
                    return "That file is too big. The limit is 10 MB.";

                try
                {
                    var created = await _api.UploadProjectFileAsync(
                        ProjectId,
                        file.FileName,
                        stream,
                        file.ContentType);

                    created.CanDelete = true;
                    Files.Insert(0, created);
                }
                catch (Exception ex)
                {
                    if (!UseSampleDataOnFailure)
                        return FriendlyError(ex, "Couldn't upload that file.");

                    Files.Insert(0, new ProjectFileModel
                    {
                        FileId = 1000 + Files.Count,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        SizeBytes = size,
                        UploadedById = MyId,
                        UploadedByName = "You",
                        UploadedAt = DateTime.Now,
                        CanDelete = true
                    });
                }

                Refresh();
                return null;
            }
            catch (Exception)
            {
                return "Couldn't read that file.";
            }
        }

        public async Task<(string? Path, string? Error)> DownloadFileAsync(
            ProjectFileModel file)
        {
            try
            {
                string path = await _api.DownloadProjectFileAsync(
                    ProjectId,
                    file.FileId,
                    file.FileName ?? "file");

                return (path, null);
            }
            catch (Exception)
            {
                if (UseSampleDataOnFailure)
                    return (null, "Sample mode: this file isn't stored on a server, so there is nothing to open.");

                return (null, "Couldn't open that file.");
            }
        }

        public async Task<(string? Path, string? Error)> DownloadProjectFileAsync()
        {
            if (Files.Count == 0)
                return (null, "No file is attached to this project.");

            try
            {
                string fileName = ProjectFileDisplayName;

                string path = await _api.DownloadProjectFileAsync(
                    ProjectId,
                    Files[0].FileId,
                    fileName);

                return (path, null);
            }
            catch (Exception ex)
            {
                return (null, FriendlyError(ex, "Couldn't download project file."));
            }
        }

        public async Task<string?> DeleteFileAsync(ProjectFileModel file)
        {
            if (!(IsLeader || file.UploadedById == MyId))
                return "Only the person who uploaded this file or the Team Lead can delete it.";

            try
            {
                await _api.DeleteProjectFileAsync(ProjectId, file.FileId);
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure)
                    return FriendlyError(ex, "Couldn't delete that file.");
            }

            Files.Remove(file);
            Refresh();

            return null;
        }

        public async Task<string?> AddMemberAsync(string email)
        {
            email = email?.Trim() ?? string.Empty;

            if (!IsLeader)
                return "Only the Team Lead can add members.";

            if (email.Length == 0 || !email.Contains('@') || !email.Contains('.'))
                return "Enter a valid student email.";

            if (Members.Any(m =>
                string.Equals(
                    m.Student?.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase)))
            {
                return "That student is already in this project.";
            }

            try
            {
                await _api.AddMemberAsync(ProjectId, email);

                var fresh = await _api.GetProjectMembersAsync(ProjectId);

                Members.Clear();

                foreach (var member in fresh)
                {
                    member.CanRemove = IsLeader && !member.IsLeader;
                    Members.Add(member);
                }

                Refresh();
                return null;
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure)
                    return FriendlyError(ex, "Couldn't add that member.");

                Members.Add(new ProjectMemberModel
                {
                    Student = new UserModel
                    {
                        UserId = 100 + Members.Count,
                        Name = email.Split('@')[0],
                        Surname = string.Empty,
                        Email = email
                    },
                    Role = "MEMBER",
                    CanRemove = true
                });

                Refresh();
                return null;
            }
        }

        public async Task<string?> RemoveMemberAsync(ProjectMemberModel member)
        {
            if (!IsLeader)
                return "Only the Team Lead can remove members.";

            if (member.IsLeader)
                return "The Team Lead can't be removed.";

            try
            {
                await _api.RemoveMemberAsync(ProjectId, member.Student.Email);
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure)
                    return FriendlyError(ex, "Couldn't remove that member.");
            }

            Members.Remove(member);
            Refresh();

            return null;
        }

        public async Task<string?> DisbandAsync()
        {
            if (!IsLeader)
                return "Only the Team Lead can disband the project.";

            try
            {
                await _api.DisbandProjectAsync(ProjectId);
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure)
                    return FriendlyError(ex, "Couldn't disband the project.");
            }

            return null;
        }

        public async Task<string?> LeaveAsync()
        {
            if (IsLeader)
                return "The Team Lead can't leave. Disband the project instead.";

            try
            {
                string me = _api.CurrentUserEmail;
                await _api.RemoveMemberAsync(ProjectId, me);
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure)
                    return FriendlyError(ex, "Couldn't leave the project.");
            }

            return null;
        }

        private static string FormatDate(DateTime? date) =>
            date?.ToString("d MMM yyyy", CultureInfo.InvariantCulture) ?? "-";

        private static string FriendlyError(Exception ex, string fallback)
        {
            if (ex is HttpRequestException http)
            {
                return http.StatusCode switch
                {
                    HttpStatusCode.Forbidden => "Only the Team Lead can do that.",
                    HttpStatusCode.NotFound => "That project, student or file could not be found.",
                    HttpStatusCode.RequestEntityTooLarge => "That file is too big for the server.",
                    HttpStatusCode.UnsupportedMediaType => "The server doesn't accept that file type.",
                    HttpStatusCode.Conflict => string.IsNullOrWhiteSpace(http.Message) ? fallback : http.Message,
                    _ => fallback
                };
            }

            return fallback;
        }

        private void Refresh()
        {
            OnPropertyChanged(string.Empty);
        }

        private static List<ProjectFileModel> SampleFiles() => new()
        {
            new()
            {
                FileId = 1,
                FileName = "UniConnect_Brief.pdf",
                ContentType = "application/pdf",
                SizeBytes = 245_760,
                UploadedById = 1,
                UploadedByName = "Tshepang Bambo",
                UploadedAt = DateTime.Today.AddDays(-20)
            },
            new()
            {
                FileId = 2,
                FileName = "Login_wireframe.png",
                ContentType = "image/png",
                SizeBytes = 1_310_720,
                UploadedById = 2,
                UploadedByName = "Lebogang Malatsi",
                UploadedAt = DateTime.Today.AddDays(-6)
            }
        };

        private (ProjectDetailsModel, List<ProjectMemberModel>) SampleData()
        {
            string myRole = SampleViewerIsLeader ? "LEADER" : "MEMBER";

            var details = new ProjectDetailsModel
            {
                ProjectId = ProjectId == 0 ? 1 : ProjectId,
                ProjectName = "UniConnect",
                ProjectDescription = "A lightweight project management app for student software engineering teams: task allocation, tracking, deadlines and chat.",
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(14),
                Status = "INPROGRESS",
                ProjectFileName = "UniConnect_Brief.pdf",
                ProjectCode = 482913,
                MyRole = myRole
            };

            var members = new List<ProjectMemberModel>
            {
                new()
                {
                    Student = new UserModel
                    {
                        UserId = 4,
                        Name = "Molebogeng",
                        Surname = "Mashishi",
                        Email = "molebogeng@example.com"
                    },
                    Role = "MEMBER"
                }
            };

            return (details, members);
        }

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
