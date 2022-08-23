using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class MovingPlane : MonoBehaviour
    {

        public Transform lPivot;
        public Transform rPivot;
        public Transform movingPlane;
        public bool changePlayerLane = true;
        public bool startAtLeftPos = true;
        public float movingSpeed = 40;
        bool hasMove = false;
        bool playerHaveJumpOff = false;
        bool relateToPlayerSpeed = true;

        // Use this for initialization
        void Start()
        {
            RigidPlayerController.PlayerHaveJump += OnPlayerHaveJump;
            if (startAtLeftPos)
                movingPlane.localPosition = lPivot.localPosition;
            else
                movingPlane.localPosition = rPivot.localPosition;
        }

        private void OnDestroy()
        {
            RigidPlayerController.PlayerHaveJump -= OnPlayerHaveJump;
        }

        private void OnPlayerHaveJump()
        {
            if (hasMove)
            {
                playerHaveJumpOff = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Player") && !hasMove)
            {
                hasMove = true;
                RigidPlayerController rigidPlayerController = other.GetComponent<RigidPlayerController>();

                if (rigidPlayerController != null)
                {
                    if (Mathf.Abs(other.transform.position.x - movingPlane.position.x) > Mathf.Abs((GameManager.Instance.lCenter.position.x - GameManager.Instance.rCenter.position.x) / 8))
                    {
                        playerHaveJumpOff = true;
                    }
                }
                StartCoroutine(MovePlane(other.gameObject));
            }
        }

        private IEnumerator MovePlane(GameObject player)
        {
            yield return null;
            float distance = (rPivot.position - lPivot.position).magnitude;
            int dir = 1;
            if (changePlayerLane)
            {
                if (startAtLeftPos)
                {
                    distance = rPivot.position.x - movingPlane.position.x;
                    dir = 1;
                }
                else
                {
                    distance = lPivot.position.x - movingPlane.position.x;
                    dir = -1;
                }
            }
            distance = Mathf.Abs(distance);
            float hasMovedDistance = 0;
            while (hasMovedDistance < distance)
            {
                if (player != null)
                {
                    if (relateToPlayerSpeed && !changePlayerLane)
                    {
                        RigidPlayerController rbController = player.GetComponent<RigidPlayerController>();
                        if (rbController)
                            movingSpeed = rbController.playerSpeed.z * 0.95f;

                    }
                    float d = dir * Time.deltaTime * movingSpeed;
                    if (changePlayerLane)
                    {
                        movingPlane.position += new Vector3(1, 0, 0) * d;
                    }
                    else
                        movingPlane.position += new Vector3(0, 0, 1) * d;
                    if ((!playerHaveJumpOff) && (changePlayerLane))
                    {
                        RigidPlayerController rigidPlayerController = player.GetComponent<RigidPlayerController>();
                        if (rigidPlayerController != null)
                        {
                            rigidPlayerController.overrideTransformX = true;
                            rigidPlayerController.transform.position += new Vector3(1, 0, 0) * d;
                            rigidPlayerController.lastdir = 0;
                        }
                    }
                    hasMovedDistance += Mathf.Abs(d);
                    yield return new WaitForEndOfFrame();
                }
                else
                    yield break;
            }
        }
    }
}
