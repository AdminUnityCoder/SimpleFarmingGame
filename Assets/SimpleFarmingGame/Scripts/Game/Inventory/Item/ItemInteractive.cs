using System.Collections;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class ItemInteractive : MonoBehaviour
    {
        private bool m_IsPlayAnimation; // 是否正在播放动画
        private WaitForSeconds m_AnimationInterval = new(0.04f);

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (m_IsPlayAnimation == false)
            {
                // 对方在左侧 向右摇晃
                StartCoroutine
                (
                    other.transform.position.x < transform.position.x
                        ? ShakeRightCoroutine()
                        : ShakeLeftCoroutine()
                );
                EventSystem.CallPlaySoundEvent(SoundName.Rustle);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (m_IsPlayAnimation == false)
            {
                // 对方在左侧 向右摇晃
                StartCoroutine
                (
                    other.transform.position.x > transform.position.x
                        ? ShakeRightCoroutine()
                        : ShakeLeftCoroutine()
                );
                EventSystem.CallPlaySoundEvent(SoundName.Rustle);
            }
        }

        private IEnumerator ShakeLeftCoroutine()
        {
            m_IsPlayAnimation = true;

            for (int i = 0; i < 4; ++i)
            {
                transform.GetChild(0).Rotate(0, 0, 2);
                yield return m_AnimationInterval;
            }

            for (int i = 0; i < 5; ++i)
            {
                transform.GetChild(0).Rotate(0, 0, -2);
                yield return m_AnimationInterval;
            }

            transform.GetChild(0).Rotate(0, 0, 2);
            yield return m_AnimationInterval;
            m_IsPlayAnimation = false;
        }

        private IEnumerator ShakeRightCoroutine()
        {
            m_IsPlayAnimation = true;

            for (int i = 0; i < 4; ++i)
            {
                transform.GetChild(0).Rotate(0, 0, -2);
                yield return m_AnimationInterval;
            }

            for (int i = 0; i < 5; ++i)
            {
                transform.GetChild(0).Rotate(0, 0, 2);
                yield return m_AnimationInterval;
            }

            transform.GetChild(0).Rotate(0, 0, -2);
            yield return m_AnimationInterval;
            m_IsPlayAnimation = false;
        }
    }
}