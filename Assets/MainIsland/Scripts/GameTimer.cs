using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance;

    public TMP_Text text;
    private float startTime = 0;
    private List<float> times = new();
    public List<string> labels = new();

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        enabled = false;
    }

    void Update()
    {
        var now = Time.time;
        StringBuilder stringBuilder = new();
        if (labels.Count >= times.Count + 1)
        {
            stringBuilder.AppendLine(labels[times.Count]);
        }
        else
        {
            stringBuilder.AppendLine();
        }
        stringBuilder.AppendLine(FormatTime(now - startTime));
        stringBuilder.AppendLine();
        for (int i = 0; i < times.Count; i++)
        {
            if (labels.Count > i)
            {
                labels.Add($"{labels[i]}: ");
            }
            stringBuilder.AppendLine(FormatTime(times[i]));
        }
        text.text = stringBuilder.ToString();
    }

    public void Begin(string label = null)
    {
        gameObject.SetActive(true);
        enabled = true;
        startTime = Time.time;
        times.Clear();
        labels.Clear();
        if (label != null)
        {
            labels.Add(label);
        }
    }
    public void Step(string label = null)
    {
        var last = times.Count == 0 ? startTime : times[^1];
        times.Add(Time.time - last);
        if (label != null)
        {
            labels.Add(label);
        }
    }
    public void Stop()
    {
        Update(); // Certifica que o último tempo é exibido
        enabled = false;
    }
    public void Clear()
    {
        Stop();
        gameObject.SetActive(false);
    }

    private string FormatTime(float time)
    {
        var span = (int)time;
        var seconds = span % 60;
        var minutes = span / 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
}
