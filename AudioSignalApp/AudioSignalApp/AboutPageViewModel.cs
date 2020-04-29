// <copyright file="AboutPageViewModel.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace XamlMvvm
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using AudioSignalApp;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    /// <summary>
    /// AboutPageViewModel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class AboutPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPageViewModel"/> class.
        /// </summary>
        public AboutPageViewModel()
        {
            this.ClickCommand = new Command<string>(async (url) =>
            {
                await Launcher.OpenAsync(url);
            });
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the click command.
        /// </summary>
        /// <value>
        /// The click command.
        /// </value>
        public Command ClickCommand { get; }
    }
}