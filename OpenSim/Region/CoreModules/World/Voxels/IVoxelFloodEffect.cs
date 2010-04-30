
using System;

namespace OpenSim.Region.CoreModules.World.Voxels
{
	public interface IVoxelEffect
	{
		public void FloodEffect(IVoxelChannel, bool[,] mask, Vector3 min, Vector3 max, double str);
	}
}
