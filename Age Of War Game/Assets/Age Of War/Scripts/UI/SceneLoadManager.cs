using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RiptideNetworking;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet;
using FishNet.Object;
using AgeOfWar.Data;
using AgeOfWar.Core;

namespace AgeOfWar.UI
{
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
        private static bool RegisteredBroadcasts = false;

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
                    LoadSceneBroadcast SceneLoadBroadcast = new LoadSceneBroadcast()
                    {
                        SentFromPlayer = PlayerManager.Instance.LocalPlayer.PlayerID,
                        SceneName = name,
                    };

                    InstanceFinder.ClientManager.Broadcast(SceneLoadBroadcast);
                }
            }
        }

        #region Load Scene RPC

        #region Fishnet

        public void LoadSceneBroadcast_Server(NetworkConnection Conn, LoadSceneBroadcast Broadcast)
        {
            NetworkObject nob = Conn.FirstObject;
            if (nob == null)
            {
                return;
            }
            InstanceFinder.ServerManager.Broadcast(nob, Broadcast);
        }

        public void LoadSceneBroadcast_Client(LoadSceneBroadcast Broadcast)
        {
            if (Broadcast.SentFromPlayer != PlayerManager.Instance.LocalPlayer.PlayerID)
            {
                LoadScene(Broadcast.SceneName);
            }
        }

        #endregion

        #endregion

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

        public void InitializeBroadcasts()
        {
            if (InstanceFinder.NetworkManager != null && !RegisteredBroadcasts)
            {
                RegisteredBroadcasts = true;
                InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<LoadSceneBroadcast>(LoadSceneBroadcast_Server);
                InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<LoadSceneBroadcast>(LoadSceneBroadcast_Client);
            }
        }
    }

    public struct LoadSceneBroadcast : IBroadcast
    {
        public ushort SentFromPlayer;
        public string SceneName;
    }
}
