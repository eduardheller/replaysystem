using UnityEngine;

namespace IREX.ReplaySystem.Stream
{
    public interface IReplayReader 
    {
        Vector3 ReadVector3();
        Quaternion ReadQuaternion();
        byte ReadByte();
        ushort ReadUInt16();
        int ReadInt32();
        long ReadInt64();
        int Read();
        bool ReadBoolean();
        float ReadSingle();
        string ReadString();
        System.IO.Stream GetStream();
    }
}
