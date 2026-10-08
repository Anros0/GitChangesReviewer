using GitChangesReviewer.Configuration;
using GitChangesReviewer.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using GitChangesReviewer.Exceptions;
using Newtonsoft.Json;

namespace GitChangesReviewer.Services
{
    public class AiService(AppSettings aiSettings)
    {
        public async Task<AiResponse> SendToAiAsync(string codeChanges, AiModel aiModel)
        {
            if (string.IsNullOrWhiteSpace(codeChanges))
                throw new ArgumentNullException(nameof(codeChanges));

            aiModel ??= aiSettings.Models.FirstOrDefault();

            if (aiModel == null)
                throw new Exception("Не найдено ни одной модели ИИ в конфигурации. Проверьте appsettings.json.");

            var jsonPayload = PreparePayload(codeChanges, aiModel);
            var responseString = await CallAi(aiModel.Url, jsonPayload);

            try
            {
                return JsonConvert.DeserializeObject<AiResponse>(responseString);
            }
            catch(Exception ex)
            {
                throw new AppException("Не удалось распарсить ответ ИИ.", ex);
            }
        }

        private static string PreparePayload(string codeChanges, AiModel aiModel)
        {
            const string systemPrompt =
                "Ты — эксперт-разработчик и код-ревьюер. Твоя задача — проанализировать изменения в коде (git diff). " +
                "Найди потенциальные баги, проблемы с безопасностью, нарушения best practices и предложи улучшения. " +
                "Отвечай кратко, по делу, на русском языке. Используй Markdown для форматирования.";

            var userPrompt = $"Проанализируйте следующие изменения в коде:\n\n{codeChanges}";

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
                    num_ctx = 8192 // Контекстное окно (увеличьте, если изменения очень большие)
                }
            };
            var jsonPayload = JsonConvert.SerializeObject(payload);

            return jsonPayload;
        }

        private static async Task<string> CallAi(string url, string jsonPayload)
        {
            string responseString;
            try
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10); // ИИ может думать долго

                var response = await client.PostAsync(url, content);

                responseString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(responseString);
            }
            catch (Exception ex)
            {
                throw new AppException(
                    $"❌ Ошибка при обращении к ИИ: {ex.Message}\nУбедитесь, что Ollama запущена (ollama serve).");
            }

            return responseString;
        }
    }
}
