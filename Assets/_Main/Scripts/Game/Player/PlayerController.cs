using System;
using DG.Tweening;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    
    [RequireComponent(typeof(PlayerCameraController))]
    public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Serialized Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField] private GameObject playerUiPrefab;

        #endregion

        #region Private Fields

        //True, when the user is firing
        private bool _isFiring = false;

        private PlayerCameraController _cameraController = null;
        
        private PlayerAnimationController _animationController = null;

        private CharacterController _characterController = null;

        private PlayerBase _player = null;

        #endregion

        #region Static Fields
        
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion
        
        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        private void Awake()
        {
            _cameraController = GetComponent<PlayerCameraController>();

            _characterController = GetComponent<CharacterController>();

            _animationController = GetComponent<PlayerAnimationController>();

            _player = GetComponent<PlayerBase>();
            
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Validate Camera Visibility
            _cameraController.ValidateStatus(photonView.IsMine);
            
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            LocalPlayerInstance = photonView.IsMine ? gameObject : LocalPlayerInstance;

            // Instantiate PlayerUI and Assign
            // InitializePlayerUI();
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        private void Update()
        {
            if (photonView.IsMine)
                ProcessInputs();
        }

        public override void OnEnable()
        {
            // Always call the base to remove callbacks
            base.OnEnable();
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable ();
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_isFiring);
                stream.SendNext(_inputDirection);
            }
            else
            {
                _isFiring = (bool) stream.ReceiveNext();
                _inputDirection = (Vector2) stream.ReceiveNext();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Invokes when level is loaded.
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, 
            UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
                transform.position = new Vector3(0f, 5f, 0f);
        }
        
        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        private void ProcessInputs()
        {
            ValidateLocomotion();
        }

        /// <summary>
        /// Instantiates PlayerUI prefab and assign the object's target as this
        /// </summary>
        private void InitializePlayerUI()
        {
            if (playerUiPrefab != null)
                Instantiate(playerUiPrefab).SendMessage ("SetTarget", this, 
                    SendMessageOptions.RequireReceiver);
            else
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> " +
                                 "PlayerUiPrefab reference on player Prefab.", this);
        }

        private bool IsMoving()
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            return (h * h + v * v) > .1F;
        }
        
        #region Actions

        private void OnFireAction(bool status)
        {
            _isFiring = status;
            
            _cameraController.ProcessState(Enums.PlayerStates.OnShoot);
            
            Debug.Log(_isFiring ? "Fire" : "Not Fire");
        }

        private void OnAimAction(Vector2 aimPos)
        {
            _cameraController.ProcessState(Enums.PlayerStates.OnAim);
            
            Debug.LogFormat("On Aim at {0}", aimPos);
        }

        #region Movement

        [Header("@MovementSystem")] 
        [SerializeField] private float horizontalRotationSpeed = 10F;
        [SerializeField] private float verticalRotationSpeed = 10F;
        
        [SerializeField] private float speed = 10F;
        
        private Vector2 _inputDirection = Vector2.zero;

        private bool CanProcessLocomotion()
        {
            return true;
        }
        
        private void ValidateLocomotion()
        {
            if (CanProcessLocomotion())
            {
                ProcessMovement();
            
                ProcessRotation();

                ProcessAim();
            }
        }

        private void ProcessMovement()
        {
            // Handle Player Movement
            _inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var moveHorizontalAxis = _inputDirection.x * transform.right;
            var moveVerticalAxis = _inputDirection.y * transform.forward;
            var directionX = moveHorizontalAxis.x + moveVerticalAxis.x;
            var directionZ = moveHorizontalAxis.z + moveVerticalAxis.z;
            var fallSpeed = Physics.gravity.y / (2.5F * speed);
            var direction = new Vector3(directionX, fallSpeed, directionZ);
            var moveVelocity = direction * (DetermineMovementSpeed(_inputDirection) * Time.deltaTime);
            
            _characterController.Move(moveVelocity);

            var currentVelocity = _characterController.velocity.magnitude;
            _animationController.ProcessDirection(currentVelocity > .15F ? _inputDirection : Vector2.zero);
        }

        private float DetermineMovementSpeed(Vector2 inputDirection)
        {
            // Detect Strafe Movement
            if (Mathf.Abs(inputDirection.x) > 0F)
                return speed * .6F;
            
            // Detect Backward Movement
            if (inputDirection.y < 0F)
                return speed * .75F;
            
            return speed;
        }
 
        private void ProcessRotation()
        {
            var horizontalInput = Input.GetAxis("Mouse X");
            var rotationSpeedMultiplier = Mathf.Abs(horizontalInput);
            var rotationDirection = new Vector3(horizontalInput, 0F, 0F);
            
            if (rotationDirection.magnitude > float.Epsilon)
            {
                // Handle Player Rotation
                var currentRotation = transform.eulerAngles;
                var targetRotationAngle = Quaternion.LookRotation(rotationDirection, Vector3.up).eulerAngles.y;
                targetRotationAngle = targetRotationAngle < 180 ? targetRotationAngle : targetRotationAngle - 360;
                var calculatedTargetRotation = new Vector3(0F, currentRotation.y + targetRotationAngle, 0F);
                transform.eulerAngles =
                    Vector3.Lerp(currentRotation, calculatedTargetRotation, 
                        Time.deltaTime * rotationSpeedMultiplier * horizontalRotationSpeed);
            }
        }
        
        #endregion

        #region AimSystem

        [Header("@AimSystem")] 
        [SerializeField] private Transform aimTarget = null;
        [SerializeField] private Image aimSprite = null;
        [SerializeField] private Vector2 aimLimits = Vector2.zero;
        [SerializeField] private float aimSpeed = 5F;

        private float _aimAlpha = 0.5F;

        private void ProcessAim()
        {
            var verticalInput = Input.GetAxis("Mouse Y");
            _aimAlpha += verticalInput * Time.deltaTime * aimSpeed;
            _aimAlpha = Mathf.Clamp(_aimAlpha, 0F, 1F);

            aimTarget.localPosition = Vector3.Lerp(new Vector3(0F, 0F, aimLimits.x), 
                new Vector3(0F, 0F, aimLimits.y), _aimAlpha);

            _cameraController.ValidateCameraRotation(_aimAlpha);

            var aimTargetColor = Color.white;
            aimTargetColor.a = .25F;
            aimSprite.color = Color.Lerp(Color.white, aimTargetColor, _aimAlpha);

            var aimLerp1 = Vector2.Lerp(new Vector2(0, -35), new Vector2(0, -200), _aimAlpha);
            var aimLerp2 = Vector2.Lerp(aimLerp1, new Vector2(0, 35), _aimAlpha);
            aimSprite.rectTransform.anchoredPosition = aimLerp2;
            aimSprite.transform.localScale = Vector3.Lerp(Vector3.one * .4F, Vector3.one, 
                _aimAlpha);
        }

        #endregion

        #endregion
        
        #endregion
    }
}