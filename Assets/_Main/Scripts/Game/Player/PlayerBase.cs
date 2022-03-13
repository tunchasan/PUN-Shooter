using Com.MyCompany.MyGame;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerBase : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Cursor.visible = false;
    }

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