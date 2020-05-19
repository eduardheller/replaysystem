using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;

namespace IREX.ReplaySystem.State
{
    public static class ReplayDeltaFactory
    {
        public static IReplayFrameRecorder CreateReplayDeltaRecorder(IReplayWriter writer, 
            IReplayFrameRecorder baseFrame, ReplayDeltaMode replayDeltaMode)
        {
            switch (replayDeltaMode)
            {
                case ReplayDeltaMode.DeltaCompression:
                    return new ReplayDeltaRecorder(writer,baseFrame);
                case ReplayDeltaMode.SubFrame:
                    return new ReplaySubframeRecorder(writer,baseFrame);
                default:
                    return null;
            }
        }
        
        public static IReplayFramePlayer CreateReplayDeltaPlayer(IReplayReader reader, int hz, ReplayDeltaMode deltaMode, 
            ReplayMetaTimelineHeader metaTimelineHeader, IReplayFramePlayer baseFrame)
        {
            switch (deltaMode)
            {
                case ReplayDeltaMode.DeltaCompression:
                    return new ReplayDeltaPlayer(reader, hz,metaTimelineHeader,baseFrame);
                case ReplayDeltaMode.SubFrame:
                    return new ReplaySubframePlayer(reader, hz,metaTimelineHeader,baseFrame);
                default:
                    return null;
            }
        }
    }
}