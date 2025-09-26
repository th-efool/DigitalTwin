using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
using Screen = UnityEngine.Device.Screen; // To support Device Simulator on Unity 2021.1+
#endif

// Manager class for the debug popup
namespace IngameDebugConsole
{
	public class DebugLogPopup : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public Image InfoImage; // added for realvirtual.io
		public Image WarningImage; // added for realvirtual.io
		public Image ErrorImage; // added for realvirtual.io
		
		private RectTransform popupTransform;

		// Dimensions of the popup divided by 2
		private Vector2 halfSize;

		// Background image that will change color to indicate an alert
		private Image backgroundImage;

		// Canvas group to modify visibility of the popup
		private CanvasGroup canvasGroup;

#pragma warning disable 0649
		[SerializeField]
		private DebugLogManager debugManager;
		
		[SerializeField]
		private Text newInfoCountText;
		[SerializeField]
		private Text newWarningCountText;
		[SerializeField]
		private Text newErrorCountText;

		[SerializeField]
		private Color alertColorInfo;
		[SerializeField]
		private Color alertColorWarning;
		[SerializeField]
		private Color alertColorError;
#pragma warning restore 0649

		// Number of new debug entries since the log window has been closed
		private int newInfoCount = 0, newWarningCount = 0, newErrorCount = 0;

		private Color normalColor;

		private bool isPopupBeingDragged = false;
		private Vector2 normalizedPosition;

		// Coroutines for simple code-based animations
		private IEnumerator moveToPosCoroutine = null;

		public bool IsVisible { get; private set; }

		private void Awake()
		{
			popupTransform = (RectTransform) transform;
			backgroundImage = GetComponent<Image>();
			canvasGroup = GetComponent<CanvasGroup>();

			normalColor = backgroundImage.color;

			// added for  realvirtual.io
			/*halfSize = popupTransform.sizeDelta * 0.5f;

			Vector2 pos = popupTransform.anchoredPosition;
			if( pos.x != 0f || pos.y != 0f )
				normalizedPosition = pos.normalized; // Respect the initial popup position set in the prefab
			else
				normalizedPosition = new Vector2( 0.5f, 0f ); // Right edge by default*/
		}

		public void NewLogsArrived( int newInfo, int newWarning, int newError )
		{
			if( newInfo > 0 )
			{
				newInfoCount += newInfo;
				newInfoCountText.text = newInfoCount.ToString();
			}

			if( newWarning > 0 )
			{
				newWarningCount += newWarning;
				newWarningCountText.text = newWarningCount.ToString();
			}

			if( newError > 0 )
			{
				newErrorCount += newError;
				newErrorCountText.text = newErrorCount.ToString();
			}

			if( newErrorCount > 0 )
				backgroundImage.color = alertColorError;
			else if( newWarningCount > 0 )
				backgroundImage.color = alertColorWarning;
			else
				backgroundImage.color = alertColorInfo;
		}

		public void ResetValues()
		{
			newInfoCount = 0;
			newWarningCount = 0;
			newErrorCount = 0;

			newInfoCountText.text = "0";
			newWarningCountText.text = "0";
			newErrorCountText.text = "0";

			backgroundImage.color = normalColor;
		}

		// A simple smooth movement animation
		private IEnumerator MoveToPosAnimation( Vector2 targetPos )
		{
			float modifier = 0f;
			Vector2 initialPos = popupTransform.anchoredPosition;

			while( modifier < 1f )
			{
				modifier += 4f * Time.unscaledDeltaTime;
				popupTransform.anchoredPosition = Vector2.Lerp( initialPos, targetPos, modifier );

				yield return null;
			}
		}

		// Popup is clicked
		public void OnPointerClick( PointerEventData data )
		{
			// Hide the popup and show the log window
			if( !isPopupBeingDragged )
				debugManager.ShowLogWindow();
		}

		// Hides the log window and shows the popup
		public void Show()
		{
			canvasGroup.blocksRaycasts = true;
			canvasGroup.alpha = debugManager.popupOpacity;
			IsVisible = true;

			// Reset the counters
			//ResetValues();

			// Update position in case resolution was changed while the popup was hidden
			UpdatePosition( true );
		}

		// Hide the popup
		public void Hide()
		{
			canvasGroup.blocksRaycasts = false;
			canvasGroup.alpha = 0f;

			IsVisible = false;
			isPopupBeingDragged = false;
		}

		public void OnBeginDrag( PointerEventData data )
		{
			isPopupBeingDragged = true;

			// If a smooth movement animation is in progress, cancel it
			if( moveToPosCoroutine != null )
			{
				StopCoroutine( moveToPosCoroutine );
				moveToPosCoroutine = null;
			}
		}

		// Reposition the popup
		public void OnDrag( PointerEventData data )
		{
			Vector2 localPoint;
			if( RectTransformUtility.ScreenPointToLocalPointInRectangle( debugManager.canvasTR, data.position, data.pressEventCamera, out localPoint ) )
				popupTransform.anchoredPosition = localPoint;
		}

		// Smoothly translate the popup to the nearest edge
		public void OnEndDrag( PointerEventData data )
		{
			isPopupBeingDragged = false;
			UpdatePosition( false );
		}

		// There are 2 different spaces used in these calculations:
		// RectTransform space: raw anchoredPosition of the popup that's in range [-canvasSize/2, canvasSize/2]
		// Safe area space: Screen.safeArea space that's in range [safeAreaBottomLeft, safeAreaTopRight] where these corner positions
		//                  are all positive (calculated from bottom left corner of the screen instead of the center of the screen)
		public void UpdatePosition( bool immediately )
		{
		    // realvirtual.io - removed from the original for keeping position at the left bottom of the screen
		}

		public void LateUpdate()
		{
			// only show error if errorcounter is > 0
			if (newErrorCount > 0)
			{
				ErrorImage.enabled = true;
				newErrorCountText.enabled = true;
			}
			else
			{
				ErrorImage.enabled = false;
				newErrorCountText.enabled = false;
			}
			
			// only show warning if warningcounter is > 0
			if (newWarningCount > 0)
			{
				WarningImage.enabled = true;
				newWarningCountText.enabled = true;
			}
			else
			{
				WarningImage.enabled = false;
				newWarningCountText.enabled = false;
			}
			
			// only show info if infocounter is > 0
			if (newInfoCount > 0)
			{
				InfoImage.enabled = true;
				newInfoCountText.enabled = true;
			}
			else
			{
				InfoImage.enabled = false;
				newInfoCountText.enabled = false;
			}
		}
	}
}