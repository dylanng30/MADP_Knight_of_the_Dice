using MADP.Managers;

namespace MADP.Views.MainPanels.Interfaces
{
    public interface IMainPanel
    {
        void Initialize(GameManager gameManager);
        void Show();
        void Hide();
    }
}