﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverflowMenuGUI : GUIPanel
{
    private Texture worldIcon;
    private float buttonHeight;

    public override Rect GetRect(float width, float height)
    {
        buttonHeight = height * .12f;
        return new Rect(width - height * .4f, height * .13f, height * .4f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        stealFocus = false;
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
        {
            worldIcon = actionBar.worldIcon;
        }
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        if (MenuButton("World", worldIcon))
        {
            PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
            if (propsGUI != null)
            {
                propsGUI.worldSelected = true;
                propsGUI.normallyOpen = true;
            }
        }
    }

    private bool MenuButton(string name, Texture icon)
    {
        bool pressed = GUILayout.Button(name, GUILayout.Height(buttonHeight));
        Rect iconRect = GUILayoutUtility.GetLastRect();
        iconRect.width = iconRect.height;
        GUIStyle iconAlign = new GUIStyle(GUI.skin.label);
        iconAlign.alignment = TextAnchor.MiddleCenter;
        GUI.Label(iconRect, icon, iconAlign);
        if (pressed)
            Destroy(this);
        return pressed;
    }
}