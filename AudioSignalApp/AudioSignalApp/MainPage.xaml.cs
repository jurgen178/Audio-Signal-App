// <copyright file="MainPage.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Android.Media;
    using SkiaSharp;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    /// <summary>
    /// MainPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ContentPage" />
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Stop recording.
        /// </summary>
        public static bool RecordEnabled;

        /// <summary>
        /// Is current system theme dark.
        /// </summary>
        public static bool IsCurrentSystemThemeDark;

        /// <summary>
        /// The sample rate in Hz.
        /// </summary>
        public static int SampleRateInHz;

        /// <summary>
        /// The umin faktor.
        /// </summary>
        public static int UminFaktor;

        /// <summary>
        /// Is umin visible.
        /// </summary>
        public static bool IsUminVisible;

        /// <summary>
        /// Is audio signal visible.
        /// </summary>
        public static bool IsAudioSignalVisible;

        /// <summary>
        /// Is spektrogramm visible.
        /// </summary>
        public static bool IsSpektrogrammVisible;

        /// <summary>
        /// The selected theme.
        /// </summary>
        public static SelectedThemeEnum SelectedTheme;

        /// <summary>
        /// Display Leistungsspektrum.
        /// </summary>
        public static bool Leistungsspektrum;

        /// <summary>
        /// Frequenzaufloesung.
        /// </summary>
        public static int FrequenzAufloesung;

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        public static void InitializeSettings()
        {
            RecordEnabled = true;
            IsCurrentSystemThemeDark = true;
            SampleRateInHz = Preferences.Get($"{PreferenceName.SampleRateInHz}", 11025);
            UminFaktor = Preferences.Get($"{PreferenceName.UminFaktor}", 4);
            IsUminVisible = Preferences.Get($"{PreferenceName.IsUminVisible}", true);
            IsAudioSignalVisible = Preferences.Get($"{PreferenceName.IsAudioSignalVisible}", true);
            IsSpektrogrammVisible = Preferences.Get($"{PreferenceName.IsSpektrogrammVisible}", true);
            SelectedTheme = (SelectedThemeEnum)Preferences.Get($"{PreferenceName.SelectedTheme}", (int)SelectedThemeEnum.Auto);
            Leistungsspektrum = Preferences.Get($"{PreferenceName.Leistungsspektrum}", true);
            FrequenzAufloesung = Preferences.Get($"{PreferenceName.FrequenzAufloesungInHz}", 5);
        }

        // Nicht static machen.
        private readonly object fftLock = new object();
        private readonly object audioLock = new object();

        private short[] audioBuffer = null;
        private int[] fftBuffer = null;

        private AudioRecord audioRecord = null;
        private int requestedSampleSize = -1;
        private int requestedFrequenzAufloesung = -1;
        private bool validData = false;
        private string errorMsg = string.Empty;

        private SKPaint filterRect = new SKPaint
        {
        };

        private SKPaint fftFreqText = new SKPaint
        {
            TextSize = 20,
            IsAntialias = true,
        };

        private SKPaint audioText = new SKPaint
        {
            Color = SKColors.Yellow,
            TextSize = 40,
            IsAntialias = true,
        };

        private SKPaint fehlerText = new SKPaint
        {
            TextSize = 40,
            IsAntialias = true,
            TextAlign = SKTextAlign.Left,
        };

        private SKPaint fftSignalLines = new SKPaint
        {
        };

        private SKPaint fftSignal = new SKPaint
        {
            StrokeWidth = 3,
            IsAntialias = true,
        };

        private SKPaint freqMaxRect = new SKPaint
        {
        };

        private SKPaint zaehler = new SKPaint
        {
            Color = SKColors.DarkOrange,
            TextSize = 100,
            TextAlign = SKTextAlign.Right,
            IsAntialias = true,
        };

        private SKPaint lineText = new SKPaint
        {
            Color = SKColors.DarkOrange,
            TextSize = 30,
            IsAntialias = true,
        };

        private SKPaint audioLines = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
        };

        private SKPaint audio = new SKPaint
        {
            Color = SKColors.LightCyan,
            IsAntialias = true,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeSettings();
            this.InitializeComponent();

            // Update UI every 100ms.
            Device.StartTimer(TimeSpan.FromSeconds(1 / 10f), () =>
            {
                this.fftSignalCanvasView.InvalidateSurface();

                if (this.UminCanvasView.IsVisible != IsUminVisible
                   || this.audioSignalCanvasView.IsVisible != IsAudioSignalVisible
                   || this.spektrogrammSignalCanvasView.IsVisible != IsSpektrogrammVisible)
                {
                    spektrogrammBitmap = null;
                }

                this.UminCanvasView.IsVisible = IsUminVisible;
                this.audioSignalCanvasView.IsVisible = IsAudioSignalVisible;
                this.spektrogrammSignalCanvasView.IsVisible = IsSpektrogrammVisible;

                if (IsAudioSignalVisible)
                {
                    this.audioSignalCanvasView.InvalidateSurface();
                }

                if (IsSpektrogrammVisible)
                {
                    this.spektrogrammSignalCanvasView.InvalidateSurface();
                }

                return true;
            });

            this.GetAudioPermissionAsync().ConfigureAwait(true);

            // Backgroundthread processing the audio signal.
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (MainPage.RecordEnabled)
                        {
                            this.UpdateAudioRecord(SampleRateInHz, FrequenzAufloesung);
                            this.GetAudioRecord();
                            this.GoertzelFFT();
                        }

                        Thread.Sleep(50);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        /// <summary>
        /// Sets the theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        public static void SetTheme(SelectedThemeEnum theme)
        {
            SelectedTheme = theme;
            Preferences.Set($"{PreferenceName.SelectedTheme}", (int)theme);

            if ((theme == SelectedThemeEnum.Auto && IsCurrentSystemThemeDark)
                || MainPage.SelectedTheme == SelectedThemeEnum.Dark)
            {
                App.Current.Resources = new DarkTheme();
            }
            else
            if ((theme == SelectedThemeEnum.Auto && !IsCurrentSystemThemeDark)
                || MainPage.SelectedTheme == SelectedThemeEnum.Light)
            {
                App.Current.Resources = new LightTheme();
            }

            spektrogrammBitmap = null;
        }

        /// <summary>
        /// Gets the color of the theme.
        /// </summary>
        /// <param name="colorname">The colorname.</param>
        /// <returns>SKColor.</returns>
        private SKColor GetThemeColor(string colorname)
        {
            var color = (Xamarin.Forms.Color)App.Current.Resources[colorname];
            return new SKColor(
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255),
                (byte)(color.A * 255));
        }

        /// <summary>
        /// Indicates that the <see cref="T:Xamarin.Forms.Page" /> has been assigned a size.
        /// </summary>
        /// <param name="width">The width allocated to the <see cref="T:Xamarin.Forms.Page" />.</param>
        /// <param name="height">The height allocated to the <see cref="T:Xamarin.Forms.Page" />.</param>
        /// <remarks>
        /// To be added.
        /// </remarks>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                if (width > height)
                {
                    this.signalViews.Orientation = StackOrientation.Horizontal;
                }
                else
                {
                    this.signalViews.Orientation = StackOrientation.Vertical;
                }

                spektrogrammBitmap = null;

                this.panX1 = 0;
                this.panX2 = double.MaxValue;
                this.leftPan = true;
            }
        }

        /// <summary>
        /// Gets the audio permission asynchronous.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetAudioPermissionAsync()
        {
            var status = await this.CheckAndRequestPermissionAsync(new Permissions.Microphone());
            if (status != PermissionStatus.Granted)
            {
                _ = this.DisplayAlert("Audio Erlaubnis", "Die Erlaubnis das Audiosignal zu lesen wurde abgelehnt.", "Ok");
                return;
            }
        }

        /// <summary>
        /// Checks the and request permission asynchronous.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="permission">The permission.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
                    where T : Permissions.BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }

        /// <summary>
        /// Checks the and request permission asynchronous.
        /// </summary>
        /// <typeparam name="TPermission">The type of the permission.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<PermissionStatus> CheckAndRequestPermissionAsync<TPermission>()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status;
        }

        /// <summary>
        /// Updates the audio record.
        /// </summary>
        /// <param name="samples">The samples.</param>
        /// <param name="frequenzAufloesung">The frequenz aufloesung.</param>
        /// <exception cref="Exception">
        /// Invalid buffer size calculated; audio settings used may not be supported on this device
        /// or
        /// Unable to successfully initialize AudioStream; reporting State.Uninitialized.  If using an emulator, make sure it has access to the system microphone.
        /// </exception>
        public void UpdateAudioRecord(int samples, int frequenzAufloesung)
        {
            if (samples < 64 || frequenzAufloesung < 1)
            {
                return;
            }

            if (this.audioRecord != null
                && this.requestedSampleSize == samples
                && this.requestedFrequenzAufloesung == frequenzAufloesung)
            {
                return;
            }

            this.requestedSampleSize = samples;
            this.requestedFrequenzAufloesung = frequenzAufloesung;

            int bufferSize = AudioRecord.GetMinBufferSize(SampleRateInHz, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit);

            if (bufferSize < 0)
            {
                throw new Exception("Invalid buffer size calculated; audio settings used may not be supported on this device");
            }

            bufferSize = SampleRateInHz / frequenzAufloesung;
            bufferSize += bufferSize % 2;

            // Frequenzauflösung = Abtastrate / FFTLänge
            // FFT = 1024 and Sampling Rate = 8192,
            // Frequenzauflösung einer Spektrallinie: 8192 / 1024 = 8 Hz.
            try
            {
                this.N = bufferSize;
                this.c_real = new float[this.N];
                this.c_imag = new float[this.N];
                this.y_real = new float[this.N];
                this.y_imag = new float[this.N];

                if (this.audioRecord != null)
                {
                    this.audioRecord.Stop();
                }

                this.audioRecord = new AudioRecord(
                    AudioSource.Mic, // Hardware source of recording.
                    SampleRateInHz, // Frequency
                    ChannelIn.Mono, // Mono or stereo
                    Encoding.Pcm16bit, // Audio encoding
                    this.N); // Length of the audio clip.

                if (this.audioRecord.State == State.Uninitialized)
                {
                    throw new Exception("Unable to successfully initialize AudioStream; reporting State.Uninitialized.  If using an emulator, make sure it has access to the system microphone.");
                }
            }
            catch (Exception e)
            {
                this.errorMsg = e.Message;
                this.audioRecord = null;
                this.audioBuffer = null;
                this.validData = false;
                return;
            }

            lock (this.audioLock)
            {
                this.audioBuffer = new short[bufferSize];
            }

            this.audioRecord.StartRecording();
            this.validData = true;
        }

        /// <summary>
        /// Gets the audio record.
        /// </summary>
        public void GetAudioRecord()
        {
            lock (this.audioLock)
            {
                if (this.audioBuffer != null)
                {
                    this.audioRecord.Read(this.audioBuffer, 0, this.audioBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Gets the maximum value in the buffer.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Maximum value.</returns>
        private int GetBufferMaxValue(int m, short[] buffer)
        {
            for (int x = 0; x < buffer.Length; x++)
            {
                int a = Math.Abs((int)buffer[x]);
                if (a > m)
                {
                    m = a;
                }
            }

            return m + 1;
        }

        private int GetBufferMaxValue(int m, int[] buffer, SKRect drawRect, double panX1, double panX2)
        {
            int N2 = buffer.Length;
            double logN = Math.Log10(N2);
            double logScale = logN / N2;

            // Logarithmischen Anzeigewert in linearen Bereich umrechnen.
            double k1 = (Math.Min(0, panX1) - (drawRect.Left + 1)) * N2 / drawRect.Width;
            double k2 = (Math.Min(panX2, drawRect.Width) - (drawRect.Left + 1)) * N2 / drawRect.Width;
            int logIndex1 = (int)(Math.Pow(10, k1 * logScale) - 0.5);
            int logIndex2 = (int)(Math.Pow(10, k2 * logScale) - 0.5) + 1;

            for (int x = 0; x < N2; x++)
            {
                if (x >= logIndex1 && x < logIndex2)
                {
                    int a = Math.Abs(buffer[x]);
                    if (a > m)
                    {
                        m = a;
                    }
                }
            }

            return m + 1;
        }
    }
}
