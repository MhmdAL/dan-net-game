using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GridUtils
{
    public const int CellSize = 1;

    public static int2 GetCell(Vector2 pos)
    {
        var xPos = (int)((pos.x + (CellSize / 2f) * math.sign(pos.x)) / CellSize);
        var yPos = (int)((pos.y + (CellSize / 2f) * math.sign(pos.y)) / CellSize);

        return new int2(xPos, yPos);
    }

    public static float2 GetCellWorldPosition(int2 cell)
    {
        var xPos = CellSize * cell.x;
        var yPos = CellSize * cell.y;

        return new float2(xPos, yPos);
    }

    // Gets the list of cells which form a bounding box around a circle 
    public static NativeList<int2> GetBoundingBox(int2 originCell, float radius)
    {
        var cellsIncluded = new NativeList<int2>(Allocator.Temp);

        var bbTopLeft = new float2(originCell.x - radius, originCell.y + radius);
        var bbBottomRight = new float2(originCell.x + radius, originCell.y - radius);
        var topLeftCell = GetCell(bbTopLeft);
        var bottomRightCell = GetCell(bbBottomRight);

        for (int y = topLeftCell.y; y > bottomRightCell.y - 1; y--)
        {
            for (int x = topLeftCell.x; x < bottomRightCell.x + 1; x++)
            {
                cellsIncluded.Add(new int2(x, y));
            }
        }

        return cellsIncluded;
    }

    // Gets the list of cells covered by the radius
    public static NativeList<int2> GetCoveredCells(int2 originCell, float radius)
    {
        // TODO: Find ways to optimize this
        
        var cellsIncluded = new NativeList<int2>(Allocator.Temp);

        var bbTopLeft = new float2(originCell.x - radius, originCell.y + radius);
        var bbBottomRight = new float2(originCell.x + radius, originCell.y - radius);
        var topLeftCell = GetCell(bbTopLeft);
        var bottomRightCell = GetCell(bbBottomRight);

        for (int y = topLeftCell.y; y > bottomRightCell.y - 1; y--)
        {
            for (int x = topLeftCell.x; x < bottomRightCell.x + 1; x++)
            {
                var cellRadius = radius / CellSize;

                var cellCenter = new int2(x, y); 
                
                if (IsCellCovered(originCell, cellCenter, cellRadius))
                {
                    cellsIncluded.Add(cellCenter);
                }
            }
        }

        return cellsIncluded;
    }

    private static bool IsCellCovered(int2 originCell, int2 targetCell, float cellRadius)
    {
        // Check if any corner or midpoint of the cell is included

        var sqRadius = cellRadius * cellRadius;

        var cp1 = targetCell + new int2(1, 1) * CellSize;
        var mp1 = targetCell + new int2(1, 0) * CellSize;
        var cp2 = targetCell + new int2(1, -1) * CellSize;
        var mp2 = targetCell + new int2(0, -1) * CellSize;
        var cp3 = targetCell + new int2(-1, -1) * CellSize;
        var mp3 = targetCell + new int2(-1, 0) * CellSize;
        var cp4 = targetCell + new int2(-1, 1) * CellSize;
        var mp4 = targetCell + new int2(0, 1) * CellSize;

        if (math.distancesq(originCell, cp1) < sqRadius
            || math.distancesq(originCell, cp2) < sqRadius
            || math.distancesq(originCell, cp3) < sqRadius
            || math.distancesq(originCell, cp4) < sqRadius
            || math.distancesq(originCell, mp1) < sqRadius
            || math.distancesq(originCell, mp2) < sqRadius
            || math.distancesq(originCell, mp3) < sqRadius
            || math.distancesq(originCell, mp4) < sqRadius)
        {
            return true;
        }

        return false;
    }

    public static void DrawCell(int2 gridPos, Color? c = null)
    {
        var lowerLeft = new Vector3(gridPos.x * CellSize - CellSize / 2f, gridPos.y * CellSize - CellSize / 2f, 0);
        var upperLeft = new Vector3(gridPos.x * CellSize - CellSize / 2f, gridPos.y * CellSize + CellSize / 2f, 0);

        var lowerRight = new Vector3(gridPos.x * CellSize + CellSize / 2f, gridPos.y * CellSize - CellSize / 2f, 0);
        var upperRight = new Vector3(gridPos.x * CellSize + CellSize / 2f, gridPos.y * CellSize + CellSize / 2f, 0);

        Debug.DrawLine(lowerLeft, upperLeft, c ?? Color.cyan);
        Debug.DrawLine(lowerRight, upperRight, c ?? Color.cyan);
        Debug.DrawLine(lowerLeft, lowerRight, c ?? Color.cyan);
        Debug.DrawLine(upperLeft, upperRight, c ?? Color.cyan);
    }
    
    public static void DrawCells(NativeArray<int2> cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            DrawCell(cells[i]);
        }
    }
    
    public static void DrawCircle(Vector3 position, float radius, int segments, Color color)
    {
        // If either radius or number of segments are less or equal to 0, skip drawing
        if (radius <= 0.0f || segments <= 0)
        {
            return;
        }

        // Single segment of the circle covers (360 / number of segments) degrees
        float angleStep = (360.0f / segments);

        // Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
        // which are required by Unity's Mathf class trigonometry methods

        angleStep *= Mathf.Deg2Rad;

        // lineStart and lineEnd variables are declared outside of the following for loop
        Vector3 lineStart = Vector3.zero;
        Vector3 lineEnd = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            // Line start is defined as starting angle of the current segment (i)
            lineStart.x = Mathf.Cos(angleStep * i);
            lineStart.y = Mathf.Sin(angleStep * i);

            // Line end is defined by the angle of the next segment (i+1)
            lineEnd.x = Mathf.Cos(angleStep * (i + 1));
            lineEnd.y = Mathf.Sin(angleStep * (i + 1));

            // Results are multiplied so they match the desired radius
            lineStart *= radius;
            lineEnd *= radius;

            // Results are offset by the desired position/origin 
            lineStart += position;
            lineEnd += position;

            // Points are connected using DrawLine method and using the passed color
            Debug.DrawLine(lineStart, lineEnd, color);
        }
    }
}