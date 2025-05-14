using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CompasXR.Core.Data;
using System;

namespace CompasXR.Robots.Model
{
    [System.Serializable]
    public class Robot
    {
        public string Name;

        // public List<Manipulator> AttachedTools = new List<Manipulator>();
        public Dictionary<string, object> Attributes = new Dictionary<string, object>();
        public GameObject UnityObject;
        public List<RobotJoint> Joints = new List<RobotJoint>();
        public List<Link> Links = new List<Link>();
        public Semantics Semantics = new Semantics();

        public Robot(string name = "Robot",
                    Dictionary<string, object> attributes = null,
                    GameObject unityObject = null,
                    List<RobotJoint> joints = null,
                    List<Link> links = null,
                    Semantics semantics = null)
        {
            Name = name;
            Attributes = attributes ?? new Dictionary<string, object>();
            UnityObject = unityObject ?? new GameObject(name);
            Joints = joints ?? new List<RobotJoint>();
            Links = links ?? new List<Link>();
            Semantics = semantics ?? new Semantics();
        }

        public static Robot Parse(object jsonData, string name="Robot")
        {
            var data = jsonData as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Mass.Parse");
            return FromData(data, name);
        }    
        public static Robot FromData(Dictionary<string, object> data, string name)
        {
            string robotName = name;
            Dictionary<string, object> attributes = DictionaryHelpers.GetAsDictionary(data, "attr");

            List<Dictionary<string, object>> robotJointData = DictionaryHelpers.GetListFromDict(data, "joints");    
            List<RobotJoint> robotJoints = new List<RobotJoint>();
            foreach (var jointData in robotJointData)
            {
                RobotJoint joint = RobotJoint.FromData(jointData);
                robotJoints.Add(joint);
            }

            List<Dictionary<string, object>> robotData = DictionaryHelpers.GetListFromDict(data, "links");    
            List<Link> robotLinkData = new List<Link>();
            foreach (var linkData in robotData)
            {
                Link link = Link.FromData(linkData);
                robotLinkData.Add(link);
            }

            if(robotJoints.Count == 0)
            {
                Debug.LogError("Parse Robot : No joints found in JSON.");
                return null;
            }
            if(robotLinkData.Count == 0)
            {
                Debug.LogError("Parse Robot : No links found in JSON.");
                return null;
            }

            Robot robot = new Robot
            {
                Name = robotName,
                Attributes = attributes ?? new Dictionary<string, object>(),
                Joints = robotJoints,
                Links = robotLinkData,
                Semantics = new Semantics()
            };

            return robot;
        }
        public static GameObject CreateRobotAsGameObjectWithRosSharp(string name, Robot roobot, GameObject parent)
        {
            GameObject robot = new GameObject(name);
            robot.transform.SetParent(parent.transform);
            robot.transform.localPosition = Vector3.zero;
            robot.transform.localRotation = Quaternion.identity;

            List<GameObject> generatedLinkObjects = new List<GameObject>();
            foreach (var link in roobot.Links)
            {
                GameObject linkObject = link.CreateLinkGameObjectFromRosSharp(link);

                if(generatedLinkObjects.Count > 0)
                {
                    GameObject previousLinkObject = generatedLinkObjects[generatedLinkObjects.Count - 1];
                    linkObject.transform.SetParent(previousLinkObject.transform);
                }
                else
                {
                    linkObject.transform.SetParent(robot.transform);
                }
                generatedLinkObjects.Add(linkObject);

            }

            return robot;
        }
    }

    [System.Serializable]
    public class Model
    {
    }

    // [System.Serializable]
    // public class Joint
    // {
    // }

    // [System.Serializable]
    // public class Link
    // {

    // }

    [System.Serializable]
    public class Semantics
    {
    }


    [System.Serializable]
    public class RobotModel
    {

    }
}
