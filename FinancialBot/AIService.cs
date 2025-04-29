using Google.Ai.Generativelanguage.V1Beta2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// Определяем пространство имен для удобства
//using Content = Google.Ai.Generativelanguage.V1Beta2.;

namespace FinancialBot
{
    public class AIService
    {
        private readonly string? _apiKey;
        private readonly string modelName = "gemini-2.5-flash-latest"; // Имя модели отдельно
        private readonly string _endpoint;

        public AIService(IConfiguration configuration)
        {
            _apiKey = configuration["AppSettings:GeminiApiKey"];
            _endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}";
        }

        public async Task<string?> GetGeminiResponse()
        {
            using (var client = new HttpClient())
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                    new { parts = new[] { new { text = "Опиши детально инвестиционный план в наше время. " +
                    "Формат текста должен быть адаптирован под Telegram, он должен быть краисвым и корректно отображаться." } } }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseString, options);

                string text = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;

                return text;
            }
        }

        // Классы для десериализации
        public class GeminiResponse
        {
            public Candidate[] Candidates { get; set; }
        }

        public class Candidate
        {
            public Content Content { get; set; }
        }

        public class Content
        {
            public Part[] Parts { get; set; }
        }

        public class Part
        {
            public string Text { get; set; }
        }
    }
}
