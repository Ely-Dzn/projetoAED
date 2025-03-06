using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SequenceGuide : MonoBehaviour
{
    [SerializeField]
    GameObject prefab;
    [SerializeField]
    Vector3 offset;

    public void Display(IEnumerable<Color> sequence)
    {
        ClearChildren();
        Vector3 pos = Vector3.zero;
        foreach (var color in sequence)
        {
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = pos;
            go.GetComponent<Renderer>().material.color = color;
            pos += offset;
        }
    }
    public void Display(IEnumerable<int> sequence, IList<Color> colors)
    {
        Display(sequence.Select(i => colors[i]));
    }
    public void Clear()
    {
        ClearChildren();
    }
    void ClearChildren()
    {
        Utils.ClearChildren(transform);
    }
}
