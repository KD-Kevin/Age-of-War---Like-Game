using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RiptideNetworking;

public class SceneLoadManager : MonoBehaviour
{
    [SerializeField]
    private TipLocalizationManager TipManager;

    public static SceneLoadManager Instance;
    [SerializeField] CanvasGroup LoadCanvas;
    [SerializeField]
    private TMPro.TextMeshProUGUI TipText;
    [SerializeField]
    private TMPro.TextMeshProUGUI LoadingPercentText;
    [SerializeField]
    private Image LoadingPercentImage;
    private float _lerpTime = 2;
    private string _sceneName;
    private bool _loadingScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            //SceneManager.sceneLoaded += SceneLoaded;
            Application.targetFrameRate = 240;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        StartCoroutine(FadeCanvasOut());
    }

    public void LoadScene(string name, bool SendNetworkCall = false)
    {
        if (!_loadingScene)
        {
            _loadingScene = true;
            gameObject.SetActive(true);

            StartCoroutine(FadeCanvasIn());
            _sceneName = name;

            if (SendNetworkCall)
            {
                //Debug.Log("Change Scene");
                Message message = Message.Create(MessageSendMode.reliable, MessageId.ChangeScene);
                message.AddUShort(PlayerManager.Instance.LocalPlayer.PlayerID);
                message.AddString(name);
                NetworkManager.Instance.Client.Send(message);
            }
        }
    }

    [MessageHandler((ushort)MessageId.ChangeScene)]
    private static void LoadScene(ushort fromClientId, Message message)
    {
        //Debug.Log("Change Scene");
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.ChangeScene);
        string Scenename = message.GetString();
        messageToSend.AddUShort(newPlayerId);
        messageToSend.AddString(Scenename);
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            NetworkManager.Instance.Server.Send(messageToSend, player.PlayerID);
        }
    }

    [MessageHandler((ushort)MessageId.ChangeScene)]
    private static void SendConfirmation(Message message)
    {
        //Debug.Log("Change Scene");
        ushort newPlayerId = message.GetUShort();
        string Scenename = message.GetString();

        Instance.LoadScene(Scenename);
    }

    private IEnumerator FadeCanvasOut()
    {
        float timeLeft = _lerpTime;

        if (timeLeft == 0) // snap to alpha if duration is zero
        {
            LoadCanvas.alpha = 1;
        }

        while (timeLeft > 0)
        {
            float tempIntensity = Mathf.Lerp(1, 0, (1 - (timeLeft / _lerpTime)));
            LoadCanvas.alpha = tempIntensity;

            timeLeft = Mathf.Max(0, timeLeft - Time.deltaTime);
            yield return null;
        }
        LoadCanvas.alpha = 0;
    }

    private IEnumerator FadeCanvasIn()
    {
        float timeLeft = _lerpTime;

        if (timeLeft == 0) // snap to alpha if duration is zero
        {
            LoadCanvas.alpha = 0;
        }

        while (timeLeft > 0)
        {
            float tempIntensity = Mathf.Lerp(0, 1, (1 - (timeLeft / _lerpTime)));
            LoadCanvas.alpha = tempIntensity;

            timeLeft = Mathf.Max(0, timeLeft - Time.deltaTime);
            yield return null;
        }

        LoadCanvas.alpha = 1;

        //SceneManager.LoadSceneAsync(_sceneName);
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_sceneName);

        TipText.text = TipManager.GetLocalizations(LanguageSelector.CurrentLocalization).GetRandomTip();
        while (!asyncLoad.isDone)
        {
            int PercentageLoaded = Mathf.RoundToInt(asyncLoad.progress * 100);
            LoadingPercentText.text = PercentageLoaded + "%";
            LoadingPercentImage.fillAmount = asyncLoad.progress;

            yield return null;
        }

        LoadingPercentText.text = 100 + "%";
        LoadingPercentImage.fillAmount = 1;
        _loadingScene = false;
        gameObject.SetActive(false);
    }
}
