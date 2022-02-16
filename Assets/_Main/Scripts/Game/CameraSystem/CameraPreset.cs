using Com.MyCompany.MyGame;
using UnityEngine;

[System.Serializable]
public class CameraPreset
{
    public string title = "";
    public Enums.PlayerStates state = Enums.PlayerStates.None;
    public float fieldOfView = 60F;
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
}