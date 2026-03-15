using System.Collections.Generic;
using UnityEngine;

public class WindowRobotController : MonoBehaviour
{
    [Header("References")]
    public Transform windowOrigin;
    public CoverageGrid coverageGrid;

    [Header("Path (UV)")]
    public List<Vector2> pathUv = new List<Vector2>();

    [Header("Kinematics")]
    public float speedMps = 0.25f;
    public float wipeRadiusMeters = 0.08f;
    public float sprayRadiusMeters = 0.12f;

    [Header("Tools")]
    public bool sprayEnabled = true;
    public float sprayDutyCycle = 0.6f;
    public float batteryDrainPerSec = 0.002f;
    public float tankDrainPerSecWhenSpray = 0.01f;

    private int targetIndex;
    private float battery01 = 1.0f;
    private float tank01 = 1.0f;

    private void Reset()
    {
        pathUv = BuildZigzagPath(8, 12);
    }

    private void Awake()
    {
        if (coverageGrid == null)
            coverageGrid = FindObjectOfType<CoverageGrid>();

        if (pathUv.Count == 0)
            pathUv = BuildZigzagPath(8, 12);
    }

    private void Update()
    {
        if (windowOrigin == null || coverageGrid == null || pathUv.Count == 0)
            return;

        MoveAlongPath(Time.deltaTime);

        Vector2 uv = GetCurrentUV();
        coverageGrid.MarkWipe(uv, wipeRadiusMeters);

        bool sprayingNow = sprayEnabled && tank01 > 0f && IsSprayPhase(Time.time);
        if (sprayingNow)
            coverageGrid.MarkPaint(uv, sprayRadiusMeters);

        battery01 = Mathf.Clamp01(battery01 - batteryDrainPerSec * Time.deltaTime);
        if (sprayingNow)
            tank01 = Mathf.Clamp01(tank01 - tankDrainPerSecWhenSpray * Time.deltaTime);
    }

    private void MoveAlongPath(float dt)
    {
        Vector3 current = transform.position;
        Vector3 target = UVToWorld(pathUv[targetIndex]);

        float step = speedMps * dt;
        Vector3 next = Vector3.MoveTowards(current, target, step);
        transform.position = next;

        Vector3 forward = (target - current);
        if (forward.sqrMagnitude > 1e-6f)
            transform.rotation = Quaternion.LookRotation(forward.normalized, windowOrigin.up);

        if (Vector3.Distance(next, target) < 0.002f)
            targetIndex = (targetIndex + 1) % pathUv.Count;
    }

    public Vector2 GetCurrentUV()
    {
        Vector3 local = windowOrigin.InverseTransformPoint(transform.position);
        float u = Mathf.Clamp01(local.x / coverageGrid.width + 0.5f);
        float v = Mathf.Clamp01(local.y / coverageGrid.height + 0.5f);
        return new Vector2(u, v);
    }

    private Vector3 UVToWorld(Vector2 uv)
    {
        float x = (uv.x - 0.5f) * coverageGrid.width;
        float y = (uv.y - 0.5f) * coverageGrid.height;
        Vector3 local = new Vector3(x, y, 0f);
        return windowOrigin.TransformPoint(local);
    }

    private bool IsSprayPhase(float timeSec)
    {
        float t = Mathf.Repeat(timeSec, 1f);
        return t <= sprayDutyCycle;
    }

    private List<Vector2> BuildZigzagPath(int strips, int pointsPerStrip)
    {
        var points = new List<Vector2>();
        for (int r = 0; r < strips; r++)
        {
            float v = strips == 1 ? 0.5f : (float)r / (strips - 1);
            if (r % 2 == 0)
            {
                for (int c = 0; c < pointsPerStrip; c++)
                {
                    float u = pointsPerStrip == 1 ? 0.5f : (float)c / (pointsPerStrip - 1);
                    points.Add(new Vector2(u, v));
                }
            }
            else
            {
                for (int c = pointsPerStrip - 1; c >= 0; c--)
                {
                    float u = pointsPerStrip == 1 ? 0.5f : (float)c / (pointsPerStrip - 1);
                    points.Add(new Vector2(u, v));
                }
            }
        }
        return points;
    }

    public RobotTelemetry CaptureTelemetry(string robotId)
    {
        bool sprayingNow = sprayEnabled && tank01 > 0f && IsSprayPhase(Time.time);
        return new RobotTelemetry
        {
            robotId = robotId,
            simTime = Time.time,
            worldPosition = transform.position,
            worldEuler = transform.eulerAngles,
            speed = speedMps,
            spraying = sprayingNow,
            wipeCoverage01 = coverageGrid.WipeCoverage01,
            paintCoverage01 = coverageGrid.PaintCoverage01,
            wipedAreaM2 = coverageGrid.WipedAreaM2,
            paintedAreaM2 = coverageGrid.PaintedAreaM2,
            battery01 = battery01,
            tank01 = tank01,
            status = battery01 <= 0.05f ? "LOW_BATTERY" : "RUNNING"
        };
    }
}
