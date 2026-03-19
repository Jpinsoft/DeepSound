using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DeepSoundMac.App.ViewModels;

namespace DeepSoundMac.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseCarrierFile_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Carrier Audio File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("WAV Audio Files") { Patterns = new[] { "*.wav" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0 && DataContext is MainWindowViewModel vm)
        {
            vm.CarrierFilePath = files[0].Path.LocalPath;
        }
    }

    private async void AddSecretFiles_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Secret Files to Hide",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (DataContext is MainWindowViewModel vm)
        {
            foreach (var file in files)
            {
                vm.AddSecretFileCommand.Execute(file.Path.LocalPath);
            }
        }
    }

    private async void BrowseOutputDirectory_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Output Directory",
            AllowMultiple = false
        });

        if (folders.Count > 0 && DataContext is MainWindowViewModel vm)
        {
            vm.OutputDirectory = folders[0].Path.LocalPath;
        }
    }

    private void OnSecretFilesDrop(object? sender, DragEventArgs e)
    {
        if (sender is Border border)
            border.Background = Brushes.Transparent;

        if (DataContext is not MainWindowViewModel vm)
            return;

        var files = e.Data.GetFiles();
        if (files == null)
            return;

        foreach (var file in files)
        {
            vm.AddSecretFileCommand.Execute(file.Path.LocalPath);
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (sender is Border border && e.Data.Contains(DataFormats.Files))
            border.Background = new SolidColorBrush(Color.FromArgb(30, 39, 174, 96));
    }

    private void OnDragLeave(object? sender, RoutedEventArgs e)
    {
        if (sender is Border border)
            border.Background = new SolidColorBrush(Color.Parse("#F8F9FA"));
    }
}