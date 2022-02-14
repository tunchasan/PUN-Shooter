using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MyCompany.MyGame
{
    public class PlayerUIController : MonoBehaviour
    {
        #region Private Serializable Fields

        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private TextMeshProUGUI playerNameText;

        [Tooltip("UI Slider to display Player's Health")]
        [SerializeField]
        private Slider playerHealthSlider;
        
        #endregion

        #region Private Fields

        private PlayerController target = null;
        
        private float characterControllerHeight = 0f;
        private Transform targetTransform;
        private Renderer targetRenderer;
        private CanvasGroup _canvasGroup;
        private Vector3 targetPosition;

        #endregion

        #region Public Fields

        [Tooltip("Pixel offset from the player target")]
        [SerializeField]
        private Vector3 screenOffset = new Vector3(0f,30f,0f);

        #endregion
        
        #region MonoBehaviour Callbacks

        private void Awake()
        {
            transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
            
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            // Reflect the Player Health
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = target.Health;
            }
            
            // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void LateUpdate()
        {
            // Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
            if (targetRenderer != null)
            {
                _canvasGroup.alpha = targetRenderer.isVisible ? 1f : 0f;
            }

            // #Critical
            // Follow the Target GameObject on screen.
            if (targetTransform != null)
            {
                targetPosition = targetTransform.position;
                targetPosition.y += characterControllerHeight;
                transform.position = Camera.main.WorldToScreenPoint (targetPosition) + screenOffset * 1.5F;
            }
        }
        
        #endregion

        #region Public Methods

        public void SetTarget(PlayerController _target)
        {
            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            
            // Cache references for efficiency
            target = _target;
            
            targetTransform = this.target.GetComponent<Transform>();
            targetRenderer = this.target.GetComponent<Renderer>();
            var characterController = _target.GetComponent<CharacterController> ();
            // Get data from the Player that won't change during the lifetime of this Component
            if (characterController != null)
                characterControllerHeight = characterController.height;
            
            if (playerNameText != null)
                playerNameText.text = target.photonView.Owner.NickName;
        }

        #endregion
    }
}