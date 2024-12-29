using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }
    [SerializeField]
    private AssetDict assetDict;
    private Dictionary<string, Object> dict = new();
    public bool Loaded { get; private set; } = false;

    void Awake()
    {
        Instance = this;
        foreach (var item in assetDict.assets)
        {
            dict.Add(item.name, item.value);
        }
        Loaded = true;
    }

    public static T Load<T>(string key) where T : Object
    {
#if UNITY_EDITOR
        if (!Instance || !Instance.Loaded)
        {
            Debug.LogError("AssetManager não foi carregado ainda");
        }
        if (!Instance.dict.ContainsKey(key))
        {
            Debug.LogError($"Chave \"{key}\" não encontrada no AssetManager");
        }
#endif
        return Instance.dict[key] as T;
    }

    public static bool Has(string key)
    {
        return Instance.dict.ContainsKey(key);
    }
}
