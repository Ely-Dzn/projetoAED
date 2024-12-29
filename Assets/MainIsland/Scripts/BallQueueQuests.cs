using SpatialSys.UnitySDK;
using UnityEngine;

public class BallQueueQuests : MonoBehaviour
{
    private QuestWrapper quest;
    private BallQueueGroup group;
    [SerializeField]
    private GameObject colorsDisplay;

    void Awake()
    {
        quest = new QuestWrapper(GetComponent<SpatialQuest>());
        group = GetComponent<BallQueueGroup>();

        quest.AddTaskHandler(id: 1,
            start: (task) => { },
            update: (task) => { },
            finish: (task) => { });
    }

    private void Update()
    {
        quest.Update();
    }
}