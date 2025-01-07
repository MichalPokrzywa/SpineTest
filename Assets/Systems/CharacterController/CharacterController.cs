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

        [HideInInspector]
        public SkeletonAnimation skeletonAnimation { get; private set; }
        private AnimationState animationState;
        private bool movingRight = true;
        private bool isTurning = false;
        private bool isCoughing = false;
        [HideInInspector]
        public bool isClimbing = false;
        private Vector3 lastFramePosition;
        private LadderController nearbyLadder;
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
            if (isCoughing || isTurning || isClimbing) return;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0)
            {
                HandleMovement(horizontal);
            }
            if (vertical != 0)
            {
                HandleLadderInteraction(vertical);
            }
            else if (horizontal == 0 && vertical == 0 && characterState == CharacterState.Moving)
            {
                SetIdleState();
            }
        }

        private void HandleMovement(float horizontal)
        {
            if ((horizontal > 0 && !movingRight) || (horizontal < 0 && movingRight))
            {
                TriggerTurnAnimation();
                movingRight = horizontal > 0;
            }
            else
            {
                if (characterState == CharacterState.Idle)
                {
                    SetWalkingState();
                }
                transform.Translate(Vector3.right * horizontal * speed * Time.deltaTime);
            }
        }
        private void HandleLadderInteraction(float vertical)
        {
            if (nearbyLadder == null) return;

            bool goingUp = vertical > 0;
            Debug.Log(goingUp);
            isClimbing = true;
            nearbyLadder.ClimbLadder(goingUp);
        }

        private void SetWalkingState()
        {
            characterState = CharacterState.Moving;
            SetAnimation(1, walking, true);
        }

        public void SetIdleState()
        {
            characterState = CharacterState.Idle;
            SetAnimation(1, idle, true);
        }

        public void TriggerCoughAnimation()
        {
            if (isCoughing) return;

            isCoughing = true;
            TrackEntry coughTrack = animationState.SetAnimation(1, cough, false);

            coughTrack.Complete += entry =>
            {
                SetAnimation(1, characterState == CharacterState.Idle ? idle : walking, true);
                isCoughing = false;
            };
        }

        private void TriggerTurnAnimation()
        {
            isTurning = true;
            TrackEntry rotateTrack = animationState.SetAnimation(1, rotate, false);

            rotateTrack.Complete += entry =>
            {
                FlipCharacter();
                SetIdleState();
                isTurning = false;
            };
        }
        
        private void FlipCharacter()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out LadderController ladder))
            {
                nearbyLadder = ladder;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out LadderController ladder) && ladder == nearbyLadder)
            {
                nearbyLadder = null;
            }
        }
        private void SetAnimation(int trackIndex, AnimationReferenceAsset animation, bool loop)
        {
            animationState.SetAnimation(trackIndex, animation, loop);
        }
    }

    public enum CharacterState
    {
        Idle,
        Moving
    }
}
