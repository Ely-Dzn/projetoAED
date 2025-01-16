using SpatialSys.UnitySDK;
using System.Collections.Generic;
using System.Linq;
using System;

using Task = SpatialSys.UnitySDK.SpatialQuest.Task;
using UnityEngine;

public class QuestWrapper
{
    public class TaskHandler
    {
        public readonly Task task;
        private bool running = false;
        private readonly Action<Task> onStart;
        private readonly Action<Task> onUpdate;
        private readonly Action<Task> onFinish;
        private readonly Action<Task> onCleanup;
        public void Start() => onStart?.Invoke(task);
        public void Update()
        {
            if (!running)
            {
                running = true;
                Start();
            }
            onUpdate?.Invoke(task);
        }
        public void Finish() => onFinish?.Invoke(task);
        public void Cleanup() => onCleanup?.Invoke(task);

        public TaskHandler(
            QuestWrapper quest,
            Task task,
            Action<Task> onStart = null,
            Action<Task> onUpdate = null,
            Action<Task> onFinish = null,
            Action<Task> onCleanup = null)
        {
            this.task = task;
            this.onStart = onStart;
            this.onUpdate = onUpdate;
            this.onFinish = onFinish;
            this.onCleanup = onCleanup;

            task.onStartedEvent += Start;
            task.onCompletedEvent += () =>
            {
                Finish();
                Cleanup();
            };
            quest.quest.onResetEvent += () =>
            {
                if (running) Cleanup();
            };
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

        foreach (var task in quest.tasks)
        {
            task.onStartedEvent += new Action(() =>
            {
                Debug.Log($"Task {task.name} started");
            });
            task.onCompletedEvent += new Action(() =>
            {
                Debug.Log($"Task {task.name} completed");
            });
        }
    }
    public void AddTaskHandler(
        uint id,
        Action<Task> start = null,
        Action<Task> update = null,
        Action<Task> finish = null,
        Action<Task> cleanup = null)
    {
        try
        {
            var task = quest.tasks.Where(task => task.id == id).First();
            var handler = new TaskHandler(this, task, start, update, finish, cleanup);
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
