using System.Collections;
using SpatialSys.UnitySDK;
using UnityEngine;
using UnityEngine.UI;

public class BookQuests : MonoBehaviour
{
    private QuestWrapper quest;
    private BookStackGroup stacks;
    [SerializeField]
    private GameObject colorsDisplay;
    private int[] task1Order = { 3, 1, 2, 4, 0 };
    private bool playing = false;

    IEnumerator Start()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());
        stacks = GetComponent<BookStackGroup>();

        //TODO: usar "yield return null;"
        yield return new WaitUntil(() => stacks.Lists != null && stacks.Lists.Count > 0 && stacks.Lists[0].Count > 0);

        stacks.OnInteractEvent += HandleStart;

        // Separar o livro verde para começar o timer
        quest.AddTaskHandler(1,
            start: (task) =>
            {
                colorsDisplay.SetActive(false);
                foreach (var x in quest.quest.tasks)
                {
                    GameTimer.Instance.labels.Add(x.name);
                }
            },
            update: (task) =>
            {
                foreach (var stack in stacks.Lists)
                {
                    var item = stack.Slots[0].Item;
                    if (stack.Count == 1 && item.Color == stacks.colors[1])
                    {
                        task.CompleteTask();
                        return;
                    }
                }
            },
            finish: (task) =>
            {
                GameTimer.Instance.Begin();
            });

        quest.AddTaskHandler(2,
            start: (task) =>
            {
                colorsDisplay.SetActive(true);
                for (int i = 0; i < colorsDisplay.transform.childCount; i++)
                {
                    var el = colorsDisplay.transform.GetChild(i).GetComponent<RawImage>();
                    el.color = stacks.colors[task1Order[i]];
                }
            },
            update: (task) =>
            {
                foreach (var stack in stacks.Lists)
                {
                    int progress = 0;
                    for (int i = 0; i < task1Order.Length; i++)
                    {
                        var item = stack.Slots[i].Item;
                        if (item && item.Color == stacks.colors[task1Order[i]])
                        {
                            progress++;
                        }
                    }
                    task.progress = progress;
                    if (progress == 5)
                    {
                        task.CompleteTask();
                        return;
                    }
                }
            },
            finish: (task) =>
            {
                GameTimer.Instance.Stop();
                colorsDisplay.SetActive(false);
                playing = false;
            });
    }

    void HandleStart()
    {
        playing = true;
        quest.quest.StartQuest();
        stacks.OnInteractEvent -= HandleStart;
    }
    void Update()
    {
        if (!playing || quest.quest.status != QuestStatus.InProgress) return;

        quest.Update();
    }
}
