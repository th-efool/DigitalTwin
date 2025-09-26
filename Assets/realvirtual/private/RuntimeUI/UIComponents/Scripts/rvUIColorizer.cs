
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace realvirtual
{
    public class rvUIColorizer : MonoBehaviour, IUISkinEdit
    {
        private realvirtualController _controller;
        
        public enum SkinColor
        {
            Selected,
            Font,
            Window,
            Button
        }

        public SkinColor skinColor;


        void OnValidate()
        {
            
            Colorize(skinColor);
            
            
        }
        
        void Awake(){
            Colorize(skinColor);
        }
        
        public void Colorize(string colorname){
            SkinColor skinColor = SkinColor.Font;
            if(colorname == "selected"){
                skinColor = SkinColor.Selected;
            }
            if(colorname == "font"){
                skinColor = SkinColor.Font;
            }
            if(colorname == "window"){
                skinColor = SkinColor.Window;
            }
            if(colorname == "button"){
                skinColor = SkinColor.Button;
            }
            Colorize(skinColor);
        }

        public void Colorize(SkinColor skinColor){
            
            RealvirtualUISkin skin = SkinController.GetCurrentSkin();
            if(skin==null)
                return;
            this.skinColor = skinColor;
            

            Color color = GetColor();

            TMP_Text textMeshPro = GetComponent<TMP_Text>();
            if (textMeshPro != null)
            {
                textMeshPro.color = color;

                TextMeshProUGUI textMeshProUGUI = GetComponent<TextMeshProUGUI>();
                textMeshProUGUI.font = skin.BaseFontTMP;
            }

            Image uiImage = GetComponent<Image>();

            if (uiImage != null)
            {
                uiImage.color = color;

                
            }

            Text text = GetComponent<Text>();
            if (text != null)
            {
                text.color = color;
                text.font = skin.BaseFont;
            }

        }

        private Color GetColor()
        {
            RealvirtualUISkin skin = SkinController.GetCurrentSkin();            

            if(skinColor == SkinColor.Selected){
                return skin.SelectedColor;
            }

            if(skinColor == SkinColor.Font){
                return skin.WindowFontColor;
            }

            if(skinColor == SkinColor.Window){
                return skin.WindowContentBackgroundColor;
            }

            if(skinColor == SkinColor.Button){
                return skin.WindowButtonColor;
            }

        

            return Color.white;
        }

        public void UpdateUISkinParameter(RealvirtualUISkin skin)
        {

            Colorize(skinColor);

            
        }
    }
}