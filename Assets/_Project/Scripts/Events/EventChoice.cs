using System;
using UnityEngine;

namespace ForTheCompany.Events
{
    [Serializable]
    public class EventChoice
    {
        public string label = "조사한다";

        [Header("Player Effects")]
        public int hpDelta;
        public int apDelta;

        [Header("Facility Effects")]
        public int suspicionDelta;
        public int dataIntegrityDelta;

        [Header("Outcome")]
        [TextArea(2, 4)]
        public string resultText = "조사를 진행했습니다.";
    }
}
