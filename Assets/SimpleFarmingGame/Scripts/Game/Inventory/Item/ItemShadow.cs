using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemShadow : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ItemSprite;
        private SpriteRenderer m_ShadowSprite;
        private Color m_BlackTranslucentColor = new(0f, 0f, 0f, 0.3f);

        private void Awake()
        {
            m_ShadowSprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            m_ShadowSprite.sprite = ItemSprite.sprite;
            m_ShadowSprite.color = m_BlackTranslucentColor;
        }
    }
}