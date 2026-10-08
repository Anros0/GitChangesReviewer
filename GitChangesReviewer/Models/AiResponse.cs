using Newtonsoft.Json;
using GitChangesReviewer.Helpers;

namespace GitChangesReviewer.Models
{
    public class AiResponse
    {
        public string Model { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        public Message Message { get; set; }

        [JsonProperty("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonProperty("eval_count")]
        public int EvalCount { get; set; }

        [JsonProperty("total_duration")]
        public long TotalDuration { get; set; }

        public TimeSpan TotalTime => TimeSpanHelper.FromNanoseconds(TotalDuration);

        public int TotalCount => PromptEvalCount + EvalCount;
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
