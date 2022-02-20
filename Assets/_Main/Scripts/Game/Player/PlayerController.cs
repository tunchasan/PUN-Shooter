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
        [SerializeField] private float rotationSpeed = 10F;
        
        [SerializeField] private float speed = 10F;
        
        private Vector2 _lastMousePosition = Vector2.zero;

        private void InitializeMovementSystem()
        {
            _lastMousePosition = new Vector2(Screen.width / 2F, Screen.height / 2F);
        }

        private void ValidateMovement()
        {
            //_cameraController.ProcessState(Enums.PlayerStates.OnRun);
            
            var currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var delta = currentMousePosition - _lastMousePosition;
            _lastMousePosition = currentMousePosition;
            var rotationDirection = new Vector3(delta.x, 0F, delta.y).normalized;
            
            if (rotationDirection.magnitude > .1F)
            {
                var targetRotationAngle = Quaternion.LookRotation(rotationDirection, Vector3.up).eulerAngles.y;
                targetRotationAngle = targetRotationAngle < 180 ? targetRotationAngle : targetRotationAngle - 360;
                targetRotationAngle = Mathf.Clamp(targetRotationAngle, -45F, 45F);
                transform.RotateAround(transform.position, Vector3.up, targetRotationAngle * Time.deltaTime * rotationSpeed);
                
                // TODO
                
                // Rotate Camera Up / Down
                
                // Review Rotation Speed
            }
            
            var moveHorizontalAxis = Input.GetAxis("Horizontal") * transform.right;
            var moveVerticalAxis = Input.GetAxis("Vertical") * transform.forward;
            var direction = new Vector3(moveHorizontalAxis.x + moveVerticalAxis.x, Physics.gravity.y,
                moveHorizontalAxis.z + moveVerticalAxis.z);;
            if (direction.magnitude > .1F)
            {
                var moveVelocity = direction * speed;
                _characterController.Move(moveVelocity * Time.deltaTime);
                
                // TODO
                
                // Accelerated Motion
            }
        }

        #endregion

        #endregion
        
        #endregion
    }
}