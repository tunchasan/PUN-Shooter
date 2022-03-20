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

        [Tooltip("The Character's TPS Camera")]
        [SerializeField] private CinemachineVirtualCamera playerCamera = null;
        
        #endregion

        #region Private Fields
        
        [Tooltip("Stores Character's Initial TPS Camera Settings")]
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
            var preset = CameraPresetContainer.Instance.Find(state);

            return preset ?? _initialPreset;
        }

        #endregion
        
        #region Public Methods

        public void ValidateStatus(bool status)
        {
            // Activates the camera if it's player itself
            playerCamera.gameObject.SetActive(status);
        }

        public void ValidateCameraRotation(float aimAlpha)
        {
            // Handle Camera Rotations
            var target = playerCamera.transform;
            var rotationA = new Vector3(55F, 0F, 0F);
            var rotationB = new Vector3(-70F, 0F, 0F);
            target.localRotation = Quaternion.Lerp(Quaternion.Euler(rotationA), Quaternion.Euler(rotationB), aimAlpha);
            
            // Handle Camera Positions
            var currentPosition = target.localPosition;
            var pointA = new Vector3(currentPosition.x, 3.5F, -.6F);
            var pointB = DetermineCameraPreset(Enums.PlayerStates.OnIdle).position;
            var pointC = new Vector3(currentPosition.x, .6F, -1.25F);
            var lerp1 = Vector3.Lerp(pointA, pointB, aimAlpha);
            target.localPosition = Vector3.Lerp(lerp1, pointC, aimAlpha);
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
                {
                    var preset = DetermineCameraPreset(state);
                    
                    if(preset != null)
                        Animate(preset);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #endregion
    }
}