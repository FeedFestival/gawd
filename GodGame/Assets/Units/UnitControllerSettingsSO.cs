using UnityEngine;
namespace Game.UnitController
{
    [CreateAssetMenu(fileName = "UnitControllerSettings", menuName = "ScriptableObjects/UnitController", order = 1)]
    public class UnitControllerSettingsSO : ScriptableObject
    {
        public GameObject HexFadedPrefab;
        public GameObject HexEdgePrefab;
        public GameObject PathDotPrefab;
    }
}
