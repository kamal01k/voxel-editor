﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ColorMode
{
    MATTE, GLOSSY, METAL, UNLIT, GLASS, ADD, MULTIPLY
}

public class MaterialSelectorGUI : GUIPanel
{
    private static Texture2D whiteTexture;
    private const int NUM_COLUMNS = 4;
    private const int TEXTURE_MARGIN = 20;
    private const float CATEGORY_BUTTON_ASPECT = 3.0f;
    private const string BACK_BUTTON = "Back";
    private const string PREVIEW_SUFFIX = "_preview";
    private const string PREVIEW_SUFFIX_EXT = PREVIEW_SUFFIX + ".mat";
    private static readonly ColorMode[] OPAQUE_COLOR_MODES = new ColorMode[]
    {
        ColorMode.MATTE, ColorMode.GLOSSY, ColorMode.METAL, ColorMode.UNLIT
    };
    private static readonly ColorMode[] TRANSPARENT_COLOR_MODES = new ColorMode[]
    {
        ColorMode.MATTE, ColorMode.GLASS, ColorMode.UNLIT, ColorMode.ADD, ColorMode.MULTIPLY
    };

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public string rootDirectory = "GameAssets/Materials";
    public bool onlyUnlit = false;
    public bool allowAlpha = false;
    public bool allowNullMaterial = false;
    public bool closeOnSelect = true;
    public Material highlightMaterial = null; // the current selected material

    private int tab;
    private string materialDirectory;
    private List<Material> materials;
    private List<string> materialSubDirectories;
    private ColorPickerGUI colorPicker;
    private ColorMode colorMode;

    private static readonly Lazy<GUIStyle> directoryButtonStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.button);
        style.padding.left = 16;
        style.padding.right = 16;
        return style;
    });

    public void Start()
    {
        if (onlyUnlit)
            colorMode = ColorMode.UNLIT;
        else
            colorMode = ColorMode.MATTE;

        materialDirectory = rootDirectory;
        UpdateMaterialDirectory();
        if (highlightMaterial != null && ResourcesDirectory.IsCustomMaterial(highlightMaterial))
        {
            highlightMaterial = Instantiate(highlightMaterial);
            tab = 0;
            colorMode = ResourcesDirectory.GetCustomMaterialColorMode(highlightMaterial);
        }
        else
            tab = 1;
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
    }

    public override void WindowGUI()
    {
        TutorialGUI.TutorialHighlight("material type");
        if (allowNullMaterial)
            tab = GUILayout.SelectionGrid(tab, new string[] { "Color", "Texture", "None" }, 3);
        else
            tab = GUILayout.SelectionGrid(tab, new string[] { "Color", "Texture" }, 2);
        TutorialGUI.ClearHighlight();

        if (tab == 0)
            ColorTab();
        else if (colorPicker != null)
            Destroy(colorPicker);
        if (tab == 1)
            TextureTab();
        else
        {
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;
            if (materialDirectory != rootDirectory)
            {
                materialDirectory = rootDirectory;
                UpdateMaterialDirectory();
            }
        }
        if (tab == 2)
            NoneTab();
    }

    private void ColorTab()
    {
        if (highlightMaterial == null || !ResourcesDirectory.IsCustomMaterial(highlightMaterial))
        {
            highlightMaterial = ResourcesDirectory.MakeCustomMaterial(colorMode, allowAlpha);
            if (allowAlpha)
                highlightMaterial.color = new Color(0, 0, 1, 0.25f);
            else
                highlightMaterial.color = Color.red;
            if (handler != null)
                handler(highlightMaterial);
        }
        ColorMode newMode;
        if (onlyUnlit)
        {
            newMode = ColorMode.UNLIT;
        }
        else if (allowAlpha)
        {
            int m = System.Array.IndexOf(TRANSPARENT_COLOR_MODES, colorMode);
            m = GUILayout.SelectionGrid(m,
                new string[] { "Matte", "Glass", "Unlit", "Add", "Multiply" },
                5, GUI.skin.GetStyle("button_tab"));
            newMode = TRANSPARENT_COLOR_MODES[m];
        }
        else
        {
            int m = System.Array.IndexOf(OPAQUE_COLOR_MODES, colorMode);
            m = GUILayout.SelectionGrid(m,
                new string[] { "Matte", "Glossy", "Metal", "Unlit" },
                4, GUI.skin.GetStyle("button_tab"));
            newMode = OPAQUE_COLOR_MODES[m];
        }
        if (newMode != colorMode)
        {
            Material newMat = ResourcesDirectory.MakeCustomMaterial(newMode, allowAlpha);
            newMat.color = highlightMaterial.color;
            highlightMaterial = newMat;
            colorMode = newMode;
            if (handler != null)
                handler(highlightMaterial);
        }
        if (colorPicker == null)
        {
            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            colorPicker.SetColor(highlightMaterial.color);
            colorPicker.includeAlpha = allowAlpha;
            colorPicker.handler = (Color c) =>
            {
                highlightMaterial.color = c;
                if (handler != null)
                    handler(highlightMaterial);
            };
        }
        colorPicker.WindowGUI();
    }

    private void TextureTab()
    {
        if (materials == null)
            return;
        scroll = GUILayout.BeginScrollView(scroll);
        Rect rowRect = new Rect();
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            if (i % NUM_COLUMNS == 0)
                rowRect = GUILayoutUtility.GetAspectRect(NUM_COLUMNS * CATEGORY_BUTTON_ASPECT);
            Rect buttonRect = rowRect;
            buttonRect.width = buttonRect.height * CATEGORY_BUTTON_ASPECT;
            buttonRect.x = buttonRect.width * (i % NUM_COLUMNS);
            string subDir = materialSubDirectories[i];
            bool selected;
            if (subDir == BACK_BUTTON)
                // highlight the button
                selected = !GUI.Toggle(buttonRect, true, subDir, directoryButtonStyle.Value);
            else
                selected = GUI.Button(buttonRect, subDir, directoryButtonStyle.Value);
            if (selected)
            {
                scroll = new Vector2(0, 0);
                MaterialDirectorySelected(materialSubDirectories[i]);
            }
        }
        for (int i = 0; i < materials.Count; i++)
        {
            if (i % NUM_COLUMNS == 0)
                rowRect = GUILayoutUtility.GetAspectRect(NUM_COLUMNS);
            Rect buttonRect = rowRect;
            buttonRect.width = buttonRect.height;
            buttonRect.x = buttonRect.width * (i % NUM_COLUMNS);
            Rect textureRect = new Rect(
                buttonRect.xMin + TEXTURE_MARGIN, buttonRect.yMin + TEXTURE_MARGIN,
                buttonRect.width - TEXTURE_MARGIN * 2, buttonRect.height - TEXTURE_MARGIN * 2);
            Material material = materials[i];
            bool selected;
            if (material == highlightMaterial)
                // highlight the button
                selected = !GUI.Toggle(buttonRect, true, "", GUI.skin.button);
            else
                selected = GUI.Button(buttonRect, "");
            if (selected)
                MaterialSelected(material);
            DrawMaterialTexture(material, textureRect, allowAlpha);
        }
        GUILayout.EndScrollView();
    }

    private void NoneTab()
    {
        if (highlightMaterial != null)
            MaterialSelected(null);
    }

    void UpdateMaterialDirectory()
    {
        materialSubDirectories = new List<string>();
        if (materialDirectory != rootDirectory)
            materialSubDirectories.Add(BACK_BUTTON);
        materials = new List<Material>();
        foreach (string dirEntry in ResourcesDirectory.dirList)
        {
            if (dirEntry.Length <= 2)
                continue;
            string newDirEntry = dirEntry.Substring(2);
            string fileName = Path.GetFileName(newDirEntry);
            string extension = Path.GetExtension(newDirEntry);
            string directory = Path.GetDirectoryName(newDirEntry);
            if (fileName.StartsWith("$"))
                continue; // special alternate materials for game
            if (directory != materialDirectory)
                continue;
            if (extension == "")
                materialSubDirectories.Add(fileName);
            else if (extension == ".mat")
            {
                if (fileName.EndsWith(PREVIEW_SUFFIX_EXT))
                    materials.RemoveAt(materials.Count - 1); // special preview material which replaces the previous
                materials.Add(ResourcesDirectory.GetMaterial(newDirEntry));
            }
        }

        Resources.UnloadUnusedAssets();
    }

    private void MaterialDirectorySelected(string name)
    {
        if (name == BACK_BUTTON)
        {
            if (materialDirectory.Trim() != "")
                materialDirectory = Path.GetDirectoryName(materialDirectory);
            UpdateMaterialDirectory();
            return;
        }
        else
        {
            if (materialDirectory.Trim() == "")
                materialDirectory = name;
            else
                materialDirectory += "/" + name;
            UpdateMaterialDirectory();
        }
    }

    private void MaterialSelected(Material material)
    {
        highlightMaterial = material;
        if (handler != null)
        {
            if (material != null && material.name.EndsWith(PREVIEW_SUFFIX))
            {
                string newPath = materialDirectory + "/"
                    + material.name.Substring(0, material.name.Length - PREVIEW_SUFFIX.Length);
                material = Resources.Load<Material>(newPath);
            }
            handler(material);
        }
        if (closeOnSelect)
            Destroy(this);
    }

    public static void DrawMaterialTexture(Material mat, Rect rect, bool alpha)
    {
        if (mat == null)
            return;
        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }
        Rect texCoords = new Rect(Vector2.zero, Vector2.one);
        Texture texture = whiteTexture;
        if (mat.HasProperty("_MainTex") && mat.mainTexture != null)
        {
            texture = mat.mainTexture;
            texCoords = new Rect(Vector2.zero, mat.mainTextureScale);
        }
        else if (mat.HasProperty("_ColorControl"))
            // water shader
            texture = mat.GetTexture("_ColorControl");
        else if (mat.HasProperty("_FrontTex"))
            // skybox
            texture = mat.GetTexture("_FrontTex");

        Color baseColor = GUI.color;
        if (mat.HasProperty("_Color"))
        {
            if (GUI.color.a > 1)
                // fixes transparent colors becoming opaque while scrolling
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);
            GUI.color *= mat.color;
        }
        else if (texture == whiteTexture)
        {
            // no color or texture
            texture = GUIIconSet.instance.missingTexture;
            alpha = true;
        }
        GUI.DrawTextureWithTexCoords(rect, texture, texCoords, alpha);
        GUI.color = baseColor;
    }
}
