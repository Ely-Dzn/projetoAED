using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;

public class MessageDisplay : MonoBehaviour
{
    //TODO: renomear "main" e permitir múltiplas instâncias
    public static MessageDisplay Instance { get; private set; }

    [SerializeField]
    private GameObject messagePrefab;
    [SerializeField]
    private GameObject messageObject;
    [SerializeField]
    private TMP_Text tmptext;
    [SerializeField]
    private Transform target = null;

    private void Awake()
    {
        Instance = this;

        messagePrefab = AssetManager.Load<GameObject>("Prefabs/Warning");

        messageObject = Instantiate(messagePrefab);
        messageObject.transform.SetParent(transform);
        tmptext = messageObject.GetComponentInChildren<TextMeshProUGUI>();

        messageObject.SetActive(false);
    }

    public void ShowWarning(string text, Transform target)
    {
        ClearMessage();
        this.target = target;
        tmptext.text = text;
        var player = SpatialBridge.actorService.localActor.avatar;
        var dir = player.position - target.position;
        dir.y = 0;
        dir.Normalize();
        messageObject.transform.SetPositionAndRotation(
            target.position + dir,
            Quaternion.LookRotation(-dir));
        messageObject.SetActive(true);
        Invoke(nameof(ClearMessage), 2f);
    }
    private void ClearMessage()
    {
        CancelInvoke(nameof(ClearMessage));
        if (target != null)
        {
            messageObject.SetActive(false);
            target = null;
        }
    }
}