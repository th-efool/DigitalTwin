using System;
using UnityEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace realvirtual
{
    public class rvUIPopupWindow : MonoBehaviour
    {
        public bool CancelButton;
        public Button CancelButtonObject;
        public string CancelButtonText;
        public int CloseWindowAutomaticallyAfterSeconds;
        public bool IsSizeable;
        public bool OkButton;
        public string OkButtonText;
        public UnityEvent OnClickCancelButton;
        public UnityEvent OnClickSubmitButton;
        public GameObject ResizeHeight;
        public GameObject ResizeWidth;
        public Button SubmitButtonObject;
        public string Title;
        public string WindowText;

        public static void Open(string msg, Action onsubmit, Action onCancel, string textokbutton, string textcancelbutton, string title = "", int closeafterseconds = 0, bool sizeable = false)
        {
            //var UIpopupPrefab = UnityEngine.Resources.Load<GameObject>("rvUIPopupWindow");
            rvUIPopupWindow UIpopupPrefab = FindAnyObjectByType<rvUIPopupWindow>(FindObjectsInactive.Include);

            GameObject window = Instantiate(UIpopupPrefab.gameObject, UIpopupPrefab.transform.parent);
            window.SetActive(true);
            window.transform.name = "UIPopupWindow";

            window.GetComponent<rvUIPopupWindow>().SetContent(msg, onsubmit, onCancel, textokbutton, textcancelbutton,
                title, closeafterseconds, sizeable);
        }
        public void Init()
        {
            gameObject.SetActive(true);

            HeadlineText.text = Title;
            MessageText.text = WindowText;

            if (OkButton)
            {
                SubmitButtonObject.gameObject.SetActive(true);
                SubmitButtonObject.onClick.RemoveAllListeners();
                SubmitButtonObject.onClick.AddListener(OnClickSubmitButton.Invoke);
                SubmitButtonObject.GetComponentInChildren<rvUIButton>().SetText(OkButtonText);
                SubmitButtonObject.onClick.AddListener(DestroyWindow);
            }
            else
            {
                SubmitButtonObject.gameObject.SetActive(false);
            }

            if (CancelButton)
            {
                CancelButtonObject.gameObject.SetActive(true);
                CancelButtonObject.onClick.RemoveAllListeners();
                CancelButtonObject.onClick.AddListener(OnClickCancelButton.Invoke);
                CancelButtonObject.GetComponentInChildren<rvUIButton>().SetText(CancelButtonText);
                CancelButtonObject.onClick.AddListener(DestroyWindow);
            }
            else
            {
                CancelButtonObject.gameObject.SetActive(false);
            }

            if (IsSizeable)
            {
                ResizeWidth.GetComponent<PanelResizer>().enabled = true;
                ResizeHeight.GetComponent<PanelResizer>().enabled = true;
            }

            else
            {
                ResizeWidth.GetComponent<PanelResizer>().enabled = false;
                ResizeHeight.GetComponent<PanelResizer>().enabled = false;
            }
        }

        private void HideWindow()
        {
            gameObject.SetActive(false);
        }

        private void DestroyWindow()
        {
            Destroy(gameObject);
        }

        public void SetContent(string msg, Action onsubmit, Action onCancel,
            string textokbutton, string textcancelbutton, string title = "", int closeafterseconds = 0,
            bool sizeable = false)
        {
            if (textokbutton == "")
            {
                OkButton = false;
            }
            else
            {
                OkButton = true;
                OkButtonText = textokbutton;
                if (onsubmit != null) OnClickSubmitButton.AddListener(onsubmit.Invoke);
            }

            if (textcancelbutton == "")
            {
                CancelButton = false;
            }
            else
            {
                CancelButton = true;
                CancelButtonText = textcancelbutton;
                if (onCancel != null) OnClickCancelButton.AddListener(onCancel.Invoke);
            }

            Title = title;
            WindowText = msg;
            CloseWindowAutomaticallyAfterSeconds = closeafterseconds;
            IsSizeable = sizeable;
            Init();


            if (CloseWindowAutomaticallyAfterSeconds > 0)
                Invoke("HideWindow", CloseWindowAutomaticallyAfterSeconds);
        }
        public TextMeshProUGUI HeadlineText;
        public TextMeshProUGUI MessageText;
    }
}