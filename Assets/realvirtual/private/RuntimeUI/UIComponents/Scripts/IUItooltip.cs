using UnityEngine;

namespace realvirtual
{

    public interface IUItooltip
    {

        void ShowTooltip(ref string _tooltip, ref UI.Tooltipposition currPos, ref Vector3[] corners);
        void HideTooltip();

        float getHeigth();
        float getWidth();

        public void OnDragStarted()
        {
        }

    }


}