using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json;

using UnityEngine;

namespace CompasXR.Robots.Data
{

    public class Trajectory //TODO: Double check this class and make sure it is correct.
    {
        public List<JointTrajectoryPoint> Points { get; set; }
        // public List<AttachedCollisionMeshes> AttachedCollisionMeshes { get; set; }
        public List<string> JointNames { get; set; }
        public Configuration StartConfiguration { get; set; }
        public float? PlanningTime { get; set; }
        public float? Fraction { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public Trajectory(
            List<JointTrajectoryPoint> points,
            Configuration startConfiguration,
            List<string> jointNames=null,
            float? planningTime=null,
            float? fraction=null,
            Dictionary<string, object> attributes = null
        )
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            StartConfiguration = startConfiguration ?? throw new ArgumentNullException(nameof(startConfiguration));
            JointNames = jointNames ?? GetJointNames();
            PlanningTime = planningTime;
            Fraction = fraction;
            Attributes = attributes ?? new Dictionary<string, object>();
        }
        private List<string> GetJointNames()
        {
            return Points.FirstOrDefault()?.JointNames ?? new List<string>();
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "points", Points.Select(point => point.GetData()).ToList() },
                { "start_configuration", StartConfiguration.GetData() },
                { "joint_names", JointNames },
                { "planning_time", PlanningTime },
                { "fraction", Fraction },
                { "attributes", Attributes }
            };
            return data;
        }

        public static Trajectory Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }
        public static Trajectory FromData(Dictionary<string, object> jsonDataDict)
        {
            //TODO: Test this and maybe make some exception loops.
            List<JointTrajectoryPoint> points = ((List<object>)jsonDataDict["points"]).Select(obj => JointTrajectoryPoint.FromData((Dictionary<string, object>)obj)).ToList();
            Configuration startConfiguration = Configuration.FromData((Dictionary<string, object>)jsonDataDict["start_configuration"]);
            List<string> jointNames = jsonDataDict.ContainsKey("joint_names") ? ((List<object>)jsonDataDict["joint_names"]).Select(obj => obj.ToString()).ToList() : null;
            float? planningTime = jsonDataDict.ContainsKey("planning_time") ? Convert.ToSingle(jsonDataDict["planning_time"]) : null;
            float? fraction = jsonDataDict.ContainsKey("fraction") ? Convert.ToSingle(jsonDataDict["fraction"]) : null;
            Dictionary<string, object> attributes = jsonDataDict.ContainsKey("attributes") ? (Dictionary<string, object>)jsonDataDict["attributes"] : null;
            Trajectory trajectory = new Trajectory(points, startConfiguration, jointNames, planningTime, fraction, attributes);
            return trajectory;
        }
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
