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
        private CameraPreset _initialPreset = new CameraPreset();

        [Tooltip("Stores playerCamera Animations as Tweens")]
        private Tween[] _cameraAnimations = new Tween[3];

        [Tooltip("Stores playerCamera ShakeAnimation as Tween")]
        private Tween _cameraShakeAnimation = null;

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
            
            _cameraAnimations[0] = playerCamera.transform.DOLocalMove(targetPreset.position, targetPreset.animDuration)
                .SetEase(targetPreset.animType);
            _cameraAnimations[1] = playerCamera.transform.DOLocalRotate(targetPreset.rotation, targetPreset.animDuration)
                .SetEase(targetPreset.animType);
            _cameraAnimations[2] = DOTween.To(() => playerCamera.m_Lens.FieldOfView,
                x => playerCamera.m_Lens.FieldOfView = x, targetPreset.fieldOfView, targetPreset.animDuration)
                .SetEase(targetPreset.animType);
        }
        
        private void StopAllAnimations()
        {
            foreach (var anim in _cameraAnimations)
                anim?.Kill();
        }

        private void ShakeCamera()
        {
            _cameraShakeAnimation?.Kill();
            
            _cameraShakeAnimation = 
                playerCamera.transform.DOShakeRotation(.5F, 
                    Random.Range(5F, 10F), 10, 15F);
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
            if(state == Enums.PlayerStates.OnShoot)
                ShakeCamera();
            
            Animate(DetermineCameraPreset(state));
        }

        #endregion
    }
}