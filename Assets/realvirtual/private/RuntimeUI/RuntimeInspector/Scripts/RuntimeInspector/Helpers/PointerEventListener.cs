using UnityEngine;
using UnityEngine.EventSystems;
#pragma warning disable CS3001
namespace RuntimeInspectorNamespace
{
	public class PointerEventListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
	{
		public delegate void PointerEvent( PointerEventData eventData );

		public event PointerEvent PointerDown, PointerUp, PointerClick;

		public void OnPointerDown( PointerEventData eventData )
		{
			if( PointerDown != null )
				PointerDown( eventData );
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			if( PointerUp != null )
				PointerUp( eventData );
		}

		public void OnPointerClick( PointerEventData eventData )
		{
			if( PointerClick != null )
				PointerClick( eventData );
		}
	}
}