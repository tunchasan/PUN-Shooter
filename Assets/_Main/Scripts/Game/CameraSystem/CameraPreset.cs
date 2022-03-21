using DG.Tweening;
using UnityEngine;

namespace Com.MyCompany.MyGame.Camera
{
    [System.Serializable]
    public class CameraPreset
    {
        [SerializeField] private string title = "";
        [SerializeField] private Enums.PlayerStates state = Enums.PlayerStates.None;
        [SerializeField] private float fieldOfView = 60F;
        [SerializeField] private float animDuration = .5F;
        [SerializeField] private Ease animType = Ease.Linear;
        [SerializeField] private Vector3 position = Vector3.zero;

        public string GetTitle => title;
        public Enums.PlayerStates GetState => state;
        public float GetFieldOfView => fieldOfView;
        public float GetAnimationDuration => animDuration;
        public Ease GetAnimationType => animType;
        public Vector3 GetCameraOffset => position;

        public void SetTitle(string value) => title = value;
        public void SetState(Enums.PlayerStates value) => state = value;
        public void SetFieldOfView(float value) => fieldOfView = value;
        public void SetAnimationDuration(float value) => animDuration = value;
        public void SetEase(Ease value) => animType = value;
        public void SetCameraOffset(Vector3 value) => position = value;
    }
}