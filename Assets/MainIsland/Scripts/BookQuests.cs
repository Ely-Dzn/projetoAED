using System.Collections;
using SpatialSys.UnitySDK;
using UnityEngine;
using UnityEngine.UI;

public class BookQuests : MonoBehaviour
{
    private QuestWrapper quest;
    private BookStackGroup group;
    [SerializeField]
    private GameObject colorsDisplay;
    private int[] task1Order = { 3, 1, 2, 4, 0 };
    public SpatialInteractable startButton;
    private bool playing = false;

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());
        group = GetComponent<BookStackGroup>();

        //TODO: usar "yield return null;"
        yield return new WaitUntil(() => group.Lists != null && group.Lists.Count > 0 && group.Lists[0].Count > 0);

        startButton.onInteractEvent += HandleButton;

        group.GrabArea.onExitEvent += () =>
        {
            if (playing)
            {
                HandleButton();
            }
        };

        // Separar o livro verde
        quest.AddTaskHandler(1,
            start: (task) =>
            {
                GameTimer.Instance.Begin();
                colorsDisplay.SetActive(false);
                foreach (var t in quest.quest.tasks)
                {
                    GameTimer.Instance.labels.Add(t.name);
                }
            },
            update: (task) =>
            {
                foreach (var stack in group.Lists)
                {
                    var item = stack.Slots[0].Item;
                    if (stack.Count == 1 && item.Color == group.colors[1])
                    {
                        task.CompleteTask();
                        return;
                    }
                }
            });

        quest.AddTaskHandler(2,
            start: (task) =>
            {
                colorsDisplay.SetActive(true);
                for (int i = 0; i < colorsDisplay.transform.childCount; i++)
                {
                    var el = colorsDisplay.transform.GetChild(i).GetComponent<RawImage>();
                    el.color = group.colors[task1Order[i]];
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

                task.progress = bestProgress;
                if (bestProgress == task1Order.Length)
                {
                    GameTimer.Instance.Stop();
                    playing = false;
                    task.CompleteTask();
                    return;
                }
            },
            cleanup: (task) =>
            {
                colorsDisplay.SetActive(false);
            });
    }

    void HandleButton()
    {
        if (playing)
        {
            playing = false;
            GameTimer.Instance.Stop();
            quest.quest.ResetQuest();
            startButton.interactText = "Começar";
        }
        else
        {
            group.ResetBooks();
            playing = true;
            quest.quest.StartQuest();
            startButton.interactText = "Parar";
        }
    }
    void Update()
    {
        if (!playing || quest.quest.status != QuestStatus.InProgress) return;

        quest.Update();
    }
}
