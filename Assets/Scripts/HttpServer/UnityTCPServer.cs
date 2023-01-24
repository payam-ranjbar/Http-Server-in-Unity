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
        [SerializeField] private ServerProperties properties;
        [SerializeField] private MediaStorageProperties mediaStorageProperties;
        private HttpListener _listener;
        private bool _isRunning = true;
        private MediaSaver _mediaSaver;

        private Thread _listenerThread;

        private string URL => $"http://{properties.IP}:{properties.PORT}";
        private string GetEndpoint(string api) => $"{URL}/{api}";

        private void Awake()
        {
            _mediaSaver = new MediaSaver(mediaStorageProperties);
        }

        void Start()
        {
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

        private string GetHtml()
        {
            var htmlCode = properties.HtmlPage.text;
            return htmlCode;
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

        private void SaveImage(HttpListenerContext context,  string extension, int fileLength)
        {
            using var reader = new BinaryReader(context.Request.InputStream);
            _mediaSaver.SaveImage(reader.ReadBytes(fileLength), extension);
            reader.Close();
        }

        private void HandleHomepage(HttpListenerContext context)
        {
            var request = context.Request;

            if (!IsValidGetRequest(request, "/") ) return;

            if (RequestHasQuery(request)) return;
            
            var response = context.Response;

            SendMessage(response, GetHtml());
        }

        private void ListenGET(HttpListenerContext context, string endpoint, Action<HttpListenerResponse> callback)
        {
            if(!IsValidGetRequest(context.Request, endpoint)) return;

            callback?.Invoke(context.Response);
            
            SendOk(context.Response);
        }

        private void Listen(HttpListenerContext context, string method, string endpoint,
            Action<HttpListenerResponse> callback)
        {
            var request = context.Request;
            if (!CheckRequestMethod(request, method)) return;

            if (!CheckRequestEndpoint(request, endpoint)) return;
            
            callback?.Invoke(context.Response);
            
            SendOk(context.Response);
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
            

            SaveImage(context, extension, length);

            Thread.Sleep(1000);

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