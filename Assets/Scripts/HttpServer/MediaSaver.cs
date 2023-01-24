using System.IO;
using UnityEngine;

namespace HttpServer
{
    public class MediaSaver
    {
        private readonly MediaStorageProperties _properties;


        public MediaSaver(MediaStorageProperties properties)
        {
            _properties = properties;
        }
        public void SaveImage(byte[] data, string extension)
        {
            
            var targetFilePath = $"{_properties.GetImagePath()}.{extension}";
            var targetFile = new FileStream(targetFilePath, FileMode.OpenOrCreate);
            using (var writer = new BinaryWriter(targetFile))
            {
                writer.Write(data);
                writer.Flush();
                writer.Close();
            }
            _properties.IncreaseIndex();
        }
        
    }
}