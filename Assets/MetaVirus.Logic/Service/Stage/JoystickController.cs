using System;
using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.FairyGUI;
using GameEngine.Utils;
using MetaVirus.Logic.Player;
using UnityEngine;
using UnityEngine.Events;

public class JoystickController : MonoBehaviour
{
    private FairyGUIService _fairyService;

    private GComponent _joystick;
    private GImage _center;
    private GButton _btnCamera;
    private GImage _imgCamera;
    private Vector3 _centerPos;
    private Vector3 _joystickPos;

    private GComponent _ui;
    private GObject[] _arrows;

    //private PlayerCameraController _cameraController;

    public UnityAction<Vector3> OnJoystickMoved { get; set; }

    public UnityAction OnJoystickStopped { get; set; }

    private int[] _cameraAngles = new[] { -35, -45, -90 };

    private readonly Vector2 _joystickUp = new Vector2(0, -1);

    private bool _uiAdded = false;

    void Start()
    {
        //_cameraController = GetComponent<PlayerCameraController>();
        StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        _fairyService = GameFramework.GetService<FairyGUIService>();

        var task = _fairyService.AddPackageAsync("GamePlay");
        yield return task.AsCoroution();

        _ui = UIPackage.CreateObject("GamePlay", "StageUI").asCom;

        var touchArea = _ui.GetChild("TouchArea").asButton;
        touchArea.onTouchBegin.Add(OnJoystickAreaTouched);
        touchArea.onTouchMove.Add(OnJoystickAreaTouched);
        touchArea.onTouchEnd.Add(OnJoystickAreaTouched);


        var joystick = _ui.GetChild("Joystick").asCom;

        var arrowT = joystick.GetChild("arrow_lt");
        var arrowR = joystick.GetChild("arrow_rt");
        var arrowL = joystick.GetChild("arrow_lb");
        var arrowB = joystick.GetChild("arrow_rb");

        var center = joystick.GetChild("n1").asImage;

        _joystick = joystick;
        _center = center;
        _arrows = new[] { arrowT, arrowR, arrowB, arrowL };


        _btnCamera = _ui.GetChild("n2").asButton;
        _imgCamera = _btnCamera.GetChild("icon").asImage;

        _btnCamera.onClick.Add(ChangeCameraAngle);
        _fairyService.AddToGRootFullscreen(_ui);
        _uiAdded = true;
        yield return null;
        _joystickPos = joystick.position;
    }

    private void ChangeCameraAngle()
    {
        //_cameraController.ChangeRearPos();
    }

    // private void OnDisable()
    // {
    //     GRoot.inst.RemoveChild(_ui);
    //     _uiAdded = false;
    // }
    //
    // private void OnEnable()
    // {
    //     if (_ui != null && !_uiAdded)
    //     {
    //         _fairyService.AddToGRootFullscreen(_ui);
    //         _uiAdded = true;
    //     }
    // }

    private void OnJoystickAreaTouched(EventContext context)
    {
        var p = GRoot.inst.GlobalToLocal(context.inputEvent.position);
        switch (context.type)
        {
            case "onTouchBegin":
                _joystick.position = p;
                _centerPos = _center.position;
                break;
            case "onTouchMove":
                var sub = (Vector3)p - _joystick.position;
                var distance = sub.magnitude;
                if (distance > 120)
                {
                    sub = sub.normalized * 120;
                }

                _center.position = _centerPos + sub;

                //计算方向夹角
                var a = Vector2.Dot(_joystickUp, sub.normalized);
                a = Mathf.Acos(a) * Mathf.Rad2Deg;

                var c = Mathf.Sign(Vector3.Cross(_joystickUp, sub.normalized).z);
                if (c < 0)
                {
                    a = 360 - a;
                }

                var idx = a switch
                {
                    > 45 and < 135 => 1,
                    > 135 and < 225 => 2,
                    > 225 and < 315 => 3,
                    _ => 0
                };

                for (var i = 0; i < _arrows.Length; i++)
                {
                    _arrows[i].visible = idx == i;
                }

                OnJoystickMoved?.Invoke(sub);
                break;
            case "onTouchEnd":
                foreach (var t in _arrows)
                {
                    t.visible = false;
                }

                _center.position = _centerPos;
                _joystick.position = _joystickPos;
                OnJoystickStopped?.Invoke();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (_imgCamera != null)
        // {
        //     _imgCamera.rotation =
        //         Mathf.Lerp(_imgCamera.rotation, _cameraAngles[_cameraController.CurrRearPos], Time.deltaTime * 10);
        // }
    }
}