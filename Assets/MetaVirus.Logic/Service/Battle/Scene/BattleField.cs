using System;
using System.Collections;
using System.Collections.Generic;
using cfg.common;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.Scene;
using MilkShake;
using UnityEngine;
using UnityEngine.Serialization;

public class BattleField : MonoBehaviour
{
    public enum CameraControlPoint
    {
        Right,
        Center,
        Left
    }

    public BattleCamera battleCamera;
    public Transform[] upSideSlot;

    public Transform[] downSideSlot;

    [SerializeField] private Transform[] cameraPosControlPoints;
    [SerializeField] private Transform[] cameraPosNearControlPoints;
    [SerializeField] private Transform[] cameraAimPosControlPoints;

    public Transform cameraCloseUpRearPoint;

    public CloseUpCameraAnchor closeUpCameraAnchor;

    public Transform ControlPointRightAimPos => cameraAimPosControlPoints[0];
    public Transform ControlPointCenterAimPos => cameraAimPosControlPoints[1];
    public Transform ControlPointLeftAimPos => cameraAimPosControlPoints[2];

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;

        var pos = Vector3.up * closeUpCameraAnchor.baseHeight;
        Gizmos.DrawLine(Vector3.zero, pos);

        var shootDir = Quaternion.Euler(closeUpCameraAnchor.cameraShootAngle) * Vector3.up;
        shootDir.Normalize();

        Gizmos.DrawLine(pos, pos + shootDir * closeUpCameraAnchor.shootDistance);

        var handlePos = pos + shootDir * closeUpCameraAnchor.shootDistance;

        Gizmos.DrawCube(handlePos, new Vector3(0.3f, 0.3f, 0.3f));

        var handleDir = pos - handlePos;

        handlePos = transform.TransformPoint(handlePos);
        handleDir = transform.TransformDirection(handleDir);

        UnityEditor.Handles.ArrowHandleCap(0, handlePos, Quaternion.LookRotation(handleDir), 1,
            EventType.Repaint);
    }
#endif


    // Start is called before the first frame update
    void Start()
    {
        battleCamera = GetComponentInChildren<BattleCamera>();
    }

    public Transform GetCameraControlPoint(CameraControlPoint controlPoint, bool rear = true)
    {
        var point = rear ? cameraPosControlPoints : cameraPosNearControlPoints;
        return point[(int)controlPoint];
    }

    public Transform GetCameraAimControlPoint(CameraControlPoint controlPoint)
    {
        var point = cameraAimPosControlPoints;
        return point[(int)controlPoint];
    }

    // public Transform GetCameraControlPointCenter(bool rear = true)
    // {
    //     var point = rear ? cameraPosControlPoints : cameraPosNearControlPoints;
    //     return point[1];
    // }
    //
    // public Transform GetCameraControlPointLeft(bool rear = true)
    // {
    //     var point = rear ? cameraPosControlPoints : cameraPosNearControlPoints;
    //     return point[2];
    // }


    /// <summary>
    /// 取得距离position最近的摄像机瞄准点
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public CameraControlPoint GetCloseControlPointAim(Vector3 position)
    {
        var close = 0;
        var closeDist = Vector3.Distance(cameraAimPosControlPoints[close].position, position);
        for (var i = 1; i < cameraAimPosControlPoints.Length; i++)
        {
            var dist = Vector3.Distance(cameraAimPosControlPoints[i].position, position);
            if (dist < closeDist)
            {
                closeDist = dist;
                close = i;
            }
        }

        return (CameraControlPoint)close;
    }


    public CameraControlPoint GetSlotControlPoint(int slot, bool rear = true)
    {
        switch (slot)
        {
            case 1 or 4 or 7:
                return CameraControlPoint.Left;
            case 2 or 5 or 8:
                return CameraControlPoint.Center;
            case 3 or 6 or 9:
                return CameraControlPoint.Right;
        }

        return CameraControlPoint.Center;
    }

    public CameraControlPoint GetCloseControlPoint(Vector3 position, bool rear = true)
    {
        var points = rear ? cameraPosControlPoints : cameraPosNearControlPoints;

        var idx = 0;
        var closeDist = Vector3.Distance(points[0].position, position);
        for (var i = 1; i < points.Length; i++)
        {
            var dist = Vector3.Distance(points[i].position, position);
            if (dist < closeDist)
            {
                idx = i;
                closeDist = dist;
            }
        }

        return (CameraControlPoint)idx;
    }

    /// <summary>
    /// 取得对角线控制点
    /// 左→右 中→中 右→左
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public CameraControlPoint GetDiagonalPoint(CameraControlPoint point)
    {
        switch (point)
        {
            case CameraControlPoint.Right:
                return CameraControlPoint.Left;
            case CameraControlPoint.Center:
                return CameraControlPoint.Center;
            case CameraControlPoint.Left:
                return CameraControlPoint.Right;
        }

        return CameraControlPoint.Center;
    }

    /// <summary>
    /// 取得战斗单位所在位置
    /// </summary>
    /// <param name="battleUnit"></param>
    /// <returns></returns>
    public Transform GetSlotTrans(BattleUnit battleUnit)
    {
        var side = battleUnit.Side;
        var slots = upSideSlot;
        if (side == BattleUnitSide.Source)
        {
            slots = downSideSlot;
        }

        return GetSlotTrans(slots, battleUnit.Slot);
    }

    public Transform GetSlotTrans(BattleUnitSide side, int slotId)
    {
        var slots = side == BattleUnitSide.Source ? downSideSlot : upSideSlot;
        return GetSlotTrans(slots, slotId);
    }

    /// <summary>
    /// 取得上侧战斗位置
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="slotId">1 ~ 9</param>
    /// <returns></returns>
    private static Transform GetSlotTrans(Transform[] trans, int slotId)
    {
        slotId = Mathf.Min(slotId, 9);
        slotId = Mathf.Max(slotId, 1);

        return trans[slotId - 1];
    }
}