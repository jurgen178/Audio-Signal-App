// <copyright file="MainPageViewModel.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace XamlMvvm
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AudioSignalApp;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    /// <summary>
    /// MainPageViewModel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class MainPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.StartStopCommand = new Command(() => this.StartStop());
            this.SettingCommand = new Command(async () => await this.SettingHandler());
            this.AboutCommand = new Command(async () => await this.AboutHandler());
            this.ResetCommand = new Command(() => this.ResetSettings());

            this.SampleRateInHz = Preferences.Get($"{PreferenceName.SampleRateInHz}", 11025);

            this.SampleRateInHzCommand = new Command(() =>
            {
                string newSampleRate = Regex.Replace(this.SampleRateInHzWert, "\\D", string.Empty);    // \D = alles außer 0-9
                int.TryParse(newSampleRate, out int value);
                this.SampleRateInHz = value;
            });
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the start stop command.
        /// </summary>
        /// <value>
        /// The start stop command.
        /// </value>
        public Command StartStopCommand { get; }

        /// <summary>
        /// Gets the setting command.
        /// </summary>
        /// <value>
        /// The setting command.
        /// </value>
        public Command SettingCommand { get; }

        /// <summary>
        /// Gets the about command.
        /// </summary>
        /// <value>
        /// The about command.
        /// </value>
        public Command AboutCommand { get; }

        /// <summary>
        /// Gets the sample rate in hz command.
        /// </summary>
        /// <value>
        /// The sample rate in hz command.
        /// </value>
        public Command SampleRateInHzCommand { get; }

        /// <summary>
        /// Gets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public Command ResetCommand { get; }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Settings the handler.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SettingHandler()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SettingPage());
        }

        /// <summary>
        /// Abouts the handler.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AboutHandler()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AboutPage());
        }

        /// <summary>
        /// Start stop handler.
        /// </summary>
        public void StartStop()
        {
            MainPage.RecordEnabled = !MainPage.RecordEnabled;

            this.StartStopImage = MainPage.RecordEnabled ? "stop.png" : "start.png";
        }

        /// <summary>
        /// Reset the settings.
        /// </summary>
        public void ResetSettings()
        {
            Preferences.Clear();
            MainPage.InitializeSettings();
            MainPage.SetTheme(SelectedThemeEnum.Auto);
            this.SampleRateInHz = Preferences.Get($"{PreferenceName.SampleRateInHz}", 11025);
        }

        /// <summary>
        /// Gets or sets the sample rate in Hz.
        /// </summary>
        /// <value>
        /// The sample rate in Hz.
        /// </value>
        public int SampleRateInHz
        {
            get
            {
                return Preferences.Get($"{PreferenceName.SampleRateInHz}", 11025);
            }

            set
            {
                int sampleRateInHz = (value > 22050) ? 11025 : ((value < 4000) ? 11025 : value);
                sampleRateInHz = sampleRateInHz % 11025 == 0 ? sampleRateInHz : sampleRateInHz / 100 * 100;
                Preferences.Set($"{PreferenceName.SampleRateInHz}", sampleRateInHz);
                this.OnPropertyChanged(nameof(this.SampleRateInHz));
                this.SampleRateInHzWert = $"{sampleRateInHz} Hz";
                MainPage.SampleRateInHz = sampleRateInHz;
            }
        }

        private string sampleRateInHzWert = string.Empty;

        /// <summary>
        /// Gets or sets the sample rate in Hz.
        /// </summary>
        /// <value>
        /// The sample rate in Hz.
        /// </value>
        public string SampleRateInHzWert
        {
            get => this.sampleRateInHzWert;
            set
            {
                this.sampleRateInHzWert = value;
                this.OnPropertyChanged(nameof(this.SampleRateInHzWert));
            }
        }

        private string startStopImage = "stop.png";

        /// <summary>
        /// Gets or sets the start stop image.
        /// </summary>
        /// <value>
        /// The start stop image.
        /// </value>
        public string StartStopImage
        {
            get => this.startStopImage;
            set
            {
                this.startStopImage = value;
                this.OnPropertyChanged(nameof(this.StartStopImage));
            }
        }
    }
}