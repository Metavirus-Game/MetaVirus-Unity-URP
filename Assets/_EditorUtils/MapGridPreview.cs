using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridPreview : MonoBehaviour
{
    public int midLine = 25;
    public int gridWidth = 10;
    public int gridHeight = 24;

    public Vector2 mapSize;

    private void OnDrawGizmos()
    {
        var m = Gizmos.matrix;
        Gizmos.matrix = transform.worldToLocalMatrix;

        var centerPoint = new Vector3(gridWidth / 2f, 0, midLine);

        for (var x = 0; x < mapSize.x; x += gridWidth)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(centerPoint, new Vector3(gridWidth, 1, gridHeight));
            centerPoint.x += gridWidth;
        }

        Gizmos.matrix = m;
    }
}