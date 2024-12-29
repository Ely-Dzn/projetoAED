using SpatialSys.UnitySDK;
using UnityEngine;

public class GameSlot : MonoBehaviour
{
    public GameList Parent;
    public int index;
    [field: SerializeField]
    public GameItem Item { get; protected set; }
    public GameObject GameObject => Item.GameObject;
    public Transform Transform => Item.GameObject.transform;
    public bool IsFilled => GameObject != null;
    public SpatialInteractable interactable;
    public Outline outline;
    public delegate void InteractHandler(GameSlot slot);
    public event InteractHandler OnInteractEvent;

    void Start()
    {
        if (interactable == null)
            interactable = GetComponentInChildren<SpatialInteractable>(true);
        if (interactable != null)
            interactable.onInteractEvent += SendOnInteract;
        outline = GetComponentInChildren<Outline>(true);
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.onInteractEvent -= SendOnInteract;
    }

    void SendOnInteract()
    {
        OnInteractEvent?.Invoke(this);
    }

    public void Insert(GameItem item, bool resetTransform = false)
    {
        Item = item;
        if (resetTransform)
            item.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        item.Transform.SetParent(transform, !resetTransform);
        if (outline == null)
            outline = GetComponentInChildren<Outline>(true);
    }
    public void Insert(GameObject go, bool resetTransform = false)
    {
        Insert(new GameItem(go), resetTransform: resetTransform);
    }

    public GameItem Extract()
    {
        var oldItem = Item;
        Item = null;
        if (outline != null && outline.gameObject != gameObject)
            outline = null;
        return oldItem;
    }

    void Reset()
    {
        Parent = GetComponentInParent<GameStack>();
    }
}
