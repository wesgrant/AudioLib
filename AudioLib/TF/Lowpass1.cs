﻿using AudioLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioLib.TF
{
	public sealed class Lowpass1 : TransferVariable
	{
		public const int P_FREQ = 0;

		public Lowpass1(float fs) : base(fs, 1)
		{ }

		public override void Update()
		{
			double[] b = new double[2];
			double[] a = new double[2];

			// PRevent going over the Nyquist frequency
			if(parameters[P_FREQ] >= fs * 0.5)
				parameters[P_FREQ] = fs * 0.499;

			// Compensate for frequency in bilinear transform
			float f = (float)(2.0 * fs * (Math.Tan((parameters[P_FREQ] * 2 * Math.PI) / (fs * 2))));
			if (f == 0) f = 0.0001f; // prevent divByZero exception
		
			b[0] = f;
			b[1] = f;
		
			a[0] = f+2*fs;
			a[1] = f-2*fs;

			var aInv = 1 / a[0];

			// normalize
			b[0] = b[0] * aInv;
			b[1] = b[1] * aInv;

			a[1] = a[1] * aInv;
			a[0] = 1;

			this.B = b;
			this.A = a;
		}
	}
}
