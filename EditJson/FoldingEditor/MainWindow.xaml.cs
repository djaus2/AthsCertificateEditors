using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace AthleticsCertificateMaker.EditCert
{
    public partial class MainWindow : Window
    {
        private string? _currentPath;
        private FoldingManager? _foldingManager;
        private JsonFoldingStrategy _foldingStrategy = new JsonFoldingStrategy();
        private DispatcherTimer _foldingUpdateTimer;

        public MainWindow()
        {
            InitializeComponent();

            // set basic highlighting if available
            try
            {
                var def = HighlightingManager.Instance.GetDefinition("JavaScript") ?? HighlightingManager.Instance.GetDefinition("JSON");
                if (def != null) Editor.SyntaxHighlighting = def;
            }
            catch { }

            _foldingManager = FoldingManager.Install(Editor.TextArea);
            _foldingUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _foldingUpdateTimer.Tick += (s, e) => { UpdateFoldings(); _foldingUpdateTimer.Stop(); };

            Editor.TextChanged += (s, e) => { _foldingUpdateTimer.Stop(); _foldingUpdateTimer.Start(); UpdateStatus(); };

            // start with default
            Editor.Text = GetDefaultJson();
            UpdateFoldings();
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            LblStatus.Text = $"Length: {Editor.Text.Length}   Lines: {Editor.Document.LineCount}";
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            Editor.Text = GetDefaultJson();
            _currentPath = null;
            LblPath.Text = "(new)";
            UpdateFoldings();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };
            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    var txt = File.ReadAllText(dlg.FileName);
                    // try to pretty print
                    try
                    {
                        var obj = JsonSerializer.Deserialize<object>(txt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        txt = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                    }
                    catch { }

                    Editor.Text = txt;
                    _currentPath = dlg.FileName;
                    LblPath.Text = _currentPath;
                    UpdateFoldings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentPath)) { BtnSaveAs_Click(sender, e); return; }
            SaveToPath(_currentPath);
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*", FileName = "CertificateText.json" };
            if (dlg.ShowDialog(this) == true) SaveToPath(dlg.FileName);
        }

        private void SaveToPath(string path)
        {
            try
            {
                // validate json
                var txt = Editor.Text;
                using var doc = JsonDocument.Parse(txt);
                File.WriteAllText(path, txt);
                _currentPath = path;
                LblPath.Text = path;
                MessageBox.Show(this, "Saved", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (JsonException jex)
            {
                MessageBox.Show(this, jex.Message, "Invalid JSON", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateFoldings()
        {
            if (_foldingManager == null) return;
            try
            {
                _foldingStrategy.UpdateFoldings(_foldingManager, Editor.Document);
            }
            catch { }
        }

        private string GetDefaultJson()
        {
            // reuse the default from other editors
            return System.IO.File.Exists("CertificateText.json") ? File.ReadAllText("CertificateText.json") : "{\n  \"CertificateTemplateFile\": \"DoncasterCertificateTemplate2.png\"\n}";
        }
    }

    // Simple bracket-based JSON folding strategy
    public class JsonFoldingStrategy
    {
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var newFoldings = CreateNewFoldings(document);
            manager.UpdateFoldings(newFoldings, -1);
        }

        private IEnumerable<NewFolding> CreateNewFoldings(TextDocument document)
        {
            var foldings = new List<NewFolding>();
            var text = document.Text;
            var stack = new Stack<int>();
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '{' || ch == '[')
                {
                    stack.Push(i);
                }
                else if (ch == '}' || ch == ']')
                {
                    if (stack.Count > 0)
                    {
                        var start = stack.Pop();
                        var end = i;
                        // create folding only if spans multiple chars
                        if (end > start + 1)
                        {
                            int foldingStart = start;
                            int foldingEnd = end + 1; // exclusive
                            // create a short name
                            var name = text.Substring(start, Math.Min(32, Math.Max(0, end - start + 1))).Replace('\r', ' ').Replace('\n', ' ');
                            foldings.Add(new NewFolding(foldingStart, foldingEnd) { Name = name });
                        }
                    }
                }
            }

            foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return foldings;
        }
    }
}
