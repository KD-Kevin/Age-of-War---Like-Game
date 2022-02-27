using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkSelectorCatagory : MonoBehaviour
{
    [SerializeField]
    private PerkUiSelectionElement PerUiElementPrefab;
    [SerializeField]
    private Transform PrefabSpawn;
    [SerializeField]
    private GameObject NoPerksInCatagoryObject;

    private List<PerkUiSelectionElement> ElementPool = new List<PerkUiSelectionElement>();
    private List<PerkUiSelectionElement> ActiveElements = new List<PerkUiSelectionElement>();

    public void DisableCatagory()
    {
        if (ActiveElements.Count > 0)
        {
            foreach (PerkUiSelectionElement element in ActiveElements)
            {
                element.gameObject.SetActive(false);
                ElementPool.Add(element);
            }
            ActiveElements.Clear();
        }
    }

    public void EnableCatagory(List<Perk> PerkList, PerkTypes SortType, Perk CurrentlySelectedPerk1, Perk CurrentlySelectedPerk2)
    {
        foreach (Perk perk in PerkList)
        {
            if (SortType == PerkTypes.All || perk.IsPerkType(SortType))
            {
                PerkUiSelectionElement NewElement = GetNewElement();
                NewElement.SetUi(perk);

                if (perk == CurrentlySelectedPerk1 || perk == CurrentlySelectedPerk2)
                {
                    NewElement.SetSelectionColor();
                }
            }
        }

        NoPerksInCatagoryObject.gameObject.SetActive(ActiveElements.Count <= 0);
    }

    public PerkUiSelectionElement GetNewElement()
    {
        PerkUiSelectionElement NewElement;

        if (ElementPool.Count > 0)
        {
            NewElement = ElementPool[0];
            NewElement.gameObject.SetActive(true);
            ElementPool.RemoveAt(0);
        }
        else
        {
            NewElement = Instantiate(PerUiElementPrefab, PrefabSpawn);
            NewElement.CorrespondingCatagory = this;
        }
        ActiveElements.Add(NewElement);

        return NewElement;
    }

    public void OnSelect(PerkUiSelectionElement SelectedUi)
    {
        PerkSelector.Instance.SelectPerk(SelectedUi);
    }

    public void ResetUi(Perk CurrentlySelectedPerk1, Perk CurrentlySelectedPerk2)
    {
        foreach(PerkUiSelectionElement Element in ActiveElements)
        {
            if (Element.CorrespondingPerk == CurrentlySelectedPerk1 || Element.CorrespondingPerk == CurrentlySelectedPerk2)
            {
                Element.SetSelectionColor();
            }
            else
            {
                Element.ResetUi();
            }
        }
    }
}
