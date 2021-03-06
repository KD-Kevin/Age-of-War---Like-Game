using AgeOfWar.Core.Units;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.UI
{
    public class UnitHealthBarManager : MonoBehaviour
    {
        public static UnitHealthBarManager Instance;

        [SerializeField]
        private UnitHealBarBase HealthBarPrefab;
        [SerializeField]
        private Transform HealthBarPrefabSpawn;

        private List<UnitHealBarBase> Active = new List<UnitHealBarBase>();
        private List<UnitHealBarBase> Pool = new List<UnitHealBarBase>();
        private List<UnitHealBarBase> ReturnList = new List<UnitHealBarBase>();
        private List<UnitHealBarBase> AddList = new List<UnitHealBarBase>();

        private void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            foreach (UnitHealBarBase HealthBar in AddList)
            {
                Active.Add(HealthBar);
            }
            AddList.Clear();

            foreach (UnitHealBarBase HealthBar in Active)
            {
                HealthBar.UpdateUi();
            }

            foreach (UnitHealBarBase HealthBar in ReturnList)
            {
                Active.Remove(HealthBar);
                Pool.Add(HealthBar);
            }
            ReturnList.Clear();
        }

        public void ReturnUi(UnitHealBarBase unitHealthBar)
        {
            unitHealthBar.gameObject.SetActive(false);
            ReturnList.Add(unitHealthBar);
        }

        public void GetHealthBar(BaseUnitBehaviour ForUnit)
        {
            UnitHealBarBase NewBar;
            if (Pool.Count > 0)
            {
                NewBar = Pool[0];
                Pool.RemoveAt(0);
                NewBar.gameObject.SetActive(true);
            }
            else
            {
                NewBar = Instantiate(HealthBarPrefab, HealthBarPrefabSpawn);
            }

            NewBar.SetUnit(ForUnit);
            AddList.Add(NewBar);
        }
    }
}