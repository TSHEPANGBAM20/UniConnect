
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using UniConnect.MAUI.Models;

namespace UniConnect.MAUI.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

#if ANDROID
        // BACKEND: Android emulator connects through 10.0.2.2.
        public static string BaseUrl = "http://10.0.2.2:8080";
#elif IOS
        // BACKEND: iOS backend address.
        public static string BaseUrl = "http://localhost:8080";
#else
        // BACKEND: Local development backend address.
        public static string BaseUrl = "http://localhost:8080";
#endif

        // BACKEND:
        // These values are populated after a successful login.
        public long CurrentUserId { get; private set; }
        public string CurrentUserName { get; private set; }
        public string CurrentUserSurname { get; private set; }
        public string CurrentUserEmail { get; private set; }

        public ApiService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        private void SetAuthToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);
        }

        // ==========================================
        // AUTHENTICATION
        // ==========================================

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // BACKEND:
            // POST /auth/login

            var response = await _http.PostAsJsonAsync(
                "/auth/login", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            CurrentUserName = result.Name;
            CurrentUserId = result.UserId;
            CurrentUserEmail = result.Email;
            CurrentUserSurname = result.Surname;

            Console.WriteLine(CurrentUserId);

            return result;
        }

        public async Task<UserModel> RegisterAsync(RegisterRequest request)
        {
            // BACKEND:
            // POST /auth/register

            var response = await _http.PostAsJsonAsync(
                "/auth/register", request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        // ==========================================
        // PROJECT MEMBERS
        // ==========================================

        public async Task AddMemberAsync(long projectId, string email)
        {
            // BACKEND:
            // POST /projects/addMember

            var response = await _http.PostAsync(
                $"/projects/addMember?projectId={projectId}&email={Uri.EscapeDataString(email)}",
                null);

            await ThrowIfFailedAsync(response);
        }

        // ==========================================
        // PROJECTS
        // ==========================================

        public async Task<List<ProjectModel>> GetProjectsAsync(long id)
        {
            // BACKEND:
            // POST /projects/showStudentProjects?studentId={id}

            var response = await _http.PostAsync(
                $"/projects/showStudentProjects?studentId={id}", null);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProjectModel>>();
        }

        public async Task<ProjectModel> CreateProjectAsync(
            CreateProjectRequest request, long id)
        {
            // BACKEND:
            // POST /projects/createProject?id={id}

            var response = await _http.PostAsJsonAsync(
                $"/projects/createProject?id={id}", request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProjectModel>();
        }

        public async Task<ProjectDetailsModel> GetProjectDetailsAsync(
            long projectId)
        {
            // BACKEND:
            // POST /projects/getProjectInfo?projectId={projectId}

            var response = await _http.PostAsync(
                $"/projects/getProjectInfo?projectId={projectId}", null);

            await ThrowIfFailedAsync(response);

            return await response.Content.ReadFromJsonAsync<ProjectDetailsModel>();
        }

        // Update project details - Team Lead only
        public async Task<ProjectDetailsModel> UpdateProjectAsync(
            long projectId,
            ProjectDetailsModel project)
        {
            // BACKEND:
            // POST /projects/updateProject?projectId={projectId}&studentId={studentId}

            if (CurrentUserId <= 0)
            {
                throw new InvalidOperationException(
                    "A student must be logged in to edit a project.");
            }

            var requestData = new
            {
                ProjectName = project.ProjectName,
                ProjectDescription = project.ProjectDescription,
                StartDate = project.StartDate?.ToString("yyyy-MM-dd"),
                EndDate = project.EndDate?.ToString("yyyy-MM-dd"),
                Status = project.Status
            };

            var response = await _http.PostAsJsonAsync(
                $"/projects/updateProject?projectId={projectId}&studentId={CurrentUserId}",
                requestData);

            await ThrowIfFailedAsync(response);

            return await response.Content.ReadFromJsonAsync<ProjectDetailsModel>()
                ?? throw new InvalidOperationException(
                    "The backend did not return the updated project.");
        }

        public async Task<List<ProjectMemberModel>> GetProjectMembersAsync(
            long projectId)
        {
            // BACKEND:
            // POST /projects/showStudentsInProject?projectId={projectId}

            var response = await _http.PostAsync(
                $"/projects/showStudentsInProject?projectId={projectId}", null);

            await ThrowIfFailedAsync(response);

            var members =
                await response.Content.ReadFromJsonAsync<List<ProjectMemberModel>>();

            return members;
        }

        public async Task RemoveMemberAsync(
            long projectId, string studentEmail)
        {
            // BACKEND:
            // DELETE /projects/removeMemberFromgroup

            var response = await _http.DeleteAsync(
                $"/projects/removeMemberFromgroup?studentEmail={Uri.EscapeDataString(studentEmail)}&projectId={projectId}");

            await ThrowIfFailedAsync(response);
        }

        public async Task DisbandProjectAsync(long projectId)
        {
            // BACKEND:
            // DELETE /projects/disbandGroup

            var response = await _http.DeleteAsync(
                $"/projects/disbandGroup?projectId={projectId}");

            await ThrowIfFailedAsync(response);
        }

        public async Task<long> JoinProjectByCodeAsync(int projectCode)
        {
            // BACKEND:
            // POST /projects/joinByCode

            string email = CurrentUserEmail ??
                throw new InvalidOperationException("Not logged in");

            var response = await _http.PostAsync(
                $"/projects/joinByCode?projectCode={projectCode}&email={Uri.EscapeDataString(email)}",
                null);

            await ThrowIfFailedAsync(response);

            var member =
                await response.Content.ReadFromJsonAsync<ProjectMemberModel>();

            if (member.Project == null)
            {
                throw new InvalidOperationException(
                    "The server did not say which project was joined.");
            }

            return member.Project.ProjectId;
        }

        // ==========================================
        // PROJECT FILES
        // ==========================================

        public async Task<List<ProjectFileModel>> GetProjectFilesAsync(
            long projectId)
        {
            // BACKEND:
            // GET /projects/{projectId}/files?studentId={studentId}

            var response = await _http.GetAsync(
                $"/projects/{projectId}/files?studentId={RequireUserId()}");

            await ThrowIfFailedAsync(response);

            return await response.Content.ReadFromJsonAsync<List<ProjectFileModel>>()
                   ?? new List<ProjectFileModel>();
        }

        public async Task<ProjectFileModel> UploadProjectFileAsync(
            long projectId,
            string fileName,
            Stream content,
            string? contentType)
        {
            // BACKEND:
            // POST /projects/{projectId}/files?studentId={studentId}

            using var form = new MultipartFormDataContent();

            var part = new StreamContent(content);

            part.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(contentType)
                        ? "application/octet-stream"
                        : contentType);

            form.Add(part, "projectFile", fileName);

            var response = await _http.PostAsync(
                $"/projects/{projectId}/files?studentId={RequireUserId()}",
                form);

            await ThrowIfFailedAsync(response);

            return (await response.Content.ReadFromJsonAsync<ProjectFileModel>())!;
        }

        public async Task<string> DownloadProjectFileAsync(
            long projectId,
            long fileId,
            string fileName)
        {
            // BACKEND:
            // GET /projects/{projectId}/files/{fileId}/download

            var response = await _http.GetAsync(
                $"/projects/{projectId}/files/{fileId}/download?studentId={RequireUserId()}");

            await ThrowIfFailedAsync(response);

            string safeName = Path.GetFileName(fileName);

            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "project-file";
            }

            string path = Path.Combine(
                FileSystem.CacheDirectory, safeName);

            await File.WriteAllBytesAsync(
                path, await response.Content.ReadAsByteArrayAsync());

            return path;
        }

        public async Task DeleteProjectFileAsync(
            long projectId, long fileId)
        {
            // BACKEND:
            // DELETE /projects/{projectId}/files/{fileId}

            var response = await _http.DeleteAsync(
                $"/projects/{projectId}/files/{fileId}?requesterId={RequireUserId()}");

            await ThrowIfFailedAsync(response);
        }

        private long RequireUserId()
        {
            if (CurrentUserId <= 0)
            {
                throw new InvalidOperationException(
                    "A student must be logged in to perform this action.");
            }

            return CurrentUserId;
        }

        // ==========================================
        // API ERROR HANDLING
        // ==========================================

        private static async Task ThrowIfFailedAsync(
            HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string message =
                $"Request failed ({(int)response.StatusCode})";

            try
            {
                using var doc = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                if (doc.RootElement.TryGetProperty(
                        "message", out var m) &&
                    m.GetString() is { Length: > 0 } text)
                {
                    message = text;
                }
            }
            catch (Exception)
            {
                // Response body was empty or not JSON.
            }

            throw new HttpRequestException(
                message, null, response.StatusCode);
        }

        // ==========================================
        // TASKS
        // ==========================================

        public async Task<TaskModel> CreateTaskAsync(
            long projectId, CreateTaskRequest request)
        {
            // BACKEND:
            // POST /tasks/createTask/{projectId}

            if (projectId <= 0)
            {
                throw new Exception("A valid project is required.");
            }

            if (request == null)
            {
                throw new Exception("Task details are required.");
            }

            if (!request.AssignedTo.HasValue ||
                request.AssignedTo.Value <= 0)
            {
                throw new Exception("A valid student must be assigned to the task.");
            }

            var taskData = new
            {
                Title = request.TaskTitle,
                Description = request.TaskDescription,
                Priority = request.Priority,

                DueDate = request.DueDate?.ToString(
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture),

                Status = "TODO",

                // The actual student ID selected in CreateTaskPage.
                AssignedTo = request.AssignedTo.Value
            };

            var response = await _http.PostAsJsonAsync(
                $"/tasks/createTask/{projectId}", taskData);

            await ThrowIfFailedAsync(response);

            return await response.Content.ReadFromJsonAsync<TaskModel>()
                ?? throw new Exception(
                    "The backend did not return the created task.");
        }

        public async Task<List<TaskModel>> GetTasksAsync(long projectId)
        {
            // BACKEND:
            // GET /tasks/getTaskByProject/{projectId}

            var response = await _http.GetAsync(
                $"/tasks/getTaskByProject/{projectId}");

            response.EnsureSuccessStatusCode();

            var tasks =
                await response.Content.ReadFromJsonAsync<List<TaskModel>>();

            return tasks ?? new List<TaskModel>();
        }

        public async Task UpdateTaskStatusAsync(
            long taskId, string status)
        {
            // BACKEND:
            // PUT /tasks/updateStatus/{taskId}/status

            var request = new UpdateTaskStatusRequest
            {
                Status = status
            };

            var response = await _http.PutAsJsonAsync(
                $"/tasks/updateStatus/{taskId}/status", request);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            // BACKEND:
            // DELETE /tasks/deleteTask/{taskId}

            var response = await _http.DeleteAsync(
                $"/tasks/deleteTask/{taskId}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<TaskModel>> GetCalendarTasksAsync()
        {
            // BACKEND:
            // GET /tasks/TaskCalendar/calendar?studentId={CurrentUserId}

            var response = await _http.GetAsync(
                $"/tasks/TaskCalendar/calendar?studentId={CurrentUserId}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        }

        // ==========================================
        // DEADLINE NOTIFICATIONS
        // ==========================================

        public async Task<List<NotificationModel>> GetNotificationsAsync()
        {
            // BACKEND:
            // GET /tasks/notifications/{studentId}
            // Tasks with status DONE are excluded.

            if (CurrentUserId <= 0)
            {
                throw new InvalidOperationException(
                    "A student must be logged in to retrieve notifications.");
            }

            var response = await _http.GetAsync(
                $"/tasks/notifications/{CurrentUserId}");

            await ThrowIfFailedAsync(response);

            return await response.Content.ReadFromJsonAsync<List<NotificationModel>>()
                   ?? new List<NotificationModel>();
        }

        // ==========================================
        // CHAT
        // ==========================================

        public async Task<List<MessageModel>> GetMessagesAsync(
            long projectId)
        {
            // BACKEND:
            // POST /chat/chatMessages?projectId={projectId}

            var response = await _http.PostAsync(
                $"/chat/chatMessages?projectId={projectId}", null);

            response.EnsureSuccessStatusCode();

            var messages =
                await response.Content.ReadFromJsonAsync<List<MessageModel>>()
                ?? new List<MessageModel>();

            foreach (var message in messages)
            {
                message.IsOwnMessage =
                    message.SenderId == CurrentUserId;
            }

            return messages;
        }

        public async Task SendMessageAsync(
            long projectId, string text)
        {
            // BACKEND:
            // POST /chat/sendMessage

            var response = await _http.PostAsJsonAsync(
                $"/chat/sendMessage?projectId={projectId}&senderId={CurrentUserId}",
                new SendMessageRequest
                {
                    Message = text
                });

            response.EnsureSuccessStatusCode();
        }

        // ==========================================
        // STUDENT PROFILE
        // ==========================================

        public async Task UpdateStudentProfileAsync(
            string name,
            string surname,
            string email)
        {
            // BACKEND:
            // PUT /auth/updateEmailOrName
            // Updates the student's first name, surname and email.

            var response = await _http.PutAsync(
                $"/auth/updateEmailOrName?id={CurrentUserId}" +
                $"&updatedName={Uri.EscapeDataString(name)}" +
                $"&updatedSurname={Uri.EscapeDataString(surname)}" +
                $"&updatedEmail={Uri.EscapeDataString(email)}",
                null);

            await ThrowIfFailedAsync(response);

            // Update the logged-in user's details after a successful response.
            CurrentUserName = name;
            CurrentUserSurname = surname;
            CurrentUserEmail = email;
        }
    }
}
