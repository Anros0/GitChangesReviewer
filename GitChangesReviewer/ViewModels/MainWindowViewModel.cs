using DevExpress.Mvvm;
using GitChangesReviewer.Configuration;
using GitChangesReviewer.Services;
using GitChangesReviewer.ViewModels.Abstract;
using Markdig;
using System.IO;

namespace GitChangesReviewer.ViewModels
{
    public class MainWindowViewModel : BusyViewModel
    {
        private readonly GitService _gitService;
        private readonly AiService _aiService;
        private readonly AppSettings _appSettings;

        public MainWindowViewModel(DialogService dialogService, 
            GitService gitService,
            AiService aiService, 
            AppSettings appSettings ) : base(dialogService)
        {
            _gitService = gitService;
            _aiService = aiService;
            _appSettings = appSettings;
            SelectFolderCommand = new DelegateCommand(SelectFolder);
            StartReviewCommand = GetAsyncCommand(StartReview, canExecute: CanStartReview);
            LoadedCommand = GetAsyncCommand(InitAsync);

            Models = appSettings.Models;
            SelectedModel = Models.FirstOrDefault();
        }

        public DelegateCommand SelectFolderCommand { get; }
        public AsyncCommand StartReviewCommand { get; }
        public AsyncCommand LoadedCommand { get; }

        public TimeSpan? TotalTime
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
                StartReviewCommand.RaiseCanExecuteChanged();
            }
        }

        public int TotalCount
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
                StartReviewCommand.RaiseCanExecuteChanged();
            }
        }

        public string Folder
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
                StartReviewCommand.RaiseCanExecuteChanged();
            }
        }

        public string Changes
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public string ReviewHtml
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTabIndex
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public List<AiModel> Models
        {
            get;
            set
            {
                if (Equals(value, field)) return;
                field = value;
                OnPropertyChanged();
            }
        }
    

        public AiModel SelectedModel
        {
            get;
            set
            {
                if (Equals(value, field)) return;
                field = value;
                OnPropertyChanged();
            }
        }


        private async Task InitAsync()
        {
            if (CanStartReview())
                await StartReview();
        }

        private void SelectFolder()
        {
            Folder = DialogService.SelectFolder();
        }

        public bool CanStartReview() => !string.IsNullOrEmpty(Folder) && Directory.Exists(Folder);

        private async Task StartReview()
        {
            SelectedTabIndex = 0;
            TotalCount = 0;
            TotalTime = null;

            ReviewHtml = "";
            Changes = await _gitService.GetChangesAsync(Folder);

            if (string.IsNullOrWhiteSpace(Changes))
            {
                Changes = "НЕТ ИЗМЕНЕНИЙ";
                return;
            }

            var aiResponse = await _aiService.SendToAiAsync(Changes, SelectedModel);

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            ReviewHtml = Markdown.ToHtml(aiResponse.Message.Content, pipeline);
            TotalTime = aiResponse.TotalTime;
            TotalCount = aiResponse.TotalCount;

            SelectedTabIndex = 1;
        }
    }
}
