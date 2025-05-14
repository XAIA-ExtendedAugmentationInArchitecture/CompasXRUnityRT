using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CompasXR.Core.Data;
using CompasXR.Core;
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
        
        //TODO: ROBOT PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        Dictionary<string, object> robotRoot = DictionaryHelpers.ConvertObjectToDictionary(root);
        Dictionary<string, object> robotData = DictionaryHelpers.GetAsDictionary(robotRoot, "data");
        Dictionary<string, object> robotModel = DictionaryHelpers.GetAsDictionary(robotData, "model");

        if (robotModel == null)
        {
            Debug.LogError("No robot model found in JSON.");
            return;
        }

        Robot robot = Robot.FromData(robotModel, "UR20");
        Debug.Log($"Parsed robot: {robot.Name}");
        Debug.Log($"Parsed robot Joints Count: {robot.Joints.Count}");
        Debug.Log($"Parsed robot Links Count: {robot.Links.Count}");

        Link upperArmLink = robot.Links[3];
        GameObject upperArmLinkTest = upperArmLink.CreateLinkGameObjectFromRosSharp(upperArmLink);
        GameObject robotTestParent = new GameObject("RobotParentTEST");
        GameObject robotTest = Robot.CreateRobotAsGameObjectWithRosSharp(robot.Name, robot, robotTestParent);
        Debug.Log($"Robot upper arm link Data: {JsonConvert.SerializeObject(upperArmLink.GetData())}");


        //TODO: ROBOT GENERATION POTITION TESTING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // GameObject cubeTesting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // cubeTesting.GetComponent<Renderer>().material.color = Color.red;
        // cubeTesting.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // foreach(var link in robot.Links)
        // {
        //     Debug.Log($"Parsed Link From Robot Model : Joints Count :  {link.Name}: {link.Joints.Count}");
        //     Debug.Log($"Parsed Link From Robot Model : Visuals Count : {link.Name}: {link.Visual.Count}");
        //     // Debug.Log($"Parsed Link From Robot Model : Meshes Count : {link.Name}: {link.Visual[0].Geometry.Shape.Data.Meshes.Count}");

        //     if (link.Visual.Count <= 0)
        //     {
        //         Debug.LogError($"TEST INSTANTIATION : Link Visual is Null : {link.Name} see {JsonConvert.SerializeObject(link.Visual)} ");
        //         continue;

        //     }

        //     if (link.Visual.Count > 0)
        //     {
        //         foreach (var visual in link.Visual)
        //         {
        //             foreach (var mesh in visual.Geometry.Shape.Data.Meshes)
        //             {
        //                 mesh.GenerateMeshFromRHMesh(link.Name);
        //             }
        //         }
        //     }

        //     Frame Origin = link.Visual[0].Origin;
        //     Frame inertialOrigin = link.Inertial.Origin;

        //     if (inertialOrigin.point != null && inertialOrigin.xaxis != null && inertialOrigin.yaxis != null)
        //     {
        //         GameObject linkInertialGameObject = GameObject.Instantiate(cubeTesting);
        //         linkInertialGameObject.name = link.Name + "_Inertial";
        //         linkInertialGameObject.GetComponent<Renderer>().material.color = Color.blue;
        //         GameObject inertialObjectInstantation = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(linkInertialGameObject, inertialOrigin.point, inertialOrigin.xaxis, inertialOrigin.yaxis, false, false);
        //         Destroy(linkInertialGameObject);

        //     }
        //     else
        //     {
        //         Debug.LogError($"TEST INSTANTIATION : Link Inertial Origin is Null : {link.Name} see {JsonConvert.SerializeObject(inertialOrigin)} ");
        //     }
        //     if (Origin.point != null && Origin.xaxis != null && Origin.yaxis != null)
        //     {
        //         GameObject linkVisualGameObject = GameObject.Instantiate(cubeTesting);
        //         linkVisualGameObject.name = link.Name + "_Visual";
        //         linkVisualGameObject.GetComponent<Renderer>().material.color = Color.yellow;
        //         GameObject visualObjectInstantation = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(linkVisualGameObject, Origin.point, Origin.xaxis, Origin.yaxis, false, false);
        //         Destroy(linkVisualGameObject);
        //     }
        //     else
        //     {
        //         Debug.LogError($"TEST INSTANTIATION : Link Visual Origin is Null : {link.Name} see {JsonConvert.SerializeObject(Origin)} ");
        //     }
            
        //     if (link.Joints.Count > 0)
        //     {
        //         foreach (var joint in link.Joints)
        //         {
        //             Debug.Log($"TEST INSTANTIATION : Parsed Link From Robot Model : Instantiating Joints Count :  {link.Name}: {joint.Name}");
        //             if (joint.Origin.point != null && joint.Origin.xaxis != null && joint.Origin.yaxis != null)
        //             {
        //                 GameObject linkJointGameObject = GameObject.Instantiate(cubeTesting);
        //                 linkJointGameObject.name = link.Name + "_Joint_" + joint.Name;
        //                 linkJointGameObject.GetComponent<Renderer>().material.color = Color.green;
        //                 GameObject jointObjectInstantation = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(linkJointGameObject, joint.Origin.point, joint.Origin.xaxis, joint.Origin.yaxis, false, false);
        //                 Destroy(linkJointGameObject);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogError($"Link Joint Origin is Null : {link.Name} see {JsonConvert.SerializeObject(Origin)} ");
        //     }
        // }

        
        // //TODO: JOINT PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // JArray joints = root["data"]?["model"]?["joints"] as JArray;

        // if (joints == null)
        // {
        //     Debug.LogError("No joints found in JSON.");
        //     return;
        // }

        // List<RobotJoint> parsedJoints = new List<RobotJoint>();
        // foreach (var joint in joints)
        // {
        //     Dictionary<string, object> jointDict = DictionaryHelpers.ConvertObjectToDictionary(joint);

        //     RobotJoint parsedJoint = RobotJoint.FromData(jointDict);
        //     Debug.Log($"Parsed joint: {parsedJoint.Name}");
        //     Debug.Log($"Parsed joint type: {parsedJoint.Type}");
        //     parsedJoints.Add(parsedJoint);
        // }

        // Debug.Log($"Parsed Joints Count after: {parsedJoints.Count}");
        // //TODO: LINKS PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // JArray links = root["data"]?["model"]?["links"] as JArray;

        // if (links == null)
        // {
        //     Debug.LogError("No links found in JSON.");
        //     return;
        // }

        // List<Link> parsedLinks = new List<Link>();

        // foreach (var link in links)
        // {
        //     Dictionary<string, object> linkDict = DictionaryHelpers.ConvertObjectToDictionary(link);

        //     Link parsedLink = Link.FromData(linkDict);
        //     parsedLinks.Add(parsedLink);
        //     Debug.Log($"Parsed link: {parsedLink.Name}");
        //     Debug.Log($"Parsed link type: {parsedLink.Type}");
        //     Debug.Log($"Parsed link parsing Joints Count: {parsedLink.Joints.Count}");
        //     // Debug.Log($"Parsed link Joints: {JsonConvert.SerializeObject(parsedLink.joints)}");
        //     // Debug.Log($"Parsed link Collisions: {JsonConvert.SerializeObject(parsedLink.collision)}");
        // }

        // Debug.Log($"Parsed Links Count after: {parsedLinks.Count}");

        // List<string> jointNames = new List<string>();
        // foreach (var joint in parsedJoints)
        // {
        //     jointNames.Add(joint.Name);
        // }

        // List<string> linkNames = new List<string>();
        // foreach (var link in parsedLinks)
        // {
        //     linkNames.Add(link.Name);
        // }
        // Debug.Log($"Parsing Names : Joints Names {jointNames.Count}: {JsonConvert.SerializeObject(jointNames)}");
        // Debug.Log($"Parsing Names : Links Names {linkNames.Count}: {JsonConvert.SerializeObject(linkNames)}");  

        //TODO: MESH PARSING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // JArray linksagain = root["data"]?["model"]?["links"] as JArray;
        // if (linksagain == null)
        // {
        //     Debug.LogError("No links found in JSON.");
        //     return;
        // }

        // int meshCount = 0;

        // foreach (var link in linksagain)
        // {
        //     var collisions = link["collision"] as JArray;
        //     if (collisions == null) continue;

        //     foreach (var collision in collisions)
        //     {
        //         var meshes = collision["geometry"]?["shape"]?["data"]?["meshes"] as JArray;
        //         if (meshes == null) continue;

        //         foreach (var meshToken in meshes)
        //         {
        //             JObject meshObj = meshToken["data"] as JObject;
        //             if (meshObj == null)
        //             {
        //                 Debug.LogWarning("Mesh data was null.");
        //                 continue;
        //             }

        //             // Wrap it as a JSON string that matches Mesh.Parse expectations
        //             JObject meshWrapper = new JObject
        //             {
        //                 ["data"] = meshObj
        //             };

        //             string meshJson = meshWrapper.ToString(Formatting.None);

        //             try
        //             {
        //                 CompasMesh parsedMesh = CompasMesh.Parse(meshJson);
        //                 parsedMesh.GenerateMeshFromRHMeshDebug();
        //                 meshCount++;
        //             }
        //             catch (System.Exception ex)
        //             {
        //                 Debug.LogError($"Error parsing mesh {meshCount}: {ex.Message}");
        //             }
        //         }
        //     }
        // }

        // Debug.Log($"Total parsed and generated meshes: {meshCount}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
