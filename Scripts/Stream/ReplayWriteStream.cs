using System.IO;
using IREX.ReplaySystem.Monobehaviours;
using UnityEngine;

namespace IREX.ReplaySystem.Stream
{
    public class ReplayWriteStream : IReplayWriter
    {
        private readonly BinaryWriter _writer;
        private readonly bool _compressFloat = ReplayManager.Instance.compressFloats;
        public ReplayWriteStream(System.IO.Stream stream)
        {
            _writer = new BinaryWriter(stream);
        }
    
        public void Write(Vector3 vector3)
        {
            Write(vector3.x);
            Write(vector3.y);
            Write(vector3.z);
        }

        public void Write(Quaternion quaternion)
        {

            /*
        var max = quaternion.x;
        byte index = 0;
        
        if (quaternion.y > max)
        {
            max = quaternion.y;
            index = 1;
        }
        
        if (quaternion.z > max)
        {
            max = quaternion.z;
            index = 2;
        }
        
        if (quaternion.w > max)
        {
            max = quaternion.w;
            index = 3;
        }

        Write(index);

        switch (index)
        {
            case 0:
                Write(quaternion.y);
                Write(quaternion.z);
                Write(quaternion.w);
                break;
            case 1:
                Write(quaternion.x);
                Write(quaternion.z);
                Write(quaternion.w);
                break;
            case 2:
                Write(quaternion.x);
                Write(quaternion.y);
                Write(quaternion.w);
                break;
            case 3:
                Write(quaternion.x);
                Write(quaternion.y);
                Write(quaternion.z);
                break;
        }
        */
            Write(quaternion.x);
            Write(quaternion.y);
            Write(quaternion.z);
            Write(quaternion.w);
        }
    
        public void Write(byte x)
        {        
            _writer.Write(x);
        }

        public void Write(ushort x)
        {        
            _writer.Write(x);
        }

    
        public void Write(int x)
        {
            _writer.Write(x);
        }
    
        public void Write(bool x)
        {        
            _writer.Write(x);
        }
    
        public void Write(float x)
        {
            if (_compressFloat)
            {
                var fixedValue = (short)(x * 512);
                _writer.Write(fixedValue);
            }
            else
            {
                _writer.Write(x);
            }

        }
    
        public void Write(string x)
        {
            _writer.Write(x);
        }
    
        public void Write(long x)
        {        
            _writer.Write(x);
        }


        public System.IO.Stream GetStream()
        {
            return _writer.BaseStream;
        }

        public BinaryWriter GetBinaryWriter()
        {
            return _writer;
        }
    }
}


