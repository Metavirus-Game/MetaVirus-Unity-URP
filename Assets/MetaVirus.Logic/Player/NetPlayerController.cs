using System;
using System.Collections.Generic;
using GameEngine;
using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.FsmStates.NetPlayerFsm;
using UnityEngine;

namespace MetaVirus.Logic.Player
{
    public class NetPlayerController : MonoBehaviour
    {
        public struct WayPoint
        {
            public readonly Vector3 Position;
            public readonly Vector3 Rotation;

            public WayPoint(Vector3 position, Vector3 rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }

        private FsmEntity<NetPlayerController> _fsm;

        public float moveSpeed = 5;
        private GameObject _controlObject;

        public bool HasWayPoint => WayPoints.Count > 0;
        public List<WayPoint> WayPoints { get; } = new();

        public GameObject ControlObject
        {
            get => _controlObject;
            private set
            {
                _controlObject = value;
                _controlObject.transform.SetParent(transform, false);
                _controlObject.transform.localPosition = Vector3.zero;
            }
        }

        private GridItem _gridItem;

        public void SetControlData(GameObject controlObject, GridItem gridItemData)
        {
            ControlObject = controlObject;
            _gridItem = gridItemData;
            if (_fsm == null)
            {
                _fsm = GameFramework.GetService<FsmService>()
                    .CreateFsm("NetPlayerController-" + _gridItem.Type + "-" + _gridItem.ID, this,
                        new NetPlayerStateIdle(),
                        new NetPlayerStateWaiting(),
                        new NetPlayerStateMoving());
                _fsm.Start<NetPlayerStateIdle>();
            }
        }

        public void SetRotation(Vector3 euler)
        {
            if (_controlObject != null)
            {
                _controlObject.transform.eulerAngles = euler;
            }
        }

        public float CurrentSpeed { get; set; } = 0;

        private Animator _animator;

        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                }

                return _animator;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        private void OnDestroy()
        {
            GameFramework.GetService<FsmService>().DestroyFsm<NetPlayerController>(_fsm.Name);
        }

        public void AddWayPoint(Vector3 position, Vector3 rotation)
        {
            if (_controlObject == null || position == _controlObject.transform.position)
            {
                return;
            }

            WayPoints.Add(new WayPoint(position, rotation));
        }

        // Update is called once per frame
        void Update()
        {
            // if (Animator != null)
            // {
            //     Animator.SetFloat(Constants.AniParamName.Velocity, CurrentSpeed);
            // }
            //
            // if (WayPoints.Count > 0)
            // {
            //     var dest = WayPoints[0];
            //     var position = transform.position;
            //     if (position == dest.Position)
            //     {
            //         WayPoints.RemoveAt(0);
            //         if (WayPoints.Count == 0)
            //         {
            //             ControlObject.transform.eulerAngles = dest.Rotation;
            //             CurrentSpeed = 0;
            //             return;
            //         }
            //
            //         dest = WayPoints[0];
            //     }
            //
            //     CurrentSpeed = 1;
            //     var nextPosition = Vector3.MoveTowards(position, dest.Position,
            //         moveSpeed * Time.deltaTime);
            //     var dir = nextPosition - position;
            //     transform.position = nextPosition;
            //     dir.y = 0;
            //     ControlObject.transform.forward = dir;
            // }
            // else
            // {
            //     CurrentSpeed = 0;
            // }

            if (_controlObject != null)
                _controlObject.transform.localPosition = Vector3.zero;
        }
    }
}