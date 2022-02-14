using UnityEngine;
using Photon.Pun;

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

        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;

        #endregion

        #region Private Fields

        //True, when the user is firing
        private bool _isFiring = false;

        private PlayerCameraController _cameraController = null;

        #endregion

        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;
        
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;

        #endregion
        
        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        private void Awake()
        {
            _cameraController = GetComponent<PlayerCameraController>();
            
            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
        }

        private void Start()
        {
            // Validate Camera Visibility
            _cameraController.ValidateStatus(photonView.IsMine);
            
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
                LocalPlayerInstance = gameObject;
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
            
            //Debug.LogError(transform.parent.name);
            
            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
            
            if (PlayerUiPrefab != null)
            {
                var _uiGo =  Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage ("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        private void Update()
        {
            if (photonView.IsMine)
            {
                ProcessInputs();
                
                // Game Over State
                if (Health <= 0f)
                    GameManager.Instance.LeaveRoom();
                
                // trigger Beams active state
                if (beams != null && _isFiring != beams.activeInHierarchy)
                    beams.SetActive(_isFiring);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
                return;
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
                return;
            
            Health -= 0.1f;
        }

        private void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
                return;

            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
                return;
            
            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
            Health -= 0.1f * Time.deltaTime;
        }

        #if !UNITY_5_4_OR_NEWER
        
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        private void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        
        #endif

        #if UNITY_5_4_OR_NEWER
        
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable ();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        #endif

        private void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
                transform.position = new Vector3(0f, 5f, 0f);
            
            var _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        
        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        private void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!_isFiring)
                    _isFiring = true;
            }
            
            if (Input.GetButtonUp("Fire1"))
            {
                if (_isFiring)
                    _isFiring = false;
            }
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_isFiring);
                stream.SendNext(Health);
            }
            else
            {
                this._isFiring = (bool) stream.ReceiveNext();
                this.Health = (float) stream.ReceiveNext();
            }
        }

        #endregion

        #region Private Methods

        #if UNITY_5_4_OR_NEWER
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endif

        #endregion
    }
}