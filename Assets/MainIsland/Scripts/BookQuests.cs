using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

public class BookQuests : MonoBehaviour
{
    private SpatialQuest quest;
    private BookStackGroup stacks;
    private IQuestTask currentTask;
    private int currentTaskIdx;
    private List<GameObject> initialBooks;
    [SerializeField]
    private GameObject colorsDisplay;
    private int[] task1Order = { 3, 1, 2, 4, 0 };

    IEnumerator Start()
    {
        quest = GetComponent<SpatialQuest>();
        stacks = GetComponent<BookStackGroup>();
        yield return new WaitUntil(() => stacks.Stacks != null && stacks.Stacks.Count > 0 && stacks.Stacks[0].Count > 0);
        initialBooks = new();
        foreach (var slot in stacks.Stacks[0].Slots)
        {
            initialBooks.Add(slot.Item);
        }
        currentTask = quest.quest.tasks[0];
        currentTaskIdx = 0;
    }

    void Update()
    {
        // Inicializar a lista aqui pois o BookStackGroup n�o estava pronto no Start
        if (initialBooks == null) return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            NextTask();
        }

        if (currentTask == null) return;

        switch (currentTask.id)
        {
            case 1:
                {
                    foreach (var stack in stacks.Stacks)
                    {
                        if (stack.Count == 1 && stack.Slots[0].Item == initialBooks[1])
                        {
                            NextTask();
                        }
                    }
                    break;
                }
            case 2:
                {
                    //TODO: mover para uuma fun��o de inicializa��o
                    colorsDisplay.SetActive(true);
                    for (int i = 0; i < colorsDisplay.transform.childCount; i++)
                    {
                        var el = colorsDisplay.transform.GetChild(i).GetComponent<RawImage>();
                        el.color = stacks.colors[task1Order[i]];
                    }

                    foreach (var stack in stacks.Stacks)
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
                        }
                    }
                    break;
                }
        }

        DebugUtils.Log(currentTask.name
            + " " + currentTask.progress
            + "/" + currentTask.progressSteps
            + " " + currentTask.status);
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
        if (currentTask == null) return;
    }
}
