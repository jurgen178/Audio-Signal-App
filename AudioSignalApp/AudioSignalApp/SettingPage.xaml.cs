// <copyright file="SettingPage.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using System;
    using Xamarin.Essentials;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// SettingPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ContentPage" />
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingPage"/> class.
        /// </summary>
        public SettingPage()
        {
            this.InitializeComponent();
        }

        // Windows style: Klick auf den Checkboxtext ändert auch Checkbox.
        private void IsUminVisibleTap(object sender, EventArgs e)
        {
            this.IsUminVisibleCheckbox.IsChecked = !MainPage.IsUminVisible;
        }

        private void IsAudioSignalVisibleTap(object sender, EventArgs e)
        {
            this.IsAudioSignalVisibleCheckbox.IsChecked = !MainPage.IsAudioSignalVisible;
        }

        private void IsSpektrogrammVisibleTap(object sender, EventArgs e)
        {
            this.IsSpektrogrammVisibleCheckbox.IsChecked = !MainPage.IsSpektrogrammVisible;
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedTheme = Preferences.Get($"{PreferenceName.SelectedTheme}", (int)SelectedThemeEnum.Auto);
            Xamarin.Forms.Picker picker = sender as Xamarin.Forms.Picker;
            if (selectedTheme != picker.SelectedIndex)
            {
                MainPage.SetTheme((SelectedThemeEnum)picker.SelectedIndex);
            }
        }
    }
}