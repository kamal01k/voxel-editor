﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Substance", "An entity made of blocks", "cube-outline", typeof(Substance));

    public HashSet<Voxel> voxels;

    private Color _highlight = Color.clear;
    public Material highlightMaterial;
    public Color highlight
    {
        get
        {
            return _highlight;
        }
        set
        {
            _highlight = value;
            if (highlightMaterial == null)
                highlightMaterial = Material.Instantiate(Voxel.highlightMaterial);
            highlightMaterial.color = _highlight;
        }
    }
    private Color oldHighlight = Color.black;
    private bool willUpdateHighlight;
    public VoxelFace defaultPaint;

    public Substance()
    {
        voxels = new HashSet<Voxel>();
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override EntityComponent InitEntityGameObject(VoxelArray voxelArray, bool storeComponent = true)
    {
        GameObject substanceObject = new GameObject();
        substanceObject.name = "Substance";
        substanceObject.transform.parent = voxelArray.transform;
        substanceObject.transform.position = PositionInEditor();
        foreach (Voxel voxel in voxels)
        {
            if (storeComponent)
            {
                voxel.transform.parent = substanceObject.transform;
            }
            else
            {
                // clone
                Voxel vClone = voxel.Clone();
                vClone.transform.parent = substanceObject.transform;
                vClone.transform.position = voxel.transform.position;
                vClone.transform.rotation = voxel.transform.rotation;
            }
        }
        SubstanceComponent component = substanceObject.AddComponent<SubstanceComponent>();
        component.entity = this;
        component.substance = this;
        component.health = health;
        if (storeComponent)
            this.component = component;
        return component;
    }

    public override bool AliveInEditor()
    {
        return voxels.Count != 0;
    }

    // called by voxel
    public void AddVoxel(Voxel v)
    {
        voxels.Add(v);
    }

    public void RemoveVoxel(Voxel v)
    {
        voxels.Remove(v);
    }

    public override void UpdateEntityEditor()
    {
        base.UpdateEntityEditor();
        foreach (Voxel v in voxels)
            v.UpdateVoxel();
    }

    public override Vector3 PositionInEditor()
    {
        Bounds voxelBounds = new Bounds();
        foreach (Voxel voxel in voxels)
        {
            if (voxelBounds.extents == Vector3.zero)
                voxelBounds = voxel.GetBounds();
            else
                voxelBounds.Encapsulate(voxel.GetBounds());
        }
        return voxelBounds.center;
    }

    public void UpdateHighlight()
    {
        // EntityReferencePropertyManager has a pattern of calling UpdateHighlight twice per frame,
        // once to set the color to Clear and once to set it back to the highlight.
        // This is designed to prevent regenerating the mesh more often than necessary.
        if (!willUpdateHighlight)
        {
            foreach (Voxel v in voxels)
            {
                v.StartCoroutine(UpdateHighlightCoroutine());
                break;
            }
            willUpdateHighlight = true;
        }
    }

    private IEnumerator UpdateHighlightCoroutine()
    {
        yield return null;
        if (oldHighlight != highlight)
        {
            foreach (Voxel v in voxels)
                v.UpdateVoxel();
            oldHighlight = highlight;
        }
        willUpdateHighlight = false;
    }
}

public class SubstanceComponent : DynamicEntityComponent
{
    public Substance substance;

    public override void Start()
    {
        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        // no physics by default (could be disabled by a Physics behavior)
        rigidBody.isKinematic = true;

        base.Start();
    }
}
