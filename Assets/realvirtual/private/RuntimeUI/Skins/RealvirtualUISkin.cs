using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace realvirtual
{
    [Serializable]
    [CreateAssetMenu(fileName = "RealvirtualUISkin", menuName = "realvirtual/Create realvirtual UI skin", order = 1)]
    public class RealvirtualUISkin : ScriptableObject
    {

        [Header("realvirtual skin options")]
        
        [Header("Toolbar colors")] 

        [SerializeField] public Color ToolbarBackgroundColor;
        [SerializeField] public Color ToolbarButtonColor;
        [SerializeField] public Color ToolbarHoverColor;
        [SerializeField] public Color ToolbarSelectedColor;
        [SerializeField] public Color ToolbarButtonIconColor;
        [SerializeField] public Color ToolbarButtonToggleActiveColor;

        [Header("Mainmenu Settings")] 

        [SerializeField] public Color MainMenuBackgroundColor;
        [SerializeField] public Color MainMenuHoverColor;
        [SerializeField] public Font MainMenuFont;
        [SerializeField] public int MainMenuFontSize;
        [SerializeField] public Color MainMenuFontColor;

        [Header("Collection Browser Settings")] 
        [SerializeField] public Color CollectionBrowserBackgroundColor;
        [SerializeField] public Color CollectionBrowserHoverColor;
        [SerializeField] public Font CollectionBrowserFont;
        [SerializeField] public Color CollectionBrowserFontColor;
        [SerializeField] public int CollectionBrowserFontSize;
        
        [Header("Annotation Settings")]
        [SerializeField] public Color AnnotationBackgroundColor;
        [SerializeField] public Color AnnotationFontColor;
        [SerializeField] public Font AnnotationFont;
        
        [Header("Tooltip Settings")]
        [SerializeField] public Color TooltipBackgroundColor;
        [SerializeField] public Color TooltipFontColor;
        [SerializeField] public Font TooltipFont;

        [Header("Window Settings")] 
        [SerializeField] public Color WindowHeaderBackgroundColor;
        [SerializeField] public Color WindowContentBackgroundColor;
        [SerializeField] public Color WindowButtonColor;
        [SerializeField] public Color WindowHoverColor;
        [SerializeField] public Color WindowSelectedColor;
        [SerializeField] public Font WindowFont;
        [SerializeField] public Color WindowFontColor;
        [SerializeField] public int WindowFontSize;
        [SerializeField] public int WindowMessageFontSize;


        [Header("General Settings")]
        [SerializeField] public Color SelectedColor;
        [SerializeField] public TMP_FontAsset BaseFontTMP;
        [SerializeField] public Font BaseFont;

    }
 
}
