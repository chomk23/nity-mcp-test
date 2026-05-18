using System;
using UnityEngine;

namespace ForTheCompany.Systems
{
    public class FacilityState : MonoBehaviour
    {
        public static FacilityState Instance { get; private set; }

        [Header("Global Facility")]
        public int suspicionLevel = 0;
        public int dataIntegrity = 100;

        public event Action OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Modify(int suspicionDelta, int dataDelta)
        {
            if (suspicionDelta == 0 && dataDelta == 0) return;
            suspicionLevel = Mathf.Max(0, suspicionLevel + suspicionDelta);
            dataIntegrity = Mathf.Clamp(dataIntegrity + dataDelta, 0, 100);
            OnStateChanged?.Invoke();
        }
    }
}
