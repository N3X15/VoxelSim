
using System;
using OpenMetaverse;

namespace OpenSim.Region.Framework.Scenes
{
	[Flags]
	public enum	VoxFlags :byte
	{
		Solid	= 0x01,
		Fluid	= 0x02,
		Damp	= 0x04,
		Toxic	= 0x08
	}
	
	public class Voxel
	{
		public Voxel(){}
		public Voxel(byte[] b)
		{
			Flags=(VoxFlags)b[0];
			MaterialID=b[1];
		}
		public byte[] asBytes()
		{
			// Flags Material
			// 0x00 0x00
			return new byte[]{(byte)Flags,MaterialID};
		}
		public Vector3		Position=new Vector3();
		public VoxFlags		Flags=0;
		public byte	   		MaterialID=0x00;
		public int			Temp=0;
		public bool			ERROR=false;
		
		public delegate void MoveDelegate(Vector3 from, Vector3 to);
		public event MoveDelegate Moved;
		
		public delegate void RemoveDelegate();
		public event RemoveDelegate Removed;
		
		public delegate void ModifyDelegate(VoxFlags Flags,byte Material);
		public event ModifyDelegate Modified;
	}
}
