using InterviewDanica.Api.Data;
using InterviewDanica.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InterviewDanica.Api.Tests;

public static class Utils {
    public static string UString() {
        return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()),"[^a-zA-Z0-9]","");
    }
    public static DBContext GetMockDB() {
        var options = new DbContextOptionsBuilder<DBContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        return new DBContext(options);
    }
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; // Ensure case-insensitive deserialization
    public static StringContent Serialize(object obj) {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json,Encoding.UTF8,"application/json");
    }
    public static List<T> DeserializeList<T>(string json) => JsonSerializer.Deserialize<List<T>>(json,Options)!;
    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json,Options)!;
    public static ILogger<T> GetMockLog<T>() {
        return new MockLogger<T>();
    }
    public class MockLogger<T> : ILogger<T> {
        private readonly List<string> logMessages = new();
        public IReadOnlyList<string> LogMessages => logMessages.AsReadOnly();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            return null;
        }
        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState,Exception?,string> formatter) {
            var message = formatter(state,exception);
            logMessages.Add($"[{logLevel}] {message}");
        }
    }
}
internal class ApiClient<TDTO> {
    private readonly string url;
    public static HttpClient Client { get; } = new HttpClient();
    public ApiClient(string table) {
        url = "http://localhost:5254/api/" + table;
    }
    async public Task authorize() {
        /* Awaiting API, just in case */
        for(int i = 0, l = 10; i < l; i++) { 
            try {
                var loginApi = new ApiClient<LoginResponseDTO>("login");
                var response = await loginApi.PostAndRead(new { username = "admin",password = "password" });
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",response.Token);
                return;
            } catch {
                await Task.Delay(333);
            }
        }
    }
    public async Task<List<TDTO>> GetAll() {
        var response = await Client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        return Utils.DeserializeList<TDTO>(json);
    }
    public async Task<TDTO> GetById(Guid id) {
        var response = await Client.GetAsync($"{url}/{id}");
        var json = await response.Content.ReadAsStringAsync();
        return Utils.Deserialize<TDTO>(json);
    }
    public async Task<HttpResponseMessage> Post(object request) {
        return await Client.PostAsync(url,Utils.Serialize(request));
    }
    public async Task<TDTO> PostAndRead(object request) {
        var response = await Post(request);
        var json = await response.Content.ReadAsStringAsync();
        return Utils.Deserialize<TDTO>(json);
    }
    public async Task<HttpResponseMessage> Put(Guid id,object request) {
        return await Client.PutAsync($"{url}/{id}",Utils.Serialize(request));
    }
    public async Task<HttpResponseMessage> Delete(Guid id) {
        return await Client.DeleteAsync($"{url}/{id}");
    }
}