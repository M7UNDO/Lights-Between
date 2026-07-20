using UnityEngine;
using UnityEngine.AI;

public interface ICreatureState
{
    void Enter();
    void Update();
    void Exit();
}

public class CreatureHiddenState : ICreatureState
{
    private CreatureAI ai;

    public CreatureHiddenState(CreatureAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        if (ai.Agent != null)
        {
            ai.Agent.isStopped = true;
            ai.Agent.ResetPath();
        }

        ai.SetVisibility(false);

        if (ai.VoiceAudio != null)
        {
            ai.VoiceAudio.EnableWhispers(false);
        }

        if (ai.LightSensor != null)
        {
            ai.LightSensor.ResetLightStatus();
        }
    }

    public void Update() { }
    public void Exit() { }
}

public class CreatureDisabledState : ICreatureState
{
    private CreatureAI ai;

    public CreatureDisabledState(CreatureAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        if (ai.Agent != null && ai.Agent.isActiveAndEnabled && ai.Agent.isOnNavMesh)
        {
            ai.Agent.isStopped = true;
            ai.Agent.ResetPath();
        }

        ai.SetVisibility(false);

        if (ai.VoiceAudio != null)
        {
            ai.VoiceAudio.EnableWhispers(false);
        }

        if (ai.LightSensor != null)
        {
            ai.LightSensor.ResetLightStatus();
        }
    }

    public void Update() { }
    public void Exit() { }
}

public class CreatureRevealState : ICreatureState
{
    private CreatureAI ai;
    private Transform revealPoint;

    public CreatureRevealState(CreatureAI ai, Transform revealPoint)
    {
        this.ai = ai;
        this.revealPoint = revealPoint;
    }

    public void Enter()
    {
        ai.WarpToPoint(revealPoint);
        ai.SetVisibility(true);

        if (ai.Agent != null)
        {
            ai.Agent.isStopped = true;
            ai.Agent.ResetPath();
        }

        if (ai.VoiceAudio != null)
        {
            ai.VoiceAudio.EnableWhispers(true);
        }
    }

    public void Update()
    {
        if (ai.LookAtPlayerWhenRevealed && ai.Player != null)
        {
            Vector3 direction = ai.Player.position - ai.Visuals.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                ai.Visuals.transform.rotation = Quaternion.Slerp(ai.Visuals.transform.rotation, targetRotation, Time.deltaTime * ai.LookAtPlayerSpeed);
            }
        }
    }

    public void Exit() { }
}

public class CreatureChaseState : ICreatureState
{
    private CreatureAI ai;

    public CreatureChaseState(CreatureAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.SetVisibility(true);

        if (ai.Agent != null)
        {
            ai.Agent.isStopped = false;
            ai.Agent.speed = ai.ChaseSpeed;
        }

        if (ai.VoiceAudio != null)
        {
            ai.VoiceAudio.EnableWhispers(false);
            ai.VoiceAudio.PlayChaseVoice();
        }
    }

    public void Update()
    {
        if (ai.Agent == null || ai.Player == null) return;

        ai.Agent.SetDestination(ai.Player.position);

        float distance = Vector3.Distance(ai.transform.position, ai.Player.position);
        if (distance <= ai.KillDistance)
        {
            FPSController playerController = ai.Player.GetComponent<FPSController>();
            PlayerKill playerKill = Object.FindFirstObjectByType<PlayerKill>(FindObjectsInactive.Include);

            if (playerController != null && playerKill != null)
            {
                playerKill.gameObject.SetActive(true);
                playerKill.ExecuteCatchSequence(playerController, ai.gameObject);
                ai.ChangeState(new CreatureDisabledState(ai));
            }
        }
    }

    public void Exit() { }
}

public class CreatureFleeState : ICreatureState
{
    private CreatureAI ai;
    private float stateTimer;
    private Vector3 fleeTarget;

    public CreatureFleeState(CreatureAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        stateTimer = 0f;
        ai.SetVisibility(true);

        Vector3 away = (ai.transform.position - ai.Player.position).normalized;
        if (away.sqrMagnitude <= 0.01f) away = -ai.transform.forward;

        fleeTarget = ai.transform.position + away * ai.FleeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            fleeTarget = hit.position;
        }

        if (ai.Agent != null)
        {
            ai.Agent.isStopped = false;
            ai.Agent.speed = ai.FleeSpeed;
            ai.Agent.SetDestination(fleeTarget);
        }

        if (ai.VoiceAudio != null)
        {
            ai.VoiceAudio.EnableWhispers(false);
            ai.VoiceAudio.PlayLightReaction();
        }
    }

    public void Update()
    {
        stateTimer += Time.deltaTime;

        if (ai.Agent == null)
        {
            ai.Disappear();
            return;
        }

        float distance = Vector3.Distance(ai.transform.position, ai.Player.position);

        if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= 0.8f)
        {
            ai.Disappear();
            return;
        }

        if (distance >= ai.DisappearDistance || stateTimer >= ai.FleeDuration)
        {
            ai.Disappear();
        }
    }

    public void Exit() { }
}

public class CreatureDynamicState : ICreatureState
{
    private CreatureAI ai;
    private bool isSabotaging;

    public CreatureDynamicState(CreatureAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.SetVisibility(true);
        isSabotaging = false;

        if (ai.VoiceAudio != null)
        {
            ai.VoiceAudio.EnableWhispers(true);
        }
    }

    public void Update()
    {
        UpdateTension();
        EvaluateBehavior();
    }

    private void UpdateTension()
    {
        if (ai.GeneratorSystem != null && ai.GeneratorSystem.promptMessage == "Generator is running")
        {
            ai.TensionLevel += ai.TensionIncreaseRate * Time.deltaTime;
        }
        else
        {
            ai.TensionLevel = Mathf.Max(0f, ai.TensionLevel - (ai.TensionIncreaseRate * 0.5f) * Time.deltaTime);
        }
    }

    private void EvaluateBehavior()
    {
        if (ai.TensionLevel >= ai.DirectHuntThreshold)
        {
            ai.ChangeState(new CreatureChaseState(ai));
            return;
        }

        if (ai.GeneratorSystem != null && ai.GeneratorSystem.promptMessage == "Generator is running" && !isSabotaging)
        {
            if (Random.value < 0.05f * Time.deltaTime)
            {
                isSabotaging = true;
            }
        }

        if (isSabotaging)
        {
            ExecuteSabotage();
        }
        else
        {
            ExecuteStalk();
        }
    }

    private void ExecuteSabotage()
    {
        if (ai.BreakerBoxTransform != null && ai.Agent != null)
        {
            ai.Agent.isStopped = false;
            ai.Agent.SetDestination(ai.BreakerBoxTransform.position);

            if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= 1.5f)
            {
                if (ai.GeneratorSystem != null)
                {
                    ai.GeneratorSystem.TurnGeneratorOff();
                }
                isSabotaging = false;
                ai.Disappear();
            }
        }
    }

    private void ExecuteStalk()
    {
        if (ai.Agent == null || ai.Player == null) return;

        Vector3 stalkPosition = ai.Player.position - ai.Player.forward * ai.StalkDistance;
        if (NavMesh.SamplePosition(stalkPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            ai.Agent.isStopped = false;
            ai.Agent.SetDestination(hit.position);
        }
    }

    public void Exit() { }
}