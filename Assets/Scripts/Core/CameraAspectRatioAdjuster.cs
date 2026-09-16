using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class CameraAspectRatioAdjuster : MonoBehaviour
    {
        private Camera _camera;
        private float _lastAspectRatio;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            AdjustCameraSize();
        }

#if UNITY_EDITOR
        private void Update()
        {
            var currentRatio = (float)Screen.height / Screen.width;
            if (Mathf.Abs(currentRatio - _lastAspectRatio) > 0.01f)
            {
                AdjustCameraSize();
            }
        }
#endif

        private void AdjustCameraSize()
        {
            if (_camera == null) return;

            var currentRatio = (float)Screen.height / Screen.width;
            Debug.Log(Screen.height);
            Debug.Log(Screen.width);
            _lastAspectRatio = currentRatio;
            _camera.orthographicSize = CalculateCameraSize(currentRatio);
        }

        private float CalculateCameraSize(float currentRatio)
        {
            if (currentRatio <= 1.333333f) return 10.2f;
            if (currentRatio <= 1.431655f) return 10.2f;
            if (currentRatio <= 1.6f) return 10.5f;
            if (currentRatio <= 1.78f) return 10.6f;
            if (currentRatio <= 2.0f) return 11.7f;
            if (currentRatio <= 2.055556f) return 11.7f;
            if (currentRatio <= 2.088889f) return 12f;
            if (currentRatio <= 2.25f) return 12.5f;
            if (currentRatio <= 2.3f) return 13f;
            if (currentRatio <= 2.4f) return 13.5f;
            
            return 14f;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_camera == null || !_camera.orthographic) return;

            var height = _camera.orthographicSize * 2f;
            var width = height * _camera.aspect;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
        }
#endif
    }
}