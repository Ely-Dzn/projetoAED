using System.Collections;
using SpatialSys.UnitySDK;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StackQueueQuest : MonoBehaviour
{
    private QuestWrapper quest;
    //TODO: criar novos grupos? ou um grupo unificado?
    // lembrar de desativar os grupos antigos
    public BookStackGroup stacks;
    public BallQueueGroup queues;
    public SpatialInteractable startButton;
    private bool playing = false;

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());

        //TODO: usar "yield return null;"
        yield return new WaitUntil(() => stacks.Lists != null && stacks.Lists.Count > 0 && stacks.Lists[0].Count > 0);

        startButton.onInteractEvent += HandleButton;
        stacks.GrabArea.onExitEvent += StopPlaying;
        queues.GrabArea.onExitEvent += StopPlaying;
        quest.quest.onCompletedEvent += StopPlaying;

        quest.AddTaskHandler(1,
            start: (task) =>
            {
                stacks.Clear();
                stacks.Populate(BookStackGroup.defaultBooks);
                queues.Clear();
                queues.Populate(BallQueueGroup.defaultItems);
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
        if (playing) StopPlaying();
        else StartPlaying();
    }
    void StartPlaying()
    {
        playing = true;
        foreach (var t in quest.quest.tasks)
        {
            GameTimer.Instance.labels.Add(t.name);
        }
        GameTimer.Instance.Begin();
        quest.Start();
        startButton.interactText = "Parar";
    }
    void StopPlaying()
    {
        stacks.Clear();
        stacks.Populate(BookStackGroup.defaultBooks);
        playing = false;
        GameTimer.Instance.Stop();
        quest.Reset();
        startButton.interactText = "Começar";
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
