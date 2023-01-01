using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SimpleFarmingGame.Game
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> PoolPrefab;
        private List<ObjectPool<GameObject>> m_EffectPoolList = new();

        private WaitForSeconds m_WaitForSeconds;
        private const float WaitForTime = 1.5f;

        private void OnEnable()
        {
            EventSystem.ParticleEffectEvent += OnParticleEffectEvent;
            EventSystem.InitSoundEffectEvent += InitSoundEffect;
        }

        private void OnDisable()
        {
            EventSystem.ParticleEffectEvent -= OnParticleEffectEvent;
            EventSystem.InitSoundEffectEvent -= InitSoundEffect;
        }

        private void Start()
        {
            CreateObjectPool();
            // CreateSoundEffectPool();
            m_WaitForSeconds = new WaitForSeconds(WaitForTime);
        }

        private void CreateObjectPool()
        {
            foreach (GameObject prefab in PoolPrefab) // 循环生成GameObject
            {
                Transform parent = new GameObject(prefab.name).transform;
                parent.SetParent(transform);

                ObjectPool<GameObject> newPool = new ObjectPool<GameObject>
                (
                    createFunc: () => Instantiate(prefab, parent)
                  , actionOnGet: gameObj => gameObj.SetActive(true)
                  , actionOnRelease: gameObj => gameObj.SetActive(false)
                  , actionOnDestroy: Destroy
                );

                m_EffectPoolList.Add(newPool);
            }
        }

        private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 particleGenerationPosition)
        {
            ObjectPool<GameObject> objectPool = effectType switch
            {
                ParticleEffectType.FallingLeaves01 => m_EffectPoolList[0]
              , ParticleEffectType.FallingLeaves02 => m_EffectPoolList[1]
              , ParticleEffectType.Rock => m_EffectPoolList[2]
              , ParticleEffectType.Grass => m_EffectPoolList[3]
              , _ => null
            };

            if (objectPool == null) return;
            GameObject gameObj = objectPool.Get();
            gameObj.transform.position = particleGenerationPosition;
            StartCoroutine(ReleaseCoroutine(objectPool, gameObj));
        }

        private IEnumerator ReleaseCoroutine(IObjectPool<GameObject> objectPool, GameObject gameObj)
        {
            yield return m_WaitForSeconds;
            objectPool.Release(gameObj);
        }

        // private void InitSoundEffect(SoundDetails soundDetails)
        // {
        //     ObjectPool<GameObject> soundPool = m_EffectPoolList[4];
        //     GameObject gameObj = soundPool.Get();
        //     gameObj.GetComponent<Sound>().SetSound(soundDetails);
        //     StartCoroutine(ReleaseSoundCoroutine(soundPool, gameObj, soundDetails));
        // }
        //
        // private IEnumerator ReleaseSoundCoroutine
        //     (IObjectPool<GameObject> soundPool, GameObject gameObj, SoundDetails soundDetails)
        // {
        //     yield return new WaitForSeconds(soundDetails.SoundClip.length);
        //     soundPool.Release(gameObj);
        // }

        #region EffectPool

        private Queue<GameObject> m_SoundQueue = new();

        private void CreateSoundEffectPool()
        {
            Transform parent = new GameObject("SoundEffect").transform;
            parent.SetParent(transform);
            for (int i = 0; i < 20; ++i) // 预生成
            {
                GameObject soundObj = Instantiate(PoolPrefab[4], parent);
                soundObj.SetActive(false);
                m_SoundQueue.Enqueue(soundObj);
            }
        }

        private GameObject GetSoundEffectObject()
        {
            if (m_SoundQueue.Count < 2)
            {
                CreateSoundEffectPool();
            }

            return m_SoundQueue.Dequeue();
        }

        private void InitSoundEffect(SoundDetails soundDetails)
        {
            GameObject soundObj = GetSoundEffectObject();
            soundObj.GetComponent<Sound>().SetSound(soundDetails);
            soundObj.SetActive(true);
            StartCoroutine(ReleaseSoundCoroutine(soundObj, soundDetails.SoundClip.length));
        }

        private IEnumerator ReleaseSoundCoroutine(GameObject soundObj, float duration)
        {
            yield return new WaitForSeconds(duration);
            soundObj.SetActive(false);
            m_SoundQueue.Enqueue(soundObj);
        }

        #endregion
    }
}