using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BallGameItem : GameItem
{
    private Color color;
    public Color Color => color;
    public BallGameItem(Color color) : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Ball")))
    {
        this.color = color;
        var renderer = Transform.GetComponent<Renderer>();
        var mat = renderer.material;
        mat.color = color;
        renderer.material = mat;
        // Aumentar raio para melhorar a seleção
        Transform.GetComponentInChildren<SphereCollider>().radius *= 2;
    }
    public static BallGameItem MakeGhost()
    {
        var item = new BallGameItem(Color.clear);
        var renderer = item.GameObject.GetComponentInChildren<Renderer>(true);
        var transparent = AssetManager.Load<Material>("Materials/Transparent");
        renderer.materials = Enumerable.Repeat(transparent, renderer.materials.Length).ToArray();
        return item;
    }
}

[DisallowMultipleComponent]
public class TestQueueGroup : MonoBehaviour
{
    private bool playing = false;

    public TestQueue queue;
    public List<Color> colors;
    [SerializeField]
    private SpatialTriggerEvent grabArea;
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private GameObject mainBall;
    private LineRenderer mainBallLine;
    private SpatialInteractable mainBallInteractable;
    [SerializeField]
    private GameObject bowl;
    [SerializeField]
    private Transform bowlSpawn;
    private SpatialInteractable bowlInteractable;
    private Outline bowlOutline;
    private List<Transform> bowlItems = new();
    public Vector3 queueStartPos;
    public Transform hole;
    private Vector3 queueDirection;
    private BallGameItem grabbed = null;
    public BallGameItem Grabbed
    {
        get
        {
            return grabbed;
        }
        set
        {
            grabbed = value;
            //TODO
        }
    }
    private List<Transform> transit = new();
    [SerializeField]
    private RawImage colorDisplay;
    private Color nextColor;
    private Color NextColor
    {
        get => nextColor;
        set
        {
            nextColor = value;
            colorDisplay.color = value;
        }
    }
    private int collected = 0;
    private int moves = 0;
    private int round = 0;
    private float speed = 0;

    void Start()
    {
        queue.group = this;
        queue.grabArea = grabArea;
        queue.grabGroup = this;
        queue.OnInteractEvent += (_, _) => moves++;

        queue.BackGhost.Insert(BallGameItem.MakeGhost(), resetTransform: true);

        mainBallLine = mainBall.GetComponentInChildren<LineRenderer>();
        mainBallLine.positionCount = 2;
        mainBallLine.SetPosition(0, mainBall.transform.position);
        mainBallLine.SetPosition(1, mainBall.transform.position);
        mainBallLine.enabled = false;
        mainBallInteractable = mainBall.GetComponentInChildren<SpatialInteractable>();
        mainBallInteractable.onInteractEvent += StartPlaying;

        bowlInteractable = bowl.GetComponentInChildren<SpatialInteractable>();
        bowlInteractable.onInteractEvent += OnBowlInteract;
        bowlInteractable.enabled = false;

        bowlOutline = bowl.GetComponentInChildren<Outline>();
        bowlOutline.enabled = false;

        colorDisplay.enabled = false;

        grabArea.onExitEvent += StopPlaying;

        queueStartPos = queue.transform.position;

        queueDirection = (hole.position - queueStartPos).normalized;
    }

    private void Update()
    {
        for (var i = transit.Count - 1; i >= 0; i--)
        {
            var item = transit[i];
            //var rand = Random.insideUnitCircle * 0.25f;
            //item.transform.position = bowlSpawn.position + new Vector3(rand.x, 0, rand.y);
            item.position = Vector3.Lerp(item.position, bowlSpawn.position, 0.5f);
            if (Vector3.Distance(item.position, bowlSpawn.position) < 0.1f)
            {
                transit.RemoveAt(i);
                var col = item.GetComponentInChildren<SphereCollider>(true);
                col.enabled = true;
                col.gameObject.AddComponent<Rigidbody>();
                col.radius /= 2;
            }
        }

        if (!playing) return;

        queue.transform.position += speed * Time.deltaTime * queueDirection;
        // Caso esteja no buraco
        if (Vector3.Dot(queue.transform.position - hole.position, queueDirection) > 0)
        {
            var item = queue.Front;
            queue.Remove(queue.FrontSlot);
            item.Destroy();
            queue.transform.position -= queueDirection * 0.5f;
            if (queue.Count == 0)
            {
                AdvanceRound();
            }
        }

        if (Raycast.HasHit)
        {
            mainBallLine.SetPosition(1, Raycast.Hit.point);
        }

        if (Grabbed &&
            (!Utils.CanUseRaycast()
            || (Raycast.HasHit && Raycast.Hit.collider.gameObject == bowl)))
        {
            bowlOutline.enabled = true;
            bowlInteractable.enabled = true;
        }
        else
        {
            bowlInteractable.enabled = false;
            bowlOutline.enabled = false;
        }

        if (Grabbed)
        {
            Grabbed.Transform.position = Vector3.Lerp(Grabbed.Transform.position, mainBall.transform.position, 0.5f);
        }
    }

    private void OnBowlInteract()
    {
        if (!playing || !Grabbed) return;
        var item = Grabbed;
        if (item.Color == NextColor)
        {
            collected++;
            NextColor = GetNextColor();
        }
        else
        {
            speed += 0.02f;
        }
        queue.transform.position -= queueDirection * 0.25f;
        Debug.Log($"Coletados: {collected}, Movimentações: {moves}");
        Grabbed = null;
        bowlItems.Add(item.Transform);
        item.Transform.GetComponentInChildren<Outline>().enabled = false;
        item.Transform.SetParent(bowl.transform);
        item.GameObject.layer = Layers.IgnoreRaycast;
        transit.Add(item.Transform);

        if (queue.Count == 0)
        {
            AdvanceRound();
        }
    }

    private void StartPlaying()
    {
        if (playing) return;
        playing = true;
        mainBallInteractable.enabled = false;
        mainBallLine.enabled = true;
        colorDisplay.enabled = true;
        moves = 0;
        collected = 0;
        round = 0;
        AdvanceRound();
    }
    private void StopPlaying()
    {
        playing = false;
        round = 0;
        mainBallInteractable.enabled = true;
        mainBallLine.enabled = false;
        bowlInteractable.enabled = false;
        bowlOutline.enabled = false;
        colorDisplay.enabled = false;
        queue.Clear();
        if (Grabbed) Grabbed.Destroy();
        Grabbed = null;
        foreach (var item in bowlItems)
        {
            if (item) Destroy(item.gameObject);
        }
        bowlItems.Clear();
    }

    private void AdvanceRound()
    {
        if (round >= 4)
        {
            StopPlaying();
            return;
        }
        round++;
        speed = 0.1f + 0.1f * round;
        queue.transform.localPosition = Vector3.zero;
        var n = queue.MaxSize * round / 4;
        for (int i = 0; i < n; i++)
        {
            var item = new BallGameItem(GetRandomColor());
            queue.Push(item, resetTransform: true);
        }
        NextColor = GetNextColor();
    }

    private Color GetNextColor()
    {
        List<int> hasColors = new();
        foreach (var slot in queue.Slots)
        {
            if (!slot.IsFilled) continue;
            var renderer = slot.Item.GameObject.GetComponent<Renderer>();
            if (!renderer) continue;
            hasColors.Add(colors.FindIndex(c => c == renderer.material.color));
        }
        int size = hasColors.Count;
        if (size == 0) return colors[0];
        return colors[hasColors[Random.Range(0, size)]];
    }

    protected Color GetRandomColor()
    {
        return colors[Random.Range(0, colors.Count)];
    }
}
