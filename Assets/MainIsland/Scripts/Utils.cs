using System;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;

public interface IColoredItem
{
    Color Color { get; }
}

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
            if (!child.TryGetComponent<T>(out var comp)) continue;
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

    public static IEnumerable<Color> GetColorSequence<T>(GameList<T> list)
        where T : GameItem, IColoredItem
    {
        foreach (var slot in list.Slots)
        {
            if (slot.IsFilled)
            {
                yield return slot.Item.Color;
            }
        }
    }
    public static (bool ok, int count) CompareSequence<T>(
        IEnumerable<T> sequence1,
        IEnumerable<T> sequence2)
    {
        var seq1 = sequence1.GetEnumerator();
        var seq2 = sequence2.GetEnumerator();
        var count = 0;
        while (seq1.MoveNext() && seq2.MoveNext())
        {
            count++;
            if (!seq1.Current.Equals(seq2.Current))
            {
                return (false, count);
            }
        }
        if (seq1.MoveNext() || seq2.MoveNext())
        {
            return (false, count);
        }
        return (true, count);
    }
    public static (bool ok, int count) CompareColorSequence<T>(
        GameList<T> list,
        IEnumerable<int> sequence,
        IList<Color> colors
    ) where T : GameItem, IColoredItem
    {
        return CompareSequence(
            GetColorSequence(list),
            sequence.Select(colorId => colors[colorId]));
    }
    public static (bool ok, int count) CompareColorSequences<T>(
        IEnumerable<GameList<T>> lists,
        IEnumerable<IEnumerable<int>> sequences,
        IList<Color> colors
    ) where T : GameItem, IColoredItem
    {
        var list = lists.GetEnumerator();
        var seq = sequences.GetEnumerator();
        var total = 0;
        var allOk = true;
        while (list.MoveNext() && seq.MoveNext())
        {
            var (ok, count) = CompareColorSequence(list.Current, seq.Current, colors);
            total += count;
            allOk &= ok;
        }
        return (allOk, total);
    }

    public static bool CanUseRaycast()
    {
        return SpatialSys.UnitySDK.SpatialBridge.actorService.localActor.platform == SpatialSys.UnitySDK.SpatialPlatform.Web;
    }
}