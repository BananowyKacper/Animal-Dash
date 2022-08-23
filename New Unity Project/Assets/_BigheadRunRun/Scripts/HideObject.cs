using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class HideObject : MonoBehaviour
    {

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("CleanUp"))
            {
                if (!transform.parent.gameObject.Equals(GameManager.Instance.originalWall))
                {
                    transform.parent.gameObject.SetActive(false);
                    WallManager.Instance.wallList.Add(transform.parent.gameObject);
                }
            }
        }
    }
}
