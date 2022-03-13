using System;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public Action OnGroundedAnimationComplete;
    
        public void OnGrounded()
        {
            OnGroundedAnimationComplete?.Invoke();
        }
    }
}