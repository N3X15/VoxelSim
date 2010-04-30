/*
 *  Procedurality v. 0.1 rev. 20070307
 *  Copyright 2007 Oddlabs ApS
 *
 *
 *  This file is part of Procedurality.
 *  Procedurality is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *
 *  Procedurality is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Foobar; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
 */
using System;

namespace Procedurality
{

	public class Tools {
		private Tools() {
		}
	
		public static float modulo(float x, float n) {
			return x - n*(float)Math.Floor(x/n);
		}
	
		public static int modulo(int x, int n) {
			return (int)(x - n*(float)Math.Floor((float)x/n));
		}
	
		public static float interpolateLinear(float v1, float v2, float fraction) {
			return v1*(1f - fraction) + v2*fraction;
		}
	
		public static float interpolateSmooth(float v1, float v2, float fraction) {
			if (fraction < 0.5f) {
				fraction = 2f*fraction*fraction;
			} else {
				fraction = 1f - 2f*(fraction - 1f)*(fraction - 1f);
			}
			return v1*(1f - fraction) + v2*fraction;
		}
	
		public static float interpolateSmooth2(float v1, float v2, float fraction) {
			float fraction2 = fraction*fraction;
			fraction = 3*fraction2 - 2f*fraction*fraction2;
			return v1*(1f - fraction) + v2*fraction;
		}
	
		public static float interpolateCubic(float v0, float v1, float v2, float v3, float fraction) {
			float p = (v3 - v2) - (v0 - v1);
			float q = (v0 - v1) - p;
			float r = v2 - v0;
			float fraction2 = fraction*fraction;
			return p*fraction*fraction2 + q*fraction2 + r*fraction + v1;
		}
	
		public static float rampLinear(float start, float end, float segment_length, float x) {
			x = modulo(x, segment_length);
			if (x < start) return 0f;
			if (x > end) return 1f;
			return interpolateLinear(0f, 1f, (x - start)/(end - start));
		}
	
		public static float step(float start, float end, float segment_length, float x) {
			x = modulo(x, segment_length);
			if (x < start || x > end) return 0f;
			return 1f;
		}
	
		public static float sawtooth(float x) {
			x++; // hack!
			float x_frac = x - (int)x;
			if (x_frac < 0.5) {
				return 2*x_frac;
			} else {
				return -2*x_frac + 2;
			}
		}
	
		public static float gaussify(float x) {
			if (x >=0 && x < 0.5) {
				return 0.5f*(float)Math.Sqrt(2*x);
			}
			if (x >=0.5f && x <= 1f) {
				return 1f - 0.5f*(float)Math.Sqrt(-2*x + 2);
			}
			return 0;
		}
	
		public static float gaussify(float x, float exponent) {
			if (x >=0 && x < 0.5) {
				return 0.5f*(float)Math.Pow(2*x, exponent);
			}
			if (x >=0.5f && x <= 1f) {
				return 1f - 0.5f*(float)Math.Pow(-2*x + 2, exponent);
			}
			return 0;
		}
	
		public static float gain(float gain, float x) {
			if (x < 0.5f)
				return (float)(Math.Pow(2 * x, Math.Log(1 - gain)/Math.Log(0.5d))/2f);
			else
				return 1f - (float)(Math.Pow(2 - 2 * x, Math.Log(1 - gain)/Math.Log(0.5d))/2f);
		}
	}
}
