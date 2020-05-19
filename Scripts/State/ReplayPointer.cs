using System.Collections.Generic;
using System.Diagnostics.Contracts;
using IREX.ReplaySystem.Timeline;
using UnityEngine;

namespace IREX.ReplaySystem.State
{
    public class ReplayPointer
    {
        public struct Snap
        {
            public int Index;
            public int Offset;
        }
        
        private readonly List<Snap> _replayPositionsInStream;
        public int Count => _replayPositionsInStream.Count;
        
        
        public ReplayPointer()
        {
            _replayPositionsInStream = new List<Snap>();
        }
        
        public void AddParentFrames(ReplayTimeline parentTimeline,int fromFrame)
        {
            _replayPositionsInStream.InsertRange(0,
                parentTimeline.MetaTimelineHeader.ReplayPointer.GetEntries().GetRange(0,fromFrame));
        }
        
        
        // Index is -2, because the last position is the startbyte non existing snapshot in the end
        public Snap GetLastStartingPosition => _replayPositionsInStream[_replayPositionsInStream.Count - 2];
        public int GetLastStartingPositionIndex => _replayPositionsInStream.Count - 2;


        

        public Snap GetLastEndingPosition => _replayPositionsInStream[_replayPositionsInStream.Count - 1];
        public int GetLastEndingPositionIndex => _replayPositionsInStream.Count - 1;

    
    
        public Snap GetFirstStartingPosition => _replayPositionsInStream[0];
    

        public Snap GetStartingPositionAt(int pos)
        {
            return _replayPositionsInStream[pos];
        }


        public void Clear()
        {
            _replayPositionsInStream.Clear();
        }

        public void Add(int startingPosition, int offset)
        {
            _replayPositionsInStream.Add(new Snap() { Index = startingPosition, Offset = offset});
        }

        public List<Snap> GetEntries() => _replayPositionsInStream;


    }
}
