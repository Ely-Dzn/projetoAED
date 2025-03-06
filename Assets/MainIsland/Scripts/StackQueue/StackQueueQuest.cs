using System.Collections;
using SpatialSys.UnitySDK;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StackQueueQuest : MonoBehaviour
{
    private const string QUEST_ID = "stack_and_queue";

    private QuestWrapper quest;
    public StackQueueGroup group;
    public ToggleButton startButton;
    private bool playing = false;
    private SequenceGuide[] sequenceGuides;

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());

        yield return new WaitUntil(() => group.Lists != null && group.Lists.Count > 0 && group.Lists[0].Count > 0);

        sequenceGuides = new SequenceGuide[group.Lists.Count];
        for (int i = 0; i < group.Lists.Count; i++)
        {
            sequenceGuides[i] = group.Lists[i].gameObject.GetComponentInChildren<SequenceGuide>(true);
        }

        startButton.onToggle += HandleButton;
        group.GrabArea.onExitEvent += StopPlaying;
        quest.quest.onCompletedEvent += StopPlaying;

        // Inverter a pilha
        int[][] task1Items = {
            new int[]{ },
            new int[]{ 3, 1, 2, 4, 0 },
            new int[]{ },
            new int[]{ },
        };
        int[] task1Target = task1Items[1].Reverse().ToArray();
        quest.AddTaskHandler(1,
            start: (task) =>
            {
                group.Clear();
                group.Populate(task1Items);
                group.stack1.enabled = false;
                group.queue2.enabled = false;
                sequenceGuides[1].Display(task1Target, group.colors);
            },
            update: (task) =>
            {
                if (Utils.CompareColorSequence(group.stack2, task1Target, group.colors).ok)
                {
                    task.CompleteTask();
                }
            },
            cleanup: (task) =>
            {
                ClearGuides();
                group.stack1.enabled = true;
                group.queue2.enabled = true;
            });

        // Combinar duas pilhas numa fila
        int[][] task2Items = {
            new int[]{ 1, 3, 4 },
            new int[]{ 2, 4, 0 },
            new int[]{ },
            new int[]{ },
        };
        int[] task2Target = { 1, 3, 4, 2, 4, 0 };
        quest.AddTaskHandler(2,
            start: (task) =>
            {
                group.Clear();
                group.Populate(task2Items);
                group.queue2.enabled = false;
                sequenceGuides[2].Display(task2Target, group.colors);
            },
            update: (task) =>
            {
                if (Utils.CompareColorSequence(group.queue1, task2Target, group.colors).ok)
                {
                    task.CompleteTask();
                }
            },
            cleanup: (task) =>
            {
                ClearGuides();
                group.queue2.enabled = true;
            });
    }

    void ClearGuides()
    {
        foreach (var guide in sequenceGuides)
        {
            guide.Clear();
        }
    }

    void HandleButton()
    {
        if (startButton.State) StartPlaying();
        else StopPlaying();
        startButton.State = playing;
    }
    void StartPlaying()
    {
        if (QuestSync.Instance.GetCurrent() != null)
        {
            QuestSync.Instance.StopAny();
        }
        QuestSync.Instance.SetCurrent(QUEST_ID, StopPlaying);
        playing = true;
        startButton.State = true;
        foreach (var t in quest.quest.tasks)
        {
            GameTimer.Instance.labels.Add(t.name);
        }
        GameTimer.Instance.Begin();
        quest.Start();
    }
    void StopPlaying()
    {
        QuestSync.Instance.Stop(QUEST_ID);
        group.Clear();
        group.Populate(StackQueueGroup.defaultItems);
        playing = false;
        startButton.State = false;
        GameTimer.Instance.Stop();
        quest.Reset();
    }
    void Update()
    {
        if (!playing || quest.quest.status != QuestStatus.InProgress) return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            SkipTask();
        }

        quest.Update();
    }
    void SkipTask()
    {
        quest.ActiveTask?.CompleteTask();
    }
}
