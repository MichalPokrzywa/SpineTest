using Spine.Unity;
using Spine;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using CharacterController = Character.CharacterController;
public class LadderController : MonoBehaviour
{
    [Header("Player References")]
    private SkeletonAnimation skeletonAnimation;
    private Transform player; 

    [Header("Animation Names")]
    [SerializeField] private AnimationReferenceAsset align;
    [SerializeField] private AnimationReferenceAsset climb ;
    [SerializeField] private AnimationReferenceAsset dismount;

    [Header("Ladder Settings")]
    public Transform topPoint; // Top of the ladder
    public Transform bottomPoint; // Bottom of the ladder
    public Transform finalTopPosition; // Final top position after dismount

    [Header("Climb Settings")]
    public float climbSpeed = 1f;

    private bool isClimbing = false; 
    private bool isGoingUp = false;

    public void ClimbLadder(bool isGoingUp)
    {
        if (isClimbing) return;

        this.isGoingUp = isGoingUp;
        player = CharacterController.Instance.transform;
        player.GetComponent<Rigidbody>().useGravity = false;
        skeletonAnimation = CharacterController.Instance.skeletonAnimation;
        switch (isGoingUp)
        {
            case true when IsNear(bottomPoint.position):
                StartClimbing(bottomPoint.position, topPoint.position);
                break;
            case false when IsNear(finalTopPosition.position):
                StartClimbing(topPoint.position, bottomPoint.position);
                break;
        }
    }

    private void StartClimbing(Vector3 start, Vector3 end)
    {
        isClimbing = true;
        TrackEntry alignTrack = isGoingUp
            ? skeletonAnimation.state.SetAnimation(1, align, false)
            : skeletonAnimation.state.SetAnimation(1, dismount, false);
        if (!isGoingUp)
        {
            alignTrack.TimeScale = -1f; // Reverse animation
            alignTrack.TrackTime = alignTrack.AnimationEnd; // Start from the end

            // Monitor reverse playback completion manually
            StartCoroutine(WaitForReverseAnimationCompletion(alignTrack, () =>
            {
                PlayClimbAnimation(start, end);
            }));
        }
        else
        {
            alignTrack.Complete += entry =>
            {
                PlayClimbAnimation(start, end);
            };
        }

        StartCoroutine(MovePlayerDuringAnimation(player.position, start, alignTrack.AnimationEnd, null));
    }

    private void PlayClimbAnimation(Vector3 start, Vector3 end)
    {
        TrackEntry climbTrack = skeletonAnimation.state.SetAnimation(1, climb, true);
        float climbDuration = Vector3.Distance(start, end) / climbSpeed;

        StartCoroutine(MovePlayerDuringAnimation(start, end, climbDuration, PlayDismountAnimation));
    }

    private void PlayDismountAnimation()
    {
        TrackEntry dismountTrack = skeletonAnimation.state.SetAnimation(1, dismount, false);
        StartCoroutine(MovePlayerDuringAnimation(player.position, isGoingUp ? finalTopPosition.position : bottomPoint.position, dismountTrack.AnimationEnd, null));
        dismountTrack.Complete += entry =>
        {
            isClimbing = false;
            CharacterController.Instance.isClimbing = false;
            CharacterController.Instance.SetIdleState();
            player.GetComponent<Rigidbody>().useGravity = true;
            player.position = isGoingUp ? finalTopPosition.position : bottomPoint.position;
        };
    }

    private IEnumerator MovePlayerDuringAnimation(Vector3 start, Vector3 end, float duration, UnityAction onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            player.position = Vector3.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.position = end;
        onComplete?.Invoke();
    }
    private IEnumerator WaitForReverseAnimationCompletion(TrackEntry track, UnityAction onComplete)
    {
        while (track.TrackTime > 0)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }
    private bool IsNear(Vector3 position)
    {
        return Vector3.Distance(player.position, position) < 1f;
    }

}
