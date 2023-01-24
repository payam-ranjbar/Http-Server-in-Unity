using UnityEngine;

namespace HttpServer
{
    [CreateAssetMenu(fileName = "Media-Storage-Properties", menuName = "SO/Media Storage Properties", order = 0)]
    public class MediaStorageProperties : ScriptableObject
    {
        [SerializeField] private string imageRootPath;
        [SerializeField] private int imageIndex;

        [SerializeField] private string tempFilePath;
        [SerializeField] private string imageResourcesPath;

        public string ImageRootPath => $"Assets/Resources/{imageResourcesPath}";

        public int ImageIndex => imageIndex;

        public string TempFilePath => tempFilePath;

        public string ImageResourcesPath => imageResourcesPath;

        public string GetImagePath()
        {
            return $"{ImageRootPath}/image-{imageIndex}";
        }

        public void IncreaseIndex()
        {
            imageIndex++;
        }
    }
}