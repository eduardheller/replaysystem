

namespace IREX.ReplaySystem.State
{
    public enum ReplayStateMode
    {
        Snapshot,
        Delta
    }

    public enum ReplayDeltaMode
    {
        None,
        SubFrame,
        DeltaCompression
    }
    
    public struct ReplayInfo
    {
        public int PositionOffset;
        public float Delta;
        public ReplayStateMode ReplayStateMode;
        public ReplayDeltaMode ReplayDeltaMode;
        public bool ExecuteState;
    }
}