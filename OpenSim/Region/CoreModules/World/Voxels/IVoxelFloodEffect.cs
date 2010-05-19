
using System;
using OpenSim.Region.Framework.Interfaces;
using OpenMetaverse;
namespace OpenSim.Region.CoreModules.World.Voxels
{
	public interface IVoxelEffect
	{
		void FloodEffect(IVoxelChannel c, bool[,] mask, Vector3 min, Vector3 max, double str);
	}
}
