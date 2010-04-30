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

/*
TODO

rewrite if-check at modulo to (x+n)%n
*/
using System;
using System.Security.Cryptography;

namespace Procedurality
{
	public class Voronoi {
		// coordinate indexing
		public static int X = 0;
		public static int Y = 1;
		public static int SEED = 2;
	
		private int size;
		private Random random;
		private Channel dist1;
		private Channel dist2;
		private Channel dist3;
		private Channel hit;
	
		public Voronoi(int size, int x_domains, int y_domains, int checkradius, float randomness, long seed)
		{
			this.ActuallyDoVoronoi(size, x_domains, y_domains, checkradius, randomness, seed, false);
		}
		
		public Voronoi(int size, int x_domains, int y_domains, int checkradius, float randomness, long seed, bool border) 
		{
			this.ActuallyDoVoronoi(size, x_domains, y_domains, checkradius, randomness, seed, border);
		}
		
		private void ActuallyDoVoronoi(int size, int x_domains, int y_domains, int checkradius, float randomness, long seed, bool border)
		{
			this.size = size;
			x_domains = Math.Max(1, x_domains);
			y_domains = Math.Max(1, y_domains);
			checkradius = Math.Min(Math.Max(1, checkradius), Math.Max(x_domains, y_domains));
			random = new Random((int)seed);
			dist1 = new Channel(size, size);
			dist2 = new Channel(size, size);
			dist3 = new Channel(size, size);
			hit = new Channel(size, size);
			
			// fill in hitpoints according to distribution
			float[,,] domains = new float[x_domains,y_domains,3];
			for (int j = 0; j < y_domains; j++) {
				for (int i = 0; i < x_domains; i++) {
					domains[i,j,X] = (1 - randomness)*((i + .5f)/x_domains) + randomness*((i + (float)random.NextDouble())/x_domains);
					domains[i,j,Y] = (1 - randomness)*((j + .5f)/y_domains) + randomness*((j + (float)random.NextDouble())/y_domains);
					if (border && (j == 0 || j == y_domains-1 || i == 0 || i == x_domains-1)) {
						domains[i,j,SEED] = 0f;
					} else if (border && j != 0 && j != y_domains-1 && i != 0 && i != x_domains-1) {
						domains[i,j,SEED] = 1f;
					} else {
						domains[i,j,SEED] = (float)random.NextDouble();
					}
				}
			}
	
			// fill in pixelvalues
			for (int y = 0; y < size; y++) {
				float y_coord = (float)y/size;
				int j = (int)(y_coord * y_domains);
				for (int x = 0; x < size; x++) {
					float x_coord = (float)x/size;
					float d1 = float.MaxValue;
					float d2 = float.MaxValue;
					float d3 = float.MaxValue;
					float dist = 0;
					float hitpoint = 0;
					int i = (int)(x_coord * x_domains);
	
					// traverse neighboring domains in wrap-around style (for seamless textures)
					for (int l = -checkradius; l <= checkradius; l++) {
						int l_wrap = j + l;
						if (l_wrap < 0 || l_wrap >= y_domains) {
							l_wrap = l_wrap % y_domains;
							if (l_wrap < 0) l_wrap += y_domains;
						}
						for (int k = -checkradius; k <= checkradius; k++) {
	
							// calculate wrapped domain coords
							int k_wrap = i + k;
							if (k_wrap < 0 || k_wrap >= x_domains) {
								k_wrap = k_wrap % x_domains;
								if (k_wrap < 0) k_wrap += x_domains;
							}
							float dx = 0;
							float dy = 0;
	
							// calculate distance to current hit point taking wrap-around into consideration
							if (i + k >= 0 && i + k < x_domains) {
								dx = Math.Abs(domains[k_wrap,l_wrap,X] - x_coord);
							} else if (i + k < 0) {
								dx = Math.Abs(1 - domains[k_wrap,l_wrap,X] + x_coord);
							} else if (i + k >= x_domains) {
								dx = Math.Abs(1 - x_coord + domains[k_wrap,l_wrap,X]);
							}
							if (j + l >= 0 && j + l < y_domains) {
								dy = Math.Abs(domains[k_wrap,l_wrap,Y] - y_coord);
							} else if (j + l < 0) {
								dy = Math.Abs(1 - domains[k_wrap,l_wrap,Y] + y_coord);
							} else if (j + l >= y_domains) {
								dy = Math.Abs(1 - y_coord + domains[k_wrap,l_wrap,Y]);
							}
							dx*=x_domains;
							dy*=y_domains;
	
							// maintain F1, F2, F3 and nearest hitpoint values
							dist = dx*dx + dy*dy;
							if (dist <= d1) {
								d3 = d2;
								d2 = d1;
								d1 = dist;
								hitpoint = domains[k_wrap,l_wrap,SEED];
							} else if (dist <= d2 && dist > d1) {
								d3 = d2;
								d2 = dist;
							} else if (dist <= d3 && dist > d2) {
								d3 = dist;
							}
						}
					}
					dist1.putPixel(x, y, d1);
					dist2.putPixel(x, y, d2);
					dist3.putPixel(x, y, d3);
					hit.putPixel(x, y, hitpoint);
				}
			}
		}
	
		public Channel getDistance(float c1, float c2, float c3) {
			Channel channel = new Channel(size, size);
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					channel.putPixel(x, y, c1*dist1.getPixel(x, y) + c2*dist2.getPixel(x, y) + c3*dist3.getPixel(x, y));
				}
			}
			return channel.normalize();
		}
		
		public Channel getHitpoint() {
			return hit;
		}
		
	}
}