//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using Godot;
using GodotGameFramework;

namespace GameMain
{
    /// <summary>
    /// 主菜单 UI 逻辑。
    /// 根节点脚本，由 UIComponent 管理生命周期。
    /// </summary>
    public partial class MenuFormLogic : UIFormLogic
    {
        [Export] public Button StartGameButton;
        [Export] public Button QuitButton;

        private ProcedureMenu m_ProcedureMenu;

        protected override void OnOpen(object userData)
        {
            m_ProcedureMenu = userData as ProcedureMenu;

            if (StartGameButton != null)
                StartGameButton.Pressed += OnStartGameButtonPressed;

            if (QuitButton != null)
                QuitButton.Pressed += OnQuitButtonPressed;
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (StartGameButton != null)
                StartGameButton.Pressed -= OnStartGameButtonPressed;

            if (QuitButton != null)
                QuitButton.Pressed -= OnQuitButtonPressed;

            m_ProcedureMenu = null;
        }

        private void OnStartGameButtonPressed()
        {
            m_ProcedureMenu?.StartGame();
        }

        private void OnQuitButtonPressed()
        {
            GetTree().Quit();
        }
    }
}
