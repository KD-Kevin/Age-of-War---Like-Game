using AgeOfWar.Networking;
using UnityEngine;

namespace AgeOfWar.UI
{
    public class CountdownScript : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI CountdownText;

        // Update is called once per frame
        void Update()
        {
            if (LockstepManager.Instance.SecondsTillReconnect <= -1)
            {
                gameObject.SetActive(false);
            }

            CountdownText.text = $"{LockstepManager.Instance.SecondsTillReconnect}";
        }
    }
}
