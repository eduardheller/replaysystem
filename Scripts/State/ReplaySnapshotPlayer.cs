using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace IREX.ReplaySystem.State
{
    public class ReplaySnapshotPlayer : IReplayFramePlayer
    {
    
        public ReplaySnapshotPlayer(IReplayReader reader, int hz, ReplayMetaTimelineHeader theader) : base(reader, hz, theader, null)
        {
        }
    
        public override float ReadFrame(int currentFrame, int deltaFrames, float deltaTime)
        {
            if (Mathf.Abs(CurrentFrame -  currentFrame) != 1)
            {
                deltaTime = 1f / Hz;
            }
            CurrentFrame = currentFrame;
            var time = ReadFrameTime();
            ReadObjectsIdsAndNames();
            ReadObservableEntitiesInit(deltaTime);
            ReadObservableEntities(true, deltaTime);
            return time;
        }

        public override void ReadObjectsIdsAndNames()
        {
            var entitiesCount = ReplayReader.ReadInt32();
            var idToNames = new Dictionary<int, (string, Vector3, Quaternion)>();
            for (var i = 0; i < entitiesCount; i++)
            {
                var id = ReplayReader.ReadInt32();
                var name = ReplayReader.ReadString();
                var pos = ReplayReader.ReadVector3();
                var rot = ReplayReader.ReadQuaternion();
                idToNames.Add(id, (name,pos,rot));
            }
        
            InstantiateOrDestroyGameObjectsAtReplay(idToNames);
        }

        public override void ReadObservableEntitiesInit(float delta)
        {
            PlayAllReplayObservableConstruction(StaticEntities, ReplayStateMode.Snapshot, ReplayDeltaMode.None, delta);
            PlayAllReplayObservableConstruction(ReplayEntities, ReplayStateMode.Snapshot,ReplayDeltaMode.None,delta);
        }

        public override void ReadObservableEntities(bool executeState, float delta)
        {
            PlayAllReplayObservable(StaticEntities, executeState, ReplayStateMode.Snapshot, ReplayDeltaMode.None, delta);
            PlayAllReplayObservable(ReplayEntities, executeState, ReplayStateMode.Snapshot, ReplayDeltaMode.None, delta);
        }

        private void InstantiateOrDestroyGameObjectsAtReplay(Dictionary<int,(string, Vector3, Quaternion)> idToNames)
        {
            var idToGameObject = ReplayManager.Instance.IdToGameObject;
            var idToGameObjectWithoutStaticObjects = new Dictionary<int,GameObject>();

            foreach (var idGo in idToGameObject)
            {
                if(!idGo.Value.GetComponent<ReplayEntity>().isSceneEntity)
                    idToGameObjectWithoutStaticObjects.Add(idGo.Key,idGo.Value);
            }
            
            foreach (var idToName in idToNames)
            {
                if (idToGameObjectWithoutStaticObjects.ContainsKey(idToName.Key)) continue;
                
                var go = GameObject.Instantiate(Resources.Load (idToName.Value.Item1) as GameObject,
                    idToName.Value.Item2, idToName.Value.Item3);
                idToGameObjectWithoutStaticObjects.Add(idToName.Key, go);
                idToGameObject.Add(idToName.Key,go);

                ReplayEntities.Add(go.GetComponent<ReplayEntity>());

                //Debug.Log($"{go.name} created with id {idToName.Key}");
            }

            // Get all Ids which used to exist before this state
            var idToDestroyObjects = idToGameObjectWithoutStaticObjects.Keys.Except(idToNames.Keys);

            // Destroy all leftover ids
            foreach (var idToDestroy in idToDestroyObjects.ToList())
            {
                var go = idToGameObject[idToDestroy];
                ReplayEntities.Remove(go.GetComponent<ReplayEntity>());
                go.GetComponent<ReplayEntity>().IsDestroyed = true;
                //Debug.Log( $"{go.name} destroyed with id {go.GetComponent<ReplayEntity>().Id}");
                GameObject.Destroy(idToGameObject[idToDestroy].gameObject);
                idToGameObject.Remove(idToDestroy);
            }
        }



    }
}
