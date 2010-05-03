
using System;

namespace OpenMetaverse
{


	public class Voxel
	{
		public uint X;
		public uint Y;
		public uint Z;
		public uint MaterialID;
		public Voxel ()
		{
		}
		
		public Voxel(uint x,uint y,uint z,uint m)
		{
			X=x;
			Y=y;
			Z=z;
			MaterialID=m;
		}
		
		public Voxel(byte[] bytes, int pos)
		{
			FromBytes(bytes,pos);
		}
		public void FromBytes(byte[] bytes, int pos)
		{
			if (!BitConverter.IsLittleEndian)
            {
                // Big endian architecture
                byte[] conversionBuffer = new byte[16];

                Buffer.BlockCopy(bytes, pos, conversionBuffer, 0, 16);

                Array.Reverse(conversionBuffer, 0, 4);
                Array.Reverse(conversionBuffer, 4, 4);
                Array.Reverse(conversionBuffer, 8, 4);
                Array.Reverse(conversionBuffer, 12, 4);

                X = BitConverter.ToUInt32(conversionBuffer, 0);
                Y = BitConverter.ToUInt32(conversionBuffer, 4);
                Z = BitConverter.ToUInt32(conversionBuffer, 8);
                MaterialID = BitConverter.ToUInt32(conversionBuffer, 12);
            }
            else
            {
                // Little endian architecture
                X = BitConverter.ToUInt32(bytes, pos);
                Y = BitConverter.ToUInt32(bytes, pos + 4);
                Z = BitConverter.ToUInt32(bytes, pos + 8);
                MaterialID = BitConverter.ToUInt32(bytes, pos + 12);
            }
		}
		
		public void ToBytes(byte[] dest,int pos)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(X), 			0, dest, pos + 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 			0, dest, pos + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 			0, dest, pos + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(MaterialID),	0, dest, pos + 12, 4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(dest, pos + 0, 4);
                Array.Reverse(dest, pos + 4, 4);
                Array.Reverse(dest, pos + 8, 4);
                Array.Reverse(dest, pos + 12, 4);
            }
		}
		
		public byte[] GetBytes()
		{
			byte[] b = new byte[16];
			FromBytes(b,0);
			return b;
		}
	}
}
