
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
            vc.LoadFromFile(file);
			return vc;
		}
		
		
		public bool Save (string file,IVoxelChannel _c)
		{
			VoxelChannel c = (VoxelChannel)_c;
            c.SaveToFile(file);
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
