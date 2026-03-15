using UnityEngine;

public class CoverageGrid : MonoBehaviour
{
    [Header("Window Size (meters)")]
    public float width = 2.0f;
    public float height = 3.0f;

    [Header("Grid Resolution")]
    public int rows = 120;
    public int cols = 80;

    private bool[,] wipeVisited;
    private bool[,] paintVisited;

    private int wipeCount;
    private int paintCount;

    public void Initialize()
    {
        wipeVisited = new bool[rows, cols];
        paintVisited = new bool[rows, cols];
        wipeCount = 0;
        paintCount = 0;
    }

    private void Awake()
    {
        Initialize();
    }

    public void MarkWipe(Vector2 uv, float radiusMeters)
    {
        Mark(uv, radiusMeters, wipeVisited, ref wipeCount);
    }

    public void MarkPaint(Vector2 uv, float radiusMeters)
    {
        Mark(uv, radiusMeters, paintVisited, ref paintCount);
    }

    private void Mark(Vector2 uv, float radiusMeters, bool[,] buffer, ref int count)
    {
        int centerR = Mathf.RoundToInt(uv.y * (rows - 1));
        int centerC = Mathf.RoundToInt(uv.x * (cols - 1));

        int rExtent = Mathf.CeilToInt(radiusMeters / (height / rows));
        int cExtent = Mathf.CeilToInt(radiusMeters / (width / cols));

        int rMin = Mathf.Max(0, centerR - rExtent);
        int rMax = Mathf.Min(rows - 1, centerR + rExtent);
        int cMin = Mathf.Max(0, centerC - cExtent);
        int cMax = Mathf.Min(cols - 1, centerC + cExtent);

        float rWorld = Mathf.Max(0.0001f, height / rows);
        float cWorld = Mathf.Max(0.0001f, width / cols);

        for (int r = rMin; r <= rMax; r++)
        {
            for (int c = cMin; c <= cMax; c++)
            {
                float dr = (r - centerR) * rWorld;
                float dc = (c - centerC) * cWorld;
                float d = Mathf.Sqrt(dr * dr + dc * dc);
                if (d > radiusMeters)
                    continue;

                if (!buffer[r, c])
                {
                    buffer[r, c] = true;
                    count++;
                }
            }
        }
    }

    public float WipeCoverage01 => rows * cols == 0 ? 0f : (float)wipeCount / (rows * cols);
    public float PaintCoverage01 => rows * cols == 0 ? 0f : (float)paintCount / (rows * cols);

    public float CellArea => (width * height) / Mathf.Max(1, rows * cols);

    public float WipedAreaM2 => wipeCount * CellArea;
    public float PaintedAreaM2 => paintCount * CellArea;
}
