using UnityEngine;

public class GameItem
{
    public GameObject GameObject { get; protected set; }
    public Transform Transform => GameObject.transform;
    public GameItem(GameObject go)
    {
        GameObject = go;
    }
    public void Destroy()
    {
        GameObject.Destroy(GameObject);
    }
    public static implicit operator bool(GameItem item)
    {
        return item is not null;
    }

}
