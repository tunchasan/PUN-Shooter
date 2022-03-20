using UnityEngine;

namespace Com.MyCompany.MyGame
{
    [RequireComponent(typeof(CharacterController))]
    public class Character : MonoBehaviour
    {
        public Enums.PlayerStates CurrentState { get; private set; } = Enums.PlayerStates.None;

        public bool UpdateState(Enums.PlayerStates targetState)
        {
            if (targetState != CurrentState)
            {
                CurrentState = targetState;

                return true;
            }

            return false;
        }
    }
}