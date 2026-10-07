
using DevExpress.Mvvm;
using GitChangesReviewer.Services;

namespace GitChangesReviewer.ViewModels.Abstract
{
    public class BusyViewModel(DialogService dialogService) : BaseViewModel
    {
        protected DialogService DialogService = dialogService;

        public bool IsBusy
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public string BusyText
        {
            get;
            set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand GetAsyncCommand(Action action, string busyText = null)
        {
            return new AsyncCommand(() => PerformBusyOperationAsync(action, busyText));
        }

        public AsyncCommand GetAsyncCommand(Func<Task> action, string busyText = null, Func<bool> canExecute = null)
        {
            return new AsyncCommand(() => PerformBusyOperationAsync(action, busyText), canExecute);
        }




        private Task PerformBusyOperationAsync(Action action, string busyText = null)
        {
            return PerformBusyOperationAsync(() =>
            {
                action();
                return Task.CompletedTask;
            }, busyText);
        }

        private async Task PerformBusyOperationAsync(Func<Task> action, string busyText = null)
        {
            if (action != null)
            {
                IsBusy = true;
                BusyText = !string.IsNullOrEmpty(busyText) ? busyText : "Working...";
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    dialogService.ShowError(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
