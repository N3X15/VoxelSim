﻿/*
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Net;
using System.Reflection;
using OpenSim.Services.Interfaces;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

using OpenSim.Framework;

using OpenMetaverse;
using OpenMetaverse.Imaging;
using log4net;
using Nwc.XmlRpc;

namespace OpenSim.Services.Connectors.Hypergrid
{
    public class HypergridServiceConnector
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IAssetService m_AssetService;

        public HypergridServiceConnector(IAssetService assService)
        {
            m_AssetService = assService;
        }

        public bool LinkRegion(GridRegion info, out UUID regionID, out ulong realHandle, out string imageURL, out string reason)
        {
            regionID = UUID.Zero;
            imageURL = string.Empty;
            realHandle = 0;
            reason = string.Empty;

            Hashtable hash = new Hashtable();
            hash["region_name"] = info.RegionName;

            IList paramList = new ArrayList();
            paramList.Add(hash);

            XmlRpcRequest request = new XmlRpcRequest("link_region", paramList);
            string uri = "http://" + info.ExternalEndPoint.Address + ":" + info.HttpPort + "/";
            m_log.Debug("[HGrid]: Linking to " + uri);
            XmlRpcResponse response = null;
            try
            {
                response = request.Send(uri, 10000);
            }
            catch (Exception e)
            {
                m_log.Debug("[HGrid]: Exception " + e.Message);
                reason = "Error contacting remote server";
                return false;
            }

            if (response.IsFault)
            {
                reason = response.FaultString;
                m_log.ErrorFormat("[HGrid]: remote call returned an error: {0}", response.FaultString);
                return false;
            }

            hash = (Hashtable)response.Value;
            //foreach (Object o in hash)
            //    m_log.Debug(">> " + ((DictionaryEntry)o).Key + ":" + ((DictionaryEntry)o).Value);
            try
            {
                bool success = false;
                Boolean.TryParse((string)hash["result"], out success);
                if (success)
                {
                    UUID.TryParse((string)hash["uuid"], out regionID);
                    //m_log.Debug(">> HERE, uuid: " + uuid);
                    if ((string)hash["handle"] != null)
                    {
                        realHandle = Convert.ToUInt64((string)hash["handle"]);
                        //m_log.Debug(">> HERE, realHandle: " + realHandle);
                    }
                    if (hash["region_image"] != null)
                    {
                        imageURL = (string)hash["region_image"];
                    }
                }

            }
            catch (Exception e)
            {
                reason = "Error parsing return arguments";
                m_log.Error("[HGrid]: Got exception while parsing hyperlink response " + e.StackTrace);
                return false;
            }

            return true;
        }

        public UUID GetMapImage(UUID regionID, string imageURL)
        {
            try
            {

                WebClient c = new WebClient();
                //m_log.Debug("JPEG: " + uri);
                string filename = regionID.ToString();
                c.DownloadFile(imageURL, filename + ".jpg");
                Bitmap m = new Bitmap(filename + ".jpg");
                //m_log.Debug("Size: " + m.PhysicalDimension.Height + "-" + m.PhysicalDimension.Width);
                byte[] imageData = OpenJPEG.EncodeFromImage(m, true);
                AssetBase ass = new AssetBase(UUID.Random(), "region " + filename, (sbyte)AssetType.Texture);
                
                // !!! for now
                //info.RegionSettings.TerrainImageID = ass.FullID;

                ass.Temporary = true;
                ass.Local = true;
                ass.Data = imageData;

                m_AssetService.Store(ass);

                // finally
                return ass.FullID;

            }
            catch // LEGIT: Catching problems caused by OpenJPEG p/invoke
            {
                m_log.Warn("[HGrid]: Failed getting/storing map image, because it is probably already in the cache");
            }
            return UUID.Zero;
        }

        public GridRegion GetHyperlinkRegion(GridRegion gatekeeper, UUID regionID)
        {
            Hashtable hash = new Hashtable();
            hash["region_uuid"] = regionID.ToString();

            IList paramList = new ArrayList();
            paramList.Add(hash);

            XmlRpcRequest request = new XmlRpcRequest("get_region", paramList);
            string uri = "http://" + gatekeeper.ExternalEndPoint.Address + ":" + gatekeeper.HttpPort + "/";
            m_log.Debug("[HGrid]: contacting " + uri);
            XmlRpcResponse response = null;
            try
            {
                response = request.Send(uri, 10000);
            }
            catch (Exception e)
            {
                m_log.Debug("[HGrid]: Exception " + e.Message);
                return null;
            }

            if (response.IsFault)
            {
                m_log.ErrorFormat("[HGrid]: remote call returned an error: {0}", response.FaultString);
                return null;
            }

            hash = (Hashtable)response.Value;
            //foreach (Object o in hash)
            //    m_log.Debug(">> " + ((DictionaryEntry)o).Key + ":" + ((DictionaryEntry)o).Value);
            try
            {
                bool success = false;
                Boolean.TryParse((string)hash["result"], out success);
                if (success)
                {
                    GridRegion region = new GridRegion();

                    UUID.TryParse((string)hash["uuid"], out region.RegionID);
                    //m_log.Debug(">> HERE, uuid: " + uuid);
                    int n = 0;
                    if (hash["x"] != null)
                    {
                        Int32.TryParse((string)hash["x"], out n);
                        region.RegionLocX = n;
                    }
                    if (hash["y"] != null)
                    {
                        Int32.TryParse((string)hash["y"], out n);
                        region.RegionLocY = n;
                    }
                    if (hash["region_name"] != null)
                    {
                        region.RegionName = (string)hash["region_name"];
                    }
                    if (hash["hostname"] != null)
                        region.ExternalHostName = (string)hash["hostname"];
                    if (hash["http_port"] != null)
                    {
                        uint p = 0;
                        UInt32.TryParse((string)hash["http_port"], out p);
                        region.HttpPort = p;
                    }
                    if (hash["internal_port"] != null)
                    {
                        int p = 0;
                        Int32.TryParse((string)hash["internal_port"], out p);
                        region.InternalEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), p);
                    }

                    // Successful return
                    return region;
                }

            }
            catch (Exception e)
            {
                m_log.Error("[HGrid]: Got exception while parsing hyperlink response " + e.StackTrace);
                return null;
            }

            return null;
        }
    }
}