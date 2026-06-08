using UnityEngine;

public class SpawnPanel : MonoBehaviour
{
    [Header("UI Structure")]
    public GameObject spawnSubmenu;

    [Header("Costs")]
    public int foragerCost = 10;
    public int nurseCost = 5;
    public int houseCost = 5;
    public int builderCost = 15;
    public int droneCost = 20;

    public void TogglePanel()
    {
        NurseBeeZone zone = FindObjectOfType<NurseBeeZone>();
        if (zone != null && zone.nurseCanvas != null)
        {
            GameObject panel = zone.nurseCanvas;
            panel.SetActive(!panel.activeSelf);

            if (panel.activeSelf && ClickManager.Instance != null)
            {
                ClickManager.Instance.RegisterCanvas(panel);
            }
        }
        else
        {
            UIMessagePopup.Instance.ShowMessage("You need a Brood Chamber to breed bees!");
        }
    }

    public void SpawnForager() => TrySpawn(EggType.Forager, Bee.BeeType.Forager, foragerCost);
    public void SpawnNurse()   => TrySpawn(EggType.Nurse, Bee.BeeType.Nurse, nurseCost);
    public void SpawnHouse()   => TrySpawn(EggType.House, Bee.BeeType.House, houseCost);
    public void SpawnBuilder() => TrySpawn(EggType.Builder, Bee.BeeType.Builder, builderCost);
    public void SpawnDrone()   => TrySpawn(EggType.Drone, Bee.BeeType.Drone, droneCost);

    private void TrySpawn(EggType eggType, Bee.BeeType requiredZoneType, int cost)
    {
        if (EggNamePopup.IsOpen) return;

        // Find a valid Brood to spawn the egg
        NurseBeeZone[] broods = FindObjectsOfType<NurseBeeZone>();
        if (broods.Length == 0)
        {
            UIMessagePopup.Instance.ShowMessage("You need a Brood Chamber to spawn bees!");
            return;
        }

        // Check if required zone exists (Forager needs Storage, Nurse needs Nurse, etc)
        if (ZoneManager.Instance != null && !ZoneManager.Instance.HasZone(requiredZoneType))
        {
            UIMessagePopup.Instance.ShowMessage($"You must build a {requiredZoneType} Zone first!");
            return;
        }

        if (!HiveManager.Instance.CanSpawnBee())
        {
            UIMessagePopup.Instance.ShowMessage("Hive is full! Build more Brood Chambers.");
            return;
        }

        if (!CurrencyManager.Instance.UseHoney(cost))
        {
            UIMessagePopup.Instance.ShowMessage("Not enough honey!");
            return;
        }

        // Pick a random brood for the physical egg
        NurseBeeZone targetBrood = broods[Random.Range(0, broods.Length)];

        HiveManager.Instance.RegisterQueuedEgg();

        // Spawn it physically
        Egg worldEgg = EggSpawner.Instance.SpawnEgg(eggType, targetBrood);

        if (worldEgg != null)
        {
            worldEgg.OnHatched += HiveManager.Instance.UnregisterQueuedEgg;
            EggNamePopup.Instance.Open(worldEgg);
        }
        else
        {
            HiveManager.Instance.UnregisterQueuedEgg();
            // Refund if spawn failed unexpectedly
            CurrencyManager.Instance.AddHoney(cost);
            UIMessagePopup.Instance.ShowMessage("Failed to spawn egg.");
        }
    }
}
