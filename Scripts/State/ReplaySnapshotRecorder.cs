using System.Collections;
using System.Collections.Generic;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;

namespace IREX.ReplaySystem.State
{
    public class ReplaySnapshotRecorder : IReplayFrameRecorder
    {
        

        public ReplaySnapshotRecorder(IReplayWriter replayWriter) : base(replayWriter,null)
        {
        }
        
        public override void WriteFrame(float time, int frame, int deltaFrames)
        {
            WriteDeltaFrameCount(deltaFrames);
            WriteFrameTime(time);
            WriteObjectsIdsAndNames();
            WriteObservableEntities();
        }

        public override void WriteObjectsIdsAndNames()
        {
            ReplayWriter.Write(ReplayEntities.Count);

            foreach (var replayEntity in ReplayEntities)
            {
                ReplayWriter.Write(replayEntity.Id);
                ReplayWriter.Write(replayEntity.GameObjectName);
                ReplayWriter.Write(replayEntity.transform.position);
                ReplayWriter.Write(replayEntity.transform.rotation);
            }
                
            FrameEntities = new List<ReplayEntity>(ReplayEntities);
        }

        public override void WriteObservableEntities()
        {
            RecordAllReplayObservable(StaticEntities, ReplayStateMode.Snapshot);
            RecordAllReplayObservable(ReplayEntities, ReplayStateMode.Snapshot);
        }
    }
}