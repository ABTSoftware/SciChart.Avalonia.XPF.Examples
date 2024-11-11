using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using SciChart.Core.Utility;
using MessageBox = System.Windows.MessageBox;

namespace SciChart.Examples.Demo.Lib.Helpers.ProjectExport
{
    public static class DirectoryHelper
    {
        public static string GetFileNameFromPath(string path)
        {
            var index = path.LastIndexOf(@"\", StringComparison.InvariantCulture);

            if (index < 0)
            {
                index = path.LastIndexOf(@"/", StringComparison.InvariantCulture);
            }

            if (index > 0)
            {
                return path.Substring(index + 1, path.Length - index - 1);
            }

            return null;
        }

        public static bool IsValidDirectoryPath(string path, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(path))
            {
                error = "Path cannot be empty";
                return false;
            }

            if (SciChartRuntimeInfo.IsWindowsPlatform)
            {
                if (path.Length < 3)
                {
                    error = "Please set at least a drive";
                    return false;
                }

                if (!Regex.IsMatch(path, @"^[a-zA-Z]:\\.*$"))
                {
                    error = @"Drive should be set in the following format: ""_:\"", where ""_"" is a drive letter";
                    return false;
                }

                var invalidPathChars = new string(Path.GetInvalidPathChars()) + @":/?*""";

                if (Regex.IsMatch(path, @"^[a-zA-Z]:\\.*[" + Regex.Escape(invalidPathChars) + @"].*"))
                {
                    error = "The given path's format is not supported";
                    return false;
                }

                if (Regex.IsMatch(path, @"^.*\\(.*\.+|\.{2,}.*)($|\\)"))
                {
                    error = @"The given path with ""."" in folder name is not supported";
                    return false;
                }
            }
            else if (SciChartRuntimeInfo.IsLinuxPlatform)
            {
                if (path.Length < 2)
                {
                    error = "Please set at least one directory";
                    return false;
                }

                if (!Regex.IsMatch(path, @"^\.?\/.*$"))
                {
                    error = "Path should be set in the following format: './path' or '/path'";
                    return false;
                }

                var invalidPathChars = new string(Path.GetInvalidPathChars());

                if (Regex.IsMatch(path, @"^\.?\/.*[" + Regex.Escape(invalidPathChars) + @"].*"))
                {
                    error = "The given path's format is not supported";
                    return false;
                }
            }

            if (File.Exists(path) || !Directory.Exists(path))
            {
                error = $"Path '{path}' is not found or inaccessible";
                return false;
            }

            return true;
        }

        public static bool HasWriteAccessToDirectory(string path, out string error)
        {
            error = null;
            var fullPath = Path.Combine(path, "file.temp");
            try
            {
                if (!Directory.Exists(path))
                {
                    // We cannot verify write access to a non-existent folder
                    return true;
                }

                using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
                fs.WriteByte(0xff);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                error = "This location is not accessible for writing.";
                return false;
            }
            catch
            {
                error = "This location is not found or inaccessible.";
                return false;
            }
            finally
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
        }

        public static string GetPathForExport(string defaultPath)
        {
            var isValidPath = false;

            string selectedPath = null;

            while (!isValidPath)
            {
                var dialog = new FolderBrowserDialog { SelectedPath = defaultPath };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    isValidPath = HasWriteAccessToDirectory(dialog.SelectedPath, out string error);

                    if (isValidPath)
                    {
                        selectedPath = dialog.SelectedPath;
                    }
                    else
                    {
                        MessageBox.Show(error, "SciChart", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    isValidPath = true;
                }
            }

            return selectedPath;
        }
    }
}