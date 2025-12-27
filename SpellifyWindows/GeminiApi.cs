using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spellify;

static class GeminiApi
{
    private static readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(30) };
    
    public static string? LastError { get; private set; }
    
    public static async Task<string?> FixText(string text)
    {
        LastError = null;
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Settings.Model}:generateContent?key={Settings.ApiKey}";
            
            var prompt = $"Исправь орфографические и пунктуационные ошибки. Верни ТОЛЬКО исправленный текст:\n\n{text}";
            
            var body = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.1, maxOutputTokens = 2048 }
            };
            
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                LastError = $"HTTP {(int)response.StatusCode}: {responseJson}";
                return null;
            }
            
            using var doc = JsonDocument.Parse(responseJson);
            
            // Проверяем на ошибку в ответе
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                LastError = error.GetProperty("message").GetString();
                return null;
            }
            
            var result = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            
            return result?.Trim();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }
}
