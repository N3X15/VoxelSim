
using System;
using System.Collections;
using System.Collections.Generic;
using Algorithms;
//using Theodis.Algorithm;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Procedurality
{
	public class RiverBuilder
	{
		public Channel chan;
		public Point Start;
		public Point End;
		
		public Dijkstra Pathfinder;
		
		public List<Point> AllPossibleNodes = new List<Point>();
		private int[,] DistanceMap;
		public List<Point> RiverNodes = new List<Point>();
		
		public RiverBuilder(Channel channel)
		{
			Directions = new int[,]{
				{-1, 1},{ 0, 1},{ 1, 1},
				{-1, 0},        { 1, 0},
				{-1,-1},{ 0,-1},{ 1,-1}
			};
			
			chan=channel;
			traversalCost=GetTraversalCost();
			Console.WriteLine(" * River generator initialized with a {0}x{1} grid.",chan.Height,chan.Width);
		}
		
		int[,] traversalCost; 
		protected int relevantPixelsCount 
		{ 
			get { return chan.Width * chan.Height; } 
		} 
		
		private Rectangle GetRelevantRegion(Point start, Point finish) 
		{ 
			const int minimumSpace = 5; 
			const float expansion = 0.01F; 
			
			Rectangle rect = Rectangle.FromLTRB( 
				Math.Min(start.X, finish.X), 
				Math.Min(start.Y, finish.Y), 
				Math.Max(start.X, finish.X), 
				Math.Max(start.Y, finish.Y) 
			); 
			rect.Inflate(Math.Max((int)(rect.Width * expansion), minimumSpace), 
			Math.Max((int)(rect.Height * expansion), minimumSpace)); 
			// Make sure our relevant region stays within the bounds or calculating a gradient. 
			rect.Intersect(Rectangle.FromLTRB(1, 1, chan.Width - 1, chan.Height - 1)); 
			//Debug.Assert(rect.Contains(start), "Relevant region does not contain start point."); 
			//Debug.Assert(rect.Contains(finish), "Relevant region does not contain finish point."); 
			return rect; 
		} 
	   
		private int GetArrayIndex(Point point) 
		{ 
			if (!IsOnMap(point.X,point.Y)) 
			{
				Console.WriteLine("*** {0} is outside region borders!",point);
				return -1; 
			}
			Point offset = point; 
			return offset.Y * chan.Width + offset.X; 
	   	} 
	   	private Point GetPointFromArrayIndex(int index) 
	   	{ 
			Point point = new Point(index % chan.Width, index / chan.Width); 
			return point; 
	   	} 
	   
		private int[] GetPixelWeights() 
		{ 
			int[] weights = new int[relevantPixelsCount]; 
			for (int i = 0; i < weights.Length; i++) 
			weights[i] = GetPixelWeight(GetPointFromArrayIndex(i)); 
			return weights; 
		} 
			
		private int GetPixelWeight(Point p)
		{
			return (int)chan.getPixel(p.X,p.Y);
		}
	
	const int maximumNearbyPositions = 8; 
	
	int[,] Directions;
	enum NearbyPosition : int 
	{ 
			NorthWest,	// (-1, 1)
			North,		// (0,  1)
			NorthEast,	// (1,  1)
			West,		// (-1, 0)
			East,		// (1,  0)
			SouthWest,
			South,
			SouthEast
	} 
	private int GetNearbyPixel(int origin, NearbyPosition relative) 
	{ 
		return GetArrayIndex(GetNearbyPixel(GetPointFromArrayIndex(origin), relative)); 
	} 
	
	Point LastPoint=new Point(-1,-1);
	int LastPointCount=0;
	private Point GetNearbyPixel(Point origin, NearbyPosition relative) 
	{ 
			if(LastPoint==origin)
			{
				LastPointCount++;
				//Console.WriteLine("Stuck on {0} for {1} ticks!",origin,LastPointCount);
				if(LastPointCount>10)
				{
					Random r = new Random();
					origin.Offset(r.Next(1),r.Next(1));
					return origin;
				}
			} else {
				LastPointCount=0;
				LastPoint=origin;
			}
		Point offset = origin; 
		int i = (int)relative;
		offset.Offset(Directions[i,0],Directions[i,1]);
		return Clamp(offset); 
	} 
	private int GetRelativePosition(int start, int finish) 
	{ 
		Point startPoint = GetPointFromArrayIndex(start); 
		Point finishPoint = GetPointFromArrayIndex(finish); 
			
		foreach (NearbyPosition position in Enum.GetValues(typeof(NearbyPosition))) 
			if (GetNearbyPixel(start, position) == finish) 
	   			return (int)position; 
	   return -1; 
   } 
   private int[,] GetTraversalCost() 
   { 
	   int[] weights = GetPixelWeights(); 
	   int[,] cost = new int[relevantPixelsCount, maximumNearbyPositions]; 
	   for (int i = 0; i < weights.Length; i++) 
	   { 
		   Point origin = GetPointFromArrayIndex(i); 
		   foreach (NearbyPosition relativePosition in Enum.GetValues(typeof(NearbyPosition))) 
		   { 
			   Point relative = GetNearbyPixel(origin, relativePosition); 
			   if (IsOnMap(relative.X,relative.Y)) 
			   { 
				   int j = GetArrayIndex(relative); 
				   cost[i, (int)relativePosition] = weights[j]; 
			   } 
		   } 
	   } 
	   return cost; 
   } 
   
   private IEnumerable<int> nearbyNodesHint(int startingNode) 
   { 
	   List<int> nearbyNodes = new List<int>(maximumNearbyPositions); 
	   foreach (NearbyPosition position in Enum.GetValues(typeof(NearbyPosition))) 
	   nearbyNodes.Add(GetNearbyPixel(startingNode, position)); 
	   return nearbyNodes; 
   } 
   private int getInternodeTraversalCost(int start, int finish) 
   { 
	   int relativePosition = GetRelativePosition(start, finish); 
	   if (relativePosition < 0) return int.MaxValue; 
	   return traversalCost[start, relativePosition]; 
   } 
		private bool IsOnMap(int x,int y) // Z doesn't matter.
		{
			return (
			        x < chan.Width  && x >= 0 &&
			        y < chan.Height && y >= 0 );
		}
		private Point DirectionFromAng(double angle,int dist)
		{
			return new Point((int)Math.Sin(angle)*dist,(int)Math.Cos(angle)*dist);
		}
		/// <summary>
		/// Update pathfinding progress...
		/// </summary>
		/// <param name="VP">
		/// A <see cref="System.Int32"/> count of valid pathnodes.
		/// </param>
		/// <param name="IP">
		/// A <see cref="System.Int32"/> count of invalid pathnodes.
		/// </param>
		private void UpdateProgress(int VP,int IP,Point lp)
		{
			Console.CursorLeft = 0;
			Console.Write(" * Building River: ["+VP.ToString()+" Good, "+IP.ToString()+" Bad, lp=("+lp.ToString()+" )]          ");	
		}
		
		public List<PathFinderNode> FindPath(Point s,Point e)
		{
			s=Clamp(s);
			e=Clamp(e);
			
			int si=GetArrayIndex(s);
			int ei=GetArrayIndex(e);
			
			Console.WriteLine("FindPath: Initializing Dijkstra with {0}*{1}={2}",chan.Width,chan.Height,chan.Width*chan.Height);
			
			Dijkstra p = new Dijkstra(
			                          chan.Width*chan.Height,
			                          this.getInternodeTraversalCost,
			                          this.nearbyNodesHint);
			
			Console.WriteLine("FindPath: Locating path from {0} to {1}.",s,e);
			int[] PointPath=p.GetMinimumPath(si,ei);
			List<PathFinderNode> Path=new List<PathFinderNode>();
			foreach(int pi in PointPath)
			{
				Point pt = GetPointFromArrayIndex(pi);
				PathFinderNode pfn=new PathFinderNode();
				pfn.X=pt.X;
				pfn.Y=pt.Y;
				Path.Add(pfn);
			}
			return Path;
		}
		
/*
		public List<PathFinderNode> FindPath(Point s,Point e,int seed,float wl)
		{
			Channel mychan=chan.copy();
			
			
			
			wl=wl/512f;
			Console.WriteLine(" * Building river, starting at ({0},{1})",s.X,s.Y);
			
			bool Done=false;
			double LastDir=Math.PI*3; // Angle
			float LastHeight=mychan.getPixel(s.X,s.Y);
			
			int Invalid=0;
			int Valid=0;
			int REPEATS=0;
			
			float h = int.MaxValue;
			Point candidate=new Point(0,0);
			Point Current=Start;
			Point oldPoint=new Point(-1,-1);
			
			Console.WriteLine();
			
			Random r = new Random(seed);
			while(!Done)
			{
				candidate=new Point(-1,-1);
				for(int a=0;a<8;a++)
				{
					Point cp = new Point(direction[a,0],direction[a,1]);
					cp.X+=Current.X;
					cp.Y+=Current.Y;
					float nh=513f; // So it doesn't think the outside map = good place to go.
					if(IsOnMap(cp.X,cp.Y))
						nh= mychan.getPixel(cp.X,cp.Y);
					if(nh<=h && cp!=oldPoint)
					{
						h=nh; //Current height
						candidate=cp;
						if(IsOnMap(cp.X,cp.Y))
							mychan.putPixel(cp.X,cp.Y,2f); // Exclude already-used terrain.
					}
				}
				if(candidate==oldPoint)
				{
					Done=true;
					break;
				}
				if(candidate==(new Point(-1,-1)))
				{
					candidate=Current;
					candidate.X+=(Current.X-oldPoint.X);
					candidate.Y+=(Current.Y-oldPoint.Y);
					if(!IsOnMap(candidate.X,candidate.Y))
						return path;
					h= mychan.getPixel(candidate.X,candidate.Y);
					mychan.putPixel(candidate.X,candidate.Y,2f); // Exclude already-used terrain.
				}
				UpdateProgress(Valid,Invalid,candidate);
				if(Valid>2000) return path;
				if(IsOnMap(candidate.X,candidate.Y))
				{
					PathFinderNode np;
					np.X=candidate.X;
					np.Y=candidate.Y;
					path.Add(np);
					oldPoint=Current;
					Current=candidate;
					Valid++;
					if(!IsOnMap(candidate.X,candidate.Y))
					{
						Console.WriteLine();
						Console.WriteLine(" * <{0},{1},{2}> - River travelled outside of the map. Done.",candidate.X,candidate.Y,h);
						Done=true;
					}
					if(h<=wl)
					{
						Console.WriteLine();
						Console.WriteLine(" * <{0},{1},{2}> - River hit water level (wl={3}). Done.",candidate.X,candidate.Y,h,wl);
						Done=true;
					}
					Current=candidate;
					continue;
				}
				if(path.Count>0)
					path.RemoveAt(path.Count-1);
				Invalid++;
			}
			return path;
		}
		*/
		
		private Point Clamp(Point p)
		{
			p.X=Math.Min(chan.Width-1	, p.X);
			p.X=Math.Max(0				, p.X);
			p.Y=Math.Min(chan.Height-1	, p.Y);
			p.Y=Math.Max(0				, p.Y);
			return p;
		}
		
		public RiverBuilder GenerateRiver(float waterlevel,int seed,out List<PathFinderNode> RiverPath)
		{
			float oldDelta = chan.getMaxDelta();
			RiverPath=new List<PathFinderNode>();
			//Start = FindStartPoint(DateTime.Now.Millisecond%4);
			//End = FindEndPoint((DateTime.Now.Millisecond+1)%4);
			Random r = new Random(seed);
			List<Point> StartPoints = new List<Point>();
			
			float max=chan.findMax();
			float min=chan.findMin();
			float inv=1f/(max-min);
			
			waterlevel=Tools.interpolateLinear(0f,1f,(20f-0f)*(1f/(45f-0f)));
			float maxoffset=Tools.interpolateLinear(0f,1f,(5f-0f)*(1f/(45f-0f)));
			// Source can spawn >5m ASL, but must be >10m 
			//  from the highest altitude on the map.
			chan.normalize(); // stretch it the fsck out.
			
			Start=FindStartPoint(0);
			End=FindEndPoint(2);
			
			Console.WriteLine(" * Generating river, s={0}, e={1}.",Start,End);
			
			List<PathFinderNode> p=FindPath(Start,End);//,seed,20f);
			
			if(p==null)
			{
				Console.WriteLine(" ! Unable to retrieve path..");
				return this;
			}
				
			CleanPath(p);
			CreateBank(p);
			//CleanupFloodplain();
			
			Console.WriteLine(" * Maximum terrain delta: {0}",chan.getMaxDelta());
			chan.setMaxDelta(oldDelta/2f);
			Console.WriteLine(" * Adjusted terrain delta: {0}",chan.getMaxDelta());
			
			chan.smooth(1); // CleanupFloodplain is broken, dunno why.
			RiverPath=p;
			return this;
		}
		
		public RiverBuilder GenerateRiver(float waterlevel,int seed,Point start,Point end)
		{
			
			Console.WriteLine(" * Generating river, s={0}, e={1}.",Start,End);
			
			List<PathFinderNode> p=FindPath(Start,End);//,seed,20f);
			
			if(p==null)
			{
				Console.WriteLine(" ! Unable to retrieve path..");
				return this;
			}
				
			CleanPath(p);
			CreateBank(p);
			//CleanupFloodplain();
			
			Console.WriteLine(" * Maximum terrain delta: {0}",chan.getMaxDelta());
			chan.setMaxDelta(chan.getMaxDelta()/5f);
			Console.WriteLine(" * Adjusted terrain delta: {0}",chan.getMaxDelta());
			
			chan.smooth(1); // CleanupFloodplain is broken, dunno why.
			
			return this;
		}
		
		public void DebugPathFinder(int fromX, int fromY, int x, int y, PathFinderNodeType type, int totalCost, int cost)
		{
			Console.WriteLine("({0},{1}) -> ({2},{3}): Costs {4}",fromX,fromY,x,y,cost);
		}
		private void CleanPath(List<PathFinderNode> nodes)
		{
			Console.WriteLine("\n * Cleaning up path... ("+nodes.Count.ToString()+" nodes...)");
			for ( int i = nodes.Count-1; i > 0 ; --i )
			{
				//Console.Write(i.ToString()+",");
				int x,y;
					x=nodes[i].X;
					y=nodes[i].Y;
				float z=chan.getPixel(x,y);
				
				// Lower the vertices half way down 
				z=z*0.5f;
				
				// Find previous river height
				float pz=1f;
				if(i<nodes.Count)
					chan.getPixel(nodes[i-1].X,nodes[i-1].Y);
				
				
			   // If we actually went up (water doesn't go up)
			   // then move us slightly lower than the previous vertex
			   if (i < nodes.Count && z >= pz) 
			      z = pz-0.01f;
			
			   // store the new height in the heightmap
			   chan.putPixel(x,y,z);
			}
			Console.WriteLine("Done.");
		}
		
		private void CleanupFloodplain()
		{
			Console.WriteLine(" * Cleaning up floodplain...");
			
			// Initialize the acceptable height changes in the terrain
			// this will affect the smoothness
			float fDeltaY =  0.1f;
			float fAdjY = 0.03f;
			
			// Initialize the repeat bool to run once
			bool bAltered = true;
			// loop while we are making changes
			float hD=0f;
			int pass=0;
			while ( bAltered )
			{
			   // assume we aren't going to make changes
			   bAltered = false;
				pass++;
				int x,y;
			   // for the entire terrain minus the last row and column
				Console.WriteLine("  > Pass {0} ...",pass);
			   for ( x = 0; x < chan.Width-2; x++ )
			   {
			      for ( y = 0; y < chan.Height-2; y++ )
			      {
					 //Console.CursorLeft=0;
					 //Console.Write(" - Processing point ({0},{1}) ...",x,y);
			         // Check our right and our north neighbors
					float cD=chan.getPixel(x,y)-chan.getPixel(x,y+1);
					if(cD>hD)
						hD=cD;
			        if (cD > fDeltaY )
			        {
			         	chan.putPixel(x,y,chan.getPixel(x,y)-fAdjY);
			         	bAltered = true;
					 	//Console.WriteLine("Corrected ({0},{1})...",x,y);
					}
					cD=chan.getPixel(x,y)-chan.getPixel(x+1,y);
					if ( cD > hD )
						hD=cD;
			        if ( cD > fDeltaY )
			        {
			         	chan.putPixel(x,y,chan.getPixel(x,y)-fAdjY);
			         	bAltered = true;
					 	//Console.WriteLine("Corrected ({0},{1})...",x,y);
			         }
			      }
			   }
			   // for the entire terrain minus the first row and column
			   for ( x = chan.Width-1; x > 0; x-- )
			   {
			      for ( y = chan.Height-1; y > 0; y-- )
			      {
				        // check out our south and left neighbors
						float cD=chan.getPixel(x,y)-chan.getPixel(x,y-1);
							if(cD>hD)hD=cD;
				        if ( cD > fDeltaY )
						{
							chan.putPixel(x,y,chan.getPixel(x,y)-fAdjY);
							bAltered = true;
							//Console.WriteLine("Corrected ({0},{1})...",x,y);
						}
						cD=chan.getPixel(x,y)-chan.getPixel(x-1,y);
						if(cD>hD)
							hD=cD;
						if(cD > fDeltaY )
						{		
				         	chan.putPixel(x,y,chan.getPixel(x,y)-fAdjY);
				         	bAltered = true;
						 	//Console.WriteLine("Corrected ({0},{1})...",x,y);
				        }
			     	}
			  	}
			}
			Console.WriteLine("Highest Delta = {0}",hD);
		}
		
		float 	HEIGHT_ADJ		= 0.03f;
		int 	SLOPE_WIDTH		= 64;
		float 	SLOPE_VAL		= 0.1f;
		private void CreateBank(List<PathFinderNode> nodes)
		{
			Console.WriteLine("\n * Creating riverbank... ("+nodes.Count.ToString()+" nodes...)");
			for ( int i = nodes.Count-1; i > 0 ; --i )
			{
				int x=nodes[i].X;
				int y=nodes[i].Y;
				float z=chan.getPixel(x,y);
				
				// repeat variable	
				bool bAltered = true;
				while ( bAltered ) 
				{
					// Our default assumption is that we don't change anything
					// so we don't need to repeat the process
					bAltered = false;
					// Cycle through all valid terrain within the slope width
					// of the current position
					for ( 
					    int iX = Math.Max(0,x-SLOPE_WIDTH);
					    iX < Math.Min(chan.Width,y+SLOPE_WIDTH);
						++iX
					)
					{
						for ( 
							int iY = Math.Max(0,y-SLOPE_WIDTH);
							iY < Math.Min(chan.Height,y+SLOPE_WIDTH);
							++iY
						)
						{
							float fSlope;
							// find the slope from where we are to where we are checking
							fSlope = (chan.getPixel(iX,iY) - chan.getPixel(x,y))
								/(float)Math.Sqrt((x-iX)*(x-iX)+(y-iY)*(y-iY));
							if ( fSlope > SLOPE_VAL ) 
							{
								// the slope is too big so adjust the height and keep in mind
								// that the terrain was altered and we should make another pass
								chan.putPixel(x,y,chan.getPixel(x,y)-HEIGHT_ADJ);
								bAltered = true;
							}
						}
					}
				}
			}
			Console.WriteLine("Done.");
		}
		public Point FindStartPoint(int direction)
		{
			int x,y;
			Point candidate=new Point(0,0);
			float h,nh;
			h=nh=0f;
			switch(direction)
			{
				case 0: // North
					y=chan.Height-1;
					for(x=0;x<chan.Width;x++)
					{
						nh = chan.getPixel(x,y);
						if(nh>h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
				case 1: // East
					x=chan.Width-1;
					for(y=0;y<chan.height;y++)
					{
						nh = chan.getPixel(x,y);
						if(nh>h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
				case 2: // South
					y=0;
					for(x=0;x<chan.Width;x++)
					{
						nh = chan.getPixel(x,y);
						if(nh>h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
				case 3: // West
					x=0;
					for(y=0;y<chan.height;y++)
					{
						nh = chan.getPixel(x,y);
						if(nh>h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
			}
			return Clamp(candidate);
		}
		
		public Point FindEndPoint(int direction)
		{
			int x,y;
			Point candidate=new Point(0,0);
			float h,nh;
			h=nh=1f;
			switch(direction)
			{
				case 0: // North
					y=chan.Height-1;
					for(x=0;x<chan.Width;x++)
					{
						nh = chan.getPixel(x,y);
						if(nh<h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
				case 1: // East
					x=chan.Width-1;
					for(y=0;y<chan.Height;y++)
					{
						nh = chan.getPixel(x,y);
						if(nh<h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
				case 2: // South
					y=0;
					for(x=0;x<chan.Width;x++)
					{
						nh = chan.getPixel(x,y);
						if(nh<h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
				case 3: // West
					x=0;
					for(y=0;y<chan.Height;y++)
					{
						nh = chan.getPixel(x,y);
						if(nh<h)
						{
							h=nh;
							candidate=new Point(x,y);
						}
					}
				break;
			}
			return Clamp(candidate);
		}
		
		public Channel toChannel()
		{
			return chan;
		}
		public Layer toLayer() {
			return new Layer(chan.copy(), chan.copy(), chan.copy());
		}
	}
}
