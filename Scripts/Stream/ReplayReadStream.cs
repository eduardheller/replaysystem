using System.IO;
using IREX.ReplaySystem.Monobehaviours;
using UnityEngine;

namespace IREX.ReplaySystem.Stream
{
    public class ReplayReadStream : IReplayReader
    {
        private readonly BinaryReader _reader;
        private readonly bool _compressFloat = ReplayManager.Instance.compressFloats;
        public ReplayReadStream(System.IO.Stream stream)
        {
            _reader = new BinaryReader(stream);
        }
    
        public Vector3 ReadVector3()
        {
            var x = ReadSingle();
            var y = ReadSingle();
            var z = ReadSingle();
            return new Vector3(x,y,z);
        }

        public Quaternion ReadQuaternion()
        {        
            /* 
            var index = ReadByte();
            var a = ReadSingle();
            var b = ReadSingle();
            var c = ReadSingle();
            
            var d = Mathf.Sqrt(1 - (a * a) - (b * b) - (c * c));

            switch (index)
            {
                case 0:
                    return new Quaternion(d,a,b,c);
                case 1:
                    return new Quaternion(a,d,b,c);
                case 2:
                    return new Quaternion(a,b,d,c);
                case 3:
                    return new Quaternion(a,b,c,d);
            }
            
            Debug.LogError("Index ist "+index);
            return new Quaternion(a,b,c,d);
            
            */
            var a = ReadSingle();
            var b = ReadSingle();
            var c = ReadSingle();
            var d = ReadSingle();
            return new Quaternion(a,b,c,d);
        }
    
        public byte ReadByte()
        {
            return _reader.ReadByte();
        }
    
        public ushort ReadUInt16()
        {        
            return _reader.ReadUInt16();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }
    
        public long ReadInt64()
        {        
            return _reader.ReadInt64();
        }
    
        public int Read()
        {
            return _reader.Read();
        }
    
        public bool ReadBoolean()
        {        
            return _reader.ReadBoolean();
        }
    
        public float ReadSingle()
        {
            var val = 0f;
            if (_compressFloat)
                val = (_reader.ReadInt16() / 512f);
            else
                val = _reader.ReadSingle();
            return val;
        }
    
        public string ReadString()
        {        
            return _reader.ReadString();
        }

        public System.IO.Stream GetStream()
        {
            return _reader.BaseStream;
        }

        public BinaryReader GetBinaryReader()
        {
            return _reader;
        }
    }
}
