using SFG.AudioSystem;
using UnityEngine;

namespace SFG.InventorySystem
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
                        AudioSystem.EventSystem.CallPlaySoundEvent(SoundName.PickupPop);
                    }
                }
            }
        }
    }
}