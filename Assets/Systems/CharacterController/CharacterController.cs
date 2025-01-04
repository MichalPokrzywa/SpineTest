using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using Animation = UnityEngine.Animation;
using AnimationState = Spine.AnimationState;
using Event = Spine.Event;

namespace CharacterController
{
    public class CharacterController : MonoBehaviour
    {
        public SkeletonAnimation skeletonAnimation;
        public SkeletonRootMotion skeletonRootMotion;
        private AnimationState animationState;
        public float speed = 5f;
        public CharacterState characterState;
        private bool movingRight = true;
        private bool isTurning = false;
        private Vector3 lastFramePosition;
        public AnimationReferenceAsset idle;
        public AnimationReferenceAsset walking;
        public AnimationReferenceAsset rotate;

        void Start()
        {
            characterState = CharacterState.Idle;
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            skeletonRootMotion = GetComponent<SkeletonRootMotion>();
            animationState = skeletonAnimation.AnimationState;
        }

        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            if (!isTurning)
            {
                if (horizontal < 0 && movingRight || horizontal > 0 && !movingRight)
                {
                    TriggerTurnAnimation();
                }
                movingRight = horizontal switch
                {
                    > 0 => true,
                    < 0 => false,
                    _ => movingRight
                };
            }

            if (!isTurning)
            {


                if (horizontal != 0 && characterState == CharacterState.Idle)
                {
                    characterState = CharacterState.Moving;
                    skeletonAnimation.AnimationState.SetAnimation(0, walking, true);
                }
                else if (horizontal == 0 && characterState == CharacterState.Moving)
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, idle, true);
                    characterState = CharacterState.Idle;
                }
                transform.Translate(new Vector3(horizontal, 0, 0) * speed * Time.deltaTime);
            }
        }

        private void TriggerTurnAnimation()
        {
            isTurning = true;
            animationState.SetAnimation(0, rotate, false).Complete += EntryOnComplete;
        }

        private void EntryOnComplete(TrackEntry trackentry)
        {
            Debug.Log("kek");
            Vector3 scale = transform.localScale;
            scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
            isTurning = false;
        }
    }

    public enum CharacterState
    {
        Idle = 0,
        Moving = 1
    }
}
