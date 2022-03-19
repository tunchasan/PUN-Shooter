using Com.MyCompany.MyGame;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerBase : MonoBehaviour, IPunObservable
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

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext((byte)CurrentState);
        else
            CurrentState = (Enums.PlayerStates)((byte) stream.ReceiveNext());
    }

    #endregion
}