using UnityEngine;
using System.Collections.Generic;

public class OnScreenLogger : MonoBehaviour
{
    private Queue<string> logs = new Queue<string>();
    private const int maxLogs = 20; // show last 20 messages
    private Vector2 scrollPosition;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string msg = $"[{type}] {logString}";
        logs.Enqueue(msg);

        if (logs.Count > maxLogs)
            logs.Dequeue();
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height / 2));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (string log in logs)
        {
            GUILayout.Label(log, style);
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
