/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Net;
using log4net;
using Nini.Config;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Region.CoreModules.Framework.InterfaceCommander;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Framework.Servers.HttpServer;
using OpenMetaverse.StructuredData;
using Caps=OpenSim.Framework.Capabilities.Caps;
using System.Web;
using System.Collections.Specialized;

namespace OpenSim.Region.CoreModules.World.Voxels
{
    public class VoxelModule : INonSharedRegionModule, ICommandableModule, IVoxelModule
    {

        private static readonly string m_GetChunkCap = "9001";
        private static readonly string m_GetMaterialsCap = "9002";
        private static readonly string m_UploadMaterialsCap = "9003";
        private static readonly string m_UploadHeightmapCap = "9004";

        /// <summary>
        /// A standard set of terrain brushes and effects recognised by viewers
        /// </summary>
        public enum StandardVoxelActions : byte
        {
            Add 	= 0,
            Remove 	= 1,
			TNT		= 2
        }

        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Commander m_commander = new Commander("voxels");


        private readonly Dictionary<StandardVoxelActions, IVoxelAction> m_painteffects =
            new Dictionary<StandardVoxelActions, IVoxelAction>();

		private Dictionary<string,IVoxelFileHandler> m_loaders = new Dictionary<string, IVoxelFileHandler>();
        private VoxelChannel m_channel;
        private VoxelChannel m_revert;
        private Scene m_scene;
        private volatile bool m_tainted;
        private volatile bool[,] m_changedChunks;
        private readonly UndoStack<VoxelUndoState> m_undo = new UndoStack<VoxelUndoState>(5);

        #region ICommandableModule Members

        public ICommander CommandInterface
        {
            get { return m_commander; }
        }

        #endregion

        #region INonSharedRegionModule Members

        /// <summary>
        /// Creates and initialises a terrain module for a region
        /// </summary>
        /// <param name="scene">Region initialising</param>
        /// <param name="config">Config for the region</param>
        public void Initialise(IConfigSource config)
        {
        }

        public void AddRegion(Scene scene)
        {
            m_scene = scene;

            // Install terrain module in the simulator
            lock (m_scene)
            {
                if (m_scene.Voxels == null)
                {
                    m_channel = new VoxelChannel(Constants.RegionSize,Constants.RegionSize,256);
                    m_revert = new VoxelChannel(Constants.RegionSize,Constants.RegionSize,256);
                    m_scene.Voxels = m_channel;
                    UpdateRevertMap();
                }
                else
                {
                    m_channel = (VoxelChannel)m_scene.Voxels;
                    m_revert = new VoxelChannel(Constants.RegionSize,Constants.RegionSize,256);
                    UpdateRevertMap();
                }

				
                m_scene.RegisterModuleInterface<VoxelModule>(this);
                m_scene.EventManager.OnNewClient += EventManager_OnNewClient;
                m_scene.EventManager.OnPluginConsole += EventManager_OnPluginConsole;
                m_scene.EventManager.OnTerrainTick += EventManager_OnTerrainTick;
				m_scene.EventManager.OnRegisterCaps += HandleM_sceneEventManagerOnRegisterCaps;
                InstallInterfaces();
            }

            //InstallDefaultEffects();
            //LoadPlugins();
        }

        void HandleM_sceneEventManagerOnRegisterCaps (UUID agentID, Caps caps)
        {
            string capsBase = "/CAPS/VOX/";
        	caps.RegisterHandler("MatTable",new RestStreamHandler("POST",capsBase+"/GetMats/",HandleMatTableReq));
            caps.RegisterHandler("VoxelChunk", new RestStreamHandler("POST", capsBase + "/GetChunk/", HandleMatTableReq));
        	caps.RegisterHandler("SetMaterial",new RestStreamHandler("POST",capsBase+"/SetMat/",HandleSetMatTableReq));
        }

		string HandleSetMatTableReq(string request, string path, string param,
                                      OSHttpRequest req, OSHttpResponse res)
		{
			OSDMap input = (OSDMap)OSDParser.DeserializeLLSDXml(request);
			VoxMaterial mat = new VoxMaterial();
			mat.Name		= input["name"].AsString();
			mat.Deposit		= (DepositType)input["deposit"].AsInteger();
			mat.Density		= (float)input["density"].AsReal();
			mat.Flags		= (MatFlags)input["flags"].AsBinary()[0];
			mat.Texture		= input["texture"].AsUUID();
			mat.Type		= (MaterialType)input["type"].AsBinary()[0];
			if(input["id"].AsBinary()[0]==0x00) // 0x00=AIR_VOXEL, so we cannot set it.
				(m_scene.Voxels as VoxelChannel).AddMaterial(mat);
			else
			{
				byte id = input["id"].AsBinary()[0];
				(m_scene.Voxels as VoxelChannel).mMaterials[id]=mat;
			}
			return "OK";
		}


        private void SendChunk(OSHttpRequest request, OSHttpResponse response, int X, int Y)
        {
            string range = request.Headers.GetOne("Range");
            //m_log.DebugFormat("[GETTEXTURE]: Range {0}", range);
            byte[] chunk = m_scene.Voxels.GetChunk(X, Y);
            if (!String.IsNullOrEmpty(range))
            {
                // Range request
                int start, end;
                if (TryParseRange(range, out start, out end))
                {
                    // Before clamping start make sure we can satisfy it in order to avoid
                    // sending back the last byte instead of an error status
                    if (start >= chunk.Length)
                    {
                        response.StatusCode = (int)System.Net.HttpStatusCode.RequestedRangeNotSatisfiable;
                        return;
                    }

                    end = Utils.Clamp(end, 0, chunk.Length - 1);
                    start = Utils.Clamp(start, 0, end);
                    int len = end - start + 1;

                    //m_log.Debug("Serving " + start + " to " + end + " of " + texture.Data.Length + " bytes for texture " + texture.ID);

                    if (len < chunk.Length)
                        response.StatusCode = (int)System.Net.HttpStatusCode.PartialContent;

                    response.ContentLength = len;
                    response.ContentType = "application/octet-stream";
                    response.AddHeader("Content-Range", String.Format("bytes {0}-{1}/{2}", start, end, chunk.Length));

                    response.Body.Write(chunk, start, len);
                }
                else
                {
                    m_log.Warn("Malformed Range header: " + range);
                    response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                }
            }
            else
            {
                // Full content request
                response.ContentLength = chunk.Length;
                response.ContentType = "application/octet-stream";
                response.Body.Write(chunk, 0, chunk.Length);
            }
        }

        private bool TryParseRange(string header, out int start, out int end)
        {
            if (header.StartsWith("bytes="))
            {
                string[] rangeValues = header.Substring(6).Split('-');
                if (rangeValues.Length == 2)
                {
                    if (Int32.TryParse(rangeValues[0], out start) && Int32.TryParse(rangeValues[1], out end))
                        return true;
                }
            }

            start = end = 0;
            return false;
        }
        string HandleMatTableReq(string request, string path, string param,
                                      OSHttpRequest req, OSHttpResponse res)
        {
            return (m_scene.Voxels as VoxelChannel).mMaterials.ToString();
        }
        string HandleVoxelChunkReq(string request, string path, string param,
                                      OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            // Try to parse the texture ID from the request URL
            NameValueCollection query = HttpUtility.ParseQueryString(httpRequest.Url.Query);
            int X, Y,Z=0;
            if (!int.TryParse(query.GetOne("x"), out X) ||
                !int.TryParse(query.GetOne("y"), out Y))
            {
                httpResponse.StatusCode = 404;
                httpResponse.Send();
                return null;
            }
            if (X < 0 ||
                X > m_scene.Voxels.Width / VoxelChannel.CHUNK_SIZE_X ||
                Y < 0 ||
                Y > m_scene.Voxels.Length / VoxelChannel.CHUNK_SIZE_Y ||
                Z < 0 ||
                Z > m_scene.Voxels.Height / VoxelChannel.CHUNK_SIZE_Z)
            {
                httpResponse.StatusCode = 404;
                httpResponse.Send();
                return null;
            }

            SendChunk(httpRequest, httpResponse, X, Y);

            httpResponse.Send();
            return null;
        }
        public void RegionLoaded(Scene scene)
        {
        }

        public void RemoveRegion(Scene scene)
        {
            lock (m_scene)
            {
                // remove the commands
                m_scene.UnregisterModuleCommander(m_commander.Name);
                // remove the event-handlers
                m_scene.EventManager.OnTerrainTick -= EventManager_OnTerrainTick;
                m_scene.EventManager.OnPluginConsole -= EventManager_OnPluginConsole;
                m_scene.EventManager.OnNewClient -= EventManager_OnNewClient;
                // remove the interface
                m_scene.UnregisterModuleInterface<VoxelModule>(this);
            }
        }

        public void Close()
        {
        }

        public Type ReplaceableInterface 
        {
            get { return null; }
        }

        public string Name
        {
            get { return "VoxelModule"; }
        }

        #endregion

        //#region ITerrainModule Members

        public void UndoTerrain(IVoxelChannel channel)
        {
            m_channel = (VoxelChannel)channel;
        }

        /// <summary>
        /// Loads a terrain file from disk and installs it in the scene.
        /// </summary>
        /// <param name="filename">Filename to terrain file. Type is determined by extension.</param>
        public void LoadFromFile(string filename)
        {
            m_channel.LoadFromFile(filename);

            //m_log.Error("[TERRAIN]: Unable to load voxelspace, no file loader available for that format.");
            //throw new TerrainException(String.Format("unable to load heightmap from file {0}: no loader available for that format", filename));
        }

        /// <summary>
        /// Saves the current heightmap to a specified file.
        /// </summary>
        /// <param name="filename">The destination filename</param>
        public void SaveToFile(string filename)
        {
            try
            {
               m_channel.SaveToFile(filename);
            }
            catch (NotImplementedException)
            {
                m_log.Error("Unable to save to " + filename + ", saving of this file format has not been implemented.");
                throw new Exception(String.Format("Unable to save heightmap: saving of this file format not implemented"));
            }
            catch (IOException ioe)
            {
                m_log.Error(String.Format("[TERRAIN]: Unable to save to {0}, {1}", filename, ioe.Message));
                throw new Exception(String.Format("Unable to save heightmap: {0}", ioe.Message));
            }
        }

        /// <summary>
        /// Loads a terrain file from the specified URI
        /// </summary>
        /// <param name="filename">The name of the terrain to load</param>
        /// <param name="pathToTerrainHeightmap">The URI to the terrain height map</param>
        public void LoadFromStream(string filename, Uri pathToTerrainHeightmap)
        {
            LoadFromStream(filename, URIFetch(pathToTerrainHeightmap));
        }

        /// <summary>
        /// Loads a terrain file from a stream and installs it in the scene.
        /// </summary>
        /// <param name="filename">Filename to terrain file. Type is determined by extension.</param>
        /// <param name="stream"></param>
        public void LoadFromStream(string filename, Stream stream)
        {
            foreach (KeyValuePair<string, IVoxelFileHandler> loader in m_loaders)
            {
                if (filename.EndsWith(loader.Key))
                {
                    lock (m_scene)
                    {
                        try
                        {
                            IVoxelChannel channel = loader.Value.LoadStream(stream);
                            m_scene.Voxels = channel;
                            m_channel = (VoxelChannel)channel;
                            UpdateRevertMap();
                        }
                        catch (NotImplementedException)
                        {
                            m_log.Error("[TERRAIN]: Unable to load voxelmap, the " + loader.Value +
                                        " parser does not support file loading. (May be save only)");
                            throw new Exception(String.Format("unable to load heightmap: parser {0} does not support loading", loader.Value));
                        }
                    }

                    CheckForTerrainUpdates();
                    m_log.Info("[TERRAIN]: File (" + filename + ") loaded successfully");
                    return;
                }
            }
            m_log.Error("[TERRAIN]: Unable to load heightmap, no file loader available for that format.");
            throw new Exception(String.Format("unable to load heightmap from file {0}: no loader available for that format", filename));
        }

        private static Stream URIFetch(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            // request.Credentials = credentials;

            request.ContentLength = 0;
            request.KeepAlive = false;

            WebResponse response = request.GetResponse();
            Stream file = response.GetResponseStream();

            if (response.ContentLength == 0)
                throw new Exception(String.Format("{0} returned an empty file", uri.ToString()));

            // return new BufferedStream(file, (int) response.ContentLength);
            return new BufferedStream(file, 1000000);
        }

        /// <summary>
        /// Modify Land
        /// </summary>
        /// <param name="pos">Land-position (X,Y,0)</param>
        /// <param name="size">The size of the brush (0=small, 1=medium, 2=large)</param>
        /// <param name="action">0=LAND_LEVEL, 1=LAND_RAISE, 2=LAND_LOWER, 3=LAND_SMOOTH, 4=LAND_NOISE, 5=LAND_REVERT</param>
        /// <param name="agentId">UUID of script-owner</param>
        public void ModifyTerrain(UUID user, Vector3 pos, byte size, byte action, UUID agentId)
        {
            float duration = 0.25f;
            if (action == 0)
                duration = 4.0f;
			client_OnModifyTerrain(user,(int)pos.X,(int)pos.Y,(int)pos.Z,action,(double)duration,agentId);
        }

        /// <summary>
        /// Saves the current heightmap to a specified stream.
        /// </summary>
        /// <param name="filename">The destination filename.  Used here only to identify the image type</param>
        /// <param name="stream"></param>
        public void SaveToStream(string filename, Stream stream)
        {
            try
            {
                foreach (KeyValuePair<string, IVoxelFileHandler> loader in m_loaders)
                {
                    if (filename.EndsWith(loader.Key))
                    {
                        loader.Value.SaveStream(stream, m_channel);
                        return;
                    }
                }
            }
            catch (NotImplementedException)
            {
                m_log.Error("Unable to save to " + filename + ", saving of this file format has not been implemented.");
                throw new Exception(String.Format("Unable to save heightmap: saving of this file format not implemented"));
            }
        }

        public void TaintTerrain ()
        {
            CheckForTerrainUpdates();
        }
/*
        #region Plugin Loading Methods

        private void LoadPlugins()
        {
            m_plugineffects = new Dictionary<string, ITerrainEffect>();
            // Load the files in the Terrain/ dir
            string[] files = Directory.GetFiles("Terrain");
            foreach (string file in files)
            {
                m_log.Info("Loading effects in " + file);
                try
                {
                    Assembly library = Assembly.LoadFrom(file);
                    foreach (Type pluginType in library.GetTypes())
                    {
                        try
                        {
                            if (pluginType.IsAbstract || pluginType.IsNotPublic)
                                continue;

                            string typeName = pluginType.Name;

                            if (pluginType.GetInterface("ITerrainEffect", false) != null)
                            {
                                ITerrainEffect terEffect = (ITerrainEffect) Activator.CreateInstance(library.GetType(pluginType.ToString()));

                                InstallPlugin(typeName, terEffect);
                            }
                            else if (pluginType.GetInterface("ITerrainLoader", false) != null)
                            {
                                ITerrainLoader terLoader = (ITerrainLoader) Activator.CreateInstance(library.GetType(pluginType.ToString()));
                                m_loaders[terLoader.FileExtension] = terLoader;
                                m_log.Info("L ... " + typeName);
                            }
                        }
                        catch (AmbiguousMatchException)
                        {
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                }
            }
        }

        public void InstallPlugin(string pluginName, ITerrainEffect effect)
        {
            lock (m_plugineffects)
            {
                if (!m_plugineffects.ContainsKey(pluginName))
                {
                    m_plugineffects.Add(pluginName, effect);
                    m_log.Info("E ... " + pluginName);
                }
                else
                {
                    m_plugineffects[pluginName] = effect;
                    m_log.Warn("E ... " + pluginName + " (Replaced)");
                }
            }
        }

        #endregion

        #endregion
*/
        /// <summary>
        /// Installs into terrain module the standard suite of brushes
        /// </summary>
        private void InstallDefaultEffects()
        {
            /*// Draggable Paint Brush Effects
            m_painteffects[StandardTerrainEffects.Raise] = new RaiseSphere();
            m_painteffects[StandardTerrainEffects.Lower] = new LowerSphere();
            m_painteffects[StandardTerrainEffects.Smooth] = new SmoothSphere();
            m_painteffects[StandardTerrainEffects.Noise] = new NoiseSphere();
            m_painteffects[StandardTerrainEffects.Flatten] = new FlattenSphere();
            m_painteffects[StandardTerrainEffects.Revert] = new RevertSphere(m_revert);
            m_painteffects[StandardTerrainEffects.Erode] = new ErodeSphere();
            m_painteffects[StandardTerrainEffects.Weather] = new WeatherSphere();
            m_painteffects[StandardTerrainEffects.Olsen] = new OlsenSphere();

            // Area of effect selection effects
            m_floodeffects[StandardTerrainEffects.Raise] = new RaiseArea();
            m_floodeffects[StandardTerrainEffects.Lower] = new LowerArea();
            m_floodeffects[StandardTerrainEffects.Smooth] = new SmoothArea();
            m_floodeffects[StandardTerrainEffects.Noise] = new NoiseArea();
            m_floodeffects[StandardTerrainEffects.Flatten] = new FlattenArea();
            m_floodeffects[StandardTerrainEffects.Revert] = new RevertArea(m_revert);
			*/
            // Filesystem load/save loaders
            m_loaders[".osvox"] = new NBTFileHandler();
           /*m_loaders[".f32"] = m_loaders[".r32"];
            m_loaders[".ter"] = new Terragen();
            m_loaders[".raw"] = new LLRAW();
            m_loaders[".jpg"] = new JPEG();
            m_loaders[".jpeg"] = m_loaders[".jpg"];
            m_loaders[".bmp"] = new BMP();
            m_loaders[".png"] = new PNG();
            m_loaders[".gif"] = new GIF();
            m_loaders[".tif"] = new TIFF();
            m_loaders[".tiff"] = m_loaders[".tif"];*/
        }

        /// <summary>
        /// Saves the current state of the region into the revert map buffer.
        /// </summary>
        public void UpdateRevertMap()
        {
            for (int z = 0; z < m_channel.Height; z++)
                for (int x = 0; x < m_channel.Width; x++)
                	for (int y = 0; y < m_channel.Height; y++)
                    	m_revert[x, y, z] = m_channel[x, y, z];
        }
/*
        /// <summary>
        /// Loads a tile from a larger terrain file and installs it into the region.
        /// </summary>
        /// <param name="filename">The terrain file to load</param>
        /// <param name="fileWidth">The width of the file in units</param>
        /// <param name="fileHeight">The height of the file in units</param>
        /// <param name="fileStartX">Where to begin our slice</param>
        /// <param name="fileStartY">Where to begin our slice</param>
        public void LoadFromFile(string filename, int fileWidth, int fileHeight, int fileStartX, int fileStartY)
        {
            int offsetX = (int) m_scene.RegionInfo.RegionLocX - fileStartX;
            int offsetY = (int) m_scene.RegionInfo.RegionLocY - fileStartY;

            if (offsetX >= 0 && offsetX < fileWidth && offsetY >= 0 && offsetY < fileHeight)
            {
                // this region is included in the tile request
                foreach (KeyValuePair<string, ITerrainLoader> loader in m_loaders)
                {
                    if (filename.EndsWith(loader.Key))
                    {
                        lock (m_scene)
                        {
                            ITerrainChannel channel = loader.Value.LoadFile(filename, offsetX, offsetY,
                                                                            fileWidth, fileHeight,
                                                                            (int) Constants.RegionSize,
                                                                            (int) Constants.RegionSize);
                            m_scene.Heightmap = channel;
                            m_channel = channel;
                            UpdateRevertMap();
                        }
                        return;
                    }
                }
            }
        }
*/
        /// <summary>
        /// Performs updates to the region periodically, synchronising physics and other heightmap aware sections
        /// </summary>
        private void EventManager_OnTerrainTick()
        {
            if (m_tainted)
            {
                m_tainted = false;
                m_scene.PhysicsScene.SetTerrain(m_channel.GetFloatsSerialised());
                m_scene.SaveTerrain();

                // Clients who look at the map will never see changes after they looked at the map, so i've commented this out.
                //m_scene.CreateTerrainTexture(true);
            }
        }

        /// <summary>
        /// Processes commandline input. Do not call directly.
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        private void EventManager_OnPluginConsole(string[] args)
        {
            if (args[0] == "terrain")
            {
                if (args.Length == 1)
                {
                    m_commander.ProcessConsoleCommand("help", new string[0]);
                    return;
                }

                string[] tmpArgs = new string[args.Length - 2];
                int i;
                for (i = 2; i < args.Length; i++)
                    tmpArgs[i - 2] = args[i];

                m_commander.ProcessConsoleCommand(args[1], tmpArgs);
            }
        }

        /// <summary>
        /// Installs terrain brush hook to IClientAPI
        /// </summary>
        /// <param name="client"></param>
        private void EventManager_OnNewClient(IClientAPI client)
        {
            client.OnModifyTerrain += delegate(UUID user, float height, float seconds, byte size, byte action, float north, float west, float south, float east, UUID agentId) {
				client_OnModifyTerrain(user,(int)west,(int)south,(int)height,action,(double)seconds,agentId);
			};
            client.OnBakeTerrain += client_OnBakeTerrain;
            client.OnLandUndo += client_OnLandUndo;
        }

        /// <summary>
        /// Checks to see if the terrain has been modified since last check
        /// but won't attempt to limit those changes to the limits specified in the estate settings
        /// currently invoked by the command line operations in the region server only
        /// </summary>
        private void CheckForTerrainUpdates()
        {

            List<VoxelUpdate> FineChanges = new List<VoxelUpdate>();
            List<ChunkUpdate> ChunkChanges = new List<ChunkUpdate>();

            // NumVoxelsPerChunk/4 = Maximum fine changes PER UPDATE allowed.
            int MaxFineChangesAllowed = (VoxelChannel.CHUNK_SIZE_X * VoxelChannel.CHUNK_SIZE_Y * VoxelChannel.CHUNK_SIZE_Z) / 4;
            int NumFineChanges = 0;

            // For each chunk...
            for (int cx = 0; cx < (m_channel.Width / VoxelChannel.CHUNK_SIZE_X); cx++)
            {
                for (int cy = 0; cy < (m_channel.Length / VoxelChannel.CHUNK_SIZE_Y); cy++)
                {
                    // Get voxels in each chunk...
                    byte[] data_a = m_channel.GetChunkData(cx, cy);
                    byte[] data_b = m_revert.GetChunkData(cx, cy);

                    // Compare
                    for (int x = 0; x < VoxelChannel.CHUNK_SIZE_X; x++)
                    {
                        for (int y = 0; y < VoxelChannel.CHUNK_SIZE_Y; y++)
                        {
                            for (int z = 0; z < VoxelChannel.CHUNK_SIZE_Z; z++)
                            {
                                // Compare newest voxel with revert map voxel
                                byte a = m_channel.GetChunkBlock(ref data_a, x, y, z);
                                if (a != m_channel.GetChunkBlock(ref data_b, x, y, z))
                                {
                                    // Too many fine updates?
                                    NumFineChanges++;
                                    if (NumFineChanges > MaxFineChangesAllowed)
                                    {
                                        // Clear fine updates, reset counter, and add a coarse update.
                                        FineChanges.Clear();
                                        NumFineChanges = 0;
                                        ChunkUpdate cu = new ChunkUpdate();
                                        cu.X = cx;
                                        cu.Y = cy;
                                        cu.Z = 0;
                                        ChunkChanges.Add(cu);
                                    }
                                    else
                                    {
                                        // Otherwise, add a fine update.
                                        VoxelUpdate vu = new VoxelUpdate();
                                        vu.X = x;
                                        vu.Y = y;
                                        vu.Z = z;
                                        vu.Type = a;
                                        FineChanges.Add(vu);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            m_scene.ForEachClient(delegate(IClientAPI cli)
            {
                foreach (VoxelUpdate up in FineChanges)
                {
                    cli.SendVoxelUpdate(up.X, up.Y, up.Z, up.Type);
                }
                foreach (ChunkUpdate up in ChunkChanges)
                {
                    cli.SendChunkUpdate(up.X, up.Y, up.Z);
                }
            });
        }
		
        /// <summary>
        /// Checks to see height deltas in the tainted terrain patch at xStart ,yStart
        /// are all within the current estate limits
        /// <returns>true if changes were limited, false otherwise</returns>
        /// </summary>
        private bool LimitChannelChanges(int z)
        {
			// TODO: This needs work.
			return false;
			
            bool changesLimited = false;
            double minDelta = m_scene.RegionInfo.RegionSettings.TerrainLowerLimit;
            double maxDelta = m_scene.RegionInfo.RegionSettings.TerrainRaiseLimit;

            // loop through the height map for this patch and compare it against
            // the revert map
            for (int x = 0; x < m_channel.Width; x++)
            {
                for (int y = 0; y < m_channel.Length; y++)
                {
                    double requestedHeight = m_channel.GetHeightAt(x, y);
                    double bakedHeight = m_revert.GetHeightAt(x, y);
					int MyVoxel=m_channel[x,y,z];
					int BakedVoxel=m_revert[x,y,z];
                
					double requestedDelta = requestedHeight - bakedHeight;

                    if (requestedDelta > maxDelta)
                    {
                        m_channel.SetVoxel(x, y, z, (byte)BakedVoxel);
                        changesLimited = true;
                    }
                    else if (requestedDelta < minDelta)
                    {
                        m_channel.SetVoxel(x, y, z, (byte)BakedVoxel);
                        changesLimited = true;
                    }
                }
            }

            return changesLimited;
        }

        private void client_OnLandUndo(IClientAPI client)
        {
            lock (m_undo)
            {
                if (m_undo.Count > 0)
                {
                    VoxelUndoState goback = m_undo.Pop();
                    if (goback != null)
                        goback.PlaybackState();
                }
            }
        }

        private void client_OnModifyTerrain(UUID user, int x, int y, int z, byte action, double str, UUID agentId)
        {
            bool god = m_scene.Permissions.IsGod(user);
            bool allowed = false;
            if (m_painteffects.ContainsKey((StandardVoxelActions) action))
            {
                bool[,] allowMask = new bool[m_channel.Width,m_channel.Length];
                allowMask.Initialize();

                if (x>=0 && y>=0 && z>=0 && x<m_channel.Width && y<m_channel.Length && z<m_channel.Height)
                {
                    if (m_scene.Permissions.CanTerraformLand(agentId, new Vector3(x,y,0)))
                    {
                        allowMask[x, y] = true;
                        allowed = true;
                    }
                }
                if (allowed)
                {
                    StoreUndoState();
                    m_painteffects[(StandardVoxelActions) action].PaintEffect(
                        m_channel, allowMask, x,y,z,str);

                    CheckForTerrainUpdates(); //revert changes outside estate limits
                }
            }
        }

        private void client_OnBakeTerrain(IClientAPI remoteClient)
        {
            // Not a good permissions check (see client_OnModifyTerrain above), need to check the entire area.
            // for now check a point in the centre of the region

            if (m_scene.Permissions.CanIssueEstateCommand(remoteClient.AgentId, true))
            {
                InterfaceBakeTerrain(null); //bake terrain does not use the passed in parameter
            }
        }

        private void StoreUndoState()
        {
            lock (m_undo)
            {
                if (m_undo.Count > 0)
                {
                    VoxelUndoState last = m_undo.Peek();
                    if (last != null)
                    {
                        if (last.Compare(m_channel))
                            return;
                    }
                }

                VoxelUndoState nUndo = new VoxelUndoState(this, m_channel);
                m_undo.Push(nUndo);
            }
        }

        #region Console Commands

        private void InterfaceLoadFile(Object[] args)
        {
            LoadFromFile((string) args[0]);
            CheckForTerrainUpdates();
        }

        private void InterfaceLoadTileFile(Object[] args)
        {
			return;
			/*
            LoadFromFile((string) args[0],
                         (int) args[1],
                         (int) args[2],
                         (int) args[3],
                         (int) args[4]);
            CheckForTerrainUpdates();*/
        }

        private void InterfaceSaveFile(Object[] args)
        {
            SaveToFile((string) args[0]);
        }

        private void InterfaceBakeTerrain(Object[] args)
        {
            UpdateRevertMap();
        }

        private void InterfaceRevertTerrain(Object[] args)
        {
            int x, y, z;
            for (x = 0; x < m_channel.Width; x++)
            	for (y = 0; y < m_channel.Length; y++)
                	for (z = 0; z < m_channel.Height; z++)
                    m_channel[x, y,z] = m_revert[x, y,z];

            CheckForTerrainUpdates();
        }

        private void InterfaceFlipTerrain(Object[] args)
        {
            String direction = (String)args[0];

            if (direction.ToLower().StartsWith("y"))
            {
                for (int x = 0; x < Constants.RegionSize; x++)
                {
                    for (int y = 0; y < Constants.RegionSize / 2; y++)
                    {
                    	for (int z = 0; z < Constants.RegionSize; z++)
                    	{
	                        byte vox = m_channel.GetVoxel(x, y, z);
	                        byte flippedVox = m_channel.GetVoxel(x, (int)Constants.RegionSize - 1 - y, z);
	                        m_channel.SetVoxel(x, y, z, flippedVox);
	                        m_channel.SetVoxel(x, (int)Constants.RegionSize - 1 - y, z, vox);
                    	}
					}
                }
            }
            else if (direction.ToLower().StartsWith("x"))
            {
                for (int y = 0; y < Constants.RegionSize; y++)
                {
                    for (int x = 0; x < Constants.RegionSize / 2; x++)
                    {
                    	for (int z = 0; z < Constants.RegionSize; z++)
                    	{
	                        byte v = m_channel.GetVoxel(x, y, z);
	                        byte flippedVox = m_channel.GetVoxel((int)Constants.RegionSize - 1 - x, y, z);
	                        m_channel.SetVoxel(x, y, z, flippedVox);
	                        m_channel.SetVoxel((int)Constants.RegionSize - 1 - x, y, z, v);
						}
                    }
                }
            }
            else
            {
                m_log.Error("Unrecognised direction - need x or y");
            }


            CheckForTerrainUpdates();
        }

        private void InterfaceRescaleTerrain(Object[] args)
        {
			m_log.Error("Unimplemented for voxels.");
        }

        private void InterfaceElevateTerrain(Object[] args)
        {
            int x, y, z;
			int zo = (int)args[0];
            for (x = 0; x < m_channel.Width; x++)
			{
                for (y = 0; y < m_channel.Length; y++)
				{
					// Move voxels up X units.
					for(z=m_channel.Height-1;z>0;z--)
					{
						// just overwrite stuff that would be moved out of bounds
						if(z+zo > m_channel.Height-1)
							continue;
                    	m_channel.SetVoxel(x, y, z+zo, m_channel.GetVoxel(x,y,z));
					}
				}
			}
            CheckForTerrainUpdates();
        }

        private void InterfaceMultiplyTerrain(Object[] args)
        {
			m_log.Error("Unimplemented for voxels.");
			/*
            int x, y;
            for (x = 0; x < m_channel.Width; x++)
                for (y = 0; y < m_channel.Height; y++)
                    m_channel[x, y] *= (double) args[0];
            */
            CheckForTerrainUpdates();
        }

        private void InterfaceLowerTerrain(Object[] args)
        {
            int x, y, z;
			int zo = (int)args[0];
            for (x = 0; x < m_channel.Width; x++)
			{
                for (y = 0; y < m_channel.Length; y++)
				{
					// Move voxels down X units.
					for(z=0;z<m_channel.Height;z++)
					{
						// just overwrite stuff that would be moved out of bounds
						if(z-zo < 0)
							continue;
                    	m_channel.SetVoxel(x, y, z-zo, m_channel.GetVoxel(x,y,z));
					}
				}
			}
            CheckForTerrainUpdates();
        }

        private void InterfaceFillTerrain(Object[] args)
        {
            int x, y, z;

            for (x = 0; x < m_channel.Width; x++)
			{
                for (y = 0; y < m_channel.Length; y++)
				{
                	for (z = 0; z < m_channel.Height; z++)
					{
						byte v = 0x00;
						if(z<=(double)args[0])
						{
                    		v=0x01;
						}
						m_channel.SetVoxel(x,y,z,v);
					}
				}
			}
            CheckForTerrainUpdates();
        }

        private void InterfaceShowDebugStats(Object[] args)
        {
            double max = Double.MinValue;
            double min = double.MaxValue;
            double sum = 0;

            int x;
            for (x = 0; x < m_channel.Width; x++)
            {
                int y;
                for (y = 0; y < m_channel.Length; y++)
                {
					int z;
					double ch = m_channel.GetHeightAt(x,y);
					
                    sum += ch;
                    if (max < ch)
                        max = ch;
                    if (min > ch)
                        min = ch;
                }
            }

            double avg = sum / (m_channel.Length * m_channel.Width);

            m_log.Info("Channel " + m_channel.Width + "x" + m_channel.Length);
            m_log.Info("max/min/avg/sum: " + max + "/" + min + "/" + avg + "/" + sum);
        }

        private void InterfaceEnableExperimentalBrushes(Object[] args)
        {
			m_log.Error("They're all experimental on VoxelSim :V");
			/*
            if ((bool) args[0])
            {
                m_painteffects[StandardTerrainEffects.Revert] = new WeatherSphere();
                m_painteffects[StandardTerrainEffects.Flatten] = new OlsenSphere();
                m_painteffects[StandardTerrainEffects.Smooth] = new ErodeSphere();
            }
            else
            {
                InstallDefaultEffects();
            }*/
        }
		
		private void InterfaceGenerate(Object[] args)
		{
			
			m_scene.Voxels=(m_scene.Voxels as VoxelChannel).Generate("default");
			TaintTerrain();
		}

        private void InterfaceRunPluginEffect(Object[] args)
        {
			/*
            if ((string) args[0] == "list")
            {
                m_log.Info("List of loaded plugins");
                foreach (KeyValuePair<string, IVoxelEffect> kvp in m_plugineffects)
                {
                    m_log.Info(kvp.Key);
                }
                return;
            }
            if ((string) args[0] == "reload")
            {
                LoadPlugins();
                return;
            }
            if (m_plugineffects.ContainsKey((string) args[0]))
            {
                m_plugineffects[(string) args[0]].RunEffect(m_channel);
                CheckForTerrainUpdates();
            }
            else
            {
                m_log.Warn("No such plugin effect loaded.");
            }*/
        }

        private void InstallInterfaces()
        {
            // Load / Save
            string supportedFileExtensions = "";
            foreach (KeyValuePair<string, IVoxelFileHandler> loader in m_loaders)
                supportedFileExtensions += " " + loader.Key + " (" + loader.Value + ")";

            Command loadFromFileCommand =
                new Command("load", CommandIntentions.COMMAND_HAZARDOUS, InterfaceLoadFile, "Loads a terrain from a specified file.");
            loadFromFileCommand.AddArgument("filename",
                                            "The file you wish to load from, the file extension determines the loader to be used. Supported extensions include: " +
                                            supportedFileExtensions, "String");

            Command saveToFileCommand =
                new Command("save", CommandIntentions.COMMAND_NON_HAZARDOUS, InterfaceSaveFile, "Saves the current heightmap to a specified file.");
            saveToFileCommand.AddArgument("filename",
                                          "The destination filename for your heightmap, the file extension determines the format to save in. Supported extensions include: " +
                                          supportedFileExtensions, "String");

            Command loadFromTileCommand =
                new Command("load-tile", CommandIntentions.COMMAND_HAZARDOUS, InterfaceLoadTileFile, "Loads a terrain from a section of a larger file.");
            loadFromTileCommand.AddArgument("filename",
                                            "The file you wish to load from, the file extension determines the loader to be used. Supported extensions include: " +
                                            supportedFileExtensions, "String");
            loadFromTileCommand.AddArgument("file width", "The width of the file in tiles", "Integer");
            loadFromTileCommand.AddArgument("file height", "The height of the file in tiles", "Integer");
            loadFromTileCommand.AddArgument("minimum X tile", "The X region coordinate of the first section on the file",
                                            "Integer");
            loadFromTileCommand.AddArgument("minimum Y tile", "The Y region coordinate of the first section on the file",
                                            "Integer");

            // Terrain adjustments
            Command fillRegionCommand =
                new Command("fill", CommandIntentions.COMMAND_HAZARDOUS, InterfaceFillTerrain, "Fills the current heightmap with a specified value.");
            fillRegionCommand.AddArgument("value", "The numeric value of the height you wish to set your region to.",
                                          "Double");

            Command elevateCommand =
                new Command("elevate", CommandIntentions.COMMAND_HAZARDOUS, InterfaceElevateTerrain, "Raises the current heightmap by the specified amount.");
            elevateCommand.AddArgument("amount", "The amount of height to add to the terrain in meters.", "Double");

            Command lowerCommand =
                new Command("lower", CommandIntentions.COMMAND_HAZARDOUS, InterfaceLowerTerrain, "Lowers the current heightmap by the specified amount.");
            lowerCommand.AddArgument("amount", "The amount of height to remove from the terrain in meters.", "Double");

            Command multiplyCommand =
                new Command("multiply", CommandIntentions.COMMAND_HAZARDOUS, InterfaceMultiplyTerrain, "Multiplies the heightmap by the value specified.");
            multiplyCommand.AddArgument("value", "The value to multiply the heightmap by.", "Double");

            Command bakeRegionCommand =
                new Command("bake", CommandIntentions.COMMAND_HAZARDOUS, InterfaceBakeTerrain, "Saves the current terrain into the regions revert map.");
            Command revertRegionCommand =
                new Command("revert", CommandIntentions.COMMAND_HAZARDOUS, InterfaceRevertTerrain, "Loads the revert map terrain into the regions heightmap.");

            Command flipCommand =
                new Command("flip", CommandIntentions.COMMAND_HAZARDOUS, InterfaceFlipTerrain, "Flips the current terrain about the X or Y axis");
            flipCommand.AddArgument("direction", "[x|y] the direction to flip the terrain in", "String");

			Command generateCommand=
				new Command("generate",CommandIntentions.COMMAND_HAZARDOUS, InterfaceGenerate,"Generate terrain.");
			generateCommand.AddArgument("Frequency","Perlin frequency","Double");
			generateCommand.AddArgument("Lucunarity","Hell if I know","Double");
			generateCommand.AddArgument("Persistance","Hell if I know","Double");
			generateCommand.AddArgument("Octaves","Hell if I know","Integer");
			
            Command rescaleCommand =
                new Command("rescale", CommandIntentions.COMMAND_HAZARDOUS, InterfaceRescaleTerrain, "Rescales the current terrain to fit between the given min and max heights");
            rescaleCommand.AddArgument("min", "min terrain height after rescaling", "Double");
            rescaleCommand.AddArgument("max", "max terrain height after rescaling", "Double");


            // Debug
            Command showDebugStatsCommand =
                new Command("stats", CommandIntentions.COMMAND_STATISTICAL, InterfaceShowDebugStats,
                            "Shows some information about the regions heightmap for debugging purposes.");

            Command experimentalBrushesCommand =
                new Command("newbrushes", CommandIntentions.COMMAND_HAZARDOUS, InterfaceEnableExperimentalBrushes,
                            "Enables experimental brushes which replace the standard terrain brushes. WARNING: This is a debug setting and may be removed at any time.");
            experimentalBrushesCommand.AddArgument("Enabled?", "true / false - Enable new brushes", "Boolean");

            //Plugins
            Command pluginRunCommand =
                new Command("effect", CommandIntentions.COMMAND_HAZARDOUS, InterfaceRunPluginEffect, "Runs a specified plugin effect");
            pluginRunCommand.AddArgument("name", "The plugin effect you wish to run, or 'list' to see all plugins", "String");

            m_commander.RegisterCommand("load", loadFromFileCommand);
            m_commander.RegisterCommand("load-tile", loadFromTileCommand);
            m_commander.RegisterCommand("save", saveToFileCommand);
            m_commander.RegisterCommand("fill", fillRegionCommand);
            m_commander.RegisterCommand("elevate", elevateCommand);
            m_commander.RegisterCommand("lower", lowerCommand);
            m_commander.RegisterCommand("multiply", multiplyCommand);
            m_commander.RegisterCommand("bake", bakeRegionCommand);
            m_commander.RegisterCommand("revert", revertRegionCommand);
            m_commander.RegisterCommand("newbrushes", experimentalBrushesCommand);
            m_commander.RegisterCommand("stats", showDebugStatsCommand);
            m_commander.RegisterCommand("effect", pluginRunCommand);
            m_commander.RegisterCommand("flip", flipCommand);
            m_commander.RegisterCommand("rescale", rescaleCommand);
            m_commander.RegisterCommand("generate", generateCommand);

            // Add this to our scene so scripts can call these functions
            m_scene.RegisterModuleCommander(m_commander);
        }


        #endregion
    }

    internal struct VoxelUpdate
    {
        public int X;
        public int Y;
        public int Z;
        public byte Type;
    }

    internal struct ChunkUpdate
    {
        public int X;
        public int Y;
        public int Z;
    }
}
