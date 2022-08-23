using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class PlaneScript : MonoBehaviour
    {

        public bool originalPlane = false;
        public Vector3 planeHolderSize = new Vector3(0, 0, 300);
        public bool spawned = false;
        private void Start()
        {
            if (!originalPlane)
                SpawnCoin();
            GameManager.GameStateChanged += OnGameStateChanged;
            if (originalPlane)
                SpawnNextPlane();
        }
        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState, GameState oldGameState)
        {
            if (gameState.Equals(GameState.Prepare) && originalPlane)
                SpawnNextPlane();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!originalPlane && other.tag.Equals("Player"))
            {
                SpawnNextPlane();
            }
            if (other.tag.Equals("CleanUp") && !originalPlane)
            {
                Destroy(gameObject);
            }
        }

        private void SpawnCoin()
        {
            for (int i = 0; i < planeHolderSize.z / 100; i++)
            {
                float rd = UnityEngine.Random.Range(0f, 1f);
                if (rd < GameManager.Instance.coinFrequency)
                {
                    Vector3 pos = new Vector3(GameManager.Instance.lCenter.position.x, 10, transform.position.z + UnityEngine.Random.Range(0, planeHolderSize.z - 100));
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        pos.x = GameManager.Instance.rCenter.position.x;
                    }
                    if (Physics.Raycast(pos, Vector3.down, 50, GameManager.Instance.groundLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        GameObject coin = Instantiate(GameManager.Instance.coinPrefab);
                        coin.transform.position = pos;
                        GameManager.Instance.coinList.Add(coin);
                    }
                }
            }
        }

        private void SpawnNextPlane()
        {
            if (!spawned)
            {
                int randomIndex = UnityEngine.Random.Range(0, PlaneManager.Instance.planeHolderPrefabList.Count);
                GameObject nextPlane = Instantiate(PlaneManager.Instance.planeHolderPrefabList[randomIndex], transform.position + planeHolderSize, Quaternion.identity);
                nextPlane.transform.parent = PlaneManager.Instance.transform;
                spawned = true;
            }
        }
    }
}
