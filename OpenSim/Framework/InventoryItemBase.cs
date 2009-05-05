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
using OpenMetaverse;

namespace OpenSim.Framework
{
    /// <summary>
    /// Inventory Item - contains all the properties associated with an individual inventory piece.
    /// </summary>
    public class InventoryItemBase : InventoryNodeBase, ICloneable
    {       
        /// <value>
        /// The inventory type of the item.  This is slightly different from the asset type in some situations.
        /// </value>        
        public int InvType { get; set; }

        /// <value>
        /// The folder this item is contained in
        /// </value>        
        public UUID Folder { get; set; }

        /// <value>
        /// The creator of this item
        /// </value>        
        public string CreatorId { get; set; }      

        /// <value>
        /// The creator of this item expressed as a UUID.  Database plugins don't need to set this, it will be set by
        /// upstream code (or set by the get accessor if left unset).
        /// </value>
        public UUID CreatorIdAsUuid 
        {
            get
            {
                if (UUID.Zero == m_creatorIdAsUuid)
                {
                    UUID.TryParse(CreatorId, out m_creatorIdAsUuid);
                }

                return m_creatorIdAsUuid;
            }
            
            set
            {
                m_creatorIdAsUuid = value;
            }
        }     
        protected UUID m_creatorIdAsUuid = UUID.Zero;

        /// <value>
        /// The description of the inventory item (must be less than 64 characters)
        /// </value>
        public string Description { get; set; }

        /// <value>
        ///
        /// </value>          
        public uint NextPermissions { get; set; }

        /// <value>
        /// A mask containing permissions for the current owner (cannot be enforced)
        /// </value>        
        public uint CurrentPermissions { get; set; }

        /// <value>
        ///
        /// </value>        
        public uint BasePermissions { get; set; }

        /// <value>
        ///
        /// </value>        
        public uint EveryOnePermissions { get; set; }

        /// <value>
        ///
        /// </value>        
        public uint GroupPermissions { get; set; }

        /// <value>
        /// This is an enumerated value determining the type of asset (eg Notecard, Sound, Object, etc)
        /// </value>        
        public int AssetType { get; set; }

        /// <value>
        /// The UUID of the associated asset on the asset server
        /// </value>        
        public UUID AssetID { get; set; }

        /// <value>
        ///
        /// </value>        
        public UUID GroupID { get; set; }

        /// <value>
        ///
        /// </value>        
        public bool GroupOwned { get; set; }

        /// <value>
        ///
        /// </value>        
        public int SalePrice { get; set; }

        /// <value>
        ///
        /// </value>        
        public byte SaleType { get; set; }

        /// <value>
        ///
        /// </value>        
        public uint Flags { get; set; }

        /// <value>
        ///
        /// </value>        
        public int CreationDate { get; set; }

        public InventoryItemBase()
        {
            Description = String.Empty;
            CreationDate = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
