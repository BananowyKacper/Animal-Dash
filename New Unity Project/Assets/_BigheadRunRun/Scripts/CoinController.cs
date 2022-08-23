using UnityEngine;
using System.Collections;
using System;

namespace BigheadRunRun
{
    public class CoinController : MonoBehaviour
    {

        private bool stop;
        // Use this for initialization
        void Start()
        {
            StartCoroutine(ScaleUp());
        }

        private IEnumerator ScaleUp()
        {
            Vector3 normalScale = transform.localScale;
            transform.localScale = Vector3.zero;
            do
            {
                transform.localScale += normalScale * Time.deltaTime;
                yield return null;
            } while (transform.localScale.z <= normalScale.z);
            StartCoroutine(Bounce());
            StartCoroutine(Rotate());
        }

        public void GoUp()
        {
            stop = true;
            StartCoroutine(Up());
        }

        IEnumerator Rotate()
        {
            while (true)
            {
                transform.Rotate(Vector3.up * 2f);
                yield return null;
            }
        }

        IEnumerator Bounce()
        {
            while (true)
            {
                float bounceTime = 1f;

                float startY = transform.position.y;
                float endY = startY + 0.5f;

                float t = 0;
                while (t < bounceTime / 2f)
                {
                    if (stop)
                        yield break;
                    t += Time.deltaTime;
                    float fraction = t / (bounceTime / 2f);
                    float newY = Mathf.Lerp(startY, endY, fraction);
                    Vector3 newPos = transform.position;
                    newPos.y = newY;
                    transform.position = newPos;
                    yield return null;
                }

                float r = 0;
                while (r < bounceTime / 2f)
                {
                    if (stop)
                        yield break;
                    r += Time.deltaTime;
                    float fraction = r / (bounceTime / 2f);
                    float newY = Mathf.Lerp(endY, startY, fraction);
                    Vector3 newPos = transform.position;
                    newPos.y = newY;
                    transform.position = newPos;
                    yield return null;
                }
            }
        }

        //Move up
        IEnumerator Up()
        {
            float time = 1f;

            float startY = transform.position.y;
            float endY = startY + 10f;

            float t = 0;
            while (t < time / 2f)
            {
                t += Time.deltaTime;
                float fraction = t / (time / 2f);
                float newY = Mathf.Lerp(startY, endY, fraction);
                Vector3 newPos = transform.position;
                newPos.y = newY;
                transform.position = newPos;
                yield return null;
            }

            gameObject.SetActive(false);
            GetComponent<MeshCollider>().enabled = true;
            transform.position = Vector3.zero;
            transform.parent = CoinManager.Instance.transform;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("DeathPlane"))
            {
                Destroy(gameObject);
            }
            if (other.tag.Equals("Player"))
            {
                CoinManager.Instance.AddCoins(1);
                SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
                StartCoroutine(ScaleDownThenDestroy());
            }
        }

        private IEnumerator ScaleDownThenDestroy()
        {
            Vector3 normalScale = transform.localScale;
            float time = 1;
            while (time > 0)
            {
                transform.localScale = normalScale * (2 - Mathf.Pow((time - 0.75f) * 4, 2));
                if (transform.localScale.z < 0)
                    transform.localScale = Vector3.zero;
                time -= Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
