using UnityEngine;
using ForTheCompany.Managers;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    public class SpyTickSystem : MonoBehaviour
    {
        [Header("Real Spy")]
        [Range(0f, 1f)] public float realClueChance = 0.55f;
        public int realClueSuspicion = 8;
        public int realClueDataDamage = 4;

        [Header("Red Herring (Innocent NPC)")]
        [Range(0f, 1f)] public float falseClueChance = 0.25f;
        public int falseClueSuspicion = 6;

        private int lastProcessedTurn = -1;

        private void Update()
        {
            var tm = TurnManager.Instance;
            if (tm == null) return;

            if (tm.turnNumber > lastProcessedTurn && tm.turnNumber > 1)
            {
                lastProcessedTurn = tm.turnNumber;
                ProcessTurn();
            }
        }

        private void ProcessTurn()
        {
            var roster = NPCRoster.Instance;
            if (roster == null || roster.Spy == null) return;

            if (Random.value < realClueChance)
            {
                roster.Spy.AddSuspicion(realClueSuspicion);
                if (FacilityState.Instance != null)
                    FacilityState.Instance.Modify(0, -realClueDataDamage);
                Debug.Log($"[Clue] {roster.Spy.DisplayName} 주변에서 의심스러운 흔적 (+{realClueSuspicion} 의심, -{realClueDataDamage} 데이터)");
            }

            if (Random.value < falseClueChance && roster.npcs.Count > 1)
            {
                NPCActor target = null;
                int safety = 8;
                while (safety-- > 0)
                {
                    var pick = roster.npcs[Random.Range(0, roster.npcs.Count)];
                    if (pick != null && !pick.isSpy) { target = pick; break; }
                }
                if (target != null)
                {
                    target.AddSuspicion(falseClueSuspicion);
                    Debug.Log($"[?] {target.DisplayName} 도 수상한 움직임 (+{falseClueSuspicion} 의심)");
                }
            }
        }
    }
}
