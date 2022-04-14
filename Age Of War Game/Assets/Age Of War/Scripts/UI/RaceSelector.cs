using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceSelector : MonoBehaviour
{
    public List<RaceDataScriptableObject> RaceDataList = new List<RaceDataScriptableObject>();
    [SerializeField]
    private RaceSelectionUiElement RaceUiPrefab;
    [SerializeField]
    private Transform PrefabSpawn;
    [SerializeField]
    private ButtonManagerBasic ConfirmButtonManager;
    [SerializeField]
    private Button ConfirmButton;

    private List<RaceSelectionUiElement> RaceUiElements = new List<RaceSelectionUiElement>();
    public RaceDataScriptableObject CurrentDataSelected { get; set; }
    public RaceSelectionUiElement CurrentDataUiSelected { get; set; }
    public static RaceSelector Instance = null;
    private System.Action<RaceDataScriptableObject> ConfirmationCallback = null;
    private System.Action CancellationCallback = null;

    public void SetInstance()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        CurrentDataSelected = null;
        ConfirmButton.interactable = false;
        ConfirmButtonManager.useRipple = false;
        if (RaceUiElements.Count == 0)
        {
            foreach (RaceDataScriptableObject data in RaceDataList)
            {
                RaceSelectionUiElement NewElement = Instantiate(RaceUiPrefab, PrefabSpawn);
                NewElement.SetUi(data);

                RaceUiElements.Add(NewElement);
            }
        }
        else
        {
            foreach(RaceSelectionUiElement ui in RaceUiElements)
            {
                ui.ResetUi();
            }
        }
    }

    public void SelectData(RaceSelectionUiElement SelectUi)
    {
        if (SelectUi == CurrentDataUiSelected)
        {
            return;
        }
        CurrentDataUiSelected?.ResetUi();
        CurrentDataUiSelected = SelectUi;
        CurrentDataSelected = SelectUi.CorrespondingData;
        ConfirmButton.interactable = true;
        ConfirmButtonManager.useRipple = true;
    }

    public void OpenRaceSelector(System.Action<RaceDataScriptableObject> ConfirmCallback, System.Action CancelCallback)
    {
        gameObject.SetActive(true);
        ConfirmationCallback = ConfirmCallback;
        CancellationCallback = CancelCallback;
    }

    public void ConfirmSelection()
    {
        ConfirmationCallback(CurrentDataSelected);
        gameObject.SetActive(false);
    }

    public void CancelSelection()
    {
        CancellationCallback();
        gameObject.SetActive(false);
    }

    public int GetRaceIndex(RaceDataScriptableObject WantedRaceInfo)
    {
        if (WantedRaceInfo == null)
        {
            return -1;
        }

        for(int index = 0; index < RaceDataList.Count; index++)
        {
            if (RaceDataList[index].DisplayName == WantedRaceInfo.DisplayName)
            {
                return index;
            }
        }

        return -1;
    }
}
