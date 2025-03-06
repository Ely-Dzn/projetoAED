using System.Collections;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;
using UnityEngine.UI;

public class BookQuests : MonoBehaviour
{
    private const string QUEST_ID = "book_stack";

    private QuestWrapper quest;
    private BookStackGroup group;
    [SerializeField]
    private GameObject task1Anim;
    public ToggleButton startButton;
    private bool playing = false;
    private SequenceGuide[] sequenceGuides;

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());
        group = GetComponent<BookStackGroup>();

        yield return new WaitUntil(() => group.Lists != null && group.Lists.Count > 0 && group.Lists[0].Count > 0);

        sequenceGuides = new SequenceGuide[group.Lists.Count];
        for (int i = 0; i < group.Lists.Count; i++)
        {
            sequenceGuides[i] = group.Lists[i].gameObject.GetComponentInChildren<SequenceGuide>(true);
        }

        startButton.onToggle += HandleButton;
        group.GrabArea.onExitEvent += StopPlaying;
        quest.quest.onCompletedEvent += StopPlaying;

        // Colocar a primeira pilha na mesma ordem, sem o verde
        int[] task1Order = { 0, 2, 3, 4 };
        quest.AddTaskHandler(1,
            start: (task) =>
            {
                group.Clear();
                group.Populate(BookStackGroup.defaultBooks);
                task1Anim.SetActive(true);
                sequenceGuides[0].Display(task1Order, group.colors);
            },
            update: (task) =>
            {
                bool anyOk = false;
                int bestProgress = 0;
                foreach (var stack in group.Lists)
                {
                    var (ok, count) = Utils.CompareColorSequence(stack, task1Order, group.colors);
                    anyOk |= ok;
                    bestProgress = Mathf.Max(bestProgress, count);
                }

                //task.progress = bestProgress;
                if (anyOk) task.CompleteTask();
            },
            cleanup: (task) =>
            {
                ClearGuides();
                task1Anim.SetActive(false);
            });

        // separar os livros azuis e vermelhos em duas pilhas
        quest.AddTaskHandler(2,
            start: (task) =>
            {
                group.Clear();
                group.Populate(new int[][]{
                    new int[] { 0, 2, 2, 0 },
                    new int[] { 2, 0, 2, 2, 2 },
                    new int[] { 0, 0, 2, 0 },
                });
                sequenceGuides[0].Display(new int[] { 2, 2, 2, 2, 2, 2, 2 }, group.colors);
                sequenceGuides[2].Display(new int[] { 0, 0, 0, 0, 0, 0 }, group.colors);
            },
            update: (task) =>
            {
                if (GrabManager.Grabbed) return;

                var colorRed = group.colors[0];
                var colorBlue = group.colors[2];
                int[] red = new int[] { 0, 0, 0 };
                int[] blue = new int[] { 0, 0, 0 };
                for (int i = 0; i < group.Lists.Count; i++)
                {
                    var stack = group.Lists[i];
                    for (int j = 0; j < stack.Count; j++)
                    {
                        if (stack.Slots[j].Item.Color == colorRed)
                            red[i]++;
                        if (stack.Slots[j].Item.Color == colorBlue)
                            blue[i]++;
                    }
                }

                task.progress = red.Max() + blue.Max();
                var intersect = false;
                var empty = 0;
                for (int i = 0; i < group.Lists.Count; i++)
                {
                    intersect |= red[i] > 0 && blue[i] > 0;
                    empty += red[i] + blue[i] == 0 ? 1 : 0;
                }
                if (!intersect && empty == 1)
                {
                    task.CompleteTask();
                }
            },
            cleanup: (task) =>
            {
                ClearGuides();
            });

        // separar três cores
        quest.AddTaskHandler(3,
            start: (task) =>
            {
                group.Clear();
                group.Populate(new int[][]{
                    new int[] { 1, 0, 0 },
                    new int[] { 2, 2, 1, 1 },
                    new int[] { 2, 1, 0, 0 },
                });
                sequenceGuides[0].Display(new int[] { 0, 0, 0, 0 }, group.colors);
                sequenceGuides[1].Display(new int[] { 1, 1, 1, 1 }, group.colors);
                sequenceGuides[2].Display(new int[] { 2, 2, 2 }, group.colors);
            },
            update: (task) =>
            {
                if (GrabManager.Grabbed) return;

                var colorRed = group.colors[0];
                var colorBlue = group.colors[2];
                var colorGreen = group.colors[1];
                int[] red = new int[] { 0, 0, 0 };
                int[] blue = new int[] { 0, 0, 0 };
                int[] green = new int[] { 0, 0, 0 };
                for (int i = 0; i < group.Lists.Count; i++)
                {
                    var stack = group.Lists[i];
                    for (int j = 0; j < stack.Count; j++)
                    {
                        if (stack.Slots[j].Item.Color == colorRed)
                            red[i]++;
                        if (stack.Slots[j].Item.Color == colorBlue)
                            blue[i]++;
                        if (stack.Slots[j].Item.Color == colorGreen)
                            green[i]++;
                    }
                }

                task.progress = red.Max() + blue.Max() + green.Max();
                var intersect = false;
                for (int i = 0; i < group.Lists.Count; i++)
                {
                    intersect |= red[i] > 0 && blue[i] > 0;
                    intersect |= red[i] > 0 && green[i] > 0;
                    intersect |= blue[i] > 0 && green[i] > 0;
                }
                if (!intersect)
                {
                    task.CompleteTask();
                }
            },
            cleanup: (task) =>
            {
                ClearGuides();
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
        group.Populate(BookStackGroup.defaultBooks);
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
