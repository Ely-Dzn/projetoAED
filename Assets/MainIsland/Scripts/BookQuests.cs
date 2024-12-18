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
    private bool playing = false;

    IEnumerator Start()
    {
        quest = GetComponent<SpatialQuest>();
        stacks = GetComponent<BookStackGroup>();

        //TODO: usar "yield return null;"
        yield return new WaitUntil(() => stacks.Lists != null && stacks.Lists.Count > 0 && stacks.Lists[0].Count > 0);

        stacks.OnInteractEvent += (slot, list) =>
        {
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
        GameTimer.Instance.Step();
        if (currentTaskIdx >= quest.quest.tasks.Count)
        {
            currentTask = null;
            playing = false;
            GameTimer.Instance.Stop();
            return;
        }
        currentTask = quest.quest.tasks[currentTaskIdx];
    }

    void Task1Init()
    {
        colorsDisplay.SetActive(false);
        GameTimer.Instance.Begin();
        foreach (var x in quest.quest.tasks)
        {
            GameTimer.Instance.labels.Add(x.name);
        }
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
