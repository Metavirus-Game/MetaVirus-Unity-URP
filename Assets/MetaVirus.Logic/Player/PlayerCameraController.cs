using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        public Transform nearPos;
        public Transform[] rearPos;
        public Camera playerCamera;
        public float cameraMoveSpeed = 1;

        private Vector3 _cameraPos;
        [SerializeField] private int currRearPos = 0;

        public int CurrRearPos => currRearPos;

        private Transform CurrRearTrans => rearPos[currRearPos];

        /// <summary>
        /// 镜头移动模式改为和地图中的cameraAnchor对齐，始终与主角保持指定的距离
        /// </summary>
        private PlayerCameraAnchor _cameraAnchor;

        private IEnumerator Start()
        {
            //寻找地图中的CameraAnchor
            _cameraAnchor = FindObjectOfType<PlayerCameraAnchor>();

            while (_cameraAnchor == null)
            {
                yield return null;
                Debug.LogWarning("PlayerCameraAnchor not found in current scene, keep trying...");
                _cameraAnchor = FindObjectOfType<PlayerCameraAnchor>();
            }

            _cameraAnchor.Init(playerCamera.transform, transform.position);

            playerCamera.transform.SetParent(null);
        }

        private void OnDestroy()
        {
            Object.Destroy(playerCamera.gameObject);
        }

        public int ChangeRearPos()
        {
            //镜头改成伸缩了，不做角度变化
            //currRearPos = (currRearPos + 1) % rearPos.Length;
            return currRearPos;
        }

        private void Update()
        {
            //取消镜头遮挡自动移动
            // var pp = nearPos.position;
            // var cdir = CurrRearTrans.position - pp;
            // if (Physics.Raycast(pp, cdir, out var hit, cdir.magnitude, -1))
            // {
            //     if (hit.collider != null)
            //     {
            //         _cameraPos = hit.point;
            //     }
            // }
            // else
            // {
            //     _cameraPos = CurrRearTrans.position;
            // }

            // playerCamera.transform.position =
            //     Vector3.Lerp(playerCamera.transform.position, _cameraPos, Time.deltaTime * cameraMoveSpeed);
            //
            // playerCamera.transform.forward =
            //     Vector3.Lerp(playerCamera.transform.forward, CurrRearTrans.forward, Time.deltaTime * cameraMoveSpeed);

            if (_cameraAnchor != null)
            {
                _cameraAnchor.UpdatePlayerPos(transform.position);
            }
        }
    }
}