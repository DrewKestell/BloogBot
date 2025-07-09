using System;
using System.IO;
using System.Text;

namespace VMAP
{
    /// <summary>
    /// Represents a model spawn point in a map, loaded from .vmtree/.vmtile files.
    /// Contains position, rotation, scale, bounds and identifying info.
    /// </summary>
    public class ModelSpawn
    {
        public ModelFlags flags;
        public ushort adtId;     // ADT tile id (for grouping in terrain)
        public uint ID;          // Unique spawn ID or reference
        public Vector3 iPos;     // Position in world coordinates
        public Vector3 iRot;     // Rotation in degrees (pitch, yaw, roll) or (Y, X, Z) depending on usage
        public float iScale;     // Uniform scale
        public AABox iBound;     // Bounding box of the model in world coordinates (if provided in data)
        public string name = string.Empty; // Model filename or identifier

        public override bool Equals(object? obj)
        {
            if (obj is ModelSpawn other)
                return this.ID == other.ID;
            return false;
        }
        public override int GetHashCode() => (int)ID;

        /// <summary>
        /// Read a ModelSpawn from a binary stream (exactly as written by extractor).
        /// Returns false if at EOF or on error.
        /// </summary>
        public static bool ReadFromFile(BinaryReader br, out ModelSpawn spawn)
        {
            spawn = new ModelSpawn();
            try
            {
                //Console.WriteLine("ModelSpawn: Starting ReadFromFile");

                // flags, ADT id, instance ID
                //Console.WriteLine("ModelSpawn: Reading flags...");
                uint flagsVal = br.ReadUInt32();
                spawn.flags = (ModelFlags)flagsVal;
                //Console.WriteLine($"ModelSpawn: flags=0x{flagsVal:X}");

                //Console.WriteLine("ModelSpawn: Reading ADT ID...");
                spawn.adtId = br.ReadUInt16();
                //Console.WriteLine($"ModelSpawn: adtId={spawn.adtId}");

                //Console.WriteLine("ModelSpawn: Reading instance ID...");
                spawn.ID = br.ReadUInt32();
                //Console.WriteLine($"ModelSpawn: ID={spawn.ID}");

                // position
                //Console.WriteLine("ModelSpawn: Reading position...");
                float px = br.ReadSingle();
                float py = br.ReadSingle();
                float pz = br.ReadSingle();
                spawn.iPos = new Vector3(px, py, pz);
                //Console.WriteLine($"ModelSpawn: iPos={spawn.iPos}");

                // rotation
                //Console.WriteLine("ModelSpawn: Reading rotation...");
                float rx = br.ReadSingle();
                float ry = br.ReadSingle();
                float rz = br.ReadSingle();
                spawn.iRot = new Vector3(rx, ry, rz);
                //Console.WriteLine($"ModelSpawn: iRot={spawn.iRot}");

                // scale
                //Console.WriteLine("ModelSpawn: Reading scale...");
                spawn.iScale = br.ReadSingle();
                //Console.WriteLine($"ModelSpawn: iScale={spawn.iScale}");

                // optional bounding box
                if (spawn.flags.HasFlag(ModelFlags.MOD_HAS_BOUND))
                {
                    //Console.WriteLine("ModelSpawn: Reading bounding box...");
                    float blx = br.ReadSingle();
                    float bly = br.ReadSingle();
                    float blz = br.ReadSingle();
                    float bhx = br.ReadSingle();
                    float bhy = br.ReadSingle();
                    float bhz = br.ReadSingle();
                    spawn.iBound = new AABox(new Vector3(blx, bly, blz), new Vector3(bhx, bhy, bhz));
                    //Console.WriteLine($"ModelSpawn: iBound Min={spawn.iBound.Min}, Max={spawn.iBound.Max}");
                }
                else
                {
                    spawn.iBound = AABox.Zero;
                    //Console.WriteLine("ModelSpawn: No bounding box present");
                }

                // name
                //Console.WriteLine("ModelSpawn: Reading name length...");
                uint nameLen = br.ReadUInt32();
                //Console.WriteLine($"ModelSpawn: nameLen={nameLen}");
                if (nameLen > 0)
                {
                    //Console.WriteLine("ModelSpawn: Reading name bytes...");
                    byte[] nameBytes = br.ReadBytes((int)nameLen);
                    if (nameBytes.Length != nameLen)
                        throw new EndOfStreamException("Unexpected EOF while reading ModelSpawn name");
                    spawn.name = System.Text.Encoding.ASCII.GetString(nameBytes);
                    //Console.WriteLine($"ModelSpawn: name='{spawn.name}'");
                }
                else
                {
                    spawn.name = string.Empty;
                    //Console.WriteLine("ModelSpawn: No name present");
                }

                //Console.WriteLine("ModelSpawn: ReadFromFile completed successfully");
                return true;
            }
            catch (EndOfStreamException eof)
            {
                Console.WriteLine($"ModelSpawn: EndOfStreamException: {eof.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ModelSpawn: Exception: {ex.Message}");
                return false;
            }
        }

    }
}
