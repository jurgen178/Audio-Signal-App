// <copyright file="SettingViewModel.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace XamlMvvm
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using AudioSignalApp;
    using Xamarin.Essentials;

    /// <summary>
    /// SettingViewModel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class SettingViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingViewModel"/> class.
        /// </summary>
        public SettingViewModel()
        {
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets or sets the umin faktor.
        /// </summary>
        /// <value>
        /// The umin faktor.
        /// </value>
        public int UminFaktor
        {
            get => Preferences.Get($"{PreferenceName.UminFaktor}", 2);
            set
            {
                int uminFaktor = Math.Min(Math.Max(value, 1), 100);
                Preferences.Set($"{PreferenceName.UminFaktor}", uminFaktor);
                this.OnPropertyChanged(nameof(this.UminFaktor));
                MainPage.UminFaktor = uminFaktor;
            }
        }

        /// <summary>
        /// Gets or sets Frequenzaufloesung.
        /// </summary>
        /// <value>
        /// Frequenzaufloesung in Hz.
        /// </value>
        public int FrequenzAufloesungInHz
        {
            // Frequenzauflösung = Abtastrate / FFTBlockLänge
            // FFT = 1024, Sampling Rate = 8192 :
            // Frequenzauflösung einer Spektrallinie: 8192 / 1024 = 8 Hz.
            get => Preferences.Get($"{PreferenceName.FrequenzAufloesungInHz}", 5);
            set
            {
                Preferences.Set($"{PreferenceName.FrequenzAufloesungInHz}", value);
                this.OnPropertyChanged(nameof(this.FrequenzAufloesungInHz));
                MainPage.FrequenzAufloesung = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is umin visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is umin visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsUminVisible
        {
            get => Preferences.Get($"{PreferenceName.IsUminVisible}", true);
            set
            {
                Preferences.Set($"{PreferenceName.IsUminVisible}", value);
                this.OnPropertyChanged(nameof(this.IsUminVisible));
                MainPage.IsUminVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is audio signal visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is audio signal visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsAudioSignalVisible
        {
            get => Preferences.Get($"{PreferenceName.IsAudioSignalVisible}", true);
            set
            {
                Preferences.Set($"{PreferenceName.IsAudioSignalVisible}", value);
                this.OnPropertyChanged(nameof(this.IsAudioSignalVisible));
                MainPage.IsAudioSignalVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is spektrogramm visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is spektrogramm visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsSpektrogrammVisible
        {
            get => Preferences.Get($"{PreferenceName.IsSpektrogrammVisible}", true);
            set
            {
                Preferences.Set($"{PreferenceName.IsSpektrogrammVisible}", value);
                this.OnPropertyChanged(nameof(this.IsSpektrogrammVisible));
                MainPage.IsSpektrogrammVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SettingViewModel"/> is leistungsspektrum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if leistungsspektrum; otherwise, <c>false</c>.
        /// </value>
        public bool Leistungsspektrum
        {
            get => Preferences.Get($"{PreferenceName.Leistungsspektrum}", true);
            set
            {
                Preferences.Set($"{PreferenceName.Leistungsspektrum}", value);
                this.OnPropertyChanged(nameof(this.Leistungsspektrum));
                MainPage.Leistungsspektrum = value;
            }
        }

        /// <summary>
        /// Gets the theme selection list.
        /// </summary>
        /// <value>
        /// The theme selection list.
        /// </value>
        public IList<string> ThemeSelectionList
        {
            get
            {
                return new List<string> { "Systemstandardeinstellung", "Hell", "Dunkel" };
            }
        }

        /// <summary>
        /// Gets the selected theme.
        /// </summary>
        /// <value>
        /// The selected theme.
        /// </value>
        public SelectedThemeEnum SelectedTheme
        {
            get => (SelectedThemeEnum)Preferences.Get($"{PreferenceName.SelectedTheme}", (int)SelectedThemeEnum.Auto);
        }
    }
}