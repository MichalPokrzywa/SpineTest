using System.Collections;
using UnityEngine;

namespace Enviroment
{
    public class BoomPopup : MonoBehaviour
    {
        public GameObject speechBubble;
        public float bubbleLifeTime;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameObject bubble = Instantiate(speechBubble, transform.position + Vector3.up * 2, Quaternion.identity);
                StartCoroutine(BubbleLifeTime(bubble));
            }
        }

        private IEnumerator BubbleLifeTime(GameObject bubble)
        {
            yield return new WaitForSeconds(bubbleLifeTime);
            Destroy(bubble);
        }
    }
}
