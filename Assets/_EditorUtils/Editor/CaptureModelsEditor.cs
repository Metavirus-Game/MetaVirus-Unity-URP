using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _EditorUtils;
using GameEngine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CaptureModelsToPNG))]
public class CaptureModelsEditor : Editor
{
    private CaptureMode _mode = CaptureMode.怪物整合模式3个一组;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _mode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", _mode);

        GUILayout.Space(10);

        if (GUILayout.Button("Capture To File"))
        {
            var capture = serializedObject.targetObject.GetComponent<CaptureModelsToPNG>();
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

                    if (_mode == CaptureMode.怪物整合模式3个一组)
                    {
                        DoCaptureIntegrated(info, capture.Models);
                    }
                    else
                    {
                        DoCaptureIndividual(info, capture.Models);
                    }

                    //GameFramework.Inst.StartCoroutine(DoCapture(info, capture.Models));
                }
            }
        }
    }

    private async void DoCaptureIndividual(DirectoryInfo info, ModelHelper[] models)
    {
        var nameList = "";
        var p = Path.Combine(info.FullName, "idividual");
        info = new DirectoryInfo(p);
        if (!info.Exists)
        {
            info.Create();
        }

        var modelIndex = 1;
        var currPack = 0;
        foreach (var model in models)
        {
            model.captureMode = CaptureMode.怪物独立模式;
            model.gameObject.SetActive(true);
            int.TryParse(model.packId, out var packId);

            if (currPack != packId)
            {
                currPack = packId;
                modelIndex = 1;
            }

            var count = model.transform.childCount;
            for (var i = 0; i < count; i++)
            {
                var go = model.ShowIndividual(i);
                await Task.Delay((int)(model.captureDelay * 1000));
                var idStr = $"{packId}{modelIndex:D2}{i + 1}";
                var fileName = $"{idStr}.png";
                var itemName = go.name;
                CaptureModel(info, model, fileName);

                nameList += $"{idStr}\t{itemName}{Environment.NewLine}";

                Debug.Log($"{idStr}\t{itemName}");
                await Task.Delay(500);
            }

            modelIndex++;
            model.gameObject.SetActive(false);
        }

        var listFile = Path.Combine(info.FullName, "model_list.txt");
        await File.WriteAllTextAsync(listFile, nameList);
        EditorUtility.DisplayDialog("message", "Completed", "Ok");
    }

    private async void DoCaptureIntegrated(DirectoryInfo info, ModelHelper[] models)
    {
        var p = Path.Combine(info.FullName, "integrated");
        info = new DirectoryInfo(p);
        if (!info.Exists)
        {
            info.Create();
        }

        foreach (var model in models)
        {
            model.captureMode = CaptureMode.怪物整合模式3个一组;
            model.gameObject.SetActive(true);
            await Task.Delay((int)(model.captureDelay * 1000));
            CaptureModel(info, model);
            await Task.Delay(500);
            model.gameObject.SetActive(false);
        }

        EditorUtility.DisplayDialog("message", "Completed", "Ok");
    }

    private void CaptureModel(DirectoryInfo dir, ModelHelper model, string fileName = null)
    {
        var pngName = fileName ?? $"Pack{model.packId}-{model.gameObject.name}.png";
        var savePath = Path.Combine(dir.FullName, pngName);
        ScreenCapture.CaptureScreenshot(savePath);
    }
}