using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace AthleticsCertificateMaker.Editor
{
    public partial class MainWindow : Window
    {
        private string? _currentPath;

        public MainWindow()
        {
            InitializeComponent();
            SetStatus("Ready");
            TxtJson.Text = GetDefaultJson();
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            TxtJson.Text = GetDefaultJson();
            _currentPath = null;
            SetStatus("New document");
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    var txt = File.ReadAllText(dlg.FileName);
                    // pretty-format if possible
                    try
                    {
                        using var doc = JsonDocument.Parse(txt);
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        var obj = JsonSerializer.Deserialize<object>(txt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var formatted = JsonSerializer.Serialize(obj, options);
                        TxtJson.Text = formatted;
                    }
                    catch
                    {
                        TxtJson.Text = txt;
                    }

                    _currentPath = dlg.FileName;
                    SetStatus($"Loaded: {Path.GetFileName(_currentPath)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetStatus("Load failed");
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentPath))
            {
                BtnSaveAs_Click(sender, e);
                return;
            }

            SaveToPath(_currentPath);
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dlg.FileName = _currentPath is null ? "CertificateText.json" : Path.GetFileName(_currentPath);
            if (dlg.ShowDialog(this) == true)
            {
                SaveToPath(dlg.FileName);
            }
        }

        private void SaveToPath(string path)
        {
            try
            {
                // Validate JSON
                var txt = TxtJson.Text;
                using var doc = JsonDocument.Parse(txt);

                File.WriteAllText(path, txt);
                _currentPath = path;
                SetStatus($"Saved: {Path.GetFileName(path)}");
            }
            catch (JsonException jex)
            {
                MessageBox.Show(this, jex.Message, "Invalid JSON", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetStatus("Save failed - invalid JSON");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatus("Save failed");
            }
        }

        private void SetStatus(string text)
        {
            LblStatus.Text = text;
        }

        private string GetDefaultJson()
        {
            // A reasonable default based on the repository's CertificateText.json
            return @"{
  ""CertificateTemplateFile"": ""DoncasterCertificateTemplate2.png"",
  ""ParticipantCsvFile"": ""results.csv"",
  ""ParticipantCsvDelimiter"": ""Auto"",
  ""SendUsingAccount"": ""djaus@sportronics.com.au"",
  ""DefaultTextColor"": ""Red"",
  ""DefaultLabelColor"": ""LightGreen"",
  ""DefaultValueColor"": ""Yellow"",
  ""DefaultLabelTextSize"": 70,
  ""DefaultDataTextSize"": 90,
  ""CertificateSmoothingMode"": ""HighQuality"",
  ""CertificateTextRenderingHint"": ""AntiAliasGridFit"",
  ""ShowLocation"": false,
  ""ShowHost"": true,
  ""CertificateText"": [
    {
      ""Name"": ""Title"",
      ""Text"": ""1 HOUR TRACK RUN"",
      ""Color"": ""Red"",
      ""Font"": ""Times New Roman"",
      ""Size"": 85,
      ""Style"": ""Bold"",
      ""Y"": 70
    },
    {
      ""Name"": ""Location"",
      ""Text"": ""Doncaster Athletics Centre"",
      ""Color"": ""White"",
      ""Font"": ""Arial Narrow"",
      ""Size"": 38,
      ""Style"": ""Regular"",
      ""Y"": 320
    },
    {
      ""Name"": ""Host"",
      ""Text"": ""Doncaster Masters Athletics"",
      ""Color"": ""White"",
      ""Font"": ""Arial Narrow"",
      ""Size"": 38,
      ""Style"": ""Regular"",
      ""Y"": 360
    },
    {
      ""Name"": ""Date"",
      ""Text"": ""5th October 2026"",
      ""Color"": ""LightGreen"",
      ""Font"": ""Arial Narrow"",
      ""Size"": 51,
      ""Style"": ""Regular"",
      ""Y"": 410
    }
  ],
  ""CertificateFields"": {
    ""Standard"": [
      {
        ""Name"": ""Name"",
        ""Label"": ""Name:"",
        ""LabelFont"": ""Arial"",
        ""LabelSize"": 70,
        ""LabelStyle"": ""Bold"",
        ""LabelColor"": ""LightGreen"",
        ""LabelY"": 800,
        ""ValueFont"": ""Arial"",
        ""ValueSize"": 90,
        ""ValueStyle"": ""Regular"",
        ""ValueColor"": ""Yellow"",
        ""ValueY"": 900
      },
      {
        ""Name"": ""Distance"",
        ""Label"": ""Distance"",
        ""Suffix"": "" metres"",
        ""LabelFont"": ""Arial"",
        ""LabelSize"": 70,
        ""LabelStyle"": ""Bold"",
        ""LabelColor"": ""LightGreen"",
        ""LabelY"": 1100,
        ""ValueFont"": ""Arial"",
        ""ValueSize"": 90,
        ""ValueStyle"": ""Regular"",
        ""ValueColor"": ""Yellow"",
        ""ValueY"": 1200
      }
    ],
    ""Time"": [
      {
        ""Name"": ""Name"",
        ""Label"": ""Name"",
        ""LabelFont"": ""Arial"",
        ""LabelSize"": 70,
        ""LabelStyle"": ""Bold"",
        ""LabelColor"": ""LightGreen"",
        ""LabelY"": 800,
        ""ValueFont"": ""Arial"",
        ""ValueSize"": 90,
        ""ValueStyle"": ""Regular"",
        ""ValueColor"": ""Yellow"",
        ""ValueY"": 900
      },
      {
        ""Name"": ""Distance"",
        ""Label"": ""Distance"",
        ""Suffix"": "" metres"",
        ""LabelFont"": ""Arial"",
        ""LabelSize"": 70,
        ""LabelStyle"": ""Bold"",
        ""LabelColor"": ""LightGreen"",
        ""LabelY"": 1050,
        ""ValueFont"": ""Arial"",
        ""ValueSize"": 90,
        ""ValueStyle"": ""Regular"",
        ""ValueColor"": ""Yellow"",
        ""ValueY"": 1150
      },
      {
        ""Name"": ""Time"",
        ""Label"": ""Time"",
        ""LabelFont"": ""Arial"",
        ""LabelSize"": 70,
        ""LabelStyle"": ""Bold"",
        ""LabelColor"": ""LightGreen"",
        ""LabelY"": 1300,
        ""ValueFont"": ""Arial"",
        ""ValueSize"": 90,
        ""ValueStyle"": ""Regular"",
        ""ValueColor"": ""Yellow"",
        ""ValueY"": 1400
      }
    ]
  }
}";
        }
    }
}
