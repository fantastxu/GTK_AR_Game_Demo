// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheWorldBeyond.Environment.RoomEnvironment.Editor
{
    public class CyberpunkTextureGenerator : EditorWindow
    {
        private int textureSize = 512;
        private float circuitDensity = 0.3f;
        private float scanlineFrequency = 20f;
        private Color primaryColor = new Color(0f, 1f, 1f, 1f); // Cyan
        private Color secondaryColor = new Color(1f, 0f, 1f, 1f); // Magenta
        private string savePath = "Assets/TheWorldBeyond/Environment/RoomEnvironment/textures/CyberpunkPattern.png";

        [MenuItem("TheWorldBeyond/Generate Cyberpunk Texture")]
        public static void ShowWindow()
        {
            GetWindow<CyberpunkTextureGenerator>("Cyberpunk Texture Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Cyberpunk Texture Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 256, 2048);
            circuitDensity = EditorGUILayout.Slider("Circuit Density", circuitDensity, 0.1f, 0.8f);
            scanlineFrequency = EditorGUILayout.Slider("Scanline Frequency", scanlineFrequency, 5f, 50f);
            primaryColor = EditorGUILayout.ColorField("Primary Color (Cyan)", primaryColor);
            secondaryColor = EditorGUILayout.ColorField("Secondary Color (Magenta)", secondaryColor);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Save Path:", EditorStyles.miniBoldLabel);
            savePath = EditorGUILayout.TextField(savePath);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Circuit Board Texture", GUILayout.Height(30)))
            {
                GenerateCircuitBoardTexture();
            }

            if (GUILayout.Button("Generate Data Stream Texture", GUILayout.Height(30)))
            {
                GenerateDataStreamTexture();
            }

            if (GUILayout.Button("Generate Glitch Pattern Texture", GUILayout.Height(30)))
            {
                GenerateGlitchPatternTexture();
            }
        }

        private void GenerateCircuitBoardTexture()
        {
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float nx = (float)x / textureSize;
                    float ny = (float)y / textureSize;

                    // Circuit lines - horizontal and vertical
                    float horizontalLines = Mathf.PerlinNoise(nx * 20f, ny * 2f);
                    float verticalLines = Mathf.PerlinNoise(nx * 2f, ny * 20f);

                    // Grid pattern
                    float gridX = Mathf.Abs(Mathf.Sin(nx * textureSize * 0.5f));
                    float gridY = Mathf.Abs(Mathf.Sin(ny * textureSize * 0.5f));
                    float grid = (gridX < 0.1f || gridY < 0.1f) ? 1f : 0f;

                    // Circuit nodes
                    float nodes = Mathf.PerlinNoise(nx * 15f, ny * 15f);
                    nodes = nodes > (1f - circuitDensity) ? 1f : 0f;

                    // Combine patterns
                    float circuit = Mathf.Max(
                        horizontalLines > 0.7f ? 1f : 0f,
                        verticalLines > 0.7f ? 1f : 0f
                    );
                    circuit = Mathf.Max(circuit, grid * 0.5f);
                    circuit = Mathf.Max(circuit, nodes);

                    // Add noise
                    float noise = Mathf.PerlinNoise(nx * 50f, ny * 50f) * 0.2f;

                    // Color selection based on pattern
                    Color finalColor;
                    if (circuit > 0.5f)
                    {
                        finalColor = Color.Lerp(primaryColor, secondaryColor, nodes);
                    }
                    else
                    {
                        finalColor = new Color(0, 0, 0, 1);
                    }

                    finalColor.r += noise;
                    finalColor.g += noise;
                    finalColor.b += noise;

                    texture.SetPixel(x, y, finalColor);
                }
            }

            texture.Apply();
            SaveTexture(texture, savePath.Replace(".png", "_Circuit.png"));
        }

        private void GenerateDataStreamTexture()
        {
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float nx = (float)x / textureSize;
                    float ny = (float)y / textureSize;

                    // Vertical data streams
                    float streams = Mathf.PerlinNoise(nx * 30f, ny * 5f + Time.realtimeSinceStartup);
                    streams = Mathf.Pow(streams, 2f);

                    // Scanlines
                    float scanline = Mathf.Abs(Mathf.Sin(ny * scanlineFrequency));
                    scanline = scanline < 0.1f ? 1f : 0.3f;

                    // Digital noise
                    float digitalNoise = Mathf.PerlinNoise(nx * 100f, ny * 100f);
                    digitalNoise = digitalNoise > 0.8f ? 1f : 0f;

                    // Combine
                    float intensity = streams * scanline + digitalNoise * 0.5f;

                    Color finalColor = Color.Lerp(Color.black, primaryColor, intensity);
                    finalColor = Color.Lerp(finalColor, secondaryColor, digitalNoise * 0.3f);

                    texture.SetPixel(x, y, finalColor);
                }
            }

            texture.Apply();
            SaveTexture(texture, savePath.Replace(".png", "_DataStream.png"));
        }

        private void GenerateGlitchPatternTexture()
        {
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float nx = (float)x / textureSize;
                    float ny = (float)y / textureSize;

                    // Horizontal glitch bars
                    float glitchBars = Mathf.PerlinNoise(0, ny * 50f);
                    glitchBars = glitchBars > 0.7f ? 1f : 0f;

                    // RGB shift zones
                    float shiftZone = Mathf.PerlinNoise(nx * 10f, ny * 10f);

                    // Digital corruption
                    float corruption = Mathf.PerlinNoise(nx * 80f, ny * 80f);
                    corruption = corruption > 0.85f ? 1f : 0f;

                    // Combine
                    Color finalColor = Color.black;

                    if (glitchBars > 0.5f)
                    {
                        finalColor = primaryColor;
                    }

                    if (corruption > 0.5f)
                    {
                        finalColor = Color.Lerp(finalColor, secondaryColor, 0.7f);
                    }

                    // Add some base noise
                    float baseNoise = Mathf.PerlinNoise(nx * 40f, ny * 40f) * 0.3f;
                    finalColor.r += baseNoise;
                    finalColor.g += baseNoise;
                    finalColor.b += baseNoise;

                    // Store shift zone in alpha for shader use
                    finalColor.a = shiftZone;

                    texture.SetPixel(x, y, finalColor);
                }
            }

            texture.Apply();
            SaveTexture(texture, savePath.Replace(".png", "_Glitch.png"));
        }

        private void SaveTexture(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            // Set texture import settings
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = textureSize;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            Debug.Log($"Cyberpunk texture saved to: {path}");
            EditorUtility.DisplayDialog("Success", $"Texture generated successfully!\n{path}", "OK");
        }
    }
}
