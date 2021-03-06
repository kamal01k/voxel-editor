﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeGUI : GUIPanel
{
    public Texture background;
    public float backgroundScale;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(0, 0, width, height);
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;

        base.OnEnable();
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        Color baseColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.black;
        GUI.Box(panelRect, "");
        GUI.backgroundColor = baseColor;
        if (background != null)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("", GUIStyle.none,
                GUILayout.Width(background.width * backgroundScale),
                GUILayout.Height(background.height * backgroundScale));
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), background);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
    }
}