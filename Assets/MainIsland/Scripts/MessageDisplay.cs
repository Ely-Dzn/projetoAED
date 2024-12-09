using System;
using System.Collections;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;

public class MessageDisplay : MonoBehaviour
{
    private static MessageDisplay INSTANCE;

    private static GameObject messagePrefab;
    private static GameObject canvas;
    private static TMP_Text tmptext;
    private static Transform target = null;

    private void Start()
    {
        INSTANCE = this;

        messagePrefab = AssetManager.Load<GameObject>("Prefabs/Warning");

        canvas = Instantiate(messagePrefab);
        tmptext = canvas.GetComponentInChildren<TextMeshProUGUI>();

        canvas.SetActive(false);
    }

    public static void ShowWarning(String text, Transform target)
    {
        INSTANCE.ClearMessage();
        MessageDisplay.target = target;
        tmptext.text = text;
        var player = SpatialBridge.actorService.localActor.avatar;
        var dir = player.position - target.position;
        dir.y = 0;
        dir.Normalize();
        canvas.transform.SetPositionAndRotation(
            target.position + dir,
            Quaternion.LookRotation(-dir));
        canvas.SetActive(true);
        INSTANCE.Invoke(nameof(ClearMessage), 2f);
    }
    private void ClearMessage()
    {
        CancelInvoke(nameof(ClearMessage));
        if (target != null)
        {
            canvas.SetActive(false);
            target = null;
        }
    }
}