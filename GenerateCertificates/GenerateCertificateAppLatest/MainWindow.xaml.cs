using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Text.Json;
using Microsoft.Win32;
using AthleticsCertificateMaker.GenerateCertificateApp.Models;
using AthleticsCertificateMaker.GenerateCertificateApp.Services;

namespace AthleticsCertificateMaker.GenerateCertificateApp;

public partial class MainWindow : Window
{
    private enum CertificateRunMode
    {
        BuildOnly,
        BuildAndSave,
        BuildAndSend
    }

    private readonly AppRuntimeSettingsService _appSettingsService = new();

    private string _settingsPath = string.Empty;
    private string _resultsPath = string.Empty;
    private string _templatePath = string.Empty;
    private CertificateConfig? _config;


    public MainWindow()
    {
        InitializeComponent();
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        var appSettings = _appSettingsService.Load();

        var settingsPath = appSettings.SettingsJsonPath;
        if (string.IsNullOrWhiteSpace(settingsPath) || !File.Exists(settingsPath))
        {
            settingsPath = Path.Combine(Environment.CurrentDirectory, "CertificateText.json");
        }

        if (File.Exists(settingsPath))
        {
            LoadSettings(settingsPath);
            if (!string.IsNullOrWhiteSpace(appSettings.ResultsPath))
            {
                _resultsPath = appSettings.ResultsPath;
            }

            if (!string.IsNullOrWhiteSpace(appSettings.TemplatePath))
            {
                _templatePath = appSettings.TemplatePath;
            }

            UpdatePathTextBoxes();
        }
        else
        {
            Log("Select File > Get Settings from JSON to choose CertificateText.json.");
        }
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        => SelectSettingsFile();

    private void OpenResultsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open Results CSV/TSV",
            Filter = "CSV/TSV Files (*.csv;*.tsv;*.txt)|*.csv;*.tsv;*.txt|All files (*.*)|*.*",
            FileName = Path.GetFileName(_resultsPath)
        };

        if (dialog.ShowDialog(this) == true)
        {
            _resultsPath = dialog.FileName;
            UpdatePathTextBoxes();
            PersistAppSettings();
            Log($"Results file selected: {_resultsPath}");
        }
    }

    private void SelectTemplateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select certificate template image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
            FileName = Path.GetFileName(_templatePath)
        };

        if (dialog.ShowDialog(this) == true)
        {
            _templatePath = dialog.FileName;
            UpdatePathTextBoxes();
            PersistAppSettings();
            Log($"Template selected: {_templatePath}");
        }
    }

    private void SaveSettingsToJsonMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_config is null || string.IsNullOrWhiteSpace(_settingsPath))
            {
                throw new InvalidOperationException("Settings are not loaded.");
            }

            _config.CertificateTemplateFile = ToRelativeOrAbsolute(_settingsPath, _templatePath);
            _config.ParticipantCsvFile = ToRelativeOrAbsolute(_settingsPath, _resultsPath);

            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_settingsPath, json);
            Log($"Settings saved to JSON: {_settingsPath}");
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Save settings failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearAppSettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            "Clear saved app settings (last used JSON, results and template paths)?",
            "Clear App Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        _appSettingsService.Save(new AppRuntimeSettings());

        _settingsPath = string.Empty;
        _resultsPath = string.Empty;
        _templatePath = string.Empty;
        _config = null;

        SettingsPathTextBox.Text = string.Empty;
        ResultsPathTextBox.Text = string.Empty;
        TemplatePathTextBox.Text = string.Empty;
        CertificatesHeader.IsEnabled = false;

        SummaryTextBlock.Text = "App settings cleared";
        Log("App runtime settings cleared.");
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        => Close();

    private void BuildCertificatesMenuItem_Click(object sender, RoutedEventArgs e)
        => RunCertificateBuild(CertificateRunMode.BuildOnly);

    private void BuildAndSaveCertificatesMenuItem_Click(object sender, RoutedEventArgs e)
        => RunCertificateBuild(CertificateRunMode.BuildAndSave);

    private void BuildAndSendCertificatesMenuItem_Click(object sender, RoutedEventArgs e)
        => RunCertificateBuild(CertificateRunMode.BuildAndSend);

    private void SendDraftsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_config is null)
            {
                throw new InvalidOperationException("Settings are not loaded. Use File > Get Settings from JSON.");
            }

            var confirm = MessageBox.Show(
                this,
                "Send all certificate drafts created by this app?",
                "Confirm send drafts",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            var count = OutlookEmailService.SendMarkedDrafts(_config.SendUsingAccount, Log);
            SummaryTextBlock.Text = $"Drafts sent: {count}";
            Log($"Drafts sent: {count}");
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Send Drafts failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearDraftsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var confirm = MessageBox.Show(
                this,
                "Delete all certificate drafts created by this app?",
                "Confirm clear drafts",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            var count = OutlookEmailService.ClearMarkedDrafts(Log);
            SummaryTextBlock.Text = $"Drafts cleared: {count}";
            Log($"Drafts cleared: {count}");
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Clear Drafts failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CopyLogMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(LogTextBox.Text ?? string.Empty);
        SummaryTextBlock.Text = "Log copied";
    }

    private void ClearLogMenuItem_Click(object sender, RoutedEventArgs e)
    {
        LogTextBox.Clear();
        SummaryTextBlock.Text = "Log cleared";
    }

    private void BrowseGeneratedCertificatesMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var outputFolder = Path.Combine(GetRootFolder(), "Output", "PNG");
            if (!Directory.Exists(outputFolder))
            {
                throw new DirectoryNotFoundException($"Generated certificates folder not found: {outputFolder}");
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = outputFolder,
                UseShellExecute = true
            });

            Log($"Opened generated certificates folder: {outputFolder}");
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Browse generated certificates failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CleanMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var confirmation = MessageBox.Show(
                this,
                "Remove generated output files?",
                "Clear generated files",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            var rootFolder = GetRootFolder();
            Log($"Cleaning generated files under: {rootFolder}");

            RunClean(rootFolder);

            Log("Clear completed.");
            SummaryTextBlock.Text = "Cleared";
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Clear failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
        => RunCertificateBuild(CertificateRunMode.BuildOnly);

    private void RunCertificateBuild(CertificateRunMode mode)
    {
        try
        {
            if (_config is null)
            {
                throw new InvalidOperationException("Settings are not loaded. Use File > Get Settings from JSON.");
            }

            if (string.IsNullOrWhiteSpace(_resultsPath) || !File.Exists(_resultsPath))
            {
                throw new FileNotFoundException("Results file not found. Use File > Select Results...", _resultsPath);
            }

            if (string.IsNullOrWhiteSpace(_templatePath) || !File.Exists(_templatePath))
            {
                throw new FileNotFoundException("Template image file not found.", _templatePath);
            }

            SummaryTextBlock.Text = mode switch
            {
                CertificateRunMode.BuildAndSave => "Generating and saving drafts...",
                CertificateRunMode.BuildAndSend => "Generating and sending...",
                _ => "Generating..."
            };
            Log("Loading participants...");

            var participants = ParticipantCsvLoader.Load(_resultsPath, _config.ParticipantCsvDelimiter);
            Log($"Participants loaded: {participants.Count}");

            var rootFolder = Path.GetDirectoryName(_settingsPath) ?? Environment.CurrentDirectory;
            var result = CertificateGenerator.Generate(rootFolder, _config, participants, Log, _templatePath);

            if (mode == CertificateRunMode.BuildAndSave || mode == CertificateRunMode.BuildAndSend)
            {
                if (mode == CertificateRunMode.BuildAndSend)
                {
                    var confirmSend = MessageBox.Show(
                        this,
                        "Build and Send will immediately send emails via Outlook. Continue?",
                        "Confirm send",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (confirmSend != MessageBoxResult.Yes)
                    {
                        SummaryTextBlock.Text = "Send cancelled";
                        Log("Send cancelled by user.");
                        return;
                    }
                }

                var deliveryMode = mode == CertificateRunMode.BuildAndSend
                    ? EmailDeliveryMode.Send
                    : EmailDeliveryMode.Save;

                var emailCount = OutlookEmailService.ProcessCertificates(
                    result.GeneratedCertificates,
                    result.ResultsCsvPath,
                    _config.SendUsingAccount,
                    deliveryMode,
                    Log);

                Log(mode == CertificateRunMode.BuildAndSend
                    ? $"Emails sent: {emailCount}"
                    : $"Drafts saved: {emailCount}");
            }

            SummaryTextBlock.Text = $"Done. Certificates: {result.CertificatesGenerated}";
            Log($"Completed. Output folder: {result.OutputFolder}");
            Log($"Official results CSV: {result.ResultsCsvPath}");
        }
        catch (Exception ex)
        {
            SummaryTextBlock.Text = "Failed";
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Generation failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            //GenerateButton.IsEnabled = true;
            
        }
    }

    private void SelectSettingsFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open Certificate Settings JSON",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = Path.GetFileName(_settingsPath)
        };

        if (dialog.ShowDialog(this) == true)
        {
            LoadSettings(dialog.FileName);
            PersistAppSettings();
        }
    }

    private void LoadSettings(string settingsPath)
    {
        _config = CertificateConfigLoader.Load(settingsPath);
        _settingsPath = settingsPath;

        _templatePath = ResolvePath(_settingsPath, _config.CertificateTemplateFile);

        var configuredResultsPath = ResolvePath(_settingsPath, _config.ParticipantCsvFile);
        if (File.Exists(configuredResultsPath))
        {
            _resultsPath = configuredResultsPath;
        }
        else if (string.IsNullOrWhiteSpace(_resultsPath))
        {
            _resultsPath = configuredResultsPath;
        }

        UpdatePathTextBoxes();

        Log($"Settings loaded: {_settingsPath}");
        Log($"Configured ParticipantCsvFile: {_config.ParticipantCsvFile}");
        Log($"Configured template: {_config.CertificateTemplateFile}");
    }

    private void UpdatePathTextBoxes()
    {
        SettingsPathTextBox.Text = _settingsPath;
        ResultsPathTextBox.Text = _resultsPath;
        TemplatePathTextBox.Text = _templatePath;
        CertificatesHeader.IsEnabled = true;
    }

    private void PersistAppSettings()
    {
        _appSettingsService.Save(new AppRuntimeSettings
        {
            SettingsJsonPath = _settingsPath,
            ResultsPath = _resultsPath,
            TemplatePath = _templatePath
        });
    }

    private static string ResolvePath(string settingsPath, string filePath)
    {
        if (Path.IsPathRooted(filePath))
        {
            return filePath;
        }

        var root = Path.GetDirectoryName(settingsPath) ?? Environment.CurrentDirectory;
        return Path.Combine(root, filePath);
    }

    private string GetRootFolder()
    {
        if (!string.IsNullOrWhiteSpace(_settingsPath))
        {
            return Path.GetDirectoryName(_settingsPath) ?? Environment.CurrentDirectory;
        }

        return Environment.CurrentDirectory;
    }

    private static string ToRelativeOrAbsolute(string settingsPath, string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return string.Empty;
        }

        var settingsDir = Path.GetDirectoryName(settingsPath) ?? Environment.CurrentDirectory;
        var settingsDirFull = Path.GetFullPath(settingsDir)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var candidateFull = Path.GetFullPath(fullPath);

        if (candidateFull.StartsWith(settingsDirFull, StringComparison.InvariantCultureIgnoreCase))
        {
            return Path.GetRelativePath(settingsDir, candidateFull);
        }

        return candidateFull;
    }

    private void RunClean(string repoRoot)
    {
        var clearFolders = new[] { "Output", "Results", "logs" };
        var deletePaths = new[] { "," };
        var keepNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "004-Fred Nurk.png"
        };

        foreach (var folder in clearFolders)
        {
            var folderPath = Path.Combine(repoRoot, folder);
            if (!Directory.Exists(folderPath))
            {
                continue;
            }

            foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                var name = Path.GetFileName(filePath);
                if (name is not null && keepNames.Contains(name))
                {
                    continue;
                }

                RemoveRepoItem(filePath, repoRoot, recurse: false);
            }
        }

        foreach (var relPath in deletePaths)
        {
            var fullPath = Path.Combine(repoRoot, relPath);
            if (File.Exists(fullPath))
            {
                RemoveRepoItem(fullPath, repoRoot, recurse: false);
            }
            else if (Directory.Exists(fullPath))
            {
                RemoveRepoItem(fullPath, repoRoot, recurse: true);
            }
        }

        foreach (var rootFile in Directory.GetFiles(repoRoot, "~$*.xlsx", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(rootFile);
            if (name is not null && keepNames.Contains(name))
            {
                continue;
            }

            RemoveRepoItem(rootFile, repoRoot, recurse: false);
        }
    }

    private void RemoveRepoItem(string fullPath, string repoRoot, bool recurse)
    {
        var fullRepoPath = Path.GetFullPath(repoRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var fullItemPath = Path.GetFullPath(fullPath);

        if (!fullItemPath.StartsWith(fullRepoPath, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        try
        {
            if (Directory.Exists(fullItemPath))
            {
                Directory.Delete(fullItemPath, recurse);
                Log($"Removed: {fullItemPath}");
            }
            else if (File.Exists(fullItemPath))
            {
                File.Delete(fullItemPath);
                Log($"Removed: {fullItemPath}");
            }
        }
        catch (Exception ex)
        {
            Log($"ERROR removing '{fullItemPath}': {ex.Message}");
        }
    }

    private void Log(string message)
    {
        LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}");
        LogTextBox.ScrollToEnd();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow
        {
            Owner = this
        };

        aboutWindow.ShowDialog();
    }
}
