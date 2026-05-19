using UnityEngine;
using UnityEngine.InputSystem;

namespace ForTheCompany.Core
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 22f, -14f);
        public float followSmooth = 8f;

        [Header("Zoom")]
        public float zoomSpeed = 4f;
        public float minZoom = 0.5f;
        public float maxZoom = 1.8f;
        public float zoomLerp = 10f;

        private float zoomLevel = 1f;
        private float zoomTarget = 1f;

        private void LateUpdate()
        {
            if (target == null) return;

            HandleZoomInput();

            zoomLevel = Mathf.Lerp(zoomLevel, zoomTarget, zoomLerp * Time.deltaTime);
            Vector3 scaledOffset = offset * zoomLevel;

            Vector3 desired = target.position + scaledOffset;
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
            transform.LookAt(target.position);
        }

        private void HandleZoomInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            // Positive scroll = zoom in (smaller multiplier)
            zoomTarget -= scroll * zoomSpeed * 0.01f;
            zoomTarget = Mathf.Clamp(zoomTarget, minZoom, maxZoom);
        }
    }
}
