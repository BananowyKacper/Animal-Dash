using UnityEngine;
using System.Collections;
using System;

namespace BigheadRunRun
{
    public class CameraController : MonoBehaviour
    {
        public Transform playerTransform;
        private Vector3 velocity = Vector3.zero;
        private Vector3 originalDistance;

        [Header("Camera Follow Smooth-Time")]
        public float smoothTime = 0.1f;

        [Header("Shaking Effect")]
        // How long the camera shaking.
        public float shakeDuration = 0.1f;
        // Amplitude of the shake. A larger value shakes the camera harder.
        public float shakeAmount = 0.2f;
        public float decreaseFactor = 0.3f;
        [HideInInspector]
        public Vector3 originalPos;

        private float currentShakeDuration;
        private float currentDistance;
        Vector3 defaultPos;
        public Vector3 camPos = Vector3.zero;
        private void OnEnable()
        {
            GameManager.PlayerHasBeenSpawned += OnPlayerHasBeenSpawned;
        }

        private void OnDisable()
        {
            GameManager.PlayerHasBeenSpawned -= OnPlayerHasBeenSpawned;
        }

        private void OnPlayerHasBeenSpawned(Transform playerTranformTemp)
        {
            playerTransform = playerTranformTemp;
        }

        void Start()
        {
            defaultPos = transform.position;
            originalDistance = transform.position - playerTransform.transform.position;
        }

        void Update()
        {
            Shader.SetGlobalVector("_CamPos", transform.position);
            if (GameManager.Instance.GameState == GameState.Playing && playerTransform != null)
            {
                Vector3 pos = playerTransform.position + originalDistance;
                if (GameManager.Instance.lockCameraMovementX)
                {
                    float xPos = transform.position.x;
                    pos.x = xPos;
                }
                if (GameManager.Instance.lockCameraMovementY)
                {
                    float yPos = transform.position.y;
                    pos.y = yPos;
                }
                transform.position = Vector3.SmoothDamp(transform.position, pos, ref velocity, smoothTime);
            }
            camPos = transform.position;
        }

        public void FixPosition()
        {
            transform.position = playerTransform.position + originalDistance;
        }

        public void ShakeCamera()
        {
            StartCoroutine(Shake());
        }

        IEnumerator Shake()
        {
            originalPos = transform.position;
            currentShakeDuration = shakeDuration;
            while (currentShakeDuration > 0)
            {
                transform.position = originalPos + UnityEngine.Random.insideUnitSphere * shakeAmount;
                currentShakeDuration -= Time.deltaTime * decreaseFactor;
                yield return null;
            }
            transform.position = originalPos;
        }
        public void ResetPos()
        {
            transform.position = defaultPos;
        }
    }
}
