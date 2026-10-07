using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Windows;

namespace GitChangesReviewer.Services
{
    public class DialogService(ILogger<DialogService> logger)
    {
        public string SelectFolder(string start = null)
        {
            var dialog = new OpenFolderDialog();
            if (start != null)
                dialog.InitialDirectory = start;
            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public void ShowError(Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            var message = ex.InnerException == null
                ? ex.Message
                : $"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.InnerException.Message}";

            ShowError(message);
        }

        public void ShowMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }


        public void ShowError(string message)
        {
            ShowMessage(message, "Error");
        }
    }
}
