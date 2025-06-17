namespace RH.Services
{
    public class ModalService
    {
        public event Action? OnShow;

        public void Show() => OnShow?.Invoke();
    }

}
