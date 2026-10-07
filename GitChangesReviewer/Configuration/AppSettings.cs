namespace GitChangesReviewer.Configuration
{
    public class AppSettings
    {
        public List<AiModel> Models { get; set; }
    }

    public class AiModel
    {
        public string ModelName { get; set; }
        public string Url { get; set; }
    }
}
