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
    }
}
