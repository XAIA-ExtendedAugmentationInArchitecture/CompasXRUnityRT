using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CompasXR.Core.Data;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace CompasXR.Robots.Model
{
    [Serializable]
    public class Mass
    {
        public float Value { get; set; }

        public static Mass Parse(object jsondata)
        {
            var data = jsondata as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Mass.Parse");
            return FromData(data);
        }

        public static Mass FromData(Dictionary<string, object> data)
        {
            return new Mass
            {
                Value = Convert.ToSingle(data.GetValueOrDefault("value") ?? 0f)
            };
        }
    }

    [Serializable]
    public class Inertia
    {
        public float Ixx { get; set; }
        public float Ixy { get; set; }
        public float Ixz { get; set; }
        public float Iyy { get; set; }
        public float Iyz { get; set; }
        public float Izz { get; set; }

        public static Inertia Parse(object jsondata)
        {
            var data = jsondata as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Inertia.Parse");
            return FromData(data);
        }

        public static Inertia FromData(Dictionary<string, object> data)
        {
            return new Inertia
            {
                Ixx = Convert.ToSingle(data.GetValueOrDefault("ixx") ?? 0f),
                Ixy = Convert.ToSingle(data.GetValueOrDefault("ixy") ?? 0f),
                Ixz = Convert.ToSingle(data.GetValueOrDefault("ixz") ?? 0f),
                Iyy = Convert.ToSingle(data.GetValueOrDefault("iyy") ?? 0f),
                Iyz = Convert.ToSingle(data.GetValueOrDefault("iyz") ?? 0f),
                Izz = Convert.ToSingle(data.GetValueOrDefault("izz") ?? 0f)
            };
        }
    }

    [Serializable]
    public class Inertial
    {
        public Frame Origin { get; set; }
        public Mass Mass { get; set; }
        public Inertia Inertia { get; set; }
        public Dictionary<string, object> Attr { get; set; }

        public static Inertial Parse(object jsondata)
        {
            var data = jsondata as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Inertial.Parse");
            return FromData(data);
        }

        public static Inertial FromData(Dictionary<string, object> data)
        {
            Dictionary<string, object> originDict = DictionaryHelpers.GetAsDictionary(data, "origin");
            if (originDict.Count <= 0)
            {
                throw new ArgumentException("Inertial origin is required.");
            }
            else
            {
                Debug.Log("Inertial origin is not null " + JsonConvert.SerializeObject(originDict));
            }
            Frame origin = Frame.FromData(originDict);

            Dictionary<string, object> massDict = DictionaryHelpers.GetAsDictionary(data, "mass");
            Mass mass = Mass.FromData(massDict);

            Dictionary<string, object> inertiaDict = DictionaryHelpers.GetAsDictionary(data, "inertia");
            Inertia inertia = Inertia.FromData(inertiaDict);

            Dictionary<string, object> attrDict = DictionaryHelpers.GetAsDictionary(data, "attr");
            
            return new Inertial
            {
                Origin = origin,
                Mass = mass,
                Inertia = inertia,
                Attr = attrDict ?? new Dictionary<string, object>()
            };
        }
    }

    public class LinkItem
    {
        public Matrix4x4? InitTransformation { get; set; }
        public Matrix4x4? CurrentTransformation { get; set; }
        public object NativeGeometry { get; set; }
        public static Matrix4x4 GetTransformationMatrix(Dictionary<string, object> data, string key) //TODO: Put this in the Visual class and check the other classes.
        {
            var tDict = DictionaryHelpers.GetAsDictionary(data, key);
            if (tDict.Count == 0)
            {
                return Matrix4x4.identity;
            }

            if (!tDict.TryGetValue("matrix", out var rawMatrix) || rawMatrix == null)
                throw new KeyNotFoundException($"No 'matrix' entry under '{key}'.");

            List<object> rowsObj;
            if (rawMatrix is List<object> lo)
            {
                rowsObj = lo;
            }
            else if (rawMatrix is JArray ja)
            {
                rowsObj = ja.ToObject<List<object>>();
            }
            else
            {
                throw new ArgumentException($"'{key}.matrix' must be a List<object> or JArray, but was {rawMatrix.GetType()}.");
            }

            if (rowsObj.Count != 4)
                throw new ArgumentException($"'{key}.matrix' must have 4 rows, but found {rowsObj.Count}.");


            var rows = new float[4][];
            for (int i = 0; i < 4; i++)
            {
                List<object> rowList;
                var rawRow = rowsObj[i];

                if (rawRow is List<object> rl)
                {
                    rowList = rl;
                }
                else if (rawRow is JArray rja)
                {
                    rowList = rja.ToObject<List<object>>();
                }
                else
                {
                    throw new ArgumentException($"Row {i} of '{key}.matrix' must be List<object> or JArray; got {rawRow.GetType()}.");
                }

                if (rowList.Count != 4)
                    throw new ArgumentException($"Row {i} of '{key}.matrix' must have 4 elements, but has {rowList.Count}.");

                var row = new float[4];
                for (int j = 0; j < 4; j++)
                {
                    row[j] = Convert.ToSingle(rowList[j]);
                }
                rows[i] = row;
            }

            var c0 = new Vector4(rows[0][0], rows[1][0], rows[2][0], rows[3][0]);
            var c1 = new Vector4(rows[0][1], rows[1][1], rows[2][1], rows[3][1]);
            var c2 = new Vector4(rows[0][2], rows[1][2], rows[2][2], rows[3][2]);
            var c3 = new Vector4(rows[0][3], rows[1][3], rows[2][3], rows[3][3]);

            return new Matrix4x4(c0, c1, c2, c3);
        }

    }

    [Serializable]
    public class Visual : LinkItem
    {
        public MeshDescriptor Geometry { get; set; }
        public Frame Origin { get; set; }
        public string Name { get; set; }
        public Material Material { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public static Visual Parse(object jsondata)
        {
            var data = jsondata as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Visual.Parse");
            return FromData(data);
        }

        public static Material CreateUnityMaterialFromDescription(Dictionary<string, object> description)
        {
            Material material = new Material(Shader.Find("Standard"));
            Dictionary<string, object> materialData = DictionaryHelpers.GetAsDictionary(description, "color");
            if (materialData != null)
            {
                Color color = new Color(
                    Convert.ToSingle(materialData.GetValueOrDefault("red") ?? 0f),
                    Convert.ToSingle(materialData.GetValueOrDefault("green") ?? 0f),
                    Convert.ToSingle(materialData.GetValueOrDefault("blue") ?? 0f),
                    Convert.ToSingle(materialData.GetValueOrDefault("alpha") ?? 1f)
                );
                material.color = color;
            }
            return material;
        }

        public static Visual FromData(Dictionary<string, object> data)
        {
            Dictionary<string, object> materialDescription = DictionaryHelpers.GetAsDictionary(data, "material");
            Material material = CreateUnityMaterialFromDescription(materialDescription);
            Dictionary<string, object> geometryData = DictionaryHelpers.GetAsDictionary(data, "geometry");
            MeshDescriptor geometry = MeshDescriptor.FromData(geometryData);

            Dictionary<string, object> frameDict = DictionaryHelpers.GetAsDictionary(data, "origin");
            if (frameDict == null)
            {
                throw new ArgumentException("Visual origin is required.");
            }
            else
            {
                Debug.Log("Visual origin is not null " + JsonConvert.SerializeObject(frameDict));
            }
            Frame origin = Frame.FromData(frameDict);
            
            Dictionary<string, object> attrDict = DictionaryHelpers.GetAsDictionary(data, "attr");

            Matrix4x4 initTransformation = GetTransformationMatrix(data, "init_transformation");
            Matrix4x4 currentTransformation = GetTransformationMatrix(data, "current_transformation");


            return new Visual
            {
                Geometry = geometry,
                Origin = origin,
                Name = data.GetValueOrDefault("name")?.ToString(),
                Material = material,
                Attributes = attrDict ?? new Dictionary<string, object>(),
                InitTransformation = initTransformation,
                CurrentTransformation = currentTransformation
            };
        }
    }


    [Serializable]
    public class Collision : LinkItem
    {
        public object Geometry { get; set; }
        public Frame Origin { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public static Collision Parse(object jsondata)
        {
            var d = jsondata as Dictionary<string, object>;
            if (d == null) throw new ArgumentException("Expected a Dictionary<string, object> for Collision.Parse");
            return FromData(d);
        }

        public static Collision FromData(Dictionary<string, object> data)
        {
            
            Dictionary<string, object> geometryData = DictionaryHelpers.GetAsDictionary(data, "geometry");
            MeshDescriptor geometry = MeshDescriptor.FromData(geometryData);

            Dictionary<string, object> originDict = DictionaryHelpers.GetAsDictionary(data, "origin");
            if (originDict.Count <= 0)
            {
                throw new ArgumentException("Collision origin is required.");
            }
            else
            {
                Debug.Log("Collision origin is not null " + JsonConvert.SerializeObject(originDict));
            }
            Frame origin = Frame.FromData(originDict);

            Dictionary<string, object> attrDict = DictionaryHelpers.GetAsDictionary(data, "attr");

            Matrix4x4 initTransformation = GetTransformationMatrix(data, "init_transformation");
            Matrix4x4 currentTransformation = GetTransformationMatrix(data, "current_transformation");

            return new Collision
            {
                Geometry = geometry,
                Origin = origin,
                Name = data.GetValueOrDefault("name")?.ToString(),
                Attributes = attrDict ?? new Dictionary<string, object>(),
                InitTransformation = initTransformation,
                CurrentTransformation = currentTransformation
            };
        }
    }

    [Serializable]
    public class Link
    {
        public string Name { get; set; }
        public string? Type { get; set; } 
        public List<Visual>? Visual { get; set; }
        public List<Collision>? Collision { get; set; }
        public Inertial? Inertial { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<RobotJoint> Joints { get; set; }

        public static Link Parse(object jsondata)
        {
            var data = jsondata as Dictionary<string, object>;
            if (data == null) throw new ArgumentException("Expected a Dictionary<string, object> for Link.Parse");
            return FromData(data);
        }

        public static Link FromData(Dictionary<string, object> data)
        {
            string name = data.GetValueOrDefault("name")?.ToString();
            Debug.Log("PARSE LINK : Link name is not null " + name);
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Link name is required.");
            }
            string? type = data.GetValueOrDefault("type")?.ToString();
            List<Dictionary<string, object>> visualData = DictionaryHelpers.GetListFromDict(data, "visual");
            List<Dictionary<string, object>> collisionData = DictionaryHelpers.GetListFromDict(data, "collision");
            List<Visual> visualParsed = new List<Visual>();
            if (visualData.Count > 0)
            {
                foreach (var visualItem in visualData)
                {
                    visualParsed.Add(Robots.Model.Visual.FromData(visualItem));
                }
            }
            List<Collision> collisionParsed = new List<Collision>();
            if (collisionData.Count > 0)
            {
                Debug.Log($"Collision data count is  {collisionData.Count}");
                Debug.Log($"Collision data is  {JsonConvert.SerializeObject(collisionData)}");
                foreach (var collisionItem in collisionData)
                {
                    collisionParsed.Add(Robots.Model.Collision.FromData(collisionItem));
                }
            }    
            
            Dictionary<string, object> inertialData = DictionaryHelpers.GetAsDictionary(data, "inertial");
            Inertial inertialParsed = new Inertial();
            if(inertialData.Count > 0)
            {
                inertialParsed = Inertial.FromData(inertialData);
            }
            else
            {
                inertialParsed = new Inertial();
            }
            Dictionary<string, object> attrDict = DictionaryHelpers.GetAsDictionary(data, "attr");

            List<Dictionary<string, object>> joints = DictionaryHelpers.GetListFromDict(data, "joints");
            List<RobotJoint> jointsParsed = new List<RobotJoint>();
            if (joints.Count > 0)
            {
                foreach (var jointItem in joints)
                {
                    jointsParsed.Add(RobotJoint.FromData(jointItem));
                }
            }

            return new Link
            {
                Name = name,
                Type = type,
                Visual = visualParsed ?? new List<Visual>(),
                Collision = collisionParsed,
                Inertial = inertialParsed ?? new Inertial(),
                Attributes = attrDict ?? new Dictionary<string, object>(),
                Joints = jointsParsed ?? new List<RobotJoint>()
            };
        }
    }


    [Serializable]
    public class MeshDescriptor
    {
        public Dictionary<string, object> Attributes { get; set; }
        public ShapeInfo Shape { get; set; }
        public static MeshDescriptor Parse(object jsondata)
        {
            var dict = jsondata as Dictionary<string, object>;
            if (dict == null) throw new ArgumentException("Expected a Dictionary<string,object> for MeshDescriptor.Parse");
            return FromData(dict);
        }

        public static MeshDescriptor FromData(Dictionary<string, object> data)
        {
            var shapeDict = DictionaryHelpers.GetAsDictionary(data, "shape");
            var attrDict = DictionaryHelpers.GetAsDictionary(data, "attr");
            var meshesData = new MeshDescriptor
            {
                Attributes  = attrDict ?? new Dictionary<string, object>(),
                Shape = ShapeInfo.FromData(shapeDict),
            };
            return meshesData;
        }

        [Serializable]
        public class ShapeInfo
        {
            public Dictionary<string, object> Attributes { get; set; }
            public DataInfo? Data { get; set; }
            public string dtype { get; set; }
            public string guid  { get; set; }

            public static ShapeInfo FromData(Dictionary<string, object> data)
            {
                DataInfo dataInfo = new DataInfo();
                if (data != null && data.ContainsKey("data"))
                {
                    Dictionary<string, object> dataDict = DictionaryHelpers.GetAsDictionary(data, "data");
                    dataInfo = DataInfo.FromData(dataDict);
                }
                else
                {
                    Debug.LogWarning("ShapeInfo data is null or does not contain 'data' key.");
                }

                return new ShapeInfo
                {
                    Attributes  = data.GetValueOrDefault("attr") as Dictionary<string, object> ?? new Dictionary<string, object>(),
                    Data  = dataInfo,
                    dtype = data.GetValueOrDefault("dtype")?.ToString(),
                    guid  = data.GetValueOrDefault("guid")?.ToString()
                };
            }
        }

        [Serializable]
        public class DataInfo
        {
            public Dictionary<string, object> Attributes { get; set; }
            public string FileName { get; set; }
            public List<CompasMesh> Meshes { get; set; }
            public float[] Scale { get; set; }

            public static DataInfo FromData(Dictionary<string, object> data)
            {
                
                if (data == null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                Dictionary<string, object> attrDict = DictionaryHelpers.GetAsDictionary(data, "attr");

                float[] scaleArray = Array.Empty<float>();
                if (data.TryGetValue("scale", out var rawScale) && rawScale != null) //TODO: the problem is here...
                {
                    List<object> scaleList = null;

                    if (rawScale is List<object> lo)
                    {
                        scaleList = lo;
                    }
                    else if (rawScale is JArray ja)
                    {
                        scaleList = ja.ToObject<List<object>>();
                    }
                    else if (rawScale is JToken jt && jt.Type == JTokenType.Array)
                    {
                        scaleList = ((JArray)jt).ToObject<List<object>>();
                    }

                    if (scaleList != null)
                    {
                        // your converter now gets a proper List<object>
                        scaleArray = DataConverters.ConvertDatatoFloatArray(scaleList);
                    }
                    else
                    {
                        Debug.LogWarning($"Unexpected type for 'scale': {rawScale.GetType()}. Using empty array.");
                    }
                }
                
                
                List<Dictionary<string, object>> meshesData = DictionaryHelpers.GetListFromDict(data, "meshes");
                List<CompasMesh> meshes = new List<CompasMesh>();
                if (meshesData != null)
                {
                    foreach (var mesh in meshesData)
                    {
                        meshes.Add(CompasMesh.FromData(mesh));
                    }
                }

                return new DataInfo
                {
                    Attributes = attrDict ?? new Dictionary<string, object>(),
                    FileName = data.GetValueOrDefault("filename")?.ToString(),
                    Meshes = meshes, 
                    Scale = scaleArray
                };
            }
        }
    }
}