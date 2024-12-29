using SpatialSys.UnitySDK;
using System.Collections.Generic;
using System.Linq;
using System;

using Task = SpatialSys.UnitySDK.SpatialQuest.Task;

public class QuestWrapper
{
    class TaskHandler
    {
        public readonly Task task;
        public Action<Task> OnStart { get; }
        public Action<Task> OnUpdate { get; }
        public Action<Task> OnFinish { get; }
        public void Start() => OnStart?.Invoke(task);
        public void Update() => OnUpdate?.Invoke(task);
        public void Finish() => OnFinish?.Invoke(task);

        public TaskHandler(Task task,
            Action<Task> onStart = null,
            Action<Task> onUpdate = null,
            Action<Task> onFinish = null)
        {
            this.task = task;
            OnStart = onStart;
            OnUpdate = onUpdate;
            OnFinish = onFinish;
            task.onStartedEvent += Start;
            task.onCompletedEvent += Finish;
        }
    }

    public readonly SpatialQuest quest;
    private readonly Dictionary<uint, TaskHandler> handlers = new();

    public Task ActiveTask
    {
        get
        {
            return quest.tasks
                .Where(task => task.status == QuestStatus.InProgress)
                .FirstOrDefault();
        }
    }

    public QuestWrapper(SpatialQuest quest)
    {
        this.quest = quest;
    }
    public void AddTaskHandler(uint id,
        Action<Task> start = null,
        Action<Task> update = null,
        Action<Task> finish = null)
    {
        try
        {
            var task = quest.tasks.Where(task => task.id == id).First();
            var handler = new TaskHandler(task, start, update, finish);
            handlers[id] = handler;
        }
        catch
        {
            throw new Exception($"Task id {id} não existe na quest '{quest.name}'");
        }
    }

    public void Update()
    {
        if (quest.status != QuestStatus.InProgress) return;
        var task = ActiveTask;
        if (task == null) return;
        TaskHandler handler;
        if (handlers.TryGetValue(task.id, out handler))
        {
            handler.Update();
        }
    }
}
