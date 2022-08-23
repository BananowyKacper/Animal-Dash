using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace BigheadRunRun
{
    public class CharacterScroller : MonoBehaviour
    {
        public static CharacterScroller Instance { get; private set; }

        [Header("Scroller Config")]
        public GameObject characterScrollerCamera = null;
        public float minScale = 1f;
        public float maxScale = 1.5f;
        public float characterSpace = 3f;
        public float moveForwardAmount = 2f;
        public float swipeThresholdX = 5f;
        public float swipeThresholdY = 30f;
        public float rotateSpeed = 30f;
        public float snapTime = 0.3f;
        public float resetRotateSpeed = 180f;
        public ScrollerStyle scrollerStyle = ScrollerStyle.Line;
        public bool usingDefaultPosition = true;
        public float characterScrollerRadius = 100f;
        [Range(0.1f, 1f)]
        public float scrollSpeedFactor = 0.25f;
        public Vector3 centerPoint;
        public Vector3 originalScale = Vector3.one;
        public Vector3 originalRotation = Vector3.zero;
        public float touchDragThreshold = 0.2f;

        [Header("Object References")]
        public Text totalCoins;
        public Text priceText;
        public Image priceImg;
        public Button selectButon;
        public Button unlockButton;
        public Button lockButton;
        public Color lockColor = Color.black;
        public Transform defaultPlayerPos;
        public GameObject tapToPlayText;
        public GameObject tapToUnlockText;
        public GameObject notEnoughCoins;
        public GameObject lockCharacterImage;
        public GameObject price;

        List<GameObject> listCharacter = new List<GameObject>();
        GameObject currentCharacter;
        GameObject lastCurrentCharacter;
        IEnumerator rotateCoroutine;
        Vector3 startPos;
        Vector3 endPos;
        float startTime;
        float endTime;
        bool isCurrentCharacterRotating = false;
        public bool hasMoved = false;
        [HideInInspector]
        public GameObject playerTemp;

        public enum ScrollerStyle
        {
            Line,
            Circle
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (currentCharacter)
            {
                StartRotateCurrentCharacter();
            }
        }

        float characterAngleSpace = 1f;
        float currentAngle = 0f;
        // Use this for initialization
        void Start()
        {
            lockColor.a = 0;    // need this for later setting material colors to work

            int currentCharacterIndex = CharacterManager.Instance.CurrentCharacterIndex;
            currentCharacterIndex = Mathf.Clamp(currentCharacterIndex, 0, CharacterManager.Instance.characters.Length - 1);
            centerPoint = transform.TransformPoint(centerPoint);

            switch (scrollerStyle)
            {
                case ScrollerStyle.Line:
                    characterScrollerCamera.GetComponent<Camera>().orthographic = true;
                    break;
                case ScrollerStyle.Circle:
                    characterScrollerCamera.GetComponent<Camera>().orthographic = false;
                    break;
                default:
                    break;
            }

            characterAngleSpace = Mathf.PI * 2 / CharacterManager.Instance.characters.Length;
            currentAngle = currentCharacterIndex * characterAngleSpace;
            for (int i = 0; i < CharacterManager.Instance.characters.Length; i++)
            {
                int deltaIndex = i - currentCharacterIndex;
                if (usingDefaultPosition)
                    centerPoint = CharacterManager.Instance.characters[i].transform.position;
                GameObject character = (GameObject)Instantiate(CharacterManager.Instance.characters[i], centerPoint, Quaternion.Euler(originalRotation.x, originalRotation.y, originalRotation.z));
                DisablePhysics(character);
                //ChangeCharacterLayer(character);
                Character charData = character.GetComponentInChildren<Character>();
                charData.characterSequenceNumber = i;
                listCharacter.Add(character);
                character.transform.localScale = originalScale;

                // Set color based on locking status
                Renderer[] charRdr = character.GetComponentsInChildren<Renderer>();
                if (charData.IsUnlocked)
                {
                    foreach (var item in charRdr)
                    {
                        item.material.SetColor("_Color", Color.white);
                    }
                }
                else
                {
                    foreach (var item in charRdr)
                    {
                        item.material.SetColor("_Color", lockColor);
                    }
                }
                // Set as child of this object
                character.transform.parent = transform;
                switch (scrollerStyle)
                {
                    case ScrollerStyle.Line:
                        character.transform.localPosition += new Vector3(deltaIndex * characterSpace, 0, 0);
                        break;
                    case ScrollerStyle.Circle:
                        character.transform.localPosition = transform.InverseTransformPoint(centerPoint) + new Vector3(Mathf.Sin(-currentAngle + i * characterAngleSpace), 0, -Mathf.Cos(-currentAngle + i * characterAngleSpace)) * characterScrollerRadius;
                        break;
                    default:
                        break;
                }
                // Set layer for camera culling
                character.gameObject.layer = LayerMask.NameToLayer("CharacterSelectionUI");
            }
            // Highlight current character
            currentCharacter = listCharacter[currentCharacterIndex];
            switch (scrollerStyle)
            {
                case ScrollerStyle.Line:
                    currentCharacter.transform.localScale = maxScale * originalScale;
                    currentCharacter.transform.localPosition += moveForwardAmount * Vector3.forward;
                    break;
                case ScrollerStyle.Circle:
                    currentCharacter.transform.localScale = maxScale * originalScale;
                    break;
                default:
                    break;
            }

            lastCurrentCharacter = null;
            StartRotateCurrentCharacter();
            ChangePlayBtnState();
        }

        internal void StartGameOrUnlockCharacter()
        {
            if (tapToPlayText.activeInHierarchy)
            {
                SelectCharacter();
                GameManager.Instance.StartGame();
            }
            else
            {
                UnlockCharacter();
                ChangePlayBtnState();
            }
        }

        private void DisablePhysics(GameObject character)
        {
            foreach (var item in character.GetComponentsInChildren<SpringJoint>())
            {
                DestroyImmediate(item);
            }
            foreach (var item in character.GetComponentsInChildren<Rigidbody>())
            {
                DestroyImmediate(item);
            }
            foreach (var item in character.GetComponentsInChildren<SphereCollider>())
            {
                DestroyImmediate(item);
            }
        }

        private void ChangeCharacterLayer(GameObject character)
        {
            character.transform.GetChild(1).gameObject.layer = 9;
        }

        private IEnumerator DestroyFixedJoint(GameObject character)
        {
            yield return new WaitForEndOfFrame();
            DestroyImmediate(character.transform.GetChild(0).GetComponent<FixedJoint>());
            DestroyImmediate(character.transform.GetChild(0).GetComponent<Rigidbody>());
        }

        // Update is called once per frame
        void Update()
        {
            #region Scrolling
            // Do the scrolling stuff
            if (Input.GetMouseButtonDown(0))    // first touch
            {
                startPos = Input.mousePosition;
                startTime = Time.time;
                hasMoved = false;
            }
            else if (Input.GetMouseButton(0))   // touch stays
            {
                endPos = Input.mousePosition;
                endTime = Time.time;

                float deltaX = Mathf.Abs(startPos.x - endPos.x);
                //float deltaY = Mathf.Abs(startPos.y - endPos.y);

                if (deltaX >= swipeThresholdX)
                {
                    hasMoved = true;
                    if (isCurrentCharacterRotating)
                        StopRotateCurrentCharacter(true);

                    float speed = deltaX / (endTime - startTime);
                    Vector3 dir = (startPos.x - endPos.x < 0) ? Vector3.right : Vector3.left;
                    Vector3 moveVector = dir * (speed / 10) * scrollSpeedFactor * Time.deltaTime;
                    currentAngle -= moveVector.x * characterAngleSpace / 5;
                    if (currentAngle > Mathf.PI * 2)
                    {
                        currentAngle -= Mathf.PI * 2;
                    }
                    else if (currentAngle < 0)
                    {
                        currentAngle += Mathf.PI * 2;
                    }
                    // Move and scale the children
                    for (int i = 0; i < listCharacter.Count; i++)
                    {
                        switch (scrollerStyle)
                        {
                            case ScrollerStyle.Line:
                                MoveAndScale(listCharacter[i].transform, moveVector);
                                break;
                            case ScrollerStyle.Circle:
                                MoveAndScaleCircleVer(listCharacter[i].transform, moveVector, i);
                                break;
                            default:
                                break;
                        }

                    }

                    // Update for next step
                    startPos = endPos;
                    startTime = endTime;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (hasMoved)
                {
                    // Store the last currentCharacter
                    lastCurrentCharacter = currentCharacter;

                    // Update current character to the one nearest to center point
                    switch (scrollerStyle)
                    {
                        case ScrollerStyle.Line:
                            currentCharacter = FindCharacterNearestToCenter();
                            // Snap
                            float snapDistance = transform.InverseTransformPoint(centerPoint).x - currentCharacter.transform.localPosition.x;
                            StartCoroutine(SnapAndRotate(snapDistance));
                            break;
                        case ScrollerStyle.Circle:
                            currentCharacter = FindCharacterNearestToCenterCircleVer();
                            StartCoroutine(SnapAndRotateCircleVer());
                            break;
                        default:
                            break;
                    }
                    ChangePlayBtnState();
                }
            }

            #endregion

            // Update UI
            totalCoins.text = CoinManager.Instance.Coins.ToString();
            Character charData = currentCharacter.GetComponentInChildren<Character>();

            if (!charData.isFree)
            {
                priceText.gameObject.SetActive(true);
                priceText.text = charData.price.ToString();
                priceImg.gameObject.SetActive(true);
            }
            else
            {
                priceText.gameObject.SetActive(false);
                priceImg.gameObject.SetActive(false);
            }

            if (currentCharacter != lastCurrentCharacter)
            {
                if (charData.IsUnlocked)
                {
                    unlockButton.gameObject.SetActive(false);
                    lockButton.gameObject.SetActive(false);
                    selectButon.gameObject.SetActive(true);
                }
                else
                {
                    selectButon.gameObject.SetActive(false);
                    if (CoinManager.Instance.Coins >= charData.price)
                    {
                        unlockButton.gameObject.SetActive(true);
                        lockButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        unlockButton.gameObject.SetActive(false);
                        lockButton.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void ChangePlayBtnState()
        {
            if (currentCharacter.GetComponentInChildren<Character>().IsUnlocked)
            {
                tapToPlayText.SetActive(true);
                tapToUnlockText.SetActive(false);
                notEnoughCoins.SetActive(false);
                price.SetActive(false);
                lockCharacterImage.SetActive(false);
            }
            else
            {
                price.SetActive(true);
                tapToPlayText.SetActive(false);
                if (currentCharacter.GetComponentInChildren<Character>().price <= CoinManager.Instance.Coins)
                {
                    lockCharacterImage.SetActive(false);
                    tapToUnlockText.SetActive(true);
                    notEnoughCoins.SetActive(false);
                }
                else
                {
                    lockCharacterImage.SetActive(true);
                    tapToUnlockText.SetActive(false);
                    notEnoughCoins.SetActive(true);
                }
            }
        }

        void MoveAndScale(Transform tf, Vector3 moveVector)
        {
            // Move
            tf.localPosition += moveVector;

            // Scale and move forward according to distance from current position to center position
            float d = Mathf.Abs(tf.localPosition.x - transform.InverseTransformPoint(centerPoint).x);
            if (d < (characterSpace / 2))
            {
                float factor = 1 - d / (characterSpace / 2);
                float scaleFactor = Mathf.Lerp(minScale, maxScale, factor);
                tf.localScale = scaleFactor * originalScale;

                float fd = Mathf.Lerp(0, moveForwardAmount, factor);
                Vector3 pos = tf.localPosition;
                pos.z = transform.InverseTransformPoint(centerPoint).z + fd;
                tf.localPosition = pos;
            }
            else
            {
                tf.localScale = minScale * originalScale;
                Vector3 pos = tf.localPosition;
                pos.z = transform.InverseTransformPoint(centerPoint).z;
                tf.localPosition = pos;
            }
        }

        void MoveAndScaleCircleVer(Transform tf, Vector3 moveVector, int index)
        {
            // Move
            tf.localPosition = transform.InverseTransformPoint(centerPoint) + new Vector3(Mathf.Sin(-currentAngle + index * characterAngleSpace), 0, -Mathf.Cos(-currentAngle + index * characterAngleSpace)) * characterScrollerRadius;
            //Scale and move forward according to distance from current position to center position
            float d = Mathf.Abs(Vector3.Angle(Vector3.back, (tf.localPosition - transform.InverseTransformPoint(centerPoint)).normalized) * Mathf.Deg2Rad);
            if (d < (characterAngleSpace / 2))
            {
                float factor = 1 - d / (characterAngleSpace / 2);
                float scaleFactor = Mathf.Lerp(minScale, maxScale, factor);
                tf.localScale = scaleFactor * originalScale;
            }
            else
            {
                tf.localScale = minScale * originalScale;
            }
        }

        GameObject FindCharacterNearestToCenter()
        {
            float min = -1;
            GameObject nearestObj = null;

            for (int i = 0; i < listCharacter.Count; i++)
            {
                float d = Mathf.Abs((listCharacter[i].transform.position - centerPoint).magnitude);
                if (d < min || min < 0)
                {
                    min = d;
                    nearestObj = listCharacter[i];
                }
            }

            return nearestObj;
        }

        GameObject FindCharacterNearestToCenterCircleVer()
        {
            GameObject nearestObj = null;

            int neareastObjIndex = Mathf.RoundToInt(currentAngle / characterAngleSpace);
            if (neareastObjIndex < 0)
            {
                neareastObjIndex = listCharacter.Count - 1;
            }
            else if (neareastObjIndex > listCharacter.Count - 1)
            {
                neareastObjIndex = 0;
            }
            nearestObj = listCharacter[neareastObjIndex];
            return nearestObj;
        }


        IEnumerator SnapAndRotate(float snapDistance)
        {
            float snapDistanceAbs = Mathf.Abs(snapDistance);
            float snapSpeed = snapDistanceAbs / snapTime;
            float sign = snapDistance / snapDistanceAbs;
            float movedDistance = 0;

            SoundManager.Instance.PlaySound(SoundManager.Instance.tick);

            while (Mathf.Abs(movedDistance) < snapDistanceAbs)
            {
                float d = sign * snapSpeed * Time.deltaTime;
                float remainedDistance = Mathf.Abs(snapDistanceAbs - Mathf.Abs(movedDistance));
                d = Mathf.Clamp(d, -remainedDistance, remainedDistance);

                Vector3 moveVector = new Vector3(d, 0, 0);
                for (int i = 0; i < listCharacter.Count; i++)
                {
                    MoveAndScale(listCharacter[i].transform, moveVector);
                }

                movedDistance += d;
                yield return null;
            }

            if (currentCharacter != lastCurrentCharacter || !isCurrentCharacterRotating)
            {
                // Stop rotating the last current character
                StopRotateCurrentCharacter();

                // Now rotate the new current character
                StartRotateCurrentCharacter();
            }
        }

        IEnumerator SnapAndRotateCircleVer()
        {
            float nextAngle = Mathf.RoundToInt(currentAngle / characterAngleSpace) * characterAngleSpace;
            SoundManager.Instance.PlaySound(SoundManager.Instance.tick);
            while (Mathf.Abs(currentAngle - nextAngle) > 0.01f)
            {
                Vector3 moveVector = new Vector3((nextAngle - currentAngle) / snapTime * 10 * Time.deltaTime, 0, 0);
                currentAngle += moveVector.x * characterAngleSpace;
                if (currentAngle > Mathf.PI * 2)
                {
                    currentAngle -= Mathf.PI * 2;
                }
                else if (currentAngle < 0)
                {
                    currentAngle += Mathf.PI * 2;
                }
                for (int i = 0; i < listCharacter.Count; i++)
                {
                    MoveAndScaleCircleVer(listCharacter[i].transform, moveVector, i);
                }
                yield return null;
            }
            if (currentCharacter != lastCurrentCharacter || !isCurrentCharacterRotating)
            {
                // Stop rotating the last current character
                StopRotateCurrentCharacter();

                // Now rotate the new current character
                StartRotateCurrentCharacter();
            }
        }

        void StartRotateCurrentCharacter()
        {
            StopRotateCurrentCharacter(false);   // stop previous rotation if any
            rotateCoroutine = CRRotateCharacter(currentCharacter.transform);
            StartCoroutine(rotateCoroutine);
            isCurrentCharacterRotating = true;
        }

        void StopRotateCurrentCharacter(bool resetRotation = false)
        {
            if (rotateCoroutine != null)
            {
                StopCoroutine(rotateCoroutine);
            }

            isCurrentCharacterRotating = false;

            if (resetRotation)
                StartCoroutine(CRResetCharacterRotation(currentCharacter.transform));
        }

        IEnumerator CRRotateCharacter(Transform charTf)
        {
            while (true)
            {
                charTf.Rotate(new Vector3(0, rotateSpeed * Time.deltaTime, 0));
                yield return null;
            }
        }

        IEnumerator CRResetCharacterRotation(Transform charTf)
        {
            Vector3 startRotation = charTf.rotation.eulerAngles;
            Vector3 endRotation = originalRotation;
            float timePast = 0;
            float rotateAngle = Mathf.Abs(endRotation.y - startRotation.y);
            float rotateTime = rotateAngle / resetRotateSpeed;

            while (timePast < rotateTime)
            {
                timePast += Time.deltaTime;
                Vector3 rotation = Vector3.Lerp(startRotation, endRotation, timePast / rotateTime);
                charTf.rotation = Quaternion.Euler(rotation);
                yield return null;
            }
        }

        public void UnlockCharacter()
        {
            bool unlockSucceeded = currentCharacter.GetComponentInChildren<Character>().Unlock();
            if (unlockSucceeded)
            {
                Renderer[] charRdr = currentCharacter.GetComponentsInChildren<Renderer>();
                foreach (var item in charRdr)
                {
                    item.material.SetColor("_Color", Color.white);
                }
                unlockButton.gameObject.SetActive(false);
                selectButon.gameObject.SetActive(true);

                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            }
        }

        public void SelectCharacter()
        {
            CharacterManager.Instance.CurrentCharacterIndex = currentCharacter.GetComponentInChildren<Character>().characterSequenceNumber;
            playerTemp = Instantiate(CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex]) as GameObject;
        }
    }
}
