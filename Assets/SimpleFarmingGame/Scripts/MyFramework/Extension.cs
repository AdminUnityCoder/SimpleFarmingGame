using UnityEngine;
using UnityEngine.UI;

namespace MyFramework
{
    public static class TransformExtension
    {
        #region Position

        public static T SetPosition<T>(this T selfComponent, Vector3 position) where T : Component
        {
            selfComponent.transform.position = position;
            return selfComponent;
        }

        public static Vector3 GetPosition<T>(this T selfComponent) where T : Component
        {
            return selfComponent.transform.position;
        }

        #endregion
    }

    public static class BehaviourExtension
    {
        public static T Enable<T>(this T selfBehaviour) where T : Behaviour
        {
            selfBehaviour.enabled = true;
            return selfBehaviour;
        }

        public static T Disable<T>(this T selfBehaviour) where T : Behaviour
        {
            selfBehaviour.enabled = false;
            return selfBehaviour;
        }

        public static T SetSprite<T>(this T selfBehaviour, Sprite sprite) where T : Image
        {
            selfBehaviour.sprite = sprite;
            return selfBehaviour;
        }
    }

    public static class GameObjectExtension
    {
        #region Show

        public static GameObject Show(this GameObject selfObj)
        {
            selfObj.SetActive(true);
            return selfObj;
        }

        public static T Show<T>(this T selfComponent) where T : Component
        {
            selfComponent.gameObject.Show();
            return selfComponent;
        }

        #endregion

        #region Hide

        public static GameObject Hide(this GameObject selfObj)
        {
            selfObj.SetActive(false);
            return selfObj;
        }

        public static T Hide<T>(this T selfComponent) where T : Component
        {
            selfComponent.gameObject.Hide();
            return selfComponent;
        }

        #endregion

        #region DestroyGameObj

        public static void DestroyGameObj<T>(this T selfBehaviour) where T : Component
        {
            selfBehaviour.gameObject.DestroySelf();
        }

        #endregion
    }

    public static class ObjectExtension
    {
        #region Destroy Self

        public static void DestroySelf<T>(this T selfObj) where T : Object
        {
            Object.Destroy(selfObj);
        }

        #endregion
    }
}