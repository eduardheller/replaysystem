using UnityEngine;

namespace IREX.ReplaySystem.Stream
{
    public interface IReplayWriter
    {
        void Write(Vector3 vector3);

        void Write(Quaternion quaternion);
        
        void Write(byte x);

        void Write(ushort x);
        void Write(int x);

        void Write(bool x);

        void Write(float x);
        void Write(string x);

        void Write(long x);
        System.IO.Stream GetStream();
    }
}
