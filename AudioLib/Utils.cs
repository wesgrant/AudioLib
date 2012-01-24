﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioLib
{
	public class Utils
	{
		public static double ExpResponse(double input)
		{
			return (double)(Math.Pow(20, input)-1)/19;
		}

		public static double LogResponse(double input)
		{
			return 2f*input-(double)(Math.Pow(20, input)-1)/19;

		}

		public static double DB2gain(double input)
		{
			return (double)Math.Pow(10, input/20);
		}

		public static double Gain2DB(double input)
		{
			return (double)(20*Math.Log10(input));
		}

		public static double[] Linspace(double min, double max, int num)
		{
			double space = (max-min)/(num-1);
			double[] output = new double[num];
			output[0] = min;
			double runningVal = min;
			for(int i=1; i<num; i++)
			{
				runningVal = runningVal + space;
				output[i] =  (double)runningVal;
			}

			return output;
		}

		public static double Min(double[] input)
		{
			double min = input[0];
			for(int i=1; i < input.Length; i++)
				if(input[i] < min)
					min = input[i];
		
			return min;
		}

		public static double Max(double[] input)
		{
			double max = input[0];
			for(int i=1; i < input.Length; i++)
				if(input[i] > max)
					max = input[i];

			return max;
		}

		public static double Average(double[] input)
		{
			double ave = 0;
			for(int i=0; i < input.Length; i++)
				ave += input[i];

			return ave / (double)input.Length;
		}
	
		public static double RMS(double[] input)
		{
			double rms = 0;
			for(int i=0; i < input.Length; i++)
				rms += input[i] * input[i];

			return (double)Math.Sqrt(rms /(double)input.Length);
		}

		public static double[] Gain(double[] input, double gain)
		{
			double[] output = new double[input.Length];

			for (int i = 0; i < input.Length; i++)
				output[i] = input[i] * gain;

			return output;
		}

		public static void GainInPlace(double[] input, double gain)
		{
			for(int i=0; i < input.Length; i++)
				input[i] = input[i] * gain;
		}

		public static double[] Saturate(double[] input, double max)
		{
			return Utils.Saturate(input, -max, max);
		}

		public static double[] Saturate(double[] input, double min, double max)
		{
			double[] output = new double[input.Length];

			for(int i=0; i<input.Length; i++)
			{
				if(input[i] < min)
					output[i] = min;
				else if (input[i] > max)
					output[i] = max;
				else
					output[i] = input[i];
			}

			return output;
		}

		public static void SaturateInPlace(double[] input, double max)
		{
			Utils.SaturateInPlace(input, -max, max);
		}

		public static void SaturateInPlace(double[] input, double min, double max)
		{
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] < min)
					input[i] = min;
				else if (input[i] > max)
					input[i] = max;
			}
		}

		/// <summary>
		/// Create a Sinc wave
		/// </summary>
		/// <param name="omega">Range between 0...1 (0.5 = Nyquist, fs/2)</param>
		/// <param name="M">size of output is 2M+1</param>
		/// <returns></returns>
		public static double[] Sinc(double omega, int M)
		{
			double[] x = Linspace(-M, M, 2 * M + 1);
			double[] output = new double[2 * M + 1];

			for (int i = 0; i < M; i++)
			{
				output[i] = (double)(Math.Sin(Math.PI * 2.0 * x[i] * omega) / (Math.PI * 2.0 * x[i] * omega));
				output[2 * M - i] = output[i];
			}

			output[M] = 1;

			return output;
		}

		/// <summary>
		/// Create a window
		/// </summary>
		/// <param name="M">M length of output window N = 2M+1</param>
		/// <param name="type">type can be Bartlett, Hann, Hamming or Blackman</param>
		/// <returns></returns>
		public static double[] Window(int M, String type)
		{
			double[] n = Linspace(-M, M, 2 * M + 1);
			double[] output = new double[2 * M + 1];

			if (type.ToLower().Equals("bartlett"))
			{
				for (int i = 0; i < output.Length; i++)
					output[i] = (double)((n[i] > 0) ? (1 - n[i] / (M + 1)) : (1 + n[i] / (M + 1)));
			}
			else if (type.ToLower().Equals("hann"))
			{
				for (int i = 0; i < output.Length; i++)
					output[i] = (double)(0.5 * (1 + Math.Cos(2 * Math.PI * n[i] / (2 * M + 1))));
			}
			else if (type.ToLower().Equals("hamming"))
			{
				for (int i = 0; i < output.Length; i++)
					output[i] = (double)(0.54 + 0.46 * Math.Cos(2 * Math.PI * n[i] / (2 * M + 1)));
			}
			else if (type.ToLower().Equals("blackman"))
			{
				for (int i = 0; i < output.Length; i++)
					output[i] = (double)(0.42 + 0.5 * Math.Cos(2 * Math.PI * n[i] / (2 * M + 1)) + 0.08 * Math.Cos(4 * Math.PI * n[i] / (2 * M + 1)));
			}
			else // Rect window
			{
				for (int i = 0; i < output.Length; i++)
					output[i] = 1;
			}

			return output;
		}

		public static double[] SincFilter(double wc, int M, String type)
		{
			double[] a = Sinc(wc, M);
			double[] b = Window(M, type);

			for (int i = 0; i < b.Length; i++)
				b[i] = a[i] * b[i];

			var sum = b.Sum();

			for (int i = 0; i < b.Length; i++)
				b[i] = b[i] / sum;

			return b;
		}

		

		/// <summary>
		/// Creates an array containing a sine wave
		/// </summary>
		/// <param name="len">number of samples in the output array</param>
		/// <param name="phase">The starting phase of the signal</param>
		/// <param name="rad">The normalized frequency, radians per sample. Values 0...pi (other values are valid but are aliased)</param>
		/// <param name="mag">Magnitude, V_peak</param>
		/// <returns></returns>
		public static double[] Sinewave(int len, double phase, double rad, double mag)
		{
			double[] output = new double[len];

			for(int i=0; i<output.Length; i++)
			{
				output[i] = (double)Math.Sin(phase + i*rad) * mag;
			}

			return output;
		}

		public static double[] Saw(int len, double mag)
		{
			double unit = 2 * mag / (len-1);
			double[] output = new double[len];
			for (int i = 0; i < len; i++)
				output[i] = (mag - i * unit);

			return output;
		}

		public static double[] SawAdditive(int len, int partials, double mag)
		{
			mag = (double)(2 / Math.PI * mag);
			double[] output = new double[len];

			for (int i = 1; i < partials; i++)
			{
				for (int j = 0; j < len; j++)
				{
					output[j] = output[j] + (double)Math.Sin(i / (double)len * 2.0 * Math.PI * j) / i;
				}
			}

			// Set magnitude
			for (int j = 0; j < len; j++)
			{
				output[j] = output[j] * mag;
			}

			return output;
		}

		public static double[] Square(int len, double mag, double width)
		{
			int highSample = (int)(width * len + 0.5f);
			int startSample = (len - highSample)/2;

			double[] output = new double[len];
			for (int i = 0; i < len; i++)
			{
				if (i >= startSample && i < (startSample + highSample))
					output[i] = mag;
				else
					output[i] = -mag;
			}

			return output;
		}

		public static double[] SquareAdditive(int len, int partials, double mag)
		{
			mag = (double)(4 / Math.PI * mag);
			double[] output = new double[len];

			for (int i = 1; i < partials; i=i+2)
			{
				for (int j = 0; j < len; j++)
				{
					output[j] = output[j] + (double)Math.Sin(i / (double)len * 2.0 * Math.PI * j) / i;
				}
			}

			// Set magnitude
			for (int j = 0; j < len; j++)
			{
				output[j] = output[j] * mag;
			}

			return output;
		}

		public static double[] Triangle(int len, double mag)
		{
			int half = len / 2 + 1;

			double[] output = new double[len];

			for (int i = 0; i < half; i++)
				output[i] = mag - 2 * mag * i / (half-1);

			for (int i = half; i < len; i++)
				output[i] = output[2*(half-1) - i];

			return output;
		}

		public static double[] TriangleAdditive(int len, int partials, double mag)
		{
			mag = (double)(4 / Math.PI * mag);
			double[] output = new double[len];

			double p = 1.0f;

			for (int i = 1; i < partials; i = i + 2)
			{
				for (int j = 0; j < len; j++)
				{
					output[j] = output[j] + (double)Math.Sin(i / (double)len * 2.0 * Math.PI * j) * (double)(8.0 / (i*i*Math.PI*Math.PI) * p);
				}

				p = -1.0f * p; // invert signal between coefficients
			}

			return output;
		}

		
		
	}
}
