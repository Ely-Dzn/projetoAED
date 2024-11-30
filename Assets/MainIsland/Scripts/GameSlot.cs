using System.Collections;
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
    public delegate void OnInteract(GameSlot slot);
    public OnInteract onInteract;

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
        onInteract?.Invoke(this);
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
    public GameObject Remove()
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
