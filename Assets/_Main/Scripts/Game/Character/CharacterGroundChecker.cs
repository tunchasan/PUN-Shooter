using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Helper class that detects slopes, heights and simulates
    /// character's falling action with it's animations.
    /// </summary>
    public class CharacterGroundChecker : MonoBehaviour
    {
        #region Summary

        [Title("Summary")]
        [InfoBox("Helper class that detects slopes, heights and simulates " +
                 "character's falling action with it's animations.")]
        [HideLabel]
        [DisplayAsString]
        public string summary = "";

        #endregion
        
        #region Private Fields
        
        private Character _character = null;

        private UnityEngine.CharacterController _characterController = null;

        private CharacterAnimation _animationController = null;

        private CharacterAnimationEvents _animationEventHandler = null;
        
        #endregion
        
        #region MonoBehaviour Callbacks

        private void OnEnable()
        {
            _animationEventHandler.OnGroundedAnimationComplete += ProcessOnGroundedAction;
        }
        
        private void OnDisable()
        {
            _animationEventHandler.OnGroundedAnimationComplete -= ProcessOnGroundedAction;
        }

        #endregion

        #region Public Methods

        public void InitializeSystem(Character character, 
            UnityEngine.CharacterController controller, 
            CharacterAnimation characterAnimation, 
            CharacterAnimationEvents animationEvents)
        {
            _character = character;
            _characterController = controller;
            _animationController = characterAnimation;
            _animationEventHandler = animationEvents;
            
            StartCoroutine(ValidateStatus());
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

            if (nextStatus != _character.CurrentState)
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

                    _character.UpdateState(Enums.PlayerStates.OnFalling);
                    
                    break;
                }

                case Enums.PlayerStates.OnGrounded:
                {
                    if (_character.CurrentState == Enums.PlayerStates.OnFalling)
                    {
                        _animationController.PlayGroundedAnimation();

                        _character.UpdateState(Enums.PlayerStates.OnGrounded);
                    }
                    
                    break;
                }
            }
        }

        private void ProcessOnGroundedAction()
        {
            if (_character.CurrentState == Enums.PlayerStates.OnGrounded)
            {
                Debug.Log("Character has grounded");

                _character.UpdateState(Enums.PlayerStates.OnIdle);
            }
        }

        #endregion
    } 
}