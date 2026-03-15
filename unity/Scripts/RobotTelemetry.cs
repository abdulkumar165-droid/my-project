using System;
using UnityEngine;

[Serializable]
public class RobotTelemetry
{
    public string robotId;
    public float simTime;
    public Vector3 worldPosition;
    public Vector3 worldEuler;
    public float speed;
    public bool spraying;
    public float wipeCoverage01;
    public float paintCoverage01;
    public float wipedAreaM2;
    public float paintedAreaM2;
    public float battery01;
    public float tank01;
    public string status;
}
