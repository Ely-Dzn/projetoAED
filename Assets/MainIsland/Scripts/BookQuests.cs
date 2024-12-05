using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
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

    IEnumerator Start()
    {
        quest = GetComponent<SpatialQuest>();
        stacks = GetComponent<BookStackGroup>();

        yield return new WaitUntil(() => stacks.Lists != null && stacks.Lists.Count > 0 && stacks.Lists[0].Count > 0);

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

        var taskChanged = prevTaskIdx != currentTaskIdx;
        prevTaskIdx = currentTaskIdx;

        switch (currentTask.id)
        {
            case 1:
                Task1();
                break;
            case 2:
                if (taskChanged) Task2Init();
                Task2();
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
        if (currentTaskIdx >= quest.quest.tasks.Count)
        {
            currentTask = null;
            return;
        }
        currentTask = quest.quest.tasks[currentTaskIdx];
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
