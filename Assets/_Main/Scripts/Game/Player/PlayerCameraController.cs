using Cinemachine;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Tooltip("The Player's TPS Camera")] 
        [SerializeField] private CinemachineVirtualCamera playerCamera = null;

        public void ValidateStatus(bool status)
        {
            // Activates the camera if it's player itself
            playerCamera.gameObject.SetActive(status);
        }

        public void ProcessState(Enums.PlayerStates state)
        {
            switch (state)
            {
                case Enums.PlayerStates.None:
                {
                    // TODO
                    
                    return;
                }

                case Enums.PlayerStates.OnIdle:
                {
                    // TODO
                    
                    return;
                }
                
                case Enums.PlayerStates.OnJump:
                {
                    // TODO
                    
                    return;
                }
                
                case Enums.PlayerStates.OnRun:
                {
                    // TODO
                    
                    return;
                }
                
                case Enums.PlayerStates.OnAim:
                {
                    // TODO
                    
                    return;
                }
                
                case Enums.PlayerStates.OnShoot:
                {
                    // TODO
                    
                    return;
                }
                
                case Enums.PlayerStates.OnDeath:
                {
                    // TODO
                    
                    return;
                }
            }
        }
    }
}