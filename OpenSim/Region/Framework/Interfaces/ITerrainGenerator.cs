using System;

namespace OpenSim.Region.Framework.Interfaces
{
    public interface ITerrainGenerator
    {
        string Name { get; }

        void Initialize(MaterialMap matmap, long seed);
        void Generate(ref IVoxelChannel vc, long X, long Y);
        void Save(string Folder);
        void Load(string Folder);

        string Author { get; }
        string Version { get; }

        bool GenerateCaves      { get; set; }
        bool GenerateDungeons   { get; set; }
        bool GenerateOres       { get; set; }
        bool GenerateWater      { get; set; }
        bool HellMode           { get; set; }
        bool NoPreservation     { get; set; }
        bool GenerateTrees      { get; set; }
    }
}
