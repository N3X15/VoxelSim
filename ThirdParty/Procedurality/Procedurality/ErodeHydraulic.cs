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
	public class ErosionHydraulic 
	{
	
		public static Channel erode1(Channel channel, Channel rain_map, float vaporization, int rain_freq, int iterations) 
		{
			Channel water_map = new Channel(channel.width, channel.height).fill(0f);
			Channel vapor_map = rain_map.copy().multiply(0.5f);
			Channel water_map_diff = new Channel(channel.width, channel.height).fill(0f);
	
			Console.Write("Hydraulic erosion 1: ");
	
			for (int i = 0; i < iterations; i++) {
	
				Console.Write(".");
	
				// save frames
				/*
				if (channel.width > 128 && i%10 == 0) {
					if (i < 10) {
						channel.toLayer().saveAsPNG("erosion00" + i);
					} else if (i < 100) {
						channel.toLayer().saveAsPNG("erosion0" + i);
					} else {
						channel.toLayer().saveAsPNG("erosion" + i);
					}
				}
				*/
	
				// rain erodes the underlying terrain
				if (i%rain_freq == 0) {
					channel.channelSubtract(vapor_map);
					water_map.channelAdd(rain_map);
				}
	
				// water and sediment transport
				for (int y = 1; y < channel.height - 1; y++) {
					for (int x = 1; x < channel.width - 1; x++) {
	
						// calculate total heights and height differences
						float h = channel.getPixel(x, y) + water_map.getPixel(x, y);
	
						float h1 = channel.getPixel(x, y + 1) + water_map.getPixel(x, y + 1);
						float h2 = channel.getPixel(x - 1, y) + water_map.getPixel(x - 1, y);
						float h3 = channel.getPixel(x + 1, y) + water_map.getPixel(x + 1, y);
						float h4 = channel.getPixel(x, y - 1) + water_map.getPixel(x, y - 1);
						float h5 = channel.getPixel(x - 1, y + 1) + water_map.getPixel(x - 1, y + 1);
						float h6 = channel.getPixel(x + 1, y + 1) + water_map.getPixel(x + 1, y + 1);
						float h7 = channel.getPixel(x - 1, y - 1) + water_map.getPixel(x - 1, y - 1);
						float h8 = channel.getPixel(x + 1, y - 1) + water_map.getPixel(x + 1, y - 1);
	
						float d1 = h - h1;
						float d2 = h - h2;
						float d3 = h - h3;
						float d4 = h - h4;
						float d5 = h - h5;
						float d6 = h - h6;
						float d7 = h - h7;
						float d8 = h - h8;
	
						// calculate amount of water to transport
						float total_height = 0;
						float total_height_diff = 0;
						int cells = 1;
	
						if (d1 > 0) {
							total_height_diff+= d1;
							total_height+= h1;
							cells++;
						}
						if (d2 > 0) {
							total_height_diff+= d2;
							total_height+= h2;
							cells++;
						}
						if (d3 > 0) {
							total_height_diff+= d3;
							total_height+= h3;
							cells++;
						}
						if (d4 > 0) {
							total_height_diff+= d4;
							total_height+= h4;
							cells++;
						}
						if (d5 > 0) {
							total_height_diff+= d5;
							total_height+= h5;
							cells++;
						}
						if (d6 > 0) {
							total_height_diff+= d6;
							total_height+= h6;
							cells++;
						}
						if (d7 > 0) {
							total_height_diff+= d7;
							total_height+= h7;
							cells++;
						}
						if (d8 > 0) {
							total_height_diff+= d8;
							total_height+= h8;
							cells++;
						}
	
						if (cells == 1) {
							continue;
						}
	
						float avr_height = total_height/cells;
						float water_amount = Math.Min(water_map.getPixel(x, y), h - avr_height);
						water_map_diff.putPixel(x, y, water_map_diff.getPixel(x, y) - water_amount);
						float total_height_diff_inv = water_amount/total_height_diff;
	
						// transport water
						if (d1 > 0) {
							water_map_diff.putPixel(x, y + 1, water_map_diff.getPixel(x, y + 1) + d1*total_height_diff_inv);
						}
						if (d2 > 0) {
							water_map_diff.putPixel(x - 1, y, water_map_diff.getPixel(x - 1, y) + d2*total_height_diff_inv);
						}
						if (d3 > 0) {
							water_map_diff.putPixel(x + 1, y, water_map_diff.getPixel(x + 1, y) + d3*total_height_diff_inv);
						}
						if (d4 > 0) {
							water_map_diff.putPixel(x, y - 1, water_map_diff.getPixel(x, y - 1) + d4*total_height_diff_inv);
						}
						if (d5 > 0) {
							water_map_diff.putPixel(x - 1, y + 1, water_map_diff.getPixel(x - 1, y + 1) + d5*total_height_diff_inv);
						}
						if (d6 > 0) {
							water_map_diff.putPixel(x + 1, y + 1, water_map_diff.getPixel(x + 1, y + 1) + d6*total_height_diff_inv);
						}
						if (d7 > 0) {
							water_map_diff.putPixel(x - 1, y - 1, water_map_diff.getPixel(x - 1, y - 1) + d7*total_height_diff_inv);
						}
						if (d8 > 0) {
							water_map_diff.putPixel(x + 1, y - 1, water_map_diff.getPixel(x + 1, y - 1) + d8*total_height_diff_inv);
						}
					}
				}
	
				// apply changes to water_map
				water_map.channelAddNoClip(water_map_diff);
				water_map_diff.fill(0f);
	
				// vaporize water
				channel.channelAddNoClip(water_map.copy().channelSubtract(water_map.addClip(-vaporization)).multiply(0.5f));
			}
	
			// force evaporation of remaining water
			//channel.channelAdd(water_map.multiply(0.5f));
			
			Console.WriteLine("DONE");
			
			return channel;
		}
	
		public static Channel erode2(Channel channel, Channel rain_map, float vaporization, int rain_freq, int iterations) {
			Channel water_map = new Channel(channel.width, channel.height).fill(0f);
			Channel vapor_map = rain_map.copy().multiply(0.5f);
			Channel water_map_diff = new Channel(channel.width, channel.height).fill(0f);
	
			Console.Write("Hydraulic erosion 2: ");
	
			for (int i = 0; i < iterations; i++) {
			
				Console.Write(".");
	
				// save frames
				/*
				if (channel.width > 128 && i%10 == 0) {
					if (i < 10) {
						channel.toLayer().saveAsPNG("erosion00" + i);
					} else if (i < 100) {
						channel.toLayer().saveAsPNG("erosion0" + i);
					} else {
						channel.toLayer().saveAsPNG("erosion" + i);
					}
				}
				*/
	
				// rain erodes the underlying terrain
				if (i%rain_freq == 0) {
					channel.channelSubtract(vapor_map);
					water_map.channelAdd(rain_map);
				}
	
				// water and sediment transport
				for (int y = 1; y < channel.height - 1; y++) {
					for (int x = 1; x < channel.width - 1; x++) {
	
						// calculate total heights and height differences
						float h = channel.getPixel(x, y) + water_map.getPixel(x, y);
	
						float h1 = channel.getPixel(x, y + 1) + water_map.getPixel(x, y + 1);
						float h2 = channel.getPixel(x - 1, y) + water_map.getPixel(x - 1, y);
						float h3 = channel.getPixel(x + 1, y) + water_map.getPixel(x + 1, y);
						float h4 = channel.getPixel(x, y - 1) + water_map.getPixel(x, y - 1);
	
						float d1 = h - h1;
						float d2 = h - h2;
						float d3 = h - h3;
						float d4 = h - h4;
	
						// calculate amount of water to transport
						float total_height = 0;
						float total_height_diff = 0;
						int cells = 1;
	
						if (d1 > 0) {
							total_height_diff+= d1;
							total_height+= h1;
							cells++;
						}
						if (d2 > 0) {
							total_height_diff+= d2;
							total_height+= h2;
							cells++;
						}
						if (d3 > 0) {
							total_height_diff+= d3;
							total_height+= h3;
							cells++;
						}
						if (d4 > 0) {
							total_height_diff+= d4;
							total_height+= h4;
							cells++;
						}
	
						if (cells == 1) {
							continue;
						}
	
						float avr_height = total_height/cells;
						float water_amount = Math.Min(water_map.getPixel(x, y), h - avr_height);
						water_map_diff.putPixel(x, y, water_map_diff.getPixel(x, y) - water_amount);
						float total_height_diff_inv = water_amount/total_height_diff;
	
						// transport water
						if (d1 > 0) {
							water_map_diff.putPixel(x, y + 1, water_map_diff.getPixel(x, y + 1) + d1*total_height_diff_inv);
						}
						if (d2 > 0) {
							water_map_diff.putPixel(x - 1, y, water_map_diff.getPixel(x - 1, y) + d2*total_height_diff_inv);
						}
						if (d3 > 0) {
							water_map_diff.putPixel(x + 1, y, water_map_diff.getPixel(x + 1, y) + d3*total_height_diff_inv);
						}
						if (d4 > 0) {
							water_map_diff.putPixel(x, y - 1, water_map_diff.getPixel(x, y - 1) + d4*total_height_diff_inv);
						}
					}
				}
	
				// apply changes to water_map
				water_map.channelAddNoClip(water_map_diff);
				water_map_diff.fill(0f);
	
				// vaporize water
				channel.channelAddNoClip(water_map.copy().channelSubtract(water_map.addClip(-vaporization)).multiply(0.5f));
			}
	
			// force evaporation of remaining water
			channel.channelAdd(water_map.multiply(0.5f));
			
			Console.WriteLine("DONE");
			
			return channel;
		}
	
		public static Channel erode3(Channel channel, Channel rain_map, float vaporization, int rain_freq, int iterations) {
			Channel vapor_map = rain_map.copy().multiply(0.5f);
			Channel height_map_diff = new Channel(channel.width, channel.height).fill(0f);
			Channel water_map = new Channel(channel.width, channel.height).fill(0f);
			Channel water_map_diff = new Channel(channel.width, channel.height).fill(0f);
			Channel sediment_map = new Channel(channel.width, channel.height).fill(0f);
			Channel sediment_map_diff = new Channel(channel.width, channel.height).fill(0f);
	
			Console.Write("Hydraulic erosion 3: ");
	
			for (int i = 0; i < iterations; i++) {
			
				Console.Write(".");
	
				// save frames
				/*
				if (channel.width > 128 && i%8 == 0) {
					if (i < 10) {
						channel.toLayer().saveAsPNG("erosion00" + i);
					} else if (i < 100) {
						channel.toLayer().saveAsPNG("erosion0" + i);
					} else {
						channel.toLayer().saveAsPNG("erosion" + i);
					}
				}
				*/
	
				// rain
				if (i%rain_freq == 0) {
					water_map.channelAdd(rain_map);
				}
	
				// water and sediment transport
				for (int y = 1; y < channel.height - 1; y++) {
					for (int x = 1; x < channel.width - 1; x++) {
	
						// calculate total heights and height differences
						float h = channel.getPixel(x, y) + water_map.getPixel(x, y);
	
						float h1 = channel.getPixel(x, y + 1) + water_map.getPixel(x, y + 1) + sediment_map.getPixel(x, y + 1);
						float h2 = channel.getPixel(x - 1, y) + water_map.getPixel(x - 1, y) + sediment_map.getPixel(x - 1, y);
						float h3 = channel.getPixel(x + 1, y) + water_map.getPixel(x + 1, y) + sediment_map.getPixel(x + 1, y);
						float h4 = channel.getPixel(x, y - 1) + water_map.getPixel(x, y - 1) + sediment_map.getPixel(x, y - 1);
	
						float d1 = h - h1;
						float d2 = h - h2;
						float d3 = h - h3;
						float d4 = h - h4;
	
						// calculate amount of water and sediment to transport
						float total_height = 0;
						float total_height_diff = 0;
						int cells = 1;
	
						if (d1 > 0) {
							total_height_diff+= d1;
							total_height+= h1;
							cells++;
						}
						if (d2 > 0) {
							total_height_diff+= d2;
							total_height+= h2;
							cells++;
						}
						if (d3 > 0) {
							total_height_diff+= d3;
							total_height+= h3;
							cells++;
						}
						if (d4 > 0) {
							total_height_diff+= d4;
							total_height+= h4;
							cells++;
						}
	
						if (cells == 1) {
							continue;
						}
	
						float avr_height = total_height/cells;
	
						float water_amount = Math.Min(water_map.getPixel(x, y), h - avr_height);
						water_map_diff.putPixel(x, y, water_map_diff.getPixel(x, y) - water_amount);
						float water_inv = water_amount/total_height_diff;
	
						float sediment_amount = sediment_map.getPixel(x, y);
						sediment_map_diff.putPixel(x, y, sediment_map_diff.getPixel(x, y) - sediment_amount);
						float sediment_inv = sediment_amount/total_height_diff;
	
						float dissolve;
	
						// transport water and sediment and dissolve more material
						if (d1 > 0) {
							water_map_diff.putPixel(x, y + 1, water_map_diff.getPixel(x, y + 1) + d1*water_inv);
							dissolve = 10f*d1*water_amount;
							sediment_map_diff.putPixel(x, y + 1, sediment_map_diff.getPixel(x, y + 1) + d1*sediment_inv + dissolve);
							height_map_diff.putPixel(x, y + 1, height_map_diff.getPixel(x, y + 1) - dissolve);
						}
						if (d2 > 0) {
							water_map_diff.putPixel(x - 1, y, water_map_diff.getPixel(x - 1, y) + d2*water_inv);
							dissolve = 10f*d2*water_amount;
							sediment_map_diff.putPixel(x - 1, y, sediment_map_diff.getPixel(x - 1, y) + d2*sediment_inv + dissolve);
							height_map_diff.putPixel(x - 1, y, height_map_diff.getPixel(x - 1, y) - dissolve);
						}
						if (d3 > 0) {
							water_map_diff.putPixel(x + 1, y, water_map_diff.getPixel(x + 1, y) + d3*water_inv);
							dissolve = 10f*d3*water_amount;
							sediment_map_diff.putPixel(x + 1, y, sediment_map_diff.getPixel(x + 1, y) + d3*sediment_inv + dissolve);
							height_map_diff.putPixel(x + 1, y, height_map_diff.getPixel(x + 1, y) - dissolve);
						}
						if (d4 > 0) {
							water_map_diff.putPixel(x, y - 1, water_map_diff.getPixel(x, y - 1) + d4*water_inv);
							dissolve = 10f*d4*water_amount;
							sediment_map_diff.putPixel(x, y - 1, sediment_map_diff.getPixel(x, y - 1) + d4*sediment_inv + dissolve);
							height_map_diff.putPixel(x, y - 1, height_map_diff.getPixel(x, y - 1) - dissolve);
						}
					}
				}
	
				// apply changes to water map
				water_map.channelAddNoClip(water_map_diff);
	
				// apply changes to sediment map
				sediment_map.channelAddNoClip(sediment_map_diff);
	
				// apply changes to height map
				channel.channelAddNoClip(height_map_diff);
	
				// water vaporization
				water_map.addClip(-vaporization);
	
				// sedimentation
				sediment_map_diff = sediment_map.copy().channelSubtract(water_map);
				sediment_map.channelSubtract(sediment_map_diff);
				channel.channelAddNoClip(sediment_map_diff);
	
				// clear diff maps
				water_map_diff.fill(0f);
				height_map_diff.fill(0f);
				sediment_map_diff.fill(0f);
			}
	
			// force evaporation of remaining water
			//channel.channelAdd(water_map.multiply(0.5f));
			
			Console.WriteLine("DONE");
			
			return channel;
		}
	
		public static Channel erode4(Channel channel, float rain_amount, float vaporization, int rain_freq, int iterations) {
			Channel water_map = new Channel(channel.width, channel.height).fill(0f);
			Channel water_map_diff = new Channel(channel.width, channel.height).fill(0f);
			Channel height_map_diff = new Channel(channel.width, channel.height).fill(0f);
	
			Console.Write("Hydraulic erosion 4: ");
	
			for (int i = 0; i < iterations; i++) {
			
				Console.Write(".");
	
				// save frames
				/*
				if (channel.width > 128 && i%10 == 0) {
					if (i < 10) {
						channel.toLayer().saveAsPNG("erosion00" + i);
					} else if (i < 100) {
						channel.toLayer().saveAsPNG("erosion0" + i);
					} else {
						channel.toLayer().saveAsPNG("erosion" + i);
					}
				}
				*/
	
				// rain erodes the underlying terrain
				if (i%rain_freq == 0) {
					water_map.channelAdd(channel.copy().multiply(rain_amount));
				}
	
				// water and sediment transport
				for (int y = 1; y < channel.height - 1; y++) {
					for (int x = 1; x < channel.width - 1; x++) {
	
						// calculate total heights and height differences
						float h = channel.getPixel(x, y) + water_map.getPixel(x, y);
	
						float h1 = channel.getPixel(x, y + 1) + water_map.getPixel(x, y + 1);
						float h2 = channel.getPixel(x - 1, y) + water_map.getPixel(x - 1, y);
						float h3 = channel.getPixel(x + 1, y) + water_map.getPixel(x + 1, y);
						float h4 = channel.getPixel(x, y - 1) + water_map.getPixel(x, y - 1);
	
						float d1 = h - h1;
						float d2 = h - h2;
						float d3 = h - h3;
						float d4 = h - h4;
	
						// calculate amount of water to transport
						float total_height = 0;
						float total_height_diff = 0;
						int cells = 1;
	
						if (d1 > 0) {
							total_height_diff+= d1;
							total_height+= h1;
							cells++;
						}
						if (d2 > 0) {
							total_height_diff+= d2;
							total_height+= h2;
							cells++;
						}
						if (d3 > 0) {
							total_height_diff+= d3;
							total_height+= h3;
							cells++;
						}
						if (d4 > 0) {
							total_height_diff+= d4;
							total_height+= h4;
							cells++;
						}
	
						if (cells == 1) {
							continue;
						}
	
						float avr_height = total_height/cells;
						float water_amount = Math.Min(water_map.getPixel(x, y), h - avr_height);
						water_map_diff.putPixel(x, y, water_map_diff.getPixel(x, y) - water_amount);
						float total_height_diff_inv = water_amount/total_height_diff;
	
						// transport water
						if (d1 > 0) {
							water_amount = d1*total_height_diff_inv;
							water_map_diff.putPixel(x, y + 1, water_map_diff.getPixel(x, y + 1) + water_amount);
							height_map_diff.putPixel(x, y + 1, height_map_diff.getPixel(x, y + 1) - 0.1f*water_amount);
						}
						if (d2 > 0) {
							water_amount = d2*total_height_diff_inv;
							water_map_diff.putPixel(x - 1, y, water_map_diff.getPixel(x - 1, y) + water_amount);
							height_map_diff.putPixel(x - 1, y, height_map_diff.getPixel(x - 1, y) - 0.1f*water_amount);
						}
						if (d3 > 0) {
							water_amount = d3*total_height_diff_inv;
							water_map_diff.putPixel(x + 1, y, water_map_diff.getPixel(x + 1, y) + water_amount);
							height_map_diff.putPixel(x + 1, y, height_map_diff.getPixel(x + 1, y) - 0.1f*water_amount);
						}
						if (d4 > 0) {
							water_amount = d4*total_height_diff_inv;
							water_map_diff.putPixel(x, y - 1, water_map_diff.getPixel(x, y - 1) + water_amount);
							height_map_diff.putPixel(x, y - 1, height_map_diff.getPixel(x, y - 1) - 0.1f*water_amount);
						}
					}
				}
	
				// apply changes to water map
				water_map.channelAddNoClip(water_map_diff);
				water_map_diff.fill(0f);
	
				// apply changes to height map
				channel.channelAddNoClip(height_map_diff);
				height_map_diff.fill(0f);
	
				// vaporize water
				channel.channelAddNoClip(water_map.copy().channelSubtract(water_map.addClip(-vaporization)).multiply(0.5f));
			}
	
			// force evaporation of remaining water
			channel.channelAdd(water_map.multiply(0.5f));
			
			Console.WriteLine("DONE");
			
			return channel;
		}
	
		public static Channel erode5(Channel channel, Channel rain, float erosion_water, float erosion_flow, float evaporation, float water_threshold, float solulibility, int ipr, int iterations) {
			Channel w  = new Channel(channel.width, channel.height); // water map
			Channel dw = new Channel(channel.width, channel.height); // delta water map
			Channel s  = new Channel(channel.width, channel.height); // sediment map
			Channel ds = new Channel(channel.width, channel.height); // delta sediment map
	
			Console.Write("Hydraulic erosion 5: ");
	
			for (int i = 0; i < iterations; i++) {
			
				Console.Write(".");
	
				// save frames
				/*
				if (channel.width > 128 && i%10 == 0) {
					if (i < 10) {
						channel.toLayer().saveAsPNG("erosion00" + i);
					} else if (i < 100) {
						channel.toLayer().saveAsPNG("erosion0" + i);
					} else {
						channel.toLayer().saveAsPNG("erosion" + i);
					}
				}
				*/
	
				// water is added according to rain map
				if (i%ipr == 0) {
					w.channelAdd(rain);
				}
	
				// the presence of water dissolves material
				channel.channelSubtract(w.copy().multiply(erosion_water));
				s.channelAdd(w.copy().multiply(erosion_water));
	
				// water and sediment are transported
				float h, h1, h2, h3, h4, d1, d2, d3, d4, total_height, total_height_diff, total_height_diff_inv, avr_height, water_amount, sediment_amount;
				int cells;
				for (int y = 0; y < channel.height; y++) {
					for (int x = 0; x < channel.width; x++) {
	
						// water transport
						// calculate total heights and height differences
						h = channel.getPixel(x, y) + w.getPixel(x, y) + s.getPixel(x, y);
	
						h1 = channel.getPixelWrap(x    , y + 1) + w.getPixelWrap(x    , y + 1) + s.getPixelWrap(x    , y + 1);
						h2 = channel.getPixelWrap(x - 1, y    ) + w.getPixelWrap(x - 1, y    ) + s.getPixelWrap(x - 1, y    );
						h3 = channel.getPixelWrap(x + 1, y    ) + w.getPixelWrap(x + 1, y    ) + s.getPixelWrap(x + 1, y    );
						h4 = channel.getPixelWrap(x    , y - 1) + w.getPixelWrap(x    , y - 1) + s.getPixelWrap(x    , y - 1);
	
						d1 = h - h1;
						d2 = h - h2;
						d3 = h - h3;
						d4 = h - h4;
	
						// calculate amount of water to transport
						total_height = 0f;
						total_height_diff = 0f;
						cells = 1;
	
						if (d1 > 0) {
							total_height_diff+= d1;
							total_height+= h1;
							cells++;
						}
						if (d2 > 0) {
							total_height_diff+= d2;
							total_height+= h2;
							cells++;
						}
						if (d3 > 0) {
							total_height_diff+= d3;
							total_height+= h3;
							cells++;
						}
						if (d4 > 0) {
							total_height_diff+= d4;
							total_height+= h4;
							cells++;
						}
	
						if (cells == 1) {
							continue;
						}
	
						avr_height = total_height/cells;
						water_amount = Math.Min(w.getPixel(x, y), h - avr_height);
						dw.putPixel(x, y, dw.getPixel(x, y) - water_amount);
						total_height_diff_inv = water_amount/total_height_diff;
	
						// transport water
						if (d1 > 0) {
							dw.putPixelWrap(x, y + 1, dw.getPixelWrap(x, y + 1) + d1*total_height_diff_inv);
						}
						if (d2 > 0) {
							dw.putPixelWrap(x - 1, y, dw.getPixelWrap(x - 1, y) + d2*total_height_diff_inv);
						}
						if (d3 > 0) {
							dw.putPixelWrap(x + 1, y, dw.getPixelWrap(x + 1, y) + d3*total_height_diff_inv);
						}
						if (d4 > 0) {
							dw.putPixelWrap(x, y - 1, dw.getPixelWrap(x, y - 1) + d4*total_height_diff_inv);
						}
	
						// sediment transport
						/*
						h = s.getPixel(x, y);
	
						h1 = s.getPixelWrap(x    , y + 1);
						h2 = s.getPixelWrap(x - 1, y    );
						h3 = s.getPixelWrap(x + 1, y    );
						h4 = s.getPixelWrap(x    , y - 1);
	
						d1 = h - h1;
						d2 = h - h2;
						d3 = h - h3;
						d4 = h - h4;
	
						// calculate amount of sediment to transport
						total_height = 0f;
						total_height_diff = 0f;
						cells = 1;
	
						if (d1 > 0) {
							total_height_diff+= d1;
							total_height+= h1;
							cells++;
						}
						if (d2 > 0) {
							total_height_diff+= d2;
							total_height+= h2;
							cells++;
						}
						if (d3 > 0) {
							total_height_diff+= d3;
							total_height+= h3;
							cells++;
						}
						if (d4 > 0) {
							total_height_diff+= d4;
							total_height+= h4;
							cells++;
						}
	
						if (cells == 1) {
							continue;
						}
	
						avr_height = total_height/cells;
						sediment_amount = Math.Min(s.getPixel(x, y), h - avr_height);
						ds.putPixel(x, y, ds.getPixel(x, y) - sediment_amount);
						total_height_diff_inv = sediment_amount/total_height_diff;
	
						// transport sediment
						if (d1 > 0) {
							ds.putPixelWrap(x, y + 1, ds.getPixelWrap(x, y + 1) + d1*total_height_diff_inv);
						}
						if (d2 > 0) {
							ds.putPixelWrap(x - 1, y, ds.getPixelWrap(x - 1, y) + d2*total_height_diff_inv);
						}
						if (d3 > 0) {
							ds.putPixelWrap(x + 1, y, ds.getPixelWrap(x + 1, y) + d3*total_height_diff_inv);
						}
						if (d4 > 0) {
							ds.putPixelWrap(x, y - 1, ds.getPixelWrap(x, y - 1) + d4*total_height_diff_inv);
						}
						*/
					}
				}
	
				// more sediment is dissolved according to amount of water flow
				/*
				channel.channelSubtract(dw.copy().fill(0f, Float.MIN_VALUE, 0f).multiply(erosion_flow));
				s.channelAdd(dw.copy().fill(0f, Float.MIN_VALUE, 0f).multiply(erosion_flow));
				*/
	
				// apply water and sediment delta maps
				w.channelAdd(dw);
				//w.fill(0f, Float.MIN_VALUE, water_threshold); // remove water below threshold amount
				s.channelAdd(ds);
				dw.fill(0f);
				ds.fill(0f);
	
				// water evaporates
				w.multiply(evaporation);
	
				// sediment is deposited
				for (int y = 0; y < channel.height; y++) {
					for (int x = 0; x < channel.width; x++) {
						float deposition = s.getPixel(x, y) - w.getPixel(x, y)*solulibility;
						if (deposition > 0) {
							s.putPixel(x, y, s.getPixel(x, y) - deposition);
							channel.putPixel(x, y, channel.getPixel(x, y) + deposition);
						}
					}
				}
			}
			
			Console.WriteLine("DONE");
			
			return channel;
		}
	
	}
}