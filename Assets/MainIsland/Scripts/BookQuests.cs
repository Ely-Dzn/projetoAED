using System.Collections;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;
using UnityEngine.UI;

public class BookQuests : MonoBehaviour
{
    private QuestWrapper quest;
    private BookStackGroup group;
    [SerializeField]
    private GameObject colorsDisplay;
    private int[] task1Order = { 0, 2, 3, 4 };
    [SerializeField]
    private GameObject task1Anim;
    public SpatialInteractable startButton;
    private bool playing = false;

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());
        group = GetComponent<BookStackGroup>();

        //TODO: usar "yield return null;"
        yield return new WaitUntil(() => group.Lists != null && group.Lists.Count > 0 && group.Lists[0].Count > 0);

        startButton.onInteractEvent += HandleButton;
        group.GrabArea.onExitEvent += StopPlaying;
        quest.quest.onCompletedEvent += StopPlaying;

        // Colocar a primeira pilha na mesma ordem, sem o verde
        quest.AddTaskHandler(1,
            start: (task) =>
            {
                group.ResetBooks();
                group.PopulateBooks(BookStackGroup.defaultBooks);
                task1Anim.SetActive(true);
                for (int i = 0; i < colorsDisplay.transform.childCount; i++)
                {
                    var el = colorsDisplay.transform.GetChild(i).GetComponent<RawImage>();
                    if (i >= task1Order.Length)
                    {
                        el.enabled = false;
                    }
                    else
                    {
                        el.enabled = true;
                        el.color = group.colors[task1Order[i]];
                    }
                }
            },
            update: (task) =>
            {
                int bestProgress = 0;
                foreach (var stack in group.Lists)
                {
                    int progress = 0;
                    for (int i = 0; i < task1Order.Length; i++)
                    {
                        var item = stack.Slots[i].Item;
                        if (item && item.Color == group.colors[task1Order[i]])
                        {
                            progress++;
                        }
                    }
                    bestProgress = Mathf.Max(bestProgress, progress);
                }

                //task.progress = bestProgress;
                if (bestProgress == task1Order.Length)
                {
                    task.CompleteTask();
                    return;
                }
            },
            cleanup: (task) =>
            {
                task1Anim.SetActive(false);
            });

        // separar os livros azuis e vermelhos em duas pilhas
        quest.AddTaskHandler(2,
            start: (task) =>
            {
                group.ResetBooks();
                group.PopulateBooks(new int[][]{
                    new int[] { 0, 2, 2, 0 },
                    new int[] { 2, 0, 2, 2, 2 },
                    new int[] { 0, 0, 2, 0 },
                });
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
                    return;
                }
            });

        // separar três cores
        quest.AddTaskHandler(3,
            start: (task) =>
            {
                group.ResetBooks();
                group.PopulateBooks(new int[][]{
                    new int[] { 1, 0, 0 },
                    new int[] { 2, 2, 1, 1 },
                    new int[] { 2, 1, 0, 0 },
                });
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
                    return;
                }
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
        quest.quest.StartQuest();
        startButton.interactText = "Parar";
    }
    void StopPlaying()
    {
        group.ResetBooks();
        group.PopulateBooks(BookStackGroup.defaultBooks);
        playing = false;
        GameTimer.Instance.Stop();
        quest.quest.ResetQuest();
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
