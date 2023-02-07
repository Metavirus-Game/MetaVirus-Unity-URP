using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _EditorUtils;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaptureCameraToPNG))]
public class CaptureCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Capture To File"))
        {
            var capture = serializedObject.targetObject.GetComponent<CaptureCameraToPNG>();
            if (capture)
            {
                var path = capture.pngSavePath;
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("Save Path is empty!");
                }
                else
                {
                    var info = new DirectoryInfo(path);
                    if (!info.Exists)
                    {
                        info.Create();
                    }

                    var ms = $"{DateTime.Now.Second}{DateTime.Now.Millisecond}";
                    var pngName = $"Capture_{Application.productName}_{ms}.png";
                    var savePath = Path.Combine(info.FullName, pngName);
                    ScreenCapture.CaptureScreenshot(savePath);
                }
            }
        }
    }
}