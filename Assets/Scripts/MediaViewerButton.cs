using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class MediaViewerButton : MonoBehaviour
    {

        public RawImage image;
        public UnityEvent OnClick => button.onClick;
        public Button button;

    }
}