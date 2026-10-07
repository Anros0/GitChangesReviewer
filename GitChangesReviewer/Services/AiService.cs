using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using GitChangesReviewer.Configuration;

namespace GitChangesReviewer.Services
{
    public class AiService(AppSettings aiSettings)
    {
        public async Task<string> SendToAiAsync(string codeChanges, AiModel aiModel)
        {
            aiModel ??= aiSettings.Models.FirstOrDefault();

            //const string modelName = "qwen2.5-coder:7b";
            //const string ollamaUrl = "http://localhost:11434/api/chat";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(10); // ИИ может думать долго


            var systemPrompt = "Ты — эксперт-разработчик и код-ревьюер. Твоя задача — проанализировать изменения в коде (git diff). " +
                               "Найди потенциальные баги, проблемы с безопасностью, нарушения best practices и предложи улучшения. " +
                               "Отвечай кратко, по делу, на русском языке. Используй Markdown для форматирования.";

            string userPrompt = $"Проанализируйте следующие изменения в коде:\n\n{codeChanges}";

            // Формируем JSON для Ollama API
            var payload = new
            {
                model = aiModel.ModelName,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                stream = false, // false - ждем полный ответ, true - стриминг (сложнее в реализации)
                options = new
                {
                    temperature = 0.2, // Низкая температура для более точного и строгого анализа
                    num_ctx = 8192     // Контекстное окно (увеличьте, если изменения очень большие)
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(aiModel.Url, content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(responseString);

                // Парсим ответ Ollama
                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetProperty("content").GetString();
                }

                return "Не удалось распарсить ответ ИИ.";
            }
            catch (Exception ex)
            {
                return $"❌ Ошибка при обращении к ИИ: {ex.Message}\nУбедитесь, что Ollama запущена (ollama serve).";
            }
        }
    }
}
