using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class Wall : MonoBehaviour
    {

        public GameObject wallPrefab;
        public GameObject wallChildPrefab;
        public bool spawned = false;
        List<MovingWall> movingWallList = new List<MovingWall>();
        // Use this for initialization
        void Start()
        {
            if (spawned)
            {
                spawned = true;
                SpawnNewWall();
            }
            GenerateMovingWall();
        }

        private void GenerateMovingWall()
        {
            List<int> validPosZ = new List<int>();
            List<int> validPosY = new List<int>();
            List<int> validPosY2 = new List<int>();
            for (int i = 0; i < GameManager.Instance.numberOfMovingWall; i++)
            {
                validPosZ.Add(i);

            }
            for (int i = 0; i < GameManager.Instance.numberOfMovingWall / 2; i++)
            {
                validPosY.Add(i);
                validPosY2.Add(i);
            }

            for (int i = 0; i < GameManager.Instance.numberOfMovingWall; i++)
            {
                int indexZ = UnityEngine.Random.Range(0, validPosZ.Count - 1);
                int posZ = validPosZ[indexZ];
                validPosZ.RemoveAt(indexZ);
                int indexY = 0;
                float offsetX = Mathf.Abs(transform.GetChild(0).transform.position.x);
                int posY = 0;
                if (validPosY.Count != 0 && validPosY2.Count != 0)
                {
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        indexY = UnityEngine.Random.Range(0, validPosY.Count - 1);
                        offsetX = -Mathf.Abs(transform.GetChild(0).transform.position.x);
                        posY = validPosY[indexY] * 2;
                        validPosY.RemoveAt(indexY);
                    }
                    else
                    {
                        indexY = UnityEngine.Random.Range(0, validPosY2.Count - 1);
                        posY = validPosY2[indexY] * 2;
                        validPosY2.RemoveAt(indexY);
                    }
                }
                else if (validPosY.Count != 0)
                {
                    indexY = UnityEngine.Random.Range(0, validPosY.Count - 1);
                    offsetX = -Mathf.Abs(transform.GetChild(0).transform.position.x);
                    posY = validPosY[indexY] * 2;
                    validPosY.RemoveAt(indexY);
                }
                else if (validPosY.Count != 0)
                {
                    indexY = UnityEngine.Random.Range(0, validPosY2.Count - 1);
                    posY = validPosY2[indexY] * 2;
                    validPosY2.RemoveAt(indexY);
                }

                GameObject wallChild = Instantiate(wallChildPrefab, transform.position, Quaternion.identity);
                Vector3 offsetPositionVector = new Vector3(Mathf.Abs(transform.GetChild(0).transform.position.x), ((float)posY / GameManager.Instance.numberOfMovingWall) * 200f + 200f, ((float)posZ / GameManager.Instance.numberOfMovingWall) * 500f);
                offsetPositionVector.x = offsetX;
                wallChild.transform.position += offsetPositionVector;
                Vector3 localScale = wallChild.transform.localScale;
                wallChild.transform.localScale = localScale;
                MovingWall movingWallScript = wallChild.GetComponent<MovingWall>();
                if (movingWallScript)
                    movingWallScript.indexY = posY;
                wallChild.transform.SetParent(transform);
                movingWallList.Add(wallChild.GetComponent<MovingWall>());
            }
            StartCoroutine(UpdateFFTTarget());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Player") && !spawned)
            {
                spawned = true;
                SpawnNewWall();
            }
        }

        private void SpawnNewWall()
        {
            GameObject newWall;
            if (WallManager.Instance.wallList.Count == 0)
            {
                newWall = Instantiate(wallPrefab, transform.position + new Vector3(0, 0, 500), Quaternion.identity);
            }
            else
            {
                newWall = WallManager.Instance.wallList[0];
                WallManager.Instance.wallList.RemoveAt(0);
                newWall.SetActive(true);
                newWall.transform.position = transform.position + new Vector3(0, 0, 500);
            }

            GameManager.Instance.InstantiatedWall.Add(newWall);
            newWall.GetComponent<Wall>().spawned = false;
        }

        private IEnumerator UpdateFFTTarget()
        {
            yield return new WaitForEndOfFrame();
            while (gameObject.activeInHierarchy)
            {
                yield return new WaitForSeconds(1f / SoundManager.Instance.FFTsamplePerSecond);
                UpdateTargetScale();
            }
        }

        private void UpdateTargetScale()
        {
            foreach (var movingWall in movingWallList)
            {
                float distance = movingWall.pos.z - GameManager.Instance.cameraController.camPos.z;
                if (GameManager.Instance.enableMovingWall && distance < 400 && distance > 100)
                {
                    Vector3 localScale = movingWall.startScale;
                    float scalex1 = 40 * (SoundManager.Instance.sampleDownData[movingWall.sampleIndex] * GameManager.Instance.fftVisualizationMagnitute / SoundManager.Instance.maxVol) + GameManager.Instance.fftOffsetFactor;
                    float scalex2 = 40 * (SoundManager.Instance.sampleDownData[movingWall.sampleIndex + 1] * GameManager.Instance.fftVisualizationMagnitute / SoundManager.Instance.maxVol) + GameManager.Instance.fftOffsetFactor;
                    localScale.x = Mathf.Lerp(scalex1, scalex2, movingWall.x);
                    movingWall.targetScale = localScale;
                }
            }

        }
    }
}
