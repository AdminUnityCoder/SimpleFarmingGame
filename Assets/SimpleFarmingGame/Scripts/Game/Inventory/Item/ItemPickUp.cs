using MyFramework;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Item item)) return;
            if (!item.ItemDetails.CanPickedUp) return;
            InventoryManager.Instance.AddItemToBag(item);
            item.DestroyGameObj();
            EventSystem.CallPlaySoundEvent(SoundName.PickupPop);
        }
    }
}