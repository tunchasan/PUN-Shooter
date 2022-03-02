using Photon.Pun;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerAnimationController : MonoBehaviourPun
    {
        #region Private Serializable Fields

        [SerializeField]
        private float directionDampTime = 0.25f;

        #endregion
        
        #region Private Fields

        private Animator _animator = null;
        
        private Vector2 _direction = Vector2.zero;

        #endregion
        
        #region MonoBehaviour Callbacks

        // Use this for initialization
        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            
            if (!_animator)
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        // Update is called once per frame
        private void Update()
        {
            // // deal with Jumping
            // var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            // // only allow jumping if we are running.
            // if (stateInfo.IsName("Base Layer.Run"))
            // {
            //     // When using trigger parameter
            //     if (Input.GetKey(KeyCode.Space))
            //     {
            //         
            //         
            //         _animator.SetTrigger("Jump");
            //     }
            // }
            //
            // var h = Input.GetAxis("Horizontal");
            // var v = Input.GetAxis("Vertical");
            //
            // v = v < 0 ? 0 : v;
            //
            // _animator.SetFloat("Speed", h * h + v * v);
            //
            // _animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);
        }

        #endregion

        #region Private Methods

        private bool CanProcessAnimation()
        {
            if (!_animator) return false;

            if (photonView.IsMine == false && PhotonNetwork.IsConnected) return false;

            return true;
        }

        #endregion
        
        #region Public Methods

        public void ProcessDirection(Vector2 direction)
        {
            if (CanProcessAnimation())
            {
                _animator.SetFloat("directionX", direction.x, .1F, Time.deltaTime);
                _animator.SetFloat("directionY", direction.y,.1F, Time.deltaTime);
            }
        }

        #endregion
    }
}