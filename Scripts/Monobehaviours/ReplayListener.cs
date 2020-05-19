using System.Collections.Generic;
using UnityEngine;

namespace IREX.ReplaySystem.Monobehaviours
{
    public class ReplayListener : MonoBehaviour
    {
        
        private void OnEnable()
        {
            ReplayManager.OnWentLive += OnWentLive;
            ReplayManager.OnReplayLoading += OnReplayLoading;
            ReplayManager.OnReplayLoaded += OnReplayLoaded;
        }

        private void OnDisable()
        {
            ReplayManager.OnWentLive -= OnWentLive;
            ReplayManager.OnReplayLoading -= OnReplayLoading;
            ReplayManager.OnReplayLoaded -= OnReplayLoaded;
        }



        private void OnReplayLoaded()
        {
        }
        
        
        private void OnReplayLoading()
        {

        }
        
        private void OnWentLive(List<ReplayEntity> replayEntities)
        {

        }
    }
}
