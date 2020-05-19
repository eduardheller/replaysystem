using System;
using System.Collections;
using System.IO;
using System.Linq;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.ReplayFrame;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IREX.ReplaySystem.Core
{
    public class ReplayRecorder 
    {

        private IReplayWriter _replayWriter;
        private readonly ReplayPointer _replayStartPositionsInStream;
        private IReplayFrameRecorder _replaySnapshotRecorder;
        private IReplayFrameRecorder _replayDeltaRecorder;

        private int _snapshotAtFrame;
        private int _tick = 1;
        private int _fromFrame;
        private int _hz;
        private MemoryStream _parentStream;
        public long SizeOfStream { get; private set; }
        public float RecordingTime { get; private set; }
        
        public float ReplayRate { get; private set; }


        private ReplayDeltaMode _replayDeltaMode;
        private int TimeLineNumber { get; set; }
        private int TimelineInheritance { get; set; }
        public delegate void RecordingStarted(ReplayRecorder replayRecorder);
    
        public static event RecordingStarted OnRecordStarted;
        public static event Action OnRecordStopped;

        public static event Action OnRecordFrameChanged;
        public ReplayRecorder()
        {
            _replayStartPositionsInStream = new ReplayPointer();
        }


        public IEnumerator Record(byte hz, byte snapshotAtFrame, 
            ReplayDeltaMode deltaMode, MemoryStream parentStream = null, 
            int newTimline = 0, int fromTimeLine = 255 , int fromFrame = 0, float recordingTime = 0)
        {
            if (deltaMode == ReplayDeltaMode.None)
                snapshotAtFrame = 1;
            ReplayRate = 1f / hz;
            _hz = hz;
            TimelineInheritance = fromTimeLine;
            _fromFrame = fromFrame;
            _snapshotAtFrame = snapshotAtFrame;
            _replayDeltaMode = deltaMode;
            
            if (newTimline == 0)
            {
                Init(new MemoryStream(),0);
            }
            else
            {
                Init(new MemoryStream(), recordingTime);
                _parentStream = parentStream;
            }
            TimeLineNumber = newTimline;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                WriteReplayDataToMemory();
            }
        }
    
        public System.IO.Stream StopRecording()
        {
            return GetStream();
        }

        public void OnStopRecording()
        {
            OnRecordStopped?.Invoke();
        }
    
    
        private void Init(System.IO.Stream stream, float recTime = 0)
        {
            _replayWriter = new ReplayWriteStream(stream);
            _replaySnapshotRecorder = new ReplaySnapshotRecorder(_replayWriter);
            _replayDeltaRecorder = ReplayDeltaFactory.CreateReplayDeltaRecorder(
                _replayWriter, 
                _replaySnapshotRecorder, 
                _replayDeltaMode);
     
            _replayStartPositionsInStream.Clear();
            RegisterEnitites();
            RecordingTime = recTime;
            OnRecordStarted?.Invoke(this);

            Debug.Log("ASDASDASA");
            _tick = 1;
        }
    

        private void RegisterEnitites()
        {
            var entities = GameObject.FindObjectsOfType<ReplayEntity>();

            var orderedEntities = entities.OrderBy(x => x.name).ToArray();
            var i = 1;
            foreach (var entity in orderedEntities)
            {
                if(entity.isSceneEntity)
                    entity.RegisterEntity(i++);
                else
                    entity.RegisterEntity();
            }
        }

        private void WriteReplayDataToMemory()
        {
            RecordingTime += Time.deltaTime;
            if (RecordingTime >= ReplayRate*_tick)
            {
                if (!_replayWriter.GetStream().CanWrite) return;
                // Add Streamposition of start of the frame to the keyFrameLists
                _replayStartPositionsInStream.Add((int)_replayWriter.GetStream().Position,0);
                
                var snapshotAtFrame = _snapshotAtFrame;
                var deltaFrames = ((_tick-1) % snapshotAtFrame);
                
                if (deltaFrames > 0)
                    _replayDeltaRecorder.WriteFrame(RecordingTime,_tick-1, (int)deltaFrames);
                else if (deltaFrames == 0)
                    _replaySnapshotRecorder.WriteFrame(RecordingTime,_tick-1, (int)deltaFrames);

                _tick++;
                OnRecordFrameChanged?.Invoke();
    
            }

            SizeOfStream = _replayWriter.GetStream().Length;
        }
    
        private MemoryStream GetStream()
        {
            var stream =  GetStreamWithHeader();
            return stream;
        }

        private MemoryStream GetStreamWithHeader()
        {
            MemoryStream ms;
            var streamWithNewHeader = new MemoryStream();
            var writer = new ReplayWriteStream(streamWithNewHeader);
            if(_parentStream == null) WriteReplayHeader(writer);
            WriteTimelineHeaderToStream(writer);
         
            // Write Header to the beginning of the file
            _replayWriter.GetStream().Position = 0;
            writer.GetBinaryWriter().Flush();
            streamWithNewHeader.Position = streamWithNewHeader.Length;
            _replayWriter.GetStream().CopyTo(streamWithNewHeader);
            
            streamWithNewHeader.Position = 0;
            
            if (_parentStream != null) ms = _parentStream;
            else ms = new MemoryStream();
            ms.Position = ms.Length;
            streamWithNewHeader.CopyTo(ms);
            // Offset 2 Bytes to change timelinecount in header
            ms.Position = 4;
            ms.WriteByte((byte)(TimeLineNumber+1));
            ms.Position = 0;
            ResetWriter();
            return ms;
        }

        private int WriteReplayHeader(IReplayWriter writer)
        {
            var header = new ReplayMetaHeader(
                1,
                (byte)SceneManager.GetActiveScene().buildIndex,
                (byte)_hz,
                _replayDeltaMode,
                    (byte)(TimeLineNumber+1)
            );
        
            return header.WriteFrame(writer);
        }

        private void WriteTimelineHeaderToStream(IReplayWriter writer)
        {
            _replayStartPositionsInStream.Add((int)_replayWriter.GetStream().Position,0);
            // add last byte position
            var timelineHeader = new ReplayMetaTimelineHeader(
                (byte)TimelineInheritance,
                _fromFrame,
                _replayStartPositionsInStream);
            
            timelineHeader.WriteFrame(writer);
        }

        private void ResetWriter()
        {
            ReplayManager.Instance.ReplayEntities.Clear();
            _replayWriter.GetStream().Flush();
            _replayWriter.GetStream().Close();
            RecordingTime = 0.0f;
        }
    
    
    }
}
