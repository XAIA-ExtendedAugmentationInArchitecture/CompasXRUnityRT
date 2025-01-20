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
        public List<JointTrajectoryPoint> Configurations { get; set; }
        // public List<AttachedCollisionMeshes> AttachedCollisionMeshes { get; set; }
        public List<string> JointNames { get; set; }
        public JointTrajectoryPoint StartConfiguration { get; set; }
        public float PlanningTime { get; set; }
        public float Fraction { get; set; }

        //TODO: ADD THE REST OF THE SHIT.....
    }

    //TODO: THESE THINGS BELOW SHOULD BE DONE....
    public class JointTrajectoryPoint : Configuration
    {
        public List<float> Accelerations { get; set; }
        public List<float> Velocities { get; set; }
        public List<float> Effort { get; set; }

        public JointTrajectoryPoint(
            List<float> jointValues,
            List<string> jointNames,
            List<float> accelerations = null,
            List<float> velocities = null,
            List<float> effort = null,
            List<int> jointTypes = null
        ) : base(jointValues, jointNames, jointTypes)
        {
            Accelerations = accelerations ?? new List<float>();
            Velocities = velocities ?? new List<float>();
            Effort = effort ?? new List<float>();
        }

        public new Dictionary<string, object> GetData()
        {
            var data = base.GetData();
            data["accelerations"] = Accelerations;
            data["velocities"] = Velocities;
            data["effort"] = Effort;
            return data;
        }
        public static new JointTrajectoryPoint Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }

        public static new JointTrajectoryPoint FromData(Dictionary<string, object> jsonDataDict)
        {
            Configuration baseConfig = Configuration.FromData(jsonDataDict);

            JointTrajectoryPoint jointTrajectoryPoint = new JointTrajectoryPoint(
                baseConfig.JointValues, 
                baseConfig.JointNames, 
                jsonDataDict.ContainsKey("accelerations") ? ((List<object>)jsonDataDict["accelerations"]).Select(Convert.ToSingle).ToList() : null,
                jsonDataDict.ContainsKey("velocities") ? ((List<object>)jsonDataDict["velocities"]).Select(Convert.ToSingle).ToList() : null,
                jsonDataDict.ContainsKey("effort") ? ((List<object>)jsonDataDict["effort"]).Select(Convert.ToSingle).ToList() : null,
                baseConfig.JointTypes
            ); //TODO: CHECK THE PARSING HERE MIGHT RESULT IN ERRORS.
            return jointTrajectoryPoint;
        }
    }

    public class Configuration
    {
        public List<float> JointValues { get; set; }
        public List<string> JointNames { get; set; }
        public List<int> JointTypes { get; set; } //TODO: Does this make sense to make an ENUM?
        public Dictionary<string, float> JointsDict { get; set; }
        public Configuration(List<float> jointValues, List<string> jointNames, List<int> jointTypes = null)
        {
            JointValues = jointValues ?? throw new ArgumentNullException(nameof(jointValues));
            JointNames = jointNames ?? throw new ArgumentNullException(nameof(jointNames));
            JointTypes = jointTypes ?? new List<int>();
            JointsDict = CreateJointDict();
        }

        private Dictionary<string, float> CreateJointDict()
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
            List<int> jointTypes = new List<int>();            

            if (jsonDataDict.ContainsKey("joint_types"))
            {
                jointTypes = ((List<object>)jsonDataDict["joint_types"]).Select(Convert.ToInt32).ToList(); //TODO: Does this make sense to make an ENUM?
            }
            else
            {
                Debug.LogWarning("Joint types not found in configuration data. Setting to empty list.");
            }

            Configuration configuration = new Configuration(jointValues, jointNames, jointTypes);

            return configuration;
        }
        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "joint_values", JointValues },
                { "joint_names", JointNames },
                { "joint_types", JointTypes }
            };
            return data;
        }
    }

}
