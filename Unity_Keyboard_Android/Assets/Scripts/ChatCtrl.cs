using Proxima;
using TMPro;
using UMI;
using UnityEngine;
using UnityEngine.UI;

public class ChatCtrl : MonoBehaviour
{
    public Canvas canvas;
    public TextMeshProUGUI text;
    public TMP_InputField inputField;
    private MobileInputField mobileInputField;
    public Button bgButton;
    public Button sendButton;

    private RectTransform rectTransform;

    private RectTransform panelRect;

    void Awake()
    {
        MobileInput.Init();
        MobileInput.OnKeyboardAction += OnKeyboardAction;
        panelRect = transform.parent as RectTransform;
    }

    void OnDestroy()
    {
        MobileInput.OnKeyboardAction -= OnKeyboardAction;
        mobileInputField.OnReturnPressed -= SendMessage;
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mobileInputField = inputField.GetComponent<MobileInputField>();
        mobileInputField.OnReturnPressed += SendMessage;
        mobileInputField.IsManualHideControl = true;
        sendButton.onClick.AddListener(SendMessage);
        bgButton.onClick.AddListener(HideKeyboard);
    }

    void OnKeyboardAction(bool isShow, int height)
    {
        Debug.Log($"OnKeyboardAction: {isShow} {height} panelRect:{panelRect.rect}");
        UpdatePanelPosition(height);
    }

    void Update()
    {
        Debug.Log($"Update panelRect:{panelRect.rect}");
    }

    void SendMessage()
    {
        string message = mobileInputField.Text;
        text.text = message;
        Debug.Log($"SendMessage: {message}");
        mobileInputField.Text = "";
        inputField.text = "";
        mobileInputField.SetFocus(false);
    }

    void HideKeyboard()
    {
        mobileInputField.SetFocus(false);
    }

    public float GetKeyboardHeight()
    {
#if UNITY_EDITOR
        return 0f;
#elif UNITY_ANDROID
    using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
        using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
        {
            View.Call("getWindowVisibleDisplayFrame", rect);
            return ((float)(Screen.height - rect.Call<int>("height"))) * canvas.scaleFactor;
        }
    }
#elif UNITY_IOS
    return (float)TouchScreenKeyboard.area.height * canvas.scaleFactor;
#else
    return 0f;
#endif
    }

    private float lastKeyboardHeight = 0;

    private void UpdatePanelPosition(float keyboardHeight)
    {
        if (lastKeyboardHeight == keyboardHeight)
        {
            return;
        }
        lastKeyboardHeight = keyboardHeight;
        rectTransform.anchoredPosition = new Vector2(0, keyboardHeight);
    }

    public int proximPort { get; set; } = 7759;
    public string proximPassword { get; set; } = "123456";

    public void OpenConsole()
    {
        var objGameStart = GameObject.Find("GameStart");
        var objProxima = new GameObject("Proxima");
        objProxima.transform.SetParent(objGameStart.transform);

        var proxima = objProxima.AddComponent<ProximaInspector>();
        proxima.Port = proximPort;
        proxima.Password = proximPassword;
        proxima.StartOnEnable = true;
        proxima.InstantiateStatusUI = true;
        proxima.InstantiateConnectUI = true;
        proxima.Run();
    }

    public void CloseConsole()
    {
        var objProxima = GameObject.Find("GameStart/Proxima");
        if (objProxima != null)
        {
            var proxima = objProxima.GetComponent<ProximaInspector>();
            if (proxima != null)
            {
                proxima.Stop();
            }
            GameObject.Destroy(objProxima);
        }
    }
}
