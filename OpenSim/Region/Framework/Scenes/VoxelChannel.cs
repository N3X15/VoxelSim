
using System;
using OpenMetaverse;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using LibNbt;
using LibNbt.Tags;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Framework;

namespace OpenSim.Region.Framework.Scenes
{
	public enum ReplaceMode
	{
		NONE,
		A_ONLY,
		B_ONLY
	}
	public class VoxelChannel:IVoxelChannel
	{
		public Voxel[,,] Voxels;
		
		public int XScale;
		public int YScale;
		public int ZScale;
		
		public bool this[int x,int y,int z]
		{
			get{
				return (Voxels[x,y,z].Flags&VoxFlags.Solid)==VoxFlags.Solid;
			}
			set
			{
				Voxels[x,y,z].Flags|=VoxFlags.Solid;
			}
		}
		public int Height
		{
			get{return ZScale;}
		}
		public int Width
		{
			get{return XScale;}
		}
		public int Length
		{
			get{return YScale;}
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">
		/// X size
		/// </param>
		/// <param name="y">
		/// Y size
		/// </param>
		/// <param name="z">
		/// Z size
		/// </param>
		public VoxelChannel(int x,int y,int z)
		{
			Voxels = new Voxel[x,y,z];
			XScale=x;
			YScale=y;
			ZScale=z;
			
			FillVoxels(new Vector3(0,0,0),new Vector3(x,y,z),new Voxel());
		}
		public VoxelChannel(uint x,uint y,uint z)
		{
			Voxels = new Voxel[(int)x,(int)y,(int)z];
			XScale=(int)x;
			YScale=(int)y;
			ZScale=(int)z;
			
			FillVoxels(new Vector3(0,0,0),new Vector3((int)x,(int)y,(int)z),new Voxel());
		}
		
		public void SetVoxel(int x,int y,int z,Voxel voxel)
		{
			SetVoxel(new Vector3(x,y,z),voxel);
		}
		public bool Tainted(int x,int y,int z)
		{
			// TODO: Implement.
			return false;
		}
		public void SetVoxel(Vector3 pos, Voxel v)
		{
			if(!inGrid(pos)) return;
			v.Position=pos;
			int x,y,z;
			x=(int)Math.Round(pos.X);
			y=(int)Math.Round(pos.Y);
			z=(int)Math.Round(pos.Z);
			Voxels[x,y,z]=v;
		}
		
		public IVoxelChannel MakeCopy()
		{
			VoxelChannel vc = new VoxelChannel(XScale,YScale,ZScale);
			vc.Voxels=Voxels;
			return vc;
		}
		
		public bool inGrid(Vector3 pos)
		{
			
			return !(0 > pos.X || XScale < pos.X ||
			   0 > pos.Y || YScale < pos.Y ||
			   0 > pos.Z || ZScale < pos.Z );
		}
		
		public Voxel GetVoxel(Vector3 pos)
		{
			if(!inGrid(pos)) return null;
			int x,y,z;
			x=(int)Math.Round(pos.X);
			y=(int)Math.Round(pos.Y);
			z=(int)Math.Round(pos.Z);
			return Voxels[x,y,z];
		}
		public Voxel GetVoxel(int x,int y,int z)
		{
			if(!inGrid(new Vector3(x,y,z))) return null;
			return Voxels[x,y,z];
		}
		
		public void MoveVoxel(Vector3 from, Vector3 to)
		{
			SetVoxel(to,GetVoxel(from));
		}
		
		public void FillVoxels(Vector3 min,Vector3 max,Voxel v)
		{
			
			int x,y,z,X,Y,Z;
			x=(int)Math.Round(min.X);
			y=(int)Math.Round(min.Y);
			z=(int)Math.Round(min.Z);
			
			X=(int)Math.Round(max.X);
			Y=(int)Math.Round(max.Y);
			Z=(int)Math.Round(max.Z);
			
			for(;z<Z;z++)
				for(;y<Y;y++)
					for(;x<X;x++)
						SetVoxel(new Vector3(x,y,z),v);
		}
		
		static public VoxelChannel AND(VoxelChannel a,VoxelChannel b,ReplaceMode rep)
		{
			int x,y,z;
			for(z=0;z<a.ZScale;z++)
			{
				for(y=0;y<a.YScale;y++)
				{
					for(x=0;x<a.XScale;x++)
					{
						Voxel av=a.GetVoxel(new Vector3(x,y,z));
						Voxel bv=b.GetVoxel(new Vector3(x,y,z));
						if((av.Flags&VoxFlags.Solid)>0 && (av.Flags&VoxFlags.Solid)>0)
						{
							a.SetVoxel(new Vector3(x,y,z),bv);
						}
					}
				}
			}
			return a;
		}
		
		public static VoxelChannel operator+(VoxelChannel a, VoxelChannel b)
		{
			int x,y,z;
			for(z=0;z<a.ZScale;z++)
			{
				for(y=0;y<a.YScale;y++)
				{
					for(x=0;x<a.XScale;x++)
					{
						Voxel bv=b.GetVoxel(new Vector3(x,y,z));
						if((bv.Flags&VoxFlags.Solid)>0 || (bv.Flags&VoxFlags.Fluid)>0)
							a.SetVoxel(new Vector3(x,y,z),bv);
					}
				}
			}
			return a;
		}
		
		
		public static VoxelChannel operator-(VoxelChannel a, VoxelChannel b)
		{
			int x,y,z;
			for(z=0;z<a.ZScale;z++)
			{
				for(y=0;y<a.YScale;y++)
				{
					for(x=0;x<a.XScale;x++)
					{
						Voxel bv=b.GetVoxel(new Vector3(x,y,z));
						if((bv.Flags&VoxFlags.Solid)>0 || (bv.Flags&VoxFlags.Fluid)>0)
							a.SetVoxel(new Vector3(x,y,z),new Voxel());
					}
				}
			}
			return a;
		}
		
		public void Save(string RegionName)
		{
			VoxelLayer[] vl=GetLayers();
			foreach(VoxelLayer l in vl)
				l.Save(RegionName);
		}
		
		public void Load(string RegionName)
		{
			VoxelLayer[] vl=new VoxelLayer[ZScale];
			for(int z = 0;z<ZScale;z++)
			{
				VoxelLayer l = new VoxelLayer(z,XScale,YScale);
				l.Load(RegionName);
				
				for(int x=0;x<XScale;x++)
					for(int y=0;y<YScale;y++)
						Voxels[x,y,z]=l.Layer[x,y];
			}
		}
		
		public VoxelLayer[] GetLayers()
		{
			VoxelLayer[] vl=new VoxelLayer[ZScale];
			for(int l=0;l<ZScale;l++)
			{
				VoxelLayer lyr = new VoxelLayer(l,XScale,YScale);
				for(int x=0;x<XScale;x++)
					for(int y=0;y<YScale;y++)
						lyr.Layer[x,y]=Voxels[x,y,l];
				vl[l]=lyr;
			}
			return vl;
		}
		
		public double GetHeightAt(int x,int y)
		{
			double h=0;
			for(int z=0;z<ZScale;z++)
			{
				if((Voxels[x,y,z].Flags & VoxFlags.Solid)==VoxFlags.Solid)
					h=Math.Max(h,z);
			}
			return h;
		}
		
		/// <summary>
		/// For the physics subsystem (DEPRECIATED) and mapping.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Single[]"/>
		/// </returns>
		public float[] GetFloatsSerialised()
		{
			/* Basically, get the highest point on each column. */
			List<float> sf = new List<float>();
			for(int x=0;x<XScale;x++)
			{
				for(int y=0;y<YScale;y++)
				{
					float ch = 0;
					for(int z=0;z<ZScale;z++)
					{
						// If Voxel is Solid
						if((Voxels[x,y,z].Flags & VoxFlags.Solid)==VoxFlags.Solid)
						{
							ch=Math.Max(z,ch);
						}
					}
					sf.Add(ch);
				}
			}
			return sf.ToArray();
		}
		public IVoxelChannel Generate(string method)
		{
			return Generate(method,new object[]{});
		}
		public IVoxelChannel Generate(string method,object[] args)
		{
			bool[,,] p0,p1,p2;
			
			p0=p1=p2 = new bool[XScale,YScale,ZScale];
			
			PerlinNoise p = new PerlinNoise(DateTime.Now.Second);
			for(int z=0;z<ZScale;z++)
				for(int x=0;x<XScale;x++)
					for(int y=0;y<YScale;y++)
						p0[x,y,z]=(1==(int)Math.Round((p.Noise(x,y,(double)z/2.0)/2.0)+1.0));
			
			p = new PerlinNoise(DateTime.Now.Second+1);
			for(int z=0;z<ZScale;z++)
				for(int x=0;x<XScale;x++)
					for(int y=0;y<YScale;y++)
						p1[x,y,z]=(1==(int)Math.Round((p.Noise(x,y,(double)z/2.0)/2.0)+1.0));
			
			p = new PerlinNoise(DateTime.Now.Second+2);
			for(int z=0;z<ZScale;z++)
				for(int x=0;x<XScale;x++)
					for(int y=0;y<YScale;y++)
						p2[x,y,z]=(1==(int)Math.Round((p.Noise(x,y,(double)z/2.0)/2.0)+1.0));
			
			VoxelChannel v = new VoxelChannel(XScale,YScale,ZScale);
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						Voxel vox=new Voxel();
						// if p0 AND p1 AND NOT p2
						if(p0[x,y,z] && p1[x,y,z] && !p2[x,y,z])
							vox.Flags|=VoxFlags.Solid;
						v.Voxels[x,y,z]=vox;
					}
				}
			}
			return v;
		}
		public double[,] GetDoubles()
		{
			return GetDoubles(false);
		}
		
		public void ForEachVoxel(Action<Voxel> a)
		{
			for(int x=0;x<XScale;x++)
			{
				for(int y=0;y<YScale;y++)
				{
					for(int z=0;z<ZScale;z++)
					{
						a(Voxels[x,y,z]);
					}
				}
			}
		}
		
		public double[,] GetDoubles(bool lowest)
		{
			double[,] o = new double[XScale,YScale];
			for(int x=0;x<XScale;x++)
			{
				for(int y=0;y<YScale;y++)
				{
					o[x,y]=0;
					for(int z=0;z<ZScale;z++)
					{
						if((Voxels[x,y,z].Flags&VoxFlags.Solid)==VoxFlags.Solid)
						{
							if(o[x,y]<(double)z) 
							{
								o[x,y]=(double)z;
							}
						} else {
							if(lowest) break;
						}
					}
				}
			}
			return o;
		}
		// For finding stuck avs/objects
		public bool IsInsideTerrain(Vector3 pos)
		{
			return (Voxels[(int)pos.X,(int)pos.Y,(int)pos.Z].Flags&VoxFlags.Solid)==VoxFlags.Solid;
		}
		// For fixing stuck avs/objects.
		public Vector3 FindNearestAirVoxel(Vector3 subject, bool ForAvatar)
		{
			Vector3 nearest = new Vector3(0,0,0);
			float nd = 10000f;
			ForEachVoxel(delegate(Voxel a){
				float d = Vector3.Distance(subject,a.Position);
				if(d<nd)
				{
					if(ForAvatar)
					{
						//Avatars need 2m vertical space instead of 1m.
						int x = (int)a.Position.X;
						int y = (int)a.Position.Y;
						int z = (int)a.Position.Z;
						if(z!=255||(Voxels[x,y,z+1].Flags&VoxFlags.Solid)==VoxFlags.Solid)
							return;
					}
					nd=d;
					nearest=a.Position;
				}
			});
			return nearest;
		}
		public int[,,] ToMaterialMap()
		{
			int[,,] map = new int[XScale,YScale,ZScale];
			for(int x=0;x<XScale;x++)
				for(int y=0;y<YScale;y++)
					for(int z=0;z<ZScale;z++)
						map[x,y,z]=((Voxels[x,y,z].Flags&VoxFlags.Solid)==VoxFlags.Solid) ? 0 : -1;
			return map;
		}
		public void ImportHeightmap(float[,] heightmap)
		{
			int MX=heightmap.GetLength(0);
			int MY=heightmap.GetLength(1);
			
			
			Voxel v = new Voxel();
			v.Flags=VoxFlags.Solid;
			
			for(int x=0;x<MX;x++)
			{
				for(int y=0;y < MY;y++)
				{
					int MZ=Convert.ToInt32(heightmap[x,y]);
					for(int z=0;z<MZ;z++)
					{
						SetVoxel(x,y,z,v);
					}
				}
			}
		}
		
		public bool[] GetBoolsSerialised()
		{
			bool[] sb = new bool[XScale*YScale*ZScale];
			int i = 0;
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						sb[i]=(Voxels[x,y,z].Flags&VoxFlags.Solid)==VoxFlags.Solid;
						i++;
					}
				}
			}
			return sb;
		}
		
		public string SaveToXmlString()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Util.UTF8;
            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    WriteXML(writer);
                }
                string output = sw.ToString();
                return output;
            }
		}
		
		public byte[] ToBytes()
		{
			
			byte[] o = new byte[XScale*YScale*ZScale*2];
			int i =0;
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						byte[] v = Voxels[x,y,z].asBytes();
						o[i]=v[0];
						o[i+1]=v[1];
						i+=2;
					}
				}
			}
			return o;
		}
		
		public void FromBytes(byte[] b)
		{
			int i =0;
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						Voxels[x,y,z]=new Voxel(new byte[]{b[i],b[i+1]});
						i+=2;
					}
				}
			}
		}
		
		private void WriteXML(XmlWriter w)
		{
			w.WriteStartElement("Voxels");
			XmlSerializer xs = new XmlSerializer(typeof(byte[]));
			xs.Serialize(w,ToBytes());
			w.WriteEndElement();
		}
		public void LoadFromFile(string file)
		{
			using(NbtFile rdr = new NbtFile(file))
			{
				if(rdr.RootTag is NbtCompound && (rdr.RootTag as NbtCompound).Name.Equals("Region"))
				{
					foreach(NbtTag tag in rdr.RootTag.Tags)
					{
						switch(tag.Name)
						{
							case "Voxels":
								NbtByteArray vba = (NbtByteArray)tag;
								FromBytes(vba.Value);
								break;
						}
					}
				}
			}
		}
		public void SaveToFile(string file)
		{
			using(NbtFile rdr = new NbtFile())
			{
				//rdr.RootTag.Name="VoxelSim";
				rdr.RootTag.Tags.Add(new NbtByteArray("Voxels",ToBytes()));
				rdr.SaveFile(file);
			}
		}
		public void LoadFromXmlString(string data)
        {
            StringReader sr = new StringReader(data);
            XmlTextReader reader = new XmlTextReader(sr);
            reader.Read();

            ReadXml(reader);
            reader.Close();
            sr.Close();
        }

        private void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Voxels");
            FromXml(reader);
        }

        private void FromXml(XmlReader xmlReader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(byte[]));
            byte[] dataArray = (byte[])serializer.Deserialize(xmlReader);
            int index = 0;

            
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
	                    Voxel vox = new Voxel();
						vox.Flags=(VoxFlags)dataArray[index];
						vox.MaterialID=dataArray[index+1];
	                    
						index += 2;
	                    Voxels[x, y, z] = vox;
					}
                }
            }
        }
		
		//
	}
}
