
using System;

namespace Procedurality
{
	public class Crater
	{
		private Channel channel;
		public Crater(int size,int xp,int yp,double radius)
		{
			channel = new Channel(size,size);
			double ratio=64f/64f;
			double craterdepth = 10.0f*ratio;
			double rimheight = 3.0f*ratio;
			double falloff = 60.0f*ratio;
			
			BuildCrater(xp,yp,radius,rimheight,craterdepth,falloff);
		}
		
		public void BuildCrater(int x, int y, double radius, double rimheight, double craterdepth, double falloff)
		{	
			// 128-28 = 100
			int minX=x-(int)Math.Round(radius+falloff);
			int minY=y-(int)Math.Round(radius+falloff);
			// 128+28 = 156
			int maxX=x+(int)Math.Round(radius+falloff);
			int maxY=y+(int)Math.Round(radius+falloff);
			
			// Clamp 
			minX=(minX<0) ? 0 : minX;
			minY=(minY<0) ? 0 : minY;
			maxX=(maxX>256) ? 256 : maxX;
			maxY=(maxY>256) ? 256 : maxY;
			
			//Console.WriteLine(" * Building Crater @ ({0},{1}), BB @ [({2},{3}),({4},{5})]",x,y,minX,minY,maxX,maxY);
			for(int i=minX;i<maxX;i++)
			{
				for(int j=minY;j<maxY;j++)
				{
					double dx = (double)(x-i);
					double dy = (double)(y-j);
					double dist = Math.Sqrt(dx*dx + dy*dy);
					double height=0.0d;
					if(dist<radius){
						height = 1.0d-((dx*dx + dy*dy) / (radius*radius));
						height = rimheight - (height*craterdepth);
					} else if((dist-radius) < falloff) {
						double fallscale = (dist-radius)/falloff;
						height = (1.0d-fallscale) * rimheight;
					}else{
						height = 0.0d;
					}
					if(height > 1d) height = 1d;
					if(height <-1d) height =-1d;
					
					channel.putPixel(i,j,(float)height);
				}
			}
			channel.normalize();
		}
		
		public Channel toChannel()
		{
			return channel;
		}
		public Layer toLayer() {
			return new Layer(channel.copy(), channel.copy(), channel.copy());
		}
	}
}
