using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class CameraPresetContainer : Singleton<CameraPresetContainer>
    {
        #region Private Serialized Fields

        [Tooltip("Stores Character Cameras Presets")] 
        [SerializeField] private List<CameraPreset> presets = new List<CameraPreset>();

        #endregion

        #region Public Methods

        public CameraPreset Find(Enums.PlayerStates state)
        {
            var isExist = presets.Any(preset => preset.state == state);
            
            return isExist ? presets.
                FirstOrDefault((elem) => elem.state == state) : null;
        }

        #endregion
    }
}