using System.Collections;
using System.Collections.Generic;
using IREX.ReplaySystem.Core;
using UnityEngine;

namespace IREX.ReplaySystem.Monobehaviours
{
    public class ReplayCameraHandler : MonoBehaviour
    {
        private List<Camera> _cameras;
        private int _activeCameraIndex;
        private Camera _mainCamera;
        private bool _hasStartedCoroutine;


        public void Awake()
        {
            _cameras = new List<Camera>();
        }


        public void OnEnable()
        {
            ReplayPlayer.OnReplayPlayerStarted += OnReplayPlayStarted;
            ReplayManager.OnWentLive += OnLive;
        }

        private void OnDisable()
        {
            ReplayPlayer.OnReplayPlayerStarted -= OnReplayPlayStarted;
            ReplayManager.OnWentLive -= OnLive;
        }

        public void AddCamera(Camera pCamera)
        {
            _cameras.Add(pCamera);
        }


        private void OnLive(List<ReplayEntity> replayEntities)
        {
            SetReplayCamera(false);
            _cameras.Clear();
        }

        private void OnReplayPlayStarted(ReplayPlayer player)
        {
            var cameras = GameObject.FindGameObjectsWithTag("ReplayCamera");
            foreach (var cam in cameras)
            {
                _cameras.Add(cam.GetComponent<Camera>());
            }
            SetNextActiveCamera();
        }
    
        public void GoToMainCamera()
        {
            if (ReplayManager.Instance.ReplayManagerMode == ReplayManager.ReplayMode.Playing)
            {
                if (_cameras.Count == 0)
                    return;

                SetReplayCamera(false);
            }
        }
    
        public void SetNextActiveCamera()
        {
            if (ReplayManager.Instance.ReplayManagerMode == ReplayManager.ReplayMode.Playing)
            {
                if (_cameras.Count == 0)
                    return;
            
                if (_cameras[_activeCameraIndex])
                    SetReplayCamera(false);

                if (_cameras.Count > _activeCameraIndex+1)
                    _activeCameraIndex++;
                else
                    _activeCameraIndex = 0;
            
                if(_cameras[_activeCameraIndex])
                    SetReplayCamera(true);
            
            }
        }

        public void SetPreviousActiveCamera()
        {
            if (ReplayManager.Instance.ReplayManagerMode == ReplayManager.ReplayMode.Playing)
            {
                if (_cameras.Count == 0)
                    return;
            
                SetReplayCamera(false);
            
                if (_activeCameraIndex > 0)
                    _activeCameraIndex--;
                else
                    _activeCameraIndex = _cameras.Count - 1;
            
                SetReplayCamera(true);
            }
        }


        private void SetReplayCamera(bool active)
        {
            if (_cameras.Count <= 0) return;
            _cameras[_activeCameraIndex].GetComponent<Camera>().enabled = active;
            _cameras[_activeCameraIndex].GetComponent<AudioListener>().enabled = active;
        }
    
    


    }
}
