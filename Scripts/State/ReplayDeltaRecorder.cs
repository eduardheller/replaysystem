using System.Collections.Generic;
using System.Linq;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;

namespace IREX.ReplaySystem.State
{
    public class ReplayDeltaRecorder : IReplayFrameRecorder
    {

        private List<ReplayEntity> _replayEntitiesSnapshot;

        public ReplayDeltaRecorder(IReplayWriter replayWriter, IReplayFrameRecorder baseFrame) : base(replayWriter, baseFrame)
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
            var entities = new List<ReplayEntity>();
            
            var i = 0;
            foreach (var replayEntity in ReplayEntities.Where(replayEntity => !BaseFrame.FrameEntities.Contains(replayEntity)))
            {
                entities.Add(replayEntity);
                i++;
            }

            var j = 0;
            foreach (var replayEntity in BaseFrame.FrameEntities.Where(replayEntity => !ReplayEntities.Contains(replayEntity)))
            {
                entities.Add(replayEntity);
                j++;
            }
            if(i+j>0)
                BaseFrame.FrameEntities = new List<ReplayEntity>(ReplayEntities);
            
            ReplayWriter.Write((byte)(i+j));
            for(var z = 0; z<(i+j); z++)
            {
                ReplayWriter.Write(entities[z].Id);
                if (z < i) // instantiate those
                    ReplayWriter.Write(entities[z].GameObjectName);
                else if (z < (j + i)) // delete those
                    ReplayWriter.Write(""); // u dont need the name for objects to delte cause they exist already
            }
        }

        public override void WriteObservableEntities()
        {
            RecordAllReplayObservable(StaticEntities, ReplayStateMode.Delta);
            RecordAllReplayObservable(ReplayEntities, ReplayStateMode.Delta);
        }
    }
}