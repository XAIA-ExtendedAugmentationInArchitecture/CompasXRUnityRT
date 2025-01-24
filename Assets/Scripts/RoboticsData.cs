using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CompasXR.Core.Data;

using UnityEngine;

namespace CompasXR.Robots.Data
{
    public class Trajectory
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
            var pointsList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataDict["points"].ToString());
            List<JointTrajectoryPoint> trajectoryPoints = new List<JointTrajectoryPoint>();
            foreach (var point in pointsList)
            {
                JointTrajectoryPoint jointTrajectoryPoint = JointTrajectoryPoint.FromData(point);
                trajectoryPoints.Add(jointTrajectoryPoint);
            }
            if (trajectoryPoints.Count > 0)
            {
                Debug.Log($"TrajectoryFromData: Deserialized Trajectory with {trajectoryPoints.Count} points.");
            }
            else
            {
                Debug.LogWarning("TrajectoryFromData: No points found in trajectory.");
            }

            // Fix: Ensure proper conversion of 'start_configuration'
            Dictionary<string, object> startConfigurationDict = null;
            if (jsonDataDict.ContainsKey("start_configuration"))
            {
                var startConfigurationObj = jsonDataDict["start_configuration"];

                if (startConfigurationObj is JObject startConfigJObject)
                {
                    startConfigurationDict = startConfigJObject.ToObject<Dictionary<string, object>>();
                }
                else if (startConfigurationObj is Dictionary<string, object> startConfigDict)
                {
                    startConfigurationDict = startConfigDict;
                }
                else
                {
                    Debug.LogError("TrajectoryFromData : start_configuration is not in expected format.");
                }
            }
            if (startConfigurationDict == null)
            {
                throw new InvalidCastException("TrajectoryFromData : Invalid or missing 'start_configuration' field in the JSON data.");
            }
            Configuration startConfiguration = Configuration.FromData(startConfigurationDict);

            List<string> jointNames = new List<string>();
            if (jsonDataDict.ContainsKey("joint_names"))
            {
                jointNames = DataConverters.ConvertDataToStringList(jsonDataDict["joint_names"]);
            }
            else
            {
                Debug.LogWarning("TrajectoryFromData : Joint names not found in trajectory data. Setting to empty list.");
            }

            float? planningTime = jsonDataDict.ContainsKey("planning_time")
                ? Convert.ToSingle(jsonDataDict["planning_time"])
                : null;

            float? fraction = jsonDataDict.ContainsKey("fraction")
                ? Convert.ToSingle(jsonDataDict["fraction"])
                : null;

            var attributes = jsonDataDict.ContainsKey("attributes")
                ? jsonDataDict["attributes"] as Dictionary<string, object>
                : null;

            return new Trajectory(trajectoryPoints, startConfiguration, jointNames, planningTime, fraction, attributes);
        }    
    }

    public class JointTrajectoryPoint : Configuration
    {
        public List<float> Accelerations { get; set; }
        public List<float> Velocities { get; set; }
        public List<float> Effort { get; set; }
        public Dictionary<string, object> TimeFromStart { get; set; } //TODO: Needs implementation

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
            List<float> accelerations = new List<float>();
            if (jsonDataDict.ContainsKey("accelerations"))
            {
                accelerations = DataConverters.ConvertDatatoFloatList(jsonDataDict["accelerations"]);
            }
            List<float> velocities = new List<float>();
            if (jsonDataDict.ContainsKey("velocities"))
            {
                velocities = DataConverters.ConvertDatatoFloatList(jsonDataDict["velocities"]);
            }
            List<float> effort = new List<float>();
            if (jsonDataDict.ContainsKey("effort"))
            {
                effort = DataConverters.ConvertDatatoFloatList(jsonDataDict["effort"]);
            }

            JointTrajectoryPoint jointTrajectoryPoint = new JointTrajectoryPoint(
                baseConfig.JointValues, 
                baseConfig.JointNames, 
                accelerations,
                velocities,
                effort,
                baseConfig.JointTypes
            );
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
                throw new InvalidOperationException("ConfigurationCreateJointDict : JointNames and JointValues lists must have the same length.");
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
            List<float> jointValues = DataConverters.ConvertDatatoFloatList(jsonDataDict["joint_values"]);
            List<string> jointNames = DataConverters.ConvertDataToStringList(jsonDataDict["joint_names"]);
            List<int> jointTypes = new List<int>();
            if (jsonDataDict.ContainsKey("joint_types"))
            {
                jointTypes = DataConverters.ConvertDataToIntList(jsonDataDict["joint_types"]);
            }
            else
            {
                Debug.LogWarning("ConfigurationFromData : Joint types not found in configuration data. Setting to empty list.");
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

    public class AttachedCollisionMesh
    {
        public CollisionMesh CollisionMesh { get; set; }
        public string LinkName { get; set; }
        public List<string> TouchLinks { get; set; }
        public double Weight { get; set; }

        public AttachedCollisionMesh(
            CollisionMesh collisionMesh,
            string linkName,
            List<string> touchLinks = null,
            double weight = 1.0
        )
        {
            CollisionMesh = collisionMesh ?? throw new ArgumentNullException(nameof(collisionMesh));
            LinkName = linkName ?? throw new ArgumentNullException(nameof(linkName));
            TouchLinks = touchLinks ?? new List<string>();
            Weight = weight;
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "collision_mesh", CollisionMesh.GetData() },
                { "link_name", LinkName },
                { "touch_links", TouchLinks },
                { "weight", Weight }
            };
            return data;
        }
    }

    public class CollisionMesh
    {
        public Frame Frame { get; set; }
        public string Id { get; set; }
        public Mesh Mesh { get; set; }
        public string RootName { get; set; }

        public CollisionMesh(
            Frame frame,
            string id,
            Mesh mesh,
            string rootName
        )
        {
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
            RootName = rootName ?? throw new ArgumentNullException(nameof(rootName));
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "frame", Frame.GetData() },
                { "id", Id },
                { "mesh", Mesh },
                { "root_name", RootName }
            };
            return data;
        }

        public static CollisionMesh Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }

        public static CollisionMesh FromData(Dictionary<string, object> jsonDataDict)
        {
            Frame frame = Frame.FromData(jsonDataDict["frame"] as Dictionary<string, object>);
            string id = jsonDataDict["id"] as string;
            Mesh mesh = Mesh.FromData(jsonDataDict["mesh"] as Dictionary<string, object>);
            string rootName = jsonDataDict["root_name"] as string;

            return new CollisionMesh(frame, id, mesh, rootName);
        }
    }

    //TODO: MOVE TO COMPASXR.CORE.DATA NAMESPACE AND THINK ABOUT THIS...FUCK THIS WILL NOT WORK YOU NEED RH TO LH CONVERSION :(....////////////////////////////////////////////////////////////////////////////////////////
    public class Mesh
    {
        public Dictionary<string, object> Attributes { get; set; }
        public Dictionary<string, object> DefaultEdgeAttributes { get; set; }
        public Dictionary<string, object> DefaultFaceAttributes { get; set; }
        public Dictionary<string, object> DefaultVertexAttributes { get; set; }
        public Dictionary<string, int[]> Faces { get; set; }
        public Dictionary<string, object> FaceData { get; set; }
        public int MaxFace { get; set; }
        public int MaxVertex { get; set; }
        public Dictionary<string, Vertex> Vertex { get; set; }

        public Mesh(
            Dictionary<string, object> attributes,
            Dictionary<string, object> defaultEdgeAttributes,
            Dictionary<string, object> defaultFaceAttributes,
            Dictionary<string, object> defaultVertexAttributes,
            Dictionary<string, int[]> faces,
            Dictionary<string, object> faceData,
            int maxFace,
            int maxVertex,
            Dictionary<string, Vertex> vertex
        )
        {
            Attributes = attributes ?? new Dictionary<string, object>();
            DefaultEdgeAttributes = defaultEdgeAttributes ?? new Dictionary<string, object>();
            DefaultFaceAttributes = defaultFaceAttributes ?? new Dictionary<string, object>();
            DefaultVertexAttributes = defaultVertexAttributes ?? new Dictionary<string, object>();
            Faces = faces ?? new Dictionary<string, int[]>();
            FaceData = faceData ?? new Dictionary<string, object>();
            MaxFace = maxFace;
            MaxVertex = maxVertex;
            Vertex = vertex ?? new Dictionary<string, Vertex>();
        }

        public static Dictionary<string, object> GetVertexDataFromDict(Dictionary<string, Vertex> vertexDict)
        {
            Dictionary<string, object> vertexData = new Dictionary<string, object>();
            for (int i = 0; i < vertexDict.Count; i++)
            {
                vertexData[i.ToString()] = vertexDict[i.ToString()].GetData();
            }
            return vertexData;
        }
        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "attributes", Attributes },
                { "default_edge_attributes", DefaultEdgeAttributes },
                { "default_face_attributes", DefaultFaceAttributes },
                { "default_vertex_attributes", DefaultVertexAttributes },
                { "faces", Faces },
                { "face_data", FaceData },
                { "max_face", MaxFace },
                { "max_vertex", MaxVertex },
                { "vertex", Vertex }
            };
            return data;
        }
        
        public static Mesh Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }

        public static Mesh FromData(Dictionary<string, object> jsonDataDict)
        {
            if (jsonDataDict == null)
            {
                throw new ArgumentNullException(nameof(jsonDataDict), "Input data cannot be null.");
            }

            var attributes = GetSafeDictionary(jsonDataDict, "attributes");
            var defaultEdgeAttributes = GetSafeDictionary(jsonDataDict, "default_edge_attributes");
            var defaultFaceAttributes = GetSafeDictionary(jsonDataDict, "default_face_attributes");
            var defaultVertexAttributes = GetSafeDictionary(jsonDataDict, "default_vertex_attributes");
            var faces = GetSafeDictionary<int[]>(jsonDataDict, "faces");
            var faceData = GetSafeDictionary(jsonDataDict, "face_data");

            int maxFace = jsonDataDict.TryGetValue("max_face", out var maxFaceObj) ? Convert.ToInt32(maxFaceObj) : 0;
            int maxVertex = jsonDataDict.TryGetValue("max_vertex", out var maxVertexObj) ? Convert.ToInt32(maxVertexObj) : 0;

            Dictionary<string, Vertex> vertex = new Dictionary<string, Vertex>();
            if (jsonDataDict.TryGetValue("vertex", out var vertexObj) && vertexObj is Dictionary<string, object> vertexDict)
            {
                foreach (var kvp in vertexDict)
                {
                    if (kvp.Value is Dictionary<string, object> vertexData)
                    {
                        double x = DataConverters.ConvertNumericDataToDouble(vertexData["x"]);
                        double y = DataConverters.ConvertNumericDataToDouble(vertexData["y"]);
                        double z = DataConverters.ConvertNumericDataToDouble(vertexData["z"]);
                        vertex[kvp.Key] = new Vertex(x, y, z);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Mesh.FromData: Vertex data not found or invalid. Setting to empty dictionary.");
            }

            return new Mesh(attributes, defaultEdgeAttributes, defaultFaceAttributes, defaultVertexAttributes, faces, faceData, maxFace, maxVertex, vertex);
        }

        private static Dictionary<string, object> GetSafeDictionary(Dictionary<string, object> jsonDataDict, string key)
        {
            return jsonDataDict.TryGetValue(key, out var obj) && obj is Dictionary<string, object> dict ? dict : new Dictionary<string, object>();
        }

        private static Dictionary<string, int[]> GetSafeDictionary<T>(Dictionary<string, object> jsonDataDict, string key)
        {
            return jsonDataDict.TryGetValue(key, out var obj) && obj is Dictionary<string, int[]> dict ? dict : new Dictionary<string, int[]>();
        }
    }

    public class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vertex(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "x", X },
                { "y", Y },
                { "z", Z }
            };
            return data;
        }

        public static Vertex Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }

        public static Vertex FromData(Dictionary<string, object> jsonDataDict)
        {
            double x = DataConverters.ConvertNumericDataToDouble(jsonDataDict["x"]);
            double y = DataConverters.ConvertNumericDataToDouble(jsonDataDict["y"]);
            double z = DataConverters.ConvertNumericDataToDouble(jsonDataDict["z"]);
            return new Vertex(x, y, z);
        }
    }
}
