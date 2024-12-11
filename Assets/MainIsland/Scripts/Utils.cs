using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

public static class Utils
{
    public static bool IsInteractableInRange(SpatialInteractable interactable, Vector3 source)
    {
        return Vector3.Distance(source, interactable.transform.position) <= interactable.interactiveRadius;
    }
    public static bool IsInteractableInRange(SpatialInteractable interactable, IAvatar source)
    {
        return IsInteractableInRange(interactable, source.position);
    }
    public static bool IsInteractableInRange(SpatialInteractable interactable)
    {
        return IsInteractableInRange(interactable, SpatialBridge.actorService.localActor.avatar);
    }

    public static List<GameObject> GetChildren(Transform parent)
    {
        var children = new List<GameObject>();
        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i).gameObject);
        }
        return children;
    }

    public static List<T> GetChildren<T>(Transform parent)
    {
        var children = new List<T>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            T comp = child.GetComponent<T>();
            if (comp == null) continue;
            children.Add(comp);
        }
        return children;
    }

    public static void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static Quaternion QuaternionFromEuler(Vector3 euler)
    {
        return Quaternion.Euler(euler.x, euler.y, euler.z);
    }
}