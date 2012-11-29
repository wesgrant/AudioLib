﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioLib.Modules
{
	/// <summary>
	/// Biquadratic filter with multiple modes of operation
	/// For reference, see: 
	/// http://www.ti.com/lit/an/slaa447/slaa447.pdf
	/// http://www.musicdsp.org/files/Audio-EQ-Cookbook.txt
	/// http://www.musicdsp.org/files/biquad.c
	/// </summary>
	public class Biquad
	{
		public enum FilterType
		{
			LowPass = 0,
			HighPass, 
			BandPass, 
			Notch, 
			Peak, 
			LowShelf, 
			HighShelf
		};

		double _gainDb;
		double _q;
		double a0, a1, a2, b0, b1, b2;
		double x1, x2, y, y1, y2;
		double gain;

		public FilterType Type;

		/// <summary>
		/// Cutoff / Center Frequency
		/// </summary>
		public double Frequency;

		public double Samplerate;

		public double Slope;

		/// <summary>
		/// Gain for shelfing and peak filter
		/// </summary>
		public double GainDB
		{
			get { return _gainDb; }
			set
			{
				_gainDb = value;
				gain = Math.Pow(10, value / 40);
			}
		}

		/// <summary>
		/// Q-factor
		/// </summary>
		public double Q
		{
			get { return _q; }
			set
			{
				if (value == 0)
					value = 1e-12;
				_q = value;
			}
		}

		public double[] A { get { return new double[] { 1, a1, a2 }; } }

		public double[] B { get { return new double[] { b0, b1, b2 }; } }

		public Biquad()
		{

		}

		public Biquad(FilterType filterType, double samplerate)
		{
			Type = filterType;
			Samplerate = samplerate;

			GainDB = 0.0;
			Frequency = 0.5;
			Q = 0.5;
		}


		public void Update()
		{
			double omega = 2 * Math.PI * Frequency / Samplerate;
			double sinOmega = Math.Sin(omega);
			double cosOmega = Math.Cos(omega);

			double sqrtGain = 0.0;
			double alpha = 0.0;

			if (Type == FilterType.LowShelf || Type == FilterType.HighShelf)
			{
				alpha = sinOmega / 2 * Math.Sqrt((gain + 1 / gain) * (1 / Slope - 1) + 2);
				sqrtGain = Math.Sqrt(gain);
			}
			else
			{
				alpha = sinOmega / (2 * Q);
			}

			//double beta = Math.Sqrt(gain + gain);

			switch (Type)
			{
				case FilterType.LowPass:
					b0 = (1 - cosOmega) / 2;
					b1 = 1 - cosOmega;
					b2 = (1 - cosOmega) / 2;
					a0 = 1 + alpha;
					a1 = -2 * cosOmega;
					a2 = 1 - alpha;
					break;
				case FilterType.HighPass:
					b0 = (1 + cosOmega) / 2;
					b1 = -(1 + cosOmega);
					b2 = (1 + cosOmega) / 2;
					a0 = 1 + alpha;
					a1 = -2 * cosOmega;
					a2 = 1 - alpha;
					break;
				case FilterType.BandPass:
					b0 = alpha;
					b1 = 0;
					b2 = -alpha;
					a0 = 1 + alpha;
					a1 = -2 * cosOmega;
					a2 = 1 - alpha;
					break;
				case FilterType.Notch:
					b0 = 1;
					b1 = -2 * cosOmega;
					b2 = 1;
					a0 = 1 + alpha;
					a1 = -2 * cosOmega;
					a2 = 1 - alpha;
					break;
				case FilterType.Peak:
					b0 = 1 + (alpha * gain);
					b1 = -2 * cosOmega;
					b2 = 1 - (alpha * gain);
					a0 = 1 + (alpha / gain);
					a1 = -2 * cosOmega;
					a2 = 1 - (alpha / gain);
					break;
				case FilterType.LowShelf:
					b0 = gain * ((gain + 1) - (gain - 1) * cosOmega + 2 * Math.Sqrt(gain) * alpha);
					b1 = 2 * gain * ((gain - 1) - (gain + 1) * cosOmega);
					b2 = gain * ((gain + 1) - (gain - 1) * cosOmega - 2 * Math.Sqrt(gain) * alpha);
					a0 = (gain + 1) + (gain - 1) * cosOmega + 2 * Math.Sqrt(gain) * alpha;
					a1 = -2 * ((gain - 1) + (gain + 1) * cosOmega);
					a2 = (gain + 1) + (gain - 1) * cosOmega - 2 * Math.Sqrt(gain) * alpha;
					break;
				case FilterType.HighShelf:
					b0 = gain * ((gain + 1) + (gain - 1) * cosOmega + 2 * sqrtGain * alpha);
					b1 = -2 * gain * ((gain - 1) + (gain + 1) * cosOmega);
					b2 = gain * ((gain + 1) + (gain - 1) * cosOmega - 2 * sqrtGain * alpha);
					a0 = (gain + 1) - (gain - 1) * cosOmega + 2 * sqrtGain * alpha;
					a1 = 2 * ((gain - 1) - (gain + 1) * cosOmega);
					a2 = (gain + 1) - (gain - 1) * cosOmega - 2 * sqrtGain * alpha;
					break;
				/**
			case FilterType.LowShelf:
				b0 = gain     * ((gain + 1) - (gain - 1) * cosOmega + beta * sinOmega);
				b1 = 2 * gain * ((gain - 1) - (gain + 1) * cosOmega                  );
				b2 = gain     * ((gain + 1) - (gain - 1) * cosOmega - beta * sinOmega);
				a0 =            ((gain + 1) + (gain - 1) * cosOmega + beta * sinOmega);
				a1 = -2       * ((gain - 1) + (gain + 1) * cosOmega                  );
				a2 =            ((gain + 1) + (gain - 1) * cosOmega - beta * sinOmega);
				break;
			case FilterType.HighShelf:
				b0 = gain      * ((gain + 1) + (gain - 1) * cosOmega + beta * sinOmega);
				b1 = -2 * gain * ((gain - 1) + (gain + 1) * cosOmega                  );
				b2 = gain      * ((gain + 1) + (gain - 1) * cosOmega - beta * sinOmega);
				a0 =             ((gain + 1) - (gain - 1) * cosOmega + beta * sinOmega);
				a1 = 2         * ((gain - 1) - (gain + 1) * cosOmega                  );
				a2 =             ((gain + 1) - (gain - 1) * cosOmega - beta * sinOmega);
				break;
				 */
			}

			double g = 1 / a0;

			b0 = b0 * g;
			b1 = b1 * g;
			b2 = b2 * g;
			a1 = a1 * g;
			a2 = a2 * g;
		}

		/// <summary>
		/// Calculate the frequency response for a given frequency
		/// </summary>
		/// <param name="freq"></param>
		/// <returns></returns>
		public double GetResponse(double freq)
		{
			double phi = Math.Pow((Math.Sin(2 * Math.PI * freq / (2.0 * Samplerate))), 2);
			return (Math.Pow(b0 + b1 + b2, 2.0) - 4.0 * (b0 * b1 + 4.0 * b0 * b2 + b1 * b2) * phi + 16.0 * b0 * b2 * phi * phi) / (Math.Pow(1.0 + a1 + a2, 2.0) - 4.0 * (a1 + 4.0 * a2 + a1 * a2) * phi + 16.0 * a2 * phi * phi);
		}

		public double Process(double x)
		{
			y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
			x2 = x1;
			y2 = y1;
			x1 = x;
			y1 = y;

			Output = y;
			return Output;
		}

		public double Output;
	}
}