using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<string, Vector3> TransitionEvent;

        public static void CallTransitionEvent(string sceneName, Vector3 position)
        {
            TransitionEvent?.Invoke(sceneName, position);
        }

        public static event Action BeforeSceneUnloadedEvent;

        public static void CallBeforeSceneUnloadedEvent()
        {
            BeforeSceneUnloadedEvent?.Invoke();
        }

        public static event Action AfterSceneLoadedEvent;

        public static void CallAfterSceneLoadedEvent()
        {
            AfterSceneLoadedEvent?.Invoke();
        }

        public static event Action<Vector3> MoveToPositionEvent;

        public static void CallMoveToPositionEvent(Vector3 position)
        {
            MoveToPositionEvent?.Invoke(position);
        }
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class Transition : MonoBehaviour
    {
        public string SceneToGo;
        public Vector3 PositionToGo;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                EventSystem.CallTransitionEvent(SceneToGo, PositionToGo);
            }
        }
    }
}