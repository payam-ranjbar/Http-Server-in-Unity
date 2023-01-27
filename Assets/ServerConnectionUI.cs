using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ServerConnectionUI : MonoBehaviour
{
        [SerializeField] private InputField ipInputField;
        [SerializeField] private InputField portInputField;
        [SerializeField] private Dropdown ipListDropdown;

        [SerializeField] private string defaultPort = "8080";

        [SerializeField] private Button startServerButton;

        [SerializeField] private UnityTCPServer server;
        [SerializeField] private ServerProperties serverProperties;
        public string IP => ipInputField.text;
        public string Port => portInputField.text;
        public string URI => $"{IP}:{Port}";
        private void Start()
        {
                ipListDropdown.onValueChanged.AddListener(OnOptionChange);
                FillTheListOfIps();
                portInputField.text = defaultPort;
                startServerButton.onClick.AddListener(StartServer);
        }

        private void FillTheListOfIps()
        {
                var ips = IPUtils.GetIPList().Select(ip => new Dropdown.OptionData(ip)).ToList();
                ipListDropdown.options = ips;
                
        }

        private void OnOptionChange(int index)
        { 
                ipInputField.text = ipListDropdown.options[index].text;
        }

        private void StartServer()
        {
                serverProperties.IP = IP;
                serverProperties.PORT = Port;
                server.StartServer();
        }
        

}