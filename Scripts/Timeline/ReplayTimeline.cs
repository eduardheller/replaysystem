using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using UnityEngine;

namespace IREX.ReplaySystem.Timeline
{
    public class ReplayTimeline 
    {
        public int TimelineIndex { get; private set; }
        public float ReplayLength { get; private set; }
        public int Frame { get; set; }
        public float PlayingTime { get; set; }
        public int FrameSize { get; private set; }
        public readonly ReplayMetaTimelineHeader MetaTimelineHeader;
        public readonly IReplayFramePlayer ReplaySnapshotPlayer;
        public readonly IReplayFramePlayer ReplayDeltaPlayer;
        public IReplayFramePlayer CurrentFramePlayer;
        private readonly IReplayReader _reader;
        public ReplayDeltaMode deltaMode {get;}
        
        public ReplayTimeline(ReplayMetaHeader metaHeader, ReplayMetaTimelineHeader metaTimelineHeader, IReplayReader reader, int index)
        {
            MetaTimelineHeader = metaTimelineHeader;
            _reader = reader;
            
            ReplaySnapshotPlayer = new ReplaySnapshotPlayer(reader, 
                metaHeader.Hz , 
                metaTimelineHeader);
            
            ReplayDeltaPlayer = ReplayDeltaFactory.CreateReplayDeltaPlayer(reader, 
                metaHeader.Hz, (ReplayDeltaMode)metaHeader.DeltaMode, 
                metaTimelineHeader, 
                ReplaySnapshotPlayer);
            
            TimelineIndex = index;
            this.deltaMode = (ReplayDeltaMode)metaHeader.DeltaMode;
            ReplayLength = ReplaySnapshotPlayer.GetLastFrameTime();
            FrameSize = metaTimelineHeader.ReplayPointer.Count - 2;
            CurrentFramePlayer = ReplaySnapshotPlayer;
        }
        


        
    }
}
