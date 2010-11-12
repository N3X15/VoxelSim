using System;
using System.Collections.Generic;
using LibNbt;
using LibNbt.Tags;
using LibNoise;
using System.Text;
using System.ComponentModel;
using System.IO;
using OpenSim.Region.Framework.Interfaces;
using System.Reflection;
using log4net;

namespace OpenSim.Region.Framework.Scenes
{
    public class HillGenerator : ITerrainGenerator
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected long Seed;

        protected Random rand;
        // Main terrain noise (two combined Perlin noises)
        protected FastNoise TerrainNoise;
        protected FastNoise ContinentNoise;
        protected Perlin CaveNoise;
        protected Perlin GravelNoise;
        protected Perlin TreeNoise;

        protected MaterialMap mMap;

        public double _CaveThreshold = 0.70d;
        public int WaterHeight = 63;
        public int DERT_DEPTH = 6;
        double _TerrainDivisor = 0.33;
        double _CaveDivisor = 2.0;
        double HeightDivisor = 1.5;
        private int ContinentNoiseOctaves;

        public double Frequency { get; set; }
        public NoiseQuality NoiseQuality { get; set; }
        public int OctaveCount { get; set; }
        public double Lacunarity { get; set; }
        public double Persistance { get; set; }
        public double ContinentNoiseFrequency { get; set; }
        public double CaveThreshold
        {
            get { return _CaveThreshold; }
            set { _CaveThreshold = value; }
        }
        [Description("Z-axis stretching of the terrain.  (z*TerrainDivisor)")]
        public double TerrainDivisor
        {
            get { return _TerrainDivisor; }
            set { _TerrainDivisor = value; }
        }
        [Description("Z-axis stretching of cave systems.  (z*CaveDivisor)")]
        public double CaveDivisor
        {
            get { return _CaveDivisor; }
            set { _CaveDivisor = value; }
        }
        public string Name
        {
            get
            {
                return "Default";
            }
        }

        public HillGenerator()
        {
            Frequency = 0.03;
            ContinentNoiseFrequency = Frequency / 2.0;
            Lacunarity = 0.01;
            Persistance = 0.01;
            OctaveCount = 1;
        }

        public HillGenerator(long seed)
        {
            Frequency = 0.03;
            Lacunarity = 0.01;
            Persistance = 0.01;
            OctaveCount = 1;
        }

        /// <summary>
        /// From the VoxelSim project
        /// http://github.com/N3X15/VoxelSim
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="chunksize"></param>
        /// <returns></returns>
        public void Generate(ref IVoxelChannel _mh, long X, long Y)
        {
            VoxelChannel mh = (VoxelChannel)_mh;

            m_log.InfoFormat("Generating terrain for a <{0},{1},{2}>m area.", mh.XScale, mh.YScale, mh.ZScale);

            bool PlaceGravel = ((GravelNoise.GetValue((X * mh.XScale), (Y * mh.YScale), 0) + 1) / 2.0) > 0.90d;

            int ZH = (int)mh.ZScale;
            byte[, ,] b = new byte[mh.XScale, mh.YScale, mh.ZScale];
            m_log.Debug("Stand by, generating terrain...");
            Console.WriteLine();
            for (int x = 0; x < mh.XScale; x++)
            {
                Console.CursorLeft = 0;
                Console.Write(" * Generating slice {0}/{1}... ({2}% complete)", x, mh.XScale, (int)System.Math.Round(((double)x / (double)mh.XScale) * 100d));
                for (int y = 0; y < mh.YScale; y++)
                {
                    for (int z = 0; z < ZH; z++)
                    {
                        int intensity = z * (255 / ZH);
                        double heightoffset = (ContinentNoise.GetValue(x + (X * mh.XScale), y + (Y * mh.YScale), 0) + 1d) / 2.0;
                        //Console.WriteLine("HeightOffset {0}",heightoffset);
                        //if (z == 0)
                        //    b[x, y, z] = 7;
                        ////else if (x == 0 && y == 0)
                        ////    b[x, y, z] = 1;
                        //else
                        //{
                            bool d1 = ((TerrainNoise.GetValue(x + (X * mh.XScale), y + (Y * mh.YScale), z * TerrainDivisor) + 1) / 2.0) > System.Math.Pow((((double)z * (HeightDivisor + (heightoffset))) / (double)ZH), 3d); // 3d
                            double _do = ((CaveNoise.GetValue(x + (X * mh.XScale), y + (Y * mh.YScale), z * CaveDivisor) + 1) / 2.0);
                            bool d3 = _do > CaveThreshold;
                            // XOR?
                            if (d1)//if (!(!d1 || !d2))
                            {
                                //Console.Write("#");
                                b[x, y, z] = (d3) ? b[x, y, z] : mMap.Rock.ID;
                                //if (x == 0|| y == 0)
                                //    b[x, y, z] = 41;
                            }
                            else if (z == 1)
                                b[x, y, z] = 11;
                        //}
                    }
                }
            }
            Console.WriteLine();
            //Console.WriteLine("Done generating chunk.  [{0},{1}]",min,max);
            for (int x = 0; x < mh.XScale; x++)
            {
                Console.CursorLeft = 0;
                Console.Write(" * Applying sediment to slice {0}/{1}... ({2}% complete)", x, mh.XScale, (int)System.Math.Round(((double)x / (double)mh.XScale) * 100d));
                //Console.WriteLine();
                for (int y = 0; y < mh.YScale; y++)
                {
                    bool HavePloppedGrass = false;
                    bool HaveTouchedSoil = false;
                    for (int z = (int)mh.ZScale - 1; z > 0; z--)
                    {
                        if (b[x, y, z] == mMap.Rock.ID)
                        {
                            HaveTouchedSoil = true;
                            if (z + DERT_DEPTH >= ZH)
                                continue;
                            byte ddt = b[x, y, z + DERT_DEPTH];
                            if (ddt == mMap.Air.ID || ddt == mMap.Water.ID)
                            {
                                if (z - DERT_DEPTH <= WaterHeight && GenerateWater)
                                    b[x, y, z] = mMap.Sand.ID; // (!PlaceGravel) ? ...
                                else
                                    b[x, y, z] = (HavePloppedGrass) ? mMap.Soil.ID : mMap.Grass.ID;
                                if (!HavePloppedGrass)
                                    HavePloppedGrass = true;
                            }
                            else
                            {
                                z = 0;
                            }
                        }
                        else if (b[x, y, z] == 0 && z <= WaterHeight && !HaveTouchedSoil && GenerateWater)
                        {
                            b[x, y, z] = mMap.Water.ID;
                        }
                    }
                }
            }
            Console.WriteLine();
            /*
            for (int x = 0; x < mh.XScale; x++)
            {
                for (int y = 0; y < mh.YScale; y++)
                {
                    int z = 1;
                    // TODO Yell at Notch for not making Lava occlude. :|
                    if (b[x, y, z] == 0)
                        b[x, y, z] = mMap.Lava.ID; // Lava for air.
                    else if (b[x, y, z] == mMap.Water.ID)
                        b[x, y, z] = mMap.Obsidian.ID; // Obsidian for underwater shit.
                }
            }
            */
            mh.SetTo(b);
        }
        /*
        public override void AddTrees(ref byte[, ,] b, int X, int Y, int H)
        {
            if (!GenerateTrees) return;
            for (int t = 0; t < (int)(TreeNoise.GetValue(X, Y, 0) * 10.0); t++)
            {
                int _x = rand.Next(2, 13);
                int _y = rand.Next(2, 13);
                for (int z = (int)H - 2; z > 0; z--)
                {
                    switch (b[_x, _y, z])
                    {
                        case 0: // Air
                            continue;
                        case 2:
                            Utils.GrowTree(ref b, rand, _x, _y, z + 1);
                            break;
                        case 51:
                            Utils.GrowCactus(ref b, rand, _x, _y, z + 1);
                            break;
                        default: break;
                    }
                }
            }
        }
        */
        public bool GenerateCaves
        {
            get;
            set;
        }

        public bool GenerateDungeons
        {
            get;
            set;
        }

        public bool GenerateOres
        {
            get;
            set;
        }

        public bool GenerateWater
        {
            get;
            set;
        }

        public bool HellMode
        {
            get;
            set;
        }

        public bool NoPreservation
        {
            get;
            set;
        }

        public bool GenerateTrees
        {
            get;
            set;
        }

        public void Save(string Folder)
        {
            string f = Path.Combine(Folder, "DefaultMapGenerator.dat");
            NbtFile nf = new NbtFile(f);
            nf.RootTag = new NbtCompound("__ROOT__");
            NbtCompound c = new NbtCompound("DefaultMapGenerator");
            c.Tags.Add(new NbtByte("GenerateCaves", (byte)(GenerateCaves ? 1 : 0)));
            c.Tags.Add(new NbtByte("GenerateDungeons", (byte)(GenerateDungeons ? 1 : 0)));
            c.Tags.Add(new NbtByte("GenerateOres", (byte)(GenerateOres ? 1 : 0)));
            c.Tags.Add(new NbtByte("GenerateWater", (byte)(GenerateWater ? 1 : 0)));
            c.Tags.Add(new NbtByte("HellMode", (byte)(HellMode ? 1 : 0)));
            c.Tags.Add(new NbtByte("GenerateTrees", (byte)(GenerateTrees ? 1 : 0)));
            c.Tags.Add(new NbtDouble("Frequency", Frequency));
            c.Tags.Add(new NbtByte("NoiseQuality", (byte)NoiseQuality));
            c.Tags.Add(new NbtInt("OctaveCount", OctaveCount));
            c.Tags.Add(new NbtDouble("Lacunarity", Lacunarity));
            c.Tags.Add(new NbtDouble("Persistance", Persistance));
            c.Tags.Add(new NbtDouble("ContinentNoiseFrequency", ContinentNoiseFrequency));
            c.Tags.Add(new NbtDouble("CaveThreshold", CaveThreshold));
            nf.RootTag.Tags.Add(c);
            nf.SaveFile(f);
        }

        public void Load(string Folder)
        {
            string f = Path.Combine(Folder, "DefaultMapGenerator.dat");
            if (!File.Exists(f)) return;

            NbtFile nf = new NbtFile(f);
            nf.LoadFile(f);
            GenerateCaves = nf.Query<NbtByte>("/DefaultMapGenerator/GenerateCaves").Value == 0x01 ? true : false;
            GenerateDungeons = nf.Query<NbtByte>("/DefaultMapGenerator/GenerateDungeons").Value == 0x01 ? true : false;
            GenerateOres = nf.Query<NbtByte>("/DefaultMapGenerator/GenerateOres").Value == 0x01 ? true : false;
            GenerateWater = nf.Query<NbtByte>("/DefaultMapGenerator/GenerateWater").Value == 0x01 ? true : false;
            HellMode = nf.Query<NbtByte>("/DefaultMapGenerator/HellMode").Value == 0x01 ? true : false;
            GenerateTrees = nf.Query<NbtByte>("/DefaultMapGenerator/GenerateTrees").Value == 0x01 ? true : false;
            Frequency = nf.Query<NbtDouble>("/DefaultMapGenerator/Frequency").Value;
            NoiseQuality = (NoiseQuality)nf.Query<NbtByte>("/DefaultMapGenerator/NoiseQuality").Value;
            OctaveCount = nf.Query<NbtInt>("/DefaultMapGenerator/OctaveCount").Value;
            Lacunarity = nf.Query<NbtDouble>("/DefaultMapGenerator/Lacunarity").Value;
            Persistance = nf.Query<NbtDouble>("/DefaultMapGenerator/Persistance").Value;
            ContinentNoiseFrequency = nf.Query<NbtDouble>("/DefaultMapGenerator/ContinentNoiseFrequency").Value;
            CaveThreshold = nf.Query<NbtDouble>("/DefaultMapGenerator/CaveThreshold").Value;
        }
        public string Author
        {
            get { return "Rob \"N3X15\" Nelson"; }
        }

        public string Version
        {
            get { return "07292010"; }
        }

        public void Initialize(MaterialMap matmap, long seed)
        {
            mMap = matmap;
            this.Seed = seed;

            Frequency = 0.03;
            ContinentNoiseFrequency = Frequency / 2.0;
            Lacunarity = 0.01;
            Persistance = 0.01;
            OctaveCount = 1;

            Seed = seed;
            TerrainNoise = new FastNoise();
            ContinentNoise = new FastNoise();
            CaveNoise = new Perlin();
            GravelNoise = new Perlin();
            TreeNoise = new Perlin();
            TerrainNoise.Seed = (int)Seed;
            ContinentNoise.Seed = (int)Seed + 2;
            CaveNoise.Seed = (int)Seed + 3;
            GravelNoise.Seed = (int)Seed + 4;
            TreeNoise.Seed = (int)Seed + 4;
            rand = new Random((int)Seed);


            TerrainNoise.Frequency = Frequency;
            TerrainNoise.NoiseQuality = NoiseQuality;
            TerrainNoise.OctaveCount = OctaveCount;
            TerrainNoise.Lacunarity = Lacunarity;
            TerrainNoise.Persistence = Persistance;

            ContinentNoise.Frequency = ContinentNoiseFrequency;
            ContinentNoise.NoiseQuality = NoiseQuality;
            ContinentNoise.OctaveCount = OctaveCount;
            ContinentNoise.Lacunarity = Lacunarity;
            ContinentNoise.Persistence = Persistance;

            CaveNoise.Frequency = Frequency;
            CaveNoise.NoiseQuality = NoiseQuality;
            CaveNoise.OctaveCount = OctaveCount + 2;
            CaveNoise.Lacunarity = Lacunarity;
            CaveNoise.Persistence = Persistance;

            GravelNoise.Frequency = Frequency;
            GravelNoise.NoiseQuality = NoiseQuality;
            GravelNoise.OctaveCount = OctaveCount;
            GravelNoise.Lacunarity = Lacunarity;
            GravelNoise.Persistence = Persistance;

            TreeNoise.Frequency = Frequency + 2;
            TreeNoise.NoiseQuality = NoiseQuality;
            TreeNoise.OctaveCount = OctaveCount;
            TreeNoise.Lacunarity = Lacunarity;
            TreeNoise.Persistence = Persistance;
        }
    }
}
