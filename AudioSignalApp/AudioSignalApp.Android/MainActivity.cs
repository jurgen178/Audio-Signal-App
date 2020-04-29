// <copyright file="SettingViewModel.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Android.Runtime;
    using Android.OS;
    using Android.Content.Res;

    [Activity(Label = "AudioSignalApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="savedInstanceState">State of the saved instance.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            SetAppTheme();
        }

        /// <summary>
        /// Sets the application theme.
        /// </summary>
        private void SetAppTheme()
        {
            bool isDark = Resources.Configuration.UiMode.HasFlag(UiMode.NightYes);

            if (MainPage.SelectedTheme == SelectedThemeEnum.Auto && isDark
                || MainPage.SelectedTheme == SelectedThemeEnum.Dark)
            {
                App.Current.Resources = new DarkTheme();
            }
            else
            if (MainPage.SelectedTheme == SelectedThemeEnum.Auto && !isDark
                || MainPage.SelectedTheme == SelectedThemeEnum.Light)
            {
                App.Current.Resources = new LightTheme();
            }

            MainPage.IsCurrentSystemThemeDark = isDark;
        }

        /// <summary>
        /// To be added.
        /// </summary>
        /// <param name="requestCode">To be added.</param>
        /// <param name="permissions">To be added.</param>
        /// <param name="grantResults">To be added.</param>
        /// <remarks>
        /// Portions of this page are modifications based on work created and shared by the <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android Open Source Project</a></format> and used according to terms described in the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative Commons 2.5 Attribution License.</a></format>
        /// </remarks>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}