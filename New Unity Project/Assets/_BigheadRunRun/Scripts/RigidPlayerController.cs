using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigheadRunRun
{
    public class RigidPlayerController : MonoBehaviour
    {
        public static event System.Action PlayerHaveJump = delegate { };
        public PlayerController playerController;
        public Rigidbody rb;
        float startZ = 0;
        [HideInInspector]
        public int lastdir = 1;
        int lane = 1;
        [HideInInspector]
        public bool onGround = true;
        float startJumpTime = 0;
        public bool overrideTransformX = false;
        public int numberOfRolling = 2;
        float playerStuckCountingDownTime = 0.3f;
        Vector3[] centerPosList = new Vector3[10];
        public float objectHeigh;
        public LayerMask groundLayerMask;
        [HideInInspector]
        public Vector3 playerSpeed = Vector3.zero;
        // Use this for initialization
        void Start()
        {
            if (rb)
            {
                Jump(-1);
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                for (int i = 0; i < 10; i++)
                {
                    centerPosList[i] = transform.position;
                }
                startZ = transform.position.z;
            }
        }

        private void Jump(int v)
        {
            CheckOnGround();
            if (onGround && lastdir != v)
            {
                PlayerHaveJump();
                startJumpTime = Time.time;
                overrideTransformX = false;
                StartCoroutine(JumpCoroutine(v, startJumpTime));
                lastdir = v;
                onGround = false;
                if (GameManager.Instance.pointCountingMode.Equals(GameManager.PointCountingMode.ByJumping))
                    ScoreManager.Instance.AddScore(GameManager.Instance.pointPerjump);
                SoundManager.Instance.PlaySound(SoundManager.Instance.jump);
            }
        }

        private IEnumerator JumpCoroutine(int dir, float startTime)
        {
            rb.useGravity = false;
            float distance = 0;
            float startY = transform.position.y;
            if (dir > 0)
                distance = GameManager.Instance.rCenter.position.x - transform.position.x;
            else
                distance = GameManager.Instance.lCenter.position.x - transform.position.x;
            distance = Mathf.Abs(distance);
            float hasMovedDistance = 0;
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            while (startTime == startJumpTime && hasMovedDistance < distance && GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                float d = dir * Time.deltaTime * distance / GameManager.Instance.jumpTime;
                if (!overrideTransformX)
                    transform.position += new Vector3(1, 0, 0) * d;
                float ytemp = (1 - Mathf.Pow((hasMovedDistance / distance - 0.5f) * 2, 2)) * GameManager.Instance.jumpHeight;
                Vector3 transformTemp = transform.position;
                transformTemp.y = startY + ytemp;
                transform.position = transformTemp;
                hasMovedDistance += Mathf.Abs(d);
                float rotationZ = Mathf.Lerp(0f, 360f * numberOfRolling, Mathf.Clamp(hasMovedDistance / distance, 0, 1));
                if (dir > 0)
                    rotationZ = Mathf.Lerp(0f, 360f * numberOfRolling, Mathf.Clamp(1 - hasMovedDistance / distance, 0, 1));
                transform.localEulerAngles = new Vector3(0, 0, rotationZ);
                yield return new WaitForEndOfFrame();
            }
            if (startTime == startJumpTime && !overrideTransformX && GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                Vector3 transformTemp = transform.position;
                if (dir > 0)
                    transformTemp.x = GameManager.Instance.rCenter.position.x;
                else
                    transformTemp.x = GameManager.Instance.lCenter.position.x;
                transform.position = transformTemp;
                rb.constraints = RigidbodyConstraints.None;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                lane = -lane;
                rb.useGravity = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                if ((GameManager.Instance.pointCountingMode.Equals(GameManager.PointCountingMode.ByDistance)) && (transform.position.z - startZ) / GameManager.Instance.distancePerPoint >= 1)
                {
                    startZ = transform.position.z;
                    ScoreManager.Instance.AddScore(1);
                }
                //running
                float speedZ = GetCenterVelocity().z;
                if (speedZ < GameManager.Instance.playerSpeed / 3 && playerStuckCountingDownTime == 0.3f)
                    StartCoroutine(CheckPlayerStuck());
                Vector3 playerPos = transform.position;
                playerPos.z += GameManager.Instance.playerSpeed * 1.1f * Time.deltaTime;
                transform.position = playerPos;
                if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
                {
                    lane = FindClosestLane();
                    Jump(-lane);
                }
            }
        }

        private void FixedUpdate()
        {
            if (rb)
                rb.AddForce(Vector3.down * 500, ForceMode.Acceleration);
        }

        private IEnumerator CheckPlayerStuck()
        {
            playerStuckCountingDownTime -= Time.deltaTime;
            while (playerStuckCountingDownTime > 0)
            {
                float speedz = GetCenterVelocity().z;
                if (speedz > GameManager.Instance.playerSpeed / 2)
                {
                    playerStuckCountingDownTime = 0.3f;
                    break;
                }
                playerStuckCountingDownTime -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            if (playerStuckCountingDownTime != 0.3f)
            {
                Die();
            }
        }

        private void Die()
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;
            GetComponent<PlayerController>().Die();
            GetComponentInChildren<Animator>().SetTrigger("Die");
        }

        public Vector3 GetCenterVelocity()
        {
            Vector3 result = Vector3.zero;
            for (int i = 0; i < 9; i++)
            {
                centerPosList[i] = centerPosList[i + 1];
            }
            centerPosList[9] = transform.position;
            result = (centerPosList[9] - centerPosList[0]) / (Time.deltaTime * 9);
            playerSpeed = result;
            return result;
        }

        private int FindClosestLane()
        {
            if (Mathf.Abs(transform.position.x - GameManager.Instance.lCenter.position.x) < Mathf.Abs(transform.position.x - GameManager.Instance.rCenter.position.x))
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        private void CheckOnGround()
        {
            if (GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                RaycastHit hit;
                if (Physics.SphereCast(transform.position, 5, Vector3.down, out hit, objectHeigh + 10, groundLayerMask, QueryTriggerInteraction.Collide))
                {
                    onGround = true;
                }
                else
                {
                    onGround = false;
                }
            }
        }

        //hit death plane
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("DeathPlane") && GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                Die();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag.Equals("DeathPlane") && GameManager.Instance.GameState.Equals(GameState.Playing))
            {
                Die();
            }
        }
    }
}
