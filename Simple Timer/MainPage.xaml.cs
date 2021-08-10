using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Notepads.Brushes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Simple_Timer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public int PaneWidth { get; set; } = 240;
        public Configs Configs { get; set; }
        private List<TextBlock> AverageTexts { get; set; } = new List<TextBlock>();
        private DispatcherTimer UpdateTimer { get; set; }
        private DispatcherTimer DelayTimer { get; set; }
        private Stopwatch TimerTimer { get; set; } = new Stopwatch();
        private DropDownButton ScrambleTitleDropDownButton { get; set; }
        private TextBlock ScrambleText { get; set; }
        private bool isDown = false;
        private bool isTiming = false;
        private bool isInspecting = false;
        private bool canStartTiming = false;
        private bool configLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            Window.Current.SetTitleBar(AppTitleBar);

            DelayTimer = new DispatcherTimer();
            DelayTimer.Tick += DelayFinished;

            UpdateTimer = new DispatcherTimer();
            UpdateTimer.Tick += TimerUpdateLoop;
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 8);
            UpdateTimer.Start();

            Configs = new Configs();
            var task = Task.Run(Configs.Load);
            task.Wait();

            ScrambleFontSizeSlider.Value = Configs.ScrambleFontSize;
            TimerFontSizeSlider.Value = Configs.TimerFontSize;
            mo3Toggle.IsOn = Configs.Mo3Toggle;
            ao5Toggle.IsOn = Configs.Ao5Toggle;
            ao12Toggle.IsOn = Configs.Ao12Toggle;
            InspectionToggle.IsOn = Configs.InspectionToggle;
            ScrambleToggle.IsOn = Configs.ScrambleToggle;
            AcrylicToggle.IsOn = Configs.AcrylicToggle;

            if (!(ScrambleTitleDropDownButton is null))
            {
                ScrambleTitleDropDownButton.Content = Configs.ScrambleCubeType;
                ScrambleText.Text = ScrambleGenerator.GetScrambleMoves(Configs.ScrambleCubeType);
            }

            configLoaded = true;

            ScrambleText.FontSize = Configs.ScrambleFontSize;

            var navViewBrush = Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush;
            PaneGrid.Background = new HostBackdropAcrylicBrush() 
            { 
                TintOpacity = 0.4f, 
                LuminosityColor = navViewBrush.Color 
            };
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Height = sender.Height;
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey != Windows.System.VirtualKey.Space)
                return;

            if (!isTiming)
            {
                DelayTimer.Stop();
                TimerText.Foreground = Resources["DefaultTextForegroundThemeBrush"] as Brush;

                if (canStartTiming)
                {
                    if (Configs.InspectionToggle && !isInspecting)
                    {
                        isInspecting = true;
                        TimerTimer.Restart();
                        TimerTimer.Start();
                    }
                    else
                    {
                        isInspecting = false;
                        isTiming = true;
                        TimerTimer.Restart();
                        TimerTimer.Start();
                        canStartTiming = !canStartTiming;
                    }
                }
            }
            else
            {
                isTiming = false;
                ScrambleText.Text = ScrambleGenerator.GetScrambleMoves(ScrambleTitleDropDownButton.Content as string);
            }

            isDown = false;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey != Windows.System.VirtualKey.Space)
                return;

            if (isDown)
                return;

            if (!isTiming && !isInspecting)
            {
                TimerText.Foreground = new SolidColorBrush(Colors.Red);
                DelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                DelayTimer.Start();
            }
            else if (isTiming && !isInspecting)
            {
                TimerText.Foreground = Resources["DefaultTextForegroundThemeBrush"] as Brush;
                TimerTimer.Stop();
            }

            isDown = true;
        }

        private void DelayFinished(object sender, object e)
        {
            DelayTimer.Stop();

            if (isDown)
            {
                canStartTiming = true;
                TimerText.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void TimerUpdateLoop(object sender, object args)
        {
            var timer = sender as DispatcherTimer;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 8);

            var time = TimerTimer.Elapsed;

            if (time.Hours > 0)
                TimerText.Text = time.ToString(@"h\:mm\:ss.fff");
            else if (time.Minutes > 0)
                TimerText.Text = time.ToString(@"m\:ss\.fff");
            else
                TimerText.Text = time.ToString(@"s\.fff");

            if (isInspecting)
                TimerText.Text = time.Seconds.ToString();
        }

        private void UpdateAverageTexts()
        {
            AverageTexts.Sort((a, b) => b.Name.CompareTo(a.Name));

            for (int i = MainSectionPanel.Children.Count - 1; i > -1; --i)
            {
                var text = MainSectionPanel.Children[i] as TextBlock;

                if (!(text is null) && (text.Name == "mo3Text" || text.Name == "ao5Text" || text.Name == "ao12Text"))
                {
                    MainSectionPanel.Children.RemoveAt(i);
                }
            }

            if (configLoaded)
                Configs.SetToggles();

            foreach (var text in AverageTexts)
            {
                MainSectionPanel.Children.Add(text);

                if (text.Name == "mo3Text") Configs.Mo3Toggle = true;
                else if (text.Name == "ao5Text") Configs.Ao5Toggle = true;
                else if (text.Name == "ao12Text") Configs.Ao12Toggle = true;
            }

            if (!configLoaded)
                return;

            var task = Task.Run(Configs.Save);
            task.Wait();
        }

        private void mo3Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            if (toggle.IsOn)
            {
                var text = new TextBlock();
                text.Name = "mo3Text";
                text.Text = "mo3: -";
                text.HorizontalAlignment = HorizontalAlignment.Center;
                AverageTexts.Add(text);
            }
            else
            {
                foreach (var text in AverageTexts)
                {
                    if (text.Name == "mo3Text")
                    {
                        AverageTexts.Remove(text);
                        break;
                    }
                }
            }

            UpdateAverageTexts();
        }

        private void ao5Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            if (toggle.IsOn)
            {
                var text = new TextBlock();
                text.Name = "ao5Text";
                text.Text = "ao5: -";
                text.HorizontalAlignment = HorizontalAlignment.Center;
                AverageTexts.Add(text);
            }
            else
            {
                foreach (var text in AverageTexts)
                {
                    if (text.Name == "ao5Text")
                    {
                        AverageTexts.Remove(text);
                        break;
                    }
                }
            }

            UpdateAverageTexts();
        }

        private void ao12Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            if (toggle.IsOn)
            {
                var text = new TextBlock();
                text.Name = "ao12Text";
                text.Text = "ao12: -";
                text.HorizontalAlignment = HorizontalAlignment.Center;
                AverageTexts.Add(text);
            }
            else
            {
                foreach (var text in AverageTexts)
                {
                    if (text.Name == "ao12Text")
                    {
                        AverageTexts.Remove(text);
                        break;
                    }
                }
            }

            UpdateAverageTexts();
        }
        private void ScrambleFontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Configs is null || !configLoaded)
                return;

            var slider = sender as Slider;
            Configs.ScrambleFontSize = (int)slider.Value;
            ScrambleText.FontSize = Configs.ScrambleFontSize;

            if (!configLoaded)
                return;

            var task = Task.Run(Configs.Save);
            task.Wait();
        }

        private void TimerFontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Configs is null || !configLoaded)
                return;

            var slider = sender as Slider;
            Configs.TimerFontSize = (int)slider.Value;

            if (!configLoaded)
                return;

            var task = Task.Run(Configs.Save);
            task.Wait();
        }

        private void ScrambleToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            if (toggle.IsOn)
            {
                var scramblePanel = new StackPanel();
                scramblePanel.Name = "ScramblePanel";
                scramblePanel.HorizontalAlignment = HorizontalAlignment.Center;
                scramblePanel.VerticalAlignment = VerticalAlignment.Top;
                scramblePanel.Margin = new Thickness(10, 20, 10, 10);

                ScrambleTitleDropDownButton = new DropDownButton();
                ScrambleTitleDropDownButton.CornerRadius = new CornerRadius(4d);
                ScrambleTitleDropDownButton.HorizontalAlignment = HorizontalAlignment.Center;
                ScrambleTitleDropDownButton.Content = "Select a Cube";
                ScrambleTitleDropDownButton.Width = 180;
                ScrambleTitleDropDownButton.Flyout = new MenuFlyout() { Placement = FlyoutPlacementMode.Bottom };

                var menuFlyout = ScrambleTitleDropDownButton.Flyout as MenuFlyout;
                menuFlyout.Items.Add(new MenuFlyoutItem() { Text = "2x2x2" });
                menuFlyout.Items.Add(new MenuFlyoutItem() { Text = "3x3x3" });
                menuFlyout.Items.Add(new MenuFlyoutItem() { Text = "4x4x4" });

                foreach (MenuFlyoutItem item in menuFlyout.Items)
                {
                    item.Click += ScrambleTitleChanged;
                }

                ScrambleText = new TextBlock();
                ScrambleText.TextAlignment = TextAlignment.Center;
                ScrambleText.HorizontalAlignment = HorizontalAlignment.Center;
                ScrambleText.Style = Resources["BodyTextBlockStyle"] as Style;
                ScrambleText.Text = "Scramble Moves";

                scramblePanel.Children.Add(ScrambleTitleDropDownButton);
                scramblePanel.Children.Add(ScrambleText);

                MainSectionGrid.Children.Add(scramblePanel);
            }
            else
            {
                for (int i = MainSectionGrid.Children.Count; i > -1; --i)
                {
                    var panel = MainSectionGrid.Children[i] as StackPanel;

                    if (panel is null || panel.Name != "ScramblePanel")
                        continue;

                    MainSectionGrid.Children.RemoveAt(i);
                    break;
                }
            }

            Configs.ScrambleToggle = toggle.IsOn;
            var task = Task.Run(Configs.Save);
            task.Wait();
        }

        private void ScrambleTitleChanged(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            ScrambleTitleDropDownButton.Content = item.Text;
            Configs.ScrambleCubeType = item.Text;

            var task = Task.Run(Configs.Save);
            task.Wait();

            ScrambleText.Text = ScrambleGenerator.GetScrambleMoves(item.Text);
        }

        private void InspectionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            Configs.InspectionToggle = toggle.IsOn;

            var task = Task.Run(Configs.Save);
            task.Wait();
        }

        private void AcrylicToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;

            if (toggle.IsOn)
            {
                var pageBrush = Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush;
                MainSectionAcrylicGrid.Background = new HostBackdropAcrylicBrush()
                {
                    TintOpacity = 0.2f,
                    LuminosityColor = pageBrush.Color
                };
            }
            else
            {
                MainSectionAcrylicGrid.Background = Resources["ApplicationPageBackgroundThemeBrush"] as Brush;
            }

            Configs.AcrylicToggle = toggle.IsOn;
            var task = Task.Run(Configs.Save);
            task.Wait();
        }
    }
}
