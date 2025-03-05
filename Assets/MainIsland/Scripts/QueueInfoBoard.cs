using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using SpatialSys.UnitySDK;

public class QueueInfoBoard : GameQueue<QueueInfoBoard.Item>
{
    public class Item : GameItem
    {
        private Color color;
        public Color Color => color;
        public Item(Color color) : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/InfoBoardItem")))
        {
            this.color = color;
            GameObject.GetComponentInChildren<Renderer>(true).material.color = color;
        }
        public static Item MakeGhost()
        {
            var item = new Item(Color.clear);
            var renderer = item.GameObject.GetComponentInChildren<Renderer>(true);
            var transparent = AssetManager.Load<Material>("Materials/Transparent");
            renderer.materials = Enumerable.Repeat(transparent, renderer.materials.Length).ToArray();
            return item;
        }
        bool animatingEntry = false;
        public IEnumerator AnimateEntry()
        {
            animatingEntry = true;
            Vector3 to = Transform.localPosition;
            Vector3 from = to + new Vector3(-1, 0, 0);
            float scaleTo = Transform.localScale.x;
            float time = 0;
            float duration = 0.3f;

            while (time < duration)
            {
                if (!animatingEntry) break;
                float pos = time / duration;
                Transform.localPosition = Vector3.Lerp(from, to, pos);
                Transform.localScale = Vector3.one * Mathf.Lerp(0, scaleTo, pos);
                time += Time.deltaTime;
                yield return null;
            }
            Transform.localPosition = to;
            Transform.localScale = Vector3.one * scaleTo;

            animatingEntry = false;
        }
        public IEnumerator AnimateDestroy()
        {
            while (animatingEntry) yield return null;

            Vector3 from = Transform.localPosition;
            Vector3 to = from + new Vector3(1, 0, 0);
            float scaleFrom = Transform.localScale.x;
            float time = 0;
            float duration = 0.3f;

            while (time < duration)
            {
                float pos = time / duration;
                Transform.localPosition = Vector3.Lerp(from, to, pos);
                Transform.localScale = Vector3.one * Mathf.Lerp(scaleFrom, 0, pos);
                time += Time.deltaTime;
                yield return null;
            }

            Destroy();
        }
    }

    public List<Color> colors;
    public SpatialInteractable pushButton;
    public SpatialInteractable popButton;
    public GameObject frontIndicator;
    public GameObject backIndicator;

    void Start()
    {
        StartCoroutine(AnimationLoop());
        frontIndicator.transform.position = Slots[0].transform.position;
        backIndicator.transform.position = Slots[0].transform.position;
    }

    new void Update()
    {
        base.Update();
        frontIndicator.SetActive(Count > 1);

        frontIndicator.transform.position = Vector3.Lerp(
            frontIndicator.transform.position,
            FrontSlot?.transform.position ?? Slots[0].transform.position,
            Mathf.Max(Time.deltaTime * 10f));
        backIndicator.transform.position = Vector3.Lerp(
            backIndicator.transform.position,
            BackSlot?.transform.position ?? Slots[0].transform.position,
            Mathf.Max(Time.deltaTime * 10f));
    }

    private IEnumerator AnimationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            bool push = Random.value > 0.5f;
            if (IsEmpty())
            {
                push = true;
            }
            if (IsFull())
            {
                push = false;
            }

            if (push)
            {
                HandlePush();
            }
            else
            {
                HandlePop();
            }
        }
    }

    void HandlePush()
    {
        var item = new Item(GetNextColor());
        Push(item, resetTransform: true);
        StartCoroutine(item.AnimateEntry());
    }
    void HandlePop()
    {
        var item = Front;
        Pop();
        if (item)
        {
            item.Transform.SetParent(transform, true);
            StartCoroutine(item.AnimateDestroy());
        }
    }

    Color GetRandomColor()
    {
        return colors[Random.Range(0, colors.Count)];
    }
    Color GetNextColor()
    {
        int i = Random.Range(0, colors.Count);
        for (int j = 0; j < colors.Count; j++)
        {
            var color = colors[i];
            var colorInUse = false;
            foreach (var slot in Slots)
            {
                if (slot.IsFilled && slot.Item.Color == color)
                {
                    colorInUse = true;
                    break;
                }
            }
            if (!colorInUse) break;
            i = (i + 1) % colors.Count;
        }
        return colors[i];
    }
}
