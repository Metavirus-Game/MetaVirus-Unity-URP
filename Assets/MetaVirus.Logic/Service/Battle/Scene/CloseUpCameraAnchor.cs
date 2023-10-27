using System;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Scene
{
    [Serializable]
    public class CloseUpCameraAnchor
    {
        public struct BattleUnitAnchor
        {
            public BattleUnitAnchor(Vector3 position, Quaternion rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }

            public Vector3 position { get; }
            public Quaternion rotation { get; }
        }

        public Vector3 cameraShootAngle = new Vector3(15, -15, 0);
        public float shootDistance = 5;
        public float baseHeight = 2;

        public BattleUnitAnchor GetUnitCloseUpCameraPos(BattleUnitEntity entity)
        {
            var pos = Vector3.up * baseHeight;
            var shootDir = Quaternion.Euler(cameraShootAngle) * Vector3.up;
            shootDir.Normalize();

            var shootPos = pos + shootDir * shootDistance;
            shootDir = pos - shootPos;

            shootPos = entity.Transform.TransformPoint(shootPos);
            shootDir = entity.Transform.TransformDirection(shootDir);

            var shootRot = Quaternion.LookRotation(shootDir);
            return new BattleUnitAnchor(shootPos, shootRot);
        }
    }
}