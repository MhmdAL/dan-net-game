using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
 
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
 
        lineRenderer.SetPosition(0, new Vector3(-4, 4, 0));
        lineRenderer.SetPosition(1, new Vector3(-4, 0, 0));
        lineRenderer.SetPosition(2, new Vector3(-4, -4, 0));
        lineRenderer.SetPosition(3, new Vector3(0, -4, 0));
        lineRenderer.SetPosition(4, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(5, new Vector3(0, 4, 0));
        lineRenderer.SetPosition(6, new Vector3(4, 4, 0));
        lineRenderer.SetPosition(7, new Vector3(4, 0, 0));
        lineRenderer.SetPosition(8, new Vector3(4, -4, 0));
    }
}
