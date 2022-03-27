using System;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public Action OnGroundedAnimationComplete;
    
        public void OnGrounded()
        {
            OnGroundedAnimationComplete?.Invoke();
        }
    }
}