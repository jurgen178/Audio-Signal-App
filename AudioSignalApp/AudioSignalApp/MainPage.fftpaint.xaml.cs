// <copyright file="MainPage.fftpaint.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SkiaSharp;
    using SkiaSharp.Views.Forms;
    using Xamarin.Forms;

    /// <summary>
    /// MainPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ContentPage" />
    public partial class MainPage : ContentPage
    {
        private int freqMax = 0;

        private double panStartX1 = 0;
        private double panX1 = 0;
        private double panStartX2 = 0;
        private double panX2 = double.MaxValue;
        private bool leftPan = true;

        /// <summary>
        /// FFTs signal pan.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PanUpdatedEventArgs"/> instance containing the event data.</param>
        private void fftSignalPan(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    break;

                case GestureStatus.Running:
                    if (this.leftPan)
                    {
                        double x = this.panStartX1 + (2 * e.TotalX);
                        if (x < this.panX2)
                        {
                            this.panX1 = x;
                        }
                    }
                    else
                    {
                        double x = this.panStartX2 + (2 * e.TotalX);
                        if (x > this.panX1)
                        {
                            this.panX2 = x;
                        }
                    }

                    break;

                case GestureStatus.Completed:
                    break;
            }
        }

        /// <summary>
        /// FFT signal tap.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fftSignalTap(object sender, EventArgs e)
        {
            this.leftPan = true;
            this.panX1 = 0;
            this.panX2 = (sender as SKCanvasView).CanvasSize.Width;
        }

        /// <summary>
        /// FFT signal touch.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SKTouchEventArgs"/> instance containing the event data.</param>
        private void fftSignalTouch(SKCanvasView sender, SKTouchEventArgs e)
        {
            double width = sender.CanvasSize.Width;
            double x = e.Location.X;

            this.leftPan = x < this.panX1 || x < width / 2;

            if (this.leftPan)
            {
                this.panX1 = this.panStartX1 = e.Location.X;
            }
            else
            {
                this.panX2 = this.panStartX2 = e.Location.X;
            }
        }

        /// <summary>
        /// Multiline property.
        /// </summary>
        public class Line
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            /// <value>
            /// The width.
            /// </value>
            public float Width { get; set; }
        }

        private float DrawText(SKCanvas canvas, string text, SKRect area, SKPaint paint)
        {
            float lineHeight = paint.TextSize * 1.2f;
            var lines = this.SplitLines(text, paint, area.Width);

            float y = area.Top;
            float x = 6;

            foreach (var line in lines)
            {
                y += lineHeight;
                canvas.DrawText(line.Value, x, y, paint);
            }

            // Center
            // var y = area.MidY - height / 2;

            // foreach (var line in lines)
            // {
            //    y += lineHeight;
            //    var x = area.MidX - line.Width / 2;
            //    canvas.DrawText(line.Value, x, y, paint);
            // }
            float height = lines.Length * lineHeight;
            return height;
        }

        private Line[] SplitLines(string text, SKPaint paint, float maxWidth)
        {
            var spaceWidth = paint.MeasureText(" ");
            var lines = text.Split('\n');

            return lines.SelectMany((line) =>
            {
                var result = new List<Line>();

                var words = line.Split(new[] { " " }, StringSplitOptions.None);

                var lineResult = new StringBuilder();
                float width = 0;
                foreach (var word in words)
                {
                    var wordWidth = paint.MeasureText(word);
                    var wordWithSpaceWidth = wordWidth + spaceWidth;
                    var wordWithSpace = word + " ";

                    if (width + wordWidth > maxWidth)
                    {
                        result.Add(new Line() { Value = lineResult.ToString(), Width = width });
                        lineResult = new StringBuilder(wordWithSpace);
                        width = wordWithSpaceWidth;
                    }
                    else
                    {
                        lineResult.Append(wordWithSpace);
                        width += wordWithSpaceWidth;
                    }
                }

                result.Add(new Line() { Value = lineResult.ToString(), Width = width });

                return result.ToArray();
            }).ToArray();
        }

        /// <summary>
        /// FFT signal paint.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SKPaintGLSurfaceEventArgs"/> instance containing the event data.</param>
        private void fftSignalPaint(object sender, SKPaintGLSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            SKRect drawRect = canvas.LocalClipBounds;

            canvas.Clear(this.GetThemeColor("fftSignalBackgroundColor"));

            if (!this.validData)
            {
                this.fehlerText.Color = this.GetThemeColor("fehlerTextColor");

                float y = this.fehlerText.TextSize;
                canvas.DrawText($"Audiofehler.", 10, y, this.fehlerText);

                y += this.fehlerText.TextSize;
                canvas.DrawText($"Abtastfrequenz fₛ = {SampleRateInHz} Hz", 10, y, this.fehlerText);

                if (!string.IsNullOrEmpty(this.errorMsg))
                {
                    y += this.fehlerText.TextSize;
                    SKRect drawRectErrorMsg = drawRect;
                    drawRectErrorMsg.Top = y;

                    y += this.DrawText(canvas, this.errorMsg, drawRectErrorMsg, this.fehlerText);
                }

                return;
            }

            if (this.fftBuffer == null)
            {
                return;
            }

            this.fftSignalLines.Color = this.GetThemeColor("fftSignalLinesColor");
            this.fftSignal.Color = this.GetThemeColor("fftSignalColor");
            this.freqMaxRect.Color = this.GetThemeColor("freqMaxColor");
            this.fftFreqText.Color = this.GetThemeColor("fftFreqTextColor");
            this.filterRect.Color = this.GetThemeColor("fftFilterRectColor");
            this.freqMaxRect.BlendMode = (string)App.Current.Resources["Theme"] == "Dark" ? SKBlendMode.Darken : this.freqMaxRect.BlendMode = SKBlendMode.Lighten;

            int[] fftBufferCopy;
            lock (this.fftLock)
            {
                fftBufferCopy = (int[])this.fftBuffer.Clone();
            }

            int N2 = fftBufferCopy.Length;

            double logN = Math.Log10(N2);
            double logScale = logN / N2;

            // Filter
            float panX1f = (float)this.panX1;
            float panX2f = (float)this.panX2;
            canvas.DrawRect(drawRect.Left, drawRect.Top, panX1f, drawRect.Height, this.filterRect);
            canvas.DrawRect((float)this.panX2, drawRect.Top, drawRect.Right - panX2f, drawRect.Height, this.filterRect);

            void SetPanFrequenzLabel(float x)
            {
                SKPath path = new SKPath();
                path.MoveTo(x, drawRect.MidY);
                path.LineTo(x, drawRect.MidY - 200);

                double f = Math.Pow(10, (x * N2 * logScale / drawRect.Width) - 1);
                int freqInHz = (int)(f * SampleRateInHz * 10 / 2 / (float)N2);

                canvas.DrawTextOnPath($"{freqInHz} Hz", path, 0, 0, this.fftFreqText);
            }

            if (panX1f - 1 > drawRect.Left)
            {
                SetPanFrequenzLabel(panX1f);
            }

            if (panX2f + 1 < drawRect.Right)
            {
                SetPanFrequenzLabel(panX2f);
            }

            int n = 20;
            int incY = (int)(drawRect.Height / n);
            for (float y = drawRect.Top + 1; y < drawRect.Bottom; y += incY)
            {
                canvas.DrawLine(drawRect.Left, y, drawRect.Right, y, this.fftSignalLines);
            }

            int m = this.GetBufferMaxValue(10, fftBufferCopy);

            float yh = drawRect.Height - 2;
            float y0 = drawRect.Bottom - 1;

            // Debug
            // canvas.DrawText($"{(int)panX1}", drawRect.MidX, drawRect.MidY, this.debug);

            // Log grid lines.
            int l = 10;
            for (int l10 = 0; l10 < (int)(logN + 1); l10++)
            {
                for (int l1 = 0; l1 < 10; l1++)
                {
                    int f = l * l1;
                    float f1 = (float)f * 2 * N2 / SampleRateInHz;
                    float ll1 = f == 0 ? 0 : (float)(Math.Log10(f1) / logScale);
                    float x0 = drawRect.Left + 1 + (ll1 * drawRect.Width / N2);

                    canvas.DrawLine(x0, drawRect.Top, x0, drawRect.Bottom, this.fftSignalLines);

                    SKPath path = new SKPath();
                    path.MoveTo(x0, y0 - 100);
                    path.LineTo(x0, y0 - 500);

                    canvas.DrawTextOnPath($"{f} Hz", path, 0, 0, this.fftFreqText);
                }

                l *= 10;
            }

            float y2 = y0;
            float x2 = drawRect.Left + 1;

            // FFT signal.
            int maxValue = -1;
            float freqMaxX = -1;
            for (int k = 0; k < N2; k++)
            {
                float x0 = drawRect.Left + 1 + (k * drawRect.Width / N2);

                int logIndex;
                int fftWert;
                if (x0 > this.panX1 && x0 < this.panX2)
                {
                    // Linearen Bereich 0..N2 auf log Darstellung.
                    logIndex = (int)Math.Pow(10, k * logScale);
                    logIndex = logIndex >= 0 ? (logIndex < N2 ? logIndex : (N2 - 1)) : 0;
                    fftWert = fftBufferCopy[logIndex];
                }
                else
                {
                    logIndex = 0;
                    fftWert = 0;
                }

                float y1 = y0 - (fftWert * yh / m);
                canvas.DrawLine(x2, y2, x0, y1, this.fftSignal);
                y2 = y1;

                if (fftWert > maxValue)
                {
                    maxValue = fftWert;
                    freqMaxX = x0;
                    this.freqMax = (int)(logIndex * SampleRateInHz / 2 / (float)N2);
                }

                x2 = x0;
            }

            // Draw max freq rect.
            if (freqMaxX > 0)
            {
                float rectWidth = 40;
                canvas.DrawRect(freqMaxX - (rectWidth / 2), drawRect.Top, rectWidth, drawRect.Height, this.freqMaxRect);
            }

            void SetText(string text, float textY)
            {
                float textLen = this.zaehler.MeasureText(text);
                float textX = textLen > drawRect.MidX ? textLen : drawRect.MidX;
                canvas.DrawText(text, textX, textY, this.zaehler);
            }

            SetText($"{this.freqMax} Hz", drawRect.Top + this.zaehler.TextSize);
            string uminStr = $"{this.freqMax * UminFaktor} U/min";
            int uminStrLen = uminStr.Length;
            int refUminStrLen = "12345 U/min".Length;

            if (uminStrLen < refUminStrLen)
            {
                uminStr = uminStr.PadLeft(refUminStrLen);
            }

            if (IsUminVisible)
            {
                this.UminText.Text = uminStr;
            }
            else
            {
                SetText(uminStr, drawRect.Top + (2 * this.zaehler.TextSize));
            }
        }
    }
}
