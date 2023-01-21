using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace HttpServer
{
    public class UnityTCPServer : MonoBehaviour
    {
        [SerializeField] private bool startOnAwake = true;
        private HttpListener _listener = null;
        private bool _isRunning = true;

        private Thread _listenerThread;
        private static string HTML = "";
        private static string URI = "127.0.0.1";
        private static string PORT = "8080";
        private string URL => $"http://{URI}:{PORT}";
        private string GetEndpoint(string api) => $"{URL}/{api}";

        void Start()
        {
            HTML = GetHtmlAddress();
            if(startOnAwake) StartServer();
        }

        public void StartServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(GetEndpoint(""));
            _listener.Start();
            
            Debug.Log($"Server has started on {URL}. Waiting for a connection...");

            _isRunning = true;
            _listenerThread = new Thread(RunServer);
            _listenerThread.Start();
        }

        private string GetHtmlAddress()
        {
            var t = Resources.Load("Dashboard");
            var file = ((TextAsset) t).text;
            return file;
        }

        private void RunServer()
        {
            while (_isRunning)
            {
                var context = _listener.GetContext();
                
                HandleHomepage(context);
                HandleImageUpload(context);
                HandleAudioUpload(context);
            }
        }

        private bool CheckRequestMethod(HttpListenerRequest request, string method) =>
            request.HttpMethod == method;

        private bool CheckRequestEndpoint(HttpListenerRequest request, string endpoint) =>
            request.Url.AbsolutePath == endpoint;

        private bool IsValidPostRequest(HttpListenerRequest request, string endpoint) =>
            CheckRequestMethod(request, "POST") && CheckRequestEndpoint(request, endpoint);
        
        private bool IsValidGetRequest(HttpListenerRequest request, string endpoint) =>
            CheckRequestMethod(request, "GET") && CheckRequestEndpoint(request, endpoint);

        private bool RequestHasQuery(HttpListenerRequest request) => request.QueryString.AllKeys.Length > 0;
        private void SendMessage(HttpListenerResponse response, string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg);

            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            output.Close();
        }

        private void SendCode(HttpListenerResponse response, HttpStatusCode code, string msg)
        {

            response.StatusCode = (int) code;
            SendMessage(response, msg);
            
            Debug.Log($"Sent Code {response.StatusCode}, msg: {msg}");
        }

        private void Send404Error(HttpListenerResponse response) =>
            SendCode(response, HttpStatusCode.NotFound, "404 Not found");

        private void SendOk(HttpListenerResponse response, string msg = "Success") =>
            SendCode(response, HttpStatusCode.OK, msg);
        private bool CheckLengthValid(string lengthString, out int length)
        {
            length = 0;

            try
            {
                length = int.Parse(lengthString);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Content Length is invalid");
            }

            return false;
        }

        private void SaveBinaryFile(HttpListenerContext context, string filePath, int fileLength)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);

            using (var data = new BinaryReader(context.Request.InputStream))
            {
                using (var writer = new BinaryWriter(fileStream))
                {

                    writer.Write(data.ReadBytes(fileLength));
                    writer.Flush();
                    writer.Close();
                }
                data.Close();
            }
            

        }

        private void HandleHomepage(HttpListenerContext context)
        {
            var request = context.Request;

            if (!IsValidGetRequest(request, "/") ) return;

            if (RequestHasQuery(request)) return;
            
            var response = context.Response;

            SendMessage(response, HTML);
        }

        private void HandleAudioToggle(HttpListenerContext context)
        {
            var request = context.Request;
            
            if(!IsValidGetRequest(request, "/audioToggle")) return;

            ServerEventListener.ToggleAudioSignal = true;
            
            SendOk(context.Response, "audio toggled");
        }
        void HandleAudioUpload(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (!IsValidPostRequest(request, "/audioUpload")) return;

            Thread.Sleep(1000);
            
            var header = context.Request.Headers;

            var contentType = header["Content-Type"];

            var extension = contentType switch
            {
                "audio/mpeg3" => "mp3",
                "audio/mpeg" => "mp3",
                "audio/x-mpeg-3" => "mp3",
                _ => ""
            };
            
            var fileIsValid = !string.IsNullOrEmpty(extension);

            if (!fileIsValid)
            {
                Send404Error(response);
                response.OutputStream.Close();
                return;
            }
            
            var lengthString = header["Content-Length"];

            
            if (!CheckLengthValid(lengthString, out var length))
            {
                SendCode(response, HttpStatusCode.BadRequest, "Error in Parsing Content Length");
                return;
            }

            // add music repository later
            var fileAdrs = $"Assets/Resources/audio.{extension}";
            
            SaveBinaryFile(context, fileAdrs, length);
            Thread.Sleep(1000);

            ServerEventListener.AudioPath = $"audio";
            ServerEventListener.AudioUploadSignal = true;
            SendOk(response);
            
            response.OutputStream.Close();

        }
        void HandleImageUpload(HttpListenerContext context)
        {

            var request = context.Request;
            var response = context.Response;

            if (!IsValidPostRequest(request, "/upload")) return;
            
            Thread.Sleep(1000);

            var header = context.Request.Headers;

            var contentType = header["Content-Type"];

            var extension = contentType switch
            {
                "image/jpeg" => "jpeg",
                "image/jpg" => "jpg",
                "image/png" => "png",
                _ => ""
            };
            
            var fileIsValid = !string.IsNullOrEmpty(extension);

            if (!fileIsValid)
            {
                Send404Error(response);
                response.OutputStream.Close();
                return;
            }
            
            var lengthString = header["Content-Length"];

            
            if (!CheckLengthValid(lengthString, out var length))
            {
                SendCode(response, HttpStatusCode.BadRequest, "Error in Parsing Content Length");
                return;
            }
            
            var fileAdrs = $"image.{extension}";

            SaveBinaryFile(context, fileAdrs, length);

            Thread.Sleep(1000);

            ServerEventListener.ImagePath = fileAdrs;
            ServerEventListener.ImageReceived = true;


            SendOk(response, "Image uploaded successfully!");
                
            
            response.OutputStream.Close();
        }

        void OnApplicationQuit()
            {
                _isRunning = false;
                _listener.Stop();
            }
        }
    
}