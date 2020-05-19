using IREX.ReplaySystem.Stream;

namespace IREX.ReplaySystem.State
{
    public interface IReplayObservable
    {
        void OnReplayRecord(IReplayWriter stream, ReplayInfo info);
        void OnReplayPlayInit(ReplayInfo info);
        void OnReplayPlay(IReplayReader stream, ReplayInfo info);
    }
}
