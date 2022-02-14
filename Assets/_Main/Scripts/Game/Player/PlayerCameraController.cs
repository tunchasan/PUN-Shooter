using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Tooltip("The Player's TPS Camera")] 
        [SerializeField] private GameObject playerCamera = null;

        public void ValidateStatus(bool status)
        {
            // Activates the camera if it's player itself
            playerCamera.SetActive(status);
        }
    }
}