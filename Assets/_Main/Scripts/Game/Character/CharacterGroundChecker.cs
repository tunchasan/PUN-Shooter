using System.Collections;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CharacterGroundChecker : MonoBehaviour
    {
        #region Private Fields
        
        [Header("@References")]
        [SerializeField] private  Character character = null;

        [SerializeField] private UnityEngine.CharacterController characterController = null;

        [SerializeField] private CharacterAnimation animationController = null;

        [SerializeField] private CharacterAnimationEvents animationEventHandler = null;
        
        #endregion
        
        #region MonoBehaviour Callbacks

        private void Start()
        {
            StartCoroutine(ValidateStatus());
        }

        private void OnEnable()
        {
            animationEventHandler.OnGroundedAnimationComplete += ProcessOnGroundedAction;
        }
        
        private void OnDisable()
        {
            animationEventHandler.OnGroundedAnimationComplete -= ProcessOnGroundedAction;
        }

        #endregion

        #region Private Methods

        private IEnumerator ValidateStatus()
        {
            while (true)
            {
                ChangeStatus(!characterController.isGrounded);
                
                yield return new WaitForSeconds(.1F);
            }
        }

        private void ChangeStatus(bool isFalling)
        {
            var nextStatus = isFalling ? 
                Enums.PlayerStates.OnFalling : 
                Enums.PlayerStates.OnGrounded;

            if (nextStatus != character.CurrentState)
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
                    animationController.PlayFallingAnimation();

                    character.UpdateState(Enums.PlayerStates.OnFalling);
                    
                    break;
                }

                case Enums.PlayerStates.OnGrounded:
                {
                    if (character.CurrentState == Enums.PlayerStates.OnFalling)
                    {
                        animationController.PlayGroundedAnimation();

                        character.UpdateState(Enums.PlayerStates.OnGrounded);
                    }
                    
                    break;
                }
            }
        }

        private void ProcessOnGroundedAction()
        {
            if (character.CurrentState == Enums.PlayerStates.OnGrounded)
            {
                Debug.Log("Character has grounded");

                character.UpdateState(Enums.PlayerStates.OnIdle);
            }
        }

        #endregion
    } 
}