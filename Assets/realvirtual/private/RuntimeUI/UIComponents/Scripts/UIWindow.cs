using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace realvirtual
{
    public class UIWindow : realvirtualBehavior, IUISkinEdit
    {
        public string Title;
        public string WindowText;
        public bool OkButton;
        public string OkButtonText;
        public UnityEvent OnClickSubmitButton;
        public bool CancelButton;
        public string CancelButtonText;
        public UnityEvent OnClickCancelButton;
        public GameObject Window;
        public GameObject WindowContent;
        public GameObject ButtonAreaBottom;
        public int CloseWindowAutomaticallyAfterSeconds;
        public bool IsSizeable;


        [Header("Skin Settings")] public Image BackgroundHeader;

        public Image BackgraoundContent;
        public Image BackgraoundFooter;
        private realvirtualController _controller;
        private Button[] _windowbuttons;
        private List<Text> _windowtexts;
        private GameObject CancelButtonObject;
        private Text HeadlineText;
        private Text MessageText;
        private GameObject ResizeButton;

        private GameObject SubmitButtonObject;


        // Start is called before the first frame update
        protected new void Awake()
        {
            Init();
        }

        public void UpdateUISkinParameter(RealvirtualUISkin skin)
        {
            BackgroundHeader.color = skin.WindowHeaderBackgroundColor;
            BackgraoundContent.color = skin.WindowContentBackgroundColor;
            BackgraoundFooter.color = skin.WindowContentBackgroundColor;
            foreach (var txt in _windowtexts)
            {
                if (txt.gameObject.name == "MessageText")
                    txt.fontSize = skin.WindowMessageFontSize;
                else
                    txt.fontSize = skin.WindowFontSize;
                txt.font = skin.WindowFont;
                txt.color = skin.WindowFontColor;
            }

            foreach (var button in _windowbuttons)
            {
                var colors = button.colors;
                colors.normalColor = new Color(skin.WindowButtonColor.r, skin.WindowButtonColor.g,
                    skin.WindowButtonColor.b, skin.WindowButtonColor.a);
                colors.pressedColor = new Color(skin.WindowHoverColor.r, skin.WindowHoverColor.g,
                    skin.WindowHoverColor.b, skin.WindowHoverColor.a);
                colors.highlightedColor = new Color(skin.WindowHoverColor.r, skin.WindowHoverColor.g,
                    skin.WindowHoverColor.b, skin.WindowHoverColor.a);
                button.colors = colors;
            }
        }

        public void Init()
        {
            Window.SetActive(true);
            Global.SetActiveSubObjects(Window, true);
            _controller = FindFirstObjectByType<realvirtualController>();
            SubmitButtonObject = Global.GetGameObjectByName("SubmitButton", ButtonAreaBottom);
            CancelButtonObject = Global.GetGameObjectByName("CancelButton", ButtonAreaBottom);
            ResizeButton = Global.GetGameObjectByName("WindowDragGizmo", ButtonAreaBottom);
            _windowtexts = Global.GetComponentsAlsoInactive<Text>(gameObject);
            foreach (var txt in _windowtexts)
                switch (txt.gameObject.name)
                {
                    case "HeadlineText":
                        HeadlineText = txt;
                        HeadlineText.text = Title;
                        break;
                    case "MessageText":
                        MessageText = txt;
                        MessageText.text = WindowText;
                        break;
                }

            if (OkButton)
            {
                SubmitButtonObject.SetActive(true);
                SubmitButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
                SubmitButtonObject.GetComponent<Button>().onClick.AddListener(OnClickSubmitButton.Invoke);
                SubmitButtonObject.GetComponentInChildren<Text>().text = OkButtonText;
            }
            else
            {
                SubmitButtonObject.SetActive(false);
            }

            if (CancelButton)
            {
                CancelButtonObject.SetActive(true);
                CancelButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
                CancelButtonObject.GetComponent<Button>().onClick.AddListener(OnClickCancelButton.Invoke);
                CancelButtonObject.GetComponentInChildren<Text>().text = CancelButtonText;
            }
            else
            {
                CancelButtonObject.SetActive(false);
            }

            if (IsSizeable)
            {
                ResizeButton.SetActive(true);
                var layout = ButtonAreaBottom.GetComponent<HorizontalLayoutGroup>();
                layout.padding.right = 0;
                if (GetComponent<UIElementResize>() == null) gameObject.AddComponent<UIElementResize>();
            }
            else
            {
                ResizeButton.SetActive(false);
                var layout = ButtonAreaBottom.GetComponent<HorizontalLayoutGroup>();
                layout.padding.right = 10;
            }

            _windowbuttons = gameObject.GetComponentsInChildren<Button>();
            Window.SetActive(false);
        }

        public void Show()
        {
            if (Window.activeSelf)
            {
                HideWindow();
            }
            else
            {
                Window.SetActive(true);
                if (CloseWindowAutomaticallyAfterSeconds > 0)
                    Invoke("HideWindow", CloseWindowAutomaticallyAfterSeconds);
            }
        }

        public void HideWindow()
        {
            Window.SetActive(false);
        }

        public void SetMessage(string message)
        {
            MessageText.text = message;
        }

        public void OnSubmitEvent(Action action)
        {
            // set action as unity event for submit button
            OnClickSubmitButton.RemoveAllListeners();
            OnClickSubmitButton.AddListener(action.Invoke);
        }

        public void OnCancelEvent(Action action)
        {
            OnClickCancelButton.RemoveAllListeners();
            OnClickCancelButton.AddListener(action.Invoke);
        }
    }
}