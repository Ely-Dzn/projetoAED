using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class ListGroup<L> : MonoBehaviour where L : GameList
{
    [SerializeField]
    protected GameObject container;
    public List<L> Lists { get; protected set; }
    public event GameList.InteractHandler OnInteract;

    protected void Start()
    {
        Lists = Utils.GetChildren<L>(container.transform);

        foreach (var list in Lists)
        {
            list.OnInteractEvent += OnInteract;
        }
    }
}
