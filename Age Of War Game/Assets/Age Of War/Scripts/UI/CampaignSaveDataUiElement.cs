using UnityEngine;
using TMPro;

namespace AgeOfWar.UI
{
    public class CampaignSaveDataUiElement : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI CampaignText;
        private CampaignSaveData SaveData;
        public CampaignSaveData CurrentSaveData
        {
            get
            {
                return SaveData;
            }
            set
            {
                SaveData = value;
                UpdateUi();
            }
        }

        private void UpdateUi()
        {
            CampaignText.text = SaveData.SaveDataTitle;
        }

        public void OnClick()
        {
            CampaignPage.Instance.LoadSaveData(SaveData);
        }
    }
}
