using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookQuests : MonoBehaviour
{
    private SpatialQuest quest;
    private BookStackGroup stacks;
    private IQuestTask currentTask;
    private int currentTaskIdx;
    private int prevTaskIdx = -1;
    private List<GameObject> initialBooks;
    [SerializeField]
    private GameObject colorsDisplay;
    private int[] task1Order = { 3, 1, 2, 4, 0 };
    [SerializeField]
    private TMP_Text timerText;
    private List<float> times = new();
    private Boolean playing = false;

    IEnumerator Start()
    {
        quest = GetComponent<SpatialQuest>();
        stacks = GetComponent<BookStackGroup>();

        //TODO: usar "yield return null;"
        yield return new WaitUntil(() => stacks.Lists != null && stacks.Lists.Count > 0 && stacks.Lists[0].Count > 0);

        stacks.OnInteract += (slot, list) =>
        {
            if (currentTask == null) return;
            playing = true;
        };

        initialBooks = new();
        foreach (var slot in stacks.Lists[0].Slots)
        {
            initialBooks.Add(slot.Item);
        }
        currentTask = quest.quest.tasks[0];
        currentTaskIdx = 0;
    }

    void Update()
    {
        if (initialBooks == null || currentTask == null) return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            NextTask();
        }

        if (!playing) return;

        var taskChanged = prevTaskIdx != currentTaskIdx;
        prevTaskIdx = currentTaskIdx;

        switch (currentTask.id)
        {
            case 1:
                if (taskChanged) Task1Init();
                Task1();
                break;
            case 2:
                if (taskChanged) Task2Init();
                Task2();
                // Finalização
                if (prevTaskIdx != currentTaskIdx)
                {
                    playing = false;
                }
                break;
        }

        // Atualização do timer
        if (playing)
        {
            StringBuilder ss = new();
            for (int i = 0; i < times.Count; i++)
            {
                var a = times[i];
                var b = i + 1 >= times.Count ? Time.time : times[i + 1];
                var span = (int)(b - a);
                var seconds = span % 60;
                var minutes = span / 60;
                string timeText = string.Format("{0:D2}:{1:D2}", minutes, seconds);
                ss.Append(timeText);
                ss.Append("\n");
            }

            timerText.text = ss.ToString();
        }

        if (currentTask != null)
        {
            DebugUtils.Log(currentTask.name
                + " " + currentTask.progress
                + "/" + currentTask.progressSteps
                + " " + currentTask.status);
        }
        else if (taskChanged)
        {
            DebugUtils.Log("...");
        }
    }

    void NextTask()
    {
        if (currentTask == null) return;
        currentTask.Complete();
        Debug.Log("Quest complete");
        currentTaskIdx++;
        if (currentTaskIdx >= quest.quest.tasks.Count)
        {
            currentTask = null;
            playing = false;
            return;
        }
        currentTask = quest.quest.tasks[currentTaskIdx];
    }

    void Task1Init()
    {
        colorsDisplay.SetActive(false);
        times.Clear();
        times.Add(Time.time);
        playing = true;
    }
    void Task1()
    {
        foreach (var stack in stacks.Lists)
        {
            if (stack.Count == 1 && stack.Slots[0].Item == initialBooks[1])
            {
                NextTask();
            }
        }
    }

    void Task2Init()
    {
        colorsDisplay.SetActive(true);
        for (int i = 0; i < colorsDisplay.transform.childCount; i++)
        {
            var el = colorsDisplay.transform.GetChild(i).GetComponent<RawImage>();
            el.color = stacks.colors[task1Order[i]];
        }
        times.Add(Time.time);
    }
    void Task2()
    {
        foreach (var stack in stacks.Lists)
        {
            int progress = 0;
            for (int i = 0; i < stack.Count; i++)
            {
                if (stack.Slots[i].Item == initialBooks[task1Order[i]])
                {
                    progress++;
                }
            }
            currentTask.progress = progress;
            if (progress == 5)
            {
                colorsDisplay.SetActive(false);
                NextTask();
                break;
            }
        }
    }

}
