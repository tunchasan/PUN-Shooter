using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

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
        
        // "The current Health of our player"
        private float _health = 100F;

        private CharacterController _characterController = null;

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

            InitializeMovementSystem();
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        private void Update()
        {
            if (photonView.IsMine)
            {
                ProcessInputs();
            }
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
                stream.SendNext(_health);
            }
            else
            {
                _isFiring = (bool) stream.ReceiveNext();
                _health = (float) stream.ReceiveNext();
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
            // OnPlayerFires
            if (Input.GetButtonDown("Fire1"))
            {
                OnFireAction(true);
            }
            
            // OnPlayerNotFires
            else if (Input.GetButtonUp("Fire1"))
            {
                OnFireAction(false);
            }

            // OnPlayerIdle
            if (!IsMoving())
            {
                _cameraController.ProcessState(Enums.PlayerStates.OnIdle);
            }
            
            // OnPlayerAims
            if (Input.GetButton("Fire2"))
            {
                OnAimAction(Input.mousePosition);
            }
            
            // OnPlayerNotAims
            else if (Input.GetButtonUp("Fire2"))
            {
                _cameraController.ProcessState(Enums.PlayerStates.OnIdle);
            }
            
            // OnPlayerJumps
            if(Input.GetKey(KeyCode.Space))
                _cameraController.ProcessState(Enums.PlayerStates.OnJump);
            
            ValidateMovement();
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
        
        private Vector2 _lastMousePosition = Vector2.zero;

        private void InitializeMovementSystem()
        {
            _lastMousePosition = new Vector2(Screen.width / 2F, Screen.height / 2F);
        }

        private void ValidateMovement()
        {
            var currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var delta = currentMousePosition - _lastMousePosition;
            var normalizedDelta = delta.normalized;
            var rotationDirection = new Vector3(normalizedDelta.x, 0F, 0F);
            _lastMousePosition = currentMousePosition;
            
            // Handle Camera Locomotion for "Run" state
            _cameraController.ProcessState(Enums.PlayerStates.OnRun);
            _cameraController.ValidateCameraRotation(normalizedDelta.y, verticalRotationSpeed);
            
            ProcessRotation(rotationDirection);

            ProcessMovement();
        }

        private void ProcessMovement()
        {
            // Handle Player Movement
            var inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var moveHorizontalAxis = inputDirection.x * transform.right;
            var moveVerticalAxis = inputDirection.y * transform.forward;
            var directionX = moveHorizontalAxis.x + moveVerticalAxis.x;
            var directionZ = moveHorizontalAxis.z + moveVerticalAxis.z;
            var direction = new Vector3(directionX, Physics.gravity.y, directionZ);
            
            _animationController.ProcessDirection(inputDirection);
            
            var moveVelocity = direction * speed;

            _characterController.Move(moveVelocity * Time.deltaTime);

            // TODO

            // Accelerated Motion
        }

        private void ProcessRotation(Vector3 rotationDirection)
        {
            if (rotationDirection.magnitude > float.Epsilon)
            {
                // Handle Player Rotation
                var currentRotation = transform.eulerAngles;
                var targetRotationAngle = Quaternion.LookRotation(rotationDirection, Vector3.up).eulerAngles.y;
                targetRotationAngle = targetRotationAngle < 180 ? targetRotationAngle : targetRotationAngle - 360;
                targetRotationAngle = Mathf.Clamp(targetRotationAngle, -45F, 45F);
                var calculatedTargetRotation = new Vector3(0F, currentRotation.y + targetRotationAngle, 0F);
                var calculatedAlpha = Time.deltaTime * horizontalRotationSpeed;
                transform.eulerAngles = Vector3.Lerp(currentRotation, calculatedTargetRotation, calculatedAlpha);
            }
        }

        #endregion

        #endregion
        
        #endregion
    }
}