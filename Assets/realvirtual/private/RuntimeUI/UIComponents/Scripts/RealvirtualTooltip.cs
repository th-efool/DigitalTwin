using UnityEngine;
using UnityEngine.UI;

namespace realvirtual
{
    public class RealvirtualTooltip : realvirtualBehavior,IUISkinEdit

    {
        private Text Text;
        private Image Background;
        private RectTransform rectTransform;
        private RectTransform bgRectTransform;

        protected new void Awake()
        {
            Text=GetComponentInChildren<Text>();
            Background = GetComponentInChildren<Image>();
            rectTransform = Global.GetComponentByName<RectTransform>(gameObject,"Base");
            bgRectTransform = Background.gameObject.GetComponent<RectTransform>();
            InitGame4Automation();
        }
        public void SetTooltip(string tip)
        {
            var size = Text.preferredWidth;
            bgRectTransform.sizeDelta = new Vector2(size+10,bgRectTransform.sizeDelta.y);
            Text.text = tip;
        }
        
        public void SetPosition(Vector3 pos)
        {
            rectTransform.anchoredPosition3D = pos;
        }

        public float getHeight()
        {
            return bgRectTransform.rect.height;
        }
        public float getWidth()
        {
            return bgRectTransform.rect.width;
        }
        public void UpdateUISkinParameter(RealvirtualUISkin skin)
        {
            var bgcolor = skin.TooltipBackgroundColor;
            var textcolor = skin.TooltipFontColor;
            Text.font = skin.TooltipFont;
            Text.color=new Color(textcolor.r,textcolor.g,textcolor.b,textcolor.a);
            
           
            Background.color=new Color(bgcolor.r,bgcolor.g,bgcolor.b,bgcolor.a);
        }

    }
}
