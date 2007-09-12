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
*     * Neither the name of the OpenSim Project nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/
using libsecondlife;
using Nwc.XmlRpc;

using System;
using System.Collections;

namespace OpenSim.Framework.Data
{
    /// <summary>
    /// A class which contains information known to the grid server about a region
    /// </summary>
    public class SimProfileData
    {
        /// <summary>
        /// The name of the region
        /// </summary>
        public string regionName = "";

        /// <summary>
        /// A 64-bit number combining map position into a (mostly) unique ID
        /// </summary>
        public ulong regionHandle;

        /// <summary>
        /// OGS/OpenSim Specific ID for a region
        /// </summary>
        public LLUUID UUID;

        /// <summary>
        /// Coordinates of the region
        /// </summary>
        public uint regionLocX;
        public uint regionLocY;
        public uint regionLocZ; // Reserved (round-robin, layers, etc)

        /// <summary>
        /// Authentication secrets
        /// </summary>
        /// <remarks>Not very secure, needs improvement.</remarks>
        public string regionSendKey = "";
        public string regionRecvKey = "";
        public string regionSecret = "";

        /// <summary>
        /// Whether the region is online
        /// </summary>
        public bool regionOnline;

        /// <summary>
        /// Information about the server that the region is currently hosted on
        /// </summary>
        public string serverIP = "";
        public uint serverPort;
        public string serverURI = "";

        public uint httpPort;
        public uint remotingPort;
        public string httpServerURI = "";

        /// <summary>
        /// Set of optional overrides. Can be used to create non-eulicidean spaces.
        /// </summary>
        public ulong regionNorthOverrideHandle;
        public ulong regionSouthOverrideHandle;
        public ulong regionEastOverrideHandle;
        public ulong regionWestOverrideHandle;

        /// <summary>
        /// Optional: URI Location of the region database
        /// </summary>
        /// <remarks>Used for floating sim pools where the region data is not nessecarily coupled to a specific server</remarks>
        public string regionDataURI = "";

        /// <summary>
        /// Region Asset Details
        /// </summary>
        public string regionAssetURI = "";
        public string regionAssetSendKey = "";
        public string regionAssetRecvKey = "";

        /// <summary>
        /// Region Userserver Details
        /// </summary>
        public string regionUserURI = "";
        public string regionUserSendKey = "";
        public string regionUserRecvKey = "";

        /// <summary>
        /// Region Map Texture Asset
        /// </summary>
        public LLUUID regionMapTextureID = new LLUUID("00000000-0000-0000-9999-000000000006");

        /// <summary>
        /// Get Sim profile data from grid server when in grid mode
        /// </summary>
        /// <param name="region_uuid"></param>
        /// <param name="gridserver_url"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public SimProfileData RequestSimProfileData(LLUUID region_uuid, string gridserver_url, string gridserver_sendkey, string gridserver_recvkey)
        {
            Hashtable requestData = new Hashtable();
            requestData["region_uuid"] = region_uuid.UUID.ToString();
            requestData["authkey"] = gridserver_sendkey;
            ArrayList SendParams = new ArrayList();
            SendParams.Add(requestData);
            XmlRpcRequest GridReq = new XmlRpcRequest("simulator_data_request", SendParams);
            XmlRpcResponse GridResp = GridReq.Send(gridserver_url, 3000);

            Hashtable responseData = (Hashtable)GridResp.Value;

            if (responseData.ContainsKey("error"))
            {
                return null;
            }

            SimProfileData simData = new SimProfileData();
            simData.regionLocX = Convert.ToUInt32((string)responseData["region_locx"]);
            simData.regionLocY = Convert.ToUInt32((string)responseData["region_locy"]);
            simData.regionHandle = Helpers.UIntsToLong((simData.regionLocX * 256), (simData.regionLocY * 256));
            simData.serverIP = (string)responseData["sim_ip"];
            simData.serverPort = Convert.ToUInt32((string)responseData["sim_port"]);
            simData.httpPort = Convert.ToUInt32((string)responseData["http_port"]);
            simData.remotingPort = Convert.ToUInt32((string)responseData["remoting_port"]);
            simData.serverURI = "http://" + simData.serverIP + ":" + simData.serverPort.ToString() + "/";
            simData.httpServerURI = "http://" + simData.serverIP + ":" + simData.httpPort.ToString() + "/";
            simData.UUID = new LLUUID((string)responseData["region_UUID"]);
            simData.regionName = (string)responseData["region_name"];

            return simData;            
        }
        public SimProfileData RequestSimProfileData(ulong region_handle, string gridserver_url, string gridserver_sendkey, string gridserver_recvkey)
        {
            Hashtable requestData = new Hashtable();
            requestData["region_handle"] = region_handle.ToString();
            requestData["authkey"] = gridserver_sendkey;
            ArrayList SendParams = new ArrayList();
            SendParams.Add(requestData);
            XmlRpcRequest GridReq = new XmlRpcRequest("simulator_data_request", SendParams);
            Console.WriteLine("Requesting response from GridServer URL: " + gridserver_url);
            XmlRpcResponse GridResp = GridReq.Send(gridserver_url, 3000);

            Hashtable responseData = (Hashtable)GridResp.Value;

            if (responseData.ContainsKey("error"))
            {
                return null;
            }

            SimProfileData simData = new SimProfileData();
            simData.regionLocX = Convert.ToUInt32((string)responseData["region_locx"]);
            simData.regionLocY = Convert.ToUInt32((string)responseData["region_locy"]);
            simData.regionHandle = Helpers.UIntsToLong((simData.regionLocX * 256), (simData.regionLocY * 256));
            simData.serverIP = (string)responseData["sim_ip"];
            simData.serverPort = Convert.ToUInt32((string)responseData["sim_port"]);
            simData.httpPort = Convert.ToUInt32((string)responseData["http_port"]);
            simData.remotingPort = Convert.ToUInt32((string)responseData["remoting_port"]);
            simData.httpServerURI = "http://" + simData.serverIP + ":" + simData.httpPort.ToString() + "/";
            simData.serverURI = "http://" + simData.serverIP + ":" + simData.serverPort.ToString() + "/";
            simData.UUID = new LLUUID((string)responseData["region_UUID"]);
            simData.regionName = (string)responseData["region_name"];

            return simData; 
        }
    }
}
