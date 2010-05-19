
using System;
using System.IO;
using OpenSim.Region.Framework.Interfaces;

namespace OpenSim.Region.CoreModules.World.Voxels
{
	public interface IVoxelFileHandler
	{
		/// <summary>
		/// Load a voxel channel into existance from a file.
		/// </summary>
		IVoxelChannel Load(string filename);
		
		IVoxelChannel LoadStream(Stream derp);
		
		/// <summary>
		/// Save a voxel channel to a file.
		/// </summary>
		/// <param name="filename">
		/// Filename of the file.
		/// </param>
		/// <param name="channel">
		/// Channel to save,
		/// </param>
		/// <returns>
		/// Whether the save was successful.
		/// </returns>
		bool Save(string filename,IVoxelChannel channel);
		
		void SaveStream(Stream s,IVoxelChannel channel);
	}
}
