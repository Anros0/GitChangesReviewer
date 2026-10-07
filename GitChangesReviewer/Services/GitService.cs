using System.Diagnostics;
using System.IO;
using System.Text;

namespace GitChangesReviewer.Services
{
    public class GitService
    {
        public async Task<string> GetChangesAsync(string repoPath)
        {
            var fullDiff = new StringBuilder();

            // 1. Получаем изменения в отслеживаемых файлах (tracked)
            var trackedDiff = RunGitCommand(repoPath, "diff HEAD");
            if (!string.IsNullOrWhiteSpace(trackedDiff))
            {
                fullDiff.AppendLine("=== ИЗМЕНЕНИЯ В СУЩЕСТВУЮЩИХ ФАЙЛАХ ===");
                fullDiff.AppendLine(trackedDiff);
            }

            // 2. Получаем список новых, неотслеживаемых файлов (untracked)
            var untrackedFilesList = RunGitCommand(repoPath, "ls-files --others --exclude-standard");

            if (!string.IsNullOrWhiteSpace(untrackedFilesList))
            {
                fullDiff.AppendLine("\n=== НОВЫЕ (НЕОТСЛЕЖИВАЕМЫЕ) ФАЙЛЫ ===");
                var files = untrackedFilesList.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var file in files)
                {
                    var fullPath = Path.Combine(repoPath, file.Trim());
                    if (File.Exists(fullPath))
                    {
                        // Пропускаем бинарные файлы и очень большие файлы, чтобы не сломать контекст ИИ
                        if (IsTextFile(fullPath) && new FileInfo(fullPath).Length < 100000)
                        {
                            fullDiff.AppendLine($"\n--- Новый файл: {file} ---");
                            fullDiff.AppendLine(await File.ReadAllTextAsync(fullPath));
                        }
                    }
                }
            }

            return fullDiff.ToString();
        }

        private static string RunGitCommand(string workingDir, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(processInfo);
            var output = process!.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private static bool IsTextFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            string[] textExtensions = [".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".html", ".css", ".json", ".xml", ".yaml", ".yml", ".md", ".txt", ".sql", ".sh", ".bat", ".ps1"];
            return Array.Exists(textExtensions, e => e == ext);
        }
    }
}
