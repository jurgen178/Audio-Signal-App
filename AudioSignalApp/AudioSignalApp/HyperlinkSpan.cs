// <copyright file="HyperlinkSpan.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using Xamarin.Essentials;
    using Xamarin.Forms;

    /// <summary>
    /// HyperlinkSpan.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Span" />
    public class HyperlinkSpan : Span
    {
        /// <summary>
        /// The URL property.
        /// </summary>
        public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkSpan), null);

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperlinkSpan"/> class.
        /// </summary>
        public HyperlinkSpan()
        {
            this.TextDecorations = TextDecorations.Underline;
            this.TextColor = Color.Blue;
            this.GestureRecognizers.Add(new TapGestureRecognizer
            {
                // Launcher.OpenAsync is provided by Xamarin.Essentials.
                Command = new Command(async () => await Launcher.OpenAsync(this.Url)),
            });
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url
        {
            get
            {
                return (string)this.GetValue(UrlProperty);
            }

            set
            {
                this.SetValue(UrlProperty, value);
            }
        }
    }
}