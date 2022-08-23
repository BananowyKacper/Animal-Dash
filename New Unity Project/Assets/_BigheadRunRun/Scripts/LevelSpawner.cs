using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class LevelSpawner : MonoBehaviour
    {
        public static LevelSpawner Instance { get; set; }
        public List<GameObject> levelPrefabList = new List<GameObject>();

        // Use this for initialization
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }
        void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }
        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState arg1, GameState arg2)
        {
            //GameObject levelToBeSpawned = levelPrefabList[UnityEngine.Random.Range(0,levelPrefabList.Count)];
            //if (arg1.Equals(GameState.Playing))
            //    Instantiate(levelToBeSpawned, transform.position, Quaternion.identity, transform);
        }
    }
}
