using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SciChart.Charting.Common.Extensions;
using SciChart.Core.Utility;
using SciChart.Examples.Demo.Lib.Common;

namespace SciChart.Examples.Demo.Lib.Helpers.ProjectExport
{
    public static class ProjectWriter
    {
        public static readonly string[] SciChartAssemblyNames =
        {
            "SciChart.Core.dll",
            "SciChart.Data.dll",
            "SciChart.Charting.dll",
#if !HIDE3D
            "SciChart.Charting3D.dll",
#endif
            "SciChart.Drawing.dll",
            "SciChart.Charting.DrawingTools.dll",
            "SciChart.Examples.ExternalDependencies.dll"
        };

        public static readonly string[] SciChartNuGetPackages =
        {
            "SciChart",
#if !HIDE3D
            "SciChart3D",
#endif
            "SciChart.DrawingTools",
            "SciChart.ExternalDependencies"
        };

        public static readonly string[] SciChartXpfNuGetPackages =
        {
            "SciChart.Avalonia.XPF"
        };

        public static readonly string[] ExamplesNuGetPackages =
        {
            //ExampleTitle;PackageName;PackageVersion
            "AudioAnalyzerDemo;NAudio;1.10.0",
            "VitalSignsMonitorDemo;System.Reactive;4.4.1"
        };

        public const string ExampleTheme = "Navy";

        public const string SolutionFileName = "SolutionFile.sln";
        public const string ProjectFileName = "ProjectFile.csproj";
        public const string NuGetConfigFileName = "NuGet.config";

        public const string ApplicationFileName = "App.xaml";
        public const string MainWindowFileName = "MainWindow.xaml";
        public const string ExampleResourcesFileName = "ExampleResources.xaml";

        public const string ClrNamespace = "clr-namespace:";
        public const string ViewModelKey = "ViewModel";

        public static readonly XNamespace PresentationXmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public static readonly XNamespace XXmlns = "http://schemas.microsoft.com/winfx/2006/xaml";

        public static int VersionMajor => int.Parse(SciChartRuntimeInfo.GetVersion()?.Split('.')[0].Trim('v') ?? "0");

        public static string WriteProject(Example example, string exportPath, string libsPath)
        {
            if (SciChartRuntimeInfo.IsXPF)
            {
                throw new NotSupportedException("This method is not supported, please use WriteXpfProject instead.");
            }

            return WriteProjectFiles(example, exportPath, libsPath, false, string.Empty);
        }

        public static string WriteXpfProject(Example example, string exportPath, string libsPath, string xpfLicenseKey)
        {
            if (!SciChartRuntimeInfo.IsXPF)
            {
                throw new NotSupportedException("This method is not supported, please use WriteProject instead.");
            }

            return WriteProjectFiles(example, exportPath, libsPath, true, xpfLicenseKey);
        }

        private static string WriteProjectFiles(Example example, string exportPath, string libsPath, bool isXpfExport, string xpfLicenseKey)
        {
            var files = new Dictionary<string, string>();
            var assembly = typeof(ProjectWriter).Assembly;

            var names = assembly.GetManifestResourceNames();
            var templateFiles = names.Where(x => x.Contains("Templates")).ToList();

            foreach (var templateFile in templateFiles)
            {
                var fileName = GetFileName(templateFile);
                var rcs = assembly.GetManifestResourceStream(templateFile);

                if (rcs == null) break;

                var sr = new StreamReader(rcs);
                files.Add(fileName, sr.ReadToEnd());

                sr.Close();
                sr.Dispose();

                rcs.Close();
                rcs.Dispose();
            }

            string projectName = "SciChart_" + Regex.Replace(example.Title, @"[^A-Za-z0-9]+", string.Empty);

            files[ProjectFileName] = GenerateProjectFile(files[ProjectFileName], example, libsPath, isXpfExport, xpfLicenseKey);
            files[SolutionFileName] = GenerateSolutionFile(files[SolutionFileName], projectName);

            files.RenameKey(ProjectFileName, projectName + ".csproj");
            files.RenameKey(SolutionFileName, projectName + ".sln");

            files[ApplicationFileName] = GenerateApplicationFile(files[ApplicationFileName], ExampleTheme);
            files[MainWindowFileName] = GenerateWindowFile(files[MainWindowFileName], example);

            if (isXpfExport)
            {
                files[NuGetConfigFileName] = GenerateNuGetConfigFile(files[NuGetConfigFileName], xpfLicenseKey);
            }
            else if (files.ContainsKey(NuGetConfigFileName))
            {
                files.Remove(NuGetConfigFileName);
            }

            foreach (var codeFile in example.SourceFiles)
            {
                files.Add(codeFile.Key, codeFile.Value);
            }

            if (!files.ContainsKey(ExampleResourcesFileName))
            {
                files.Add(ExampleResourcesFileName, GenerateExampleResourcesFile(ExampleTheme));
            }

            var exportDirPath = Path.Combine(exportPath, projectName);

            try
            {
                Directory.CreateDirectory(exportDirPath);

                foreach (var file in files)
                {
                    var sw = new StreamWriter(Path.Combine(exportDirPath, file.Key));
                    sw.Write(file.Value);
                    sw.Close();
                    sw.Dispose();
                }

                if (isXpfExport && !string.IsNullOrEmpty(libsPath))
                {
                    var assemblyPath = Assembly.GetExecutingAssembly().Location;
                    var sourcePath = Path.GetFullPath(Path.GetDirectoryName(assemblyPath));

                    CopyNativeLibs(sourcePath, exportDirPath, "Debug", "win-x64");
                    CopyNativeLibs(sourcePath, exportDirPath, "Debug", "win-x86");
                    CopyNativeLibs(sourcePath, exportDirPath, "Debug", "linux-x64");

                    CopyNativeLibs(sourcePath, exportDirPath, "Release", "win-x64");
                    CopyNativeLibs(sourcePath, exportDirPath, "Release", "win-x86");
                    CopyNativeLibs(sourcePath, exportDirPath, "Release", "linux-x64");
                }

                return projectName;
            }
            catch
            {
                return null;
            }
        }

        private static void CopyNativeLibs(string sourcePath, string exportPath, string config, string runtime)
        {
            var nativeSourcePath = Path.Combine(sourcePath, "runtimes", runtime, "native");

            if (!Directory.Exists(nativeSourcePath))
            {
                nativeSourcePath = sourcePath;
            }

            var nativeLibPaths = new List<string>(5);

            if (runtime.Contains("win"))
            {
                var nativeDllNames = new[]
                {
                    "AbtLicensingNative.dll",
                    "VXccelEngine2D.dll",
#if !HIDE3D
                    "VXccelEngine3D.dll",
#endif
                    "D3DCompiler_47.dll",
                    "glew32.dll"
                };

                foreach (var dllName in nativeDllNames)
                {
                    var nativeDllPath = Path.Combine(nativeSourcePath, dllName);

                    if (File.Exists(nativeDllPath))
                    {
                        nativeLibPaths.Add(nativeDllPath);
                    }
                }
            }
            else if (runtime.Contains("linux"))
            {
                var nativeSoNames = new[]
                {
                    "AbtLicensingNative.so",
                    "VXccelEngine2D.so",
#if !HIDE3D
                    "VXccelEngine3D.so"
#endif
                };

                foreach (var soName in nativeSoNames)
                {
                    var nativeSoPath = Path.Combine(nativeSourcePath, soName);

                    if (File.Exists(nativeSoPath))
                    {
                        nativeLibPaths.Add(nativeSoPath);
                    }
                }
            }

            if (nativeLibPaths.Any())
            {
                var configDestPath = Path.Combine(exportPath, "bin", config, "net6.0-windows");
                var nativeDestPath = Path.Combine(configDestPath, "runtimes", runtime, "native");

                Directory.CreateDirectory(nativeDestPath);

                foreach (var libPath in nativeLibPaths)
                {
                    var libName = Path.GetFileName(libPath);

                    File.Copy(libPath, Path.Combine(nativeDestPath, libName), true);
                }
            }
        }

        private static string GetFileName(string fullName)
        {
            return fullName
                .Replace("SciChart.Examples.Demo.Lib.Helpers.ProjectExport.Templates.", string.Empty)
                .Replace(".c.", ".cs.")
                .Replace(".txt", string.Empty);
        }

        private static string GenerateSolutionFile(string file, string projectName)
        {
            return file.Replace("[PROJECT_NAME]", projectName);
        }

        private static string GenerateApplicationFile(string file, string themeName)
        {
            return file.Replace("[THEME_NAME]", themeName);
        }

        private static string GenerateExampleResourcesFile(string themeName)
        {
            var themeText = ThemeLoader.LoadThemeFile(themeName);
            var themeXaml = XDocument.Parse(themeText);

            if (themeXaml.Root != null)
            {
                var mergedDictionaries = themeXaml.Root
                    .Elements()
                    .FirstOrDefault(x => x.Name.LocalName == "ResourceDictionary.MergedDictionaries");

                mergedDictionaries?.Remove();
#if !HIDE3D
                var style = new XElement(PresentationXmlns + "Style",
                    new XAttribute(XNamespace.Xmlns + "s3D", "http://schemas.abtsoftware.co.uk/scichart3D"),
                    new XAttribute("TargetType", "{x:Type s3D:SciChart3DSurface}"));

                var setter = new XElement(PresentationXmlns + "Setter",
                    new XAttribute("Property", "s:ThemeManager.Theme"),
                    new XAttribute("Value", ExampleTheme == "Navy" ? "SciChartv7Navy" : "SciChartv4Dark"));

                style.Add(setter);

                themeXaml.Root.Add(style);
#endif
            }

            return themeXaml.ToString();
        }

        private static string GenerateNuGetConfigFile(string file, string xpfLicenseKey)
        {
            if (!string.IsNullOrEmpty(xpfLicenseKey))
            {
                return file.Replace("[XPF_LICENSE_KEY]", xpfLicenseKey);
            }
            return file;
        }

        private static string GenerateProjectFile(string projFileSource, Example example, string libsPath, bool isXpfExport, string xpfLicenseKey)
        {
            var projXml = XDocument.Parse(projFileSource);
            if (projXml.Root != null)
            {
                var elements = projXml.Root.Elements().Where(x => x.Name.LocalName == "ItemGroup").ToList();
                if (elements.Count == 3)
                {
                    if (string.IsNullOrEmpty(libsPath))
                    {
                        // Get NuGet packages list
                        var packages = isXpfExport ? SciChartXpfNuGetPackages : SciChartNuGetPackages;

                        // Add assembly NuGet packages
                        foreach (var asmPackage in packages)
                        {
                            var el = new XElement("PackageReference",
                                new XAttribute("Include", asmPackage),
                                new XAttribute("Version", $"{VersionMajor}.*-*"));

                            elements[0].Add(el);
                        }
                    }
                    else
                    {
                        // Add assembly references
                        foreach (var asmName in SciChartAssemblyNames)
                        {
                            var el = new XElement("Reference", new XAttribute("Include", asmName.Replace(".dll", string.Empty)));
                            el.Add(new XElement("HintPath", Path.Combine(libsPath, asmName)));
                            elements[0].Add(el);
                        }
                    }

                    // Add package references for specific example NuGet packages
                    var exampleTitle = Regex.Replace(example.Title, @"\s", string.Empty);
                    var examplePackages = ExamplesNuGetPackages.Where(p => p.StartsWith(exampleTitle));
                    if (examplePackages.Any())
                    {
                        foreach (var package in examplePackages)
                        {
                            // ExampleTitle;PackageName;PackageVersion
                            var packageAttr = package.Split(';');
                            if (packageAttr.Length == 3)
                            {
                                var el = new XElement("PackageReference",
                                    new XAttribute("Include", packageAttr[1]),
                                    new XAttribute("Version", packageAttr[2]));

                                elements[1].Add(el);
                            }
                        }
                    }

                    string projXmlStr;

                    if (isXpfExport)
                    {
                        elements[2].RemoveAttributes();
                        elements[2].RemoveNodes();

                        var licenseKey = string.IsNullOrEmpty(xpfLicenseKey)
                            ? "XPF_LICENSE_KEY"
                            : xpfLicenseKey;

                        var el = new XElement("RuntimeHostConfigurationOption",
                            new XAttribute("Include", "AvaloniaUI.Xpf.LicenseKey"),
                            new XAttribute("Value", licenseKey));

                        elements[2].Add(el);

                        projXmlStr = projXml.ToString();
                        projXmlStr = projXmlStr.Replace("[PROJECT_SDK]", @"Xpf.Sdk/1.4.0-cibuild001853");
                    }
                    else
                    {
                        projXmlStr = projXml.ToString();
                        projXmlStr = projXmlStr.Replace("[PROJECT_SDK]", @"Microsoft.NET.Sdk.WindowsDesktop");
                    }

                    var targetFramework =
#if NETFRAMEWORK
                    "net462";
#elif NET
                    "net6.0-windows";
#elif NETCOREAPP3_1
                    "netcoreapp3.1";
#else
                    "net6.0-windows";
#endif
                    return projXmlStr.Replace("[PROJECT_TARGET]", targetFramework);
                }
            }

            return projFileSource;
        }

        private static string GenerateWindowFile(string windowFileSource, Example example)
        {
            var sourceFiles = example.SourceFiles;
            var viewName = DirectoryHelper.GetFileNameFromPath(example.Page.Uri);
            var xamlFile = sourceFiles.Where(p => p.Key.EndsWith(".xaml")).FirstOrDefault(x => x.Key == viewName);

            var ns = GetExampleNamespace(xamlFile.Value, out string fileName);
            var xml = XDocument.Parse(windowFileSource);

            if (xml.Root != null)
            {
                //xmlns
                xml.Root.Add(new XAttribute(XNamespace.Xmlns + "example", ClrNamespace + ns));

                var userControlElement = new XElement((XNamespace)(ClrNamespace + ns) + fileName);

                //ViewModel to Resources
                if (example.Page.ViewModel != null)
                {
                    var el = new XElement(PresentationXmlns + "Window.Resources");
                    el.Add(new XElement((XNamespace)(ClrNamespace + ns) + example.Page.ViewModel.GetType().Name,
                        new XAttribute(XXmlns + "Key", ViewModelKey)));

                    xml.Root.Add(el);

                    //Add DataContext to UserControl is needed
                    userControlElement.Add(new XAttribute("DataContext", GetStaticResource(ViewModelKey)));
                }

                xml.Root.Add(userControlElement);
            }

            return xml.ToString().Replace("[EXAMPLE_TITLE]", example.Title);
        }

        private static string GetExampleNamespace(string xamlFile, out string xamlFileName)
        {
            var xml = XDocument.Parse(xamlFile);
            if (xml.Root != null)
            {
                var xClassAttribute = xml.Root.Attributes().FirstOrDefault(x => x.Name.LocalName == "Class");
                if (xClassAttribute != null)
                {
                    var classNs = xClassAttribute.Value;
                    var index = classNs.LastIndexOf('.');

                    if (index > 0)
                    {
                        xamlFileName = classNs.Substring(index + 1, classNs.Length - index - 1);
                        return classNs.Substring(0, index).TrimEnd('.');
                    }
                }
            }
            xamlFileName = null;
            return null;
        }

        private static string GetStaticResource(string resourceKey)
        {
            return string.Format("{{StaticResource {0}}}", resourceKey);
        }
    }
}