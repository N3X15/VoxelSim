
using System;
using System.IO;
using System.Collections.Generic;
using OpenMetaverse;


namespace OpenSim.Region.Framework.Scenes
{
	public class VoxelLayer
	{
		public int VERSION=0x0001;
		Dictionary<int,int> MaterialCount = new Dictionary<int, int>();
		
		int ID=0;
		int SizeX;
		int SizeY;
		public Voxel[,] Layer;
		// HEADER
		//		Version
		//		layer ID (0-256)
		//		Size (x,y)
		//		Checksum
		//		LastChunk
		// Body
		//		(x,y) material flags
		public VoxelLayer(int id,int x, int y)
		{
			ID=id;
			Layer= new Voxel[x,y];
			SizeX=x;
			SizeY=y;
		}
		
		public void Save(string RegionName)
		{
			string fname = string.Format("terrain/{0}/{1}.lyr",RegionName,ID);
			
			if(!Directory.Exists(string.Format("terrain/{0}/",RegionName)))
				Directory.CreateDirectory(string.Format("terrain/{0}/",RegionName));
			
			using(BinaryWriter lyr = new BinaryWriter(File.OpenWrite(fname)))
			{
				lyr.Write(VERSION);
				lyr.Write(ID);
				lyr.Write(SizeX);lyr.Write(SizeY);
				lyr.Write(UUID.Zero.ToString());
				lyr.Write(ID==255);
				List<Voxel> Points = new List<Voxel>();
				for(int x=0;x<SizeX;x++)
				{
					for(int y=0;y<SizeY;y++)
					{
						if((Layer[x,y].Flags&VoxFlags.Solid)>0 || (Layer[x,y].Flags&VoxFlags.Solid)>0)
						{
							Points.Add(Layer[x,y]);
						}
					}
				}
				lyr.Write(Points.Count);
				foreach(Voxel v in Points)
				{
					int x = (int)v.Position.X;
					int y = (int)v.Position.Y;
					lyr.Write(x);
					lyr.Write(y);
					lyr.Write(Layer[x,y].MaterialID);
					lyr.Write((int)Layer[x,y].Flags);
				}
			}
		}
		
		public void Load(string RegionName)
		{
			string fname = string.Format("terrain/{0}/{1}.lyr",RegionName,ID);
			
			using(BinaryReader lyr = new BinaryReader(File.OpenRead(fname)))
			{
				if(lyr.ReadInt32()!=VERSION) throw new Exception(fname+" is too old.  You'll need to regenerate the terrain.");
				
				ID= lyr.ReadInt32();
				
				SizeX=lyr.ReadInt32();
				SizeY=lyr.ReadInt32();
				
				// Skip checksum
				lyr.ReadString();
				
				// Skip lastchunk
				lyr.ReadBoolean();
				
				int n = lyr.ReadInt32();
				for(int i = 0;i<n;i++)
				{
					Voxel v = new Voxel();
					int x=lyr.ReadInt32();
					int y=lyr.ReadInt32();
					v.Position=new OpenMetaverse.Vector3(x,y,ID+1);
					v.MaterialID=lyr.ReadByte();
					v.Flags=(VoxFlags)lyr.ReadByte();
					Layer[x,y]=v;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
	}
}
