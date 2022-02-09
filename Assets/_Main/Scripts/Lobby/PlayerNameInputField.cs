using UnityEngine;
using Photon.Pun;
using TMPro;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        #region Private Constants

        // Store the PlayerPref Key to avoid typos
        private const string PlayerNamePrefKey = "PlayerName";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        private void Start () 
        {
            var defaultName = string.Empty;
            var inputField = GetComponent<TMP_InputField>();
            if (inputField!=null)
            {
                if (PlayerPrefs.HasKey(PlayerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(PlayerNamePrefKey);
                    inputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName =  defaultName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetPlayerName(string value)
        {
            // #Important
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is null or empty");
                return;
            }
            
            PhotonNetwork.NickName = value;

            PlayerPrefs.SetString(PlayerNamePrefKey,value);
        }

        #endregion
    }
}