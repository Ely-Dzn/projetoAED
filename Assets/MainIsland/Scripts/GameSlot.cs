using SpatialSys.UnitySDK;
using UnityEngine;

public class GameSlot : MonoBehaviour
{
    public GameList Parent;
    public int index;
    [field: SerializeField]
    public GameObject Item { get; protected set; }
    public bool IsFilled => Item != null;
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

    public void Insert(GameObject item, bool resetTransform = false)
    {
        Item = item;
        if (resetTransform)
            item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        item.transform.SetParent(transform, !resetTransform);
        if (outline == null)
            outline = GetComponentInChildren<Outline>(true);
    }
    public GameObject Extract()
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
