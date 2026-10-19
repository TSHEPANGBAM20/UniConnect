using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UniConnect.MAUI.Models;

namespace UniConnect.MAUI.Services
{
    // Single source of truth for talking to the Spring Boot backend.
    // Every ViewModel calls into here rather than using HttpClient directly.
    public class ApiService
    {
        private readonly HttpClient _http;

        // Android emulator uses 10.0.2.2 to reach the host machine's localhost.
        // Change this once the backend is actually hosted somewhere shared.
        //private const string BaseUrl = "http://10.0.2.2:8080"; //the base url

#if ANDROID
    // Points to your development PC from the Android emulator
    public static string BaseUrl = "http://10.0.2.2:8080";
#elif IOS
    // Points to your development Mac/PC from the iOS simulator
    public static string BaseUrl = "http://localhost:8080";
#else
    // Fallback for production or other platforms
    public static string BaseUrl = "http://localhost:8080";
#endif

        public int? CurrentUserId { get; private set; }
        public string CurrentUserName { get; private set; }
        public string? CurrentUserEmail { get; private set; }

        public ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        private void SetAuthToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // ---------- Auth ----------

        // ENDPOINT: POST /auth/login                    [backend: AuthController.loginUser - EXISTS]
        // Body    : { "email", "password" }
        // Returns : Student JSON { id, name, surname, email, password }  (BACKEND: please stop returning the password hash)
        // Errors  : currently 500 for wrong credentials; should be 401.
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("/auth/login", request); //points to login endpoint
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            CurrentUserName = result?.Name;
            CurrentUserEmail = result?.Email;
            CurrentUserId = result is { Id: > 0 } ? result.Id : null; // used as requesterId / studentId below
            return result;
        }

        // ENDPOINT: POST /auth/register                 [backend: AuthController.registerUser - EXISTS]
        // Body    : { "name", "surname", "email", "password" }
        // Errors  : currently 500 when the email exists; should be 409.
        public async Task<UserModel> RegisterAsync(RegisterRequest request)
        {
            var response = await _http.PostAsJsonAsync("/auth/register", request); // points to 
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        // ---------- Projects ----------

        // ENDPOINT: GET /projects                       [MISMATCH - backend only has POST /projects/showStudentProjects?studentId=]
        // Wanted  : list of { projectId, projectName, endDate, role } for the logged-in student.
        //           The backend's Project JSON has "id" (not projectId) and no role - see docs/API_PROJECT_DETAILS.md
        public async Task<List<ProjectModel>> GetProjectsAsync()
        {
            var response = await _http.GetAsync("/projects");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProjectModel>>();
        }

        // ENDPOINT: POST /projects                      [MISMATCH - backend has POST /projects/createProject?id={studentId} with a Project body]
        public async Task<ProjectModel> CreateProjectAsync(CreateProjectRequest request)
        {
            var response = await _http.PostAsJsonAsync("/projects", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectModel>();
        }

        // ---------- Project details & membership ----------
        // Full request/response examples and the permission rules: docs/API_PROJECT_DETAILS.md
        // Until real tokens exist the logged-in student's id is sent as "requesterId"/"studentId"
        // and the BACKEND MUST check that student's role in the project (LEADER vs MEMBER).

        // ENDPOINT: GET /projects/{projectId}/details?studentId={me}      [NEW - backend to build]
        // Returns : { projectId, projectName, projectDescription, startDate, endDate,
        //             status, projectFileName, projectCode, myRole }   (myRole = LEADER | MEMBER for studentId)
        // Errors  : 404 project not found; 403 studentId is not a member.
        public async Task<ProjectDetailsModel> GetProjectDetailsAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/details?studentId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
            return (await response.Content.ReadFromJsonAsync<ProjectDetailsModel>())!;
        }

        // ENDPOINT: GET /projects/{projectId}/members                      [NEW - backend to build]
        // Returns : [ { studentId, name, surname, email, role } ]  role = LEADER | MEMBER.
        //           Do NOT return the password hash.
        public async Task<List<ProjectMemberModel>> GetProjectMembersAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/members");
            await ThrowIfFailedAsync(response);
            return await response.Content.ReadFromJsonAsync<List<ProjectMemberModel>>() ?? new();
        }

        // ENDPOINT: POST /projects/addMember?projectId={id}&email={email}&requesterId={me}
        //           [EXISTS: ProjectsController.addMember - but it has NO leader check and no requesterId yet]
        // Leader only (403 otherwise). 404 unknown email; 409 already a member.
        public async Task AddMemberAsync(int projectId, string email)
        {
            var response = await _http.PostAsync(
                $"/projects/addMember?projectId={projectId}&email={Uri.EscapeDataString(email)}&requesterId={RequireUserId()}",
                null);
            await ThrowIfFailedAsync(response);
        }

        // ENDPOINT: DELETE /projects/{projectId}/members/{studentId}?requesterId={me}   [NEW - backend to build]
        // Leader removing someone else: allowed. A member leaving (studentId == requesterId): allowed.
        // The LEADER can never be removed/leave (400). Anyone else removing another member: 403.
        public async Task RemoveMemberAsync(int projectId, int studentId)
        {
            var response = await _http.DeleteAsync(
                $"/projects/{projectId}/members/{studentId}?requesterId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
        }

        // ENDPOINT: DELETE /projects/{projectId}?requesterId={me}          [NEW - service deleteProject exists, no endpoint]
        // Leader only (403 otherwise). Must also delete the project's members, tasks and chat (or cascade).
        public async Task DisbandProjectAsync(int projectId)
        {
            var response = await _http.DeleteAsync($"/projects/{projectId}?requesterId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
        }

        // ENDPOINT: POST /projects/joinByCode?projectCode={6 digits}&email={myEmail}
        //           [EXISTS: ProjectsController.joinByProjectCode - see the bugs listed in docs/API_PROJECT_DETAILS.md]
        // Returns : the project the student just joined. This method accepts either
        //           { "projectId": 7 } or the current { "project": { "id": 7 } } shape.
        // Errors  : 404 no project with that code; 409 already a member.
        public async Task<int> JoinProjectByCodeAsync(int projectCode)
        {
            string email = CurrentUserEmail ?? throw new InvalidOperationException("Not logged in");
            var response = await _http.PostAsync(
                $"/projects/joinByCode?projectCode={projectCode}&email={Uri.EscapeDataString(email)}", null);
            await ThrowIfFailedAsync(response);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            if (root.TryGetProperty("projectId", out var pid) && pid.TryGetInt32(out int id1)) return id1;
            if (root.TryGetProperty("project", out var proj) &&
                proj.TryGetProperty("id", out var pid2) && pid2.TryGetInt32(out int id2)) return id2;
            throw new InvalidOperationException("The server did not say which project was joined.");
        }

        // ---------- Project files (documents and pictures) ----------
        // Separate from the single "project file" that POST /projects/uploadProject stores on the Project
        // (that endpoint keeps working; it only ever holds ONE file and a new upload replaces it).
        // These endpoints hold MANY files per project. Details + a Spring skeleton: docs/API_PROJECT_DETAILS.md

        // ENDPOINT: GET /projects/{projectId}/files?studentId={me}                [NEW - backend to build]
        // Returns : [ { fileId, fileName, contentType, sizeBytes, uploadedById, uploadedByName, uploadedAt } ]
        // Access  : any member of the project (403 otherwise).
        public async Task<List<ProjectFileModel>> GetProjectFilesAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/files?studentId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
            return await response.Content.ReadFromJsonAsync<List<ProjectFileModel>>() ?? new();
        }

        // ENDPOINT: POST /projects/{projectId}/files?uploaderId={me}              [NEW - backend to build]
        // Body    : multipart/form-data with ONE part named "file".
        // Returns : 201 + the created file { fileId, fileName, contentType, sizeBytes, uploadedById, uploadedByName, uploadedAt }
        // Access  : any member of the project (403 otherwise).
        // Rules   : max 10 MB (raise spring.servlet.multipart.max-file-size / max-request-size, default is only 1 MB);
        //           413 if too big; 415 if the type is not allowed. The app only offers
        //           .pdf .doc .docx .ppt .pptx .xls .xlsx .txt .zip .png .jpg .jpeg - the BACKEND MUST validate too.
        //           Never trust the client file name: strip any path ("../") and store as {uuid}_{safeName}.
        public async Task<ProjectFileModel> UploadProjectFileAsync(int projectId, string fileName, Stream content, string? contentType)
        {
            using var form = new MultipartFormDataContent();
            var part = new StreamContent(content);
            part.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
            form.Add(part, "file", fileName);

            var response = await _http.PostAsync($"/projects/{projectId}/files?uploaderId={RequireUserId()}", form);
            await ThrowIfFailedAsync(response);
            return (await response.Content.ReadFromJsonAsync<ProjectFileModel>())!;
        }

        // ENDPOINT: GET /projects/{projectId}/files/{fileId}/download?studentId={me}   [NEW - backend to build]
        // Returns : the raw file bytes with Content-Type and "Content-Disposition: attachment; filename=...".
        // Access  : any member of the project (403 otherwise); 404 if the file does not exist.
        // The app saves the bytes to its cache folder and opens them with the phone's viewer.
        public async Task<string> DownloadProjectFileAsync(int projectId, int fileId, string fileName)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/files/{fileId}/download?studentId={RequireUserId()}");
            await ThrowIfFailedAsync(response);

            string safeName = Path.GetFileName(fileName);
            string path = Path.Combine(FileSystem.CacheDirectory, safeName);
            await File.WriteAllBytesAsync(path, await response.Content.ReadAsByteArrayAsync());
            return path;
        }

        // ENDPOINT: DELETE /projects/{projectId}/files/{fileId}?requesterId={me}  [NEW - backend to build]
        // Access  : the person who uploaded the file OR the project's LEADER; anyone else gets 403.
        //           Delete the file from disk as well as the database row.
        public async Task DeleteProjectFileAsync(int projectId, int fileId)
        {
            var response = await _http.DeleteAsync($"/projects/{projectId}/files/{fileId}?requesterId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
        }

        private int RequireUserId() =>
            CurrentUserId ?? throw new InvalidOperationException("Not logged in");

        // Turns a failed response into an exception that carries the status code and the
        // server's "message" (if it sends one). BACKEND: please send { "message": "..." } on errors.
        private static async Task ThrowIfFailedAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string message = $"Request failed ({(int)response.StatusCode})";
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("message", out var m) && m.GetString() is { Length: > 0 } text)
                    message = text;
            }
            catch (Exception) { /* body was empty or not JSON - keep the default message */ }

            throw new HttpRequestException(message, null, response.StatusCode);
        }

        // ---------- Tasks ----------

        // ENDPOINT: GET /projects/{projectId}/tasks     [MISMATCH - backend has GET /tasks/all]
        public async Task<List<TaskModel>> GetTasksAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/tasks");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        }

        // ENDPOINT: PUT /tasks/{taskId}/status          [MISMATCH - backend has PUT /tasks/{taskId}]
        // Body    : { "status": "NOT_STARTED" | "IN_PROGRESS" | "DONE" }
        public async Task UpdateTaskStatusAsync(int taskId, string status)
        {
            var response = await _http.PutAsJsonAsync(
                $"/tasks/{taskId}/status", new UpdateTaskStatusRequest { Status = status });
            response.EnsureSuccessStatusCode();
        }

        // ---------- Chat ----------

        // ENDPOINT: GET /projects/{projectId}/chat      [NOT BUILT on the backend yet]
        public async Task<List<MessageModel>> GetMessagesAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/chat");
            response.EnsureSuccessStatusCode();
            var messages = await response.Content.ReadFromJsonAsync<List<MessageModel>>();
            foreach (var m in messages)
                m.IsOwnMessage = m.SenderId == CurrentUserId;
            return messages;
        }

        // ENDPOINT: POST /projects/{projectId}/chat/messages   [NOT BUILT on the backend yet]
        public async Task SendMessageAsync(int projectId, string text)
        {
            var response = await _http.PostAsJsonAsync(
                $"/projects/{projectId}/chat/messages", new SendMessageRequest { Message = text });
            response.EnsureSuccessStatusCode();
        }
    }
}
