using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCamera : MonoBehaviour
{
    public bool autoTrunoff = true;

    // Start is called before the first frame update
    void Start()
    {
        if (autoTrunoff)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}