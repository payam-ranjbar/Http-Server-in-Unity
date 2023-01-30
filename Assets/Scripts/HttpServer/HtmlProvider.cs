using System;
using UnityEngine;

namespace HttpServer
{
    public class HtmlProvider : MonoBehaviour
    {
        [SerializeField] private ServerProperties properties;
        
        public string HomePage => _homeHtml;

        private string _homeHtml;

        private bool ReadHomePageSignal { get; set; }

        private void Update()
        {
            if (ReadHomePageSignal)
            {
                GetHomePageHtml();
                ReadHomePageSignal = false;
            }
        }

        private void GetHomePageHtml()
        {
            _homeHtml = properties.HtmlPage.text;
        }

        public void ReadHomePage()
        {
            ReadHomePageSignal = true;
        }
        
    }
}