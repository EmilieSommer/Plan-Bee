using UnityEngine;

public class BuildZoneSelector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                BuildZone zone = hit.collider.GetComponent<BuildZone>();

                if (zone != null)
                {
                    AssignBuilders(zone);
                }
            }
        }
    }

    void AssignBuilders(BuildZone zone)
    {
        // Deselect all other zones (optional but recommended)
        BuildZone[] allZones = FindObjectsOfType<BuildZone>();
        foreach (var z in allZones)
        {
            z.isSelected = false;
        }

        // Select this one
        zone.isSelected = true;

        BuilderBee[] builders = FindObjectsOfType<BuilderBee>();

        foreach (var bee in builders)
        {
            bee.AssignBuildZone(zone);
        }
    }
}