using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Platform.Storage;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace LibVLCSharp.DVD;

public partial class MainWindow : Window
{
  private readonly LibVLC libVLC= new();
  private MediaPlayer? mediaPlayer;

  public MainWindow(){
        InitializeComponent();
        Closing+= OnWindowClosing;

        TimeSlider.AddHandler(PointerPressedEvent, OnTimeSliderPressed, handledEventsToo: true);
        TimeSlider.AddHandler(PointerReleasedEvent, OnTimeSliderReleased, handledEventsToo: true);
  }

  private async void OnOpenFile(object? sender, RoutedEventArgs e){
    var files= await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
          Title= "Select a DVD ISO file",
          AllowMultiple= false,
          FileTypeFilter= new[]
           {
             new FilePickerFileType("DVD ISO Files"){
               Patterns= new[] { "*.iso" }
             },
              new FilePickerFileType("All Files"){
                Patterns= new[] { "*" }
              }
           }
        });

    if(files.Count==0) return;

    if(mediaPlayer==null){
      mediaPlayer= new MediaPlayer(libVLC);
      mediaPlayer.TimeChanged+= OnTimeChanged;
      VideoView.MediaPlayer= mediaPlayer;
    }
    else{
      mediaPlayer.Stop();
    }

    var media= new Media(libVLC, $"dvd://{files[0].Path.LocalPath}", FromType.FromLocation);
    mediaPlayer.Play(media);
    media.Dispose();
  }

  private void OnPlay(object? sender, RoutedEventArgs e) => mediaPlayer?.Play();
  private void OnPause(object? sender, RoutedEventArgs e) => mediaPlayer?.Pause();
  private void OnStop(object? sender, RoutedEventArgs e) => mediaPlayer?.Stop();

  private void OnClose(object? sender, RoutedEventArgs e) => Close();

  private bool userDragging=false;
  private void OnTimeSliderPressed(object? sender, PointerPressedEventArgs e) => userDragging=true;
  private void OnTimeSliderReleased(object? sender, PointerReleasedEventArgs e){
    userDragging=false;
    if(mediaPlayer is null || mediaPlayer.Length<=0)  return;
    mediaPlayer.Time= (long)(TimeSlider.Value/100 *mediaPlayer.Length);
  }

  private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e){
    if(mediaPlayer is null || mediaPlayer.Length<=0 || userDragging) return;
     global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
          TimeSlider.Value= (double)e.Time/mediaPlayer.Length*100;
        });
  }

  private void OnTimeSliderChanged(object? sender, PointerCaptureLostEventArgs e){
    if(mediaPlayer is null || mediaPlayer.Length<=0) return;
    mediaPlayer.Time= (long)(TimeSlider.Value/100 *mediaPlayer.Length);
  }

  private void OnVolumeChanged(object? sender, RangeBaseValueChangedEventArgs e){
    if(mediaPlayer is not null){
      mediaPlayer.Volume= (int)e.NewValue;
    }
  }

  private void OnMinimize(object? sender, RoutedEventArgs e){
    WindowState= WindowState==WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
    this.Hide();
    this.Show();
  }
  private void OnFullScreen(object? sender, RoutedEventArgs e) => WindowState= WindowState==WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;

  private void OnWindowClosing(object? sender, WindowClosingEventArgs e){
    mediaPlayer?.Stop();
    mediaPlayer?.Dispose();
    libVLC.Dispose();
  }
}
