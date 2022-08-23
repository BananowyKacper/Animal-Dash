using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class MovingWall : MonoBehaviour
    {
        public float indexY = 0;
        public Vector3 startScale;
        public int sampleIndex = 0;
        public Vector3 targetScale;
        public float x = 0;
        public Vector3 pos = Vector3.zero;
        public Vector3 localScale = Vector3.zero;
        // Update is called once per frame
        private void Start()
        {
            sampleIndex = Mathf.Clamp((int)(GameManager.Instance.numberOfMovingWall - indexY - 1) * (SoundManager.Instance.numberOfSample - 1) / (GameManager.Instance.numberOfMovingWall - 1), 0, SoundManager.Instance.numberOfSample - 2);
            x = Mathf.Pow(((GameManager.Instance.numberOfMovingWall - indexY - 1) * (SoundManager.Instance.numberOfSample - 1) / (GameManager.Instance.numberOfMovingWall - 1) % 1), 0.5f);
            startScale = transform.localScale;
            pos = transform.position;
            localScale = transform.localScale;
        }

        void Update()
        {
            //transform base on music
            float distance = pos.z - GameManager.Instance.cameraController.camPos.z;
            if (GameManager.Instance.enableMovingWall && distance < 400 && distance > 100)
            {
                if ((targetScale.x - localScale.x) > GameManager.Instance.fftThreshold)
                {
                    localScale.x = targetScale.x;
                }
                else
                {
                    localScale.x += (targetScale.x - transform.localScale.x) * Time.deltaTime / GameManager.Instance.fftDropDownTime;
                }
                transform.localScale = localScale;
            }
        }
    }
}
