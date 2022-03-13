using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class Enums : MonoBehaviour
    {
        public enum PlayerStates
        {
            None,
            OnIdle,
            OnJump,
            OnRun,
            OnAim,
            OnShoot,
            OnDeath,
            OnFalling,
            OnGrounded
        }
    }
}