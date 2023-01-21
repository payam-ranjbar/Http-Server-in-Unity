using System;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

namespace HttpServer
{
    public class UnityHttp : MonoBehaviour
    {
        public static Action onChangeRequest;
        private static string HTML = "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><title>Dashboard</title></head><body><button id=\"btn\"><h1>Change Some Thing</h1></button></body><script>document.getElementById(\"btn\").onclick = e => {var http = new XMLHttpRequest();http.open(\"GET\",\"http://127.0.0.1:4444/Jesus\")http.send(null)}</script></html>";
        private HttpListener listener;
        private Thread listenerThread;
        private string _url;
        [SerializeField] private string file;

        public bool isRunning;
        public bool startAwake;

        private void OnValidate()
        {
            if(startAwake) StartHttpServer(IPUtils.GetBroadcastIP(), IPUtils.GetHttpPort());

        }

        void Start ()
        {

            if(startAwake && !isRunning) StartHttpServer(IPUtils.GetBroadcastIP(), IPUtils.GetHttpPort());
        }

        public void StartHttpServer(string ip, string port)
        {
            HTML = GetHtmlAddress();
            _url = GetSystemBroadcastIP(ip, port);
            // UpdateUrlFile(_url);
            InitListener(_url);
            CreateThread();
            isRunning = true;
            Debug.Log ($"Server Started on {_url}");
        }

        private string GetSystemBroadcastIP(string ip, string port)
        {
            var url = $"http://{ip}:{port}/";
            return url;
            
        }

        private static string GetSystemBroadcastIP()
        {
            var index = 3;
            var addresses = System.Net.Dns.GetHostAddresses("");
            var defaultIP = addresses[index].ToString();
            var url = "http://" + defaultIP + ":4444/";
            return url;
        }

        private void CreateThread()
        {
            listenerThread = new Thread(startListener);
            listenerThread.Start();
        }

        private void InitListener(string url)
        {
            listener = new HttpListener();
            // listener.Prefixes.Add ("http://localhost:4444/");
            listener.Prefixes.Add ("http://127.0.0.1:4444/url/");
            listener.Prefixes.Add(url);
            listener.Prefixes.Add(url + "changeImage/");
            listener.Prefixes.Add(url + "changeAudio/");
            listener.Prefixes.Add(url + "changeGyro/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();
        }

        private static void UpdateUrlFile(string url)
        {
        //     var urlAsset = Resources.Load<TextAsset>("url");
        //     var path = AssetDatabase.GetAssetPath(urlAsset);
        //     if (!File.Exists(path))
        //     {
        //         Debug.LogError($"{path} not exists");
        //         return;
        //     }
        //     using StreamWriter writer = new StreamWriter(path, false);
        //     writer.WriteLine(url);
            // writer.Close();
        }

        [ContextMenu("get adrs")]
        private string GetHtmlAddress()
        {
            var t = Resources.Load("Dashboard");
            file = ((TextAsset) t).text;
            // file = AssetDatabase.GetAssetPath(t);
            return file;
        }


        private void startListener ()
        {
            while (true) {               
                var result = listener.BeginGetContext (ListenerCallback, listener);
                result.AsyncWaitHandle.WaitOne ();
            }
        }

        private void ListenerCallback (IAsyncResult result)
        {				
            var context = listener.EndGetContext (result);		

            Debug.Log ("Method: " + context.Request.HttpMethod);
            Debug.Log ("LocalUrl: " + context.Request.Url.LocalPath);

            var api = context.Request.Url.LocalPath.ToLower();

            if (api == "/changeImage")
            {
                InovkeChangeImageAPI();
            }
            if (api == "/changeAudio")
            {
                InovkeChangeAudioAPI();
            }
            if (api == "/changeGyro")
            {
                InovkeChangeGryoAPI();
            }

            if (context.Request.QueryString.AllKeys.Length > 0)
            {
                foreach (var key in context.Request.QueryString.AllKeys)
                {
                    Debug.Log("Key: " + key + ", Value: " + context.Request.QueryString.GetValues(key)[0]);
                }
            }
            else
            {
                
                var response = context.Response;
                // using var reader = new StreamReader(file);
                var body = HTML;
                var buffer = System.Text.Encoding.UTF8.GetBytes(body);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer,0,buffer.Length);
                output.Close();
                // reader.Close();
            }
            
            if (context.Request.HttpMethod == "POST" && api == "/upload") 
            {	
                
                Thread.Sleep (1000);

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
                if (fileIsValid)
                {
                    var length = header["Content-Length"];

                    try
                    {
                        int.Parse(length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Content Length is invalid");
                    }
                    var fileAdrs = $"img_{"GUID.Generate()"}.{extension}";

                    var imageFile = new FileStream(fileAdrs, FileMode.OpenOrCreate);
                    using ( var data = new BinaryReader(context.Request.InputStream))
                    {
                        using (var writer = new BinaryWriter(imageFile))
                        {
                        
                            writer.Write(data.ReadBytes(int.Parse(length)));
                            writer.Flush();
                            writer.Close();
                        }
                        data.Close();
                    }
                    
                    InvokeImageUpload();
                }
                else
                {
                    Debug.LogError("file is not valid");
                }

               

            }
            

            context.Response.Close ();

        }

        private void InovkeChangeGryoAPI()
        {
            ServerEventListener.EnableGyroSignal = true;
        }

        private void InovkeChangeAudioAPI()
        {
            ServerEventListener.ToggleAudioSignal = true;
        }

        private void InovkeChangeImageAPI()
        {
            
            ServerEventListener.ChangeImageSignal = true;
            onChangeRequest?.Invoke();
            
        }

        private void InvokeImageUpload()
        {
            ServerEventListener.ImageSignal = true;
        }

    }
}