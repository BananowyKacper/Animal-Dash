using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class PlaneManager : MonoBehaviour
    {

        public static PlaneManager Instance { get; set; }
        public List<GameObject> planeHolderPrefabList = new List<GameObject>();

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
