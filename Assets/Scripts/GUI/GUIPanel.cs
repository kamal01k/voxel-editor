﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIPanel : MonoBehaviour {

    public const float targetHeight = 360;

    static GUISkin globalGUISkin = null;
    public GUISkin guiSkin;

    static List<GUIPanel> openPanels = new List<GUIPanel>();

    public Rect panelRect;
    public Vector2 scroll = new Vector2(0, 0);
    public float scaleFactor;
    public float scaledScreenWidth;
    public int depth = 0;

    private bool touchMoved = false;

    public void OnEnable()
    {
        openPanels.Add(this);
    }

    public void OnDisable()
    {
        openPanels.Remove(this);
    }

    public void OnGUI()
    {
        if (globalGUISkin == null)
            globalGUISkin = guiSkin;

        GUI.skin = globalGUISkin;
        GUI.depth = 1;
        GUI.enabled = true;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
                touchMoved = true;
            if (touchMoved && PanelContainsPoint(touch.position))
                GUI.enabled = false;
        }
        else
        {
            touchMoved = false;
        }

        scaleFactor = Screen.height / targetHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));
        scaledScreenWidth = Screen.width / scaleFactor;
    }

    public bool PanelContainsPoint(Vector2 point)
    {
        point.y = Screen.height - point.y;
        point /= scaleFactor;
        return panelRect.Contains(point);
    }

    public static GUIPanel PanelContainingPoint(Vector2 point)
    {
        if (openPanels.Count == 0)
            return null;

        GUIPanel match = null;
        foreach (GUIPanel panel in openPanels)
        {
            if (panel.PanelContainsPoint(point))
            {
                if (match == null || panel.depth < match.depth)
                    match = panel;
            }
        }
        return match;
    }
}
