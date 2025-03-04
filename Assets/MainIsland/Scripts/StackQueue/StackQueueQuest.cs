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

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());

        yield return new WaitUntil(() => group.Lists != null && group.Lists.Count > 0 && group.Lists[0].Count > 0);

        startButton.onToggle += HandleButton;
        group.GrabArea.onExitEvent += StopPlaying;
        quest.quest.onCompletedEvent += StopPlaying;

        quest.AddTaskHandler(1,
            start: (task) =>
            {
                group.Clear();
                group.Populate(StackQueueGroup.defaultItems);
            },
            update: (task) =>
            {
            },
            cleanup: (task) =>
            {
            });
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
