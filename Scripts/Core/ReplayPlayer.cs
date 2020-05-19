using System;
using System.Collections;
using System.IO;
using System.Linq;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.Stream;
using IREX.ReplaySystem.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace IREX.ReplaySystem.Core
{
    public class ReplayPlayer 
    {
        private IReplayReader _replayReader;
        private System.IO.Stream _stream;
        private int _currentFramePosition;
        private bool _paused;
        private bool _hasEndOfReplayReached;
        private bool _hasStartOfReplayReached;
        private int _playRate;
        public float SpeedFactor { get; set; }

        public ReplayTimeline[] Timelines;
        public ReplayMetaHeader ReplayMetaHeader { get; private set; }

        public ReplayTimeline CurrentReplayTimeline { get; set; }

        public float CurrentProgress => CurrentReplayTimeline.PlayingTime / CurrentReplayTimeline.ReplayLength;
        
        
            
        public delegate void PlayingStarted(ReplayPlayer replayPlayer);
        public static event PlayingStarted OnReplayPlayerStarted;
    
        public static event Action OnEndOfReplayReached;
        public static event Action OnStartOfReplayReached;
    
        public static event Action OnPaused;
    
        public static event Action OnResumed;

        public static event Action OnPlayFrameChanged;

        private float _timer;
        private float _delta;


        public void Pause()
        {
            if (_paused) return;
            OnPaused?.Invoke();
            _paused = true;

        }

        public void Resume()
        {
            if (!_paused) return;
            OnResumed?.Invoke();
            _paused = false;

        }

        public bool IsPaused()
        {
            return _paused;
        }
    
        public IEnumerator Play(MemoryStream memoryStream)
        {
            Init(memoryStream);
            while (true)
            {
                yield return new WaitForEndOfFrame();
                ReadReplayDataFromStream();
            }
        }
    
        private void Init(MemoryStream streamToRead)
        {
            Assert.AreNotEqual(null,streamToRead);
            _replayReader = new ReplayReadStream(streamToRead);
            _paused = false;
            DestroyAllDynamicReplayEntities();
            InitializeTimelines();
            // Invoke after all timelines has been initialized
            OnReplayPlayerStarted?.Invoke(this);
            _timer = Time.realtimeSinceStartup;
        }
        
        private void InitializeTimelines()
        {
            _replayReader.GetStream().Position = 0;
            // First of all, the header must be read, to get the timelinecounts and meta informations
            ReplayMetaHeader = new ReplayMetaHeader();
            ReplayMetaHeader.ReadFrame(_replayReader);
            _playRate = ReplayMetaHeader.Hz;
            if (ReplayManager.Instance.version != ReplayMetaHeader.Version)
                Debug.LogWarning("Version of Replay not equal");
            
            // Timelines init and assign the timelines headers to them
            Timelines = new ReplayTimeline[ReplayMetaHeader.TimelineCount];
            var sizeOfPreviousContent = 0;


            for (var i = 0; i < Timelines.Length; i++)
            {
                var replayTimelineHeader = new ReplayMetaTimelineHeader();

                // Read Headerdata from Replay
                replayTimelineHeader.ReadFrame(_replayReader);
                // Read Snapshot/Delta Positions in Stream with Offset sizeOfPreviousContent
                replayTimelineHeader.ReadSnapshotPointers(_replayReader,ReplayMetaHeader.SizeInBytes + replayTimelineHeader.SizeInBytes + sizeOfPreviousContent);
 
                // Dont add Timelineparent to 255 byte number == no timeline 
                if(replayTimelineHeader.TimelineInheritance<255)
                    replayTimelineHeader.ReplayPointer.AddParentFrames(Timelines[replayTimelineHeader.TimelineInheritance],replayTimelineHeader.StartFromFrame);
                
                // Initialize Timelines
                Timelines[i] = new ReplayTimeline(ReplayMetaHeader,replayTimelineHeader, _replayReader, i);
                _replayReader.GetStream().Position = replayTimelineHeader.ReplayPointer.GetLastEndingPosition.Index;
                sizeOfPreviousContent = replayTimelineHeader.ReplayPointer.GetLastEndingPosition.Index-ReplayMetaHeader.SizeInBytes;
       
            }
            CurrentReplayTimeline = Timelines[0];
            CurrentReplayTimeline.CurrentFramePlayer = CurrentReplayTimeline.ReplaySnapshotPlayer;
            _replayReader.GetStream().Position = CurrentReplayTimeline.MetaTimelineHeader.ReplayPointer.GetFirstStartingPosition.Index;
        }
    
        private void ReadReplayDataFromStream()
        {
  

            // Main loop for playing the replay
            if (!_paused)
            {
                // sign determines if playback is goes forward = 1, or backwards =-1
                var playDirection = 1;
                var oldPlayingTime = CurrentReplayTimeline.PlayingTime;
                // Speed determined by Time.deltaTime * SpeedFactor + Time.deltaTime
                CurrentReplayTimeline.PlayingTime += (Time.deltaTime * SpeedFactor + Time.deltaTime);
                CurrentReplayTimeline.PlayingTime = Mathf.Clamp(CurrentReplayTimeline.PlayingTime, 0f, CurrentReplayTimeline.ReplayLength);
                if (CurrentReplayTimeline.PlayingTime < oldPlayingTime)
                    playDirection = -1;

                while (playDirection * CurrentReplayTimeline.PlayingTime >= playDirection * CurrentReplayTimeline.CurrentFramePlayer.FrameTime)
                {
                    var prevFrameTime = CurrentReplayTimeline.CurrentFramePlayer.FrameTime;
                    if (CurrentReplayTimeline.Frame == CurrentReplayTimeline.MetaTimelineHeader.ReplayPointer.GetLastStartingPositionIndex+1) break;
                    ReadState(CurrentReplayTimeline.Frame, _delta);
                    var afterFramTime = CurrentReplayTimeline.CurrentFramePlayer.FrameTime;
                    _delta = Mathf.Abs(afterFramTime - prevFrameTime);
       
                    CurrentReplayTimeline.Frame += playDirection;
                    CurrentReplayTimeline.Frame = Mathf.Clamp(CurrentReplayTimeline.Frame, 
                        0, CurrentReplayTimeline.MetaTimelineHeader.ReplayPointer.GetLastStartingPositionIndex+1);
                    if (CurrentReplayTimeline.Frame == CurrentReplayTimeline.MetaTimelineHeader.ReplayPointer
                            .GetLastStartingPositionIndex)
                    {
                        CurrentReplayTimeline.PlayingTime = CurrentReplayTimeline.ReplayLength;
                    }
                    OnPlayFrameChanged?.Invoke();
                    if (CurrentReplayTimeline.Frame == 0) break;
                }
            }
            else
            {
                _timer = Time.realtimeSinceStartup;
                CurrentReplayTimeline.Frame = Mathf.Clamp(CurrentReplayTimeline.Frame, 
                    0, CurrentReplayTimeline.MetaTimelineHeader.ReplayPointer.GetLastStartingPositionIndex);
                ReadState(CurrentReplayTimeline.Frame, 1f/_playRate); 
                OnPlayFrameChanged?.Invoke();
            }

            // Check Ranges and Invoke Events for Listeners
            CheckStreamRange();
        }


        private void CheckStreamRange()
        {
            if(StreamIsOnEndOfReplay())
            {
                if(!_hasEndOfReplayReached) 
                    OnEndOfReplayReached?.Invoke();
            
                _hasEndOfReplayReached = true;
            }
            else
            {
                _hasEndOfReplayReached = false;
            }
                    
            if(StreamIsOnStartOfReplay())
            {
                if(!_hasStartOfReplayReached) 
                    OnStartOfReplayReached?.Invoke();
                        
                _hasStartOfReplayReached = true;
            }
            else
            {
                _hasStartOfReplayReached = false;
            }
        
        }
        private void ReadState(int index, float deltaTime)
        {
            if (CurrentReplayTimeline.MetaTimelineHeader.SetPointerPosition(_replayReader,index))
            {
                var deltaFrames = CurrentReplayTimeline.CurrentFramePlayer.ReadDeltaFrameCount();
                if (deltaFrames > 0)
                    CurrentReplayTimeline.CurrentFramePlayer  = CurrentReplayTimeline.ReplayDeltaPlayer;
                else if (deltaFrames == 0)
                    CurrentReplayTimeline.CurrentFramePlayer  =  CurrentReplayTimeline.ReplaySnapshotPlayer;
                
                CurrentReplayTimeline.CurrentFramePlayer.ReadFrame(index,deltaFrames, deltaTime);
            }
        }
    
    
        private bool StreamIsOnStartOfReplay()
        {
            return CurrentReplayTimeline.Frame <= 0;
        }

        private bool StreamIsOnEndOfReplay()
        {
            return CurrentReplayTimeline.Frame >= CurrentReplayTimeline.MetaTimelineHeader.ReplayPointer.GetLastStartingPositionIndex;
        }
    
    
   

        private void DestroyAllDynamicReplayEntities()
        {
            var entities = Object.FindObjectsOfType<ReplayEntity>();

            // Sort entities ordered by name
            var orderedEntities = entities.OrderBy(x => x.name).ToArray();
            var i = 1;
            foreach (var entity in orderedEntities)
            {
                if (entity.isSceneEntity)
                {
                    entity.RegisterEntity(i++);
                    ReplayManager.Instance.IdToGameObject.Add(entity.Id, entity.gameObject);
                    continue;
                }
                if(entity.IsDestroyed)
                    continue;
                GameObject.Destroy(entity.gameObject);
            }
        }
    }
}
