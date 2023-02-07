using System;
using FairyGUI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MetaVirus.Logic.Service.Battle.Scene
{
    [RequireComponent(typeof(Camera))]
    public class SceneCamera : MonoBehaviour
    {
        private Camera _camera;

        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }

                return _camera;
            }
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