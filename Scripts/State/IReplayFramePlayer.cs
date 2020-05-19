using System.Collections.Generic;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;
using UnityEngine;

namespace IREX.ReplaySystem.State
{
    public abstract class IReplayFramePlayer
    {
        public float FrameTime { get;  set;  }
        protected int CurrentFrame { get;  set;  }
        protected IReplayReader ReplayReader { get;  }
        private ReplayPointer ReplayPointer { get; }
        
        protected int Hz { get; }

        protected ReplayMetaTimelineHeader THeader { get; }
        protected IReplayFramePlayer BaseFrame { get; }
        private int TimelineFromFrame { get; }
        
        protected readonly List<ReplayEntity> ReplayEntities;
        protected readonly List<ReplayEntity> StaticEntities;

        protected IReplayFramePlayer(IReplayReader reader, int hz, ReplayMetaTimelineHeader theader, IReplayFramePlayer baseFrame)
        {
            ReplayReader = reader;
            this.Hz = hz;
            ReplayEntities = ReplayManager.Instance.ReplayEntities;
            StaticEntities = ReplayManager.Instance.ReplayStaticEntities;
            TimelineFromFrame = theader.StartFromFrame;
            THeader = theader;
            ReplayPointer = theader.ReplayPointer;
            BaseFrame = baseFrame;
        }
        
        public int ReadDeltaFrameCount()
        {
            return ReplayReader.ReadByte();
        }

        public float ReadFrameTime()
        {
            FrameTime = ReplayReader.ReadSingle();
            //Debug.Log("Frametime Read " + FrameTime);
            return FrameTime;
        }

        public float GetLastFrameTime()
        {
            var lastPosition = ReplayPointer.GetLastStartingPositionIndex;
            THeader.SetPointerPosition(ReplayReader,lastPosition);
            ReadDeltaFrameCount();
            var time = ReadFrameTime();
            // first Snapshot is always at frametime = 0 
            FrameTime = 0; 
            return time;
        }
        
        protected void PlayAllReplayObservable(List<ReplayEntity> replayEntities, bool executeState, ReplayStateMode replayMode, ReplayDeltaMode deltaMode, float delta)
        {
            foreach (var entity in replayEntities)
            {
                entity.PlayAllReplayObservable(ReplayReader,new ReplayInfo() 
                {  
                    ReplayStateMode = replayMode,
                    PositionOffset = THeader.CurrentOffset, 
                    Delta = delta,
                    ReplayDeltaMode = deltaMode,
                    ExecuteState =  executeState
                });
            }
        }
        
        protected void PlayAllReplayObservableConstruction(List<ReplayEntity> replayEntities, ReplayStateMode replayMode, ReplayDeltaMode deltaMode, float delta)
        {
            foreach (var entity in replayEntities)
            {
                entity.PlayAllReplayObservableConstruction(ReplayReader,new ReplayInfo() 
                {  
 
                    ReplayStateMode = replayMode,
                    PositionOffset = THeader.CurrentOffset, 
                    Delta = delta,
                    ReplayDeltaMode = deltaMode,
                });
            }
        }
        
        public abstract float ReadFrame(int currentFrame, int deltaFrames, float deltaTime);
        public abstract void ReadObjectsIdsAndNames();
        public abstract void ReadObservableEntitiesInit(float delta);
        public abstract void ReadObservableEntities(bool executeState, float delta);
    }
}