using AgeOfWar.Core;
using UnityEngine;

namespace AgeOfWar.UI
{
    public class EnterUsernamePage : MonoBehaviour
    {
        private string SetUserName = "No Name";

        public void OpenPage()
        {
            gameObject.SetActive(true);
        }

        public void SubmitButtopnPressed()
        {
            if (SetUserName == "No Name" || string.IsNullOrEmpty(SetUserName))
            {
                return;
            }

            PlayerManager.Instance.LocalPlayerData.Data.UserName = SetUserName;
            gameObject.SetActive(false);
        }

        public void SetName(string Username)
        {
            SetUserName = Username;
        }
    }
}
