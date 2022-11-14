using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Utils
{
    public const float CellWidth = 1;
    public const float CellHeight = 1;

    public static int2 GetQuadrant(Vector2 pos)
    {
        var xPos = (int)((pos.x + (CellWidth / 2) * math.sign(pos.x)) / CellWidth);
        var yPos = (int)((pos.y + (CellHeight / 2) * math.sign(pos.y)) / CellHeight);

        return new int2(xPos, yPos);
    }
    
    public static void DrawQuadrant(int2 gridPos, Color? c = null)
    {
        var lowerLeft = new Vector3(gridPos.x * CellWidth - CellWidth / 2, gridPos.y * CellHeight - CellHeight / 2, 0);
        var upperLeft = new Vector3(gridPos.x * CellWidth - CellWidth / 2, gridPos.y * CellHeight + CellHeight / 2, 0);
        
        var lowerRight = new Vector3(gridPos.x * CellWidth + CellWidth / 2, gridPos.y * CellHeight - CellHeight / 2, 0);
        var upperRight = new Vector3(gridPos.x * CellWidth + CellWidth / 2, gridPos.y * CellHeight + CellHeight / 2, 0);

        Debug.DrawLine(lowerLeft, upperLeft, c ?? Color.cyan);
        Debug.DrawLine(lowerRight, upperRight, c ?? Color.cyan);
        Debug.DrawLine(lowerLeft, lowerRight, c ?? Color.cyan);
        Debug.DrawLine(upperLeft, upperRight, c ?? Color.cyan);
    }
}
