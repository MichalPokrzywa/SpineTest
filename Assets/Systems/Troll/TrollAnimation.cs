using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace Troll
{
    public class TrollAnimation : MonoBehaviour
    {
        private SkeletonAnimation skeletonAnimation;
        [SerializeField] private bool loopSequence = true; // Whether the sequence should loop
        private bool isLastAnimation;
        public List<AnimationReferenceAsset> animationSequence = new List<AnimationReferenceAsset>();
        private void Start()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();

            // Start looping the animations
            PlayAnimationSequence();
        }

        private void PlayAnimationSequence()
        {
            TrackEntry currentTrack = null;

            // Iterate through the animation sequence
            for (int i = 0; i < animationSequence.Count; i++)
            {
                isLastAnimation = i == animationSequence.Count - 1;

                if (isLastAnimation && loopSequence)
                {
                    currentTrack = skeletonAnimation.state.AddAnimation(0, animationSequence[i], false,0);
                    currentTrack.Complete += _ =>
                    {
                        PlayAnimationSequence(); 
                    };
                }
                else
                {
                    skeletonAnimation.state.AddAnimation(0, animationSequence[i], false, 0);
                }
            }
        }
    }
}
