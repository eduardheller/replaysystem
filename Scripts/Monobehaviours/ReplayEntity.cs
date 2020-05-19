using System;
using System.Collections.Generic;
using System.Linq;
using IREX.ReplaySystem.Core;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using UnityEngine;
using UnityEngine.Assertions;

namespace IREX.ReplaySystem.Monobehaviours
{
    public class ReplayEntity : MonoBehaviour
    {
        public int Id { get; set; }
        public string GameObjectName { get; set; }

        public bool IsDestroyed { get; set; }

        public bool isSceneEntity;

        public Behaviour[] scriptsToDisable;
        public Behaviour[] scriptsToEnable;
        
        public Behaviour[] scriptsToDisableLive;
        public Behaviour[] scriptsToEnableLive;
        
        
        private List<IReplayObservable> _childEntities = new List<IReplayObservable>();
        public event System.Action OnReplayEntityInstantiated;
        public event System.Action OnReplayEntityDestroyed;

        private void Start()
        {
            if (ReplayManager.Instance.ReplayManagerMode == ReplayManager.ReplayMode.Recording)
            {
                RegisterEntity();
                OnReplayEntityInstantiated?.Invoke();
            }
            
            _childEntities = FindAllReplayObservables(transform);
            Assert.IsTrue(_childEntities.Count>0, "No ReplayObservables in " + name);
            //_childEntities = GetComponentsInChildren<IReplayObservable>();
        }


        void OnEnable()
        {
            ReplayPlayer.OnReplayPlayerStarted += OnReplayPlayStarted;
            ReplayManager.OnWentLive += OnWentLive;
        }

        
        void OnDisable()
        {
            ReplayPlayer.OnReplayPlayerStarted -= OnReplayPlayStarted;
            ReplayManager.OnWentLive -= OnWentLive;
        }


        void OnReplayPlayStarted(ReplayPlayer _replayPlayer)
        {
            foreach(var scriptToDisable in scriptsToDisable)
                scriptToDisable.enabled = false;

            foreach(var scriptToEnable in scriptsToEnable)
                scriptToEnable.enabled = true;
        }

        void OnWentLive(List<ReplayEntity> replayEntities)
        {
            foreach(var s in scriptsToDisableLive)
                s.enabled = false;

            foreach(var s in scriptsToEnableLive)
                s.enabled = true;
        }


        private static List<IReplayObservable> FindAllReplayObservables(Transform parent)
        {
            var replayObservables = new List<IReplayObservable>();
            
            var thisObserver = parent.GetComponents<IReplayObservable>();
    
            replayObservables.AddRange(thisObserver);
            
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                var entity = child.GetComponent<ReplayEntity>();
                if(entity)
                    continue;

                var otherReplay = FindAllReplayObservables(child);
                if(otherReplay.Count>0)
                    replayObservables.AddRange(otherReplay);
            }
      
            return replayObservables;
        }
        
        
        private void OnDestroy()
        {
            if (ReplayManager.Instance.ReplayManagerMode == ReplayManager.ReplayMode.Recording)
            {
                OnReplayEntityDestroyed?.Invoke();
                ReplayManager.Instance.ReplayEntities.Remove(GetComponent<ReplayEntity>());
            }

        }


        public void PlayAllReplayObservableConstruction(IReplayReader reader, ReplayInfo info)
        {
            foreach (var childEntity in _childEntities)
            {
                childEntity.OnReplayPlayInit(info);
            }
        }
        
        public void RecordAllReplayObservable(IReplayWriter writer, ReplayInfo info)
        {

            foreach (var childEntity in _childEntities)
            {

                if (Debug.isDebugBuild)
                {
                    // sizeof(int) weil die position um 4 nachruckt
                    writer.Write(childEntity.ToString());
                    writer.Write((int) writer.GetStream().Position + sizeof(int));
                }

                childEntity.OnReplayRecord(writer,info);

                if (Debug.isDebugBuild)
                {
                    writer.Write((int) writer.GetStream().Position + sizeof(int));
                }
            }

        }

        public void PlayAllReplayObservable(IReplayReader reader, ReplayInfo info)
        {
            
            foreach (var childEntity in _childEntities)
            {
                if (Debug.isDebugBuild)
                {
                    var name = reader.ReadString();
                    var ent = childEntity.ToString().Split('(')[0].Split(' ')[0];
                    name = name.Split('(')[0].Split(' ')[0];
                    Assert.AreEqual(name, ent, $"Expected: {name}, Actual {childEntity}, called by {name} at stage {info.ReplayStateMode}"); 
                
                    var recordedPos = reader.ReadInt32() + info.PositionOffset;
                    var playedPos = (int)reader.GetStream().Position;
                    Assert.AreEqual(recordedPos,playedPos, 
                        "REPLAY STREAM NOT IN SYNC!  at stage "+ info.ReplayStateMode + "  | "+childEntity + " :: " + childEntity.GetType() + " : " + 
                        " :: Expected Stream Position: " + (recordedPos) + " :: Actual Position: " + playedPos);

                }
                
                
                childEntity.OnReplayPlay(reader,info);
                
                
                if (Debug.isDebugBuild)
                {
                    var recordedPos = reader.ReadInt32() + info.PositionOffset;
                    var playedPos = (int) reader.GetStream().Position;
                    Assert.AreEqual(recordedPos, playedPos,
                        "REPLAY STREAM NOT IN SYNC!  at stage "+ info.ReplayStateMode + "  | "+ childEntity + " :: " + childEntity.GetType() + " : " +
                        " :: Expected Stream Position: " + (recordedPos) + " :: Actual Position: " + playedPos);
                }

            }
        }
    

        
        
        public void RegisterEntity(int i = 0)
        {
            if (IsDestroyed) return;
      
            // Namen des GameObjects auf den Prefabnamen ändern, in dem (Clone) nach dem Namen entfernt wird.
            name = name.Split('(')[0].Split(' ')[0];
            GameObjectName = name;
            Id = i == 0 ? GetInstanceID() : i;



            if (isSceneEntity)
            {
                if(!ReplayManager.Instance.ReplayStaticEntities.Contains(this))
                    ReplayManager.Instance.ReplayStaticEntities.Add(this);
            }
            else
            {
                if(!ReplayManager.Instance.ReplayEntities.Contains(this))
                    ReplayManager.Instance.ReplayEntities.Add(this);
            }
        }


    }
}
