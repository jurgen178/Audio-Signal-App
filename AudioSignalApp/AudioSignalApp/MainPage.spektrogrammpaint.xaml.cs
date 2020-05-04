// <copyright file="MainPage.spektrogrammpaint.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using System;
    using SkiaSharp;
    using SkiaSharp.Views.Forms;
    using Xamarin.Forms;

    /// <summary>
    /// MainPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ContentPage" />
    public partial class MainPage : ContentPage
    {
        private static SKBitmap spektrogrammBitmap = null;

        private int scroll = 0;

        private double width = -1;
        private double height = -1;

        /// <summary>
        /// Spektrogramm signal tap.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void spektrogrammSignalTap(object sender, EventArgs e)
        {
            spektrogrammBitmap = null;
        }

        /// <summary>
        /// Spektrogramm signal paint.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SKPaintGLSurfaceEventArgs"/> instance containing the event data.</param>
        private void spektrogrammSignalPaint(object sender, SKPaintGLSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            SKRect drawRect = canvas.LocalClipBounds;

            if (!this.validData || this.fftBuffer == null)
            {
                return;
            }

            int[] fftBufferCopy;
            lock (this.fftLock)
            {
                fftBufferCopy = (int[])this.fftBuffer.Clone();
            }

            int m = this.GetBufferMaxValue(10, fftBufferCopy, drawRect, this.panX1, this.panX2);

            if (spektrogrammBitmap == null)
            {
                this.scroll = 0;

                spektrogrammBitmap = new SKBitmap((int)drawRect.Width, (int)drawRect.Height);
                unsafe
                {
                    uint* ptr = (uint*)spektrogrammBitmap.GetPixels().ToPointer();
                    uint backgroundColor = (uint)this.GetThemeColor("spektrogrammBackgroundColor");

                    for (int x = 0; x < spektrogrammBitmap.Width; x++)
                    {
                        for (int y = 0; y < spektrogrammBitmap.Height; y++)
                        {
                            *ptr++ = backgroundColor;
                        }
                    }
                }
            }

            if (MainPage.RecordEnabled)
            {
                IntPtr pixelsAddr = spektrogrammBitmap.GetPixels();

                int N2 = fftBufferCopy.Length;
                double logN = Math.Log10(N2);
                double logScale = logN / N2;

                unsafe
                {
                    uint* ptr = (uint*)pixelsAddr.ToPointer();
                    int x = spektrogrammBitmap.Width - ++this.scroll;
                    ptr += x;
                    if (this.scroll >= spektrogrammBitmap.Width)
                    {
                        this.scroll = 0;
                    }

                    for (int y = 0; y < spektrogrammBitmap.Height; y++)
                    {
                        int k = y * N2 / spektrogrammBitmap.Height;

                        // Hohe Frequenzen sind oben.
                        k = N2 - 1 - k;
                        float x0 = k * drawRect.Width / (float)N2;

                        int n = 20 * N2 / 100; // Untere 20% der fft nicht anzeigen.
                        k = (k * (N2 - n) / N2) + n;

                        int logIndex = 0;
                        int fftWert = 0;

                        if (x0 >= this.panX1 && x0 <= this.panX2)
                        {
                            logIndex = (int)(Math.Pow(10, k * logScale) - 0.5);
                            fftWert = fftBufferCopy[logIndex >= 0 ? (logIndex < N2 ? logIndex : (N2 - 1)) : 0];
                        }
                        else
                        {
                            logIndex = 0;
                            fftWert = 0;
                        }

                        *ptr = (uint)SKColor.FromHsl(fftWert * 360 / m, 75, 50);

                        // Cursorline.
                        if (x >= 2)
                        {
                            uint* ptrCursor = ptr;
                            *--ptrCursor = 0xff000000;  // Schwarz
                            *--ptrCursor = 0xffffffff;  // Weiß
                        }

                        ptr += spektrogrammBitmap.Width;
                    }
                }
            }

            canvas.DrawBitmap(spektrogrammBitmap, 0, 0);
        }
    }
}
