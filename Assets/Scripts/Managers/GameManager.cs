using System;
using CodeBlaze.Vloxy.Demo.Utils;
using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Managers {
    
    public class GameManager : SingletonBehaviour<GameManager> {

        public VloxyInput InputMaps { get; private set; }

        protected override void Initialize() {
            InitializeApplication();
            
            InputMaps = new VloxyInput();
        }

        private void InitializeApplication() {
            if (UnityEngine.Device.SystemInfo.deviceType == DeviceType.Handheld) {
                Application.targetFrameRate = Screen.currentResolution.refreshRate;

                GameLogger.Info<GameManager>("Device Type : Handheld");
            } else {
                GameLogger.Info<GameManager>("Device Type : Desktop");
            }
        }
        
    }
    
}