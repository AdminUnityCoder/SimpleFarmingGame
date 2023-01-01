using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(menuName = "ScriptableObject/Crop/CropData", fileName = "CropDataSO")]
    public class CropDataListSO : ScriptableObject
    {
        public List<CropDetails> CropDetailsList;

        public CropDetails GetCropDetails(int cropSeedID)
        {
            return CropDetailsList.Find(cropDetails => cropDetails.CropSeedID == cropSeedID);
        }
    }
}