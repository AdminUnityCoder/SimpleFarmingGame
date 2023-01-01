using DG.Tweening;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    internal static class FadeModel
    {
        public static Color OpaqueColor = new(1f, 1f, 1f, 1f);
        public static Color TransparentColor = new(1f, 1f, 1f, 0.45f);
        public const float TweenDuration = 0.35f;
    }

    public interface IFadable
    {
        void FadeIn();
        void FadeOut();
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemFader : MonoBehaviour, IFadable
    {
        private SpriteRenderer m_SpriteRenderer;

        private void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void FadeIn()
        {
            m_SpriteRenderer.DOColor(FadeModel.OpaqueColor, FadeModel.TweenDuration);
        }

        public void FadeOut()
        {
            m_SpriteRenderer.DOColor(FadeModel.TransparentColor, FadeModel.TweenDuration);
        }
    }
}