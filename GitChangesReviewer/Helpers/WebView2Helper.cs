using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;
using System.Windows;

namespace GitChangesReviewer.Helpers
{
    public static class WebView2Helper
    {
        // 1. Регистрируем присоединяемое свойство
        public static readonly DependencyProperty HtmlContentProperty =
            DependencyProperty.RegisterAttached(
                "HtmlContent",
                typeof(string),
                typeof(WebView2Helper),
                new PropertyMetadata(null, OnHtmlContentChanged));

        public static string GetHtmlContent(DependencyObject obj) => (string)obj.GetValue(HtmlContentProperty);
        public static void SetHtmlContent(DependencyObject obj, string value) => obj.SetValue(HtmlContentProperty, value);

        // 2. Обработчик изменения свойства
        private static async void OnHtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView2 webView && e.NewValue is string html)
            {
                await UpdateHtmlAsync(webView, html);
            }
        }

        // 3. Логика безопасного обновления с учетом инициализации движка
        private static async Task UpdateHtmlAsync(WebView2 webView, string html)
        {
            if (string.IsNullOrEmpty(html)) return;

            // Если движок уже готов, сразу рендерим
            if (webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.NavigateToString(html);
            }
            else
            {
                // Если еще не готов, ждем события инициализации
                void OnInitialized(object sender, CoreWebView2InitializationCompletedEventArgs args)
                {
                    webView.CoreWebView2InitializationCompleted -= OnInitialized;
                    if (args.IsSuccess && webView.CoreWebView2 != null)
                    {
                        webView.CoreWebView2.NavigateToString(html);
                    }
                }

                webView.CoreWebView2InitializationCompleted += OnInitialized;

                // Принудительно запускаем инициализацию, если она еще не началась
                await webView.EnsureCoreWebView2Async();
            }
        }
    }
}