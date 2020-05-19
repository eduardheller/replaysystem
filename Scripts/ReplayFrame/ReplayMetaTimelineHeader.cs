
using System.Collections.Generic;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using IREX.ReplaySystem.Timeline;
using UnityEngine;
using UnityEngine.UI;

namespace IREX.ReplaySystem.ReplayFrame
{
    public class ReplayMetaTimelineHeader : IReplayMetaFrame
    {
        public byte TimelineInheritance;
        public int StartFromFrame;
        public ReplayPointer ReplayPointer { get; }
        private int _sizeOfPositions;
        
        private int _currentFrame { get; set; } 
        public int CurrentOffset { get;  set;  }
        
        public int SizeInBytes
        {
            get
            {
                var snapshotStartPositionsSize = sizeof(int) + _sizeOfPositions * sizeof(int);
                return sizeof(byte) + snapshotStartPositionsSize + sizeof(int);
            }
        }
        
        public ReplayMetaTimelineHeader()
        {
            ReplayPointer = new ReplayPointer();
            TimelineInheritance = 0;
            StartFromFrame = 0;
            _currentFrame = -1;
        }

        public ReplayMetaTimelineHeader(byte timelineInheritance, int fromFrame, ReplayPointer replayPointer)
        {
            TimelineInheritance = timelineInheritance;
            StartFromFrame = fromFrame;
            this.ReplayPointer = replayPointer;
            _currentFrame = -1;
        }
        
        public int WriteFrame(IReplayWriter writer)
        {
            writer.Write(TimelineInheritance);
            writer.Write(StartFromFrame);
            writer.Write(ReplayPointer.Count);
            
            foreach (var memoryPosition in ReplayPointer.GetEntries())
            {
                writer.Write(memoryPosition.Index);
            }

            Debug.Log("Write Timelineheader\n" + this);
            return SizeInBytes;
        }

        public int ReadFrame(IReplayReader reader)
        {
            TimelineInheritance = reader.ReadByte();
            StartFromFrame = reader.ReadInt32();
            _sizeOfPositions = reader.ReadInt32();

            return SizeInBytes;
        }

        public void ReadSnapshotPointers(IReplayReader reader, int off)
        {
            for (var i = 0; i < _sizeOfPositions; i++)
            {
                var pos = reader.ReadInt32() + off;
                ReplayPointer.Add(pos,off);
            }
            Debug.Log("Read Timelineheader\n" +this);
        }
        
        public bool SetPointerPosition(IReplayReader reader, int frame)
        {
            if (frame == _currentFrame)
                return false;
            

            _currentFrame = frame;
            var position = ReplayPointer.GetStartingPositionAt(frame);
            CurrentOffset = position.Offset;
   
            //Debug.Log($"Position: {_currentFrame} with Bytes: {position.Index}");
            // Add Offset from Headers and Startingposition of Snapshot to this Snapshot
            reader.GetStream().Position = position.Index;
            return true;
        }
                
        public override string ToString()
        {
            var message = $"StartPosition: {ReplayPointer.GetFirstStartingPosition.Index}\n" +
                          $"TimelineInheritance: {TimelineInheritance}\n" +
                          $"StartFromFrame: {StartFromFrame}\n" +
                          $"EndPosition: {ReplayPointer.GetLastEndingPosition.Index}";
            
            return message;
        }
    }
}


