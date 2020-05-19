using IREX.ReplaySystem.Stream;

namespace IREX.ReplaySystem.ReplayFrame
{
    public interface IReplayMetaFrame
    {
        int WriteFrame(IReplayWriter writer);
        int ReadFrame(IReplayReader reader);
        int SizeInBytes { get; }

    }
}