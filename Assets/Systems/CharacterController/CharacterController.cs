using Spine.Unity;
using UnityEngine;

namespace CharacterController
{
    public class CharacterController : MonoBehaviour
    {
        public SkeletonAnimation skeletonAnimation;
        public float speed = 5f;

        void Start()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            transform.Translate(new Vector3(horizontal, 0, 0) * speed * Time.deltaTime);
        }
    }
}
