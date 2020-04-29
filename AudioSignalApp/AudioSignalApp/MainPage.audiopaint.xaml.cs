// <copyright file="MainPage.audiopaint.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using SkiaSharp;
    using SkiaSharp.Views.Forms;
    using Xamarin.Forms;

    /// <summary>
    /// MainPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ContentPage" />
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Paint handler for audio signal.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SKPaintGLSurfaceEventArgs"/> instance containing the event data.</param>
        private void AudioSignalPaint(object sender, SKPaintGLSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            if (!this.validData)
            {
                return;
            }

            canvas.Clear(new SKColor(0xff238CB9));

            short[] audioBufferCopy;
            lock (this.audioLock)
            {
                if (this.audioBuffer != null)
                {
                    audioBufferCopy = (short[])this.audioBuffer.Clone();
                }
                else
                {
                    return;
                }
            }

            SKRect drawRect = canvas.LocalClipBounds;
            canvas.DrawText($"{(int)((1000 * this.audioBuffer.Length / (float)SampleRateInHz) + 0.5)} ms", 10, 100, this.audioText);

            int n = 20;
            int incX = (int)(drawRect.Width / n);
            for (float x = drawRect.Left + 1; x < drawRect.Right; x += incX)
            {
                canvas.DrawLine(x, drawRect.Top, x, drawRect.Bottom, this.audioLines);
            }

            int incY = (int)(drawRect.Height / n);
            for (float y = drawRect.MidY + incY; y < drawRect.Bottom; y += incY)
            {
                canvas.DrawLine(drawRect.Left, y, drawRect.Right, y, this.audioLines);
            }

            for (float y = drawRect.MidY - incY; y > drawRect.Top; y -= incY)
            {
                canvas.DrawLine(drawRect.Left, y, drawRect.Right, y, this.audioLines);
            }

            int m = this.GetBufferMaxValue(1600, audioBufferCopy);

            float yh = 3 * drawRect.MidY / 4;
            float y0 = drawRect.MidY;

            // Mittellinie
            canvas.DrawLine(drawRect.Left, y0, drawRect.Right, y0, this.audio);

            for (int x = 0; x < audioBufferCopy.Length; x++)
            {
                short a = audioBufferCopy[x];
                float y1 = y0 + (a * yh / m);
                float x0 = drawRect.Left + 1 + (x * drawRect.Width / audioBufferCopy.Length);
                canvas.DrawLine(x0, y0, x0, y1, this.audio);
            }
        }
    }
}
