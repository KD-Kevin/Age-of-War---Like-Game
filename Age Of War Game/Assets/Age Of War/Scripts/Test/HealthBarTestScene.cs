using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthBarTestScene : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField ChangeDamageAmountInputField;
    [SerializeField]
    private TMP_InputField ChangeHealAmountInputField;
    [SerializeField]
    private TMP_InputField ChangeStartingHealthAmountInputField;
    [SerializeField]
    private TMP_InputField ChangeStartingArmorAmountInputField;
    [SerializeField]
    private TMP_InputField ChangeStartingMagicArmorAmountInputField;
    [SerializeField]
    private TMP_InputField SegmentSizeInputField;
    [SerializeField]
    private TMP_Dropdown DamageTypeDropdown;
    [SerializeField]
    private TMP_Dropdown HealTypeDropdown;

    private UnitHealBarBase[] HealthBars;

    private int CurrentDamageAmount = 10;
    private int CurrentHealAmount = 10;

    private int NextHealthAmount = 100;
    private int NextArmorAmount = 0;
    private int NextMagicArmorAmount = 0;


    private DamageTypes DamageType = DamageTypes.Physical;
    private HealTypes HealType = HealTypes.Heal;
    private TestHealthObject TemporaryPretendUnit;
    private TestHealthObject NextPretendUnit;

    private void Awake()
    {
        // There should only be 4 objects
        HealthBars = FindObjectsOfType<UnitHealBarBase>();
        TemporaryPretendUnit = new TestHealthObject();
        NextPretendUnit = new TestHealthObject();
        ChangeDamageAmountInputField.SetTextWithoutNotify(CurrentDamageAmount.ToString());
        ChangeHealAmountInputField.SetTextWithoutNotify(CurrentHealAmount.ToString());
        ChangeStartingHealthAmountInputField.SetTextWithoutNotify(100.ToString());
        ChangeStartingArmorAmountInputField.SetTextWithoutNotify(0.ToString());
        ChangeStartingMagicArmorAmountInputField.SetTextWithoutNotify(0.ToString());
        SegmentSizeInputField.SetTextWithoutNotify(25.ToString());

        int counter = 1;
        foreach (UnitHealBarBase Healthbar in HealthBars)
        {
            Healthbar.SetUnit(TemporaryPretendUnit); 
            if (counter == 1)
            {
                Healthbar.transform.position = new Vector3(-8, 1, 0);
            }
            else if (counter == 2)
            {
                Healthbar.transform.position = new Vector3(-3, 1, 0);
            }
            else if (counter == 3)
            {
                Healthbar.transform.position = new Vector3(3, 1, 0);
            }
            else if (counter == 4)
            {
                Healthbar.transform.position = new Vector3(8, 1, 0);
            }
            counter++;
        }
    }

    private void Update()
    {
        foreach(UnitHealBarBase Healthbar in HealthBars)
        {
            Healthbar.UpdateUi();
        }
    }

    public void SetDamage(string damageString)
    {
        if (int.TryParse(damageString, out int Damage))
        {
            if (Damage <= 0)
            {
                CurrentDamageAmount = 1;
            }
            else
            {
                CurrentDamageAmount = Damage;
            }
            ChangeDamageAmountInputField.SetTextWithoutNotify(CurrentDamageAmount.ToString()); ;
        }
    }

    public void Damage()
    {
        TemporaryPretendUnit.Damage(CurrentDamageAmount, "Testing", DamageType);
    }

    public void SetDamageType(int damageTypeIndex)
    {
        string damageTypeString = DamageTypeDropdown.options[damageTypeIndex].text;
        if (damageTypeString == DamageTypes.Magical.ToString())
        {
            DamageType = DamageTypes.Magical;
            DamageTypeDropdown.SetValueWithoutNotify(1);
        }
        else if (damageTypeString == DamageTypes.True.ToString())
        {
            DamageType = DamageTypes.True;
            DamageTypeDropdown.SetValueWithoutNotify(2);
        }
        else
        {
            DamageType = DamageTypes.Physical;
            DamageTypeDropdown.SetValueWithoutNotify(0);
        }
    }

    public void Heal()
    {
        if (HealType == HealTypes.Heal)
        {
            TemporaryPretendUnit.Heal(CurrentHealAmount);
        }
        else if (HealType == HealTypes.Repair)
        {
            TemporaryPretendUnit.Repair(CurrentHealAmount);
        }
        else if (HealType == HealTypes.Restore)
        {
            TemporaryPretendUnit.Restore(CurrentHealAmount);
        }
        else if (HealType == HealTypes.RepairAndHeal)
        {
            TemporaryPretendUnit.HealAndRepair(CurrentHealAmount);
        }
        else if (HealType == HealTypes.HealAndRestore)
        {
            TemporaryPretendUnit.HealAndRestore(CurrentHealAmount);
        }
        else if (HealType == HealTypes.RepairAndRestore)
        {
            TemporaryPretendUnit.RepairAndRestore(CurrentHealAmount);
        }
        else if (HealType == HealTypes.ReverseAll)
        {
            TemporaryPretendUnit.HealRestoreAndRepair(CurrentHealAmount);
        }
        else
        {
            TemporaryPretendUnit.HealRepairAndRestore(CurrentHealAmount);
        }
    }

    public void SetHeal(string HealString)
    {
        if (int.TryParse(HealString, out int Heal))
        {
            if (Heal <= 0)
            {
                CurrentHealAmount = 1;
            }
            else
            {
                CurrentHealAmount = Heal;
            }
            ChangeHealAmountInputField.SetTextWithoutNotify(CurrentHealAmount.ToString()); ;
        }
    }

    public void SetHealType(int healTypeIndex)
    {
        string healTypeString = HealTypeDropdown.options[healTypeIndex].text;
        if (healTypeString == HealTypes.Heal.ToString())
        {
            HealType = HealTypes.Heal;
            HealTypeDropdown.SetValueWithoutNotify(0);
        }
        else if (healTypeString == HealTypes.Repair.ToString())
        {
            HealType = HealTypes.Repair;
            HealTypeDropdown.SetValueWithoutNotify(1);
        }
        else if (healTypeString == HealTypes.Restore.ToString())
        {
            HealType = HealTypes.Restore;
            HealTypeDropdown.SetValueWithoutNotify(2);
        }
        else if (healTypeString == HealTypes.RepairAndHeal.ToString())
        {
            HealType = HealTypes.RepairAndHeal;
            HealTypeDropdown.SetValueWithoutNotify(3);
        }
        else if (healTypeString == HealTypes.HealAndRestore.ToString())
        {
            HealType = HealTypes.HealAndRestore;
            HealTypeDropdown.SetValueWithoutNotify(4);
        }
        else if (healTypeString == HealTypes.RepairAndRestore.ToString())
        {
            HealType = HealTypes.RepairAndRestore;
            HealTypeDropdown.SetValueWithoutNotify(5);
        }
        else if (healTypeString == HealTypes.ReverseAll.ToString())
        {
            HealType = HealTypes.ReverseAll;
            HealTypeDropdown.SetValueWithoutNotify(6);
        }
        else
        {
            HealType = HealTypes.All;
            HealTypeDropdown.SetValueWithoutNotify(7);
        }
    }

    public void SetStartingHealth(string healthString)
    {
        if (int.TryParse(healthString, out int Health))
        {
            if (Health <= 0)
            {
                NextHealthAmount = 1;
            }
            else
            {
                NextHealthAmount = Health;
            }

            ChangeStartingHealthAmountInputField.SetTextWithoutNotify(NextHealthAmount.ToString());
        }
    }

    public void SetStartingArmor(string ArmorString)
    {
        if (int.TryParse(ArmorString, out int Armor))
        {
            if (Armor < 0)
            {
                NextArmorAmount = 0;
            }
            else
            {
                NextArmorAmount = Armor;
            }

            ChangeStartingArmorAmountInputField.SetTextWithoutNotify(NextArmorAmount.ToString());
        }
    }

    public void SetStartingMagicArmor(string MagicArmorString)
    {
        if (int.TryParse(MagicArmorString, out int MagicArmor))
        {
            if (MagicArmor < 0)
            {
                NextMagicArmorAmount = 0;
            }
            else
            {
                NextMagicArmorAmount = MagicArmor;
            }

            ChangeStartingMagicArmorAmountInputField.SetTextWithoutNotify(NextMagicArmorAmount.ToString());
        }
    }

    public void SetSegmentSize(string NewSize)
    {
        if (int.TryParse(NewSize, out int Size))
        {
            if (Size <= 0)
            {
                Size = 1;
            }

            foreach (UnitHealBarBase Healthbar in HealthBars)
            {
                if (Healthbar is UnitHealBarSegmentStyle)
                {
                    UnitHealBarSegmentStyle SegmentBar = Healthbar as UnitHealBarSegmentStyle;
                    SegmentBar.SegmentSize = Size;
                }
            }
        }
    }

    public void StartUnitOver()
    {
        NextPretendUnit.MaxHealth = NextHealthAmount;
        NextPretendUnit.MaxArmor = NextArmorAmount;
        NextPretendUnit.MaxMagicArmor = NextMagicArmorAmount;
        NextPretendUnit.Initialize();
        TestHealthObject Switch = TemporaryPretendUnit;
        TemporaryPretendUnit = NextPretendUnit;
        NextPretendUnit = Switch;
        foreach (UnitHealBarBase Healthbar in HealthBars)
        {
            Healthbar.SetUnit(TemporaryPretendUnit);
            Healthbar.UpdateUi();
        }
    }
}

[System.Serializable]
public class TestHealthObject : IHealth
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int StartingHealth { get; set; }
    public int CurrentArmor { get; set; }
    public int MaxArmor { get; set; }
    public int StartingArmor { get; set; }
    public int CurrentMagicArmor { get; set; }
    public int MaxMagicArmor { get; set; }
    public int StartingMagicArmor { get; set; }

    public TestHealthObject(int maxHealth = 100, int maxArmor = 0, int maxMagicArmor = 0)
    {
        MaxHealth = maxHealth;
        MaxArmor = maxArmor;
        MaxMagicArmor = maxMagicArmor;
        Initialize();
    }

    #region IHealth

    public virtual void Damage(int DamageAmount, string DamageReason, DamageTypes DamageType)
    {
        // Multipliers here


        // Defenses Here
        DamageAmount = Mathf.FloorToInt(1 * DamageAmount);
        int Remainder = DamageAmount;
        if (DamageType == DamageTypes.Physical)
        {
            // Modifiers Here
            if (CurrentArmor > 0)
            {
                if (DamageAmount > 10)
                {
                    DamageAmount -= 10;
                }
                else
                {
                    DamageAmount /= 2;
                }
            }
            Remainder = DamageAmount - CurrentArmor;
            CurrentArmor -= DamageAmount;
            if (CurrentArmor < 0)
            {
                CurrentArmor = 0;
            }
        }
        else if (DamageType == DamageTypes.Magical)
        {
            if (CurrentMagicArmor > 0)
            {
                if (DamageAmount > 10)
                {
                    DamageAmount -= 10;
                }
                else
                {
                    DamageAmount /= 2;
                }
            }
            Remainder = DamageAmount - CurrentMagicArmor;
            CurrentMagicArmor -= DamageAmount;
            if (CurrentMagicArmor < 0)
            {
                CurrentMagicArmor = 0;
            }
        }

        // Damage Here
        if (Remainder > 0)
        {
            CurrentHealth -= Remainder;

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
                DieOff(DamageReason);
            }
        }
    }

    public virtual void DieOff(string DeathReason)
    {

    }

    public virtual void Heal(int HealAmount)
    {
        // Modifiers Here

        if (HealAmount == 0)
        {
            return;
        }


        CurrentHealth += HealAmount;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    public virtual void HealAndRepair(int RepairAmount)
    {
        // Modifiers Here

        if (RepairAmount == 0)
        {
            return;
        }


        CurrentHealth += RepairAmount;
        int Remainder = 0;
        if (CurrentHealth > MaxHealth)
        {
            Remainder = CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }

        if (Remainder > 0)
        {
            CurrentArmor += Remainder;
            if (CurrentArmor > MaxArmor)
            {
                CurrentArmor = MaxArmor;
            }
        }
    }

    public virtual void HealAndRestore(int RestoreAmount)
    {
        // Modifiers Here


        if (RestoreAmount == 0)
        {
            return;
        }


        CurrentHealth += RestoreAmount;
        int Remainder = 0;
        if (CurrentHealth > MaxHealth)
        {
            Remainder = CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }

        if (Remainder > 0)
        {
            CurrentMagicArmor += Remainder;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                CurrentMagicArmor = MaxMagicArmor;
            }
        }
    }

    public virtual void RepairAndRestore(int RepairAmount)
    {
        // Modifiers Here

        if (RepairAmount == 0)
        {
            return;
        }


        CurrentArmor += RepairAmount;
        int Remainder = 0;
        if (CurrentArmor > MaxArmor)
        {
            Remainder = CurrentArmor - MaxArmor;
            CurrentArmor = MaxArmor;
        }

        if (Remainder > 0)
        {
            CurrentMagicArmor += Remainder;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                CurrentMagicArmor = MaxMagicArmor;
            }
        }
    }

    public virtual void RestoreAndRepair(int RestoreAmount)
    {
        // Modifiers Here

        if (RestoreAmount == 0)
        {
            return;
        }


        CurrentMagicArmor += RestoreAmount;
        int Remainder = 0;
        if (CurrentMagicArmor > MaxMagicArmor)
        {
            Remainder = CurrentMagicArmor - MaxMagicArmor;
            CurrentMagicArmor = MaxMagicArmor;
        }

        if (Remainder > 0)
        {
            CurrentArmor += Remainder;
            if (CurrentArmor > MaxArmor)
            {
                CurrentArmor = MaxArmor;
            }
        }
    }

    public virtual void HealRepairAndRestore(int Amount)
    {
        if (Amount == 0)
        {
            return;
        }


        CurrentHealth += Amount;
        int Remainder = 0;
        if (CurrentHealth > MaxHealth)
        {
            Remainder = CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }

        if (Remainder > 0)
        {
            CurrentArmor += Remainder;
            if (CurrentArmor > MaxArmor)
            {
                Remainder = CurrentArmor - MaxArmor;
                CurrentArmor = MaxArmor;
            }
            else
            {
                Remainder = 0;
            }
        }

        if (Remainder > 0)
        {
            CurrentMagicArmor += Remainder;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                Remainder = CurrentMagicArmor - MaxMagicArmor;
                CurrentMagicArmor = MaxMagicArmor;
            }
            else
            {
                Remainder = 0;
            }
        }
    }

    public virtual void HealRestoreAndRepair(int Amount)
    {
        if (Amount == 0)
        {
            return;
        }


        CurrentHealth += Amount;
        int Remainder = 0;
        if (CurrentHealth > MaxHealth)
        {
            Remainder = CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }

        if (Remainder > 0)
        {
            CurrentMagicArmor += Remainder;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                Remainder = CurrentMagicArmor - MaxMagicArmor;
                CurrentMagicArmor = MaxMagicArmor;
            }
            else
            {
                Remainder = 0;
            }
        }

        if (Remainder > 0)
        {
            CurrentArmor += Remainder;
            if (CurrentArmor > MaxArmor)
            {
                Remainder = CurrentArmor - MaxArmor;
                CurrentArmor = MaxArmor;
            }
            else
            {
                Remainder = 0;
            }
        }
    }

    public virtual void Initialize()
    {
        CurrentArmor = MaxArmor;
        CurrentHealth = MaxHealth;
        CurrentMagicArmor = MaxMagicArmor;
    }

    public virtual void Repair(int RepairAmount)
    {
        // Modifiers Here

        CurrentArmor += RepairAmount;
        if (CurrentArmor > MaxArmor)
        {
            CurrentArmor = MaxArmor;
        }
    }

    public virtual void Restore(int RestoreAmount)
    {
        // Modifiers Here

        CurrentMagicArmor += RestoreAmount;
        if (CurrentMagicArmor > MaxMagicArmor)
        {
            CurrentMagicArmor = MaxMagicArmor;
        }
    }

    #endregion
}