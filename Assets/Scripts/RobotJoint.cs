using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CompasXR.Core.Data;
using RosSharp.Urdf;
using RosSharp;

namespace CompasXR.Robots.Model
{
    public static class DictExtensions
    {
        public static object GetValueOrDefault(this Dictionary<string, object> dict, string key)
        {
            return dict != null && dict.TryGetValue(key, out var v) ? v : null;
        }
    }

    [Serializable]
    public class ParentLink
    {
        public string link { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "link", link }
            };
        }
        
        public static ParentLink Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for ParentLink.Parse");
            return FromData(dict);
        }

        public static ParentLink FromData(Dictionary<string, object> d)
        {
            return new ParentLink
            {
                link = d.GetValueOrDefault("link")?.ToString()
            };
        }
    }

    [Serializable]
    public class ChildLink
    {
        public string link { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "link", link }
            };
        }
        
        public static ChildLink Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for ChildLink.Parse");
            return FromData(dict);
        }

        public static ChildLink FromData(Dictionary<string, object> d)
        {
            return new ChildLink
            {
                link = d.GetValueOrDefault("link")?.ToString()
            };
        }
    }

    [Serializable]
    public class Calibration
    {
        public float rising { get; set; }
        public float falling { get; set; }
        public float reference_position { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "rising", rising },
                { "falling", falling },
                { "reference_position", reference_position }
            };
        }
        
        public static Calibration Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for Calibration.Parse");
            return FromData(dict);
        }

        public static Calibration FromData(Dictionary<string, object> d)
        {
            return new Calibration
            {
                rising = Convert.ToSingle(d.GetValueOrDefault("rising") ?? 0f),
                falling = Convert.ToSingle(d.GetValueOrDefault("falling") ?? 0f),
                reference_position = Convert.ToSingle(d.GetValueOrDefault("reference_position") ?? 0f)
            };
        }
    }

    [Serializable]
    public class Dynamics
    {
        public float damping { get; set; }
        public float friction { get; set; }
        public Dictionary<string, object> attr { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "damping", damping },
                { "friction", friction },
                { "attr", attr }
            };
        }
        
        public static Dynamics Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for Dynamics.Parse");
            return FromData(dict);
        }

        public static Dynamics FromData(Dictionary<string, object> d)
        {
            return new Dynamics
            {
                damping = Convert.ToSingle(d.GetValueOrDefault("damping") ?? 0f),
                friction = Convert.ToSingle(d.GetValueOrDefault("friction") ?? 0f),
                attr = d.GetValueOrDefault("attr") as Dictionary<string, object> ?? new Dictionary<string, object>()
            };
        }
    }

    [Serializable]
    public class Limit
    {
        public float effort { get; set; }
        public float velocity { get; set; }
        public float lower { get; set; }
        public float upper { get; set; }
        public Dictionary<string, object> attr { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "effort", effort },
                { "velocity", velocity },
                { "lower", lower },
                { "upper", upper },
                { "attr", attr }
            };
        }
        
        public static Limit Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for Limit.Parse");
            return FromData(dict);
        }

        public static Limit FromData(Dictionary<string, object> d)
        {
            return new Limit
            {
                lower = Convert.ToSingle(d.GetValueOrDefault("lower") ?? 0f),
                upper = Convert.ToSingle(d.GetValueOrDefault("upper") ?? 0f),
                effort = Convert.ToSingle(d.GetValueOrDefault("effort") ?? 0f),
                velocity = Convert.ToSingle(d.GetValueOrDefault("velocity") ?? 0f),
                attr = d.GetValueOrDefault("attr") as Dictionary<string, object> ?? new Dictionary<string, object>()
            };
        }
    }

    [Serializable]
    public class Mimic
    {
        public string joint { get; set; }
        public float multiplier { get; set; }
        public float offset { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "joint", joint },
                { "multiplier", multiplier },
                { "offset", offset }
            };
        }

        public static Mimic Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for Mimic.Parse");
            return FromData(dict);
        }

        public static Mimic FromData(Dictionary<string, object> data)
        {
            return new Mimic
            {
                joint = data.GetValueOrDefault("joint")?.ToString(),
                multiplier = Convert.ToSingle(data.GetValueOrDefault("multiplier") ?? 1f),
                offset = Convert.ToSingle(data.GetValueOrDefault("offset") ?? 0f)
            };
        }
    }

    [Serializable]
    public class SafetyController
    {
        public float k_velocity { get; set; }
        public float k_position { get; set; }
        public float soft_lower_limit { get; set; }
        public float soft_upper_limit { get; set; }

        public static SafetyController Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for SafetyController.Parse");
            return FromData(dict);
        }

        public static SafetyController FromData(Dictionary<string, object> d)
        {
            return new SafetyController
            {
                k_velocity = Convert.ToSingle(d.GetValueOrDefault("k_velocity") ?? 0f),
                k_position = Convert.ToSingle(d.GetValueOrDefault("k_position") ?? 0f),
                soft_lower_limit = Convert.ToSingle(d.GetValueOrDefault("soft_lower_limit") ?? 0f),
                soft_upper_limit = Convert.ToSingle(d.GetValueOrDefault("soft_upper_limit") ?? 0f)
            };
        }
    }

    [Serializable]
    public class Axis
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public Dictionary<string, object> attr { get; set; }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "x", x },
                { "y", y },
                { "z", z },
                { "attr", attr }
            };
        }
        
        public static Axis Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string, object> for Axis.Parse");
            return FromData(dict);
        }

        public static Axis FromData(Dictionary<string, object> data)
        {
            return new Axis
            {
                x = Convert.ToSingle(data.GetValueOrDefault("x") ?? 0f),
                y = Convert.ToSingle(data.GetValueOrDefault("y") ?? 0f),
                z = Convert.ToSingle(data.GetValueOrDefault("z") ?? 0f),
                attr = data.GetValueOrDefault("attr") as Dictionary<string, object> ?? new Dictionary<string, object>()
            };
        }
    }

    [Serializable]
    public class RobotJoint
    {
        public string Name { get; set; }
        public JointType Type { get; set; } //TODO: make enum
        public ParentLink Parent { get; set; }
        public ChildLink Child { get; set; }
        public Frame Origin { get; set; }
        public Axis Axis { get; set; }
        public Calibration Calibration { get; set; }
        public Dynamics Dynamics { get; set; }
        public Limit Limit { get; set; }
        public SafetyController SafetyController { get; set; }
        public Mimic Mimic { get; set; }
        public Dictionary<string, object> Attr { get; set; }
        public float Position { get; set; }

        public enum JointType
        {
            Revolute,
            Continuous,
            Prismatic,
            Fixed,
            Unknown
        }
            
        public static JointType GetJointTypeFromString(string type)
        {
            return type switch
            {
                "revolute" => JointType.Revolute,
                "continuous" => JointType.Continuous,
                "prismatic" => JointType.Prismatic,
                "fixed" => JointType.Fixed,
                _ => JointType.Unknown
            };
        }

        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "name", Name },
                { "type", Type.ToString().ToLower() },
                { "position", Position },
                { "attr", Attr },
                { "parent", Parent.GetData() },
                { "child", Child.GetData() },
                { "origin", Origin.GetData() },
                { "axis", Axis.GetData() }
            };
        }
        
        public static RobotJoint Parse(object jsondata)
        {
            var data = jsondata as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Joint.Parse");
            return FromData(data);
        }

        public static RobotJoint FromData(Dictionary<string, object> data)
        {
            var joint = new RobotJoint
            {
                Name = data.GetValueOrDefault("name")?.ToString(),
                Type = JointType.Unknown,
                Position = Convert.ToSingle(data.GetValueOrDefault("position") ?? 0f),
                Attr = data.GetValueOrDefault("attr") as Dictionary<string, object> ?? new Dictionary<string, object>()
            };

            joint.Parent = ParentLink.FromData(data["parent"] as Dictionary<string, object>);
            joint.Child  = ChildLink.FromData(data["child"]  as Dictionary<string, object>);
            Dictionary<string, object> originData = DictionaryHelpers.GetAsDictionary(data, "origin");
            joint.Origin = Frame.FromData(originData);
            joint.Axis   = Axis.FromData(data["axis"]    as Dictionary<string, object>);

            string type = data.GetValueOrDefault("type")?.ToString();
            JointType jointType = GetJointTypeFromString(type);
            if (jointType == JointType.Unknown)
            {
                Debug.LogWarning($"Joint type '{type}' is not recognized. Defaulting to 'Unknown'.");
            }
            joint.Type = jointType;

            if (data.TryGetValue("calibration", out var calabration) && calabration       != null)
                joint.Calibration = Calibration.FromData(calabration as Dictionary<string, object>);
            if (data.TryGetValue("dynamics",    out var dynamics) && dynamics      != null)
                joint.Dynamics    = Dynamics.FromData(dynamics as Dictionary<string, object>);
            if (data.TryGetValue("limit",       out var limits) && limits      != null)
                joint.Limit       = Limit.FromData(limits as Dictionary<string, object>);
            if (data.TryGetValue("safety_controller", out var safety_controler) && safety_controler != null)
                joint.SafetyController = SafetyController.FromData(safety_controler as Dictionary<string, object>);
            if (data.TryGetValue("mimic",       out var mimic) && mimic      != null)
                joint.Mimic       = Mimic.FromData(mimic as Dictionary<string, object>);
            return joint;
        }

    //TODO: COME BACK TO THIS

        public GameObject AddRosSharpJointToGameObject(GameObject parent, RobotJoint joint) //TODO: Add Rigid Bodies
        {
            switch (joint.Type)
            {
                case JointType.Revolute:
                    UrdfJointRevolute revJoint = parent.AddComponent<UrdfJointRevolute>();
                    HingeJoint hingeJoint = parent.AddComponent<HingeJoint>();
                    HingeJointAngleCalculator angleCalculator = parent.AddComponent<HingeJointAngleCalculator>();
                    HingeJointLimitsManager limitsManager = parent.AddComponent<HingeJointLimitsManager>();
                    UrdfInertial revinertial = parent.AddComponent<UrdfInertial>();
                    Rigidbody revRigidBody = parent.AddComponent<Rigidbody>();
                    revRigidBody.isKinematic = true;
                    revRigidBody.useGravity = false;
                    return parent;
                case JointType.Continuous:
                    UrdfJointContinuous contJoint = parent.AddComponent<UrdfJointContinuous>();
                    HingeJoint contHingeJoint = parent.AddComponent<HingeJoint>();
                    HingeJointAngleCalculator contAngleCalculator = parent.AddComponent<HingeJointAngleCalculator>();
                    HingeJointLimitsManager contLimitsManager = parent.AddComponent<HingeJointLimitsManager>();
                    UrdfInertial continertial = parent.AddComponent<UrdfInertial>();
                    Rigidbody contRigidBody = parent.AddComponent<Rigidbody>();
                    contRigidBody.isKinematic = true;
                    contRigidBody.useGravity = false;
                    Debug.LogWarning($"Joint type '{joint.Type}' is Work in progress.");
                    return parent;
                case JointType.Prismatic:
                    UrdfJointPrismatic prismJoint = parent.AddComponent<UrdfJointPrismatic>();
                    ConfigurableJoint configJoint = parent.AddComponent<ConfigurableJoint>();
                    PrismaticJointLimitsManager prismaticLimitsManager = parent.AddComponent<PrismaticJointLimitsManager>();
                    UrdfInertial prisinertial = parent.AddComponent<UrdfInertial>();
                    Rigidbody prismRigidBody = parent.AddComponent<Rigidbody>();
                    prismRigidBody.isKinematic = true;
                    prismRigidBody.useGravity = false;
                    Debug.LogWarning($"Joint type '{joint.Type}' is Work in progress.");
                    return parent;
                case JointType.Fixed:
                    UrdfJointFixed fixedJoint = parent.AddComponent<UrdfJointFixed>();
                    UrdfInertial fixinertial = parent.AddComponent<UrdfInertial>();
                    Rigidbody fixedRigidBody = parent.AddComponent<Rigidbody>();
                    fixedRigidBody.isKinematic = true;
                    fixedRigidBody.useGravity = false;
                    Debug.LogWarning($"Joint type '{joint.Type}' is Work in progress.");
                    return parent;
                case JointType.Unknown:
                    Debug.LogWarning($"Joint type '{joint.Type}' is Work in progress.");
                    return parent;
                default:
                    Debug.LogWarning($"Joint type '{joint.Type}' is Work in progress.");
                    return parent;
            }
        }
    }
}