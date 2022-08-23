using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class BrokenPlane : MonoBehaviour
    {
        public Rigidbody brokenPlaneRigidbody;
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Player"))
            {
                brokenPlaneRigidbody.useGravity = true;
                brokenPlaneRigidbody.isKinematic = false;
            }
        }
    }
}
