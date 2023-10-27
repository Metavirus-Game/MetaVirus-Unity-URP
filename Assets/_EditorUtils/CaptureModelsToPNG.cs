using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureModelsToPNG : MonoBehaviour
{
    private Camera _camera;

    public ModelHelper[] Models => GetComponentsInChildren<ModelHelper>(true);

    public string pngSavePath;
    // Start is called before the first frame update
}