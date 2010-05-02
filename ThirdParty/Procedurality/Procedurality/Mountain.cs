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
	public class Mountain 
	{
	
		private Random random;
		public Channel channel;
		
		public Mountain(int size, int base_freq, float pers, long seed)
		{
			this.Gen(size,base_freq,pers,seed,0,0);
		}
		public Mountain(int size, int base_freq, float pers, long seed,int x_o,int y_o) 
		{
			this.Gen(size,base_freq,pers,seed,x_o,y_o);
		}
		private void Gen(int size, int base_freq, float pers, long seed,int x_o,int y_o) {
			if(!Utils.isPowerOf2(size))
				throw new Exception("size must be power of 2");
			
			int iterations = Utils.powerOf2Log2(size);
			base_freq = Math.Max(base_freq, 0);
			base_freq = Math.Min(base_freq, iterations);
			random = new Random((int)seed);
			channel = new Channel(size, size);
	
			int x_block, y_block, x, y;
			
			if (base_freq > 0) {
				int block_size = size>>base_freq;
				for (int x_b = 0; x_b < (1<<base_freq); x_b++) {
					for (int y_b = 0; y_b < (1<<base_freq); y_b++) {
						x = x_b*block_size;
						y = y_b*block_size;
						channel.putPixel(x, y, (float)random.NextDouble());
					}
				}
			}
	
			float v1, v2, v3, v4, v5, v6, v7, v8, v9;
			
	
			for (int i = base_freq; i < iterations; i++) {
				int block_size = size>>i;
				int block_size_half = size>>(i + 1);
				float amp = (float)Math.Pow(pers, i - base_freq);
				float amp_half = 0.5f*amp;
				float avr;
				// calculate center midpoints
				if (i < 2) {
					for (x_block = 0, x = 0; x_block < (1<<i); x_block++) {
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
							v2 = channel.getPixel((x + block_size) % size, y);
							v3 = channel.getPixel(x, (y + block_size) % size);
							v4 = channel.getPixel((x + block_size) % size, (y + block_size) % size);
							avr = 0.25f*(v1 + v2 + v3 + v4);
							v5 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
							y+= block_size;
						}
						x+= block_size;
					}
				} else {
					// safe blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 1, y = block_size; y_block < (1<<i) - 1; y_block++) {
							v1 = channel.getPixel(x, y);
							v2 = channel.getPixel(x + block_size, y);
							v3 = channel.getPixel(x, y + block_size);
							v4 = channel.getPixel(x + block_size, y + block_size);
							avr = 0.25f*(v1 + v2 + v3 + v4);
							v5 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
							y+= block_size;
						}
						x+= block_size;
					}
					// left and right edge blocks
					for (x_block = 0; x_block < (1<<i); x_block+= (1<<i) - 1) {
						x = x_block*block_size;
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
							v2 = channel.getPixel((x + block_size) % size, y);
							v3 = channel.getPixel(x, (y + block_size) % size);
							v4 = channel.getPixel((x + block_size) % size, (y + block_size) % size);
							avr = 0.25f*(v1 + v2 + v3 + v4);
							v5 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
							y+= block_size;
						}
					}
					// top and bottom edge blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 0; y_block < (1<<i); y_block+= (1<<i) - 1) {
							y = y_block*block_size;
							v1 = channel.getPixel(x, y);
							v2 = channel.getPixel((x + block_size) % size, y);
							v3 = channel.getPixel(x, (y + block_size) % size);
							v4 = channel.getPixel((x + block_size) % size, (y + block_size) % size);
							avr = 0.25f*(v1 + v2 + v3 + v4);
							v5 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x + block_size_half, y + block_size_half, v5);
						}
						x+= block_size;
					}
				}
				// calculate left and bottom edge midpoints
				if (i < 2) {
					for (x_block = 0, x = 0; x_block < (1<<i); x_block++) {
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
							v2 = channel.getPixel((x + block_size) % size, y);
							v3 = channel.getPixel(x, (y + block_size) % size);
							v6 = channel.getPixel(((x - block_size_half) + size) % size, (y + block_size_half) % size);
							v7 = channel.getPixel((x + block_size_half) % size, ((y - block_size_half) + size) % size);
							avr = 0.25f*(v1 + v3 + v5 + v6);
							v8 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							avr = 0.25f*(v1 + v2 + v5 + v7);
							v9 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
							y+= block_size;
						}
						x+= block_size;
					}
				} else {
					// safe blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 1, y = block_size; y_block < (1<<i) - 1; y_block++) {
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
							v2 = channel.getPixel(x + block_size, y);
							v3 = channel.getPixel(x, y + block_size);
							v6 = channel.getPixel(x - block_size_half, y + block_size_half);
							v7 = channel.getPixel(x + block_size_half, y - block_size_half);
							avr = 0.25f*(v1 + v3 + v5 + v6);
							v8 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							avr = 0.25f*(v1 + v2 + v5 + v7);
							v9 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
							y+= block_size;
						}
						x+= block_size;
					}
					// left and right edge blocks
					for (x_block = 0; x_block < (1<<i); x_block+= (1<<i) - 1) {
						x = x_block*block_size;
						for (y_block = 0, y = 0; y_block < (1<<i); y_block++) {
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
							v2 = channel.getPixel((x + block_size) % size, y);
							v3 = channel.getPixel(x, (y + block_size) % size);
							v6 = channel.getPixel(((x - block_size_half) + size) % size, (y + block_size_half) % size);
							v7 = channel.getPixel((x + block_size_half) % size, ((y - block_size_half) + size) % size);
							avr = 0.25f*(v1 + v3 + v5 + v6);
							v8 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							avr = 0.25f*(v1 + v2 + v5 + v7);
							v9 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
							y+= block_size;
						}
					}
					// top and bottom edge blocks
					for (x_block = 1, x = block_size; x_block < (1<<i) - 1; x_block++) {
						for (y_block = 0; y_block < (1<<i); y_block+= (1<<i) - 1) {
							y = y_block*block_size;
							v1 = channel.getPixel(x, y);
							v5 = channel.getPixel(x + block_size_half, y + block_size_half);
							v2 = channel.getPixel((x + block_size) % size, y);
							v3 = channel.getPixel(x, (y + block_size) % size);
							v6 = channel.getPixel(((x - block_size_half) + size) % size, (y + block_size_half) % size);
							v7 = channel.getPixel((x + block_size_half) % size, ((y - block_size_half) + size) % size);
							avr = 0.25f*(v1 + v3 + v5 + v6);
							v8 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							avr = 0.25f*(v1 + v2 + v5 + v7);
							v9 = avr*(1f + (float)random.NextDouble()*amp - amp_half);
							channel.putPixel(x, y + block_size_half, v8);
							channel.putPixel(x + block_size_half, y, v9);
						}
						x+= block_size;
					}
				}
			}
			float[] mm=channel.findMinMax();
			Console.WriteLine("Range Mountain->[{0},{1}]",mm[0],mm[1]);
			channel.normalize();
			mm=channel.findMinMax();
			Console.WriteLine("Range Mountain->[{0},{1}]",mm[0],mm[1]);
		}
	
		public Layer toLayer() {
			return new Layer(channel, channel.copy(), channel.copy());
		}
	
		public Channel toChannel() {
			return channel;
		}
	
	}
}