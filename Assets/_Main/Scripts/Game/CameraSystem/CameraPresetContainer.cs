using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CameraPresetContainer : Singleton<CameraPresetContainer>
    {
        #region Private Serialized Fields

        [Tooltip("Stores Player Cameras Presets")] 
        [SerializeField] private List<CameraPreset> presets = new List<CameraPreset>();

        #endregion

        #region Public Methods

        public CameraPreset Find(Enums.PlayerStates state)
        {
            return presets.FirstOrDefault((elem) => elem.state == state);
        }

        #endregion
    }
}