using System;
using System.IO;
using HttpServer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ServerEventListener : MonoBehaviour
{
    public static bool ChangeImageSignal;
    public static bool ToggleAudioSignal;
    public static bool EnableGyroSignal;
    public static bool ImageSignal;
    public static bool ImageReceived;
    public UnityEvent unityEvent;
    public UnityEvent gyroEvents;
    public UnityEvent audioEvents;
    public AudioSource sfx;
    public AudioSource musicSource;

    public static string ImagePath;
    public static string AudioPath;

    public static bool AudioUploadSignal;
        
        
   
    private void PlaySFX()
    {
        if(sfx != null)
            sfx.Play();
    }
    private void Update()
    {
        if (AudioUploadSignal)
        {
            AudioUploadSignal = false;
            SetAudio();
        }
        if (ImageReceived)
        {
            ImageReceived = false;
            SetTheTexture();
        }
        if (ChangeImageSignal)
        {

            PlaySFX();
            unityEvent?.Invoke();
            // network.ChangeGamePlayClientRpc();
            ChangeImageSignal = false;
                
        }

        if (ToggleAudioSignal)
        {
            PlaySFX();

            audioEvents?.Invoke();
            // network.ChangeMusicClientRpc();
            ToggleAudioSignal = false;
        }

        if (EnableGyroSignal)
        {
            PlaySFX();

            gyroEvents?.Invoke();
            // network.ChangeGyroClientRpc();
            EnableGyroSignal = false;
        }


        if (ImageSignal)
        {
            ImageSignal = false;
            // if(viewer != null)
            //     viewer.UpdateTextures();
        }
    }

    public RawImage rawImage;

    public int imageW = 1, imageH = 1;
    private static bool _audioUploadSignal;

    private void SetTheTexture()
    {
        var imageData =  File.ReadAllBytes(ImagePath);
        Texture2D tex = new Texture2D(imageW, imageH);
        tex.LoadImage(imageData);
        rawImage.GetComponent<RawImage>().texture = tex;
        rawImage.SetNativeSize();
    }

    private void SetAudio()
    {
        // var audiofile = File.ReadAllBytes(audioPath);
        var sourceClip = Resources.Load<AudioClip>(AudioPath);
        musicSource.clip = sourceClip;
            
        musicSource.Play();
    }
}