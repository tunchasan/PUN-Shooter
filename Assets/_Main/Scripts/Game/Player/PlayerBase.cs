using Com.MyCompany.MyGame;
using UnityEngine;

[RequireComponent(typeof(PlayerController), 
    typeof(PlayerAnimationController))]
public class PlayerBase : MonoBehaviour
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
    
    private void Start()
    {
        UpdateState(Enums.PlayerStates.OnIdle);
    }
}