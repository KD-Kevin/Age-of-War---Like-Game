using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Dropdown _dropdown;

    public static Localization CurrentLocalization = Localization.English;

    private void Start()
    {
        _dropdown.options.Clear();

        List<string> options = new List<string>();

        for(int i = 0; i < (int)Localization.Number; i++)
        {
            options.Add(((Localization)i).ToString());
        }

        _dropdown.AddOptions(options);

        GetDropDown();
        CurrentLocalization = (Localization)_dropdown.value;
    }

    private void GetDropDown()
    {
        if (PlayerPrefs.GetInt("Language", 0) == 0)
        {
            _dropdown.value = 0;
        }
        else
        {
            _dropdown.value = 1;
        }
    }

    public void ChangeLanguage(int selected)
    {
        PlayerPrefs.SetInt("Language", selected);
        CurrentLocalization = (Localization)selected;

        //LocalizationField[] allLoadedLocalizationFields = FindObjectsOfType<LocalizationField>();
        //for (int l = 0; l < allLoadedLocalizationFields.Length; ++l)
        //{
        //    allLoadedLocalizationFields[l].ChangeLocalization();
        //}
    }
}

