using System;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private EventService _eventService;

        private CharacterController _controller;
        [SerializeField] private float speed = 1;
        [SerializeField] private Animator playerAni;
        [SerializeField] private GameObject playerObject;

        //public JoystickController joystickController;

        private Vector3 _move;

        // Start is called before the first frame update
        void Start()
        {
            _eventService = GameFramework.GetService<EventService>();
            _controller = GetComponent<CharacterController>();

            _eventService.On<JoystickEvent>(GameEvents.ControllerEvent.JoystickEvent, OnJoystickEvent);
            // joystickController.OnJoystickMoved += distance =>
            // {
            //     if (distance != Vector3.zero)
            //     {
            //         distance.Normalize();
            //
            //         _move.x = distance.x;
            //         _move.z = -distance.y;
            //         _move.y = 0;
            //     }
            // };
            //
            // joystickController.OnJoystickStopped += () => { _move = Vector3.zero; };
        }

        private void OnJoystickEvent(JoystickEvent evt)
        {
            if (evt.Type == JoystickEvent.JoystickEventType.Moving)
            {
                var distance = evt.Postion;
                if (distance != Vector3.zero)
                {
                    distance.Normalize();

                    _move.x = distance.x;
                    _move.z = -distance.y;
                    _move.y = 0;
                }
            }
            else
            {
                _move = Vector3.zero;
            }
        }

        private void OnDestroy()
        {
            _eventService.Remove<JoystickEvent>(GameEvents.ControllerEvent.JoystickEvent, OnJoystickEvent);
        }

        private void OnDisable()
        {
            if (playerAni == null || playerObject == null)
            {
                return;
            }

            playerAni.SetInteger(AniParamName.State, NpcAniState.Idle);
            _move = Vector3.zero;
        }

        public GameObject PlayerObject
        {
            get => playerObject;
            set
            {
                playerObject = value;
                value.SetActive(true);
                value.transform.SetParent(transform, false);
                playerAni = value.GetComponentInChildren<Animator>();
                value.transform.localPosition = Vector3.zero;
                value.transform.localRotation = Quaternion.identity;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (playerAni == null || playerObject == null)
            {
                return;
            }

            if (_move != Vector3.zero)
            {
                _controller.SimpleMove(_move * speed);
                var velocity = _controller.velocity.magnitude;
                playerAni.SetInteger(AniParamName.State, NpcAniState.Walk);
                playerAni.speed = PlayerWalkAniSpeed;
                playerObject.transform.forward = _move;
            }
            else
            {
                playerAni.speed = 1;
                playerAni.SetInteger(AniParamName.State, NpcAniState.Idle);
                playerObject.transform.localPosition = Vector3.zero;
                playerAni.transform.localPosition = Vector3.zero;
            }
        }
    }
}