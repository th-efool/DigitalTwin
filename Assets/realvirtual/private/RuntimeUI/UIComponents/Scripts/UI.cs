using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace realvirtual
{

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif


    public static class UI
    {
        public enum Tooltipposition
        {
            Above,
            Under,
            Left,
            Right
        }

        public static bool RuntimeInspectorEnabled = true;

        // public  static event FileBrowser.OnSuccess onSuccessOpen;

        private static GameObject AnnotationPrefab;
        private static GameObject UItooltip;
        private static GameObject UISignalInfo;
        private static GameObject UIpopupPrefab;
        private static GameObject UIpopupObj;
        private static GameObject UIWindowPrefab;
        private static GameObject UIWindowContentPrefab;
        private static GameObject UIWindow;
        private static UIWindow windowcomp;

        private static GameObject UIInspectorButton;

        private static string WindowContent;

        // methods to check tooltip interface of the gameobject
        public static bool HasTooltipInterface(GameObject go)
        {
            return go.GetComponent<IUItooltip>() != null;
        }

        public static GameObject CreateAnnotation(Vector3 position, Quaternion rotation, GameObject obj)
        {
            if (AnnotationPrefab == null)
                AnnotationPrefab = UnityEngine.Resources.Load<GameObject>("Annotation");
            // create instance of an prefab
            var annotation = Object.Instantiate(AnnotationPrefab, position, rotation);
            annotation.transform.SetParent(obj.transform);
            return annotation;
        }

        public static GameObject CreateTooltip(Vector3 position, Quaternion rotation, GameObject obj)
        {
            if (UItooltip == null)
                UItooltip = UnityEngine.Resources.Load<GameObject>("Tooltip");
            // create instance of an prefab
            var tooltip = Object.Instantiate(UItooltip, position, rotation);
            if (obj != null)
                tooltip.transform.SetParent(obj.transform);
            return tooltip;
        }

        public static GameObject CreateSignalInfo(Vector3 position, Quaternion rotation, GameObject obj)
        {
            if (UISignalInfo == null)
                UISignalInfo = UnityEngine.Resources.Load<GameObject>("UISignalPrefab");
            var SignalInfo = Object.Instantiate(UISignalInfo, position, rotation);
            if (obj != null)
                SignalInfo.transform.SetParent(obj.transform);
            return SignalInfo;
        }

        public static void UIOpenPopUpWindow(GameObject obj, string msg, Action onsubmit, Action onCancel,
            string textokbutton, string textcancelbutton, string title = "", int closeafterseconds = 0,
            bool sizeable = false)
        {
            var window = CreateUIwindow(null, "PopUpWindow");
            UIpopupObj = window.gameObject;
            SetUIwindowContent(window, msg, onsubmit, onCancel, textokbutton, textcancelbutton, title,
                closeafterseconds, sizeable);
            window.Show();
        }

        public static void ClosePopUpWindow()
        {
            if (UIpopupObj != null)
                Object.Destroy(UIpopupObj);
        }
        /* public static void ShowSaveDialog(Action<string[]> onsuccessSave, Action<string[]> onopen)
         {
             if (onsuccessSave != null) onSuccess += onsuccessSave.Invoke;
             //if (onopen != null) onCancel += onopen.Invoke;
             FileBrowser.ShowSaveDialog(onSuccess,onCancel,_pickMode,false,null,"Save As","Save");
         }

         public static void ShowLoadDialog(Action<string[]> onsuccess, Action oncancel, string path)
         {
             if (onsuccess != null) onSuccess += onsuccess.Invoke;
             if(oncancel !=null) onCancel += oncancel.Invoke;
             IEnumerable <string> extensions = new string[] { };
             FileBrowser.SetFilters(false,extensions);
             FileBrowser.ShowLoadDialog(onSuccess, onCancel, _pickMode, false, path, "Load Scene",
                 "Open");
         }

         public static void ShowLoadLibraryDialog(Action<string[]> onsuccessSave, Action oncancel, string path)
         {
             if (onsuccessSave != null) onSuccess += onsuccessSave.Invoke;
             if(oncancel !=null) onCancel += oncancel.Invoke;
             FileBrowser.ShowLoadDialog(onSuccess, onCancel, _pickMode, false, path, "json","Load Library",
                 "Load");
         }*/

        public static void UIOpenwindow(GameObject obj, string WindowCont, string msg, Action onsubmit, Action onCancel,
            string textokbutton, string textcancelbutton, string title = "", int closeafterseconds = 0,
            bool sizeable = false)
        {
            if (UIWindowPrefab == null)
                UIWindowPrefab = UnityEngine.Resources.Load<GameObject>("UIWindow");

            if (UIWindow == null)
            {
                windowcomp = CreateUIwindow(obj, "PopUpWindow");
                UIWindow = windowcomp.gameObject;
                UIWindow.transform.name = "UIWindowPrefab";

                if (WindowCont != "")
                {
                    WindowContent = WindowCont;
                    if (UIWindowContentPrefab == null)
                        UIWindowContentPrefab = UnityEngine.Resources.Load<GameObject>(WindowContent);
                    var content =
                        Object.Instantiate(UIWindowContentPrefab, Vector3.zero, Quaternion.identity);

                    content.transform.SetParent(windowcomp.WindowContent.transform);
                }
            }

            SetUIwindowContent(windowcomp, msg, onsubmit, onCancel, textokbutton, textcancelbutton, title,
                closeafterseconds, sizeable);
            windowcomp.Show();
        }

        public static string GetInputFieldText()
        {
            var inputpath = UIWindow.GetComponentInChildren<InputField>();
            return inputpath.text;
        }

        public static void CloseInputWindow()
        {
            windowcomp.HideWindow();
            if (UIWindow != null)
                Object.Destroy(UIWindow);
            UIWindow = null;
        }

        public static Button CreateInspectorButton()
        {
            if (UIInspectorButton == null)
                UIInspectorButton = UnityEngine.Resources.Load<GameObject>("InspectorButton");

            var button = Object.Instantiate(UIInspectorButton, Vector3.zero, Quaternion.identity);
            var buttoncomp = button.GetComponent<Button>();
            return buttoncomp;
        }

        private static UIWindow CreateUIwindow(GameObject WindowFolder, string prefab)
        {
            if (UIpopupPrefab == null)
                UIpopupPrefab = UnityEngine.Resources.Load<GameObject>(prefab);

            var window = Object.Instantiate(UIpopupPrefab, Vector3.zero, Quaternion.identity);
            window.transform.name = "PopUpWindow";
            if (WindowFolder != null)
                window.transform.SetParent(WindowFolder.transform);

            var win = window.GetComponent<UIWindow>();

            return win;
        }

        private static void SetUIwindowContent(UIWindow windowcomp, string msg, Action onsubmit, Action onCancel,
            string textokbutton, string textcancelbutton, string title = "", int closeafterseconds = 0,
            bool sizeable = false)
        {
            if (textokbutton == "")
            {
                windowcomp.OkButton = false;
            }
            else
            {
                windowcomp.OkButton = true;
                windowcomp.OkButtonText = textokbutton;
                windowcomp.OnSubmitEvent(onsubmit);
            }

            if (textcancelbutton == "")
            {
                windowcomp.CancelButton = false;
            }
            else
            {
                windowcomp.CancelButton = true;
                windowcomp.CancelButtonText = textcancelbutton;
                windowcomp.OnCancelEvent(onCancel);
            }

            windowcomp.Title = title;
            windowcomp.WindowText = msg;
            windowcomp.CloseWindowAutomaticallyAfterSeconds = closeafterseconds;
            windowcomp.IsSizeable = sizeable;
            windowcomp.Init();
        }
    }

}