using Com.MyCompany.MyGame.Camera;
using DG.Tweening;
using UnityEngine;
using Photon.Pun;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Character manager.
    /// Handles fire Input and Beams.
    /// </summary>
    
    public class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Serialized Fields
        
        [SerializeField]
        private PlayerCameraController cameraController = null;

        #endregion

        #region Private Fields

        //True, when the user is firing
        private bool _isFiring = false;
        
        private PlayerAnimationController _animationController = null;

        private UnityEngine.CharacterController _characterController = null;

        private Character _character = null;

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
            _characterController = GetComponent<UnityEngine.CharacterController>();

            _animationController = GetComponent<PlayerAnimationController>();

            _character = GetComponent<Character>();
            
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Validate Camera Visibility
            cameraController.ValidateStatus(photonView.IsMine);
            
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
            
            if(Input.GetButtonDown("Run"))
                UpdateSpeedMultiplier(true);
            if(Input.GetButtonUp("Run"))
                UpdateSpeedMultiplier(false);
            if (Input.GetButtonDown("Jump"))
                ProcessJump();
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
            
            cameraController.ProcessState(Enums.PlayerStates.OnShoot);
            
            Debug.Log(_isFiring ? "Fire" : "Not Fire");
        }

        private void OnAimAction(Vector2 aimPos)
        {
            cameraController.ProcessState(Enums.PlayerStates.OnAim);
            
            Debug.LogFormat("On Aim at {0}", aimPos);
        }

        #region Movement

        [Header("@MovementSystem")] 
        [SerializeField] private float horizontalRotationSpeed = 10F;
        [SerializeField] private float movementSpeed = 10F;
        [SerializeField] private float jumpSpeed = 5F;
        [SerializeField] private float jumpDuration = .25F;

        private float _currentSpeed = 0F;

        private bool _isRunning = false;
        private bool _isJumping = false;
        private float _jumpVelocity = 0F;
        
        private Vector2 _inputDirection = Vector2.zero;

        private void UpdateSpeedMultiplier(bool status)
        {
            _isRunning = status;
        }

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

        private bool CanJump()
        {
            return _characterController.isGrounded && !_isJumping;
        }
        
        private void ProcessJump()
        {
            if(CanJump())
            {
                _isJumping = true;

                DOTween.To(() => _jumpVelocity, x => _jumpVelocity = x, jumpSpeed, jumpDuration)
                    .SetEase(Ease.OutCirc).OnComplete(() =>
                    {
                        DOTween.To(() => _jumpVelocity, x => _jumpVelocity = x, 0F, jumpDuration)
                            .SetEase(Ease.InCirc).OnComplete(() =>
                            {
                                DOVirtual.DelayedCall(.25F, () => _isJumping = false);
                            });
                    });  
                
            }
        }

        private void ProcessMovement()
        {
            // Handle Character Movement
            _inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var moveHorizontalAxis = _inputDirection.x * transform.right;
            var moveVerticalAxis = _inputDirection.y * transform.forward;
            var directionX = moveHorizontalAxis.x + moveVerticalAxis.x;
            var directionZ = moveHorizontalAxis.z + moveVerticalAxis.z;
            var fallSpeed = Physics.gravity.y / (2.5F * movementSpeed);
            var direction = new Vector3(directionX, fallSpeed + _jumpVelocity, directionZ);
            var moveVelocity = direction * (DetermineMovementSpeed(_inputDirection) * Time.deltaTime);
            
            _characterController.Move(moveVelocity);

            DetermineLocomotionData();
        }

        private float DetermineMovementSpeed(Vector2 inputDirection)
        {
            var multiplyValue = _isRunning ? 1.5F : 1F;

            // Detect Strafe Movement || Detect Backward Movement
            if (Mathf.Abs(inputDirection.x) > 0F || inputDirection.y < 0F)
            {
                return AnimatedMovementSpeed(multiplyValue * .5F);
            }
            
            return AnimatedMovementSpeed(multiplyValue);
        }

        private float AnimatedMovementSpeed(float multiplyValue)
        {
            var targetSpeed = movementSpeed * multiplyValue;
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * 5F);
            return _currentSpeed;
        }
        
        private void DetermineLocomotionData()
        {
            var currentVelocity = _characterController.velocity.magnitude;

            var direction = currentVelocity > .15F ? _inputDirection : Vector2.zero;
            
            _animationController.ProcessLocomotion(direction, _isRunning);
        }
 
        private void ProcessRotation()
        {
            var horizontalInput = Input.GetAxis("Mouse X");
            var rotationSpeedMultiplier = Mathf.Abs(horizontalInput);
            var rotationDirection = new Vector3(horizontalInput, 0F, 0F);
            
            if (rotationDirection.magnitude > float.Epsilon)
            {
                // Handle Character Rotation
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
        [SerializeField] private GameObject aimPanel = null;
        [SerializeField] private Vector2 aimLimits = Vector2.zero;
        [SerializeField] private float aimSpeed = 5F;

        private float _aimAlpha = 0.5F;

        private void ProcessAim()
        {
            aimPanel.SetActive(photonView.IsMine);

            var verticalInput = Input.GetAxis("Mouse Y");
            _aimAlpha += verticalInput * Time.deltaTime * aimSpeed;
            _aimAlpha = Mathf.Clamp(_aimAlpha, 0F, 1F);

            aimTarget.localPosition = Vector3.Lerp(new Vector3(0F, 0F, aimLimits.x), 
                new Vector3(0F, 0F, aimLimits.y), _aimAlpha);

            cameraController.ValidateCameraRotation(_aimAlpha);
        }

        #endregion

        #endregion
        
        #endregion
    }
}