using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HttpServer
{
    public class HttpsServerStarter : MonoBehaviour
    {

        public TMP_InputField ipInput;
        public TMP_InputField portInput;
        public Button startServerBtn;
        public Button startClientBtn;
        
        public UnityEvent<string, string> onStartServer;
        public UnityEvent<string, string> onStartClient;
        
        public TMP_Text statusText;
        public TMP_Text ipHolder;
        public TMP_Text portHolder;
        public bool http;
        private string _ip;
        private string _port;
        public GameObject deactivateObjects;
        public void Start()
        {
            ipHolder.text = IPUtils.GetBroadcastIP();
            portHolder.text = http ? IPUtils.GetHttpPort() : IPUtils.GetSocketPort();

            _ip = ipHolder.text;
            _port = portHolder.text;
            ipInput.text = _ip;
            portInput.text = _port;
            ipInput.onValueChanged.AddListener(ReadIpInput);
            portInput.onValueChanged.AddListener(ReadPortInput);
            
            startServerBtn.onClick.AddListener((() =>
            {
                onStartServer?.Invoke(_ip, _port);
                if(http)
                 statusText.text = $"server is running on: http://{_ip}:{_port}/";

            }));
            if(startClientBtn == null) return;
            startClientBtn.onClick.AddListener((() =>
            {
                onStartClient?.Invoke(_ip, _port);

            }));


        }

        private void ReadIpInput(string ip)
        {
            _ip = ip;
        }
        private void ReadPortInput(string port)
        {
            _port = port;
        }
    }
}