using System.Net;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using UniConnect.MAUI.Models;

namespace UniConnect.MAUI.Services
{
    
    public class ApiService
    {
        private readonly HttpClient _http;

       

#if ANDROID
    // points to abfroid device
    public static string BaseUrl = "http://10.0.2.2:8080";
#elif IOS
    // points to ios device
    public static string BaseUrl = "http://localhost:8080";
#else
    public static string BaseUrl = "http://localhost:8080";
#endif

         public long CurrentUserId { get; private set; }
        public string CurrentUserName { get; private set; }
        public string CurrentUserEmail { get; private set; }

        public ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        private void SetAuthToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

       

       
        //auths
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("/auth/login", request); //points to login endpoint
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
             CurrentUserName = result.Name;
            CurrentUserId = result.UserId;
            CurrentUserEmail = result.Email; 
            Console.WriteLine(CurrentUserId);//tshepang // used as requesterId / studentId below
            return result;
        }

        public async Task<UserModel> RegisterAsync(RegisterRequest request)
        {
            var response = await _http.PostAsJsonAsync("/auth/register", request); // points to 
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }



        public async Task<List<ProjectMemberModel>> AddMemberAsync(long projectId,string email){
        var response = await _http.PostAsync(
        $"/projects/addMember?projectId={projectId}&email={email}",
        null);

    await ThrowIfFailedAsync(response);

    return await response.Content
        .ReadFromJsonAsync<List<ProjectMemberModel>>() ?? new();
}


        //projects
        public async Task<List<ProjectModel>> GetProjectsAsync(long id)
        {
            var response = await _http.PostAsync($"/projects/showStudentProjects?studentId={id}",null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProjectModel>>();
        }

       public async Task<ProjectModel> CreateProjectAsync(CreateProjectRequest request,long id)
        {
            var response = await _http.PostAsJsonAsync($"/projects/createProject?id={id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectModel>();
        }

     
        public async Task<ProjectDetailsModel> GetProjectDetailsAsync(long projectId)
        {
            var response = await _http.PostAsync($"/projects/getProjectInfo?projectId={projectId}",null);
            await ThrowIfFailedAsync(response);
            return await response.Content.ReadFromJsonAsync<ProjectDetailsModel>();
        }

        
        public async Task<List< ProjectMemberModel>> GetProjectMembersAsync(long projectId) 
        { 
            var response = await _http.PostAsync( $"/projects/showStudentsInProject?projectId={projectId}", null );
            await ThrowIfFailedAsync(response); 
            var members = await response.Content .ReadFromJsonAsync<List<ProjectMemberModel>>();
            return members;
        }
    
        public async Task RemoveMemberAsync(long projectId, string studentEmail)
        {
            var response = await _http.DeleteAsync(
                $"/projects/removeMemberFromgroup?studentEmail={studentEmail}&projectId={projectId}");
            await ThrowIfFailedAsync(response);
        }

        
        public async Task DisbandProjectAsync(long projectId)
        {
            var response = await _http.DeleteAsync($"/projects/disbandGroup?projectId={projectId}");
            await ThrowIfFailedAsync(response);
        }

        public async Task<long> JoinProjectByCodeAsync(int projectCode)
        {
            string email = CurrentUserEmail ?? throw new InvalidOperationException("Not logged in");
            var response = await _http.PostAsync(
                $"/projects/joinByCode?projectCode={projectCode}&email={email}", null);
            await ThrowIfFailedAsync(response);

            var member = await response.Content.ReadFromJsonAsync<ProjectMemberModel>();
            if (member.Project == null)
            throw new InvalidOperationException("The server did not say which project was joined.");

            return member.Project.ProjectId;
        }

        
        public async Task<List<ProjectFileModel>> GetProjectFilesAsync(long projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/files?studentId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
            return await response.Content.ReadFromJsonAsync<List<ProjectFileModel>>() ?? new();
        }

        public async Task<ProjectFileModel> UploadProjectFileAsync(long projectId, string fileName, Stream content, string? contentType)
        {
            using var form = new MultipartFormDataContent();
            var part = new StreamContent(content);
            part.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
            form.Add(part, "projectFile", fileName);

            var response = await _http.PostAsync($"/projects/uploadProject?projectId={projectId}", form);
            await ThrowIfFailedAsync(response);
            return (await response.Content.ReadFromJsonAsync<ProjectFileModel>())!;
        }

      
       public async Task<string> DownloadProjectFileAsync(long projectId, long fileId, string fileName) { 
        var response = await _http.GetAsync( $"/projects/downloadProject/{projectId}/download"); 
        await ThrowIfFailedAsync(response); string safeName = Path.GetFileName(fileName); 
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "project-file"; string path = Path.Combine(FileSystem.CacheDirectory, safeName); 
        await File.WriteAllBytesAsync( path, await response.Content.ReadAsByteArrayAsync()); 
        return path; 
        }


       
        public async Task DeleteProjectFileAsync(long projectId, long fileId)
        {
            var response = await _http.DeleteAsync($"/projects/{projectId}/files/{fileId}?requesterId={RequireUserId()}");
            await ThrowIfFailedAsync(response);
        }

        private long RequireUserId() =>
            CurrentUserId; //throw new InvalidOperationException("Not logged in")

        // Turns a failed response into an exception that carries the status code and the
      
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

        //tasks

        public async Task<TaskModel> CreateTaskAsync(long projectId,CreateTaskRequest request){
            if (projectId <= 0)
                throw new Exception("A valid project is required.");

            if (request == null)
                throw new Exception("Task details are required.");

            // Map the frontend request to the field names used by the
            // backend Task entity's controller (title and description).
            var taskData = new
            {
                Title = request.TaskTitle,
                Description = request.TaskDescription,
                Priority = request.Priority,
                DueDate = request.DueDate,
                Status = "TODO"
            };

            var response = await _http.PostAsJsonAsync($"/tasks/createTask/{projectId}",taskData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TaskModel>()?? throw new Exception("The backend did not return the created task.");
        }


        public async Task <List<TaskModel>> GetTasksAsync(long projectId)
        { 
            var response = await _http.GetAsync($"/tasks/getTaskByProject/{projectId}");
            response.EnsureSuccessStatusCode();
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskModel>>();
            return tasks?? new List<TaskModel>();
        }

        public async Task UpdateTaskStatusAsync(long taskId, string status)
        {
            var request = new UpdateTaskStatusRequest
            {
            Status = status
            };

            var response = await _http.PutAsJsonAsync($"/tasks/updateStatus/{taskId}/status", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var response = await _http.DeleteAsync($"/tasks/deleteTask/{taskId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<TaskModel>> GetCalendarTasksAsync()
        {
            var response = await _http.GetAsync("/tasks/TaskCalendar/calendar");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        }



        //chat
        public async Task<List<MessageModel>> GetMessagesAsync(long projectId)
        {
            var response = await _http.PostAsync($"/chat/chatMessages?projectId={projectId}",null);
            response.EnsureSuccessStatusCode();
            var messages = await response.Content.ReadFromJsonAsync<List<MessageModel>>() ?? new List<MessageModel>();
            foreach (var m in messages)
                m.IsOwnMessage = m.SenderId == CurrentUserId;
            return messages;
        }

        public async Task SendMessageAsync(long projectId, string text)
        {
            var response = await _http.PostAsJsonAsync(
                $"/chat/sendMessage?projectId={projectId}&senderId={CurrentUserId}", new SendMessageRequest { Message = text });
            response.EnsureSuccessStatusCode();
        }
    }
}
