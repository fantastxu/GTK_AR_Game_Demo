// Copyright (c) Meta Platforms, Inc. and affiliates.
// Editor script to setup Matrix material for VirtualRoom scanning effect

using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheWorldBeyond.Environment.RoomEnvironment.Editor
{
    public class MatrixMaterialSetup : EditorWindow
    {
        [MenuItem("TheWorldBeyond/Setup Matrix Room Material")]
        public static void ShowWindow()
        {
            GetWindow<MatrixMaterialSetup>("Matrix Material Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Matrix Room Material Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will:\n" +
                "1. Create OppyDimensionMatrixMaterial.mat\n" +
                "2. Configure it with Matrix textures\n" +
                "3. Update ScenePlaneMRUK and SceneVolumeMRUK prefabs",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Create Matrix Material", GUILayout.Height(40)))
            {
                CreateMatrixMaterial();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Apply to Prefabs", GUILayout.Height(40)))
            {
                ApplyToPrefabs();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Do Both (Create & Apply)", GUILayout.Height(50)))
            {
                CreateMatrixMaterial();
                ApplyToPrefabs();
            }
        }

        private static void CreateMatrixMaterial()
        {
            // Find the shader
            Shader matrixShader = Shader.Find("TheWorldBeyond/OppyDimensionMatrix");
            if (matrixShader == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Could not find shader 'TheWorldBeyond/OppyDimensionMatrix'.\n" +
                    "Make sure OppyDimensionMatrix.shader exists and compiles correctly.",
                    "OK");
                return;
            }

            // Create material
            Material matrixMaterial = new Material(matrixShader);
            matrixMaterial.name = "OppyDimensionMatrixMaterial";

            // Load textures
            Texture2D matrixTex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Matrix_effect/textures/matrix_2.png");
            Texture2D maskTex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Matrix_effect/textures/mask_0.png");

            // Load from original OppyDimensionMaterial
            Material originalMat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/TheWorldBeyond/Environment/RoomEnvironment/materials/OppyDimensionMaterial.mat");

            if (originalMat != null)
            {
                // Copy fog and lighting settings
                if (originalMat.HasProperty("_FogCubemap"))
                    matrixMaterial.SetTexture("_FogCubemap", originalMat.GetTexture("_FogCubemap"));
                if (originalMat.HasProperty("_LightingRamp"))
                    matrixMaterial.SetTexture("_LightingRamp", originalMat.GetTexture("_LightingRamp"));
                if (originalMat.HasProperty("_MainTex"))
                    matrixMaterial.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));

                matrixMaterial.SetFloat("_FogStrength", 0.5f);
                matrixMaterial.SetFloat("_FogStartDistance", 0);
                matrixMaterial.SetFloat("_FogEndDistance", 6);
                matrixMaterial.SetFloat("_FogExponent", 0.16f);
                matrixMaterial.SetFloat("_TriPlanarFalloff", 7.4f);
            }

            // Set Matrix specific properties
            if (matrixTex != null)
            {
                matrixMaterial.SetTexture("_MatrixTex", matrixTex);
                Debug.Log("Matrix texture assigned: matrix_2.png");
            }
            else
            {
                Debug.LogWarning("Could not find matrix_2.png texture");
            }

            if (maskTex != null)
            {
                matrixMaterial.SetTexture("_MaskTex", maskTex);
                Debug.Log("Mask texture assigned: mask_0.png");
            }
            else
            {
                Debug.LogWarning("Could not find mask_0.png texture");
            }

            // Matrix effect parameters
            matrixMaterial.SetColor("_MatrixColor", new Color(0f, 1f, 0.2f, 1f)); // Green
            matrixMaterial.SetFloat("_MatrixSpeed", 1.5f);
            matrixMaterial.SetFloat("_MatrixSpeedX", 0f);
            matrixMaterial.SetVector("_MatrixTiling", new Vector4(3f, 3f, 0f, 0f));
            matrixMaterial.SetFloat("_MatrixEmission", 3f);
            matrixMaterial.SetVector("_MaskTiling", new Vector4(1.5f, 1.5f, 0f, 0f));
            matrixMaterial.SetFloat("_MaskOffsetMultiplier", 2f);

            // General parameters
            matrixMaterial.SetFloat("_SaturationAmount", 1f);
            matrixMaterial.SetColor("_Color", Color.black);
            matrixMaterial.SetFloat("_EffectTimer", 1f);
            matrixMaterial.SetFloat("_InvertedMask", 1f);
            matrixMaterial.SetVector("_OppyPosition", new Vector4(0, 1000, 0, 0));
            matrixMaterial.SetVector("_EffectPosition", new Vector4(0, 1000, 0, 1));

            // Save material
            string materialPath = "Assets/TheWorldBeyond/Environment/RoomEnvironment/materials/OppyDimensionMatrixMaterial.mat";

            // Ensure directory exists
            string directory = Path.GetDirectoryName(materialPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(matrixMaterial, materialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                $"Matrix material created at:\n{materialPath}",
                "OK");

            // Select the created material
            Selection.activeObject = matrixMaterial;
            EditorGUIUtility.PingObject(matrixMaterial);
        }

        private static void ApplyToPrefabs()
        {
            // Load the matrix material
            Material matrixMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/TheWorldBeyond/Environment/RoomEnvironment/materials/OppyDimensionMatrixMaterial.mat");

            if (matrixMaterial == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Could not find OppyDimensionMatrixMaterial.mat.\n" +
                    "Please create the material first.",
                    "OK");
                return;
            }

            int updated = 0;

            // Update ScenePlaneMRUK prefab
            string planePath = "Assets/TheWorldBeyond/Environment/RoomEnvironment/prefabs/ScenePlaneMRUK.prefab";
            GameObject planePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(planePath);
            if (planePrefab != null)
            {
                UpdatePrefabMaterial(planePrefab, planePath, matrixMaterial);
                updated++;
            }

            // Update SceneVolumeMRUK prefab
            string volumePath = "Assets/TheWorldBeyond/Environment/RoomEnvironment/prefabs/SceneVolumeMRUK.prefab";
            GameObject volumePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(volumePath);
            if (volumePrefab != null)
            {
                UpdatePrefabMaterial(volumePrefab, volumePath, matrixMaterial);
                updated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Complete",
                $"Updated {updated} prefab(s) with Matrix material.",
                "OK");
        }

        private static void UpdatePrefabMaterial(GameObject prefab, string prefabPath, Material material)
        {
            // Open prefab for editing
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            // Find PassthroughMesh and update material
            MeshRenderer[] renderers = prefabRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.gameObject.name == "PassthroughMesh")
                {
                    renderer.sharedMaterial = material;
                    Debug.Log($"Updated material on {prefabPath} -> PassthroughMesh");
                }
            }

            // Find WorldBeyondRoomObject and update DarkRoomMaterial
            var roomObjects = prefabRoot.GetComponentsInChildren<WorldBeyondRoomObject>(true);
            foreach (var roomObj in roomObjects)
            {
                roomObj.DarkRoomMaterial = material;
                Debug.Log($"Updated DarkRoomMaterial on {prefabPath}");
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
