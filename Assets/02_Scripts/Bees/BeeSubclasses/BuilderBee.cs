using UnityEngine;

public class BuilderBee : Bee
{
    private Vector3Int currentJobPos;
    private bool hasJob = false;
    public float buildSpeed = 1f;

    protected override void Update()
    {
        base.Update();

        if (HiveGrid.Instance == null) return;

        if (!hasJob)
        {
            if (HiveGrid.Instance.TryDequeueBuildJob(out currentJobPos))
            {
                hasJob = true;
                targetPosition = HiveGrid.Instance.CellToWorld(currentJobPos);
                currentState = BeeState.Moving;
            }
            else
            {
                // No jobs, wait
                if (currentState != BeeState.Idle)
                    currentState = BeeState.Idle;
            }
        }
        else
        {
            // We have a job, check if we arrived
            if (currentState == BeeState.Moving)
            {
                if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
                {
                    currentState = BeeState.Working;
                }
            }
            else if (currentState == BeeState.Working)
            {
                // Is it still marked for building? (Another bee might have finished it)
                if (!HiveGrid.Instance.IsMarked(currentJobPos))
                {
                    hasJob = false;
                    currentState = BeeState.Idle;
                }
                else
                {
                    HiveGrid.Instance.AddBuildProgress(currentJobPos, buildSpeed * Time.deltaTime);
                }
            }
        }
    }

    protected override void Die()
    {
        // Ideally we'd put the job back in the queue if we die while working, 
        // but for now we just die.
        base.Die(); 
    }

    protected override void WorkBehavior() { }
    protected override void ReturnBehavior() { }
}