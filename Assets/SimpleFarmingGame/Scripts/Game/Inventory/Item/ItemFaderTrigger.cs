using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class ItemFaderTrigger : MonoBehaviour
    {
        private IFadable[] m_ItemFaders;

        private void OnTriggerEnter2D(Collider2D other)
        {
            m_ItemFaders = other.GetComponentsInChildren<IFadable>();
            if (m_ItemFaders.Length <= 0) return;
            foreach (IFadable fader in m_ItemFaders)
            {
                fader.FadeOut();
            }

            // Debug.Log(m_ItemFaders.Length);
            m_ItemFaders = null;
            // Debug.Log(m_ItemFaders.Length);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            m_ItemFaders = other.GetComponentsInChildren<IFadable>();
            if (m_ItemFaders.Length <= 0) return;
            foreach (IFadable fader in m_ItemFaders)
            {
                fader.FadeIn();
            }

            // Array.Clear(m_ItemFaders, 0, m_ItemFaders.Length);
            // Debug.Log(m_ItemFaders.Length);
            m_ItemFaders = null;
            // Debug.Log(m_ItemFaders.Length);
        }
    }
}