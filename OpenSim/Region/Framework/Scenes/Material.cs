
using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace OpenSim.Region.CoreModules.World.Terrain
{
	public enum MaterialType
	{
		Soil,
		Sand,
		Igneous,
		Metamorphic,
		Sedimentary
	}
	
	public enum DepositType
	{
		Layer,
		SmallCluster,
		LargeCluster,
		Vein
	}
	public class Material
	{
		public string 		Name		= "Granite";
		public MaterialType Type		= MaterialType.Igneous;
		public float 		Density		= 2.75f;
		public List<UUID> 	Textures 	= new List<UUID>();
		public DepositType	Deposit		= DepositType.Layer;
		
		public Material()
		{
		}
	}
}
