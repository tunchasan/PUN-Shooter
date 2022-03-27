using Photon.Pun;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CharacterAnimation : MonoBehaviourPun
    {
        #region Private Serializable Fields

        [SerializeField]
        private float directionDampTime = 0.25f;

        #endregion
        
        #region Private Fields

        private Animator _animator = null;
        
        private Vector2 _direction = Vector2.zero;

        #endregion

        #region Const Fields

        private const string DirectionX = "directionX";
        private const string DirectionY = "directionY";
        private const string IsFalling = "isFalling";
        private const string IsGrounded = "isGrounded";
        private const string Speed = "speed";

        #endregion
        
        #region MonoBehaviour Callbacks

        // Use this for initialization
        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            
            if (!_animator)
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        #endregion

        #region Private Methods

        private bool CanProcessAnimation()
        {
            if (!_animator) return false;

            return true;
        }

        #endregion
        
        #region Public Methods

        public void ProcessLocomotion(Vector2 direction, bool isRunning)
        {
            if (CanProcessAnimation())
            {
                _animator.SetFloat(DirectionX, direction.x, .1F, Time.deltaTime);
                _animator.SetFloat(DirectionY, direction.y,.1F, Time.deltaTime);
                _animator.SetFloat(Speed, isRunning ? 1F : 0F, .2F, Time.deltaTime);
            }
        }

        public void PlayFallingAnimation()
        {
            _animator.SetTrigger(IsFalling);
        }

        public void PlayGroundedAnimation()
        {
            _animator.SetTrigger(IsGrounded);
        }

        #endregion
    }
}