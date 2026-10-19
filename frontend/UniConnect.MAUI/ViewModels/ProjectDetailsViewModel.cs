using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    // Project Details screen: 1) project details + code, 2) members, 3) project actions.
    // What the viewer can do depends on their role in THIS project:
    //   Team Lead   -> add members, remove members, disband the project
    //   Team Member -> view everything, leave the project
    // The UI hides what a role can't do, but the BACKEND MUST ALSO ENFORCE these rules
    // (see docs/API_PROJECT_DETAILS.md) - hiding a button is not security.
    [QueryProperty(nameof(ProjectId), "projectId")]
    public class ProjectDetailsViewModel : INotifyPropertyChanged
    {
        // DEV SWITCH: the details/members/remove/disband endpoints don't exist on the backend yet.
        // While true, a failed call falls back to sample data (and actions work locally) so the
        // UI can be built and tested. Set to false once the API works.
        private static readonly bool UseSampleDataOnFailure = true;

        // Preview helper (sample data only): true = you see the Team Lead view, false = Team Member view.
        private const bool SampleViewerIsLeader = true;

        private readonly ApiService _api;

        public int ProjectId { get; set; }

        public ObservableCollection<ProjectMemberModel> Members { get; } = new();
        public ObservableCollection<ProjectFileModel> Files { get; } = new();

        // Upload limits. The backend must enforce these as well (see docs/API_PROJECT_DETAILS.md).
        private const long MaxFileBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip", ".png", ".jpg", ".jpeg"
        };

        private ProjectDetailsModel? _details;
        public ProjectDetailsModel? Details
        {
            get => _details;
            private set { _details = value; Refresh(); }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        // ---- Section 1 text ----
        public string ProjectName => Details?.ProjectName ?? string.Empty;
        public string Description => string.IsNullOrWhiteSpace(Details?.ProjectDescription) ? "No description." : Details!.ProjectDescription!;
        public string StartText => FormatDate(Details?.StartDate);
        public string EndText => FormatDate(Details?.EndDate);
        public string StatusText => Details?.Status switch
        {
            "INPROGRESS" => "In progress",
            "COMPLETED" => "Completed",
            null or "" => "-",
            var s => s
        };
        public string FileText => string.IsNullOrWhiteSpace(Details?.ProjectFileName) ? "No file attached" : Details!.ProjectFileName!;
        public string CodeText => Details?.ProjectCode?.ToString() ?? "------";
        public bool HasCode => Details?.ProjectCode != null;
        public string RoleText => RoleNames.Display(Details?.MyRole);

        // ---- Permissions (drive what each role sees) ----
        public bool IsLeader => RoleNames.IsLeader(Details?.MyRole);
        public bool IsNotLeader => !IsLeader;

        // ---- Section 2 (files) text ----
        public string FileCountText => Files.Count == 1 ? "1 file" : $"{Files.Count} files";
        public bool HasNoFiles => Files.Count == 0;

        // ---- Section 3 (members) text ----
        public string MemberCountText => Members.Count == 1 ? "1 member" : $"{Members.Count} members";

        // Id of the logged-in student (sample mode pretends to be student 2).
        private int MyId => _api.CurrentUserId ?? (UseSampleDataOnFailure ? 2 : -1);

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
                if (ProjectId == 0) throw new InvalidOperationException("No project selected");
                details = await _api.GetProjectDetailsAsync(ProjectId);
                members = await _api.GetProjectMembersAsync(ProjectId);
                files = await _api.GetProjectFilesAsync(ProjectId);
            }
            catch (Exception)
            {
                if (!UseSampleDataOnFailure)
                {
                    ErrorMessage = "Couldn't load project details. Check your connection and try again.";
                    IsBusy = false;
                    return;
                }
                (details, members) = SampleData();
                files = SampleFiles();
            }

            // If the server didn't say what role we have, work it out from the member list.
            if (string.IsNullOrEmpty(details.MyRole))
                details.MyRole = members.FirstOrDefault(m => m.StudentId == _api.CurrentUserId)?.Role;

            bool leader = RoleNames.IsLeader(details.MyRole);
            foreach (var m in members)
                m.CanRemove = leader && !m.IsLeader;   // set BEFORE adding so the row renders correctly

            Members.Clear();
            foreach (var m in members) Members.Add(m);

            foreach (var f in files)
                f.CanDelete = leader || f.UploadedById == MyId; // set BEFORE adding, like CanRemove
            Files.Clear();
            foreach (var f in files) Files.Add(f);

            Details = details;
            IsBusy = false;
        }

        // ---- Actions. Each returns null on success, or a message to show the user. ----

        // ---- Files. Any member can upload; only the uploader or the Team Lead can delete. ----

        // BACKEND: POST /projects/{projectId}/files?uploaderId=   (multipart, part name "file")
        public async Task<string?> UploadFileAsync(FileResult file)
        {
            string extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
                return "That file type isn't allowed. Use a PDF, Word, Excel, PowerPoint, text, zip or picture file.";

            try
            {
                await using var stream = await file.OpenReadAsync();
                long size = stream.CanSeek ? stream.Length : 0;
                if (size > MaxFileBytes) return "That file is too big. The limit is 10 MB.";

                try
                {
                    var created = await _api.UploadProjectFileAsync(ProjectId, file.FileName, stream, file.ContentType);
                    created.CanDelete = true;
                    Files.Insert(0, created);
                }
                catch (Exception ex)
                {
                    if (!UseSampleDataOnFailure) return FriendlyError(ex, "Couldn't upload that file.");
                    Files.Insert(0, new ProjectFileModel
                    {
                        FileId = 1000 + Files.Count, FileName = file.FileName, ContentType = file.ContentType,
                        SizeBytes = size, UploadedById = MyId, UploadedByName = "You",
                        UploadedAt = DateTime.Now, CanDelete = true
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

        // BACKEND: GET /projects/{projectId}/files/{fileId}/download?studentId=
        // Returns the local path of the downloaded copy so the page can open it.
        public async Task<(string? Path, string? Error)> DownloadFileAsync(ProjectFileModel file)
        {
            try
            {
                string path = await _api.DownloadProjectFileAsync(ProjectId, file.FileId, file.FileName ?? "file");
                return (path, null);
            }
            catch (Exception ex)
            {
                if (UseSampleDataOnFailure) return (null, "Sample mode: this file isn't stored on a server, so there is nothing to open.");
                return (null, FriendlyError(ex, "Couldn't open that file."));
            }
        }

        // BACKEND: DELETE /projects/{projectId}/files/{fileId}?requesterId=
        public async Task<string?> DeleteFileAsync(ProjectFileModel file)
        {
            if (!(IsLeader || file.UploadedById == MyId))
                return "Only the person who uploaded this file or the Team Lead can delete it.";

            try { await _api.DeleteProjectFileAsync(ProjectId, file.FileId); }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure) return FriendlyError(ex, "Couldn't delete that file.");
            }
            Files.Remove(file);
            Refresh();
            return null;
        }

        // Team Lead only. BACKEND: POST /projects/addMember?projectId=&email=&requesterId=
        public async Task<string?> AddMemberAsync(string? email)
        {
            email = email?.Trim() ?? string.Empty;
            if (!IsLeader) return "Only the Team Lead can add members.";
            if (email.Length == 0 || !email.Contains('@') || !email.Contains('.'))
                return "Enter a valid student email.";
            if (Members.Any(m => string.Equals(m.Email, email, StringComparison.OrdinalIgnoreCase)))
                return "That student is already in this project.";

            try
            {
                await _api.AddMemberAsync(ProjectId, email);
                var fresh = await _api.GetProjectMembersAsync(ProjectId);
                Members.Clear();
                foreach (var m in fresh) { m.CanRemove = !m.IsLeader; Members.Add(m); }
                Refresh();
                return null;
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure) return FriendlyError(ex, "Couldn't add that member.");
                Members.Add(new ProjectMemberModel
                {
                    StudentId = 100 + Members.Count, Name = email.Split('@')[0], Surname = string.Empty,
                    Email = email, Role = "MEMBER", CanRemove = true
                });
                Refresh();
                return null;
            }
        }

        // Team Lead only, and never the lead themselves.
        // BACKEND: DELETE /projects/{projectId}/members/{studentId}?requesterId=
        public async Task<string?> RemoveMemberAsync(ProjectMemberModel member)
        {
            if (!IsLeader) return "Only the Team Lead can remove members.";
            if (member.IsLeader) return "The Team Lead can't be removed.";

            try { await _api.RemoveMemberAsync(ProjectId, member.StudentId); }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure) return FriendlyError(ex, "Couldn't remove that member.");
            }
            Members.Remove(member);
            Refresh();
            return null;
        }

        // Team Lead only. BACKEND: DELETE /projects/{projectId}?requesterId=
        public async Task<string?> DisbandAsync()
        {
            if (!IsLeader) return "Only the Team Lead can disband the project.";
            try { await _api.DisbandProjectAsync(ProjectId); }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure) return FriendlyError(ex, "Couldn't disband the project.");
            }
            return null;
        }

        // Team Members only (the lead must disband instead).
        // BACKEND: DELETE /projects/{projectId}/members/{myStudentId}?requesterId={myStudentId}
        public async Task<string?> LeaveAsync()
        {
            if (IsLeader) return "The Team Lead can't leave. Disband the project instead.";
            try
            {
                int me = _api.CurrentUserId ?? throw new InvalidOperationException("Not logged in");
                await _api.RemoveMemberAsync(ProjectId, me);
            }
            catch (Exception ex)
            {
                if (!UseSampleDataOnFailure) return FriendlyError(ex, "Couldn't leave the project.");
            }
            return null;
        }

        // ---- helpers ----

        private static string FormatDate(DateTime? d) => d?.ToString("d MMM yyyy", CultureInfo.InvariantCulture) ?? "-";

        private static string FriendlyError(Exception ex, string fallback)
        {
            if (ex is HttpRequestException http)
            {
                return http.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => "Only the Team Lead can do that.",
                    System.Net.HttpStatusCode.NotFound => "That project, student or file could not be found.",
                    System.Net.HttpStatusCode.RequestEntityTooLarge => "That file is too big for the server.",
                    System.Net.HttpStatusCode.UnsupportedMediaType => "The server doesn't accept that file type.",
                    System.Net.HttpStatusCode.Conflict => string.IsNullOrWhiteSpace(http.Message) ? fallback : http.Message,
                    _ => fallback
                };
            }
            return fallback;
        }

        private void Refresh() => OnPropertyChanged(string.Empty); // re-read every bound property

        private static List<ProjectFileModel> SampleFiles() => new()
        {
            new() { FileId = 1, FileName = "UniConnect_Brief.pdf", ContentType = "application/pdf", SizeBytes = 245_760, UploadedById = 1, UploadedByName = "Tshepang Bambo", UploadedAt = DateTime.Today.AddDays(-20) },
            new() { FileId = 2, FileName = "Login_wireframe.png", ContentType = "image/png", SizeBytes = 1_310_720, UploadedById = 2, UploadedByName = "Lebogang Malatsi", UploadedAt = DateTime.Today.AddDays(-6) },
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
                new() { StudentId = 1, Name = "Tshepang", Surname = "Bambo", Email = "tshepang@example.com", Role = SampleViewerIsLeader ? "MEMBER" : "LEADER" },
                new() { StudentId = 2, Name = "Lebogang", Surname = "Malatsi", Email = "lebogang@example.com", Role = myRole },
                new() { StudentId = 3, Name = "Koketso", Surname = "Fakude", Email = "koketso@example.com", Role = "MEMBER" },
                new() { StudentId = 4, Name = "Molebogeng", Surname = "Mashishi", Email = "molebogeng@example.com", Role = "MEMBER" },
            };
            return (details, members);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

