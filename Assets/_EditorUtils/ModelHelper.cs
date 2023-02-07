using System;
using System.Collections;
using System.Collections.Generic;
using _EditorUtils;
using UnityEngine;

public class ModelHelper : MonoBehaviour
{
    public float ySpace = 2.3f;
    public float captureDelay = 1;

    public string packId = "01";

    public CaptureMode captureMode;

    private List<GameObject> _items = new();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void ShowIntegrated()
    {
        var count = transform.childCount;
        for (var i = 0; i < count; i++)
        {
            var c = transform.GetChild(i);
            var om = Instantiate(c.gameObject);
            var ot = Instantiate(c.gameObject);

            om.transform.SetParent(transform, false);
            ot.transform.SetParent(transform, false);

            om.transform.localPosition = c.localPosition + new Vector3(0, ySpace, 0);
            om.transform.localEulerAngles = new Vector3(0, 90, 0);
            ot.transform.localPosition = om.transform.localPosition + new Vector3(0, ySpace, 0);
            ot.transform.localEulerAngles = new Vector3(0, 180, 0);

            _items.Add(om);
            _items.Add(ot);
        }
    }

    public GameObject ShowIndividual(int index)
    {
        foreach (var item in _items)
        {
            Destroy(item);
        }

        _items.Clear();

        var child = transform.GetChild(index).gameObject;

        for (var i = 0; i < 3; i++)
        {
            var obj = Instantiate(child, transform, false);
            _items.Add(obj);
            obj.SetActive(true);
            obj.transform.localPosition = new Vector3(0, ySpace * i, 0);
            obj.transform.localEulerAngles = new Vector3(0, 90 * i, 0);
        }

        return child;
    }

    private void OnEnable()
    {
        for (var idx = 0; idx < transform.childCount; idx++)
        {
            transform.GetChild(idx).gameObject.SetActive(captureMode == CaptureMode.怪物整合模式3个一组);
        }

        if (captureMode == CaptureMode.怪物整合模式3个一组)
        {
            ShowIntegrated();
        }
    }

    private void OnDisable()
    {
        foreach (var item in _items)
        {
            Destroy(item);
        }

        _items.Clear();
    }
}