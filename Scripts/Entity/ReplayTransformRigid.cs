using System;
using System.Collections;
using System.Collections.Generic;
using IREX.ReplaySystem;
using IREX.ReplaySystem.Monobehaviours;
using IREX.ReplaySystem.State;
using IREX.ReplaySystem.Stream;
using UnityEngine;

public class ReplayTransformRigid : MonoBehaviour, IReplayObservable
{

    private Transform _thisTransform;
    private Rigidbody _rigidbody;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    
    private Vector3 _targetStartPosition;
    private Quaternion _targetStartRotation;
    
    private Vector3 _targetVelocity;
    private Vector3 _targetAngularVelocity;
    private bool _hasInitiated;

    public bool trackRigid;
    private float _timer;
    private float _timerDiff;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _thisTransform = transform;
        _targetPosition = transform.position;
        _targetRotation = transform.rotation;
    }
    

    private void Update()
    {
        if (ReplayManager.Instance.ReplayManagerMode != ReplayManager.ReplayMode.Playing) return;
        _timer += Time.deltaTime;
 
        var t = _timer / _timerDiff;
        transform.position = Vector3.Lerp(_targetStartPosition, _targetPosition, t);
        transform.rotation = Quaternion.Slerp(_targetStartRotation, _targetRotation, t);

        if (_rigidbody && trackRigid)
        {
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, _targetVelocity, Time.deltaTime * 15f);
            _rigidbody.angularVelocity = Vector3.Lerp(_rigidbody.angularVelocity, _targetAngularVelocity, Time.deltaTime * 15f);
        }
        
    }
    
    enum TransformFlags : Byte
    {
        None = 0,
        Transform = 1,
        Rotation = 2,
        Velocity = 4,
        AngularVelocity = 8,
        NotEnabledTransformation = 16
            
    }

    private TransformFlags _transformFlags;
    private Vector3 _snapshotPosition;
    private Quaternion _snapshotRotation;
    private Vector3 _snapshotVelocity;
    private Vector3 _snapshotAngularVelocity;
    
    public void OnReplayRecord(IReplayWriter stream, ReplayInfo info)
    {
        _transformFlags = TransformFlags.None;
        if (info.ReplayStateMode == ReplayStateMode.Snapshot)
        {
            _snapshotPosition = transform.position;
            _snapshotRotation = transform.rotation;
            
            stream.Write(_snapshotPosition);
            stream.Write(_snapshotRotation);

            if (_rigidbody && trackRigid)
            {
                _snapshotVelocity = _rigidbody.velocity;
                _snapshotAngularVelocity = _rigidbody.angularVelocity;
                stream.Write(_snapshotVelocity);
                stream.Write(_snapshotAngularVelocity);
            }

        }
        else if (info.ReplayStateMode == ReplayStateMode.Delta)
        {
            if (!ReplayMath.IsApproximatelyEqual(transform.position, _snapshotPosition))
                _transformFlags |= TransformFlags.Transform;

            if (!ReplayMath.IsApproximatelyEqual(transform.rotation, _snapshotRotation))
                _transformFlags |= TransformFlags.Rotation;

            if (_rigidbody && trackRigid)
            {
                if (!ReplayMath.IsApproximatelyEqual(_rigidbody.velocity, _snapshotVelocity))
                    _transformFlags |= TransformFlags.Velocity;

                if (!ReplayMath.IsApproximatelyEqual(_rigidbody.angularVelocity, _snapshotAngularVelocity))
                    _transformFlags |= TransformFlags.AngularVelocity;
            }

            stream.Write((byte) _transformFlags);
            if (_transformFlags.HasFlag(TransformFlags.Transform))
            {
                if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                {
                    stream.Write(transform.position - _snapshotPosition);
                    _snapshotPosition = transform.position;
                }
                else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                {
                    stream.Write(transform.position);
                }
            }

            if (_transformFlags.HasFlag(TransformFlags.Rotation))
            {
                if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                {
                    stream.Write(transform.rotation * Quaternion.Inverse(_snapshotRotation));
                    _snapshotRotation = transform.rotation;
                }
                else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                {
                    stream.Write(transform.rotation);
                }
            }

            if (_transformFlags.HasFlag(TransformFlags.Velocity))
            {
                if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                {
                    stream.Write(_rigidbody.velocity - _snapshotVelocity);
                    _snapshotVelocity = _rigidbody.velocity;
                }
                else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                {
                    stream.Write(_rigidbody.velocity);
                }
            }

            if (_transformFlags.HasFlag(TransformFlags.AngularVelocity))
            {
                if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                {
                    stream.Write(_rigidbody.angularVelocity - _snapshotAngularVelocity);
                    _snapshotAngularVelocity = _rigidbody.angularVelocity;
                }
                else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                {
                    stream.Write(_rigidbody.angularVelocity);
                }
            }
        }
    }

        public void OnReplayPlayInit(ReplayInfo info)
        {

            _targetStartPosition = _targetPosition;
            _targetStartRotation = _targetRotation;
            _timer = 0f;
            _timerDiff = info.Delta;

        }

  
        public void OnReplayPlay(IReplayReader stream, ReplayInfo info)
        {
     
            _transformFlags = TransformFlags.None;

            if (info.ReplayStateMode == ReplayStateMode.Snapshot)
            {
                
                _targetPosition = stream.ReadVector3(); 
                _targetRotation = stream.ReadQuaternion();
            
            
                if (_rigidbody && trackRigid)
                {
                    _targetVelocity = stream.ReadVector3(); 
                    _targetAngularVelocity = stream.ReadVector3();
                }
            }
            else if (info.ReplayStateMode == ReplayStateMode.Delta)
            {
                
                
                var transformFlags = (TransformFlags)stream.ReadByte();
                

                if (transformFlags.HasFlag(TransformFlags.Transform))
                {
                    if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                    {
                        var deltaPos = stream.ReadVector3();
                        _targetPosition += deltaPos;
                    }
                    else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                    {
                        var pos = stream.ReadVector3();
                        _targetPosition = pos;
                    }
                }

                if (transformFlags.HasFlag(TransformFlags.Rotation))
                {
                    if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                    {
                        var deltaRot = stream.ReadQuaternion();
                        _targetRotation = deltaRot * _targetRotation;
                    }
                    else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                    {
                        var rot = stream.ReadQuaternion();
                        _targetRotation = rot;
                    }
                }
                
                if (_rigidbody && trackRigid)
                {
                    
                    if (transformFlags.HasFlag(TransformFlags.Velocity))
                    {
                        if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                        {
                            var deltaVel = stream.ReadVector3();
                            _targetVelocity += deltaVel;
                        }
                        else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                        {
                            var vel = stream.ReadVector3();
                            _targetVelocity = vel;
                        }
                    }

                    if (transformFlags.HasFlag(TransformFlags.AngularVelocity))
                    {
                        if (info.ReplayDeltaMode == ReplayDeltaMode.DeltaCompression)
                        {
                            var deltaAng = stream.ReadVector3();
                            _targetAngularVelocity += deltaAng;
                        }
                        else if (info.ReplayDeltaMode == ReplayDeltaMode.SubFrame)
                        {
                            var velAng = stream.ReadVector3();
                            _targetAngularVelocity = velAng;
                        }
                    }
                }
            }

        }
}
