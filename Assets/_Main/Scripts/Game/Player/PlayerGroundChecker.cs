using System.Collections;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerGroundChecker : MonoBehaviour
    {
        #region Private Fields
        
        private CharacterController _characterController = null;

        private PlayerBase _player = null;

        private PlayerAnimationController _animationController = null;

        private PlayerAnimationEvents _playerAnimationEvents = null;
        
        #endregion
        
        #region MonoBehaviour Callbacks

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animationController = GetComponent<PlayerAnimationController>();
            _playerAnimationEvents = GetComponentInChildren<PlayerAnimationEvents>();
            _player = GetComponent<PlayerBase>();
        }

        private void Start()
        {
            StartCoroutine(ValidateStatus());
        }

        private void OnEnable()
        {
            _playerAnimationEvents.OnGroundedAnimationComplete += ProcessOnGroundedAction;
        }
        
        private void OnDisable()
        {
            _playerAnimationEvents.OnGroundedAnimationComplete -= ProcessOnGroundedAction;
        }

        #endregion

        #region Private Methods

        private IEnumerator ValidateStatus()
        {
            while (true)
            {
                ChangeStatus(!_characterController.isGrounded);
                
                yield return new WaitForSeconds(.1F);
            }
        }

        private void ChangeStatus(bool isFalling)
        {
            var nextStatus = isFalling ? 
                Enums.PlayerStates.OnFalling : 
                Enums.PlayerStates.OnGrounded;

            if (nextStatus != _player.CurrentState)
            {
                ProcessStatus(nextStatus);
            }
        }

        private void ProcessStatus(Enums.PlayerStates state)
        {
            switch (state)
            {
                case Enums.PlayerStates.OnFalling:
                {
                    _animationController.PlayFallingAnimation();

                    _player.UpdateState(Enums.PlayerStates.OnFalling);
                    
                    break;
                }

                case Enums.PlayerStates.OnGrounded:
                {
                    if (_player.CurrentState == Enums.PlayerStates.OnFalling)
                    {
                        _animationController.PlayGroundedAnimation();

                        _player.UpdateState(Enums.PlayerStates.OnGrounded);
                    }
                    
                    break;
                }
            }
        }

        private void ProcessOnGroundedAction()
        {
            if (_player.CurrentState == Enums.PlayerStates.OnGrounded)
            {
                Debug.Log("Player has grounded");

                _player.UpdateState(Enums.PlayerStates.OnIdle);
            }
        }

        #endregion
    } 
}