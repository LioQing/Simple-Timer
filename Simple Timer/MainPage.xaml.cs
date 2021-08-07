using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

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
        private bool isDown = false;
        private bool wasDown = false;
        private bool isTiming = false;
        private bool canStartTiming = false;
        private bool configLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp; ;

            DelayTimer = new DispatcherTimer();
            DelayTimer.Tick += DelayFinished;

            UpdateTimer = new DispatcherTimer();
            UpdateTimer.Tick += TimerUpdateLoop;
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 8);
            UpdateTimer.Start();

            Configs = new Configs();
            var task = Task.Run(Configs.Load);
            task.Wait();

            TimerFontSizeSlider.Value = Configs.TimerFontSize;
            mo3Toggle.IsOn = Configs.Mo3Toggle;
            ao5Toggle.IsOn = Configs.Ao5Toggle;
            ao12Toggle.IsOn = Configs.Ao12Toggle;

            configLoaded = true;
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (!isTiming)
            {
                DelayTimer.Stop();
                TimerText.Foreground = new SolidColorBrush(Colors.Black);
                isTiming = true;

                if (canStartTiming)
                {
                    TimerTimer.Restart();
                    TimerTimer.Start();
                    canStartTiming = !canStartTiming;
                }
            }
            else
            {
                TimerText.Foreground = new SolidColorBrush(Colors.Black);
                TimerTimer.Stop();
            }
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!isTiming)
            {
                TimerText.Foreground = new SolidColorBrush(Colors.Red);
                DelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                DelayTimer.Start();
            }
            else
            {
                isTiming = false;
            }
        }

        private void DelayFinished(object sender, object e)
        {
            DelayTimer.Stop();

            canStartTiming = true;

            if (isDown)
                TimerText.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void TimerUpdateLoop(object sender, object args)
        {
            var timer = sender as DispatcherTimer;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 8);

            TimerText.Text = TimerTimer.Elapsed.ToString("mm\\:ss\\.fff");

            wasDown = isDown;
        }

        private void UpdateAverageTexts()
        {
            AverageTexts.Sort((a, b) => b.Name.CompareTo(a.Name));

            for (int i = MainSectionPanel.Children.Count - 1; i > -1; --i)
            {
                var text = MainSectionPanel.Children[i] as TextBlock;

                if (text.Name == "mo3Text" || text.Name == "ao5Text" || text.Name == "ao12Text")
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

            Configs.Save();
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

        private async void TimerFontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Configs is null || !configLoaded)
                return;

            var slider = sender as Slider;
            Configs.TimerFontSize = (int)slider.Value;

            if (!configLoaded)
                return;

            Configs.Save();
        }
    }
}
