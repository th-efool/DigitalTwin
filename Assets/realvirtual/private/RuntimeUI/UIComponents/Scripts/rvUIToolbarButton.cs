// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace realvirtual
{
// needs TMpro
    public class rvUIToolbarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IUItooltip, IUISkinEdit
    {
        public Color ColorBaseAppearance;
        public Color ColorOnMouseOver;
        public bool useSkinColors = true;

        public Image IconStartButton;

        public UnityEvent OnClick;

        public bool UsedAsToggle;
        [ShowIf("UsedAsToggle")] public UnityEvent OnToggleOn;
        [ShowIf("UsedAsToggle")] public UnityEvent OnToggleOff;
        [ShowIf("UsedAsToggle")] public Image IconPressedTogglebutton;

        [ShowIf("UsedAsToggle")] public bool UsedAsToggleWithColorChange;

        [ShowIf("UsedAsToggleWithColorChange")]
        public Color ToggleOnColor;

        [ShowIf("UsedAsToggleWithColorChange")]
        public Color IconColorToggleActive;

        [ShowIf("UsedAsToggleWithColorChange")]
        public Color IconColorToggleInactive;


        public string ItemTooltip;
        
        public UI.Tooltipposition TooltipPosition;


        [ReadOnly] public bool IsOn;
        private RectTransform _rectTransform;

        private Image bgImg;
        private readonly Vector3[] corners = new Vector3[4];
        private Vector2[] corners2D = new Vector2[4];
        private RealvirtualUISkin currentSkin;
        private bool initialized;
        private bool istouched;

        private void Awake()
        {
            Init();
        }

        public void ToggleOn()
        {
            OnToggleOn.Invoke();
        }
        
        public void ToggleOff()
        {
            OnToggleOff.Invoke();
        }

        // activate click event
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsOn)
            {
                IsOn = false;
                if (UsedAsToggle && OnToggleOff != null)
                    OnToggleOff.Invoke();
            }
            else
            {
                IsOn = true;
                if (UsedAsToggle && OnToggleOn != null)
                    OnToggleOn.Invoke();
            }

            OnChanged(); // set local settings

            if (OnClick != null)
                OnClick.Invoke();
        }
        public void SetColor(Color color)
        {
            // get all Images in Childrens
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.gameObject != gameObject)
                     img.color = color;
            }
        }
        // activate hover color on mouse over
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (istouched)
                return;
            if (eventData.pointerId != -1)
            {
                istouched = true;
                return;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, eventData.position))
                bgImg.color = new Color(ColorOnMouseOver.r, ColorOnMouseOver.g, ColorOnMouseOver.b, ColorOnMouseOver.a);
        }
        // deactivate hover color on mouse exit
        public void OnPointerExit(PointerEventData eventData)
        {
            if (istouched)
                return;

            if (UsedAsToggleWithColorChange)
            {
                if (IsOn)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, eventData.position))
                        bgImg.color = new Color(ToggleOnColor.r, ToggleOnColor.g, ToggleOnColor.b, ToggleOnColor.a);
                }
                else
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, eventData.position))
                        bgImg.color = new Color(ColorBaseAppearance.r, ColorBaseAppearance.g, ColorBaseAppearance.b,
                            ColorBaseAppearance.a);
                }
            }
            else
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, eventData.position))
                    bgImg.color = new Color(ColorBaseAppearance.r, ColorBaseAppearance.g, ColorBaseAppearance.b,
                        ColorBaseAppearance.a);
            }
        }
        // Interface for skin change
        public void UpdateUISkinParameter(RealvirtualUISkin skin)
        {
            if (!useSkinColors)
            {
                return;
            }
            currentSkin = skin;
            if (GetComponent<StartCameraPosition>() || GetComponent<ButtonShowGroup>() ||
                GetComponent<SignalConnection>())  // Change according to usage in OverlayButtons
            {
                ColorBaseAppearance = skin.ToolbarBackgroundColor;
            }
            else
            {
                ColorBaseAppearance = skin.ToolbarButtonColor;
            }
            ColorOnMouseOver = skin.ToolbarHoverColor;
            ToggleOnColor = skin.ToolbarSelectedColor;
            IconColorToggleActive = skin.ToolbarButtonToggleActiveColor;

            if (UsedAsToggle || IconStartButton != null)
            {
                if(!useSkinColors)
                {
                    IconStartButton.color = skin.ToolbarButtonIconColor;
                    if (IconPressedTogglebutton != null) IconPressedTogglebutton.color = skin.ToolbarButtonIconColor;
                }
            }
            else
            {
                ColorBaseAppearance = skin.WindowButtonColor;
                ColorOnMouseOver = skin.WindowHoverColor;
            }

            if (initialized)
                OnChanged();
        }
        // Connection to tooltip
        public void ShowTooltip(ref string tooltip, ref UI.Tooltipposition currPos, ref Vector3[] cornersUI)
        {
            if (ItemTooltip != "")
            {
                tooltip = ItemTooltip;
                currPos = TooltipPosition;
                _rectTransform.GetWorldCorners(cornersUI);
            }
        }
        
        public float getHeigth()
        {
            return _rectTransform.rect.height;
        }

        public float getWidth()
        {
            return _rectTransform.rect.width;
        }

        public void HideTooltip()
        {
        }

        public void Init()
        {
            if (initialized)
                return;
            bgImg = GetComponent<Image>();
            bgImg.color = new Color(ColorBaseAppearance.r, ColorBaseAppearance.g, ColorBaseAppearance.b,
                ColorBaseAppearance.a);
            _rectTransform = GetComponent<RectTransform>();
            if (ItemTooltip != "") _rectTransform.GetWorldCorners(corners);

            IconStartButton.gameObject.SetActive(true);
            if (UsedAsToggle)
            {
                if (IsOn)
                    setIconPushbutton(true);
                else
                    setIconPushbutton(false);
            }
            else
            {
                IsOn = false;
            }

            initialized = true;
        }

        // set status of button
        public void SetStatus(bool newValue)
        {
            IsOn = newValue;
            OnChanged();
        }

        // set icon button appearance
        private void setIconPushbutton(bool pressed)
        {
            if (pressed)
            {
                if (IconPressedTogglebutton != IconStartButton && IconPressedTogglebutton!=null)
                {
                    IconStartButton.gameObject.SetActive(false);
                    IconPressedTogglebutton.gameObject.SetActive(true);
                }

                if (UsedAsToggleWithColorChange)
                {
                    bgImg.color = new Color(ToggleOnColor.r, ToggleOnColor.g, ToggleOnColor.b, ToggleOnColor.a);

                    var col = IconColorToggleActive;
                    IconStartButton.color = new Color(col.r, col.g, col.b, col.a);
                    if(IconPressedTogglebutton!=null)
                        IconPressedTogglebutton.color = new Color(col.r, col.g, col.b, col.a);

                }
            }
            else
            {
                if (IconPressedTogglebutton != IconStartButton && IconPressedTogglebutton!=null)
                {
                    IconStartButton.gameObject.SetActive(true);
                    IconPressedTogglebutton.gameObject.SetActive(false);
                }

                if (UsedAsToggleWithColorChange)
                {
                    bgImg.color = new Color(ColorBaseAppearance.r, ColorBaseAppearance.g, ColorBaseAppearance.b,
                        ColorBaseAppearance.a);
                    var col = IconColorToggleInactive;
                    if (IconPressedTogglebutton != null)
                        IconPressedTogglebutton.color = new Color(col.r, col.g, col.b, col.a);
                            
                    IconStartButton.color = new Color(col.r, col.g, col.b, col.a);
                }
            }
        }



        // Update appearance of button
        private void OnChanged()
        {
            if (IsOn)
            {
                if (UsedAsToggle) setIconPushbutton(true);
                if (UsedAsToggleWithColorChange)
                    bgImg.color = new Color(ToggleOnColor.r, ToggleOnColor.g, ToggleOnColor.b, ToggleOnColor.a);
            }
            else
            {
                if (UsedAsToggle) setIconPushbutton(false);
            }
        }

    }
}