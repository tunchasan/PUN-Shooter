using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Com.MyCompany.MyGame
{
    public class PlayerCameraController : MonoBehaviour
    {
        #region Private Serialized Fields

        [Tooltip("Stores camera vertical rotation limits")] 
        [SerializeField] private float cameraRotationLimit = 25F;

        [Tooltip("The Player's TPS Camera")]
        [SerializeField] private CinemachineVirtualCamera playerCamera = null;
        
        #endregion

        #region Private Fields
        
        [Tooltip("Stores Player's Initial TPS Camera Settings")]
        private CameraPreset _initialPreset = new CameraPreset();

        [Tooltip("Stores playerCamera Animations as Tweens")]
        private Tween[] _cameraAnimations = new Tween[2];

        [Tooltip("Stores processed CameraPreset")]
        private CameraPreset _currentPreset = null;

        #endregion
        
        #region MonobehaviourCallbacks

        private void Awake()
        {
            var target = playerCamera.transform;
            _initialPreset.position = target.localPosition;
            _initialPreset.fieldOfView = playerCamera.m_Lens.FieldOfView;
            _currentPreset = _initialPreset;
        }

        #endregion

        #region Private Methods
        
        private void Animate(CameraPreset targetPreset)
        {
            StopAllAnimations();

            _currentPreset = targetPreset;
            
            _cameraAnimations[0] = playerCamera.transform.DOLocalMove(targetPreset.position, targetPreset.animDuration)
                .SetEase(targetPreset.animType);
            _cameraAnimations[1] = DOTween.To(() => playerCamera.m_Lens.FieldOfView,
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
            playerCamera.GetComponent<CinemachineImpulseSource>()
                .GenerateImpulse(Random.Range(.75F, 1.25F));
        }

        private void AddLookAtTarget(Transform target)
        {
            playerCamera.LookAt = target;
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

        public void ValidateCameraRotation(float upWeight, float rotationSpeed)
        {
            // Handle Camera Rotations
            var target = playerCamera.transform;
            var currentRotation = target.localEulerAngles;
            var additiveRotationX = -upWeight * rotationSpeed;
            var targetRotationAngle = currentRotation.x + additiveRotationX;
            targetRotationAngle = targetRotationAngle < 180 ? targetRotationAngle : targetRotationAngle - 360;
            targetRotationAngle = Mathf.Clamp(targetRotationAngle, 
                -cameraRotationLimit, cameraRotationLimit);
            var targetRotation = new Vector3(targetRotationAngle, currentRotation.y, currentRotation.z);
            target.localRotation = Quaternion.Lerp(Quaternion.Euler(currentRotation), Quaternion.Euler(targetRotation), 
                Time.deltaTime * rotationSpeed);
            
            // Handle Camera Positions
            var alpha = targetRotationAngle / cameraRotationLimit;
            var defaultPosition = DetermineCameraPreset(Enums.PlayerStates.OnIdle).position;
            var currentPosition = target.localPosition;
            var targetOffset = alpha < 0F
                ? new Vector3(currentPosition.x, 1F, -2.5F)
                : new Vector3(currentPosition.x, 3.5F, -2.5F);
            var targetPosition = 
                Vector3.Lerp(defaultPosition, targetOffset, Mathf.Abs(alpha));
            target.localPosition = Vector3.Lerp(currentPosition, targetPosition, 
                Time.deltaTime * rotationSpeed);
        }

        public void ProcessState(Enums.PlayerStates state, Transform target = null)
        {
            if(_currentPreset.state == state) return;

            switch (state)
            {
                case Enums.PlayerStates.OnShoot:
                    ShakeCamera();
                    break;
                case Enums.PlayerStates.OnDeath:
                    AddLookAtTarget(target);
                    break;
                case Enums.PlayerStates.None:
                case Enums.PlayerStates.OnIdle:
                case Enums.PlayerStates.OnJump:
                case Enums.PlayerStates.OnRun:
                case Enums.PlayerStates.OnAim:
                    Animate(DetermineCameraPreset(state));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #endregion
    }
}