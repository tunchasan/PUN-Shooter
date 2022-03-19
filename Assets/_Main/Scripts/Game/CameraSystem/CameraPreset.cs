using Com.MyCompany.MyGame;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class CameraPreset
{
    public string title = "";
    public Enums.PlayerStates state = Enums.PlayerStates.None;
    public float fieldOfView = 60F;
    public float animDuration = .5F;
    public Ease animType = Ease.Linear;
    public Vector3 position = Vector3.zero;
}