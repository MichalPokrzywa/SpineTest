using UnityEngine;
using CharacterController = Character.CharacterController;

namespace Enviroment
{
    public class CoughEnviromentTrigger : MonoBehaviour
    {
        public AudioClip coughSound;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //skeletonAnimation.state.SetAnimation(0, "cough", false);
                AudioSource.PlayClipAtPoint(coughSound, transform.position);
                CharacterController.Instance.TriggerCoughAnimation();
            }
        }
    }
}
