using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(fileName = "SceneSoundListSO", menuName = "ScriptableObject/Audio/SceneSoundList")]
    public class SceneSoundListSO : ScriptableObject
    {
        public List<SceneSound> SceneSoundList;

        public SceneSound GetSceneSound(string sceneName) =>
            SceneSoundList.Find(sceneSound => sceneSound.SceneName == sceneName);
    }

    [Serializable]
    public class SceneSound
    {
        public string SceneName;
        [Tooltip("环境音")] public SoundName Ambient;
        [Tooltip("背景音")] public SoundName Music;
    }
}