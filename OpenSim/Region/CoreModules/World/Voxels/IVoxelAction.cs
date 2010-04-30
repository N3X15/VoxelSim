
using System;
namespace OpenSim.Region.CoreModules.World.Voxels
{
	public interface IVoxelAction
	{
		void PaintEffect(IVoxelChannel chan, bool[,] mask, int x, int y, int z, double strength);		
	}
}
