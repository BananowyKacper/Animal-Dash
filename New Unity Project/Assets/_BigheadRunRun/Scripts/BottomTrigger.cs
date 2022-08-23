using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class BottomTrigger : MonoBehaviour
    {
        public static event System.Action PlayerGotStuck;

        private void OnTriggerStay(Collider other)
        {
            if (PlayerGotStuck != null)
                PlayerGotStuck();
        }
    }
}
