using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json;

using UnityEngine;

namespace CompasXR.Robots.Data
{

    public class Trajectory
    {
        public List<Configuration> Configurations { get; set; }
        // public List<AttachedCollisionMeshes> AttachedCollisionMeshes { get; set; }
        public List<string> JointNames { get; set; }
        public Configuration StartConfiguration { get; set; }
        public float PlanningTime { get; set; }
        public float Fraction { get; set; }

        //TODO: ADD THE REST OF THE SHIT.....
    }

    public class Configuration
    {
        public List<float> JointValues { get; set; }
        public List<float> Accelerations { get; set; }
        public List<float> Velocities { get; set; }
        public List<string> JointNames { get; set; }
        public List<float> Effort { get; set; }
        public List<int> JointTypes { get; set; } //TODO: Does this make sense to make an ENUM?
        public Configuration(
            List<float> jointValues, 
            List<string> jointNames, 
            List<float> accelerations = null, 
            List<float> velocities = null, 
            List<float> effort = null, 
            List<int> jointTypes = null)
        {
            JointValues = jointValues ?? throw new ArgumentNullException(nameof(jointValues));
            JointNames = jointNames ?? throw new ArgumentNullException(nameof(jointNames));

            // Ensuring optional lists default to empty lists instead of remaining null
            Accelerations = accelerations ?? new List<float>();
            Velocities = velocities ?? new List<float>();
            Effort = effort ?? new List<float>();
            JointTypes = jointTypes ?? new List<int>();
        }
        public Dictionary<string, float> GetJointDict()
        {
            Dictionary<string, float> jointDict = new Dictionary<string, float>();

            if (JointNames.Count != JointValues.Count)
            {
                throw new InvalidOperationException("JointNames and JointValues lists must have the same length.");
            }

            for (int i = 0; i < JointNames.Count; i++)
            {
                jointDict[JointNames[i]] = JointValues[i];
            }

            return jointDict;
        }

        public static Configuration Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }

        public static Configuration FromData(Dictionary<string, object> jsonDataDict)
        {
            List<float> jointValues = ((List<object>)jsonDataDict["joint_values"]).Select(Convert.ToSingle).ToList();
            List<string> jointNames = ((List<object>)jsonDataDict["joint_names"]).Select(obj => obj.ToString()).ToList();

            Configuration configuration = new Configuration(jointValues, jointNames);

            if (jsonDataDict.ContainsKey("accelerations"))
            {
                configuration.Accelerations = ((List<object>)jsonDataDict["accelerations"]).Select(Convert.ToSingle).ToList();
            }
            if (jsonDataDict.ContainsKey("velocities"))
            {
                configuration.Velocities = ((List<object>)jsonDataDict["velocities"]).Select(Convert.ToSingle).ToList();
            }
            if (jsonDataDict.ContainsKey("effort"))
            {
                configuration.Effort = ((List<object>)jsonDataDict["effort"]).Select(Convert.ToSingle).ToList();
            }
            if (jsonDataDict.ContainsKey("joint_types"))
            {
                configuration.JointTypes = ((List<object>)jsonDataDict["joint_types"]).Select(Convert.ToInt32).ToList();
            }

            return configuration;
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "joint_values", JointValues },
                { "joint_names", JointNames }
            };

            if (Accelerations.Count > 0)
            {
                data["accelerations"] = Accelerations;
            }
            if (Velocities.Count > 0)
            {
                data["velocities"] = Velocities;
            }
            if (Effort.Count > 0)
            {
                data["effort"] = Effort;
            }
            if (JointTypes.Count > 0)
            {
                data["joint_types"] = JointTypes;
            }

            return data;
        }

    }

}
