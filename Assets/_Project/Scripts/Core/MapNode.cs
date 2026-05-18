using System.Collections.Generic;
using UnityEngine;

namespace ForTheCompany.Core
{
    public enum NodeKind
    {
        Start,
        SecurityPuzzle,
        Dialogue,
        Event,
        Boss
    }

    public class MapNode : MonoBehaviour
    {
        public int nodeId;
        public string displayName = "Server Room";
        public NodeKind kind = NodeKind.SecurityPuzzle;
        public List<MapNode> connections = new List<MapNode>();

        [Header("Visual")]
        public Color baseColor = new Color(0.7f, 0.8f, 1f);
        public Color clearedColor = new Color(0.4f, 0.7f, 0.4f);
        public Color lockedColor = new Color(0.35f, 0.35f, 0.4f);

        private Renderer rend;

        private void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
        }

        public bool IsCleared
        {
            get
            {
                var s = GameSession.Instance;
                return s != null && s.clearedNodeIds.Contains(nodeId);
            }
        }

        public bool IsReachable
        {
            get
            {
                var s = GameSession.Instance;
                if (s == null) return false;
                if (IsCleared) return false;

                if (s.clearedNodeIds.Count == 0) return kind == NodeKind.Start;

                foreach (var c in connections)
                    if (c != null && c.IsCleared) return true;
                return false;
            }
        }

        public void RefreshVisual()
        {
            if (rend == null) rend = GetComponentInChildren<Renderer>();
            if (rend == null) return;

            Color c = lockedColor;
            if (IsCleared) c = clearedColor;
            else if (IsReachable) c = baseColor;

            var mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", c);
            mpb.SetColor("_Color", c);
            rend.SetPropertyBlock(mpb);
        }
    }
}
