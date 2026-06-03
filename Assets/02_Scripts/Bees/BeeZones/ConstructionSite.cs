using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    public BuildZone parentZone;

    private bool building = false;

    public void StartBuild()
    {
        building = true;
    }

    public void CompleteBuild()
    {
        if (parentZone != null)
            parentZone.FinishBuild();

        Destroy(gameObject);
    }
}
