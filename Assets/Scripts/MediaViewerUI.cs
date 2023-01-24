using System;
using System.Collections.Generic;
using DefaultNamespace;
using HttpServer;
using UnityEngine;
using UnityEngine.UI;

public class MediaViewerUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private MediaViewerButton buttonPrefab;

    [SerializeField] private RawImage viewer;

    [SerializeField] private MediaStorageProperties properties;
    private List<MediaViewerButton> _buttons;

    private void Awake()
    {
        _buttons = new List<MediaViewerButton>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        var textures = GetTextures();
        
        for (var i = 0; i < textures.Length; i++)
        {
            var texture = textures[i];
            if (i >= _buttons.Count)
            {
                CreateButton(texture);
            }
            else
            {
                var btn = _buttons[i];
                btn.image.texture = texture;
                btn.OnClick.RemoveAllListeners();
                btn.OnClick.AddListener((() => View(texture)));
                
            }
        }
    }

    private void CreateButton(Texture texture)
    {
        var btn = Instantiate(buttonPrefab, buttonContainer.transform);
        btn.image.texture = texture;
        btn.OnClick.AddListener((() => View(texture)));
        _buttons.Add(btn);
    }

    private Texture [] GetTextures()
    {
        return Resources.LoadAll<Texture>(properties.ImageResourcesPath);
    }

    private void View(Texture texture)
    {
        viewer.texture = texture;
        viewer.SetNativeSize();
    }
}