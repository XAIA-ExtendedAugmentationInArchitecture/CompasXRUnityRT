using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CompasXR.Core.Data;
public class RobotMeshGeneration : MonoBehaviour
{

    public string filePath = @"C:\Users\jk6372\Desktop\00_princeton_projects\00_robotic_territories\00_git\CompasXRUnityRT\Assets\Scripts\jsontesting\robot_data.json";

    void Start()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        string jsonText = File.ReadAllText(filePath);
        JObject root = JObject.Parse(jsonText);
        JArray links = root["data"]?["model"]?["links"] as JArray;

        if (links == null)
        {
            Debug.LogError("No links found in JSON.");
            return;
        }

        int meshCount = 0;

        foreach (var link in links)
        {
            var collisions = link["collision"] as JArray;
            if (collisions == null) continue;

            foreach (var collision in collisions)
            {
                var meshes = collision["geometry"]?["shape"]?["data"]?["meshes"] as JArray;
                if (meshes == null) continue;

                foreach (var meshToken in meshes)
                {
                    JObject meshObj = meshToken["data"] as JObject;
                    if (meshObj == null)
                    {
                        Debug.LogWarning("Mesh data was null.");
                        continue;
                    }

                    // Wrap it as a JSON string that matches Mesh.Parse expectations
                    JObject meshWrapper = new JObject
                    {
                        ["data"] = meshObj
                    };

                    string meshJson = meshWrapper.ToString(Formatting.None);

                    try
                    {
                        CompasMesh parsedMesh = CompasMesh.Parse(meshJson);
                        parsedMesh.GenerateMeshFromRHMeshDebug();
                        meshCount++;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error parsing mesh {meshCount}: {ex.Message}");
                    }
                }
            }
        }

        Debug.Log($"Total parsed and generated meshes: {meshCount}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
