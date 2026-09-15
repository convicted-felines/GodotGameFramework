#if TOOLS
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GodotGameFramework.Editor.DataTableTools
{
    /// <summary>
    /// 调用 Tools/DataTableGenerator 命令行工具。
    /// </summary>
    internal static class DataTableToolRunner
    {
        private const string ToolProjectRelativePath = "Tools/DataTableGenerator/DataTableGenerator.csproj";
        private const string ToolExeRelativePath = "Tools/DataTableGenerator/bin/Release/net8.0/DataTableGenerator.exe";

        public static string Run(string projectRoot, string command, out bool success)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                success = false;
                return "Project root is invalid.";
            }

            string toolProjectPath = Path.Combine(projectRoot, ToolProjectRelativePath);
            if (!File.Exists(toolProjectPath))
            {
                success = false;
                return $"Tool project not found: {toolProjectPath}";
            }

            var log = new StringBuilder();
            log.AppendLine("=== Build DataTableGenerator ===");
            if (!TryRunProcess("dotnet", $"build \"{toolProjectPath}\" -c Release", projectRoot, log, out string buildError))
            {
                success = false;
                log.AppendLine(buildError);
                return log.ToString();
            }

            string toolExePath = Path.Combine(projectRoot, ToolExeRelativePath);
            if (!File.Exists(toolExePath))
            {
                success = false;
                log.AppendLine($"Tool executable not found: {toolExePath}");
                return log.ToString();
            }

            log.AppendLine();
            log.AppendLine($"=== Run: {command} ===");
            string arguments = $"{command} --root \"{projectRoot}\"";
            if (!TryRunProcess(toolExePath, arguments, projectRoot, log, out string runError))
            {
                success = false;
                log.AppendLine(runError);
                return log.ToString();
            }

            success = true;
            return log.ToString();
        }

        private static bool TryRunProcess(string fileName, string arguments, string workingDirectory, StringBuilder log, out string error)
        {
            error = string.Empty;

            try
            {
                using Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };

                process.Start();
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    log.AppendLine(stdout.TrimEnd());
                }

                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    log.AppendLine(stderr.TrimEnd());
                }

                if (process.ExitCode != 0)
                {
                    error = $"Process exited with code {process.ExitCode}.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
#endif
