using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class WallManager : MonoBehaviour
    {
        public static WallManager Instance { get; set; }
        public List<GameObject> wallList = new List<GameObject>();

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
    }
}
