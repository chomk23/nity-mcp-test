using System.Collections.Generic;
using UnityEngine;

namespace ForTheCompany.Events
{
    [CreateAssetMenu(menuName = "ForTheCompany/Event Card", fileName = "EventCard")]
    public class EventCard : ScriptableObject
    {
        public string title = "USB가 발견됐다";
        [TextArea(3, 6)]
        public string description = "복도 바닥에서 정체불명의 USB가 발견되었다. 어떻게 처리할 것인가?";

        public List<EventChoice> choices = new List<EventChoice>();
    }
}
