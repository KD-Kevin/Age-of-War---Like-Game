using AgeOfWar.Data;
using UnityEngine;
using UnityEngine.Events;

namespace AgeOfWar.Core.Units
{
    public class EquipmentAsthetic : MonoBehaviour
    {
        [SerializeField]
        private int AstheticID = -1;
        public int ID { get { return AstheticID; } }

        [SerializeField]
        private GameObject[] Asthetics;
        public GameObject[] EquipmentAstheticGameObjects { get { return Asthetics; } }

        public UnityEvent OnEquipOn;
        public UnityEvent OnEquipOff;

        public void TurnOff()
        {
            foreach(GameObject Asthetic in Asthetics)
            {
                Asthetic.gameObject.SetActive(false);
            }

            OnEquipOff?.Invoke();
        }

        public void TurnOn()
        {
            foreach (GameObject Asthetic in Asthetics)
            {
                Asthetic.gameObject.SetActive(true);
            }

            OnEquipOn?.Invoke();
        }
    }
}
