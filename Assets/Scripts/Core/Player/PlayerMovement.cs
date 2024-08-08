using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    
    [Header("References")]
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private Transform bodyTransform;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private ParticleSystem dustCloud;


    [Header("Settings")]
    [SerializeField]
    private float maxMovementSpeed = 4f;

    [SerializeField]
    private float turningRate = 30f;

    [SerializeField]
    private float acceleration = 4f;

    [SerializeField]
    private float particleEmmissionValue = 10f;

    private ParticleSystem.EmissionModule emissionModule;

    private Vector2 previousMovementInput = new();

    private Vector3 prevPos;

    private float currentMovementSpeed = 0f;

    private const float ParticleStopThreshold = 0.005f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.MovementEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.MovementEvent -= HandleMove;
    }

    private void Awake()
    {
        emissionModule = dustCloud.emission;
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        bodyTransform.Rotate(0f, 0f, previousMovementInput.x * -turningRate * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if ((this.transform.position - prevPos).sqrMagnitude > ParticleStopThreshold)
        {
            emissionModule.rateOverTime = particleEmmissionValue;
        }
        else
        {
            emissionModule.rateOverTime = 0;
        }
        prevPos = this.transform.position;
        if (!IsOwner)
        {
            return;
        }
        if (previousMovementInput.y == 0f)
        {
            currentMovementSpeed = 0f;
        }
        else if (currentMovementSpeed < maxMovementSpeed)
        {
            currentMovementSpeed += Time.deltaTime * acceleration * previousMovementInput.y;
        }
        rb.velocity = currentMovementSpeed * bodyTransform.up;
    }

    private void HandleMove(Vector2 direction)
    {
        previousMovementInput = direction;
    }
}
