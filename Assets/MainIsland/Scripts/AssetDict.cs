using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssetDictItem
{
    public string name;
    public UnityEngine.Object value;
}

[CreateAssetMenu(fileName = "AssetDict", menuName = "ScriptableObjects/AssetDict", order = 0)]
public class AssetDict : ScriptableObject
{
    [SerializeField]
    public List<AssetDictItem> assets;
}
