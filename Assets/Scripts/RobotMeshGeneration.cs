using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CompasXR.Core.Data;
using CompasXR.Robots.Model;
using Google.MiniJSON;
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
        
        //TODO: JOINT PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        
        JArray joints = root["data"]?["model"]?["joints"] as JArray;

        if (joints == null)
        {
            Debug.LogError("No joints found in JSON.");
            return;
        }

        foreach (var joint in joints)
        {
            Dictionary<string, object> jointDict = DictionaryHelpers.ConvertObjectToDictionary(joint);

            RobotJoint parsedJoint = RobotJoint.FromData(jointDict);
            Debug.Log($"Parsed joint: {parsedJoint.Name}");
            Debug.Log($"Parsed joint type: {parsedJoint.Type}");
        }

        //TODO: LINKS PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        JArray links = root["data"]?["model"]?["links"] as JArray;

        if (links == null)
        {
            Debug.LogError("No links found in JSON.");
            return;
        }

        List<Link> parsedLinks = new List<Link>();

        foreach (var link in links)
        {
            Dictionary<string, object> linkDict = DictionaryHelpers.ConvertObjectToDictionary(link);

            Link parsedLink = Link.FromData(linkDict);
            parsedLinks.Add(parsedLink);
            Debug.Log($"Parsed link: {parsedLink.name}");
            Debug.Log($"Parsed link type: {parsedLink.type}");
            // Debug.Log($"Parsed link Joints: {JsonConvert.SerializeObject(parsedLink.joints)}");
            // Debug.Log($"Parsed link Collisions: {JsonConvert.SerializeObject(parsedLink.collision)}");
        }

        //TODO: MESH PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        JArray linksagain = root["data"]?["model"]?["links"] as JArray;
        if (linksagain == null)
        {
            Debug.LogError("No links found in JSON.");
            return;
        }

        int meshCount = 0;

        foreach (var link in linksagain)
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
