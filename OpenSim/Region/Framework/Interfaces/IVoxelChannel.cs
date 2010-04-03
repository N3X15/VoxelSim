using OpenMetaverse;
using System;

namespace OpenSim.Region.Framework
{
	/// <summary>
	/// 3D grid of voxels
	/// </summary>
	public interface IVoxelChannel
	{
		int Height { get; }		// Z
		int Length { get; } 	// Y
        int Width  { get; } 	// X
		
        bool this[int x, int y, int z] { get; set; }
        //IVoxel bool this[int x, int y, int z] { get; set; }

        /// <summary>
        /// Squash the entire voxelspace into a single dimensioned array
        /// </summary>
        /// <returns></returns>
        bool[] GetBoolsSerialised();

        double[,] GetDoubles();
		
        bool Tainted(int x, int y, int z);
        IVoxelChannel MakeCopy();
        string SaveToXmlString();
        void LoadFromXmlString(string data);
		
		IVoxelChannel Generate(string method);
		
		void Save(string name);
		void Load(string name);
		
		bool IsInsideTerrain(Vector3 pos);
		Vector3 FindNearestAirVoxel(Vector3 subject, bool ForAvatar);
	}
}
