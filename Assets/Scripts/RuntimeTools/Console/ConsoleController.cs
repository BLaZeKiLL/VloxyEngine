using System;
using CodeBlaze.Vloxy.Demo.Managers;
using CodeBlaze.Vloxy.Demo.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CodeBlaze.Vloxy.Demo.RuntimeTools.Console {

    public class ConsoleController : MonoBehaviour {

        [SerializeField] private TMP_InputField CommandLine;
        [SerializeField] private ScrollRect Output;

        private void OnEnable() {
            GameManager.Current.InputMaps.Player.Disable();
            
            CommandLine.ActivateInputField();
            CommandLine.onSubmit.AddListener(OnCommand);
            CommandLine.Select();
        }

        private void OnDisable() {
            GameManager.Current.InputMaps.Player.Enable();
            
            CommandLine.ReleaseSelection();
            CommandLine.onSubmit.RemoveAllListeners();
            CommandLine.DeactivateInputField();
        }
        
        private void OnCommand(string command) {
            CommandLine.text = string.Empty;
            
            GameLogger.Info<ConsoleController>(command);
        }

    }

}