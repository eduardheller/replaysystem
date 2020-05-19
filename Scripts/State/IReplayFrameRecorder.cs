using System.Collections.Generic;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.Stream;

namespace IREX.ReplaySystem.State
{
    public abstract class IReplayFrameRecorder
    {

        protected IReplayWriter ReplayWriter { get; }
        protected readonly List<ReplayEntity> ReplayEntities;
        protected readonly List<ReplayEntity> StaticEntities;
        public List<ReplayEntity> FrameEntities;
        protected IReplayFrameRecorder BaseFrame { get; }

        protected IReplayFrameRecorder(IReplayWriter replayWriter, IReplayFrameRecorder baseFrame)
        {
            ReplayWriter = replayWriter;
            ReplayEntities = ReplayManager.Instance.ReplayEntities;
            StaticEntities = ReplayManager.Instance.ReplayStaticEntities;
            BaseFrame = baseFrame;
        }
        
        public abstract void WriteFrame(float time, int frame, int deltaFrames);

        public abstract void WriteObjectsIdsAndNames();

        public abstract void WriteObservableEntities();
        
        
        protected void WriteDeltaFrameCount(float deltaFrames)
        {
            ReplayWriter.Write((byte)deltaFrames);
        }
        
        protected void WriteFrameTime(float time)
        {
            ReplayWriter.Write(time);
            //Debug.Log("Frametime Write " + FrameTime);
        }
        
        
        protected void RecordAllReplayObservable(List<ReplayEntity> replayEntities, ReplayStateMode replayMode)
        {
            foreach (var entity in replayEntities)
            {
                entity.RecordAllReplayObservable(ReplayWriter,new ReplayInfo() 
                {  
                    ReplayStateMode = replayMode,
                    PositionOffset = 0, 
                    ReplayDeltaMode = ReplayManager.Instance.replayDeltaMode
                });
            }
        }
        


    }
}