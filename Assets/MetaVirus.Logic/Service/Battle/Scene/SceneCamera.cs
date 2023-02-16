using System;
using FairyGUI;
using GameEngine;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.UI;
using MetaVirus.Logic.Service.UI;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MetaVirus.Logic.Service.Battle.Scene
{
    [RequireComponent(typeof(Camera))]
    public class SceneCamera : MonoBehaviour
    {
        private Camera _camera;

        private int _cameraCullingMask = 0;
        private EventService _eventService;

        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                    _cameraCullingMask = _camera.cullingMask;
                }

                return _camera;
            }
        }

        private void Start()
        {
            _eventService = GameFramework.GetService<EventService>();
            _eventService.On<TopLayerFullscreenUIChangedEvent>(GameEvents.UIEvent.TopLayerFullscreenUIChanged,
                OnTopLayerUIChanged);
        }

        private void OnTopLayerUIChanged(TopLayerFullscreenUIChangedEvent evt)
        {
            if (evt.EventState == TopLayerFullscreenUIChangedEvent.State.Shown)
            {
                //顶层全屏ui显示了，暂停摄像机渲染
                _camera.cullingMask = 0;
            }
            else
            {
                _camera.cullingMask = _cameraCullingMask;
            }
        }

        private void OnDestroy()
        {
            _eventService.Remove<TopLayerFullscreenUIChangedEvent>(GameEvents.UIEvent.TopLayerFullscreenUIChanged,
                OnTopLayerUIChanged);
        }

        private void OnEnable()
        {
            AddUICameraToStack();
        }

        public void TurnOn()
        {
            Camera.enabled = true;
        }

        public void TurnOff()
        {
            Camera.enabled = false;
        }

        private void AddUICameraToStack()
        {
            if (Camera == null)
            {
                return;
            }

            var stageCamera = StageCamera.main;
            if (!Camera.GetUniversalAdditionalCameraData().cameraStack.Contains(stageCamera))
            {
                Camera.GetUniversalAdditionalCameraData().cameraStack.Add(stageCamera);
            }
        }
    }
}