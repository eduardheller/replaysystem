using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using Debug = UnityEngine.Debug;

namespace IREX.ReplaySystem.ReplayFrame
{
    public class ReplayMetaHeader : IReplayMetaFrame
    {
        public byte Version;
        public byte Hz;
        public byte DeltaMode;
        public byte SceneIndex;
        public byte TimelineCount;
        public int SizeInBytes
        {
            get => sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte);
        }

        public ReplayMetaHeader()
        {
            Version = 0;
            SceneIndex = 0;
            TimelineCount = 0;
            Hz = 0;
            DeltaMode = 0;
        }

        public ReplayMetaHeader(byte version, byte sceneIndex, byte hz, ReplayDeltaMode deltaMode, byte timelineCount)
        {
            Version = version;
            SceneIndex = sceneIndex;
            Hz = hz;
            DeltaMode = (byte) deltaMode;
            TimelineCount = timelineCount;
        }


        public int WriteFrame(IReplayWriter writer)
        {
            writer.Write(Version);
            writer.Write(SceneIndex);
            writer.Write(Hz);
            writer.Write(DeltaMode);
            writer.Write(TimelineCount);
        
            UnityEngine.Debug.Log("Write Header\n" + this);
            return SizeInBytes;
        }
        
        public int ReadFrame(IReplayReader reader)
        {
            Version = reader.ReadByte();
            SceneIndex = reader.ReadByte();
            Hz = reader.ReadByte();
            DeltaMode = reader.ReadByte();
            TimelineCount = reader.ReadByte();
            
            UnityEngine.Debug.Log("Read Header\n" + this);
            
            return SizeInBytes;
        }
        
        public override string ToString()
        {
            var message = $"Version: {Version}\n" +
                          $"Scene Index: {SceneIndex}\n" +
                          $"Hz: {Hz}\n" +
                          $"Delta Mode: {(ReplayDeltaMode)DeltaMode}\n" +
                                $"Timelines: {TimelineCount}";
            return message;
        }
    }
}




