using System.Net.Http.Json;
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
        private const string BaseUrl = "http://localhost:8080"; //the base url

        public int? CurrentUserId { get; private set; }
        public string CurrentUserName { get; private set; }

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

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("/auth/login", request); //points to login endpoint
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            SetAuthToken(result.Token);
            CurrentUserId = result.UserId;
            CurrentUserName = result.Name;
            return result;
        }

        public async Task<UserModel> RegisterAsync(RegisterRequest request)
        {
            var response = await _http.PostAsJsonAsync("/auth/register", request); // points to 
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        // ---------- Projects ----------

        public async Task<List<ProjectModel>> GetProjectsAsync()
        {
            var response = await _http.GetAsync("/projects");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProjectModel>>();
        }

        public async Task<ProjectModel> CreateProjectAsync(CreateProjectRequest request)
        {
            var response = await _http.PostAsJsonAsync("/projects", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectModel>();
        }

        // ---------- Tasks ----------

        public async Task<List<TaskModel>> GetTasksAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/tasks");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TaskModel>>();
        }

        public async Task UpdateTaskStatusAsync(int taskId, string status)
        {
            var response = await _http.PutAsJsonAsync(
                $"/tasks/{taskId}/status", new UpdateTaskStatusRequest { Status = status });
            response.EnsureSuccessStatusCode();
        }

        // ---------- Chat ----------

        public async Task<List<MessageModel>> GetMessagesAsync(int projectId)
        {
            var response = await _http.GetAsync($"/projects/{projectId}/chat");
            response.EnsureSuccessStatusCode();
            var messages = await response.Content.ReadFromJsonAsync<List<MessageModel>>();
            foreach (var m in messages)
                m.IsOwnMessage = m.SenderId == CurrentUserId;
            return messages;
        }

        public async Task SendMessageAsync(int projectId, string text)
        {
            var response = await _http.PostAsJsonAsync(
                $"/projects/{projectId}/chat/messages", new SendMessageRequest { Message = text });
            response.EnsureSuccessStatusCode();
        }
    }
}
