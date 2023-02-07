using System;
using UnityEngine;

namespace MetaVirus.Logic.Player
{
    public class PlayerCameraAnchor : MonoBehaviour
    {
        private Vector3 _playerPos = Vector3.zero;
        private Transform _cameraTrans;

        public float playerDistance = 10;
        public float cameraXRot = 15;
        public float cameraHeight = 8;

        public float cameraMoveSpeed = 1;

        public void Init(Transform cameraTrans, Vector3 pos)
        {
            _playerPos = pos;
            _cameraTrans = cameraTrans;
            _cameraTrans.rotation = Quaternion.Euler(cameraXRot, 0, 0);
            _cameraTrans.position = CalcCameraPos();
        }

        public void UpdatePlayerPos(Vector3 pos)
        {
            _playerPos = pos;
        }

        private Vector3 CalcCameraPos()
        {
            var pos = _playerPos;
            pos.y += cameraHeight;
            pos.z -= playerDistance;
            return pos;
        }

        private void Update()
        {
            if (_cameraTrans == null) return;
            var pos = CalcCameraPos();
            _cameraTrans.position = Vector3.Lerp(_cameraTrans.position, pos, Time.deltaTime * cameraMoveSpeed);
        }
    }
}