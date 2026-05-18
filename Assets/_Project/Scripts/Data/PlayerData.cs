using UnityEngine;

namespace ForTheCompany.Data
{
    [CreateAssetMenu(menuName = "ForTheCompany/Player Data", fileName = "PlayerData")]
    public class PlayerData : ScriptableObject
    {
        public string playerName = "Agent";
        public RoleType role = RoleType.Security;

        [Header("Stats")]
        public int maxHP = 10;
        public int maxAP = 3;

        [Header("Skill Ratings (1-5)")]
        public int hacking = 1;
        public int security = 1;
        public int investigation = 1;

        [Header("Visual")]
        public Color displayColor = Color.cyan;
    }
}
