using UnityEngine;
using ForTheCompany.Data;

namespace ForTheCompany.Player
{
    public class PlayerStats : MonoBehaviour
    {
        public PlayerData data;

        public int currentHP;
        public int currentAP;

        public bool isInsider;

        private void Awake()
        {
            if (data != null)
            {
                currentHP = data.maxHP;
                currentAP = data.maxAP;
            }
        }

        public bool CanSpendAP(int amount) => currentAP >= amount;

        public bool SpendAP(int amount)
        {
            if (!CanSpendAP(amount)) return false;
            currentAP -= amount;
            return true;
        }

        public void RefillAP()
        {
            if (data != null) currentAP = data.maxAP;
        }
    }
}
