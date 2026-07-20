using System;
using UnityEngine;
using UnityEngine.AI;

public class CreatureAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject creatureVisuals;
    [SerializeField] private Collider creatureCollider;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private CreatureVoiceAudio creatureVoiceAudio;
    [SerializeField] private CreatureLightSensor creatureLightSensor;
    [SerializeField] private GeneratorPowerSystem generatorSystem;
    [SerializeField] private Transform breakerBoxTransform;

    [Header("Dynamic AI Settings")]
    [SerializeField] private float directHuntThreshold = 85f;
    [SerializeField] private float tensionIncreaseRate = 2f;
    [SerializeField] private float stalkDistance = 12f;

    [Header("Reveal Settings")]
    [SerializeField] private Transform currentRevealPoint;
    [SerializeField] private bool lookAtPlayerWhenRevealed = true;
    [SerializeField] private float lookAtPlayerSpeed = 6f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float killDistance = 1.4f;

    [Header("Light Reaction")]
    [SerializeField] private bool reactToLight = true;
    [SerializeField] private float lightReactionDelay = 0.2f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float fleeDistance = 8f;
    [SerializeField] private float fleeDuration = 1.5f;
    [SerializeField] private float disappearDistanceFromPlayer = 10f;

    [Header("Animation")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private float animationBlendSpeed = 8f;

    public event Action OnCreatureDisappeared;

    private ICreatureState currentState;
    private float animationSpeed;
    private float lightContactTimer;

    public float TensionLevel { get; set; }

    public Transform Player => player;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;
    public CreatureVoiceAudio VoiceAudio => creatureVoiceAudio;
    public CreatureLightSensor LightSensor => creatureLightSensor;
    public GeneratorPowerSystem GeneratorSystem => generatorSystem;
    public Transform BreakerBoxTransform => breakerBoxTransform;
    public GameObject Visuals => creatureVisuals;

    public float ChaseSpeed => chaseSpeed;
    public float KillDistance => killDistance;
    public float FleeSpeed => fleeSpeed;
    public float FleeDistance => fleeDistance;
    public float FleeDuration => fleeDuration;
    public float DisappearDistance => disappearDistanceFromPlayer;
    public float TensionIncreaseRate => tensionIncreaseRate;
    public float DirectHuntThreshold => directHuntThreshold;
    public float StalkDistance => stalkDistance;
    public bool LookAtPlayerWhenRevealed => lookAtPlayerWhenRevealed;
    public float LookAtPlayerSpeed => lookAtPlayerSpeed;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (creatureVoiceAudio == null) creatureVoiceAudio = GetComponent<CreatureVoiceAudio>();
        if (creatureLightSensor == null) creatureLightSensor = GetComponent<CreatureLightSensor>();
    }

    private void Start()
    {
        ChangeState(new CreatureHiddenState(this));
    }

    private void Update()
    {
        if (player == null) return;

        HandleLightReaction();
        currentState?.Update();
        UpdateAnimation();
    }

    public void ChangeState(ICreatureState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void SetLightReaction(bool state)
    {
        reactToLight = state;
        lightContactTimer = 0f;

        if (creatureLightSensor != null)
        {
            creatureLightSensor.ResetLightStatus();
        }
    }

    private void HandleLightReaction()
    {
        if (!reactToLight) return;
        if (creatureLightSensor == null) return;
        if (currentState is CreatureHiddenState) return;
        if (currentState is CreatureDisabledState) return;
        if (currentState is CreatureFleeState) return;

        if (creatureLightSensor.isInLight)
        {
            lightContactTimer += Time.deltaTime;
        }
        else
        {
            lightContactTimer = 0f;
        }

        if (lightContactTimer >= lightReactionDelay)
        {
            lightContactTimer = 0f;
            ChangeState(new CreatureFleeState(this));
        }
    }

    public void RevealAtPoint(Transform revealPoint)
    {
        if (currentState is CreatureDisabledState) return;
        if (revealPoint == null) return;

        currentRevealPoint = revealPoint;
        ChangeState(new CreatureRevealState(this, currentRevealPoint));
    }

    public void RevealAtCurrentPoint()
    {
        if (currentRevealPoint == null) return;
        RevealAtPoint(currentRevealPoint);
    }

    public void Disappear()
    {
        if (currentState is CreatureHiddenState) return;
        if (currentState is CreatureDisabledState) return;

        ChangeState(new CreatureHiddenState(this));
        OnCreatureDisappeared?.Invoke();
    }

    public void DisappearAndDisable()
    {
        if (currentState is CreatureDisabledState) return;

        ChangeState(new CreatureDisabledState(this));
        OnCreatureDisappeared?.Invoke();
    }

    public void StartChaseFromPoint(Transform chaseStartPoint)
    {
        if (currentState is CreatureDisabledState) return;

        if (chaseStartPoint != null)
        {
            WarpToPoint(chaseStartPoint);
        }

        ChangeState(new CreatureChaseState(this));
    }

    public void StartChaseFromCurrentPosition()
    {
        if (currentState is CreatureDisabledState) return;
        ChangeState(new CreatureChaseState(this));
    }

    public void StopChaseAndHide()
    {
        if (!(currentState is CreatureChaseState)) return;

        ChangeState(new CreatureHiddenState(this));
        OnCreatureDisappeared?.Invoke();
    }

    public void StartDynamicBehavior()
    {
        if (currentState is CreatureDisabledState) return;
        ChangeState(new CreatureDynamicState(this));
    }

    public void WarpToPoint(Transform point)
    {
        if (point == null) return;

        if (agent != null && agent.enabled)
        {
            agent.Warp(point.position);
        }
        else
        {
            transform.position = point.position;
        }

        transform.rotation = point.rotation;
    }

    public void SetVisibility(bool isVisible)
    {
        if (creatureVisuals != null) creatureVisuals.SetActive(isVisible);
        if (creatureCollider != null) creatureCollider.enabled = isVisible;
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null) return;

        float targetSpeed = agent.velocity.magnitude;

        animationSpeed = Mathf.Lerp(
            animationSpeed,
            targetSpeed,
            Time.deltaTime * animationBlendSpeed
        );

        animator.SetFloat(speedParameter, animationSpeed);
    }
}