using System;
using System.Collections.Generic;
using IREX.ReplaySystem.Core;
using IREX.ReplaySystem.File;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.Timeline;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IREX.ReplaySystem.UI
{
    public class ReplayUI : MonoBehaviour
    {

        public GameObject timeLinePanel;
        public Slider speedSlider;
        public TMPro.TextMeshProUGUI speedText;
        public GameObject timeLine;
        public GameObject camBackground;
        public GameObject status;
        public TMPro.TextMeshProUGUI fileStatus;

 
    
        public Image statusIconRecord;
        public Image statusIconPlay;
        public Image statusIconPause;
        public TMPro.TextMeshProUGUI statusText;
    
        public Button liveBtn;
        public Button playBtn;
        public Button pauseBtn;
        public Button stopBtn;
        public Button recordBtn;
        public Button backBtn;
        public Button forwardBtn;
    
        private List<ReplayTimelineUI> _replayTimelineUis;
        private ReplayPlayer _replayPlayer;
        private ReplayRecorder _replayRecorder;

        private ReplayTimeline _currentTimeline;
    
        public void OnEnable()
        {
            //timelineSlider.onValueChanged.AddListener(delegate { TimelineCallback(); });
            speedSlider.onValueChanged.AddListener(delegate { SpeedChangeCallback(); });
            ReplayPlayer.OnReplayPlayerStarted += OnReplayPlayStarted;
            ReplayPlayer.OnEndOfReplayReached += OnEndOfReplayReached;
            ReplayPlayer.OnStartOfReplayReached += OnStartOfReplayReached;
            ReplayPlayer.OnPaused += OnPaused;
            ReplayPlayer.OnResumed += OnResumed;
            ReplayPlayer.OnPlayFrameChanged += OnPlayFrameChanged;
            ReplayRecorder.OnRecordStarted += OnRecordStarted;
            ReplayRecorder.OnRecordStopped += OnRecordStopped;
            ReplayRecorder.OnRecordFrameChanged += OnRecordFrameChanged;
            ReplayManager.OnWentLive += OnWentLive;

            timeLinePanel.SetActive(false);
            status.SetActive(false);
            camBackground.SetActive(false);
            playBtn.interactable = false;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(true);
            backBtn.interactable = false;
            liveBtn.interactable = false;
            forwardBtn.interactable = false;
            speedText.text = $"Speed {speedSlider.value}x";
            _replayTimelineUis = new List<ReplayTimelineUI>();

        }

        private void OnDisable()
        {
            speedSlider.onValueChanged.RemoveListener(delegate { SpeedChangeCallback(); });
            ReplayPlayer.OnReplayPlayerStarted -= OnReplayPlayStarted;
            ReplayPlayer.OnEndOfReplayReached -= OnEndOfReplayReached;
            ReplayPlayer.OnStartOfReplayReached -= OnStartOfReplayReached;
            ReplayPlayer.OnPaused -= OnPaused;
            ReplayPlayer.OnResumed -= OnResumed;
            ReplayPlayer.OnPlayFrameChanged -= OnPlayFrameChanged;
            ReplayRecorder.OnRecordStarted -= OnRecordStarted;
            ReplayRecorder.OnRecordStopped -= OnRecordStopped;
            ReplayRecorder.OnRecordFrameChanged -= OnRecordFrameChanged;
            ReplayManager.OnWentLive -= OnWentLive;
        }


        private void Start()
        {
            // Set filters (optional)
            // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
            // if all the dialogs will be using the same filters
            FileBrowser.SetFilters( true, new FileBrowser.Filter( "Replayfiles", ".replay" ) );
            // Set default filter that is selected when the dialog is shown (optional)
            // Returns true if the default filter is set successfully
            // In this case, set Images filter as the default filter
            FileBrowser.SetDefaultFilter( ".replay" );

        }
    

    
        private void OnEndOfReplayReached()
        {
            Pause();
            Debug.Log("OnEndOfReplayReached");
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            _replayPlayer.CurrentReplayTimeline.Frame = _replayPlayer.CurrentReplayTimeline.FrameSize;
            _replayPlayer.CurrentReplayTimeline.PlayingTime = _replayPlayer.CurrentReplayTimeline.ReplayLength;
            var percentage = _replayPlayer.CurrentReplayTimeline.PlayingTime / _replayPlayer.CurrentReplayTimeline.ReplayLength;
            _replayTimelineUis[id].timelineSlider.value = percentage;
            playBtn.interactable = false;
            pauseBtn.interactable = true;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(true);
            backBtn.interactable = true;
            forwardBtn.interactable = false;
            statusIconRecord.gameObject.SetActive(false);
            statusIconPlay.gameObject.SetActive(false);
            statusIconPause.gameObject.SetActive(true);
            liveBtn.interactable = true;

        }
    
        private void OnStartOfReplayReached()
        {
            Debug.Log("OnStartOfReplayReached");
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            _replayPlayer.CurrentReplayTimeline.Frame = 0;
            _replayPlayer.CurrentReplayTimeline.PlayingTime = 0;
            _replayTimelineUis[id].timelineSlider.value = 0;
            playBtn.interactable = true;
            pauseBtn.interactable = true;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(false);
            backBtn.interactable = false;
            forwardBtn.interactable = true;
            statusIconRecord.gameObject.SetActive(false);
            statusIconPlay.gameObject.SetActive(false);
            statusIconPause.gameObject.SetActive(true);
            liveBtn.interactable = true;
        }
    
        private void OnPaused()
        {
            Debug.Log("OnPaused");
            playBtn.interactable = true;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(true);
            statusIconRecord.gameObject.SetActive(false);
            statusIconPlay.gameObject.SetActive(false);
            statusIconPause.gameObject.SetActive(true);
            liveBtn.interactable = true;
            statusText.text = "Replay Paused...";
        }

        private void OnResumed()
        {
            Debug.Log("OnResumed");
            playBtn.interactable = false;
            pauseBtn.interactable = true;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(false);
            backBtn.interactable = true;
            forwardBtn.interactable = true;
            liveBtn.interactable = false;
            statusText.text = "Playing Replay...";
            statusIconRecord.gameObject.SetActive(false);
            statusIconPlay.gameObject.SetActive(true);
            statusIconPause.gameObject.SetActive(false);
        }

        private void OnRecordStarted(ReplayRecorder replayRecorder)
        {
            Debug.Log("OnRecordStarted");
            _replayRecorder = replayRecorder;
            playBtn.interactable = false;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(true);
            recordBtn.gameObject.SetActive(false);
            backBtn.interactable = false;
            forwardBtn.interactable = false;
            liveBtn.interactable = false;
            status.SetActive(true);
            statusIconRecord.gameObject.SetActive(true);
            statusIconPlay.gameObject.SetActive(false);
            statusIconPause.gameObject.SetActive(false);
            fileStatus.text = $"Record File to: {ReplayFileHandler.CurrentReplayFileName} with Compressionmethod: {ReplayManager.Instance.replayDeltaMode} at every {ReplayManager.Instance.snapshotAtFrame} Frame.";
        }
    
        private void OnRecordStopped()
        {
            Debug.Log("OnRecordStopped");
            playBtn.interactable = true;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(true);
            backBtn.interactable = false;
            forwardBtn.interactable = false;
            timeLinePanel.SetActive(false);
            camBackground.SetActive(false);
            liveBtn.interactable = false;
            status.SetActive(false);
            statusText.text = "Recording stopped and saved to file...";
            fileStatus.text = $"Record File saved: {ReplayFileHandler.CurrentReplayFileName}";
        }

        public void OnReplayPlayStarted(ReplayPlayer replayPlayer)
        {
            Debug.Log("OnReplayPlayStarted");
            _replayPlayer = replayPlayer;
            status.SetActive(true);
            statusIconRecord.gameObject.SetActive(false);
            statusIconPlay.gameObject.SetActive(true);
            statusIconPause.gameObject.SetActive(false);
            timeLinePanel.SetActive(true);
            camBackground.SetActive(true);
            statusText.text = "Playing Replay...";
            playBtn.interactable = false;
            pauseBtn.interactable = true;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(false);
            backBtn.interactable = true;
            forwardBtn.interactable = true;
            
            foreach (var timeline in _replayTimelineUis)
            {
                Destroy(timeline.gameObject);
            }
        
            _replayTimelineUis.Clear();

            for (var i = 0; i < _replayPlayer.ReplayMetaHeader.TimelineCount; i++)
            {
                _replayTimelineUis.Add(Instantiate(timeLine, timeLinePanel.transform).GetComponent<ReplayTimelineUI>());
                _replayTimelineUis[_replayTimelineUis.Count - 1].toggle.group = timeLinePanel.GetComponent<ToggleGroup>();
                _replayTimelineUis[_replayTimelineUis.Count - 1].timelineText.text =                 
                    $"{0} sec / {GetCorrectedTimeValueString(_replayPlayer.Timelines[i].ReplayLength)} sec";
                _replayTimelineUis[_replayTimelineUis.Count - 1].timelineSlider.interactable = false;
            }
        
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            _replayTimelineUis[id].timelineSlider.onValueChanged.AddListener(delegate { TimelineCallback(); });
            _replayTimelineUis[id].toggle.isOn = true;
            _replayTimelineUis[id].timelineSlider.interactable = true;
            fileStatus.text = $"Playing Replay with Compressionmethod: {_replayPlayer.CurrentReplayTimeline.deltaMode}";
        
            for (var i = 0; i < _replayPlayer.ReplayMetaHeader.TimelineCount; i++)
            {
                var newId = i;
                _replayTimelineUis[i].toggle.onValueChanged.AddListener((on) => {
                    if (on)
                    {
                        _replayTimelineUis[id].timelineSlider.onValueChanged.RemoveAllListeners();
                        _replayTimelineUis[id].timelineSlider.interactable = false;
                        _replayPlayer.Pause();
                        _replayPlayer.CurrentReplayTimeline = _replayPlayer.Timelines[newId];
                        _replayTimelineUis[newId].timelineSlider.interactable = true;
                        _replayTimelineUis[newId].timelineSlider.onValueChanged.AddListener(delegate { TimelineCallback(); });
                    }
                });
            }
        }
    
    
        private void OnWentLive(List<ReplayEntity> replayEntities)
        {
            Debug.Log("OnWentLive");
            timeLinePanel.SetActive(false);
            status.SetActive(false);
            camBackground.SetActive(false);
            playBtn.interactable = true;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            recordBtn.gameObject.SetActive(true);
            backBtn.interactable = false;
            liveBtn.interactable = false;
            forwardBtn.interactable = false;
            fileStatus.text = "Currently Live, no Replay";
        }
    
    
        public void OnPlayFrameChanged()
        {
            if (!ReplayManager.Instance.IsPaused())
            {
                var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
                _replayTimelineUis[id].timelineText.text =                 
                    $"{GetCorrectedTimeValueString(_replayPlayer.CurrentReplayTimeline.PlayingTime)} sec / {GetCorrectedTimeValueString(_replayPlayer.CurrentReplayTimeline.ReplayLength)} sec";
                var percentage = _replayPlayer.CurrentReplayTimeline.PlayingTime / _replayPlayer.CurrentReplayTimeline.ReplayLength;
                _replayTimelineUis[id].timelineSlider.value = percentage;
            }
        }

        public void OnRecordFrameChanged()
        {
            statusText.text =
                $"Recording: {_replayRecorder.RecordingTime:F1} sec, with {((_replayRecorder.SizeOfStream / 1024f)):F2} KBytes at {1f/_replayRecorder.ReplayRate} Hz";
        
        }
    
    
        private void TimelineCallback()
        {
            if (ReplayManager.Instance.IsPaused())
            {
                PlayOneFrame();

            }
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            backBtn.interactable = !(_replayTimelineUis[id].timelineSlider.value <= 0);
            forwardBtn.interactable = !(_replayTimelineUis[id].timelineSlider.value >= 1);
      
        }


        private void PlayOneFrame()
        {
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            _replayPlayer.CurrentReplayTimeline.PlayingTime = _replayTimelineUis[id].timelineSlider.value * _replayPlayer.CurrentReplayTimeline.ReplayLength;
            var progress = _replayPlayer.CurrentProgress;
            _replayPlayer.CurrentReplayTimeline.Frame =  (int) (progress * (_replayPlayer.CurrentReplayTimeline.FrameSize));

            _replayTimelineUis[id].timelineText.text = 
                $"{GetCorrectedTimeValueString(_replayPlayer.CurrentReplayTimeline.PlayingTime)} sec / {GetCorrectedTimeValueString(_replayPlayer.CurrentReplayTimeline.ReplayLength)} sec";
            playBtn.interactable = true;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            backBtn.interactable = true;
            forwardBtn.interactable = true;
  
            if(_replayPlayer.CurrentReplayTimeline.Frame!=0)
                recordBtn.gameObject.SetActive(true);
        }
    
        private void SpeedChangeCallback()
        {
            var spdSliderValue = speedSlider.value;
            //ReplayManager.Instance.Resume();
            _replayPlayer.SpeedFactor = spdSliderValue;
            speedText.text = "Speed " + (spdSliderValue+1) + "x";
        }


        public void RecordReplay()
        {
            ReplayManager.Instance.StartRecording(
                SceneManager.GetActiveScene().name + "_" + 
                DateTime.Now.ToString("MM_dd_yyyy_h_mm_ss"));
        }


        public void ShowLoadDialog()
        {
            FileBrowser.ShowLoadDialog(
                (path) =>
                {
                    Debug.Log(FileBrowser.Result);
                    ReplayFileHandler.CurrentReplayFileName = FileBrowser.Result; 
                    StartPlaying(); 
                },
                () => { }, 
                false, Application.dataPath + "/Replays","Load Replay File", "Load");
        }

    
    
        public void StartPlaying()
        {
            ReplayManager.Instance.StartPlaying(ReplayFileHandler.CurrentReplayFileName);
            Resume();
        }
    
        public void GoLive()
        {
            ReplayManager.Instance.GoLive();
        }

    
        public void Pause()
        {

            ReplayManager.Instance.Pause();

        }

        public void Resume()
        {
            if (ReplayManager.Instance.ReplayManagerMode != ReplayManager.ReplayMode.Playing)
            {
                ReplayManager.Instance.StartPlaying(ReplayFileHandler.CurrentReplayFileName);
            }
            ReplayManager.Instance.Resume();

        }

        public void Stop()
        {
            ReplayManager.Instance.StopRecording();
        }
    
        public void ForwardToEnd()
        {

            Pause();
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            _replayPlayer.CurrentReplayTimeline.Frame = _replayPlayer.CurrentReplayTimeline.FrameSize;
            _replayPlayer.CurrentReplayTimeline.PlayingTime = _replayPlayer.CurrentReplayTimeline.ReplayLength;
            var percentage = _replayPlayer.CurrentReplayTimeline.PlayingTime / _replayPlayer.CurrentReplayTimeline.ReplayLength;
            _replayTimelineUis[id].timelineSlider.value = percentage;
            playBtn.interactable = false;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            backBtn.interactable = true;
            liveBtn.interactable = true;
            forwardBtn.interactable = false;
        }

        public void BackToStart()
        {
            Pause();
            var id = _replayPlayer.CurrentReplayTimeline.TimelineIndex;
            _replayPlayer.CurrentReplayTimeline.Frame = 0;
            _replayPlayer.CurrentReplayTimeline.PlayingTime = 0;
            var percentage = _replayPlayer.CurrentReplayTimeline.PlayingTime / _replayPlayer.CurrentReplayTimeline.ReplayLength;
            _replayTimelineUis[id].timelineSlider.value = percentage;
            playBtn.interactable = true;
            pauseBtn.interactable = false;
            stopBtn.gameObject.SetActive(false);
            backBtn.interactable = false;
            liveBtn.interactable = true;
            forwardBtn.interactable = true;

        }
    
        public string GetCorrectedTimeValueString(float timeValue)
        {
            int minutes = (int)(timeValue / 60);
            int seconds = (int)(timeValue % 60);
            return $"{minutes}:{seconds:00}";
        }
    
    }
}
