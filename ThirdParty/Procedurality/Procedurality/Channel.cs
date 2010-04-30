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
using System.Collections;
using System.Collections.Generic;

namespace Procedurality
{
	public class Channel {
		public float[,] pixels;
		public int width;
		public int height;
		public bool powerof2;
	
		public Channel(int _Width, int _Height) 
		{
			pixels = new float[_Height,_Width];
			this.width = _Width;
			this.height = _Height;
			this.powerof2 = Utils.isPowerOf2(width) && Utils.isPowerOf2(height);
		}
	
		public Layer toLayer() {
			return new Layer(this, this, this);
		}
	
		public long getChecksum() {
			byte[] bytes = new byte[32];
			Crc32 checksum=new Crc32();
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			System.IO.BinaryWriter bytebuffer = new System.IO.BinaryWriter(ms);
			
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					bytebuffer.Write(getPixel(x, y));
					bytes=checksum.ComputeHash(ms);
				}
			}
			return BitConverter.ToInt64(bytes,0);
		}
	
		public int getWidth() {
			return width;
		}
	
		public int getHeight() {
			return height;
		}
	
		public void putPixel(int x, int y, float value) {
			pixels[y,x] = value;
		}
	
		public float getPixel(int x, int y) {
			return pixels[y,x];
		}
	
		public float[,] getPixels() {
			return pixels;
		}
		
		// By Rob "N3X15" Nelson, for river stuff
		/// <summary>
		/// Returns the maximum land height changes.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public float getMaxDelta()
		{
			float mD=0.0f;
			int x,y;
			for(x=0;x<Width-1;x++)
			{
				for(y=0;y<Height-1;y++)
				{
					float ch = getPixel(x,y);
					// Check neighbors.
					
					// North neighbor
					float nD = ch-getPixel(x,y+1);
					// East neighbor
					float eD = ch-getPixel(x+1,y);
					mD=Math.Max(mD,Math.Max(nD,eD));
				}
			}
			for(x=1;x<Width;x++)
			{
				for(y=1;y<Height;y++)
				{
					float ch = getPixel(x,y);
					// Check neighbors.
					
					// South neighbor
					float sD = ch-getPixel(x,y-1);
					// West neighbor
					float wD = ch-getPixel(x-1,y);
					
					mD=Math.Max(mD,Math.Max(sD,wD));
				}
			}
			return mD;
		}
		// By Rob "N3X15" Nelson, for river stuff
		/// <summary>
		/// 
		/// </summary>
		/// <param name="max">
		/// A <see cref="System.Single"/>
		/// </param>
		/// <returns>
		/// A <see cref="Channel"/>
		/// </returns>
		public Channel setMaxDelta(float max)
		{
			bool Altered=true;
			while(Altered)
			{
				Altered=false;
				//Console.WriteLine("Max so far: "+getMaxDelta().ToString());
				int x,y;
				for(x=0;x<Width-1;x++)
				{
					for(y=0;y<Height-1;y++)
					{
						float ch = getPixel(x,y);
						// Check neighbors.
						
						// North neighbor
						float nD = ch-getPixel(x,y+1);
						// East neighbor
						float eD = ch-getPixel(x+1,y);
						
						// Adjust, if needed.
						if(nD>max || eD>max)
						{
							pixels[y,x]-=0.01f;
							Altered=true;
						}
					}
				}
				for(x=1;x<Width;x++)
				{
					for(y=1;y<Height;y++)
					{
						float ch = getPixel(x,y);
						// Check neighbors.
						
						// South neighbor
						float sD = ch-getPixel(x,y-1);
						// West neighbor
						float wD = ch-getPixel(x-1,y);
						
						// Adjust, if needed.
						if(sD>max || wD>max)
						{
							pixels[y,x]-=0.01f;
							Altered=true;
						}
					}
				}
			}
			return this;
		}
	
		public void putPixelWrap(int x, int y, float value) {
			if (this.powerof2) {
				if (x < 0 || x >= width) x = (width + x) & (width - 1);
				if (y < 0 || y >= height) y = (height + y) & (height - 1);
				pixels[y,x] = value;
			} else {
				if (x < 0 || x >= width || y < 0 || y >= height) {
					pixels[(y + height) % height,(x + width) % width] = value;
				} else {
					pixels[y,x] = value;
				}
			}
		}
	
		public float getPixelWrap(int x, int y) {
			if (this.powerof2) {
				if (x < 0 || x >= width) x = (width + x) & (width - 1);
				if (y < 0 || y >= height) y = (height + y) & (height - 1);
				return pixels[y,x];
			} else {
				if (x < 0 || x >= width || y < 0 || y >= height) {
					return pixels[(y + height) % height,(x + width) % width];
				} else {
					return pixels[y,x];
				}
			}
		}
	
		public float getPixelSafe(int x, int y) {
			if (x < 0 || x >= width || y < 0 || y >= height) {
				return 0f;
			} else {
				return pixels[y,x];
			}
		}
	
		public void putPixelSafe(int x, int y, float value) {
			if (x >= 0 && x < width && y >= 0 && y < height) {
				pixels[y,x] = value;
			}
		}
	
		public void putPixelClip(int x, int y, float v) {
			if (v < 0f) pixels[y,x] = 0;
			else if (v > 1f) pixels[y,x] = 1;
			else pixels[y,x] = v;
		}
	
		public Channel fill(float value) {
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					pixels[y,x] = value;
			return this;
		}
	
		public Channel fill(float value, float min, float max) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (pixels[y,x] >= min && pixels[y,x] <= max) {
						pixels[y,x] = value;
					}
				}
			}
			return this;
		}
	
		public float findMin() {
			float min = float.MaxValue;
			float val = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					val = pixels[y,x];
					if (val < min) min = val;
				}
			}
			return min;
		}
	
		public float findMax() {
			float max = float.MinValue;
			float val = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					val = pixels[y,x];
					if (val > max) max = val;
				}
			}
			return max;
		}
	
		public float[] findMinMax() {
			float min = float.MaxValue;
			float max = float.MinValue;
			float val1 = 0;
			float val2 = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x+=2) {
					val1 = pixels[y,x];
					val2 = pixels[y,x+1];
					if (val1 <= val2) {
						if (val1 < min) min = val1;
						if (val2 > max) max = val2;
					} else {
						if (val2 < min) min = val2;
						if (val1 > max) max = val1;
					}
				}
			}
			return new float[]{min, max};
		}
	
		public float sum() {
			float sum = 0f;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					sum+= pixels[y,x];
				}
			}
			return sum;
		}
		
		public float average() {
			return sum()/(width*height);
		}
	
		public Channel copy() {
			Channel channel = new Channel(width, height);
			for(int x =0;x<this.getWidth();x++)
				for(int y =0;y<this.getWidth();y++)
					channel.putPixel(x,y,pixels[y,x]);
			return channel;
		}
	
		public Channel normalize() {
			float[] minmax = findMinMax();
			float min = minmax[0];
			float max = minmax[1];
			float factor = 1f/(max - min);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, factor*(pixels[y,x] - min));
				}
			}
			return this;
		}
	
		public Channel normalize(float new_min, float new_max) {
			float min = findMin();
			float max = findMax();
			float inv_maxmin = 1f/(max - min);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, Tools.interpolateLinear(new_min, new_max, (pixels[y,x] - min)*inv_maxmin));
				}
			}
			return this;
		}
	
		public Channel normalize(float min, float max, float new_min, float new_max) {
			float val;
			float inv_maxmin = 1f/(max - min);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					val = getPixel(x, y);
					if (val >= min && val <= max) {
						putPixel(x, y, Tools.interpolateLinear(new_min, new_max, (pixels[y,x] - min)*inv_maxmin));
					} else {
						if (val < min) {
							putPixel(x, y, new_min);
						} else {
							putPixel(x, y, new_max);
						}
					}
				}
			}
			return this;
		}
	
		public Channel normalizeSymmetric() {
			float[] minmax = findMinMax();
			float min = minmax[0];
			float max = minmax[1];
			if (min > (1 - max)) {
				min = 1f - max;
			}
			if (max < (1 - min)) {
				max = 1f - min;
			}
			float factor = 1f/(max - min);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, factor*(pixels[y,x] - min));
				}
			}
			return this;
		}
	
		public Channel clip() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixelClip(x, y, getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel crop(int x_lo, int y_lo, int x_hi, int y_hi) {
			int new_width = x_hi - x_lo + 1;
			int new_height = y_hi - y_lo + 1;
			Channel channel = new Channel(new_width, new_height);
			float[,] new_pixels = channel.getPixels();
			for (int y = y_lo; y <= y_hi; y++)
				for(int x=x_lo; x <= x_hi; x++)
					new_pixels[y - y_lo,x]=pixels[y,x];
			
			return channel;
		}
	
		public Channel cropWrap(int x_lo, int y_lo, int x_hi, int y_hi) {
			int new_width = x_hi - x_lo + 1;
			int new_height = y_hi - y_lo + 1;
			Channel channel = new Channel(new_width, new_height);
			for (int y = 0; y < new_height; y++) {
				int y_old = y + y_lo;
				for (int x = 0; x < new_width; x++) {
					int x_old = x + x_lo;
					if (x_old < 0 || x_old >= width || y_old < 0 || y_old >= height) {
						channel.putPixel(x, y, getPixelWrap(x_old, y_old));
					} else {
						channel.putPixel(x, y, getPixel(x_old, y_old));
					}
				}
			}
			return channel;
		}
	
		public Channel tile(int new_width, int new_height) {
			Channel channel = new Channel(new_width, new_height);
			for (int y = 0; y < new_height; y++) {
				for (int x = 0; x < new_width; x++) {
					if (x < width && y < height) {
						channel.putPixel(x, y, getPixel(x, y));
					} else {
						channel.putPixel(x, y, getPixelWrap(x, y));
					}
				}
			}
			pixels = channel.getPixels();
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Channel tileDouble() {
			Channel channel = new Channel(width<<1, height<<1);
			float[][] new_pixels = new float[height][];
			float[][] old_pixels = new float[height][];
			
			for(int i = 0;i<height;i++){
				new_pixels[i]=new float[width];
				old_pixels[i]=new float[width];
				for(int j=0;j<width;j++)
					old_pixels[i][j]=pixels[i,j];
			}
			
			for (int y = 0; y < height; y++) {
				Array.Copy(old_pixels[y], 0, new_pixels[y], 0, width);
				Array.Copy(old_pixels[y], 0, new_pixels[y], width, width);
				Array.Copy(old_pixels[y], 0, new_pixels[y + height], 0, width);
				Array.Copy(old_pixels[y], 0, new_pixels[y + height], width, width);
			}
			pixels = channel.getPixels();
			width = width<<1;
			height = height<<1;
			return this;
		}
	
		public Channel[] quadSplit() {
			if(!(Utils.isPowerOf2(width) && Utils.isPowerOf2(height))) throw new Exception("only power of 2 sized channels");
			Channel channel1 = this.copy().crop(0, 0, (width>>1) - 1, (height>>1) - 1);
			Channel channel2 = this.copy().crop(width>>1, 0, width - 1, (height>>1) - 1);
			Channel channel3 = this.copy().crop(0, height>>1, (width>>1) - 1, height - 1);
			Channel channel4 = this.copy().crop(width>>1, height>>1, width - 1, height - 1);
			return new Channel[]{channel1, channel2, channel3, channel4};
		}
	
		public Channel quadJoin(Channel channel1, Channel channel2, Channel channel3, Channel channel4) {
			if(!(channel1.width != channel2.width && channel2.width == channel3.width && channel3.width == channel4.width && channel1.height == channel2.height && channel2.height == channel3.height && channel3.height == channel4.height))
					throw new Exception("channels must be same size");
			if(!(width == channel1.width<<1 && height == channel1.height<<1)) throw new Exception("size mismatch");
			Channel channel = new Channel(channel1.width<<1, channel1.height<<1);
			channel.place(channel1, 0, 0);
			channel.place(channel2, channel1.width, 0);
			channel.place(channel3, 0, channel1.height);
			channel.place(channel4, channel1.width, channel1.height);
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel offset(int x_offset, int y_offset) {
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					channel.putPixel(x, y, getPixelWrap(x - x_offset, y - y_offset));
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel brightness(float brightness) {
			if (brightness > 1f) {
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						putPixelClip(x, y, brightness*getPixel(x, y));
					}
				}
			} else {
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						putPixel(x, y, brightness*getPixel(x, y));
					}
				}
			}
			return this;
		}
	
		public Channel multiply(float factor) {
			if (factor == 1)
				return this;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, factor*getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel power(float exponent) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, (float)Math.Pow(getPixel(x, y), exponent));
				}
			}
			return this;
		}
	
		public Channel power2() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float val = getPixel(x, y);
					putPixel(x, y, val*val);
				}
			}
			return this;
		}
	
		public Channel log() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, (float)Math.Log(getPixel(x, y)));
				}
			}
			return this;
		}
	
		public Channel add(float a) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, getPixel(x, y) + a);
				}
			}
			return this;
		}
	
		public Channel addClip(float add) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixelClip(x, y, getPixel(x, y) + add);
				}
			}
			return this;
		}
	
		public Channel contrast(float contrast) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixelClip(x, y, ((getPixel(x, y) - 0.5f)*contrast) + 0.5f);
				}
			}
			return this;
		}
	
		public Channel gamma(float gamma) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, (float)Math.Pow(getPixel(x, y), 1/gamma));
				}
			}
			return this;
		}
	
		public Channel gamma2() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float val = 1f - getPixel(x, y);
					putPixel(x, y, 1 - val*val);
				}
			}
			return this;
		}
	
		public Channel gamma4() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float val = 1f - getPixel(x, y);
					val = val*val;
					putPixel(x, y, 1 - val*val);
				}
			}
			return this;
		}
	
		public Channel gamma8() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float val = 1f - getPixel(x, y);
					val = val*val;
					val = val*val;
					putPixel(x, y, 1 - val*val);
				}
			}
			return this;
		}
	
		public Channel gain(float gain) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (getPixel(x, y) < 0.5f)
						putPixel(x, y, (float)(Math.Pow(2 * getPixel(x, y), Math.Log(1 - gain)/Math.Log(0.5d))/2f));
					else
						putPixel(x, y, 1f - (float)(Math.Pow(2 - 2 * getPixel(x, y), Math.Log(1 - gain)/Math.Log(0.5d))/2f));
				}
			}
			return this;
		}
	
		public Channel smoothGain() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, Tools.interpolateSmooth(0f, 1f, getPixel(x, y)));
				}
			}
			return this;
		}
	
		public Channel invert() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, 1f - getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel threshold(float start, float end) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float val = getPixel(x, y);
					if (val >= start && val <= end) {
						putPixel(x, y, 1f);
					} else {
						putPixel(x, y, 0f);
					}
				}
			}
			return this;
		}
	
		public Channel scale(int new_width, int new_height) {
			return this.scaleLinear(new_width, new_height);
		}
	
		public Channel scaleLinear(int new_width, int new_height) {
			if (width == new_width && height == new_height) {
				return this;
			}
			Channel channel = new Channel(new_width, new_height);
			float x_coord = 0;
			float y_coord = 0;
			float val1 = 0;
			float val2 = 0;
			float height_ratio = (float)height/new_height;
			float width_ratio = (float)width/new_width;
			for (int y = 0; y < new_height; y++) {
				y_coord = y*height_ratio - 0.5f;
				int y_coord_lo = (int)y_coord;
				int y_coord_hi = y_coord_lo + 1;
				for (int x = 0; x < new_width; x++) {
					x_coord = x*width_ratio - 0.5f;
					int x_coord_lo = (int)x_coord;
					int x_coord_hi = x_coord_lo + 1;
					float x_diff = x_coord - x_coord_lo;
					val1 = Tools.interpolateLinear(getPixelWrap(x_coord_lo, y_coord_lo),
						getPixelWrap(x_coord_hi, y_coord_lo),
						x_diff);
					val2 = Tools.interpolateLinear(getPixelWrap(x_coord_lo, y_coord_hi),
						getPixelWrap(x_coord_hi, y_coord_hi),
						x_diff);
					channel.putPixel(x, y, Math.Max(Math.Min(Tools.interpolateLinear(val1, val2, y_coord - y_coord_lo), 1f), 0f));
				}
			}
			pixels = channel.getPixels();
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Channel scaleCubic(int new_width, int new_height) {
			if (width == new_width && height == new_height) {
				return this;
			}
			Channel channel = new Channel(new_width, new_height);
			float x_coord, y_coord;
			float val0, val1, val2, val3;
			float height_ratio = (float)height/new_height;
			float width_ratio = (float)width/new_width;
			float x_diff, y_diff;
			int x_coord_lo, x_coord_lolo, x_coord_hi, x_coord_hihi, y_coord_lo, y_coord_lolo, y_coord_hi, y_coord_hihi;
			for (int y = 0; y < new_height; y++) {
				y_coord = y*height_ratio - 0.5f;
				y_coord_lo = (int)y_coord;
				y_coord_lolo = y_coord_lo - 1;
				y_coord_hi = y_coord_lo + 1;
				y_coord_hihi = y_coord_hi + 1;
				y_diff = y_coord - y_coord_lo;
				for (int x = 0; x < new_width; x++) {
					x_coord = x*width_ratio - 0.5f;
					x_coord_lo = (int)x_coord;
					x_coord_lolo = x_coord_lo - 1;
					x_coord_hi = x_coord_lo + 1;
					x_coord_hihi = x_coord_hi + 1;
					x_diff = x_coord - x_coord_lo;
					val0 = Tools.interpolateCubic(
						getPixelWrap(x_coord_lolo, y_coord_lolo),
						getPixelWrap(x_coord_lolo, y_coord_lo),
						getPixelWrap(x_coord_lolo, y_coord_hi),
						getPixelWrap(x_coord_lolo, y_coord_hihi),
						y_diff);
					val1 = Tools.interpolateCubic(
						getPixelWrap(x_coord_lo, y_coord_lolo),
						getPixelWrap(x_coord_lo, y_coord_lo),
						getPixelWrap(x_coord_lo, y_coord_hi),
						getPixelWrap(x_coord_lo, y_coord_hihi),
						y_diff);
					val2 = Tools.interpolateCubic(
						getPixelWrap(x_coord_hi, y_coord_lolo),
						getPixelWrap(x_coord_hi, y_coord_lo),
						getPixelWrap(x_coord_hi, y_coord_hi),
						getPixelWrap(x_coord_hi, y_coord_hihi),
						y_diff);
					val3 = Tools.interpolateCubic(
						getPixelWrap(x_coord_hihi, y_coord_lolo),
						getPixelWrap(x_coord_hihi, y_coord_lo),
						getPixelWrap(x_coord_hihi, y_coord_hi),
						getPixelWrap(x_coord_hihi, y_coord_hihi),
						y_diff);
					channel.putPixel(x, y, Math.Max(Math.Min(Tools.interpolateCubic(val0, val1, val2, val3, x_diff), 1f), 0f));
				}
			}
			pixels = channel.getPixels();
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Channel scaleFast(int new_width, int new_height) {
			if (width == new_width && height == new_height) {
				return this;
			}
			Channel channel = new Channel(new_width, new_height);
			int x_coord = 0;
			int y_coord = 0;
			for (int y = 0; y < new_height; y++) {
				for (int x = 0; x < new_width; x++) {
					x_coord = x*width/new_width;
					y_coord = y*height/new_height;
					channel.putPixel(x, y, getPixel(x_coord, y_coord));
				}
			}
			pixels = channel.getPixels();
			width = new_width;
			height = new_height;
			return this;
		}
	
		public Channel scaleDouble() {
			if(!(width == height)) throw new Exception("square images only");
	
			// calculate filter
			Channel filter = new Channel(width<<1, height<<1);
			for (int y = 0; y < height; y++) {
				int y_shift = y<<1;
				for (int x = 0; x < width; x++) {
					int x_shift = x<<1;
					float value = 0.25f*getPixel(x, y);
					filter.putPixel(x_shift, y_shift, value);
					filter.putPixel(x_shift + 1, y_shift, value);
					filter.putPixel(x_shift, y_shift + 1, value);
					filter.putPixel(x_shift + 1, y_shift + 1, value);
				}
			}
	
			// draw image
			Channel channel = new Channel(width<<1, height<<1);
			for (int y = 1; y < (height<<1) - 1; y++) {
				for (int x = 1; x < (width<<1) - 1; x++) {
					channel.putPixel(x, y, filter.getPixel(x - 1, y) + filter.getPixel(x + 1, y) + filter.getPixel(x, y - 1) + filter.getPixel(x, y + 1));
				}
			}
	
			// fix edges
			int max = (width<<1) - 1;
			for (int i = 0; i < max; i++) {
				channel.putPixel(0, i, filter.getPixelWrap(-1, i) + filter.getPixelWrap(1, i) + filter.getPixelWrap(0, i - 1) + filter.getPixelWrap(0, i + 1));
				channel.putPixel(i, 0, filter.getPixelWrap(i, -1) + filter.getPixelWrap(i, 1) + filter.getPixelWrap(i - 1, 0) + filter.getPixelWrap(i + 1, 0));
				channel.putPixel(max, i, filter.getPixelWrap(max - 1, i) + filter.getPixelWrap(max + 1, i) + filter.getPixelWrap(max, i - 1) + filter.getPixelWrap(max, i + 1));
				channel.putPixel(i, max, filter.getPixelWrap(i, max - 1) + filter.getPixelWrap(i, max + 1) + filter.getPixelWrap(i - 1, max) + filter.getPixelWrap(i + 1, max));
			}
			pixels = channel.getPixels();
			width = width<<1;
			height = height<<1;
			return this;
		}
	
		public Channel rotate(int degrees) {
			Channel channel = null;
			int tmp = width;
			switch (degrees) {
				case 90:
					channel = new Channel(height, width);
					for (int y = 0; y < height; y++) {
						for (int x = 0; x < width; x++) {
							channel.putPixel(y, width - x - 1, getPixel(x, y));
						}
					}
					width = height;
					height = tmp;
					break;
				case 180:
					channel = new Channel(width, height);
					for (int y = 0; y < height; y++) {
						for (int x = 0; x < width; x++) {
							channel.putPixel(width - x - 1, height - y - 1, getPixel(x, y));
					}
					}
					break;
				case 270:
					channel = new Channel(height, width);
					for (int y = 0; y < height; y++) {
						for (int x = 0; x < width; x++) {
							channel.putPixel(height - y - 1, x, getPixel(x, y));
						}
					}
					width = height;
					height = tmp;
					break;
				default:
					throw new Exception("Rotation degrees not a multiple of 90");
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel shear(float offset) {
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					channel.putPixel(x, y, getPixelWrap((int)(x + offset*width*((float)y/height)), y));
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel sine(int frequency) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, (float)Math.Sin(Math.PI*2*frequency*getPixel(x, y)));
				}
			}
			return this;
		}
	
		public Channel xsine(int frequency) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, (float)Math.Sin(2*Math.PI*(((float)x/width)*frequency + getPixel(x, y))));
				}
			}
			return this.normalize();
		}
	
		public Channel perturb(Channel channel1, Channel channel2) {
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float x_coord = x + width*channel1.getPixel(x, y);
					int x_coord_lo = (int)x_coord;
					int x_coord_hi = x_coord_lo + 1;
					float x_frac = x_coord - x_coord_lo;
					float y_coord = y + height*channel2.getPixel(x, y);
					int y_coord_lo = (int)y_coord;
					int y_coord_hi = y_coord_lo + 1;
					float y_frac = y_coord - y_coord_lo;
					float val1 = Tools.interpolateLinear(getPixelWrap(x_coord_lo, y_coord_lo), getPixelWrap(x_coord_hi, y_coord_lo), x_frac);
					float val2 = Tools.interpolateLinear(getPixelWrap(x_coord_lo, y_coord_hi), getPixelWrap(x_coord_hi, y_coord_hi), x_frac);
					channel.putPixel(x, y, Tools.interpolateLinear(val1, val2, y_frac));
				}
			}
			pixels = channel.getPixels();
			return this;
		}
		
		public int Height
		{
			get { return getHeight(); }
		}
		
		public int Width
		{
			get { return getWidth(); }
		}
		
		/// <summary>
		/// Smooths below-water surfaces and, optionally, adds beaches.
		/// </summary>
		/// <param name="waterlevel">
		/// A <see cref="System.Single"/>
		/// </param>
		/// <param name="beaches">
		/// A <see cref="System.Boolean"/>
		/// </param>
		/// <returns>
		/// A <see cref="Channel"/>
		/// </returns>
		public Channel silt(float waterlevel,bool beaches)
		{
			// 1. Copy Image
			// 2. Apply gauss blur to lower layer
			// 3. Bring back unblurred terrain from above the water level.
			
			// Copy heightmap
			Channel orig,newh,blurred;
			orig=newh=blurred=this;
			
			float wl=(beaches) ? waterlevel+5f:waterlevel;
			wl=wl/256f;
		
			// Gaussian blur for silt and beaches.	
			float[][] gaussian_matrix=new float[3][]{
				new float[3]{1f,2f,1f},
				new float[3]{2f,4f,2f},
				new float[3]{1f,2f,1f}
			};
			blurred=this.convolution(gaussian_matrix,32f,waterlevel/2f);
			blurred=blurred.HeightClamp(waterlevel-(0.1f/256f));
	
			for(int x=0; x < this.getWidth(); x++)
			{
				for(int y=0; y < this.getHeight(); y++)
				{
					// If > WL: use unblurred image.
					// If <=WL: Use blurred image.
					if(newh.getPixel(x,y)>wl)
						newh.putPixel(x,y,newh.getPixel(x,y));
					else
						newh.putPixel(x,y,blurred.getPixel(x,y));
				}
			}
			return newh;
		}
		
		public Channel HeightClamp(float h)
		{
			for(int x=0; x < getWidth(); x++)
			{
				for(int y=0; y < getHeight(); y++)
				{
					float nh=(getPixel(x,y)>h) ? h : getPixel(x,y);
					putPixel(x,y,nh);
				}
			}
			return this;
		}
	
		public Channel perturb(Channel perturb, float magnitude) {
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float perturbation = magnitude*(perturb.getPixel(x, y) - 0.5f);
					float x_coord = x + width*perturbation;
					int x_coord_lo = (int)x_coord;
					int x_coord_hi = x_coord_lo + 1;
					float x_frac = x_coord - x_coord_lo;
					float y_coord = y + height*perturbation;
					int y_coord_lo = (int)y_coord;
					int y_coord_hi = y_coord_lo + 1;
					float y_frac = y_coord - y_coord_lo;
					float val1 = Tools.interpolateLinear(getPixelWrap(x_coord_lo, y_coord_lo), getPixelWrap(x_coord_hi, y_coord_lo), x_frac);
					float val2 = Tools.interpolateLinear(getPixelWrap(x_coord_lo, y_coord_hi), getPixelWrap(x_coord_hi, y_coord_hi), x_frac);
					channel.putPixel(x, y, Tools.interpolateLinear(val1, val2, y_frac));
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel flipH() {
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					channel.putPixel(x, y, getPixel(width - x - 1, y));
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel flipV() {
			
			for (int y = 0; y < height; y++) {
				for(int x=0;x < width; x++)
				{
					float tmp = pixels[y,x];
					pixels[y,x] = pixels[height - y - 1,x];
					pixels[height - y - 1,x] = tmp;
				}
			}
			return this;
		}
	
		public Channel smoothFast() {
			Channel filter = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					filter.putPixel(x, y, 0.25f*getPixel(x, y));
				}
			}
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, filter.getPixelWrap(x - 1, y) + filter.getPixelWrap(x + 1, y) + filter.getPixelWrap(x, y - 1) + filter.getPixelWrap(x, y + 1));
				}
			}
			return this;
		}
	
		public Channel smooth(int radius) {
			radius = Math.Max(1, radius);
			Channel filter = new Channel(width, height);
			float factor = 1f/((2*radius + 1)*(2*radius + 1));
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					filter.putPixel(x, y, factor*getPixel(x, y));
				}
			}
			for (int x = radius; x < width - radius; x++) {
				int y = radius;
				float sum = 0f;
				for (int i = -radius; i < radius + 1; i++) {
					for (int j = -radius; j < radius + 1; j++) {
						sum += filter.getPixel(x + j, y + i);
					}
				}
				for (y++; y < height - radius; y++) {
					for (int j = -radius; j < radius + 1; j++) {
						sum -= filter.getPixel(x + j, y - radius - 1);
						sum += filter.getPixel(x + j, y + radius);
					}
					putPixel(x, y, sum);
				}
			}
			return this;
		}
	
		public Channel smoothWrap(int radius) {
			radius = Math.Max(1, radius);
			Channel filter = new Channel(width, height);
			float factor = 1f/((2*radius + 1)*(2*radius + 1));
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					filter.putPixel(x, y, factor*getPixel(x, y));
				}
			}
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					float sum = 0f;
					for (int i = -radius; i < radius + 1; i++) {
						for (int j = -radius; j < radius + 1; j++) {
							sum += filter.getPixelWrap(x + j, y + i);
						}
					}
					putPixel(x, y, sum);
				}
			}
			return this;
		}
	
		public Channel smooth(int radius, Channel mask) {
			radius = Math.Max(1, radius);
			Channel filter = new Channel(width, height);
			float factor = 1f/((2*radius + 1)*(2*radius + 1));
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					filter.putPixel(x, y, factor*getPixel(x, y));
				}
			}
			for (int x = radius; x < width - radius; x++) {
				int y = radius;
				float sum = 0f;
				for (int i = -radius; i < radius + 1; i++) {
					for (int j = -radius; j < radius + 1; j++) {
						sum += filter.getPixel(x + j, y + i);
					}
				}
				for (y++; y < height - radius; y++) {
					float alpha = mask.getPixel(x, y);
					if (alpha > 0) {
						for (int j = -radius; j < radius + 1; j++) {
							sum -= filter.getPixel(x + j, y - radius - 1);
							sum += filter.getPixel(x + j, y + radius);
						}
						putPixel(x, y, alpha*sum + (1f - alpha)*getPixel(x, y));
					}
				}
			}
			return this;
		}
	
		public Channel sharpen(int radius) {
			radius = Math.Max(1, radius);
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float value = 0;
					for (int i = -radius; i <= radius; i++) {
						for (int j = -radius; j <= radius; j++) {
							if (i == 0 && j == 0) {
								value += (2*radius + 1)*(2*radius + 1)*getPixel(x, y);
							} else {
								value -= getPixelWrap(x + i, y + j);
							}
						}
					}
					channel.putPixel(x, y, value);
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel convolution(float[][] filter, float divisor, float offset) {
			int radius = (filter.GetLength(0) - 1)/2;
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float value = 0;
					for (int i = -radius; i <= radius; i++) {
						for (int j = -radius; j <= radius; j++) {
							value += filter[i + radius][j + radius] * getPixelWrap(x + i, y + j);
						}
					}
					value = value/divisor + offset;
					channel.putPixel(x, y, value);
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel floodfill(int init_x, int init_y, float value) {
			if(init_x < width && init_x >= 0)
			{
				Console.Write("x coordinate outside image");
				return null;
			}
			if(init_y < height && init_y >= 0)
			{
				Console.Write("y coordinate outside image");
				return null;
			}
			float oldval = getPixel(init_x, init_y);
			bool[,] marked = new bool[width,height];
			marked[init_x,init_y] = true;
			Stack list = new Stack();
			list.Push(new int[]{init_x, init_y});
	
			while (list.ToArray().Length > 0) {
				int[] coords = (int[])list.Pop();
				int x = coords[0];
				int y = coords[1];
				putPixel(x, y, value);
				if (x > 0 && getPixel(x - 1, y) == oldval && !marked[x - 1,y]) {
					marked[x - 1,y] = true;
					list.Push(new int[]{x - 1, y});
				}
				if (x < width - 1 && getPixel(x + 1, y) == oldval && !marked[x + 1,y]) {
					marked[x + 1,y] = true;
					list.Push(new int[]{x + 1, y});
				}
				if (y > 0 && getPixel(x, y - 1) == oldval && !marked[x,y - 1]) {
					marked[x,y - 1] = true;
					list.Push(new int[]{x, y - 1});
				}
				if (y < height - 1 && getPixel(x, y + 1) == oldval && !marked[x,y + 1]) {
					marked[x,y + 1] = true;
					list.Push(new int[]{x, y + 1});
				}
			}
			return this;
		}
	
		public Channel largestConnected(float value) {
			Channel tmp = this.copy();
			Channel fillmap = new Channel(width, height);
			int[] fillcoords = tmp.findFirst(value);
			int max_count = 0;
			while (fillcoords[0] != -1) { // while reachable pixels remain
				int count = 0;
				int init_x = fillcoords[0];
				int init_y = fillcoords[1];
				fillmap.fill(0f);
				// flood fill
				bool[,] marked = new bool[width,height];
				marked[init_x,init_y] = true;
				Stack list = new Stack();
				list.Push(new int[]{init_x, init_y});
				while (list.ToArray().Length > 0) {
					int[] coords = (int[])list.Pop();
					int x = coords[0];
					int y = coords[1];
					tmp.putPixel(x, y, -1f);
					fillmap.putPixel(x, y, 1f);
					count++;
					if (x > 0 && tmp.getPixel(x - 1, y) == 1f && !marked[x - 1,y]) {
						marked[x - 1,y] = true;
						list.Push(new int[]{x - 1, y});
					}
					if (x < width - 1 && tmp.getPixel(x + 1, y) == 1f && !marked[x + 1,y]) {
						marked[x + 1,y] = true;
						list.Push(new int[]{x + 1, y});
					}
					if (y > 0 && tmp.getPixel(x, y - 1) == 1f && !marked[x,y - 1]) {
						marked[x,y - 1] = true;
						list.Push(new int[]{x, y - 1});
					}
					if (y < height - 1 && tmp.getPixel(x, y + 1) == 1f && !marked[x,y + 1]) {
						marked[x,y + 1] = true;
						list.Push(new int[]{x, y + 1});
					}
				}
				if (count > max_count) {
					pixels = fillmap.copy().pixels;
					max_count = count;
				}
				fillcoords = tmp.findFirst(value);
			}
			return this;
		}
	
		public float averageConnected(float value) {
			Channel tmp = this.copy();
			int[] fillcoords = tmp.findFirst(value);
			int area_count = 0;
			int area_total = 0;
			while (fillcoords[0] != -1) { // while reachable pixels remain
				area_count++;
				int count = 0;
				int init_x = fillcoords[0];
				int init_y = fillcoords[1];
				// flood fill
				bool[,] marked = new bool[width,height];
				marked[init_x,init_y] = true;
				Stack list = new Stack();
				list.Push(new int[]{init_x, init_y});
				while (list.ToArray().Length> 0) {
					int[] coords = (int[])list.Pop();
					int x = coords[0];
					int y = coords[1];
					tmp.putPixel(x, y, -1f);
					count++;
					if (x > 0 && tmp.getPixel(x - 1, y) == 1f && !marked[x - 1,y]) {
						marked[x - 1,y] = true;
						list.Push(new int[]{x - 1, y});
					}
					if (x < width - 1 && tmp.getPixel(x + 1, y) == 1f && !marked[x + 1,y]) {
						marked[x + 1,y] = true;
						list.Push(new int[]{x + 1, y});
					}
					if (y > 0 && tmp.getPixel(x, y - 1) == 1f && !marked[x,y - 1]) {
						marked[x,y - 1] = true;
						list.Push(new int[]{x, y - 1});
					}
					if (y < height - 1 && tmp.getPixel(x, y + 1) == 1f && !marked[x,y + 1]) {
						marked[x,y + 1] = true;
						list.Push(new int[]{x, y + 1});
					}
				}
				fillcoords = tmp.findFirst(value);
				area_total += count;
			}
			if (area_count == 0) {
				return -1f;
			} else {
				return area_total/area_count;
			}
		}
		
		public Channel squareFit(float value, int size) {
			Channel channel = new Channel(width, height);
			bool match;
			for (int y = 0; y <= height - size; y++) {
				for (int x = 0; x <= width - size; x++) {
					match = true;
					for (int i = 0; i < size; i++) {
						for (int j = 0; j < size; j++) {
							match = match && (getPixel(x + i, y + j) == value);
							if (!match) break;
						}
					}
					if (match) {
						channel.putPixel(x, y, value);
					}
				}
			}
			pixels = channel.copy().pixels;
			return this;
		}
		
		public Channel boxFit(float value, int width, int height) {
			Channel channel = new Channel(this.width, this.height);
			bool match;
			for (int y = 0; y <= this.height - height; y++) {
				for (int x = 0; x <= this.width - width; x++) {
					match = true;
					for (int i = 0; i < width; i++) {
						for (int j = 0; j < height; j++) {
							match = match && (getPixel(x + i, y + j) == value);
							if (!match) break;
						}
					}
					if (match) {
						channel.putPixel(x, y, value);
					}
				}
			}
			pixels = channel.copy().pixels;
			return this;
		}
	
		public int count(float value) {
			int count = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (getPixel(x, y) == value)
						count++;
				}
			}
			return count;
		}
	
		public Channel grow(float value, int radius) {
			Channel channel = this.copy();
			for (int y = radius; y < height - radius; y++) {
				for (int x = radius; x < width - radius; x++) {
					if (getPixel(x, y) == value) {
						for (int i = -radius; i <= radius; i++) {
							for (int j = -radius; j <= radius; j++) {
								channel.putPixel(x + i, y + j, value);
							}
						}
					}
				}
			}
			pixels = channel.getPixels();
			return this;
		}
		
		public Channel squareGrow(float value, int size) {
			Channel channel = this.copy();
			for (int y = 0; y <= height - size; y++) {
				for (int x = 0; x <= width - size; x++) {
					if (getPixel(x, y) == value) {
						for (int i = 0; i < size; i++) {
							for (int j = 0; j < size; j++) {
								channel.putPixel(x + i, y + j, value);
							}
						}
					}
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public int[] find(int radius, int x_start, int y_start, float value) {
			if (getPixel(x_start, y_start) == value)
				return new int[]{x_start, y_start};
			int r = 1;
			while (r <= radius) {
				int x = x_start - r;
				int x2 = x_start + r;
				for (int i = 0; i < 2*r - 1; i++) {
					int y_i = y_start - r + 1 + i;
					if (getPixelWrap(x, y_i) == value)
						return new int[]{(x + width)%width, (y_i + height)%height};
					if (getPixelWrap(x2, y_i) == value)
						return new int[]{(x2 + width)%width, (y_i + height)%height};
				}
				int y = y_start - r;
				int y2 = y_start + r;
				for (int i = 0; i < 2*r + 1; i++) {
					int x_i = x_start - r + i;
					if (getPixelWrap(x_i, y) == value)
						return new int[]{(x_i + width)%width, (y + height)%height};
					if (getPixelWrap(x_i, y2) == value)
						return new int[]{(x_i + width)%width, (y2 + height)%height};
				}
				r++;
			}
			return new int[]{-1, -1};
		}
		
		public int[] findNoWrap(int radius, int x_start, int y_start, float value) {
			if (getPixel(x_start, y_start) == value)
				return new int[]{x_start, y_start};
			int r = 1;
			while (r <= radius) {
				int x = x_start - r;
				int x2 = x_start + r;
				for (int i = 0; i < 2*r - 1; i++) {
					int y_i = y_start - r + 1 + i;
					if (x >= 0 && x < width && y_i >= 0 && y_i < height && getPixel(x, y_i) == value)
						return new int[]{x, y_i};
					if (x2 >= 0 && x2 < width && y_i >= 0 && y_i < height && getPixel(x2, y_i) == value)
						return new int[]{x2, y_i};
				}
				int y = y_start - r;
				int y2 = y_start + r;
				for (int i = 0; i < 2*r + 1; i++) {
					int x_i = x_start - r + i;
					if (x_i >= 0 && x_i < width && y >= 0 && y < height && getPixel(x_i, y) == value)
						return new int[]{x_i, y};
					if (x_i >= 0 && x_i < width && y2 >= 0 && y2 < height && getPixel(x_i, y2) == value)
						return new int[]{x_i, y2};
				}
				r++;
			}
			return new int[]{-1, -1};
		}
	
		public int[] findFirst(float value) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (getPixel(x, y) == value)
						return new int[]{x, y};
				}
			}
			return new int[]{-1, -1};
		}
	
		public Channel bump(Channel bumpmap, float lx, float ly, float shadow, float light, float ambient) {
			if(!(bumpmap.getWidth() == width && bumpmap.getHeight() == height))
				throw new Exception("bumpmap does not match channel size");
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float nx = bumpmap.getPixelWrap(x + 1, y) - bumpmap.getPixelWrap(x - 1, y);
					float ny = bumpmap.getPixelWrap(x, y + 1) - bumpmap.getPixelWrap(x, y - 1);
					float brightness = nx*lx + ny*ly;
					if (brightness >= 0) {
						channel.putPixelClip(x, y, (getPixel(x, y) + brightness*light)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow));
					} else {
						channel.putPixelClip(x, y, (getPixel(x, y) + brightness*(1 - ambient))*(bumpmap.getPixel(x, y)*shadow + 1 - shadow));
					}
				}
			}
			pixels = channel.getPixels();
			return this;
		}
		
		public Channel bumpSpecular(Channel bumpmap, float lx, float ly, float lz, float shadow, float light, int specular) {
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
					float costheta = (brightness + nzlz)/((float)Math.Sqrt(nx*nx + ny*ny + nz2)*lnorm); // can use math here, not game state affecting
					float highlight;
					if (costheta > 0) {
						highlight = (float)Math.Pow(costheta, power); // can use math here, not game state affecting
					} else {
						highlight = 0;
					}
					putPixelClip(x, y, (getPixel(x, y) + highlight*light)*(bumpmap.getPixel(x, y)*shadow + 1 - shadow));
				}
			}
			return this;
		}
	
		public Channel lineart() {
			Channel channel = new Channel(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					channel.putPixel(x, y, Math.Max(
						Math.Max(
							Math.Abs(getPixelWrap(x, y) - getPixelWrap(x - 1, y)),
							Math.Abs(getPixelWrap(x, y) - getPixelWrap(x + 1, y))),
						Math.Max(
							Math.Abs(getPixelWrap(x, y) - getPixelWrap(x, y - 1)),
							Math.Abs(getPixelWrap(x, y) - getPixelWrap(x, y + 1)))
					));
				}
			}
			pixels = channel.getPixels();
			return this;
		}
	
		public Channel relativeIntensity(int radius) {
			radius = Math.Max(1, radius);
			Channel relint = new Channel(width, height);
			float factor = 1f/((2*radius + 1)*(2*radius + 1));
			float sum, avr;
	
			for (int x = 0; x < width; x++) {
				int y = 0;
				sum = 0f;
				for (int i = -radius; i < radius + 1; i++) {
					for (int j = -radius; j < radius + 1; j++) {
						sum += getPixelWrap(x + j, y + i);
					}
				}
				for (; y < height; y++) {
					if (y > 0) {
						for (int j = -radius; j < radius + 1; j++) {
							sum -= getPixelWrap(x + j, y - radius - 1);
							sum += getPixelWrap(x + j, y + radius);
						}
					}
					avr = sum*factor;
					relint.putPixel(x, y, getPixel(x, y) - avr);
				}
			}
			return relint.add(0.5f);
		}
	
		public Channel relativeIntensityNormalized(int radius) {
			return this.relativeIntensity(radius).normalizeSymmetric();
		}
	
		public Channel[] fft() {
			if(!(width == height))
				throw new Exception("square images only");
			int size = width;
			if(!(Utils.isPowerOf2(size)))
				throw new Exception("size must be power of 2");
	
			// convert channel to complex number array
			float[] a = new float[size*size*2 + 1];
			int n = 1;
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					a[n] = getPixel(x, y);
					n += 2;
				}
			}
	
			// perform fast fourier transform
			fastFourierTransform(a, size, 1);
	
			// convert complex number array to channels
			n = 1;
			Channel magnitude = new Channel(size, size);
			Channel phase = new Channel(size, size);
			float real, imag;
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					real = a[n++];
					imag = a[n++];
					magnitude.putPixel(x, y, (float)Math.Sqrt(real*real + imag*imag));
					if (imag == 0 && real >= 0) {
						phase.putPixel(x, y, (float)Math.PI/2f);
					} else if (imag == 0 && real < 0) {
						phase.putPixel(x, y, (float)Math.PI/-2f);
					} else {
						phase.putPixel(x, y, (float)Math.Atan(real/imag));
					}
				}
			}
	
			// return magnitude and phase channels
			return new Channel[]{magnitude.offset(size>>1, size>>1), phase};
		}
	
		public Channel fftInv(Channel magni, Channel phase) {
			if(!(magni.width == magni.height && phase.width == phase.height && magni.width == phase.width))
				throw new Exception("both images must be square and same size");
			int size = magni.width;
			if(!(Utils.isPowerOf2(size)))
				throw new Exception("size must be power of 2");
	
			// convert channels to complex number array
			Channel magnitude = magni.copy().offset(size>>1, size>>1);
			float mag, pha;
			float[] a = new float[size*size*2 + 1];
			int n = 1;
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					mag = magnitude.getPixel(x, y);
					pha = phase.getPixel(x, y);
					a[n++] = mag*(float)Math.Cos(pha);
					a[n++] = mag*(float)Math.Sin(pha);
				}
			}
	
			// perform fast fourier transform
			fastFourierTransform(a, size, -1);
	
			// convert complex number array to channel
			n = 1;
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					putPixel(x, y, a[n]);
					n += 2;
				}
			}
	
			// return real component channel
			return this;
		}
	
		private void fastFourierTransform(float[] data, int size, int isign) {
			int i1, i2, i3;
			int i2rev, i3rev, ip1, ip2, ip3, ifp1, ifp2;
			int ibit, idim, k1, k2, n, nprev, nrem, ntot;
			float tempi, tempr;
			float theta, wi, wpi, wpr, wr, wtemp;
			ntot = size*size;
			nprev = 1;
			for (idim = 2; idim >= 1; idim--) {
				n = size;
				nrem = ntot / (n * nprev);
				ip1 = nprev << 1;
				ip2 = ip1 * n;
				ip3 = ip2 * nrem;
				i2rev = 1;
				for (i2 = 1; i2 <= ip2; i2 += ip1) {
					if (i2 < i2rev) {
						for (i1 = i2; i1 <= i2 + ip1 - 2; i1 += 2) {
							for (i3 = i1; i3 <= ip3; i3 += ip2) {
								i3rev = i2rev + i3 - i2;
								tempr=data[i3]; data[i3] = (data[i3rev]); data[i3rev] = tempr;
								tempr=data[i3 + 1]; data[i3 + 1] = data[i3rev + 1]; data[i3rev + 1] = tempr;
							}
						}
					}
					ibit = ip2 >> 1;
					while (ibit >= ip1 && i2rev > ibit) {
						i2rev -= ibit;
						ibit >>= 1;
					}
					i2rev += ibit;
				}
				ifp1 = ip1;
				while (ifp1 < ip2) {
					ifp2 = ifp1 << 1;
					theta = isign * ((float)Math.PI * 2) / (ifp2 / ip1);
					wtemp = (float)Math.Sin(0.5 * theta);
					wpr = -2.0f * wtemp * wtemp;
					wpi = (float)Math.Sin(theta);
					wr = 1.0f;
					wi = 0.0f;
					for (i3 = 1; i3 <= ifp1; i3 += ip1) {
						for (i1 = i3; i1 <= i3 + ip1 - 2; i1 += 2) {
							for (i2 = i1; i2 <= ip3; i2 += ifp2) {
								k1 = i2;
								k2 = k1 + ifp1;
								tempr = wr * data[k2] - wi * data[k2 + 1];
								tempi = wr * data[k2 + 1] + wi * data[k2];
								data[k2] = data[k1] - tempr;
								data[k2 + 1] = data[k1 + 1] - tempi;
								data[k1] += tempr;
								data[k1 + 1] += tempi;
							}
						}
						wr = (wtemp = wr) * wpr - wi * wpi + wr;
						wi = wi * wpr + wtemp * wpi + wi;
					}
					ifp1 = ifp2;
				}
				nprev *= n;
			}
		}
	
		public Channel erode(float talus, int iterations) {
			float h, h1, h2, h3, h4, d1, d2, d3, d4, max_d;
			int i, j;
			for (int iter = 0; iter < iterations; iter++) {
				for (int y = 1; y < height - 1; y++) {
					for (int x = 1; x < width - 1; x++) {
						h = getPixel(x, y);
						h1 = getPixel(x, y + 1);
						h2 = getPixel(x - 1, y);
						h3 = getPixel(x + 1, y);
						h4 = getPixel(x, y - 1);
						d1 = h - h1;
						d2 = h - h2;
						d3 = h - h3;
						d4 = h - h4;
						i = 0;
						j = 0;
						max_d = 0f;
						if (d1 > max_d) {
							max_d = d1;
							j = 1;
						}
						if (d2 > max_d) {
							max_d = d2;
							i = -1;
							j = 0;
						}
						if (d3 > max_d) {
							max_d = d3;
							i = 1;
							j = 0;
						}
						if (d4 > max_d) {
							max_d = d4;
							i = 0;
							j = -1;
						}
						if (max_d > talus) {
							continue;
						}
						max_d *= 0.5f;
						putPixel(x, y, getPixel(x, y) - max_d);
						putPixel(x + i, y + j, getPixel(x + i, y + j) + max_d);
					}
				}
			}
			return this;
		}
		
		public Channel erodeThermal(float talus, int iterations) {
			float h, h1, h2, h3, h4, d1, d2, d3, d4, max_d;
			int i, j;
			for (int iter = 0; iter < iterations; iter++) {
				for (int y = 1; y < height - 1; y++) {
					for (int x = 1; x < width - 1; x++) {
						h = getPixel(x, y);
						h1 = getPixel(x, y + 1);
						h2 = getPixel(x - 1, y);
						h3 = getPixel(x + 1, y);
						h4 = getPixel(x, y - 1);
						d1 = h - h1;
						d2 = h - h2;
						d3 = h - h3;
						d4 = h - h4;
						i = 0;
						j = 0;
						max_d = 0f;
						if (d1 > max_d) {
							max_d = d1;
							j = 1;
						}
						if (d2 > max_d) {
							max_d = d2;
							i = -1;
							j = 0;
						}
						if (d3 > max_d) {
							max_d = d3;
							i = 1;
							j = 0;
						}
						if (d4 > max_d) {
							max_d = d4;
							i = 0;
							j = -1;
						}
						if (max_d < talus) {
							continue;
						}
						max_d *= 0.5f;
						putPixel(x, y, getPixel(x, y) - max_d);
						putPixel(x + i, y + j, getPixel(x + i, y + j) + max_d);
					}
				}
			}
			return this;
		}
	
		public Channel place(Channel sprite, int x_offset, int y_offset) {
			for (int y = y_offset; y < y_offset + sprite.getHeight(); y++) {
				for (int x = x_offset; x < x_offset + sprite.getWidth(); x++) {
					putPixelWrap(x, y, sprite.getPixelWrap(x - x_offset, y - y_offset));
				}
			}
			return this;
		}
	
		public Channel place(Channel sprite, Channel alpha, int x_offset, int y_offset) {
			float alpha_val;
			for (int y = y_offset; y < y_offset + sprite.getHeight(); y++) {
				for (int x = x_offset; x < x_offset + sprite.getWidth(); x++) {
					alpha_val = alpha.getPixel(x - x_offset, y - y_offset);
					putPixelWrap(x, y, alpha_val*sprite.getPixelWrap(x - x_offset, y - y_offset) + (1 - alpha_val)*getPixelWrap(x, y));
				}
			}
			return this;
		}
	
		public Channel placeBrightest(Channel sprite, int x_offset, int y_offset) {
			for (int y = y_offset; y < y_offset + sprite.getHeight(); y++) {
				for (int x = x_offset; x < x_offset + sprite.getWidth(); x++) {
					putPixelWrap(x, y, Math.Max(getPixelWrap(x, y), sprite.getPixelWrap(x - x_offset, y - y_offset)));
				}
			}
			return this;
		}
	
		public Channel placeDarkest(Channel sprite, int x_offset, int y_offset) {
			for (int y = y_offset; y < y_offset + sprite.getHeight(); y++) {
				for (int x = x_offset; x < x_offset + sprite.getWidth(); x++) {
					putPixelWrap(x, y, Math.Min(getPixelWrap(x, y), sprite.getPixelWrap(x - x_offset, y - y_offset)));
				}
			}
			return this;
		}
	
		public Channel abs() {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, 2f*Math.Abs(getPixel(x, y) - 0.5f));
				}
			}
			return this;
		}
	
	
		public Channel channelBlend(Channel channel, float alpha) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, alpha*channel.getPixel(x, y) + (1 - alpha)*getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelBlend(Channel channel, Channel alpha) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float alpha_val = alpha.getPixel(x, y);
					putPixel(x, y, alpha_val*channel.getPixel(x, y) + (1 - alpha_val)*getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelAdd(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					try
					{
						putPixelClip(x, y, getPixel(x, y) + channel.getPixel(x, y));
					} catch(Exception) {
						putPixelClip(x,y,0f);
						Console.WriteLine("Failed to get pixel ("+x.ToString()+","+y.ToString()+").");
					}
				}
			}
			return this;
		}
	
		public Channel channelAddNoClip(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, getPixel(x, y) + channel.getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelSubtract(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixelClip(x, y, getPixel(x, y) - channel.getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelSubtractNoClip(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, getPixel(x, y) - channel.getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelAverage(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, (getPixel(x, y) + channel.getPixel(x, y))/2f);
				}
			}
			return this;
		}
	
		public Channel channelMultiply(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, getPixel(x, y) * channel.getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelDivide(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, getPixel(x, y) / channel.getPixel(x, y));
				}
			}
			return this;
		}
	
		public Channel channelDifference(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, Math.Abs(getPixel(x, y) - channel.getPixel(x, y)));
				}
			}
			return this;
		}
	
		public Channel channelDarkest(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, Math.Min(getPixel(x, y), channel.getPixel(x, y)));
				}
			}
			return this;
		}
	
		public Channel channelBrightest(Channel channel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					putPixel(x, y, Math.Max(getPixel(x, y), channel.getPixel(x, y)));
				}
			}
			return this;
		}	
	}
}