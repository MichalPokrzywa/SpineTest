using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using Animation = UnityEngine.Animation;
using AnimationState = Spine.AnimationState;
using Event = Spine.Event;

namespace Character
{
    public class CharacterController : Singleton<CharacterController>
    {
        [Header("Character Settings")]
        public float speed = 5f;
        public CharacterState characterState;
        [Header("Animations")]
        [SerializeField]
        private AnimationReferenceAsset idle;
        [SerializeField]
        private AnimationReferenceAsset walking;
        [SerializeField]
        private AnimationReferenceAsset rotate;
        [SerializeField]
        private AnimationReferenceAsset cough;

        private SkeletonAnimation skeletonAnimation;
        private AnimationState animationState;
        private bool movingRight = true;
        private bool isTurning = false;
        private bool isCoughing = false;
        private Vector3 lastFramePosition;

        void Start()
        {
            characterState = CharacterState.Idle;
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            animationState = skeletonAnimation.AnimationState;

            // Optional: Configure default mixing
            animationState.Data.SetMix(idle.name, walking.name, 0.2f);
            animationState.Data.SetMix(walking.name, idle.name, 0.2f);
            animationState.Data.SetMix(walking.name, rotate.name, 0.1f);
            animationState.Data.SetMix(rotate.name, idle.name, 0.2f);
            animationState.Data.SetMix(walking.name, cough.name, 0.1f);
            animationState.Data.SetMix(cough.name, idle.name, 0.1f);
            animationState.Data.SetMix(cough.name, walking.name, 0.1f);
        }

        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            if (!isCoughing)
            {
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
                        SetAnimation(1, walking, true, 1f, MixBlend.Replace);
                    }
                    else if (horizontal == 0 && characterState == CharacterState.Moving)
                    {
                        characterState = CharacterState.Idle;
                        SetAnimation(1, idle, true, 1f, MixBlend.Replace);
                    }
                    transform.Translate(new Vector3(horizontal, 0, 0) * speed * Time.deltaTime);
                }
            }

        }

        public void TriggerCoughAnimation()
        {
            isCoughing = true;
            TrackEntry coughTrack = animationState.SetAnimation(1, cough, false);
            coughTrack.MixBlend = MixBlend.Replace;
            coughTrack.Alpha = 1f;
            coughTrack.Complete += entry =>
            {
                switch (characterState)
                {
                    case CharacterState.Idle:
                        SetAnimation(1, idle, true, 1f, MixBlend.Replace);
                        break;
                    case CharacterState.Moving:
                        SetAnimation(1, walking, true, 1f, MixBlend.Replace);
                        break;
                }

                isCoughing = false;
            };

        }
        private void TriggerTurnAnimation()
        {
            isTurning = true;
            TrackEntry rotateTrack = animationState.SetAnimation(1, rotate, false);
            rotateTrack.MixBlend = MixBlend.Replace;
            rotateTrack.Complete += EntryOnComplete;
        }

        private void EntryOnComplete(TrackEntry trackentry)
        {
            SetAnimation(1, idle, true, 1f, MixBlend.Replace);
            Vector3 scale = transform.localScale;
            scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
            isTurning = false;
        }
        private void SetAnimation(int trackIndex, AnimationReferenceAsset animation, bool loop, float alpha, MixBlend blendMode)
        {
            TrackEntry track = animationState.SetAnimation(trackIndex, animation, loop);
            track.MixBlend = blendMode;
            track.Alpha = alpha;
        }
    }

    public enum CharacterState
    {
        Idle = 0,
        Moving = 1
    }
}
