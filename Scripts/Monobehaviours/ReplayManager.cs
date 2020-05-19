using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IREX.ReplaySystem.Core;
using IREX.ReplaySystem.File;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;

namespace IREX.ReplaySystem.Monobehaviours
{
    /// Use DontDestroyOnLoadScript at this or parent Gameobject to maintain replay functionality
    public class ReplayManager : MonoBehaviour
    {
        public static ReplayManager Instance { get; private set; }
        private MemoryStream _memoryStream;
        private ReplayRecorder _replayRecorder;
        private ReplayRecorder _replayRecorderDelta;
        private ReplayRecorder _replayRecorderSubFrame;
        private ReplayPlayer _replayPlayer;
        private ReplayFileHandler _replayFileHandler;
        private IEnumerator _recordCoroutine;

        private IEnumerator _playCoroutine;
        private static bool _replayPlayFlag;

        [Header("Replay Record Settings")]
        public byte version = 1;
        [Range(1,255)]
        public int hz = 60;

        [Header("Compression Settings")]
        public bool compressFloats = true;
        public float floatTolerance = 0.1f;
        
        [Header("Encoding Settings")]
        public ReplayDeltaMode replayDeltaMode;
        [Range(1,1024)]
        public uint snapshotAtFrame = 6;
        
        [Header("File Settings")]
        public string fileExtension = ".replay";
        public string directory = "Replays/";

        public List<ReplayEntity> ReplayEntities { get; set; }
        public Dictionary<int, GameObject> IdToGameObject { get; set; }
        public List<ReplayEntity> ReplayStaticEntities { get; set; }
        
        public int CurrentLevelIndex { get; private set; } = -1;
        
        public float PlaySpeed
        {
            get => _replayPlayer.SpeedFactor;
            set => _replayPlayer.SpeedFactor = value;
        }
        
        public delegate void WentLive(List<ReplayEntity> replayEntities);
        public static event WentLive OnWentLive;

        public static event System.Action OnReplayLoading;
        public static event System.Action OnReplayLoaded;

        public enum ReplayMode
        {
            Live,
            Recording,
            Playing
        }

        public ReplayMode ReplayManagerMode { get; set; }


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"{_replayPlayFlag} OnSceneLoaded");
            if (!_replayPlayFlag) return;
            Debug.Log("OnSceneLoaded2");
            StartReplay();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(this.gameObject);
                Instance = this;
                Init();
            }
        }

        private void Init()
        {
            _replayRecorder = new ReplayRecorder();
            _replayRecorderDelta = new ReplayRecorder();
            _replayRecorderSubFrame = new ReplayRecorder();
            _replayPlayer = new ReplayPlayer();
            _replayFileHandler = new ReplayFileHandler(directory,fileExtension);
            IdToGameObject = new Dictionary<int, GameObject>();
            ReplayEntities = new List<ReplayEntity>();
            ReplayStaticEntities = new List<ReplayEntity>();
            ReplayManagerMode = ReplayMode.Live;
        }
        
        private int GetReplayLevelFromStream(System.IO.Stream streamToRead)
        {
            var reader = new BinaryReader(streamToRead);
            reader.BaseStream.Position = 0;
            var replayHeader = new ReplayMetaHeader();
            replayHeader.ReadFrame(new ReplayReadStream(reader.BaseStream));
            return replayHeader.SceneIndex;
        }

        private void LoadReplayLevel(MemoryStream fileStream)
        {
            CurrentLevelIndex = GetReplayLevelFromStream(fileStream);
            SceneManager.LoadScene(CurrentLevelIndex);
        }

        private void RecordNewTimeline(string filename)
        {
            Debug.Log("Recording new Timeline...");

            StopPlaying();
            OnWentLive?.Invoke(ReplayEntities);
            StopRecording();


            _recordCoroutine = _replayRecorder.Record((byte)hz,(byte)snapshotAtFrame, replayDeltaMode,_memoryStream,
                _replayPlayer.ReplayMetaHeader.TimelineCount,
                _replayPlayer.CurrentReplayTimeline.TimelineIndex,
                _replayPlayer.CurrentReplayTimeline.Frame,
                _replayPlayer.CurrentReplayTimeline.CurrentFramePlayer.FrameTime);

            StartCoroutine(_recordCoroutine);
            ReplayManagerMode = ReplayMode.Recording;
            ReplayFileHandler.CurrentReplayFileName = filename;
        

        }


        private void ClearEntities()
        {
            IdToGameObject.Clear();
            ReplayEntities.Clear();
            ReplayStaticEntities.Clear();
        }
        
        private void StartReplay()
        {
            Debug.Log("Starting Replay");
            _replayPlayFlag = false;
            OnReplayLoaded?.Invoke();
            ReplayFileHandler.CurrentReplayFileName = 
                FileBrowserHelpers.GetFilename(Path.GetFileNameWithoutExtension(ReplayFileHandler.CurrentReplayFileName));

            _memoryStream = _replayFileHandler.GetReplayStreamFromFile(ReplayFileHandler.CurrentReplayFileName);
            ReplayManagerMode = ReplayMode.Playing;
            _playCoroutine = _replayPlayer.Play(_memoryStream);
            ClearEntities();
            StartCoroutine(_playCoroutine);
        }
        
        
        public void StartRecording(string filename)
        {
            switch (ReplayManagerMode)
            {
                case ReplayMode.Recording:
                    Debug.Log("Already Recording...");
                    break;
                case ReplayMode.Playing:
                    RecordNewTimeline(filename);
                    break;
                case ReplayMode.Live:
                    Debug.Log("Start Recording...");
                    ClearEntities();    
                    _memoryStream = new MemoryStream();
                    _recordCoroutine = _replayRecorder.Record((byte)hz, (byte)snapshotAtFrame, replayDeltaMode, _memoryStream);
                    ReplayManagerMode = ReplayMode.Recording;
                    ReplayFileHandler.CurrentReplayFileName = filename;
                    ClearEntities();
                    StartCoroutine(_recordCoroutine);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        
        public void StartPlaying(string filename)
        {

            var file = FileBrowserHelpers.GetFilename(Path.GetFileNameWithoutExtension(filename));

            if (!_replayFileHandler.ReplayExist(file)) 
                return;
            
            switch (ReplayManagerMode)
            {
                case ReplayMode.Recording:
                    _replayRecorder.StopRecording();
                    break;
                case ReplayMode.Live:
                    break;
                case ReplayMode.Playing:
                    StopPlaying();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var headerOfFile = _replayFileHandler.ReadReplayHeaderFromFile(file);
            ReplayFileHandler.CurrentReplayFileName = file;
            _replayPlayFlag = true;
            LoadReplayLevel(headerOfFile);
            OnReplayLoading?.Invoke();
        }
        
                
        public void GoLive()
        {
            if (ReplayManagerMode == ReplayMode.Live)
                return;
            
            StopRecording();
            StopPlaying();
            OnWentLive?.Invoke(ReplayEntities);
            ClearEntities();
            _memoryStream.Close();
            _memoryStream = null;
        }
        
        


        public void StopRecording()
        {
            if (ReplayManagerMode != ReplayMode.Recording) return;
            Debug.Log("Stop Recording.");
            var stream = _replayRecorder.StopRecording();
            ReplayFileHandler.CurrentReplayFileName = FileBrowserHelpers.GetFilename(Path.GetFileNameWithoutExtension(ReplayFileHandler.CurrentReplayFileName));
            _replayFileHandler.SaveReplayStreamToFile(ReplayFileHandler.CurrentReplayFileName,stream);
            StopCoroutine(_recordCoroutine);
            _replayRecorder.OnStopRecording();
            ReplayManagerMode = ReplayMode.Live;
        }
        
        public void StopPlaying()
        {
            if (ReplayManagerMode != ReplayMode.Playing) return;
            Debug.Log("Stop Playing.");
            StopCoroutine(_playCoroutine);
            ReplayManagerMode = ReplayMode.Live;
        }


        public void Pause()
        {
            if (ReplayManagerMode != ReplayMode.Playing)
                return;
            
            _replayPlayer.Pause();
        }
        
        public void Resume()
        {
            if (ReplayManagerMode != ReplayMode.Playing )
                return;
            
            _replayPlayer.Resume();
        }

        public bool IsPaused()
        {
            return _replayPlayer.IsPaused();
        }
        
        
    }


}
