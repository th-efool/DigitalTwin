// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2025 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace realvirtual
{
    public class RuntimeNews : MonoBehaviour
    {
        [Serializable]
        public class UserResponse
        {
            public bool login;
            public UserFileInfo[] fileIds;
            public UserNewsEntry[] news;
        }

        [Serializable]
        public class UserFileInfo
        {
            public string File;
            public string Size;
            public string sort;
            public string Date;
            public string id;
        }

        [Serializable]
        public class UserNewsEntry
        {
            public string category;
            public string title;
            public string text;
            public string linkTitle;
            public string link;
        }


        public string category;
        public bool showOnStart = false;
        public float delay = 0;
        public GameObject newsElementPrefab;
        public GameObject newsCanvas;
        public RectTransform contentParent;

        private UserResponse userResponse;

        //private string baseURL = "http://localhost:3000";
        private static string baseURL = "https://download.realvirtual.io";

        private void Start()
        {
            if (showOnStart)
            {
                ShowNews();
            }
        }

        public void ShowNews()
        {
            Post();
        }

        IEnumerator ShowNewsRoutine()
        {
            yield return new WaitForSeconds(delay);
            if (userResponse.news != null && userResponse.news.Length > 0)
            {
                foreach (var news in userResponse.news)
                {
                    var newsElement = Instantiate(newsElementPrefab, contentParent);
                    newsElement.GetComponent<RuntimeNewsElement>().SetNews(news.title, news.text);

                    if (news.linkTitle != "")
                    {
                        newsElement.GetComponent<RuntimeNewsElement>().SetButton(news.linkTitle,
                            () => { Application.OpenURL(news.link); });
                    }
                }

                newsCanvas.SetActive(true);
            }

        }


        private void Post()
        {
            var url = baseURL + "/news";
            var BundleWebRequest = UnityWebRequest.Get(url);
            BundleWebRequest.SetRequestHeader("Content-Type", "application/json");
            BundleWebRequest.SetRequestHeader("news_category", category);
            UnityWebRequestAsyncOperation operation = BundleWebRequest.SendWebRequest();
            operation.completed += PostCompleted;
        }

        private void PostCompleted(AsyncOperation obj)
        {
            var request = (UnityWebRequestAsyncOperation)obj;
            var response = request.webRequest.downloadHandler.text;

            try
            {
                userResponse = JsonUtility.FromJson<UserResponse>(response);

                StartCoroutine(ShowNewsRoutine());

            }
            catch 
            {
                Debug.Log("Error parsing response");
                Debug.LogError(response);
                return;
            }


        }


    }
}
