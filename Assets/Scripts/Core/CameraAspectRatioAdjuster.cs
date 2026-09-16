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
            var aspect = _camera != null ? _camera.aspect : (float)Screen.width / Screen.height;
            if (Mathf.Abs(aspect - _lastAspectRatio) > 0.001f)
            {
                AdjustCameraSize();
            }
        }
#endif

        private const float TargetTotalWidth = 10.68f;

        private void AdjustCameraSize()
        {
            if (_camera == null) return;

            var aspect = _camera.aspect;
            if (aspect <= 0f || float.IsNaN(aspect))
            {
                aspect = (float)Screen.width / Screen.height;
            }

            _lastAspectRatio = aspect;
            _camera.orthographicSize = CalculateCameraSize(aspect);
        }

        private float CalculateCameraSize(float aspect)
        {
            if (aspect <= 0f) return 9.13f;
            return TargetTotalWidth / (2f * aspect);
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