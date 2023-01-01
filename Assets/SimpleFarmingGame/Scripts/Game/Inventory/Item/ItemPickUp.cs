using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Item item))
            {
                if (item != null)
                {
                    if (item.ItemDetails.CanPickedUp)
                    {
                        InventoryManager.Instance.AddItemInBag(item, true);
                        EventSystem.CallPlaySoundEvent(SoundName.PickupPop);
                    }
                }
            }
        }
    }
}