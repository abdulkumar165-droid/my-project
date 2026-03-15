using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TelemetryHttpPublisher : MonoBehaviour
{
    public string robotId = "WR-001";
    public WindowRobotController controller;
    public string telemetryEndpoint = "http://127.0.0.1:8000/telemetry";
    public float publishHz = 10f;

    private void Awake()
    {
        if (controller == null)
            controller = FindObjectOfType<WindowRobotController>();
    }

    private void Start()
    {
        StartCoroutine(PublishLoop());
    }

    private IEnumerator PublishLoop()
    {
        var wait = new WaitForSeconds(1f / Mathf.Max(0.1f, publishHz));
        while (true)
        {
            if (controller != null)
            {
                RobotTelemetry telemetry = controller.CaptureTelemetry(robotId);
                string json = JsonUtility.ToJson(telemetry);
                yield return PostJson(telemetryEndpoint, json);
            }
            yield return wait;
        }
    }

    private IEnumerator PostJson(string url, string json)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 2;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogWarning($"Telemetry push failed: {req.error}");
        }
    }
}
