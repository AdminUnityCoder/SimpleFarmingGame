using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class MenuUI : MonoBehaviour
    {
        public GameObject[] Panels;

        public void SwitchPanel(int index)
        {
            for (int i = 0; i < Panels.Length; ++i)
            {
                if (i == index)
                {
                    // 将转换移动到本地转换列表的末尾，也就是显示在最前边（最先渲染）。
                    Panels[i].transform.SetAsLastSibling();
                }
            }
        }

        public void ExitGame()
        {
            Application.Quit();
            Debug.Log("EXIT GAME");
        }
    }
}