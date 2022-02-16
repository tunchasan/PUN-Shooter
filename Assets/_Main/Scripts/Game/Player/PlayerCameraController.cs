using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerCameraController : MonoBehaviour
    {
        #region Private Serialized Fields

        [Tooltip("The Player's TPS Camera")]
        [SerializeField] private CinemachineVirtualCamera playerCamera = null;

        #endregion

        #region Private Fields
        
        [Tooltip("Stores Player's Initial TPS Camera Settings")]
        private CameraPreset _initialPreset = null;

        [Tooltip("Stores playerCamera Animations Tween")]
        private Tween[] _cameraAnimations = new Tween[3];

        #endregion
        
        #region MonobehaviourCallbacks

        private void Awake()
        {
            var target = playerCamera.transform;
            _initialPreset.position = target.localPosition;
            _initialPreset.rotation = target.localEulerAngles;
            _initialPreset.fieldOfView = playerCamera.m_Lens.FieldOfView;
        }

        #endregion

        #region Private Methods
        
        private void Animate(CameraPreset targetPreset)
        {
            StopAllAnimations();
            _cameraAnimations[0] = playerCamera.transform.DOLocalMove(targetPreset.position, .5F);
            _cameraAnimations[1] = playerCamera.transform.DOLocalRotate(targetPreset.rotation, .5F);
            _cameraAnimations[2] = DOTween.To(() => playerCamera.m_Lens.FieldOfView,
                x => playerCamera.m_Lens.FieldOfView = x, targetPreset.fieldOfView, .5F);
        }
        
        private void StopAllAnimations()
        {
            foreach (var anim in _cameraAnimations)
                anim?.Kill();
        }

        private void ShakeCamera()
        {
            // TODO
        }

        private CameraPreset DetermineCameraPreset(Enums.PlayerStates state)
        {
            return state == Enums.PlayerStates.None ? 
                _initialPreset : 
                CameraPresetContainer.Instance.Find(state);
        }

        #endregion
        
        #region Public Methods

        public void ValidateStatus(bool status)
        {
            // Activates the camera if it's player itself
            playerCamera.gameObject.SetActive(status);
        }

        public void ProcessState(Enums.PlayerStates state)
        {
            Animate(DetermineCameraPreset(state));
        }

        #endregion
    }
}