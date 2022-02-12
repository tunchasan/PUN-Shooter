using Photon.Pun;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerAnimatorManager : MonoBehaviourPun
    {
        #region Private Serializable Fields

        [SerializeField]
        private float directionDampTime = 0.25f;

        #endregion
        
        #region Private Fields

        private Animator _animator = null;

        #endregion
        
        #region MonoBehaviour Callbacks

        // Use this for initialization
        private void Start()
        {
            _animator = GetComponent<Animator>();
            
            if (!_animator)
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        // Update is called once per frame
        private void Update()
        {
            if (photonView.IsMine == false && PhotonNetwork.IsConnected)
                return;
            
            if (!_animator)
                return;
            
            // deal with Jumping
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            // only allow jumping if we are running.
            if (stateInfo.IsName("Base Layer.Run"))
            {
                // When using trigger parameter
                if (Input.GetButtonDown("Fire2"))
                    _animator.SetTrigger("Jump");
            }

            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            
            v = v < 0 ? 0 : v;

            _animator.SetFloat("Speed", h * h + v * v);
            
            _animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);
        }

        #endregion
    }
}