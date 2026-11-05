using UnityEngine;

namespace DesertTreasureHunt
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        [Header("Target & Offset")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 3.8f, -5.5f);
        public float followSmoothTime = 0.12f;
        public float lookAheadDistance = 4f;

        [Header("Dynamic Effects")]
        public float baseFOV = 60f;
        public float maxFOV = 75f;

        private Camera cam;
        private Vector3 currentVelocity;
        private float shakeTime = 0f;
        private float shakeIntensity = 0f;

        private void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null) target = PlayerController.Instance.transform;
                return;
            }

            // Calculate target position
            Vector3 desiredPosition = new Vector3(
                Mathf.Lerp(transform.position.x, target.position.x * 0.4f, Time.deltaTime * 6f),
                target.position.y + offset.y,
                target.position.z + offset.z
            );

            // Camera Shake
            if (shakeTime > 0f)
            {
                desiredPosition += (Vector3)Random.insideUnitCircle * shakeIntensity;
                shakeTime -= Time.deltaTime;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);

            // Look target
            Vector3 lookTarget = target.position + Vector3.forward * lookAheadDistance + Vector3.up * 1.5f;
            transform.LookAt(lookTarget);

            // Dynamic FOV based on speed
            if (cam != null && GameManager.Instance != null)
            {
                float speedRatio = Mathf.Clamp01((GameManager.Instance.currentSpeed - 10f) / 15f);
                float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
            }
        }

        public void TriggerShake(float duration = 0.25f, float intensity = 0.35f)
        {
            shakeTime = duration;
            shakeIntensity = intensity;
        }
    }
}
