// // ----------------------------------------------------------------------------
// // <copyright file="KinetixSquareMenuConfigurationEditor.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kinetix.UI.EmoteWheel.Internal
{
    [CustomEditor(typeof(KinetixInputMapSO))]
    public class KinetixInputManagerEditor : Editor
    {
        // CACHE
        [SerializeField] private Texture2D bannerTexture;

        // GLOBAL
        private SerializedProperty spKinetixActionMap;

        private bool bFallBackLoaded = false;

        KinetixInputMapSO kinetixInputFallback;
        KinetixInputMapSO kinetixInputHere;

        private void OnEnable()
        {
            if (!bannerTexture)
                bannerTexture = Resources.Load<Texture2D>("KinetixLogoBanner");

            spKinetixActionMap = serializedObject.FindProperty("kinetixActionMap");
            kinetixInputHere = target as KinetixInputMapSO;

            if ( !bFallBackLoaded )
                LoadFallBackActionMap();

            if (kinetixInputHere.kinetixActionMap == null)
                kinetixInputHere.kinetixActionMap = new InputActionMap();

            if(kinetixInputHere.kinetixActionMap.actions.Count==0)
                kinetixInputHere.kinetixActionMap = kinetixInputFallback.kinetixActionMap;
        }

        private void LoadFallBackActionMap()
        {
            kinetixInputFallback = Resources.Load<KinetixInputMapSO>("InputActionMap/DefaultKinetixInputActionMapSO");
            SerializedObject soKinetixInputFallBack = new SerializedObject(kinetixInputFallback);
            
            bFallBackLoaded = true;
        }

        public override void OnInspectorGUI()
        {
            
            serializedObject.Update();
            DrawBanner();

            DrawActionMap();

            DrawSeparator();
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();            
        }

        private void DrawActionMap()
        {
            DrawSeparator();
            GUILayout.Label(new GUIContent("CUSTOM INPUT ACTION MAP"), EditorStyles.boldLabel);

            DrawSeparator();

            EditorGUILayout.PropertyField(spKinetixActionMap, new GUIContent("Action Color"));
       }

        private void DrawBanner()
        {
            if (bannerTexture != null)
                EditorGUI.DrawTextureTransparent(
                    new Rect(Screen.width / 4.0f - bannerTexture.width / 2.0f, 10, bannerTexture.width,
                        bannerTexture.height), bannerTexture);

            EditorGUILayout.Space(bannerTexture.height + 5);
        }

        private void DrawSeparator()
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);
        }
    }
}
