
using System;
using OpenMetaverse;
using System.IO;
using System.Collections;
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
using Gif.Components;
namespace OpenSim.Region.Framework.Scenes
{
	public enum ReplaceMode
	{
		NONE,
		A_ONLY,
		B_ONLY
	}

    // Provide a few common things to the terrain generator.
    public class MaterialMap
    {
        private Dictionary<byte,VoxMaterial> mMaterials = new Dictionary<byte,VoxMaterial>();
        private Dictionary<string,byte> mName2Byte = new Dictionary<string,byte>();
        private byte index=0;
        public MaterialMap()
        {
            Add("Air",new VoxMaterial());
            Add("Rock",new VoxMaterial());
            Add("Water",new VoxMaterial());
            Add("Soil",new VoxMaterial());
            Add("Grass",new VoxMaterial());
            Add("Sand",new VoxMaterial());
        }
        
        public VoxMaterial Air      { get { return mMaterials[0]; }}
        public VoxMaterial Rock     { get { return mMaterials[1]; }}
        public VoxMaterial Water    { get { return mMaterials[2]; }}
        public VoxMaterial Soil     { get { return mMaterials[3]; }}
        public VoxMaterial Grass    { get { return mMaterials[4]; }}
        public VoxMaterial Sand     { get { return mMaterials[5]; }}

        public VoxMaterial this[byte i]
        {
            get
            {
                if(mMaterials.ContainsKey(i))
                    return mMaterials[i];
                else
                    return new VoxMaterial();
            }
            set
            {
                if(mMaterials.ContainsKey(i))
                    mMaterials[i]=value;
            }
        }

        public VoxMaterial this[string n]
        {
            get
            {
                if(mName2Byte.ContainsKey(n))
                    return mMaterials[mName2Byte[n]];
                else
                    return new VoxMaterial();
            }
            set
            {
                if(mName2Byte.ContainsKey(n))
                    mMaterials[mName2Byte[n]]=value;
                else
                    Add(n,value);
            }
        }

        public void Add(string name, VoxMaterial mat)
        {
            mat.ID = index;
            mMaterials.Add(mat.ID,mat);
            mName2Byte.Add(name,mat.ID);
            index++;
        }

        public void Remove(string name)
        {
            if(mName2Byte.ContainsKey(name))
                mMaterials.Remove(mName2Byte[name]);
            index--;
        }

        public void Remove(byte id)
        {
            if(mMaterials.ContainsKey(id))
                mMaterials.Remove(id);
            index--;
        }
    
        internal void Serialize(ref XmlWriter w)
        {
 	        w.WriteStartElement("materials");
            w.WriteAttributeString("version","1");
            foreach(KeyValuePair<byte,VoxMaterial> kvp in mMaterials)
            {
                w.WriteStartElement("material");
                VoxMaterial mat = kvp.Value;
                w.WriteAttributeString("id",mat.ID.ToString());
                w.WriteAttributeString("name",mat.Name.ToString());
                w.WriteAttributeString("flags",mat.Flags.ToString());
                w.WriteAttributeString("density",mat.Density.ToString());
                w.WriteAttributeString("deposit",mat.Deposit.ToString());
                w.WriteAttributeString("texture",mat.Texture.ToString());
                w.WriteAttributeString("type",mat.Type.ToString());
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        internal void Deserialize(ref XmlElement doc)
        {
            int version = 0;
            if(!int.TryParse(doc.GetAttribute("version"),out version))
                return;

            if(version>1)
                return;

            foreach(XmlElement material in doc)
            {
                VoxMaterial mat = new VoxMaterial();
                mat.ID = byte.Parse(material.GetAttribute("id"));
                mat.Name = material.GetAttribute("name");
                mat.Flags = (MatFlags)Enum.Parse(typeof(MatFlags),material.GetAttribute("flags"));
                mat.Density = float.Parse(material.GetAttribute("density"));
                mat.Deposit = (DepositType)Enum.Parse(typeof(DepositType),material.GetAttribute("deposit"));
                mat.Texture = UUID.Parse(material.GetAttribute("texture"));
                mat.Type = (MaterialType)Enum.Parse(typeof(MaterialType),material.GetAttribute("type"));

                if(mat.ID>index)
                {
                    index=mat.ID;
                    index++;
                }

                mMaterials.Add(mat.ID,mat);
                mName2Byte.Add(mat.Name,mat.ID);
            }
        }
    
        internal void Deserialize(XmlTextReader reader)
        {
            bool keepreading=true;
            while(keepreading)
            {
                reader.Read();
                if(reader.Name.Equals("material") && reader.NodeType.Equals(XmlNodeType.Element))
                {
                    VoxMaterial mat = new VoxMaterial();
                    mat.ID = (byte)reader.ReadContentAsInt();
                    mat.Name = reader.ReadContentAsString();
                    mat.Flags = (MatFlags)Enum.Parse(typeof(MatFlags),reader.ReadContentAsString());
                    mat.Density = reader.ReadContentAsFloat();
                    mat.Deposit = (DepositType)Enum.Parse(typeof(DepositType),reader.ReadContentAsString());
                    mat.Texture = UUID.Parse(reader.ReadContentAsString());
                    mat.Type = (MaterialType)Enum.Parse(typeof(MaterialType),reader.ReadContentAsString());

                    if(mat.ID>index)
                    {
                        index=mat.ID;
                        index++;
                    }

                    mMaterials.Add(mat.ID,mat);
                    mName2Byte.Add(mat.Name,mat.ID);
                }
            }
        }
    
        internal NbtTag ToNBT()
        {
            NbtCompound mt = new NbtCompound("MaterialsTable");
			foreach(KeyValuePair<byte,VoxMaterial> mat in mMaterials)
			{
				NbtCompound cMat = new NbtCompound(mat.Value.Name);
				cMat.Tags.Add(new NbtByte(		"ID",		mat.Value.ID));
				cMat.Tags.Add(new NbtInt(		"Type",		(int)mat.Value.Type));
				cMat.Tags.Add(new NbtFloat(		"Density",	mat.Value.Density));
				cMat.Tags.Add(new NbtInt(		"Deposit",	(int)mat.Value.Deposit));
				cMat.Tags.Add(new NbtString(	"Texture",	mat.Value.Texture.ToString()));
				cMat.Tags.Add(new NbtByte(		"Flags",	(byte)mat.Value.Flags));
				mt.Tags.Add(cMat);
			}
            return mt;
        }

    
        internal void FromNbt(NbtCompound c)
        {
			foreach(NbtTag tag in c.Tags)
			{
				VoxMaterial m = new VoxMaterial();
				m.Name=tag.Name;
				foreach(NbtTag t in (tag as NbtCompound).Tags)
				{
					switch(t.Name)
					{
						case "ID":		m.ID = 						(t as NbtByte).Value; 	break;
						case "Type":	m.Type=		(MaterialType)	(t as NbtInt).Value; 	break;
						case "Density":	m.Density = 				(t as NbtFloat).Value; 	break;
						case "Deposit":	m.Deposit = (DepositType)	(t as NbtInt).Value; 	break;
						case "Texture": m.Texture = new UUID(		(t as NbtString).Value);break;
						case "Flags":	m.Flags = 	(MatFlags)		(t as NbtByte).Value; 	break;
					}
				}
				mMaterials.Add(m.ID,m);
                mName2Byte.Add(m.Name,m.ID);
                if(m.ID>index)
                {
                    index=m.ID;
                    index++;
                }

			}
        }
    }

    public interface ITerrainGenerator
    {
        void Initialize(int SizeX,int SizeY,int SizeZ, MaterialMap matmap);
        void Generate(ref VoxelChannel vox,Hashtable settings);
    }
	public class VoxelChannel:IVoxelChannel
	{
		// 256x256x256 = 16,777,216 Byte = 16 Megabyte (MB)!
        public byte[] Voxels;

		public const byte AIR_VOXEL=0x00;
		
        public MaterialMap mMaterials = new MaterialMap();
        public Dictionary<string, ITerrainGenerator> TerrainGenerators = new Dictionary<string,ITerrainGenerator>();
		
		public int XScale {get; protected set;}
		public int YScale {get; protected set;}
		public int ZScale {get; protected set;}
		
		public bool IsSolid(int x,int y,int z)
		{
			byte v = GetVoxel(x,y,z);
			
			if(v==AIR_VOXEL) return false;
			
			return (mMaterials[v].Flags & MatFlags.Solid)==MatFlags.Solid;
		}
		public int this[int x,int y,int z]
		{
			get{
				return (int)GetVoxel(x,y,z);
            }
			set
			{
				SetVoxel(x,y,z,(byte)value);
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
			Voxels = new byte[x*y*z];
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
			Voxels = new byte[x*y*z];
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
			return true;
		}
		public void AddMaterial(VoxMaterial butts)
		{
			mMaterials.Add(butts.Name,butts);
		}
		public void SetVoxel(Vector3 pos, byte v)
		{
			if(!inGrid(pos)) return;

			int px=(int)Math.Round(pos.X);
			int py=(int)Math.Round(pos.Y);
			int z=(int)Math.Round(pos.Z);

			int X = px / 16;
			int Y = py / 16;

			int x = (px >> 4) & 0xf;
			int y = (py >> 4) & 0xf;
			Voxels[y * ZScale + x * ZScale * XScale + z]=v;
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

			int px=(int)Math.Round(pos.X);
			int py=(int)Math.Round(pos.Y);
			int z=(int)Math.Round(pos.Z);

			int X = px / 16;
			int Y = py / 16;

			int x = (px >> 4) & 0xf;
			int y = (py >> 4) & 0xf;
			return Voxels[py * ZScale + px * ZScale * XScale + z];
		}
		public byte GetVoxel(int x,int y,int z)
		{
			return GetVoxel(new Vector3(x,y,z));
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
						if(As && Bs)
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
					ch=(float)GetHeightAt(x,y);
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
			Image image =  new Bitmap(XScale,YScale);
			double freq=0.03;
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
						(image as Bitmap).SetPixel(x,y,Color.FromArgb(255,0,0));
					}
				}
			}
			int ZH=Math.Min(ZScale,100);
			for(int z=0;z<ZH;z++)
			{
				int intensity=z*(255/ZH);
				for(int x=0;x<XScale;x++)
				{
					//Console.WriteLine();
					for(int y=0;y<YScale;y++)
					{
						bool d1 = ((p1.GetValue(x,y,z)+1)/2.0)>/*1d-*/Math.Pow(((double)z/(double)ZH),3d);
						bool d2 = ((p2.GetValue(x,y,z)+1)/2.0)>/*1d-*/Math.Pow(((double)z/(double)ZH),3d);
						// XOR?
						if (!(!d1 || !d2))
						{
							//Console.Write("#");
							(image as Bitmap).SetPixel(x,y,Color.FromArgb(255,intensity,intensity,intensity));
							SetVoxel(x,y,z,0x01);
						} else {
							//Console.Write(" ");
							//image.SetPixel(x,y,Color.FromArgb(255,0,0,0));
							SetVoxel(x,y,z,AIR_VOXEL);
						}
					}
				}
				Console.CursorLeft=0;
				Console.Write(" * {0}% ({1}/{2}) of layers created...",(int)(((float)(z+1)/((float)ZH))*100f),z+1,ZH);
			}
			Console.WriteLine();
			image.Save("terrain/GEN.png",System.Drawing.Imaging.ImageFormat.Png);
			image.Dispose();
			AnimatedGifEncoder e = new AnimatedGifEncoder();
			e.Start( "terrain/SLICE.gif" );
			e.SetDelay(250);
			//-1:no repeat,0:always repeat
			
			e.SetRepeat(0);

			for(int x=0;x<XScale;x++)
			{
				image = new Bitmap(YScale,ZScale);
				for(int y=0;y<YScale;y++)
				{
					//Console.WriteLine();
					for(int z=0;z<ZScale;z++)
					{
						if(IsSolid(x,y,z))
							(image as Bitmap).SetPixel(y,ZScale-z-1,Color.FromArgb(255,255,255));
						else
							(image as Bitmap).SetPixel(y,ZScale-z-1,Color.FromArgb(000,000,000));
					}
				}
				Console.CursorLeft=0;
				Console.Write(" * {0}% ({1}/{2}) frames added...",(int)(((float)(x+1)/((float)XScale))*100f),x+1,XScale);
				e.AddFrame((Image)image.Clone());
				image.Dispose();
			}
			Console.WriteLine();
			e.Finish();
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
						a(GetVoxel(x,y,z),x,y,z);
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
						map[x,y,z]=(int)GetVoxel(x,y,z);
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
							this[x,y,z]=0x00;
						}
						if(this[x,y,z]==null)
						{
							this[x,y,z]=0x00;
						}
						o[i] = (byte)this[x,y,z];
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
						this[x,y,z]=b[i];
						i++;
					}
				}
			}
		}
		
		private void WriteXML(XmlWriter w)
		{
			w.WriteStartElement("MaterialMap");
            mMaterials.Serialize(ref w);
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
							case "MaterialTable":
								NbtCompound c = (NbtCompound)tag;
								mMaterials.FromNbt(c);
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
		public void SaveToFile(string file)
		{
			using(NbtFile rdr = new NbtFile())
			{
				rdr.RootTag=new NbtCompound("__ROOT__");
				NbtCompound cMats = new NbtCompound("VoxMaterials");
				rdr.RootTag.Tags.Add(mMaterials.ToNBT());
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

        private void ReadXml(XmlTextReader reader)
        {
			reader.ReadStartElement("MaterialTable");
			mMaterials.Deserialize(reader);
            reader.ReadStartElement("Voxels");
            VoxelsFromXml(reader);
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
	                    this[x, y, z] = dataArray[index];
						index++;
					}
                }
            }
        }

        public byte[] GetChunk(int x, int y)
        {
            NbtFile file = new NbtFile();
            file.RootTag = new NbtCompound("__ROOT__");
            file.RootTag.Tags.Add(new NbtInt("x", x));
            file.RootTag.Tags.Add(new NbtInt("y", y));
            file.RootTag.Tags.Add(new NbtInt("z", 0));
            file.RootTag.Tags.Add(new NbtByteArray("c", GetChunkData(x,y)));
            byte[] endresult;
            using (MemoryStream ms = new MemoryStream())
            {
                file.SaveFile(ms,true);
                endresult= ms.ToArray();
            }
            return endresult;
        }
		
		//16x16x128 = 32768 = 32K
        // I think 64K would be OK if asked.
        public static readonly int CHUNK_SIZE_X = 16;
        public static readonly int CHUNK_SIZE_Y = 16;
        public static readonly int CHUNK_SIZE_Z = 128;
        public byte[] GetChunkData(int X, int Y, int Z=0)
        {
            byte[] d = new byte[CHUNK_SIZE_X * CHUNK_SIZE_Y * CHUNK_SIZE_Z];
            for(int x=0;x<CHUNK_SIZE_X;++x)
            {   
                for(int y=0;y<CHUNK_SIZE_Y;++y)
                {
                    for (int z = 0; z < CHUNK_SIZE_Z; ++z)
                    {
                        SetChunkBlock(ref d, GetVoxel(x + (X * CHUNK_SIZE_X), y + (Y * CHUNK_SIZE_Y), z), x, y, z);
                    }
                }
            }
            return d;
        }

        public void SetChunkBlock(ref byte[] chunk, byte type, int x, int y, int z)
        {
            chunk[y * ZScale + x * ZScale * XScale + z] = type;
        }
        public byte GetChunkBlock(ref byte[] chunk, int x, int y, int z)
        {
            chunk[y * ZScale + x * ZScale * XScale + z] = type;
        }
    }
}
