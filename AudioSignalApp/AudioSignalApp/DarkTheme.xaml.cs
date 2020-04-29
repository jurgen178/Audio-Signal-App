// <copyright file="DarkTheme.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// DarkTheme.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ResourceDictionary" />
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DarkTheme : ResourceDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DarkTheme"/> class.
        /// </summary>
        public DarkTheme()
        {
            this.InitializeComponent();
        }
    }
}