using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using TMPro;
using BitStrap;

namespace AgeOfWar.UI
{
    public class UpdateWindowManagerUi : MonoBehaviour
    {
        [SerializeField]
        private WindowManager Manager;

        [Button]
        public void UpdateValues()
        {
            foreach (WindowManager.WindowItem item in Manager.windows)
            {
                item.windowObject.gameObject.name = item.windowName + " Window";
                item.buttonObject.gameObject.name = item.windowName + " Button";

                TextMeshProUGUI[] Texts = item.buttonObject.GetComponentsInChildren<TextMeshProUGUI>();

                foreach (TextMeshProUGUI Text in Texts)
                {
                    Text.text = item.windowName;
                }
            }
        }
    }
}
