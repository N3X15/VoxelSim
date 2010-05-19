
using System;
using System.IO;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using LibNbt;
using LibNbt.Tags;
namespace OpenSim.Region.CoreModules.World.Voxels
{
	public class NBTFileHandler :IVoxelFileHandler
	{
		#region IVoxelFileHandler implementation
		public IVoxelChannel Load (string file)
		{
			VoxelChannel vc = new VoxelChannel(Constants.RegionSize,Constants.RegionSize,256);
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
								vc.FromBytes(vba.Value);
								break;
						}
					}
				}
			}
			return vc;
		}
		
		
		public bool Save (string file,IVoxelChannel _c)
		{
			VoxelChannel c = (VoxelChannel)_c;
			using(NbtFile rdr = new NbtFile())
			{
				//rdr.RootTag.Name="VoxelSim";
				rdr.RootTag.Tags.Add(new NbtByteArray("Voxels",c.ToBytes()));
				rdr.SaveFile(file);
			}
			return true;
		}
		
		public IVoxelChannel LoadStream(Stream a)
		{
			throw new NotSupportedException();
		}
		
		public void SaveStream(Stream a,IVoxelChannel c)
		{
			throw new NotSupportedException();
		}
		
		#endregion

		public NBTFileHandler ()
		{
		}
		
		
	}
}
