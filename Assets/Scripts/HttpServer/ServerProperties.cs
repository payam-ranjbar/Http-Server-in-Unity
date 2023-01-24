using UnityEngine;

namespace HttpServer
{
    [CreateAssetMenu(fileName = "Server-Properties", menuName = "SO/Server Properteis", order = 0)]
    public class ServerProperties : ScriptableObject
    {
        public string IP;
        public string PORT;

        public TextAsset HtmlPage;
    }
}