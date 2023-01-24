using UnityEngine;

namespace HttpServer
{
    [CreateAssetMenu(fileName = "Media-Storage-Properties", menuName = "SO/Media Storage Properties", order = 0)]
    public class MediaStorageProperties : ScriptableObject
    {
        [SerializeField] private string imageRootPath;
        [SerializeField] private int imageIndex;

        [SerializeField] private string tempFilePath;

        public string ImageRootPath => imageRootPath;

        public int ImageIndex => imageIndex;

        public string TempFilePath => tempFilePath;

        public string GetImagePath()
        {
            return $"{imageRootPath}/image-{imageIndex}";
        }

        public void IncreaseIndex()
        {
            imageIndex++;
        }
    }
}