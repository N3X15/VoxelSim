
using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace OpenSim.Region.Framework.Scenes
{
	public enum MaterialType: int
	{
		Soil,
		Sand,
		Igneous,
		Metamorphic,
		Sedimentary
	}
	
	public enum DepositType: int
	{
		Layer,
		SmallCluster,
		LargeCluster,
		Vein
	}
	[Flags]
	public enum	MatFlags :byte
	{
		Solid	= 0x01,
		Fluid	= 0x02,
		Damp	= 0x04,
		Toxic	= 0x08
	}
	public class VoxMaterial
	{
		public byte			ID			= 0x00;
		public string 		Name		= "Granite";
		public MaterialType Type		= MaterialType.Igneous;
		public float 		Density		= 2.75f;
		public UUID 		Texture		= UUID.Zero;
		public DepositType	Deposit		= DepositType.Layer;
		public MatFlags 	Flags		= (MatFlags)0x00;
	}
}
