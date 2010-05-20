
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
using LibNoise;
using Math=System.Math;
using VoxMaterial=OpenSim.Region.Framework.Scenes.VoxMaterial;
using System.Drawing;
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
		// 256x256x256 = 16,777,216 Byte = 16 Megabyte (MB)!
		public byte[,,] Voxels;
		/// <summary>
		/// VoxMaterial ID
		/// </summary>
		private byte cMID=0x00;
		public const byte AIR_VOXEL=0x00;
		
		public Dictionary<byte,VoxMaterial> MaterialTable = new Dictionary<byte, VoxMaterial>();
		
		public int XScale {get; protected set;}
		public int YScale {get; protected set;}
		public int ZScale {get; protected set;}
		
		public bool IsSolid(int x,int y,int z)
		{
			byte v = Voxels[x,y,z];
			
			if(v==AIR_VOXEL) return false;
			
			if(!MaterialTable.ContainsKey(v)) 
				return false;
			
			return (MaterialTable[v].Flags & MatFlags.Solid)==MatFlags.Solid;
		}
		public int this[int x,int y,int z]
		{
			get{
				return (int)Voxels[x,y,z];
			}
			set
			{
				int v = value;
				if(v>255) v=255;
				if(v<0) v=0;
				Voxels[x,y,z]=(byte)v;
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
			Voxels = new byte[x,y,z];
			XScale=x;
			YScale=y;
			ZScale=z;
			
			VoxMaterial m = new VoxMaterial();
			m.Flags=MatFlags.Solid;
			AddMaterial(m);
			
			FillVoxels(new Vector3(0,0,0),new Vector3(x,y,z),AIR_VOXEL);
		}
		public VoxelChannel(uint x,uint y,uint z)
		{
			Voxels = new byte[(int)x,(int)y,(int)z];
			XScale=(int)x;
			YScale=(int)y;
			ZScale=(int)z;
			
			VoxMaterial m = new VoxMaterial();
			m.Flags=MatFlags.Solid;
			AddMaterial(m);
			
			FillVoxels(new Vector3(0,0,0),new Vector3((int)x,(int)y,(int)z),AIR_VOXEL);
		}
		
		public void SetVoxel(int x,int y,int z,byte voxel)
		{
			SetVoxel(new Vector3(x,y,z),voxel);
		}
		public bool Tainted(int x,int y,int z)
		{
			// TODO: Implement.
			return false;
		}
		public void AddMaterial(VoxMaterial butts)
		{
			cMID++;
			MaterialTable.Add(cMID,butts);
		}
		public void SetVoxel(Vector3 pos, byte v)
		{
			if(!inGrid(pos)) return;
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
		
		public byte GetVoxel(Vector3 pos)
		{
			if(!inGrid(pos)) return AIR_VOXEL;
			int x,y,z;
			x=(int)Math.Round(pos.X);
			y=(int)Math.Round(pos.Y);
			z=(int)Math.Round(pos.Z);
			return Voxels[x,y,z];
		}
		public byte GetVoxel(int x,int y,int z)
		{
			if(!inGrid(new Vector3(x,y,z))) return AIR_VOXEL;
			byte v = Voxels[x,y,z];
			if(v==null)
				v=AIR_VOXEL;
			return v;
		}
		
		public void MoveVoxel(Vector3 from, Vector3 to)
		{
			SetVoxel(to,GetVoxel(from));
		}
		
		public void FillVoxels(Vector3 min,Vector3 max,byte v)
		{
			for(int x = (int)Math.Round(min.X);x<(int)Math.Round(max.X);x++)
			{
				for(int y = (int)Math.Round(min.Y);y<(int)Math.Round(max.Y);y++)
				{
					for(int z = (int)Math.Round(min.Z);z<(int)Math.Round(max.Z);z++)
					{
						///Console.WriteLine("({0},{1},{2}) {3}",x,y,z,v);
						SetVoxel(x,y,z,v);
					}
				}
			}
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
						bool As=a.IsSolid(x,y,z);
						bool Bs=b.IsSolid(x,y,z);
						if(As && As)
						{
							a.SetVoxel(x,y,z,b.GetVoxel(x,y,z));
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
						if(b.IsSolid(x,y,z))
							a.SetVoxel(x,y,z,b.GetVoxel(x,y,z));
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
						if(b.IsSolid(x,y,z))
							a.SetVoxel(x,y,z,0x00);
					}
				}
			}
			return a;
		}
		
		public void Save(string RegionName)
		{
			SaveToFile("terrain/"+RegionName+".osvox");
		}
		
		public void Load(string RegionName)
		{
			if(!File.Exists("terrain/"+RegionName+".osvox"))
				throw new IOException("File not found.");
			LoadFromFile("terrain/"+RegionName+".osvox");
			/*
			VoxelLayer[] vl=new VoxelLayer[ZScale];
			for(int z = 0;z<ZScale;z++)
			{
				VoxelLayer l = new VoxelLayer(z,XScale,YScale);
				l.Load(RegionName);
				
				for(int x=0;x<XScale;x++)
					for(int y=0;y<YScale;y++)
						Voxels[x,y,z]=l.Layer[x,y];
			}*/
		}
		/*
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
		*/
		public double GetHeightAt(int x,int y)
		{
			double h=0;
			for(int z=0;z<ZScale;z++)
			{
				if(IsSolid(x,y,z))
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
						if(Voxels==null)
						{
							Console.WriteLine("Voxels == NULL!");
							continue;
						}
						if(Voxels[x,y,z]==null)
						{
							Voxels[x,y,z]=AIR_VOXEL;
						}
						if(IsSolid(x,y,z))
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
			Perlin p1 = new Perlin();
			Perlin p2 = new Perlin();
			p1.Seed=DateTime.Now.Millisecond;
			p2.Seed=DateTime.Now.Millisecond+2;
			Bitmap image = new Bitmap(XScale,YScale);
			double freq=0.05;
			double lacu=0.01;
			double pers=0.01;
			int octv=1;
			
			try
			{
				freq=(Double)args[0];
				lacu=(Double)args[1];
				pers=(Double)args[2];
				octv=(int)args[3];
			}
			catch(Exception)
			{
				Console.WriteLine("Using defaults.");
			}
            p1.Frequency = freq;
            p1.NoiseQuality = NoiseQuality.Standard;
            p1.OctaveCount = octv;
            p1.Lacunarity = lacu;
            p1.Persistence = pers;
			
            p2.Frequency = freq;
            p2.NoiseQuality = NoiseQuality.Standard;
            p2.OctaveCount = octv;
            p2.Lacunarity = lacu;
            p2.Persistence = pers;
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						image.SetPixel(x,y,Color.FromArgb(255,0,0));
					}
				}
			}
			int ZH=Math.Min(ZScale,64);
			for(int z=0;z<ZH;z++)
			{
				int intensity=z*4;
				for(int x=0;x<XScale;x++)
				{
					//Console.WriteLine();
					for(int y=0;y<YScale;y++)
					{
						bool d1 = ((p1.GetValue(x,y,z)+1)/2.0)>/*1d-*/Math.Pow(((double)z/(double)ZH),4d);
						bool d2 = ((p2.GetValue(x,y,z)+1)/2.0)>/*1d-*/Math.Pow(((double)z/(double)ZH),4d);
						// XOR?
						if (!(!d1 || !d2))
						{
							//Console.Write("#");
							image.SetPixel(x,y,Color.FromArgb(255,intensity,intensity,intensity));
							Voxels[x,y,z]=0x01;
						} else {
							//Console.Write(" ");
							//image.SetPixel(x,y,Color.FromArgb(255,0,0,0));
							Voxels[x,y,z]=AIR_VOXEL;
						}
					}
				}
				Console.WriteLine("{0}% ({1}/{2}) of layers created...",(int)(((float)(z+1)/((float)ZH))*100f),z+1,ZH);
			}
			image.Save("terrain/GEN.png",System.Drawing.Imaging.ImageFormat.Png);
			image.Dispose();
			
			image = new Bitmap(YScale,ZScale);
			for(int y=0;y<XScale;y++)
			{
				//Console.WriteLine();
				for(int z=0;z<ZScale;z++)
				{
					if(IsSolid(128,y,z))
						image.SetPixel(y,ZScale-z-1,Color.FromArgb(255,255,255));
					else
						image.SetPixel(y,ZScale-z-1,Color.FromArgb(000,000,000));
				}
			}
			image.Save("terrain/SLICE.png",System.Drawing.Imaging.ImageFormat.Png);
			image.Dispose();
			return this;
		}
		public double[,] GetDoubles()
		{
			return GetDoubles(false);
		}
		
		public void ForEachVoxel(Action<byte,int,int,int> a)
		{
			for(int x=0;x<XScale;x++)
			{
				for(int y=0;y<YScale;y++)
				{
					for(int z=0;z<ZScale;z++)
					{
						a(Voxels[x,y,z],x,y,z);
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
						if(IsSolid(x,y,z))
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
			return (IsSolid((int)pos.X,(int)pos.Y,(int)pos.Z));
		}
		// For fixing stuck avs/objects.
		public Vector3 FindNearestAirVoxel(Vector3 subject, bool ForAvatar)
		{
			Vector3 nearest = new Vector3(0,0,0);
			float nd = 10000f;
			ForEachVoxel(delegate(byte a,int x,int y,int z){
				if(IsSolid(x,y,z)) return;
				float d = Vector3.Distance(subject,new Vector3(x,y,z));
				if(d<nd)
				{
					if(ForAvatar)
					{
						//Avatars need 2m vertical space instead of 1m.
						if(z==ZScale-1||IsSolid(x,y,z+1))
							return;
					}
					nd=d;
					nearest=new Vector3(x,y,z);
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
						map[x,y,z]=(int)Voxels[x,y,z];
			return map;
		}
		public void ImportHeightmap(float[,] heightmap)
		{
			int MX=heightmap.GetLength(0);
			int MY=heightmap.GetLength(1);
			
			for(int x=0;x<MX;x++)
			{
				for(int y=0;y < MY;y++)
				{
					int MZ=Convert.ToInt32(heightmap[x,y]);
					for(int z=0;z<MZ;z++)
					{
						SetVoxel(x,y,z,0x01);
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
						sb[i]=IsSolid(x,y,z);
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
			
			byte[] o = new byte[XScale*YScale*ZScale];
			int i =0;
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						if(Voxels==null)
						{
							Console.WriteLine("Voxels == NULL!");
							Voxels[x,y,z]=0x00;
						}
						if(Voxels[x,y,z]==null)
						{
							Voxels[x,y,z]=0x00;
						}
						o[i] = Voxels[x,y,z];
						i++;
					}
				}
			}
			return o;
		}
		
		public void FromBytes(byte[] b)
		{
			long rfl=XScale*YScale*ZScale;
			if(rfl != b.Length)
				throw new IOException("Voxelmap is of size "+b.Length.ToString()+" when it's supposed to be "+rfl.ToString());
			int i =0;
			for(int z=0;z<ZScale;z++)
			{
				for(int x=0;x<XScale;x++)
				{
					for(int y=0;y<YScale;y++)
					{
						Voxels[x,y,z]=b[i];
						i++;
					}
				}
			}
		}
		
		private void WriteXML(XmlWriter w)
		{
			w.WriteStartElement("VoxMaterialMap");
			XmlSerializer mm = new XmlSerializer(typeof(Dictionary<byte,VoxMaterial>));
			mm.Serialize(w,this.MaterialTable);
			w.WriteEndElement();
			w.WriteStartElement("Voxels");
			XmlSerializer vm = new XmlSerializer(typeof(byte[]));
			vm.Serialize(w,ToBytes());
			w.WriteEndElement();
		}
		public void LoadFromFile(string file)
		{
			Console.WriteLine(file);
			using(NbtFile rdr = new NbtFile(file))
			{
				if(rdr.RootTag is NbtCompound && (rdr.RootTag as NbtCompound).Name.Equals("Region"))
				{
					foreach(NbtTag tag in rdr.RootTag.Tags)
					{
						switch(tag.Name)
						{
							case "VoxMaterials":
								NbtCompound c = (NbtCompound)tag;
								LoadMatsFromNbt(c);
								break;
							case "Voxels":
								NbtByteArray vba = (NbtByteArray)tag;
								FromBytes(vba.Value);
								break;
						}
					}
				}
			}
		}
		private void LoadMatsFromNbt(NbtCompound c)
		{
			foreach(NbtTag tag in c.Tags)
			{
				VoxMaterial m = new VoxMaterial();
			}
		}
		public void SaveToFile(string file)
		{
			using(NbtFile rdr = new NbtFile())
			{
				rdr.RootTag=new NbtCompound();
				NbtCompound cMats = new NbtCompound("VoxMaterials");
				foreach(KeyValuePair<byte,VoxMaterial> mat in MaterialTable)
				{
					NbtCompound cMat = new NbtCompound(mat.Value.Name);
					cMat.Tags.Add(new NbtByte(		"ID",		mat.Value.ID));
					cMat.Tags.Add(new NbtInt(		"Type",		(int)mat.Value.Type));
					cMat.Tags.Add(new NbtFloat(		"Density",	mat.Value.Density));
					cMat.Tags.Add(new NbtInt(		"Deposit",	(int)mat.Value.Deposit));
					cMat.Tags.Add(new NbtString(	"Texture",	mat.Value.Texture.ToString()));
					cMat.Tags.Add(new NbtByte(		"Flags",	(byte)mat.Value.Flags));
					cMats.Tags.Add(cMat);
				}
				rdr.RootTag.Tags.Add(cMats);
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
			reader.ReadStartElement("MaterialTable");
			MatsFromXml(reader);
            reader.ReadStartElement("Voxels");
            VoxelsFromXml(reader);
        }
		private void MatsFromXml(XmlReader xmlReader)
		{
            XmlSerializer serializer = new XmlSerializer(typeof(Dictionary<byte,VoxMaterial>));
            MaterialTable = (Dictionary<byte,VoxMaterial>)serializer.Deserialize(xmlReader);
		}
        private void VoxelsFromXml(XmlReader xmlReader)
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
	                    Voxels[x, y, z] = dataArray[index];
						index++;
					}
                }
            }
        }
		
		//
	}
}
