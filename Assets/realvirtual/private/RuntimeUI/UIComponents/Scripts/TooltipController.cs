using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace realvirtual
{
    public class TooltipController : MonoBehaviour
    {
        public GameObject CanvasParent;
        
        private GameObject currentUIobj;
        private GameObject lastUIobj;
        private IUItooltip currTipInterface;
        private Vector3[] cornersTipInterface = new Vector3[4];

        private GameObject _globalTooltip;
        private RealvirtualTooltip _realvirtualTooltip;
        private RectTransform _rectTransform;
       
        private string currTooltipText = "";
        private UI.Tooltipposition currPos;
       // booleans to check parameters
        private bool _tooltipActive=false;
        private bool _tooltipCreated = false;
        private bool _toolTipfound = false;
        private IRaycaster raycaster;


        void Awake()
        {
            raycaster = GetComponent<IRaycaster>();
        }
        
        // Update is called once per frame
        void Update()
        {
            var raysastResults = raycaster.UIRaycast();
            _resetTooltip();
            if (raysastResults.Count > 0)
            {
                _toolTipfound = false;
                foreach (var obj in raysastResults)
                {
                    if(obj.gameObject.GetComponent<IUItooltip>() != null)
                    {
                        checkTooltipObj(obj.gameObject);
                        _globalTooltip.SetActive(false);
                        currentUIobj= obj.gameObject;
                        currTipInterface= obj.gameObject.GetComponent<IUItooltip>();
                        currTooltipText = "";
                        _toolTipfound = true;
                        break;
                    }
                }
                if (_toolTipfound )
                {
                    
                    currTipInterface.ShowTooltip(ref currTooltipText, ref currPos,ref cornersTipInterface);
                    if (currTooltipText != "" && _realvirtualTooltip != null)
                    {
                        _realvirtualTooltip.SetTooltip(currTooltipText);
                        _rectTransform = CanvasParent.GetComponent<RectTransform>();
                        _globalTooltip.SetActive(true);
                        if (lastUIobj != currentUIobj)
                        {
                            Vector3 pos = getTooltipPos();
                            _realvirtualTooltip.SetPosition(pos/ _rectTransform.localScale.x);
                            lastUIobj = currentUIobj;
                        }
                        _tooltipActive = true;
                    }
                }
            }
            else
            {
                if (_tooltipActive)
                {
                   _resetTooltip();
                   currTipInterface.HideTooltip();
                }
            }
        }

        private void _resetTooltip()
        {
            if (_tooltipCreated)
            {
                _globalTooltip.SetActive(false);
                currTooltipText = "";
                _tooltipActive = false;
                lastUIobj = null;
            }
        }
        private Vector3 getTooltipPos()
        {
            Vector3 screenpos=Vector3.zero;

            switch (currPos)
            {
                case UI.Tooltipposition.Above:
                {
                    screenpos.x = adjustX();
                    screenpos.y = cornersTipInterface[1].y + 10;
                    break;
                }
                case UI.Tooltipposition.Under:
                {

                    screenpos.x = adjustX();
                    screenpos.y = cornersTipInterface[0].y  - _realvirtualTooltip.getHeight();
                    break;
                }
                case UI.Tooltipposition.Left:
                {

                    screenpos.x = cornersTipInterface[1].x - 10 - _realvirtualTooltip.getWidth();
                    screenpos.y = adjustY();
                    break;
                }
                case UI.Tooltipposition.Right:
                {
                    screenpos.x = cornersTipInterface[2].x + 20;
                    screenpos.y = adjustY();
                    break;
                }
            }
            
            return screenpos;
        }

        private float adjustX()
        {
            float x;
          if (Input.mousePosition.x > Screen.width-_realvirtualTooltip.getWidth())
                x = Input.mousePosition.x - _realvirtualTooltip.getWidth();
            else
               x = Input.mousePosition.x;

            return x;
        }
        private float adjustY()
        {
            float y;
            if (Input.mousePosition.y >= Screen.height-_realvirtualTooltip.getHeight())
                y = Input.mousePosition.y - (_realvirtualTooltip.getHeight());
            else if(Input.mousePosition.y< _realvirtualTooltip.getHeight())
                y = Input.mousePosition.y + (_realvirtualTooltip.getHeight());
            else
                y = Input.mousePosition.y;

            return y;
        }
        private void checkTooltipObj(GameObject currObj)
        {
            if (!_tooltipCreated)
            {
                _globalTooltip = UI.CreateTooltip(currObj.transform.position, currObj.transform.rotation, null);

                _tooltipCreated = true;
            }
            _realvirtualTooltip = _globalTooltip.GetComponent<RealvirtualTooltip>();
        }
    }
}