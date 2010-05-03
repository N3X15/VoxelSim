
using System;
using OpenMetaverse;
using OpenMetaverse.Packets;

/*
						case 911: return PacketType.VoxelAdd;
						case 912: return PacketType.VoxelLayer;
						case 913: return PacketType.VoxelRemove;
						case 914: return PacketType.VoxelUpdate;
*/
namespace OpenMetaverse.Packets
{
    /// <exclude/>
    public sealed class VoxelLayerPacket : Packet
    {
        public sealed class LayerDataBlock : PacketBlock
        {
			// Layer is a sparse matrix of:
			// {x,y,MATERIAL}
            public uint Z;
			public Voxel[] Voxels;
			
            public override int Length
            {
                get
                {
                    return 4+(Voxels.Length*16);
                }
            }

            public LayerDataBlock() { }
            public LayerDataBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
                    Z = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
                    uint NumVect = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
					Voxels=new Voxel[NumVect];
                    for(uint j=0;j<NumVect;j++)
					{
						Voxels[j]=new Voxel(bytes,i);
						i+=16;
					}
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
                Utils.UIntToBytes(Z, bytes, i); i += 4;
                Utils.IntToBytes(Voxels.Length, bytes, i); i += 4;
				foreach(Voxel v in Voxels)
				{
					v.FromBytes(bytes,i);i+=16;
				}
            }

        }

        public override int Length
        {
            get
            {
                int length = 8;
                length += LayerData.Length;
                return length;
            }
        }
        public LayerDataBlock[] LayerData;

        public VoxelLayerPacket()
        {
            HasVariableBlocks = true;
            Type = PacketType.VoxelLayer;
            Header = new Header();
            Header.Frequency = PacketFrequency.Low;
            Header.ID = 912;
            Header.Reliable = true;
            LayerData = null;
        }

        public VoxelLayerPacket(byte[] bytes, ref int i) : this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(bytes, ref i, ref packetEnd, null);
        }

        override public void FromBytes(byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer)
        {
            Header.FromBytes(bytes, ref i, ref packetEnd);
            if (Header.Zerocoded && zeroBuffer != null)
            {
                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;
                bytes = zeroBuffer;
            }
            int count = (int)bytes[i++];
            if(LayerData == null || LayerData.Length != -1) {
                LayerData = new LayerDataBlock[count];
                for(int j = 0; j < count; j++)
                { LayerData[j] = new LayerDataBlock(); }
            }
            for (int j = 0; j < count; j++)
            { LayerData[j].FromBytes(bytes, ref i); }
        }

        public VoxelLayerPacket(Header head, byte[] bytes, ref int i): this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(head, bytes, ref i, ref packetEnd);
        }

        override public void FromBytes(Header header, byte[] bytes, ref int i, ref int packetEnd)
        {
            Header = header;
            int count = (int)bytes[i++];
            if(LayerData == null || LayerData.Length != count) {
                LayerData = new LayerDataBlock[count];
                for(int j = 0; j < count; j++)
                { LayerData[j] = new LayerDataBlock(); }
            }
            for (int j = 0; j < count; j++)
            { LayerData[j].FromBytes(bytes, ref i); }
        }

        public override byte[] ToBytes()
        {
            int length = 10;
            for (int j = 0; j < LayerData.Length; j++) { length += LayerData[j].Length; }
            if (Header.AckList != null && Header.AckList.Length > 0) { length += Header.AckList.Length * 4 + 1; }
            byte[] bytes = new byte[length];
            int i = 0;
            Header.ToBytes(bytes, ref i);
            bytes[i++] = (byte)LayerData.Length;
            for (int j = 0; j < LayerData.Length; j++) { LayerData[j].ToBytes(bytes, ref i); }
            if (Header.AckList != null && Header.AckList.Length > 0) { Header.AcksToBytes(bytes, ref i); }
            return bytes;
        }

        public override byte[][] ToBytesMultiple()
        {
            System.Collections.Generic.List<byte[]> packets = new System.Collections.Generic.List<byte[]>();
            int i = 0;
            int fixedLength = 10;

            byte[] ackBytes = null;
            int acksLength = 0;
            if (Header.AckList != null && Header.AckList.Length > 0) {
                Header.AppendedAcks = true;
                ackBytes = new byte[Header.AckList.Length * 4 + 1];
                Header.AcksToBytes(ackBytes, ref acksLength);
            }
            byte[] fixedBytes = new byte[fixedLength];
            Header.ToBytes(fixedBytes, ref i);
            fixedLength += 1;

            int LayerDataStart = 0;
            do
            {
                int variableLength = 0;
                int LayerDataCount = 0;

                i = LayerDataStart;
                while (fixedLength + variableLength + acksLength < Packet.MTU && i < LayerData.Length) {
                    int blockLength = LayerData[i].Length;
                    if (fixedLength + variableLength + blockLength + acksLength <= MTU) {
                        variableLength += blockLength;
                        ++LayerDataCount;
                    }
                    else { break; }
                    ++i;
                }

                byte[] packet = new byte[fixedLength + variableLength + acksLength];
                int length = fixedBytes.Length;
                Buffer.BlockCopy(fixedBytes, 0, packet, 0, length);
                if (packets.Count > 0) { packet[0] = (byte)(packet[0] & ~0x10); }

                packet[length++] = (byte)LayerDataCount;
                for (i = LayerDataStart; i < LayerDataStart + LayerDataCount; i++) { LayerData[i].ToBytes(packet, ref length); }
                LayerDataStart += LayerDataCount;

                if (acksLength > 0) {
                    Buffer.BlockCopy(ackBytes, 0, packet, length, acksLength);
                    acksLength = 0;
                }

                packets.Add(packet);
            } while (
                LayerDataStart < LayerData.Length);

            return packets.ToArray();
        }
    }
    /// <exclude/>
    public sealed class VoxelAddPacket : Packet
    {
        /// <exclude/>
        public sealed class AgentDataBlock : PacketBlock
        {
            public UUID AgentID;
            public uint Flags;

            public override int Length
            {
                get
                {
                    return 20;
                }
            }

            public AgentDataBlock() { }
            public AgentDataBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
                    AgentID.FromBytes(bytes, i); i += 16;
                    Flags = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
                AgentID.ToBytes(bytes, i); i += 16;
                Utils.UIntToBytes(Flags, bytes, i); i += 4;
            }

        }

        /// <exclude/>
        public sealed class AddVoxelBlock : PacketBlock
        {
			public Voxel NewVoxel;
			
            public override int Length
            {
                get
                {
                    return 16;
                }
            }

            public AddVoxelBlock() { }
            public AddVoxelBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
                    NewVoxel=new Voxel();
					NewVoxel.FromBytes(bytes,i);
					i+=16;
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
                NewVoxel.ToBytes(bytes,i);
				i+=16;
            }

        }

        public override int Length
        {
            get
            {
                int length = 4;
                length += AgentData.Length;
                length += 16;
                return length;
            }
        }
        public AgentDataBlock AgentData;
        public AddVoxelBlock VoxelData;

        public VoxelAddPacket()
        {
            HasVariableBlocks = false;
            Type = PacketType.MapLayerReply;
            Header = new Header();
            Header.Frequency = PacketFrequency.Low;
            Header.ID = 911;
            Header.Reliable = true;
            AgentData = new AgentDataBlock();
            VoxelData = new AddVoxelBlock();
        }

        public VoxelAddPacket(byte[] bytes, ref int i) : this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(bytes, ref i, ref packetEnd, null);
        }

        override public void FromBytes(byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer)
        {
            Header.FromBytes(bytes, ref i, ref packetEnd);
            if (Header.Zerocoded && zeroBuffer != null)
            {
                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;
                bytes = zeroBuffer;
            }
            AgentData.FromBytes(bytes, ref i);
			VoxelData.FromBytes(bytes, ref i);
        }

        public VoxelAddPacket(Header head, byte[] bytes, ref int i): this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(head, bytes, ref i, ref packetEnd);
        }

        override public void FromBytes(Header header, byte[] bytes, ref int i, ref int packetEnd)
        {
            Header = header;
            AgentData.FromBytes(bytes, ref i);
           	VoxelData.FromBytes(bytes, ref i);
        }

        public override byte[] ToBytes()
        {
            int length = 10;
            length += AgentData.Length;
            length += VoxelData.Length;
     
            if (Header.AckList != null && Header.AckList.Length > 0) { length += Header.AckList.Length * 4 + 1; }
            
			byte[] bytes = new byte[length];
            int i = 0;
            Header.ToBytes(bytes, ref i);
            AgentData.ToBytes(bytes, ref i);
            VoxelData.ToBytes(bytes, ref i);
            if (Header.AckList != null && Header.AckList.Length > 0) { Header.AcksToBytes(bytes, ref i); }
            return bytes;
        }

        public override byte[][] ToBytesMultiple()
        {
            return new byte[][] {ToBytes()};
        }
    }
    /// <exclude/>
    public sealed class VoxelRemovePacket : Packet
    {
        /// <exclude/>
        public sealed class AgentDataBlock : PacketBlock
        {
            public UUID AgentID;
            public uint Flags;

            public override int Length
            {
                get
                {
                    return 20;
                }
            }

            public AgentDataBlock() { }
            public AgentDataBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
                    AgentID.FromBytes(bytes, i); i += 16;
                    Flags = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
                AgentID.ToBytes(bytes, i); i += 16;
                Utils.UIntToBytes(Flags, bytes, i); i += 4;
            }

        }

        /// <exclude/>
        public sealed class RemoveVoxelBlock : PacketBlock
        {
			public uint X;
			public uint Y;
			public uint Z;
			
            public override int Length
            {
                get
                {
                    return 12;
                }
            }

            public RemoveVoxelBlock() { }
            public RemoveVoxelBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
                    X = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
					Y = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
					Z = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
                Utils.UIntToBytes(X, bytes, i); i += 4;
                Utils.UIntToBytes(Y, bytes, i); i += 4;
                Utils.UIntToBytes(Z, bytes, i); i += 4;
            }

        }

        public override int Length
        {
            get
            {
                int length = 4;
                length += AgentData.Length;
                length += 12;
                return length;
            }
        }
        public AgentDataBlock AgentData;
        public RemoveVoxelBlock RemoveData;

        public VoxelRemovePacket()
        {
            HasVariableBlocks = false;
            Type = PacketType.MapLayerReply;
            Header = new Header();
            Header.Frequency = PacketFrequency.Low;
            Header.ID = 913;
            Header.Reliable = true;
            AgentData = new AgentDataBlock();
            RemoveData = new RemoveVoxelBlock();
        }

        public VoxelRemovePacket(byte[] bytes, ref int i) : this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(bytes, ref i, ref packetEnd, null);
        }

        override public void FromBytes(byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer)
        {
            Header.FromBytes(bytes, ref i, ref packetEnd);
            if (Header.Zerocoded && zeroBuffer != null)
            {
                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;
                bytes = zeroBuffer;
            }
            AgentData.FromBytes(bytes, ref i);
            RemoveData.FromBytes(bytes, ref i);
        }

        public VoxelRemovePacket(Header head, byte[] bytes, ref int i): this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(head, bytes, ref i, ref packetEnd);
        }

        override public void FromBytes(Header header, byte[] bytes, ref int i, ref int packetEnd)
        {
            Header = header;
            AgentData.FromBytes(bytes, ref i);
            RemoveData.FromBytes(bytes, ref i);
        }

        public override byte[] ToBytes()
        {
            int length = 10;
            length += AgentData.Length;
            length += RemoveData.Length;
            if (Header.AckList != null && Header.AckList.Length > 0) { length += Header.AckList.Length * 4 + 1; }
            byte[] bytes = new byte[length];
            int i = 0;
            Header.ToBytes(bytes, ref i);
            AgentData.ToBytes(bytes, ref i);
            RemoveData.ToBytes(bytes, ref i);
            if (Header.AckList != null && Header.AckList.Length > 0) { Header.AcksToBytes(bytes, ref i); }
            return bytes;
        }

        public override byte[][] ToBytesMultiple()
        {
            return new byte[][]{ToBytes()};
        }
    }
	/// <exclude/>
    public sealed class VoxelUpdatePacket : Packet
    {
        /// <exclude/>
        public sealed class AgentDataBlock : PacketBlock
        {
            public UUID AgentID;
            public uint Flags;

            public override int Length
            {
                get
                {
                    return 20;
                }
            }

            public AgentDataBlock() { }
            public AgentDataBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
                    AgentID.FromBytes(bytes, i); i += 16;
                    Flags = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
                AgentID.ToBytes(bytes, i); i += 16;
                Utils.UIntToBytes(Flags, bytes, i); i += 4;
            }

        }

        /// <exclude/>
        public sealed class RemoveVoxelBlock : PacketBlock
        {
			public Voxel V;
			
            public override int Length
            {
                get
                {
                    return 16;
                }
            }

            public RemoveVoxelBlock() { }
            public RemoveVoxelBlock(byte[] bytes, ref int i)
            {
                FromBytes(bytes, ref i);
            }

            public override void FromBytes(byte[] bytes, ref int i)
            {
                try
                {
					V=new Voxel(bytes,i);i+=16;
                }
                catch (Exception)
                {
                    throw new MalformedDataException();
                }
            }

            public override void ToBytes(byte[] bytes, ref int i)
            {
            	this.V.ToBytes(bytes,i);
				i+=16;
            }

        }

        public override int Length
        {
            get
            {
                int length = 4;
                length += AgentData.Length;
                length += 12;
                return length;
            }
        }
        public AgentDataBlock AgentData;
        public RemoveVoxelBlock RemoveData;

        public VoxelUpdatePacket()
        {
            HasVariableBlocks = false;
            Type = PacketType.VoxelUpdate;
            Header = new Header();
            Header.Frequency = PacketFrequency.Low;
            Header.ID = 914;
            Header.Reliable = true;
            AgentData = new AgentDataBlock();
            RemoveData = new RemoveVoxelBlock();
        }

        public VoxelUpdatePacket(byte[] bytes, ref int i) : this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(bytes, ref i, ref packetEnd, null);
        }

        override public void FromBytes(byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer)
        {
            Header.FromBytes(bytes, ref i, ref packetEnd);
            if (Header.Zerocoded && zeroBuffer != null)
            {
                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;
                bytes = zeroBuffer;
            }
            AgentData.FromBytes(bytes, ref i);
            RemoveData.FromBytes(bytes, ref i);
        }

        public VoxelUpdatePacket(Header head, byte[] bytes, ref int i): this()
        {
            int packetEnd = bytes.Length - 1;
            FromBytes(head, bytes, ref i, ref packetEnd);
        }

        override public void FromBytes(Header header, byte[] bytes, ref int i, ref int packetEnd)
        {
            Header = header;
            AgentData.FromBytes(bytes, ref i);
            RemoveData.FromBytes(bytes, ref i);
        }

        public override byte[] ToBytes()
        {
            int length = 10;
            length += AgentData.Length;
            length += RemoveData.Length;
            if (Header.AckList != null && Header.AckList.Length > 0) { length += Header.AckList.Length * 4 + 1; }
            byte[] bytes = new byte[length];
            int i = 0;
            Header.ToBytes(bytes, ref i);
            AgentData.ToBytes(bytes, ref i);
            RemoveData.ToBytes(bytes, ref i);
            if (Header.AckList != null && Header.AckList.Length > 0) { Header.AcksToBytes(bytes, ref i); }
            return bytes;
        }

        public override byte[][] ToBytesMultiple()
        {
            return new byte[][]{ToBytes()};
        }
    }
}
