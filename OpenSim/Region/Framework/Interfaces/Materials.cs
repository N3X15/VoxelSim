using System;
using OpenMetaverse;
using System.Collections.Generic;
using System.Xml;
using LibNbt.Tags;

namespace OpenSim.Region.Framework.Interfaces
{

    // Provide a few common things to the terrain generator.
    public class MaterialMap
    {
        private Dictionary<byte, VoxMaterial> mMaterials = new Dictionary<byte, VoxMaterial>();
        private Dictionary<string, byte> mName2Byte = new Dictionary<string, byte>();
        private byte index = 0;
        public MaterialMap()
        {
            Add("Air", new VoxMaterial());
            Add("Rock", new VoxMaterial());
            Add("Water", new VoxMaterial());
            Add("Soil", new VoxMaterial());
            Add("Grass", new VoxMaterial());
            Add("Sand", new VoxMaterial());

            Rock.Flags = MatFlags.Solid;
            Water.Flags = MatFlags.Fluid;
            Soil.Flags = MatFlags.Solid;
            Grass.Flags = MatFlags.Solid;
            Sand.Flags = MatFlags.Solid;
        }

        public VoxMaterial Air { get { return mMaterials[0]; } }
        public VoxMaterial Rock { get { return mMaterials[1]; } }
        public VoxMaterial Water { get { return mMaterials[2]; } }
        public VoxMaterial Soil { get { return mMaterials[3]; } }
        public VoxMaterial Grass { get { return mMaterials[4]; } }
        public VoxMaterial Sand { get { return mMaterials[5]; } }

        public VoxMaterial this[byte i]
        {
            get
            {
                if (mMaterials.ContainsKey(i))
                    return mMaterials[i];
                else
                    return new VoxMaterial();
            }
            set
            {
                if (mMaterials.ContainsKey(i))
                    mMaterials[i] = value;
            }
        }

        public VoxMaterial this[string n]
        {
            get
            {
                if (mName2Byte.ContainsKey(n))
                    return mMaterials[mName2Byte[n]];
                else
                    return new VoxMaterial();
            }
            set
            {
                if (mName2Byte.ContainsKey(n))
                    mMaterials[mName2Byte[n]] = value;
                else
                    Add(n, value);
            }
        }

        public void Add(string name, VoxMaterial mat)
        {
            mat.ID = index;
            mMaterials.Add(mat.ID, mat);
            mName2Byte.Add(name, mat.ID);
            index++;
        }

        public void Remove(string name)
        {
            if (mName2Byte.ContainsKey(name))
                mMaterials.Remove(mName2Byte[name]);
            index--;
        }

        public void Remove(byte id)
        {
            if (mMaterials.ContainsKey(id))
                mMaterials.Remove(id);
            index--;
        }

        internal void Serialize(ref XmlWriter w)
        {
            w.WriteStartElement("materials");
            w.WriteAttributeString("version", "1");
            foreach (KeyValuePair<byte, VoxMaterial> kvp in mMaterials)
            {
                w.WriteStartElement("material");
                VoxMaterial mat = kvp.Value;
                w.WriteAttributeString("id", mat.ID.ToString());
                w.WriteAttributeString("name", mat.Name.ToString());
                w.WriteAttributeString("flags", mat.Flags.ToString());
                w.WriteAttributeString("density", mat.Density.ToString());
                w.WriteAttributeString("deposit", mat.Deposit.ToString());
                w.WriteAttributeString("texture", mat.Texture.ToString());
                w.WriteAttributeString("type", mat.Type.ToString());
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        internal void Deserialize(ref XmlElement doc)
        {
            int version = 0;
            if (!int.TryParse(doc.GetAttribute("version"), out version))
                return;

            if (version > 1)
                return;

            foreach (XmlElement material in doc)
            {
                VoxMaterial mat = new VoxMaterial();
                mat.ID = byte.Parse(material.GetAttribute("id"));
                mat.Name = material.GetAttribute("name");
                mat.Flags = (MatFlags)Enum.Parse(typeof(MatFlags), material.GetAttribute("flags"));
                mat.Density = float.Parse(material.GetAttribute("density"));
                mat.Deposit = (DepositType)Enum.Parse(typeof(DepositType), material.GetAttribute("deposit"));
                mat.Texture = UUID.Parse(material.GetAttribute("texture"));
                mat.Type = (MaterialType)Enum.Parse(typeof(MaterialType), material.GetAttribute("type"));

                if (mat.ID > index)
                {
                    index = mat.ID;
                    index++;
                }

                mMaterials.Add(mat.ID, mat);
                mName2Byte.Add(mat.Name, mat.ID);
            }
        }

        internal void Deserialize(XmlTextReader reader)
        {
            bool keepreading = true;
            while (keepreading)
            {
                reader.Read();
                if (reader.Name.Equals("material") && reader.NodeType.Equals(XmlNodeType.Element))
                {
                    VoxMaterial mat = new VoxMaterial();
                    mat.ID = (byte)reader.ReadContentAsInt();
                    mat.Name = reader.ReadContentAsString();
                    mat.Flags = (MatFlags)Enum.Parse(typeof(MatFlags), reader.ReadContentAsString());
                    mat.Density = reader.ReadContentAsFloat();
                    mat.Deposit = (DepositType)Enum.Parse(typeof(DepositType), reader.ReadContentAsString());
                    mat.Texture = UUID.Parse(reader.ReadContentAsString());
                    mat.Type = (MaterialType)Enum.Parse(typeof(MaterialType), reader.ReadContentAsString());

                    if (mat.ID > index)
                    {
                        index = mat.ID;
                        index++;
                    }

                    mMaterials.Add(mat.ID, mat);
                    mName2Byte.Add(mat.Name, mat.ID);
                }
            }
        }

        internal NbtTag ToNBT()
        {
            NbtCompound mt = new NbtCompound("MaterialsTable");
            foreach (KeyValuePair<byte, VoxMaterial> mat in mMaterials)
            {
                NbtCompound cMat = new NbtCompound(mat.Value.Name);
                cMat.Tags.Add(new NbtByte("ID", mat.Value.ID));
                cMat.Tags.Add(new NbtInt("Type", (int)mat.Value.Type));
                cMat.Tags.Add(new NbtFloat("Density", mat.Value.Density));
                cMat.Tags.Add(new NbtInt("Deposit", (int)mat.Value.Deposit));
                cMat.Tags.Add(new NbtString("Texture", mat.Value.Texture.ToString()));
                cMat.Tags.Add(new NbtByte("Flags", (byte)mat.Value.Flags));
                mt.Tags.Add(cMat);
            }
            return mt;
        }


        internal void FromNbt(NbtCompound c)
        {
            foreach (NbtTag tag in c.Tags)
            {
                VoxMaterial m = new VoxMaterial();
                m.Name = tag.Name;
                foreach (NbtTag t in (tag as NbtCompound).Tags)
                {
                    switch (t.Name)
                    {
                        case "ID": m.ID = (t as NbtByte).Value; break;
                        case "Type": m.Type = (MaterialType)(t as NbtInt).Value; break;
                        case "Density": m.Density = (t as NbtFloat).Value; break;
                        case "Deposit": m.Deposit = (DepositType)(t as NbtInt).Value; break;
                        case "Texture": m.Texture = new UUID((t as NbtString).Value); break;
                        case "Flags": m.Flags = (MatFlags)(t as NbtByte).Value; break;
                    }
                }
                mMaterials.Add(m.ID, m);
                mName2Byte.Add(m.Name, m.ID);
                if (m.ID > index)
                {
                    index = m.ID;
                    index++;
                }

            }
        }
    }
    public enum MaterialType : int
    {
        Soil,
        Sand,
        Igneous,
        Metamorphic,
        Sedimentary
    }

    public enum DepositType : int
    {
        Layer,
        SmallCluster,
        LargeCluster,
        Vein
    }
    [Flags]
    public enum MatFlags : byte
    {
        Solid = 0x01,
        Fluid = 0x02,
        Damp = 0x04,
        Toxic = 0x08
    }
    public class VoxMaterial
    {
        public byte ID = 0x00;
        public string Name = "Granite";
        public MaterialType Type = MaterialType.Igneous;
        public float Density = 2.75f;
        public UUID Texture = UUID.Zero;
        public DepositType Deposit = DepositType.Layer;
        public MatFlags Flags = (MatFlags)0x00;
    }
}

