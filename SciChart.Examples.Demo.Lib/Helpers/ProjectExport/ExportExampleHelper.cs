using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SciChart.Core.Utility;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.Navigation;

namespace SciChart.Examples.Demo.Lib.Helpers.ProjectExport
{
    public class ExportExampleHelper
    {
        private static string _scriptPath;
        private static string _exportPath;

        private static string _lastGroup;
        private static IEnumerator<Example> _exportEnumerator;

        private const string FolderName = "ExportedSolutions";
        private const string RegistryKeyString = @"Software\SciChart Ltd\SciChart";

        static ExportExampleHelper()
        {
            DefaultExportPath = AppDomain.CurrentDomain.BaseDirectory + FolderName + "\\";
        }

        public static string DefaultExportPath { get; }

        public static string ScriptPath
        {
            get
            {
                if (_scriptPath == null)
                {
                    return Path.Combine(_exportPath, "CompileExamples.bat"); 
                }

                return _scriptPath;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _scriptPath = value;
                }
            }           
        }

        public static void ExportExamplesToSolutions(IModule module)
        {
            if (SciChartRuntimeInfo.IsXPF) return;

            var basePath = DirectoryHelper.GetPathForExport(DefaultExportPath);

            if (string.IsNullOrEmpty(basePath)) return;

            _exportPath = basePath + "\\" + FolderName + "\\";

            try
            {
                if (File.Exists(ScriptPath))
                    File.Delete(ScriptPath);

                if (Directory.Exists(_exportPath))
                    Directory.Delete(_exportPath, true);

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("A permissions error occurred when deleting or creating example export paths. Please check you are not trying to export in a restricted directory such as C: or Program Files", ex);
            }

            _lastGroup = null;

            _exportEnumerator = module.Examples.Select(x => x.Value).GetEnumerator();

            Navigator.Instance.AfterNavigation += OnAfterNavigation;

            if (_exportEnumerator.MoveNext())
            {
                Navigator.Instance.Navigate(_exportEnumerator.Current);
            }
        }

        private static void OnAfterNavigation(UserControl control, AppPage page)
        {
            control.Loaded += OnControlLoaded;
        }

        private static void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            TimedMethod.Invoke(() =>
            {
                ExportExampleToSolution(ref _lastGroup, _exportEnumerator.Current);

                if (_exportEnumerator.MoveNext())
                {
                    Navigator.Instance.Navigate(_exportEnumerator.Current);
                }
                else
                {
                    Navigator.Instance.AfterNavigation -= OnAfterNavigation;

                    _exportEnumerator?.Dispose();
                }

                if (sender is UserControl control)
                {
                    control.Loaded -= OnControlLoaded;
                }

            }).After(1000).Go();
        }

        public static string TryAutomaticallyFindAssemblies()
        {
            var folderPath = GetFolderFromAssemblyPath();

            if (!string.IsNullOrEmpty(folderPath))
            {
                var isFound = SearchForCoreAssemblies(folderPath);

                return isFound ? folderPath : GetAssemblyPathFromRegistry();
            }

            return null;
        }

        public static bool SearchForCoreAssemblies(string folderPath)
        {
            var result = true;

            foreach (var asmName in ProjectWriter.SciChartAssemblyNames)
            {
                result &= IsAssemblyExist(folderPath, asmName) &&
                          IsAssemblyVersionMatch(folderPath, asmName);
            }

            return result;
        }

        private static void ExportExampleToSolution(ref string lastGroup, Example example)
        {
            string projectName = ProjectWriter.WriteProject(example, _exportPath + @"\", TryAutomaticallyFindAssemblies());

            if (!File.Exists(ScriptPath))
            {
                using (var fs = File.CreateText(ScriptPath))
                {
                    // Write the header
                    fs.WriteLine("REM Compile and run all exported SciChart examples for testing");
                    fs.WriteLine("@echo OFF");                    
                    fs.WriteLine("");
                    fs.Close();
                }                
            }

            if (lastGroup != null && example.Group != lastGroup)
            {
                using (var fs = File.AppendText(ScriptPath))
                {
                    fs.WriteLine("@echo Finished Example Group " + lastGroup + ". Press any key to continue...");
                    fs.WriteLine("pause(0)");
                    fs.WriteLine("");
                }
            }

            lastGroup = example.Group;

            using (var fs = File.AppendText(ScriptPath))
            {
                fs.WriteLine("@echo Building " + projectName);
                fs.WriteLine(@"IF NOT EXIST ""C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin"" @echo VisualStudio folder not exists with the following path: ""C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin"" ");
                fs.WriteLine(@"call ""C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"" /p:Configuration=""Debug"" ""{0}/{0}.csproj"" /p:WarningLevel=0", projectName);
                fs.WriteLine("if ERRORLEVEL 1 (");
                fs.WriteLine("   @echo - Example {0} Failed to compile >> errorlog.txt", projectName);
                fs.WriteLine(") else (");
                fs.WriteLine(@"   start """" ""{0}/bin/Debug/{0}.exe""", projectName);
                fs.WriteLine(")");
                fs.WriteLine("");
            }
        }

        private static string GetFolderFromAssemblyPath()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            if (!string.IsNullOrEmpty(assemblyPath))
            {
                var index = assemblyPath.LastIndexOf(Path.DirectorySeparatorChar);

                if (index > 0)
                {
                    return assemblyPath.Substring(0, index + 1);
                }
            }

            return null;
        }

        private static bool IsAssemblyExist(string folderPath, string assemblyName)
        {
            string fullPath = Path.Combine(folderPath, assemblyName);

            bool isExists = File.Exists(fullPath);

            return isExists;
        }

        private static bool IsAssemblyVersionMatch(string folderPath, string assemblyName)
        {
            string fullPath = Path.Combine(folderPath, assemblyName);
            string fileVersion = FileVersionInfo.GetVersionInfo(fullPath).FileVersion;
            string libVersion = SciChartRuntimeInfo.GetVersion();

            if (!string.IsNullOrEmpty(fileVersion) && !string.IsNullOrEmpty(libVersion))
            {
                var pattern = new Regex("[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+");
                return pattern.Match(fileVersion).Value == pattern.Match(libVersion).Value;
            }

            return false;
        }

        private static string GetAssemblyPathFromRegistry()
        {
            if (SciChartRuntimeInfo.IsWindowsPlatform)
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(RegistryKeyString))
                {
                    if (registryKey != null)
                    {
                        var targetLibPath =
#if NETFRAMEWORK
                        @"Lib\net462\";
#elif NET
                        @"Lib\net6.0-windows\";
#else
                        @"Lib\netcoreapp3.1\";
#endif
                        var assemblyPathFromRegistry = Path.Combine((string)registryKey.GetValue("Path"), targetLibPath);

                        return assemblyPathFromRegistry;
                    }
                }
            }

            return null;
        }  
    }
}