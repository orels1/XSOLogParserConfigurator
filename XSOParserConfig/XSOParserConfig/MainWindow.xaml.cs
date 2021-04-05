using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace XSOParserConfig
{
  /// <summary>
  /// An empty window that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainWindow : Window, INotifyPropertyChanged
  {
    private readonly bool initialLoad = true;

    public MainWindow()
    {
      ElementSoundPlayer.State = ElementSoundPlayerState.On;
      LoadConfig();
      this.InitializeComponent();
      UpdateUI();
      initialLoad = false;
      MarkDirty(false); // we need to un-dirty the ui after we populate it
    }

    public event PropertyChangedEventHandler PropertyChanged;

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private ConfigModel configContent;
    private bool dirty;
    private Visibility advancedMode = Visibility.Collapsed;

    public Visibility AdvancedMode
    {
      get => advancedMode;
      set
      {
        advancedMode = value;
        NotifyPropertyChanged();
      }
    }

    private void LoadConfig()
    {
      var userFolderPath = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\XSOverlay VRChat Parser");
      configContent = JsonSerializer.Deserialize<ConfigModel>(File.ReadAllText($@"{userFolderPath}\config.json"),
        options: new JsonSerializerOptions {ReadCommentHandling = JsonCommentHandling.Skip});
    }

    private void UpdateUI()
    {
      VolumeText.Text = $"Notification Volume: {Math.Truncate(configContent.NotificationVolume * 100d)}";
      VolumeSlider.Value = configContent.NotificationVolume * 100f;
      OpacityText.Text = $"Notification Opacity: {Math.Truncate(configContent.Opacity * 100d)}";
      OpacitySlider.Value = configContent.Opacity * 100f;
      PlayerJoinedToggle.IsChecked = configContent.DisplayPlayerJoined;
      PlayerLeftToggle.IsChecked = configContent.DisplayPlayerLeft;
      WorldChangedToggle.IsChecked = configContent.DisplayWorldChanged;
      PortalDroppedToggle.IsChecked = configContent.DisplayPortalDropped;
      ShaderKeywordsToggle.IsChecked = configContent.DisplayMaximumKeywordsExceeded;
      LogNotificationEventsToggle.IsChecked = configContent.LogNotificationEvents;
      ParseFrequencyMs.Value = configContent.ParseFrequencyMilliseconds;
      DirectoryPollFrequencyMs.Value = configContent.DirectoryPollFrequencyMilliseconds;
      OutputLogRoot.Text = configContent.OutputLogRoot;
      PlayerJoinedNotifTimeoutS.Value = configContent.PlayerJoinedNotificationTimeoutSeconds;
      PlayerJoinedIconPath.Text = configContent.PlayerJoinedIconPath;
      PlayerJoinedAudioPath.Text = configContent.PlayerJoinedAudioPath;
      PlayerLeftNotifTimeoutS.Value = configContent.PlayerLeftNotificationTimeoutSeconds;
      PlayerLeftIconPath.Text = configContent.PlayerLeftIconPath;
      PlayerLeftAudioPath.Text = configContent.PlayerLeftAudioPath;
      WorldChangedNotifTimeoutS.Value = configContent.WorldChangedNotificationTimeoutSeconds;
      WorldJoinSilenceSeconds.Value = configContent.WorldJoinSilenceSeconds;
      JoinLeaveSilenced.IsChecked = !configContent.DisplayJoinLeaveSilencedOverride;
      WorldChangedIconPath.Text = configContent.WorldChangedIconPath;
      WorldChangedAudioPath.Text = configContent.WorldChangedAudioPath;
      PortalDroppedNotifTimeoutS.Value = configContent.PortalDroppedTimeoutSeconds;
      PortalDroppedIconPath.Text = configContent.PortalDroppedIconPath;
      PortalDroppedAudioPath.Text = configContent.PortalDroppedAudioPath;
      MaximumKeywordsNotifTimeoutS.Value = configContent.MaximumKeywordsExceededTimeoutSeconds;
      MaximumKeywordsNotifCooldownS.Value = configContent.MaximumKeywordsExceededCooldownSeconds;
      MaximumKeywordsIconPath.Text = configContent.MaximumKeywordsExceededIconPath;
      MaximumKeywordsAudioPath.Text = configContent.MaximumKeywordsExceededAudioPath;
    }

    private void MarkDirty(bool state)
    {
      if (dirty == state) return;
      dirty = state;
      SaveBtn.IsEnabled = dirty;
    }

    private void VolumeChange(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (initialLoad) return;
      var truncated = (float) Math.Truncate(e.NewValue);
      VolumeText.Text = $"Notification Volume: {truncated}";
      configContent.NotificationVolume = truncated / 100f;
      ElementSoundPlayer.Play(ElementSoundKind.Focus);
      MarkDirty(true);
    }

    private async void SaveSettings(object sender, RoutedEventArgs e)
    {
      SaveBtn.Icon = new SymbolIcon(Symbol.Accept);
      Console.WriteLine("Saved!");
      var userFolderPath = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\XSOverlay VRChat Parser");
      await File.WriteAllTextAsync($@"{userFolderPath}\config.json", configContent.AsJson());
      await Task.Delay(1000);
      SaveBtn.Icon = new SymbolIcon(Symbol.Save);
      MarkDirty(false);
    }

    private async void ReloadSettings(object sender, RoutedEventArgs e)
    {
      ReloadBtn.Icon = new SymbolIcon(Symbol.Accept);
      LoadConfig();
      UpdateUI();
      await Task.Delay(1000);
      ReloadBtn.Icon = new SymbolIcon(Symbol.Refresh);
      MarkDirty(false);
    }

    private void OpacityChange(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (initialLoad) return;
      var truncated = (float) Math.Truncate(e.NewValue);
      OpacityText.Text = $"Notification Opacity: {truncated}";
      configContent.Opacity = truncated / 100f;
      ElementSoundPlayer.Play(ElementSoundKind.Focus);
      MarkDirty(true);
    }

    private void EnableNotifications(object sender, RoutedEventArgs e)
    {
      var senderName = ((CheckBox) sender).Name;
      ToggleNotificationState(senderName, true);
      MarkDirty(true);
    }

    private void DisableNotifications(object sender, RoutedEventArgs e)
    {
      var senderName = ((CheckBox) sender).Name;
      ToggleNotificationState(senderName, false);
      MarkDirty(true);
    }

    private void ToggleNotificationState(string senderName, bool targetState)
    {
      switch (senderName)
      {
        case "PlayerJoinedToggle":
        {
          configContent.DisplayPlayerJoined = targetState;
          break;
        }
        case "PlayerLeftToggle":
        {
          configContent.DisplayPlayerLeft = targetState;
          break;
        }
        case "WorldChangedToggle":
        {
          configContent.DisplayWorldChanged = targetState;
          break;
        }
        case "PortalDroppedToggle":
        {
          configContent.DisplayPortalDropped = targetState;
          break;
        }
        case "ShaderKeywordsToggle":
        {
          configContent.DisplayMaximumKeywordsExceeded = targetState;
          break;
        }
      }
    }

    private void ToggleAdvancedMode(object sender, RoutedEventArgs e)
    {
      if (AdvancedMode == Visibility.Visible)
      {
        AdvancedMode = Visibility.Collapsed;
        ElementSoundPlayer.Play(ElementSoundKind.Show);
      }
      else
      {
        AdvancedMode = Visibility.Visible;
        ElementSoundPlayer.Play(ElementSoundKind.Hide);
      }
    }

    private void ToggleJoinLeaveSilence(object sender, RoutedEventArgs e)
    {
      configContent.DisplayJoinLeaveSilencedOverride = !configContent.DisplayJoinLeaveSilencedOverride;
      MarkDirty(true);
    }

    private float scrollViewHeight = 375f;

    public float ScrollViewHeight
    {
      get => scrollViewHeight;
      set
      {
        scrollViewHeight = value;
        NotifyPropertyChanged();
      }
    }

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
      // Because the initial resize happens before the proper layout - we can end up in a state where the proper size wasn't set
      var topPanelHeight = TopPanel.ActualHeight == 0 ? 56 : TopPanel.ActualHeight;
      ScrollViewHeight = (float) (args.Size.Height - topPanelHeight + 10d); // add some padding
    }

    private void NumberBoxChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
      var senderName = sender.Name;
      switch (senderName)
      {
        case "ParseFrequencyMs":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 100)
          {
            ParseFrequencyMs.Value = 100;
          }

          configContent.ParseFrequencyMilliseconds = (long) args.NewValue;
          break;
        }
        case "DirectoryPollFrequencyMs":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 100)
          {
            DirectoryPollFrequencyMs.Value = 100;
          }

          configContent.DirectoryPollFrequencyMilliseconds = (long) args.NewValue;
          break;
        }
        case "PlayerJoinedNotifTimeoutS":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 0.5)
          {
            PlayerJoinedNotifTimeoutS.Value = 0.5;
          }

          configContent.PlayerJoinedNotificationTimeoutSeconds = (float)args.NewValue;
          break;
          }
        case "PlayerLeftNotifTimeoutS":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 0.5)
          {
            PlayerLeftNotifTimeoutS.Value = 0.5;
          }

          configContent.PlayerLeftNotificationTimeoutSeconds = (float)args.NewValue;
          break;
        }
        case "WorldChangedNotifTimeoutS":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 0.5)
          {
            WorldChangedNotifTimeoutS.Value = 0.5;
          }

          configContent.WorldChangedNotificationTimeoutSeconds = (float)args.NewValue;
          break;
        }
        case "WorldJoinSilenceSeconds":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 1)
          {
            WorldJoinSilenceSeconds.Value = 1;
          }

          configContent.WorldJoinSilenceSeconds = (long)args.NewValue;
          break;
        }
        case "PortalDroppedNotifTimeoutS":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 0.5)
          {
            PortalDroppedNotifTimeoutS.Value = 0.5;
          }

          configContent.PortalDroppedTimeoutSeconds = (float)args.NewValue;
          break;
        }
        case "MaximumKeywordsNotifTimeoutS":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 0.5)
          {
            MaximumKeywordsNotifTimeoutS.Value = 0.5;
          }

          configContent.MaximumKeywordsExceededTimeoutSeconds = (float)args.NewValue;
          break;
        }
        case "MaximumKeywordsNotifCooldownS":
        {
          if (double.IsNaN(args.NewValue) || args.NewValue < 0.5)
          {
            MaximumKeywordsNotifCooldownS.Value = 0.5;
          }

          configContent.MaximumKeywordsExceededCooldownSeconds = (float)args.NewValue;
          break;
        }
      }

      MarkDirty(true);
    }

    private void PathTextChanged(object sender, TextChangedEventArgs e)
    {
      var senderName = ((TextBox) sender).Name;
      switch (senderName)
      {
        case "OutputLogRoot":
        {
          if (OutputLogRoot.Text == "")
          {
            OutputLogRoot.Text = "%AppData%\\..\\LocalLow\\VRChat\\VRChat";
          }
          configContent.OutputLogRoot = OutputLogRoot.Text;
          break;
        }
        case "PlayerJoinedIconPath":
        {
          if (PlayerJoinedIconPath.Text == "")
          {
            PlayerJoinedIconPath.Text = "\\Resources\\Icons\\player_joined.png";
          }
          configContent.PlayerJoinedIconPath = PlayerJoinedIconPath.Text;
          break;
        }
        case "PlayerJoinedAudioPath":
        {
          if (PlayerJoinedAudioPath.Text == "")
          {
            PlayerJoinedAudioPath.Text = "\\Resources\\Audio\\player_joined.png";
          }
          configContent.PlayerJoinedAudioPath = PlayerJoinedAudioPath.Text;
          break;
        }
        case "PlayerLeftIconPath":
        {
          if (PlayerLeftIconPath.Text == "")
          {
            PlayerLeftIconPath.Text = "\\Resources\\Icons\\player_left.png";
          }
          configContent.PlayerLeftIconPath = PlayerLeftIconPath.Text;
          break;
        }
        case "PlayerLeftAudioPath":
        {
          if (PlayerLeftAudioPath.Text == "")
          {
            PlayerLeftAudioPath.Text = "\\Resources\\Audio\\player_left.png";
          }
          configContent.PlayerLeftAudioPath = PlayerLeftAudioPath.Text;
          break;
        }
        case "WorldChangedIconPath":
        {
          if (WorldChangedIconPath.Text == "")
          {
            WorldChangedIconPath.Text = "\\Resources\\Icons\\world_changed.png";
          }
          configContent.WorldChangedIconPath = WorldChangedIconPath.Text;
          break;
        }
        case "WorldChangedAudioPath":
        {
          if (WorldChangedAudioPath.Text == "")
          {
            WorldChangedAudioPath.Text = "default";
          }
          configContent.WorldChangedAudioPath = WorldChangedAudioPath.Text;
          break;
        }
        case "PortalDroppedIconPath":
        {
          if (PortalDroppedIconPath.Text == "")
          {
            PortalDroppedIconPath.Text = "\\Resources\\Icons\\world_changed.png";
          }
          configContent.PortalDroppedIconPath = PortalDroppedIconPath.Text;
          break;
        }
        case "PortalDroppedAudioPath":
        {
          if (PortalDroppedAudioPath.Text == "")
          {
            PortalDroppedAudioPath.Text = "default";
          }
          configContent.PortalDroppedAudioPath = PortalDroppedAudioPath.Text;
          break;
        }
        case "MaximumKeywordsIconPath":
        {
          if (MaximumKeywordsIconPath.Text == "")
          {
            MaximumKeywordsIconPath.Text = "\\Resources\\Icons\\keywords_exceeded.png";
          }
          configContent.MaximumKeywordsExceededIconPath = MaximumKeywordsIconPath.Text;
          break;
        }
        case "MaximumKeywordsAudioPath":
        {
          if (MaximumKeywordsAudioPath.Text == "")
          {
            MaximumKeywordsAudioPath.Text = "warning";
          }
          configContent.MaximumKeywordsExceededAudioPath = MaximumKeywordsAudioPath.Text;
          break;
        }
      }
    }
  }
}