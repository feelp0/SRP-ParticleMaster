using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public static class ShaderGUIExtensions
{
    static MaterialEditor materialEditor;
    static MaterialProperty materialProperty;
    static MaterialProperty[] materialProperties;
    static GUIContent staticLabel = new GUIContent();
    static Material mat;

    /// <summary>
    /// Display the color property with the default name given on the shader
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="property"></param>
    public static void DisplayColor( this MaterialEditor editor, MaterialProperty property )
    {
        editor.ColorProperty( property, property.displayName );
    }

    /// <summary>
    /// Display the shader property with the default name given on the shader
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="property"></param>
    public static void DisplayProperty( this MaterialEditor editor, MaterialProperty property )
    {
        editor.ShaderProperty( property, property.displayName );
    }

    /// <summary>
    /// Display the shader texture with the default name given on the shader
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="property"></param>
    /// <param name="tooltip"></param>
    public static void DisplayTexture( this MaterialEditor editor, MaterialProperty property, string tooltip = null )
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        editor.TexturePropertySingleLine( staticLabel, property );
    }

    /// <summary>
    /// Display the shader texture with the default name given on the shader
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="property"></param>
    /// <param name="tooltip"></param>
    public static void DisplayTexture( this MaterialEditor editor, MaterialProperty property, MaterialProperty extProp0, string tooltip = null )
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        editor.TexturePropertySingleLine( staticLabel, property, extProp0 );
    }

    /// <summary>
    /// Display the shader texture with the default name given on the shader
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="property"></param>
    /// <param name="tooltip"></param>
    public static void DisplayTexture( this MaterialEditor editor, MaterialProperty property, MaterialProperty extProp0, MaterialProperty extProp1, string tooltip = null )
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        editor.TexturePropertySingleLine( staticLabel, property, extProp0, extProp1 );
    }
    
    /// <summary>
    /// Return a label object with the property default name given on the shader
    /// </summary>
    /// <param name="property"></param>
    /// <param name="tooltip"></param>
    /// <returns></returns>
    public static GUIContent MakeLabel(this MaterialProperty property, string tooltip = null)
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    /// <summary>
    /// Returns a label with the given name and tooltip
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tooltip"></param>
    /// <returns></returns>
    public static GUIContent MakeLabel(string name, string tooltip = null )
    {
        staticLabel.text = name;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    /// <summary>
    /// Sets a keyword state for the material
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="keyword"></param>
    /// <param name="state"></param>
    public static void SetKeyword(this Material mat, string keyword, bool state)
    {
        if( state )
        {
            mat.EnableKeyword( keyword );
        }
        else
        {
            mat.DisableKeyword( keyword );
        }
    }

    /// <summary>
    /// Create a toggle in the GUI that sets a keyword on the material
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="label"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public static bool KeywordToggle(this Material mat, string label, string keyword)
    {
        bool val = mat.IsKeywordEnabled( keyword );
        EditorGUI.BeginChangeCheck();
        val = EditorGUILayout.Toggle( label, val );
        if( EditorGUI.EndChangeCheck() )
        {
            mat.SetKeyword( keyword, val );
        }
        return val;
    }

    public static bool KeywordToggleUndo( MaterialEditor matEditor, Material mat, string label, string keyword )
    {
        bool val = mat.IsKeywordEnabled( keyword );
        EditorGUI.BeginChangeCheck();
        val = EditorGUILayout.Toggle( label, val );
        if( EditorGUI.EndChangeCheck() )
        {
            RecordAction( matEditor, label );
            mat.SetKeyword( keyword, val );
        }
        return val;
    }

    static void RecordAction(MaterialEditor matEditor, string label )
    {
        matEditor.RegisterPropertyChangeUndo( label );
    }
}

