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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Procedurality
{
	public class Layer {
		private int width;
		private int height;
		public Channel r;
		public Channel g;
		public Channel b;
		public Channel a;
	
		public Layer(int width, int height) {
			this.width = width;
			this.height = height;
			Channel empty = new Channel(width, height);
			empty.fill(1f);
			this.r = empty;
			this.g = empty.copy();
			this.b = empty.copy();
			this.a = null;
		}
	
		public Layer(Channel r, Channel g, Channel b, Channel a) {
			this.width = r.getWidth();
			this.height = r.getHeight();
			if(!(g.getWidth() == width
				&& b.getWidth() == width
				&& g.getHeight() == height
				&& b.getHeight() == height
				&& (a == null || (a.getWidth() == width && a.getHeight() == height))))
				throw new Exception("trying to combine channels of differing sizes");
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
	
		public Layer(Channel r, Channel g, Channel b) {
			this.width = r.getWidth();
			this.height = r.getHeight();
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = null;
		}
	
		public Layer(Layer rgb, Channel a) {
			this.r = rgb.r;
			this.g = rgb.g;
			this.b = rgb.b;
			this.width = r.getWidth();
			this.height = r.getHeight();
			this.a = a;
		}
		
		public void saveAsPNG(String filename) {
			
			//Console.WriteLine("Creating bitmap, range ["+this.findMin().ToString()+","+this.findMax().ToString()+"]...");
			
			Bitmap bmp = new Bitmap(this.r.getHeight(), this.r.getWidth());

            const int pallete = 256;

            Color[] grays = new Color[pallete];
            for (int i = 0; i < grays.Length; i++)
            {
                grays[i] = Color.FromArgb(i, i, i);
            }

            for (int y = 0; y < this.getHeight(); y++)
            {
                for (int x = 0; x < this.getWidth(); x++)
                {
                    // 512 is the largest possible height before colours clamp
					/*
                    int colorindex = (int) (Math.Max(Math.Min(1.0, this.r.getPixel(x,y)), 0.0) * (pallete - 1));

                    // Handle error conditions
                    if (colorindex > pallete - 1 || colorindex < 0)
                        bmp.SetPixel(x, this.getHeight() - y - 1, Color.Red);
                    else
                        bmp.SetPixel(x, this.getHeight() - y - 1, grays[colorindex]);
                     */
					int ri = ((int)(r.getPixel(x, y)*255 + .5f)) & 0xff;
					int gi = ((int)(g.getPixel(x, y)*255 + .5f)) & 0xff;
					int bi = ((int)(b.getPixel(x, y)*255 + .5f)) & 0xff;
					int ai;
					if (a != null) {
						ai = ((int)(a.getPixel(x, y)*255 + .5f)) & 0xff;
					} else {
						ai = 255;
					}
					bmp.SetPixel(x,y,Color.FromArgb(ri,gi,bi,ai));
                }
            }
			Console.Write("Saving "+filename+"...");
			try {
				bmp.Save(filename,ImageFormat.Png);
			} catch (IOException e) {
				Console.WriteLine(string.Format(" Save failed.",filename));
				Console.Write(e.StackTrace);
			}
			Console.WriteLine(" DONE!");
		}
	
		public void addAlpha() {
			a = new Channel(width, height);
		}
		
		public void addAlpha(Channel alpha) {
			a = alpha;
		}
	
		public int getWidth() {
			return width;
		}
	
		public int getHeight() {
			return height;
		}
	
		public Channel getR() {
			return r;
		}
	
		public Channel getG() {
			return g;
		}
	
		public Channel getB() {
			return b;
		}
	
		public Channel getA() {
			return a;
		}
	
		public void putPixel(int x, int y, float r, float g, float b) {
			this.r.putPixel(x, y, r);
			this.g.putPixel(x, y, g);
			this.b.putPixel(x, y, b);
		}
	
		public void putPixel(int x, int y, float r, float g, float b, float a) {
			this.r.putPixel(x, y, r);
			this.g.putPixel(x, y, g);
			this.b.putPixel(x, y, b);
			if (this.a != null) {
				this.a.putPixel(x, y, a);
			}
		}
	
		public void putPixelWrap(int x, int y, float r, float g, float b) {
			this.r.putPixelWrap(x, y, r);
			this.g.putPixelWrap(x, y, g);
			this.b.putPixelWrap(x, y, b);
		}
	
		public void putPixelWrap(int x, int y, float r, float g, float b, float a) {
			this.r.putPixelWrap(x, y, r);
			this.g.putPixelWrap(x, y, g);
			this.b.putPixelWrap(x, y, b);
			if (this.a != null) {
				this.a.putPixelWrap(x, y, a);
			}
		}
	
		public void putPixelClip(int x, int y, float r, float g, float b) {
			this.r.putPixelClip(x, y, r);
			this.g.putPixelClip(x, y, g);
			this.b.putPixelClip(x, y, b);
		}
	
		public void putPixelClip(int x, int y, float r, float g, float b, float a) {
			this.r.putPixelClip(x, y, r);
			this.g.putPixelClip(x, y, g);
			this.b.putPixelClip(x, y, b);
			if (this.a != null) {
				this.a.putPixelClip(x, y, a);
			}
		}
	
		public void fill(float value) {
			r.fill(value);
			g.fill(value);
			b.fill(value);
		}
	
		public void fill(float r, float g, float b) {
			this.r.fill(r);
			this.g.fill(g);
			this.b.fill(b);
		}
	
		public void fill(float r, float g, float b, float a) {
			this.r.fill(r);
			this.g.fill(g);
			this.b.fill(b);
			if (this.a != null) {
				this.a.fill(a);
			}
		}
	
		public float findMin() {
			float min_r = r.findMin();
			float min_g = g.findMin();
			float min_b = b.findMin();
			return Math.Min(min_r, Math.Min(min_g, min_b));
		}
	
		public float findMax() {
			float max_r = r.findMax();
			float max_g = g.findMax();
			float max_b = b.findMax();
			return Math.Max(max_r, Math.Max(max_g, max_b));
		}
	
		public Layer copy() {
			if (a != null) {
				return new Layer(r.copy(), g.copy(), b.copy(), a.copy());
			} else {
				return new Layer(r.copy(), g.copy(), b.copy());
			}
		}
	
		public Layer normalize() {
			float min_r = r.findMin();
			float min_g = g.findMin();
			float min_b = b.findMin();
			float max_r = r.findMax();
			float max_g = g.findMax();
			float max_b = b.findMax();
			float min = Math.Min(min_r, Math.Min(min_g, min_b));
			float max = Math.Max(max_r, Math.Max(max_g, max_b));
			min_r = Tools.interpolateLinear(0, 1, (min_r - min)/(max - min));
			min_g = Tools.interpolateLinear(0, 1, (min_g - min)/(max - min));
			min_b = Tools.interpolateLinear(0, 1, (min_b - min)/(max - min));
			max_r = Tools.interpolateLinear(0, 1, (max_r - min)/(max - min));
			max_g = Tools.interpolateLinear(0, 1, (max_g - min)/(max - min));
			max_b = Tools.interpolateLinear(0, 1, (max_b - min)/(max - min));
			r.normalize(min_r, max_r);
			g.normalize(min_g, max_g);
			b.normalize(min_b, max_b);
			return this;
		}
	
		public Layer normalize(float new_min, float new_max) {
			float min_r = r.findMin();
			float min_g = g.findMin();
			float min_b = b.findMin();
			float max_r = r.findMax();
			float max_g = g.findMax();
			float max_b = b.findMax();
			float min = Math.Min(min_r, Math.Min(min_g, min_b));
			float max = Math.Max(max_r, Math.Max(max_g, max_b));
			min_r = Tools.interpolateLinear(new_min, new_max, (min_r - min)/(max - min));
			min_g = Tools.interpolateLinear(new_min, new_max, (min_g - min)/(max - min));
			min_b = Tools.interpolateLinear(new_min, new_max, (min_b - min)/(max - min));
			max_r = Tools.interpolateLinear(new_min, new_max, (max_r - min)/(max - min));
			max_g = Tools.interpolateLinear(new_min, new_max, (max_g - min)/(max - min));
			max_b = Tools.interpolateLinear(new_min, new_max, (max_b - min)/(max - min));
			r.normalize(min_r, max_r);
			g.normalize(min_g, max_g);
			b.normalize(min_b, max_b);
			return this;
		}
	
		public Layer normalize(float min, float max, float new_min, float new_max) {
			r.normalize(min, max, new_min, new_max);
			g.normalize(min, max, new_min, new_max);
			b.normalize(min, max, new_min, new_max);
			return this;
		}
	
		public Layer clip() {
			r.clip();
			g.clip();
			b.clip();
			return this;
		}
	
		public Layer crop(int x_lo, int y_lo, int x_hi, int y_hi) {
			r = r.crop(x_lo, y_lo, x_hi, y_hi);
			g = g.crop(x_lo, y_lo, x_hi, y_hi);
			b = b.crop(x_lo, y_lo, x_hi, y_hi);
			if (a != null) {
				a = a.crop(x_lo, y_lo, x_hi, y_hi);
			}
			width = r.getWidth();
			height = r.getHeight();
			return this;
		}
	
		public Layer cropWrap(int x_lo, int y_lo, int x_hi, int y_hi) {
			r = r.cropWrap(x_lo, y_lo, x_hi, y_hi);
			g = g.cropWrap(x_lo, y_lo, x_hi, y_hi);
			b = b.cropWrap(x_lo, y_lo, x_hi, y_hi);
			if (a != null) {
				a = a.cropWrap(x_lo, y_lo, x_hi, y_hi);
			}
			width = r.getWidth();
			height = r.getHeight();
			return this;
		}
	
		public Layer tile(int new_width, int new_height) {
			r = r.tile(new_width, new_height);
			g = g.tile(new_width, new_height);
			b = b.tile(new_width, new_height);
			if (a != null) {
				a = a.tile(new_width, new_height);
			}
			width = r.getWidth();
			height = r.getHeight();
			return this;
		}
	
		public Layer tileDouble() {
			r = r.tileDouble();
			g = g.tileDouble();
			b = b.tileDouble();
			if (a != null) {
				a = a.tileDouble();
			}
			width = width<<1;
			height = height<<1;
			return this;
		}
	
		public Layer offset(int x_offset, int y_offset) {
			r = r.offset(x_offset, y_offset);
			g = g.offset(x_offset, y_offset);
			b = b.offset(x_offset, y_offset);
			if (a != null) {
				a = a.offset(x_offset, y_offset);
			}
			return this;
		}
	
		public Layer brightness(float brightness) {
			r.brightness(brightness);
			g.brightness(brightness);
			b.brightness(brightness);
			return this;
		}
	
		public Layer brightness(float r, float g, float b) {
			this.r.brightness(r);
			this.g.brightness(g);
			this.b.brightness(b);
			return this;
		}
	
		public Layer multiply(float factor) {
			r.multiply(factor);
			g.multiply(factor);
			b.multiply(factor);
			return this;
		}
	
		public Layer multiply(float r, float g, float b) {
			this.r.multiply(r);
			this.g.multiply(g);
			this.b.multiply(b);
			return this;
		}
		
		public Layer multiply(float r, float g, float b, float a) {
			this.r.multiply(r);
			this.g.multiply(g);
			this.b.multiply(b);
			if (this.a != null) {
				this.a.multiply(a);
			}
			return this;
		}
	
		public Layer add(float add) {
			r.add(add);
			g.add(add);
			b.add(add);
			return this;
		}
	
		public Layer add(float r, float g, float b) {
			this.r.add(r);
			this.g.add(g);
			this.b.add(b);
			return this;
		}
		
		public Layer addClip(float r, float g, float b) {
			this.r.addClip(r);
			this.g.addClip(g);
			this.b.addClip(b);
			return this;
		}
		
		public Layer addClip(float r, float g, float b, float a) {
			this.r.addClip(r);
			this.g.addClip(g);
			this.b.addClip(b);
			if (this.a != null) {
				this.a.addClip(a);
			}
			return this;
		}
	
		public Layer contrast(float contrast) {
			r.contrast(contrast);
			g.contrast(contrast);
			b.contrast(contrast);
			return this;
		}
	
		public Layer contrast(float r, float g, float b) {
			this.r.contrast(r);
			this.g.contrast(g);
			this.b.contrast(b);
			return this;
		}
	
		public Layer gamma(float gamma) {
			r.gamma(gamma);
			g.gamma(gamma);
			b.gamma(gamma);
			return this;
		}
	
		public Layer gamma(float r, float g, float b) {
			this.r.gamma(r);
			this.g.gamma(g);
			this.b.gamma(b);
			return this;
		}
	
		public Layer gamma2() {
			r.gamma2();
			g.gamma2();
			b.gamma2();
			return this;
		}
	
		public Layer gamma4() {
			r.gamma4();
			g.gamma4();
			b.gamma4();
			return this;
		}
	
		public Layer gamma8() {
			r.gamma8();
			g.gamma8();
			b.gamma8();
			return this;
		}
	
		public Layer invert() {
			r.invert();
			g.invert();
			b.invert();
			return this;
		}
	
		public Layer threshold(float start, float end) {
			r.threshold(start, end);
			g.threshold(start, end);
			b.threshold(start, end);
			return this;
		}
	
		public Layer scale(int new_width, int new_height) {
			r = r.scale(new_width, new_height);
			g = g.scale(new_width, new_height);
			b = b.scale(new_width, new_height);
			if (a != null) {
				a = a.scale(new_width, new_height);
			}
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Layer scaleCubic(int new_width, int new_height) {
			r = r.scaleCubic(new_width, new_height);
			g = g.scaleCubic(new_width, new_height);
			b = b.scaleCubic(new_width, new_height);
			if (a != null) {
				a = a.scaleCubic(new_width, new_height);
			}
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Layer scaleFast(int new_width, int new_height) {
			r = r.scaleFast(new_width, new_height);
			g = g.scaleFast(new_width, new_height);
			b = b.scaleFast(new_width, new_height);
			if (a != null) {
				a = a.scaleFast(new_width, new_height);
			}
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Layer rotate(int degrees) {
			r = r.rotate(degrees);
			g = g.rotate(degrees);
			b = b.rotate(degrees);
			if (a != null) {
				a = a.rotate(degrees);
			}
			width = r.getWidth();
			height = r.getHeight();
			return this;
		}
	
		public Layer shear(float offset) {
			r = r.shear(offset);
			g = g.shear(offset);
			b = b.shear(offset);
			if (a != null) {
				a = a.shear(offset);
			}
			return this;
		}
	
		public Layer flipH() {
			r = r.flipH();
			g = g.flipH();
			b = b.flipH();
			if (a != null) {
				a = a.flipH();
			}
			return this;
		}
	
		public Layer flipV() {
			r = r.flipV();
			g = g.flipV();
			b = b.flipV();
			if (a != null) {
				a = a.flipV();
			}
			return this;
		}
	
		public Layer smooth(int radius) {
			r.smooth(radius);
			g.smooth(radius);
			b.smooth(radius);
			return this;
		}
	
		public Layer sharpen(int radius) {
			r.sharpen(radius);
			g.sharpen(radius);
			b.sharpen(radius);
	
			return this;
		}
	
		public Layer convolution(float[][] filter, float divisor, float offset) {
			r.convolution(filter, divisor, offset);
			g.convolution(filter, divisor, offset);
			b.convolution(filter, divisor, offset);
			return this;
		}
	
		public Layer grow(float r, float g, float b, int radius) {
			this.r.grow(r, radius);
			this.g.grow(g, radius);
			this.b.grow(b, radius);
			return this;
		}
	
		public Layer bump(Channel bumpmap, float lx, float ly, float shadow, float light_r, float light_g, float light_b, float ambient_r, float ambient_g, float ambient_b) {
			if(!(bumpmap.getWidth() == width && bumpmap.getHeight() == height))
				throw new Exception("bumpmap size does not match layer size");
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float nx = bumpmap.getPixelWrap(x + 1, y) - bumpmap.getPixelWrap(x - 1, y);
					float ny = bumpmap.getPixelWrap(x, y + 1) - bumpmap.getPixelWrap(x, y - 1);
					float brightness = nx*lx + ny*ly;
					if (brightness >= 0) {
						putPixelClip(x, y, (r.getPixel(x, y) + brightness*light_r)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow),
							(g.getPixel(x, y) + brightness*light_g)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow),
							(b.getPixel(x, y) + brightness*light_b)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow));
					} else {
						putPixelClip(x, y, (r.getPixel(x, y) + brightness*(1 - ambient_r))*(bumpmap.getPixel(x, y)*shadow + 1 - shadow),
							(g.getPixel(x, y) + brightness*(1 - ambient_g))*(bumpmap.getPixel(x, y)*shadow + 1 - shadow),
							(b.getPixel(x, y) + brightness*(1 - ambient_b))*(bumpmap.getPixel(x, y)*shadow + 1 - shadow));
					}
				}
			}
			return this;
		}
	
		public Layer bumpFast(Channel bumpmap, float lx, float light, float ambient) {
			if(!(bumpmap.getWidth() == width && bumpmap.getHeight() == height))
				throw new Exception("bumpmap size does not match layer size");
			ambient = 1f - ambient;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float brightness = lx*(bumpmap.getPixelWrap(x + 1, y) - bumpmap.getPixelWrap(x - 1, y));
					if (brightness >= 0) {
						brightness = brightness*light;
						putPixel(x, y, r.getPixel(x, y) + brightness,
							g.getPixel(x, y) + brightness,
							b.getPixel(x, y) + brightness);
					} else {
						brightness = brightness*ambient;
						putPixel(x, y, r.getPixel(x, y) + brightness,
							g.getPixel(x, y) + brightness,
							b.getPixel(x, y) + brightness);
					}
				}
			}
			return this;
		}
	
		public Layer bumpSpecular(Channel bumpmap, float lx, float ly, float lz, float shadow, float light_r, float light_g, float light_b, int specular) {
			if(!(bumpmap.getWidth() == width && bumpmap.getHeight() == height))
				throw new Exception("bumpmap size does not match layer size");
			float lnorm = (float)Math.Sqrt(lx*lx + ly*ly + lz*lz);
			float nz = 4*(1f/Math.Min(width, height));
			float nzlz = nz*lz;
			float nz2 = nz*nz;
			int power = 2<<specular;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float nx = bumpmap.getPixelWrap(x + 1, y) - bumpmap.getPixelWrap(x - 1, y);
					float ny = bumpmap.getPixelWrap(x, y + 1) - bumpmap.getPixelWrap(x, y - 1);
					float brightness = nx*lx + ny*ly;
					float costheta = (brightness + nzlz)/((float)Math.Sqrt(nx*nx + ny*ny + nz2)*lnorm);
					float highlight;
					if (costheta > 0) {
						highlight = (float)Math.Pow(costheta, power);
					} else {
						highlight = 0;
					}
					putPixelClip(x, y,
						(r.getPixel(x, y) + highlight*light_r)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow),
						(g.getPixel(x, y) + highlight*light_g)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow),
						(b.getPixel(x, y) + highlight*light_b)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow));
				}
			}
			return this;
		}
	
		public Layer toHSV() {
			float min = 0;
			float max = 0;
			float delta = 0;
			float h_val = 0;
			float s_val = 0;
			float v_val = 0;
	
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float r_val = r.getPixel(x, y);
					float g_val = g.getPixel(x, y);
					float b_val = b.getPixel(x, y);
					min = Math.Min(r_val, Math.Min(g_val, b_val));
					max = Math.Max(r_val, Math.Max(g_val, b_val));
	
					v_val = max;
					delta = max - min;
	
					if (max != 0) {
						s_val = delta/max;
					} else {
						s_val = 0;
					}
	
					if (max == r_val) {
						h_val = (g_val - b_val)/delta;
					}
					if (max == g_val) {
						h_val = 2 + (b_val - r_val)/delta;
					}
					if (max == b_val) {
						h_val = 4 + (r_val - g_val)/delta;
					}
	
					h_val /= 6;
					if (h_val < 0) {
						h_val += 1;
					}
	
					r.putPixel(x, y, h_val);
					g.putPixel(x, y, s_val);
					b.putPixel(x, y, v_val);
				}
			}
			return this;
		}
	
		public Layer toRGB() {
			int i;
			float f;
			float p;
			float q;
			float t;
			float r_val = 0;
			float g_val = 0;
			float b_val = 0;
	
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float h_val = r.getPixel(x, y);
					float s_val = g.getPixel(x, y);
					float v_val = b.getPixel(x, y);
	
					if (s_val == 0) {
						r.putPixel(x, y, v_val);
						g.putPixel(x, y, v_val);
						b.putPixel(x, y, v_val);
						continue;
					}
	
					h_val *= 6;
					i = (int)h_val;
					f = h_val - i;
					p = v_val*(1 - s_val);
					q = v_val*(1 - s_val*f);
					t = v_val*(1 - s_val*(1 - f));
	
					switch (i) {
						case 0:
							r_val = v_val;
							g_val = t;
							b_val = p;
							break;
						case 1:
							r_val = q;
							g_val = v_val;
							b_val = p;
							break;
						case 2:
							r_val = p;
							g_val = v_val;
							b_val = t;
							break;
						case 3:
							r_val = p;
							g_val = q;
							b_val = v_val;
							break;
						case 4:
							r_val = t;
							g_val = p;
							b_val = v_val;
							break;
						case 5:
							r_val = v_val;
							g_val = p;
							b_val = q;
							break;
						case 6:
							r_val = v_val;
							g_val = p;
							b_val = q;
							break;
						default:
							throw new Exception("hsv to rgb error");
					}
	
					r.putPixel(x, y, r_val);
					g.putPixel(x, y, g_val);
					b.putPixel(x, y, b_val);
				}
			}
			return this;
		}
	
		public Layer saturation(float saturation) {
			toHSV();
			float s_val;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					s_val = g.getPixel(x, y) * saturation;
					if (s_val < 0) {
						s_val = 0;
					} else {
						if (s_val > 1) {
							s_val = 1;
						}
					}
					g.putPixel(x, y, s_val);
				}
			}
			return toRGB();
		}
	
		public Layer hue(float hue) {
			toHSV();
			float h_val;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					h_val = r.getPixel(x, y) + hue;
					if (h_val < 0) {
						h_val += 1;
					} else {
						if (h_val > 1) {
							h_val -= 1;
						}
					}
					r.putPixel(x, y, h_val);
				}
			}
			return toRGB();
		}
	
		public Layer hueRotation(float min, float max, float new_min, float new_max) {
			toHSV();
			r.normalize(min, max, new_min, new_max);
			return toRGB();
		}
	
		public Layer lineart() {
			r.lineart();
			g.lineart();
			b.lineart();
			return this;
		}
	
		public Layer place(Layer sprite, int x_offset, int y_offset) {
			r.place(sprite.r, x_offset, y_offset);
			g.place(sprite.g, x_offset, y_offset);
			b.place(sprite.b, x_offset, y_offset);
			if (a != null && sprite.a != null)
				a.place(sprite.a, x_offset, y_offset);
			return this;
		}
	
		public Layer abs() {
			r.abs();
			g.abs();
			b.abs();
			return this;
		}
	
		public Layer layerBlend(Layer layer, float alpha) {
			r.channelBlend(layer.r, alpha);
			g.channelBlend(layer.g, alpha);
			b.channelBlend(layer.b, alpha);
			return this;
		}
	
		public Layer layerBlend(Layer rgb, Channel a) {
			return layerBlend(new Layer(rgb.r, rgb.g, rgb.b, a));
		}
	
		public Layer layerBlend(Layer layer) {
			if(!(layer.a != null))
				throw new Exception("cannot blend RGB only layer");
	
			if (a == null) {
				r.channelBlend(layer.r, layer.a);
				g.channelBlend(layer.g, layer.a);
				b.channelBlend(layer.b, layer.a);
			} else {
				float alpha;
				float alpha_inv;
				float r1;
				float g1;
				float b1;
				float a2;
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						if (a.getPixel(x, y) == 0) {
							putPixel(x, y, layer.r.getPixel(x, y), layer.g.getPixel(x, y), layer.b.getPixel(x, y), layer.a.getPixel(x, y));
						} else if (layer.a.getPixel(x, y) == 0) {
							continue;
						} else {
							alpha = 1f - (1f - a.getPixel(x, y))*(1f - layer.a.getPixel(x, y));
							alpha_inv = 1f/alpha;
							r1 = r.getPixel(x, y);
							g1 = g.getPixel(x, y);
							b1 = b.getPixel(x, y);
							a2 = layer.a.getPixel(x, y);
							r.putPixel(x, y, r1 - r1*a2*alpha_inv + layer.r.getPixel(x, y)*a2*alpha_inv);
							g.putPixel(x, y, g1 - g1*a2*alpha_inv + layer.g.getPixel(x, y)*a2*alpha_inv);
							b.putPixel(x, y, b1 - b1*a2*alpha_inv + layer.b.getPixel(x, y)*a2*alpha_inv);
							a.putPixel(x, y, alpha);
						}
					}
				}
			}
			return this;
		}
	
		public Layer layerAdd(Layer layer) {
			r.channelAdd(layer.r);
			g.channelAdd(layer.g);
			b.channelAdd(layer.b);
			return this;
		}
	
		public Layer layerSubtract(Layer layer) {
			r.channelSubtract(layer.r);
			g.channelSubtract(layer.g);
			b.channelSubtract(layer.b);
			return this;
		}
	
		public Layer layerAverage(Layer layer) {
			r.channelAverage(layer.r);
			g.channelAverage(layer.g);
			b.channelAverage(layer.b);
			return this;
		}
	
		public Layer layerMultiply(Layer layer) {
			r.channelMultiply(layer.r);
			g.channelMultiply(layer.g);
			b.channelMultiply(layer.b);
			return this;
		}
	
		public Layer layerDifference(Layer layer) {
			r.channelDifference(layer.r);
			g.channelDifference(layer.g);
			b.channelDifference(layer.b);
			return this;
		}
	
		public Layer layerDarkest(Layer layer) {
			r.channelDarkest(layer.r);
			g.channelDarkest(layer.g);
			b.channelDarkest(layer.b);
			return this;
		}
	
		public Layer layerBrightest(Layer layer) {
			r.channelBrightest(layer.r);
			g.channelBrightest(layer.g);
			b.channelBrightest(layer.b);
			return this;
		}
	
	}
}