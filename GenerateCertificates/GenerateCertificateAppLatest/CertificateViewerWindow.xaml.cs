using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AthleticsCertificateMaker.GenerateCertificateApp;

public partial class CertificateViewerWindow : Window
{
    public CertificateViewerWindow(string certificatePath)
    {
        InitializeComponent();

        FileNameText.Text = Path.GetFileName(certificatePath);

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(certificatePath, UriKind.Absolute);
        image.EndInit();
        image.Freeze();

        CertificateImage.Source = image;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
