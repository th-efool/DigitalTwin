
using UnityEngine;
using UnityEditor;

namespace realvirtual
{
    
    public class HelloWindow : EditorWindow
    {
        public string URLDoc = "https://doc.realvirtual.io";
        public string URLYoutube = "https://www.youtube.com/channel/UCiL22-4L3bkX-rz6bUN8ZYQ/videos";
        public string URLSupport = "https://forum.realvirtual.io";
        public string URLRate = "https://assetstore.unity.com/packages/slug/225899";
        public string URLUpgrade = "https://assetstore.unity.com/packages/slug/224043";

        public string URLReleaseNotes =
            "https://doc.realvirtual.io/advanced-topics/release-notes";
        
        public string URLLinkedIn =
            "https://www.linkedin.com/groups/9004088/";

        public string URLTraining = "https://realvirtual.io/en/training/";
     
        public string TextInto = "\n" +
                                 "realvirtual.io  is an open framework for developing industrial digital twins. realvirtual.io can be used for simulation, virtual commissioning, and 3D Human Machine Interfaces. Let's change the game for Digital Twins - affordable, shared source, extendable, and with gaming power based on Unity.";

        public string TextLinkedIn = "\nJoin our LinkedIn User Community to keep informed.";
        
        public string TextStarted = "\nPlease check our online documentation to get started.";

        public string TextYoutube = "\nOn our Youtube channel, you can find several tutorials.";

        public string TextTraining = "\nYou would like to participate in an online training?";

        public string TextSupport = "\nOn our Forum, you can ask questions and get support if needed.";

        public string TextUpgrade =
            "\nYou need more functions like more automation interfaces or the ability to work with large CAD assemblies, then you could upgrade to realvirtual Professional.";

        public string TextRate =
            "\nYou are happy with our solution? Please rate our solution on the Unity Asset store.";

        public string TextReleaseNotes =
            "\nIf you are upgrading from a previous version please first check our release notes.";



        public static Texture2D image = null;


        // Add menu named "My Window" to the Window menu
        [MenuItem("realvirtual/Info", false,999)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            HelloWindow window = ScriptableObject.CreateInstance<HelloWindow>();
            window.titleContent = new GUIContent("Welcome to realvirtual.io");
            window.Open();

        }

        public void Open()
        {
            var window = this;
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 810);
            image = (Texture2D) UnityEngine.Resources.Load("Icons/hellowindow", typeof(Texture2D));
           // Global.CenterOnMainWin(window);
            window.ShowModalUtility();
        }



        void OnGUI()
        {
            
            EditorGUILayout.LabelField("\n");
            
            EditorGUILayout.LabelField(Global.Version);

            GUILayout.Label(image);

            EditorGUILayout.LabelField(TextInto, EditorStyles.wordWrappedLabel);

            EditorGUILayout.LabelField(TextReleaseNotes, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Check the release notes"))
            {
                Application.OpenURL(URLReleaseNotes);
            }

            EditorGUILayout.LabelField(TextStarted, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Read documentation"))
            {
                Application.OpenURL(URLDoc);
            }
            
            EditorGUILayout.LabelField(TextLinkedIn, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Join User Community"))
            {
                Application.OpenURL(URLLinkedIn);
            }

            EditorGUILayout.LabelField(TextYoutube, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Watch Youtube"))
            {
                Application.OpenURL(URLYoutube);
            }

            EditorGUILayout.LabelField(TextTraining, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Register for a training"))
            {
                Application.OpenURL(URLTraining);
            }


            EditorGUILayout.LabelField(TextSupport, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Open the Forum"))
            {
                Application.OpenURL(URLSupport);
            }


            EditorGUILayout.LabelField(TextRate, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Give us a rating"))
            {
                Application.OpenURL(URLRate);
            }


            EditorGUILayout.LabelField(TextUpgrade, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Upgrade to Professional"))
            {
                Application.OpenURL(URLUpgrade);
            }

            EditorGUILayout.LabelField("\n\n");

            if (GUILayout.Button("Close"))
            {
                this.Close();
            }

        }
    }
}