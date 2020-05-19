using System.Collections.Generic;
using System.Linq;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;
using UnityEngine;

namespace IREX.ReplaySystem.State
{
    public class ReplaySubframePlayer : IReplayFramePlayer
    {
        
        private IReplayWriter _replayWriter;

        public ReplaySubframePlayer(IReplayReader reader, int hz, ReplayMetaTimelineHeader theader, IReplayFramePlayer baseFrame) : base(reader, hz, theader, baseFrame)
        {
        }
        
        private void InstantiateOrDestroyGameObjectsAtDeltaReplay(Dictionary<int,string> idToNames)
        {
            var idToGameObject = ReplayManager.Instance.IdToGameObject;
            foreach (var idToName in idToNames)
            {                    
                if (!string.IsNullOrEmpty(idToName.Value))
                {
                    if (idToGameObject.ContainsKey(idToName.Key))
                        continue;
                    var go = GameObject.Instantiate(Resources.Load (idToName.Value) as GameObject, 
                        Vector3.zero, Quaternion.identity);
                    idToGameObject.Add(idToName.Key, go);
                    var newEntity = go.GetComponent<ReplayEntity>();
                    ReplayEntities.Add(newEntity);
                    Debug.Log($"{go.name} created with id {idToName.Key}");

                }
                else
                {

                    if (!idToGameObject.ContainsKey(idToName.Key))
                        continue;
                    
                    var go = idToGameObject[idToName.Key];
                    ReplayEntities.Remove(go.GetComponent<ReplayEntity>());
                    go.GetComponent<ReplayEntity>().IsDestroyed = true;
                    Debug.Log( $"{go.name} destroyed with id {go.GetComponent<ReplayEntity>().Id}");
                    GameObject.Destroy(idToGameObject[idToName.Key].gameObject);
                    idToGameObject.Remove(idToName.Key);
                }
            }
                
        }


        public override float ReadFrame(int currentFrame, int deltaFrames, float deltaTime)
        {
            var time = 0f;
            if(CurrentFrame+1 == currentFrame)
            {
                CurrentFrame = currentFrame;
                time = ReadFrameTime();
                ReadObjectsIdsAndNames();
                // Only construct once, because delta compression method unpacks n deltaFrameCount frames 
                ReadObservableEntitiesInit(deltaTime);
                ReadObservableEntities(true, deltaTime);
            }
            else
            {
                deltaTime = 1f / Hz;
                CurrentFrame = currentFrame;
                // Read BaseFrame
                THeader.SetPointerPosition(ReplayReader,currentFrame - deltaFrames);
                BaseFrame.ReadDeltaFrameCount();
                BaseFrame.ReadFrameTime();
                BaseFrame.ReadObjectsIdsAndNames();
                // Only construct once, because delta compression method unpacks n deltaFrameCount frames 
                BaseFrame.ReadObservableEntitiesInit(deltaTime);
                BaseFrame.ReadObservableEntities(false,deltaTime);
                
                THeader.SetPointerPosition(ReplayReader,currentFrame);
                ReadDeltaFrameCount();
                time = ReadFrameTime();
                ReadObjectsIdsAndNames();
                ReadObservableEntities(true, deltaTime);
            }

          
            return time;
        }

        public override void ReadObjectsIdsAndNames()
        {
            var entitiesCount = ReplayReader.ReadByte();
            var idToNames = new Dictionary<int, string>();
            for (var i = 0; i < entitiesCount; i++)
            {
                var id = ReplayReader.ReadInt32();
                var name = ReplayReader.ReadString();

                idToNames.Add(id,name);
            }
            InstantiateOrDestroyGameObjectsAtDeltaReplay(idToNames);
        }

        public override void ReadObservableEntitiesInit(float delta)
        {
            PlayAllReplayObservableConstruction(StaticEntities,ReplayStateMode.Delta, ReplayDeltaMode.SubFrame, delta);
            PlayAllReplayObservableConstruction(ReplayEntities, ReplayStateMode.Delta , ReplayDeltaMode.SubFrame ,delta);
        }

        public override void ReadObservableEntities(bool executeState, float delta)
        {
            PlayAllReplayObservable(StaticEntities, executeState, ReplayStateMode.Delta , ReplayDeltaMode.SubFrame, delta);
            PlayAllReplayObservable(ReplayEntities, executeState, ReplayStateMode.Delta , ReplayDeltaMode.SubFrame, delta);
        }



}

}