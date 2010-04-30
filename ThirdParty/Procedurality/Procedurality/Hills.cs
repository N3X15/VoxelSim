
using System;

namespace Procedurality
{
	public class Hill
	{
		private Channel channel;
		public Hill(int size,int xp,int yp,float radius)
		{
			double r=(double)radius;
			channel = new Channel(size,size);
			double hill_height=5.0d*(radius/40.0d);
			for(int x=0;x<size;x++)
			{
				for(int y=0;y<size;y++)
				{
					double dx = (double)(yp-x);
					double dy = (double)(xp-y);
					double dist = Math.Sqrt(dx*dx + dy*dy);
					if(dist<radius)
					{
						double height = 1.0d-((dx*dx + dy*dy) / (r*r));
						height = (height*hill_height);
						channel.putPixel(x,y,Convert.ToSingle(height));//$radius^2 + (($x-$hx)^2 - ($y-(256-$hy))^2);
					}
					//channel.putPixel(x,y,(radius*radius) + (((x-dx)*(x-dx)) - ((y-dy)*(y-dy))));
				}
			}
			channel.normalize();
		}
		
		public Channel toChannel()
		{
			return channel;
		}
		public Layer toLayer() {
			return new Layer(channel, channel.copy(), channel.copy());
		}
	}
}
