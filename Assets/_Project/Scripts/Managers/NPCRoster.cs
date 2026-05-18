using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ForTheCompany.Player;

namespace ForTheCompany.Managers
{
    public class NPCRoster : MonoBehaviour
    {
        public static NPCRoster Instance { get; private set; }

        public List<NPCActor> npcs = new List<NPCActor>();

        [Header("Spy")]
        public bool assignSpyOnStart = true;
        public bool revealSpyInLog = true;

        public NPCActor Spy { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (assignSpyOnStart) AssignRandomSpy();
        }

        public void AssignRandomSpy()
        {
            if (npcs == null || npcs.Count == 0) return;

            foreach (var n in npcs)
                if (n != null) n.isSpy = false;

            int idx = Random.Range(0, npcs.Count);
            Spy = npcs[idx];
            Spy.isSpy = true;

            if (revealSpyInLog)
                Debug.Log($"[Spy Assigned] {Spy.DisplayName} 가 스파이입니다 (디버그용 표시).");
        }

        public IEnumerator RunAllNPCsTurn()
        {
            for (int i = 0; i < npcs.Count; i++)
            {
                var npc = npcs[i];
                if (npc == null) continue;

                var beh = npc.GetComponent<NPCBehavior>();
                if (beh != null)
                    yield return StartCoroutine(beh.TakeTurn());
            }
        }
    }
}
