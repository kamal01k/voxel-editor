﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceBehavior : EntityBehavior
{
    public enum ForceBehaviorMode
    {
        IMPULSE, CONTINUOUS
    }

    public static new BehaviorType objectType = new BehaviorType(
        "Force", "An instant or continuous force toward a target",
        "Only works for objects with the Physics behavior.\n"
        + "\"Impulse\" mode will cause the force to applied once the instant the behavior is enabled.\n"
        + "\"Continuous\" mode will cause the force to be continuously applied while the behavior is enabled.\n"
        + "If \"Ignore mass\" is enabled, the force will be scaled to compensate for the mass of the object.",
        "rocket", typeof(ForceBehavior));

    private ForceBehaviorMode mode = ForceBehaviorMode.CONTINUOUS;
    private bool ignoreMass = false;
    private float strength = 10;
    private Target target = new Target(3); // up

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Mode",
                () => mode,
                v => mode = (ForceBehaviorMode)v,
                PropertyGUIs.Enum),
            new Property("Ignore mass?",
                () => ignoreMass,
                v => ignoreMass = (bool)v,
                PropertyGUIs.Toggle),
            new Property("Strength",
                () => strength,
                v => strength = (float)v,
                PropertyGUIs.Float),
            new Property("Toward",
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Target)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var force = gameObject.AddComponent<ForceComponent>();
        if (mode == ForceBehaviorMode.IMPULSE)
        {
            if (ignoreMass)
                force.forceMode = ForceMode.VelocityChange;
            else
                force.forceMode = ForceMode.Impulse;
        }
        else if (mode == ForceBehaviorMode.CONTINUOUS)
        {
            if (ignoreMass)
                force.forceMode = ForceMode.Acceleration;
            else
                force.forceMode = ForceMode.Force;
        }
        force.strength = strength;
        force.target = target;
        return force;
    }
}

public class ForceComponent : BehaviorComponent
{
    public ForceMode forceMode;
    public float strength;
    public Target target;

    private Rigidbody rigidBody;
    private NewRigidbodyController player;

    public override void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        player = GetComponent<NewRigidbodyController>();
        base.Start();
    }

    public override void BehaviorEnabled()
    {
        if ((forceMode == ForceMode.Impulse || forceMode == ForceMode.VelocityChange) && rigidBody != null)
        {
            rigidBody.AddForce(target.DirectionFrom(transform.position) * strength, forceMode);
            if (player != null)
                player.disableGroundCheck = true;
        }
    }

    void FixedUpdate()
    {
        if ((forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration) && rigidBody != null)
        {
            rigidBody.AddForce(target.DirectionFrom(transform.position) * strength, forceMode);
            if (player != null)
                player.disableGroundCheck = true;
        }
    }
}