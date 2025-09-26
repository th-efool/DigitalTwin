using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace realvirtual
{
	public class UIWindowMovement : MonoBehaviour
	{
		private UIElementResize uiElementResize;
		private float minWidth = 200;
		private float minHeight = 200;
        private RectTransform canvasTR;
        private Camera canvasCam;

        public delegate void OnResizeEndDelegate();
        public event OnResizeEndDelegate EventOnResizeEnd;

     

        [SerializeField]
        public RectTransform window;
        
        private Vector2 initialTouchPos = Vector2.zero;
        private Vector2 initialAnchoredPos, initialSizeDelta;
        // Start is called before the first frame update
        public void Initialize(UIElementResize uielement)
        {
			uiElementResize = uielement;
			canvasTR = uielement.GetComponent<RectTransform>();
	        minWidth = uielement.minWidth;
	        minHeight = uielement.minHeight;
	        
        }
        
		public void OnDragStarted( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			canvasCam = pointer.pressEventCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( window, pointer.pressPosition, canvasCam, out initialTouchPos );
		}

		public void OnDrag( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			Vector2 touchPos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( window, pointer.position, canvasCam, out touchPos );
			window.anchoredPosition += touchPos - initialTouchPos;
		}

		public void OnEndDrag( BaseEventData data )
		{
			uiElementResize.EnsureWindowIsWithinBounds();
			
		}

		public void OnResizeStarted( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			canvasCam = pointer.pressEventCamera;
			initialAnchoredPos = window.anchoredPosition;
			initialSizeDelta = window.sizeDelta;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( canvasTR, pointer.pressPosition, canvasCam, out initialTouchPos );
		}

		public void OnResize( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			Vector2 touchPos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( canvasTR, pointer.position, canvasCam, out touchPos );

			Vector2 delta = touchPos - initialTouchPos;
			Vector2 newSize = initialSizeDelta + new Vector2( delta.x, -delta.y );
			Vector2 canvasSize = canvasTR.sizeDelta;

			if( newSize.x <minWidth ) newSize.x = minWidth;
			if( newSize.y < minHeight ) newSize.y = minHeight;

			if( newSize.x > canvasSize.x ) newSize.x = canvasSize.x;
			if( newSize.y > canvasSize.y ) newSize.y = canvasSize.y;

			newSize.x = (int) newSize.x;
			newSize.y = (int) newSize.y;

			delta = newSize - initialSizeDelta;

			window.anchoredPosition = initialAnchoredPos + new Vector2( delta.x * 0.5f, delta.y * -0.5f );

			if( window.sizeDelta != newSize )
			{
				
				uiElementResize.OnWindowDimensionsChanged(newSize);
			}
			
		}

		public void OnEndResize( BaseEventData data )
		{
			
			uiElementResize.EnsureWindowIsWithinBounds();
			if(EventOnResizeEnd != null)
				EventOnResizeEnd();
		}
       
    }
}
