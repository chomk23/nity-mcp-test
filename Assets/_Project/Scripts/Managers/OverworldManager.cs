using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Core;

namespace ForTheCompany.Managers
{
    public class OverworldManager : MonoBehaviour
    {
        public static OverworldManager Instance { get; private set; }

        public List<MapNode> nodes = new List<MapNode>();
        public Camera mapCamera;
        public LineRenderer linePrefab;

        public MapNode HoveredNode { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            EnsureSession();
            RefreshAll();
        }

        private void EnsureSession()
        {
            if (GameSession.Instance == null)
            {
                var go = new GameObject("GameSession");
                go.AddComponent<GameSession>();
            }

            var s = GameSession.Instance;
            if (s.spyRoleIndex < 0) s.StartNewRun();
        }

        private void Update()
        {
            if (EncounterController.Instance != null && EncounterController.Instance.IsActive) return;
            if (GameSession.Instance != null && GameSession.Instance.Outcome != RunOutcome.Ongoing) return;

            HandleHover();
            HandleClick();
        }

        private void HandleHover()
        {
            HoveredNode = null;
            if (mapCamera == null) mapCamera = Camera.main;
            if (mapCamera == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Ray ray = mapCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, 200f))
            {
                var node = hit.collider.GetComponentInParent<MapNode>();
                if (node != null) HoveredNode = node;
            }
        }

        private void HandleClick()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (!mouse.leftButton.wasPressedThisFrame) return;
            if (HoveredNode == null) return;
            if (!HoveredNode.IsReachable) return;
            if (EncounterController.Instance == null) return;

            EncounterController.Instance.Open(HoveredNode);
        }

        public void RefreshAll()
        {
            foreach (var n in nodes)
                if (n != null) n.RefreshVisual();
        }
    }
}
