using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class SRPFullParticleShaderEditor : ShaderGUI
{
    #region PROPERTIES_NAMES
    const string MAIN_TEX = "_MainTex";
    const string MAIN_COLOR = "_MainColor";
    const string VERT_COLOR_ON = "_UseVertexColor";
    const string POLAR_COORDINATES_ON = "_UsePolarCoordinates";

    const string R_ALPHA = "_RedIsAlpha";
    const string ALPHA_TRESHOLD = "_AlphaThreshold";
    const string ALPHA_SMOOTHNESS = "_AlphaSmoothness";
    const string CIRCLE_MASK_ON = "_CircleMask";
    const string OUTER_MASK = "_OuterMask";
    const string INNER_MASK = "_InnerMask";
    const string CIRCLE_MASK_SMOOTHNESS = "_CircleMaskSmoothness";

    const string SECONDARY_TEXTURE_ON = "_UseSecondaryTexture";
    const string SECONDARY_TEXTURE = "_SecondaryTexture";
    const string BLEND_METHOD = "_BlendMethod";
    const string BLENDING_FACTOR = "_BlendingFactor";

    const string BILLBOARD_ON = "_IsBillboard";
    const string CULLING = "_Culling";

    const string KEY_POLAR_COORDINATES = "_POLAR_COORDINATES";
    const string KEY_SECONDARY_TEXTURE = "_SECONDARY_TEXTURE";
    const string KEY_CIRCLE_MASK = "_CIRCLE_MASK";
    const string KEY_BILLBOARD = "_BILLBOARD";
    #endregion

    MaterialEditor materialEditor;
    MaterialProperty[] properties;

    static bool showMainProps, showSecondaryTexProps, showAlphaProps, showRenderingProps;
    Material targetMat;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.properties = properties;
        this.materialEditor = materialEditor;

        SetupProperties();
    }

    void SetupProperties()
    {
        targetMat = materialEditor.target as Material;
        EditorGUILayout.Space();
        CoreEditorUtils.DrawSplitter();

        showMainProps = CoreEditorUtils.DrawHeaderFoldout("Main Module", showMainProps, false, (Func<bool>)null, null);
        if (showMainProps)
            DrawMain();

        showSecondaryTexProps = CoreEditorUtils.DrawHeaderFoldout("Secondary Texture Module", showSecondaryTexProps, false, (Func<bool>)null, null);
        if (showSecondaryTexProps)
            DrawSecondaryTexture();

        showAlphaProps = CoreEditorUtils.DrawHeaderFoldout("Alpha Module", showAlphaProps, false, (Func<bool>)null, null);
        if (showAlphaProps)
            DrawAlpha();

        showRenderingProps = CoreEditorUtils.DrawHeaderFoldout("Rendering Module", showRenderingProps, false, (Func<bool>)null, null);
        if (showRenderingProps)
            DrawRendering();

    }

    private void DrawRendering()
    {
        MaterialProperty billboardOn = FindProperty(BILLBOARD_ON);
        MaterialProperty culling = FindProperty(CULLING);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            bool billboardState = targetMat.IsKeywordEnabled(KEY_BILLBOARD);
            EditorGUI.BeginChangeCheck();
            billboardState = EditorGUILayout.Toggle(billboardOn.displayName, billboardState);
            if (EditorGUI.EndChangeCheck())
            {
                targetMat.SetKeyword(KEY_BILLBOARD, billboardState);
            }
            materialEditor.RenderQueueField();
            materialEditor.DisplayProperty(culling);
        }
    }

    private void DrawAlpha()
    {
        MaterialProperty rAlpha = FindProperty(R_ALPHA);
        MaterialProperty circleMaskOn = FindProperty(CIRCLE_MASK_ON);
        MaterialProperty outerMask = FindProperty(OUTER_MASK);
        MaterialProperty innerMask = FindProperty(INNER_MASK);
        MaterialProperty circleMaskSmoothness = FindProperty(CIRCLE_MASK_SMOOTHNESS);
        MaterialProperty alphaBalance = FindProperty(ALPHA_TRESHOLD);
        MaterialProperty alphaContrast = FindProperty(ALPHA_SMOOTHNESS);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            bool rAlphaToggle = rAlpha.floatValue == 0 ? false : true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Toggle(rAlpha.displayName, rAlphaToggle);
            if (EditorGUI.EndChangeCheck())
            {
                rAlpha.floatValue = !rAlphaToggle == false ? 0 : 1;
            }
            materialEditor.DisplayProperty(alphaBalance);
            materialEditor.DisplayProperty(alphaContrast);

            bool circleMaskState = targetMat.IsKeywordEnabled(KEY_CIRCLE_MASK);
            EditorGUI.BeginChangeCheck();
            circleMaskState = EditorGUILayout.Toggle(circleMaskOn.displayName, circleMaskState);
            if (EditorGUI.EndChangeCheck())
            {
                targetMat.SetKeyword(KEY_CIRCLE_MASK, circleMaskState);
            }
            if (circleMaskState)
            {
                materialEditor.DisplayProperty(outerMask);
                materialEditor.DisplayProperty(innerMask);
                materialEditor.DisplayProperty(circleMaskSmoothness);
            }
        }
    }

    private void DrawSecondaryTexture()
    {

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            MaterialProperty secondaryTextureOn = FindProperty(SECONDARY_TEXTURE_ON);
            MaterialProperty secondaryTexture = FindProperty(SECONDARY_TEXTURE);
            MaterialProperty blendMethod = FindProperty(BLEND_METHOD);
            MaterialProperty blendFactor = FindProperty(BLENDING_FACTOR);

            bool secondaryTextureState = targetMat.IsKeywordEnabled(KEY_SECONDARY_TEXTURE);
            EditorGUI.BeginChangeCheck();
            secondaryTextureState = EditorGUILayout.Toggle(secondaryTextureOn.displayName, secondaryTextureState);
            if (EditorGUI.EndChangeCheck())
            {
                targetMat.SetKeyword(KEY_SECONDARY_TEXTURE, secondaryTextureState);
            }
            if (secondaryTextureState)
            {
                materialEditor.DisplayProperty(secondaryTexture);
                materialEditor.DisplayProperty(blendMethod);
                materialEditor.DisplayProperty(blendFactor);
            }
        }
    }

    private void DrawMain()
    {
        MaterialProperty mainTex = FindProperty(MAIN_TEX);
        MaterialProperty mainColor = FindProperty(MAIN_COLOR);
        MaterialProperty rAlpha = FindProperty(R_ALPHA);
        MaterialProperty polarCoordinatesOn = FindProperty(POLAR_COORDINATES_ON);
        MaterialProperty vertColorOn = FindProperty(VERT_COLOR_ON);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            materialEditor.DisplayTexture(mainTex, mainColor);
            materialEditor.TextureScaleOffsetProperty(mainTex);

            bool polarCoordinatesState = targetMat.IsKeywordEnabled(KEY_POLAR_COORDINATES);
            EditorGUI.BeginChangeCheck();
            polarCoordinatesState = EditorGUILayout.Toggle(polarCoordinatesOn.displayName, polarCoordinatesState);
            if (EditorGUI.EndChangeCheck())
            {
                targetMat.SetKeyword(KEY_POLAR_COORDINATES, polarCoordinatesState);
            }

            //bool rAlphaToggle = rAlpha.floatValue == 0 ? false : true;
            //EditorGUI.BeginChangeCheck();
            //EditorGUILayout.Toggle(rAlpha.displayName, rAlphaToggle);
            //if(EditorGUI.EndChangeCheck())
            //{
            //    rAlpha.floatValue = !rAlphaToggle == false ? 0 : 1;
            //}

            bool vertColorToggle = vertColorOn.floatValue == 0 ? false : true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Toggle(vertColorOn.displayName, vertColorToggle);
            if (EditorGUI.EndChangeCheck())
            {
                vertColorOn.floatValue = vertColorToggle == false ? 1 : 0;
            }
        }
    }

    MaterialProperty FindProperty(string propertyName)
    {
        return FindProperty(propertyName, properties);
    }
}
