using TMPro;
using UnityEngine;

public class DebugUtils : MonoBehaviour
{
    private static TMP_Text text;

    //[SuppressMessage("ApiDesign", "RS0030")]
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    public static void Log(string message)
    {
        if (!text) return;
        text.text = message;
    }
}
