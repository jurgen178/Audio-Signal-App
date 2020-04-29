// <copyright file="MainPage.dsp.xaml.cs" company="Audio Signal App">
// Copyright (c) Audio Signal App. All rights reserved.
// </copyright>

namespace AudioSignalApp
{
    using System;
    using Xamarin.Forms;

    /// <summary>
    /// MainPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.ContentPage" />
    public partial class MainPage : ContentPage
    {
        private int N = 0;
        private float[] c_real = null;
        private float[] c_imag = null;
        private float[] y_real = null;
        private float[] y_imag = null;

        /// <summary>
        /// Goertzel FFT.
        /// </summary>
        private void GoertzelFFT()
        {
            int audioBufferLen = 0;

            lock (this.audioLock)
            {
                if (this.audioBuffer != null)
                {
                    audioBufferLen = this.audioBuffer.Length;

                    // 2D-DFT
                    for (int j = 0; j < this.N; j++)
                    {
                        this.c_real[j] = this.audioBuffer[j];
                        this.c_imag[j] = 0;
                    }

                    // Diskrete Fourier-Transformation
                    // Goertzel-Algorithmus
                    //
                    // Wn = e^(2pi*j/N)
                    //
                    // y_k(n) = y_k(n-1)ùW   + f(n)    ;  y_k(n),W komplex
                    float y_re, y_im, re, im;

                    for (int k = 0; k < this.N; k++)
                    {
                        // alle y(k) berechnen
                        float w_re = (float)Math.Cos(2 * Math.PI * k / this.N);
                        float w_im = (float)Math.Sin(2 * Math.PI * k / this.N);
                        y_re = 0;
                        y_im = 0;

                        for (int n = 0; n < this.N; n++)
                        {
                            re = (y_re * w_re) - (y_im * w_im) + this.audioBuffer[n];
                            im = (y_im * w_re) + (y_re * w_im);
                            y_re = re;
                            y_im = im;
                        }

                        this.y_real[k] = ((y_re * w_re) - (y_im * w_im)) / this.N;   // damit insgesamt N+1 Mult.
                        this.y_imag[k] = ((y_im * w_re) + (y_re * w_im)) / this.N;   // -> f[N]=0
                    }
                }
            }

            if (audioBufferLen != 0)
            {
                lock (this.fftLock)
                {
                    this.fftBuffer = new int[audioBufferLen / 2];

                    if (Leistungsspektrum)
                    {
                        for (int k = 0; k < this.N / 2; k++)
                        {
                            // Leistungsspektrum
                            this.fftBuffer[k] = (int)((this.y_real[k] * this.y_real[k]) + (this.y_imag[k] * this.y_imag[k]));
                        }
                    }
                    else
                    {
                        for (int k = 0; k < this.N / 2; k++)
                        {
                            // Betragsspektrum
                            this.fftBuffer[k] = (int)Math.Sqrt((this.y_real[k] * this.y_real[k]) + (this.y_imag[k] * this.y_imag[k]));
                        }
                    }
                }
            }
        }
    }
}
