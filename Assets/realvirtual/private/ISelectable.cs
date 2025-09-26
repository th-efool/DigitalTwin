namespace realvirtual
{
    public interface ISelectable
    {


        public void OnSelected(bool multiselected);
        public void OnDeselected();

        public void OnHovered();
        public void OnUnhovered();

    }
}