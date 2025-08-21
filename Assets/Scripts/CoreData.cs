using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CompasXR.Core.Data
{
    /*
    * CompasXR.Core.Data : A namespace to define and controll various data structures and data processing methods.
    * This namespace is used to define the data structures that corelate to Compas data structures
    */

    ///////////// Class for Handeling Data conversion Inconsistencies /////////////// 

    [System.Serializable]
    public static class DataConverters
    {
        /*
        * DataConverters : A class to handle data conversion inconsistencies between different data types
        * and casting information to info required for deserilization.
        */

        public static float[] ConvertDatatoFloatArray(object data)
        {
            if (data is List<object>)
            {
                List<object> dataList = data as List<object>;
                return dataList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is float[])
            {
                return (float[])data;
            }
            else if (data is List<System.Double>)
            {
                List<System.Double> doubleList = data as List<System.Double>;
                return doubleList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is System.Single[])
            {
                return new float[] { (float)data };
            }
            else if (data is List<System.Single>)
            {
                List<System.Single> singleList = data as List<System.Single>;
                return singleList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is System.Double[])
            {
                System.Double[] doubleArray = data as System.Double[];
                return doubleArray.Select(Convert.ToSingle).ToArray();
            }
            else if (data is JArray)
            {
                JArray dataArray = data as JArray;
                return dataArray.Select(token => (float)token).ToArray();
            }
            else
            {
                Debug.LogError("DataParser: Data is not a List<Object>, List<System.Double>, System.Double Array, System.Single Array, List<System.Single>,  float Array, or JArray.");
                return null;
            }
        }
        public static List<float> ConvertDatatoFloatList(object data)
        {
            if (data is List<object>)
            {
                List<object> dataList = data as List<object>;
                return dataList.Select(Convert.ToSingle).ToList();
            }
            else if (data is float[])
            {
                float[] floatArray = data as float[];
                return floatArray.ToList();
            }
            else if (data is List<double>)
            {
                List<double> doubleList = data as List<double>;
                return doubleList.Select(Convert.ToSingle).ToList();
            }
            else if (data is float)
            {
                return new List<float> { (float)data };
            }
            else if (data is List<float>)
            {
                return data as List<float>;
            }
            else if (data is double[])
            {
                double[] doubleArray = data as double[];
                return doubleArray.Select(Convert.ToSingle).ToList();
            }
            else if (data is JArray)
            {
                JArray dataArray = data as JArray;
                return dataArray.Select(token => (float)token).ToList();
            }
            else
            {
                Debug.LogError("DataParser: Data is not a List<Object>, List<double>, double Array, float Array, float, or JArray.");
                return null;
            }
        }
        public static List<string> ConvertDataToStringList(object data)
        {
            if (data is JArray jArray)
            {
                return jArray.Select(obj => obj.ToString()).ToList();
            }
            else if (data is List<object> objectList)
            {
                return objectList.Select(obj => obj.ToString()).ToList();
            }
            else if (data is List<string> stringList)
            {
                return stringList;
            }
            else
            {
                throw new InvalidCastException("Data is not a valid type for conversion to List<string>.");
            }
        }
        public static List<int> ConvertDataToIntList(object data)
        {
            if (data is JArray jArray)
            {
                // Convert JArray to List<int>
                return jArray.Select(token => Convert.ToInt32(token)).ToList();
            }
            else if (data is List<object> objectList)
            {
                // Convert List<object> to List<int>
                return objectList.Select(obj => Convert.ToInt32(obj)).ToList();
            }
            else if (data is List<int> intList)
            {
                // Return as-is if it's already a List<int>
                return intList;
            }
            else
            {
                throw new InvalidCastException("Data is not a valid type for conversion to List<int>.");
            }
        }
        public static double ConvertNumericDataToDouble(object data)
        {
            if (data is double)
            {
                return (double)data;
            }
            else if (data is float)
            {
                return Convert.ToDouble(data);
            }
            else if (data is int)
            {
                return Convert.ToDouble(data);
            }
            else
            {
                throw new InvalidCastException("Data is not a valid type for conversion to double.");
            }
        }

    }

    public static class DictionaryHelpers
    {
        public static Dictionary<string, object> GetSafeDictionary(Dictionary<string, object> jsonDataDict, string key)
        {
            return jsonDataDict.TryGetValue(key, out var obj) && obj is Dictionary<string, object> dict ? dict : new Dictionary<string, object>();
        }
        public static Dictionary<string, int[]> GetSafeDictionary<T>(Dictionary<string, object> jsonDataDict, string key)
        {
            return jsonDataDict.TryGetValue(key, out var obj) && obj is Dictionary<string, int[]> dict ? dict : new Dictionary<string, int[]>();
        }
        public static Dictionary<string, object> GetAsDictionary(Dictionary<string, object> jsonDataDict, string key)
        {
            if (jsonDataDict == null)
            {
                return new Dictionary<string, object>();
            }
            if (!jsonDataDict.TryGetValue(key, out var obj) || obj == null)
            {
                return new Dictionary<string, object>();
            }
            if (obj is Dictionary<string, object> dict)
            {
                return dict;
            }
            if (obj is JObject jObj)
            {
                return jObj.ToObject<Dictionary<string, object>>();
            }
            if (obj is JToken jToken && jToken.Type == JTokenType.Object)
            {
                return ((JObject)jToken).ToObject<Dictionary<string, object>>();
            }

            try
            {
                var json = JsonConvert.SerializeObject(obj);
                var fallback = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return fallback ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
        public static List<Dictionary<string, object>> GetListFromDict(Dictionary<string, object> jsonDataDict, string key)
        {
            var result = new List<Dictionary<string, object>>();
            if (jsonDataDict == null || !jsonDataDict.TryGetValue(key, out var obj) || obj == null)
                return result;

            if (obj is JArray jArray)
            {
                foreach (var token in jArray)
                {
                    if (token is JObject jo)
                    {
                        result.Add(jo.ToObject<Dictionary<string, object>>());
                    }
                }
                return result;
            }

            if (obj is JToken jt && jt.Type == JTokenType.Array)
            {
                foreach (var token in (JArray)jt)
                {
                    if (token is JObject jo)
                    {
                        result.Add(jo.ToObject<Dictionary<string, object>>());
                    }
                }
                return result;
            }

            if (obj is List<object> objList)
            {
                foreach (var item in objList)
                {
                    switch (item)
                    {
                        case Dictionary<string, object> dict:
                            result.Add(dict);
                            break;
                        case JObject jo:
                            result.Add(jo.ToObject<Dictionary<string, object>>());
                            break;
                        case JToken token when token.Type == JTokenType.Object:
                            result.Add(((JObject)token).ToObject<Dictionary<string, object>>());
                            break;
                    }
                }
                return result;
            }

            if (obj is Dictionary<string, object> singleDict)
            {
                result.Add(singleDict);
                return result;
            }

            try
            {
                var jo = JObject.FromObject(obj);
                if (jo.Type == JTokenType.Object)
                {
                    result.Add(jo.ToObject<Dictionary<string, object>>());
                }
            }
            catch
            {
                Debug.LogError($"GetListFromDict: Failed to convert object to JObject for key '{key}'.");
            }

            return result;
        }
        public static Dictionary<string, object> GetListItemAsDictionary(object obj)
        {
            if (obj is JArray jArray)
            {
                return jArray.ToObject<Dictionary<string, object>>();
            }

            if (obj is Dictionary<string, object> dict)
            {
                return dict;
            }

            return new Dictionary<string, object>();
        }
        public static Dictionary<string, T> GetAsDictionary<T>(Dictionary<string, object> jsonDataDict, string key)
        {
            if (jsonDataDict.TryGetValue(key, out var obj))
            {
                // Convert from JObject if necessary
                if (obj is JObject jObj)
                {
                    return jObj.ToObject<Dictionary<string, T>>();
                }

                // Try direct cast
                if (obj is Dictionary<string, T> dict)
                {
                    return dict;
                }
            }

            // Fallback to empty dictionary
            return new Dictionary<string, T>();
        }
        public static Dictionary<string, object> ConvertObjectToDictionary(object input)
        {
            if (input is Dictionary<string, object> dict)
            {
                return dict;
            }

            if (input is JObject jobj)
            {
                return jobj.ToObject<Dictionary<string, object>>();
            }

            if (input is JToken jtoken && jtoken.Type == JTokenType.Object)
            {
                return ((JObject)jtoken).ToObject<Dictionary<string, object>>();
            }

            // Fallback: serialize-anything → Dictionary<string,object>
            var json = JsonConvert.SerializeObject(input);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }



    /////////////Classes for general Mesh Support.///////////////
    public class CompasMesh
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

        //TODO: ADDED FOR EASE DO NOT KNOW IF I NEED THEM...
        private int[] tris { get; set; }
        public Vector3[] normals { get; set; }
        public Vector2[] uv { get; set; }

        public CompasMesh(
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

        public static CompasMesh Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            return FromData(jsonDataDict);
        }

        public static CompasMesh FromData(Dictionary<string, object> jsonDataDict)
        {
            if (jsonDataDict == null)
            {
                throw new ArgumentNullException(nameof(jsonDataDict), "Input data cannot be null.");
            }

            Dictionary<string, object> dataDictionary = DictionaryHelpers.GetAsDictionary(jsonDataDict, "data"); //TODO: This is only needed when it comes from dumping a mesh directly using json_dump
            if (dataDictionary == null || dataDictionary.Count == 0)
            {
                dataDictionary = jsonDataDict; //TODO: This will cause errors when loading directly from data...
            }
            else
            {
                Debug.Log("JOE LOOK FOR ME HERE dataDictionary" + JsonConvert.SerializeObject(dataDictionary));
            }

            var attributes = DictionaryHelpers.GetSafeDictionary(dataDictionary, "attributes");
            var defaultEdgeAttributes = DictionaryHelpers.GetSafeDictionary(dataDictionary, "default_edge_attributes");
            var defaultFaceAttributes = DictionaryHelpers.GetSafeDictionary(dataDictionary, "default_face_attributes");
            var defaultVertexAttributes = DictionaryHelpers.GetSafeDictionary(dataDictionary, "default_vertex_attributes");
            // var faces = GetSafeDictionary<int[]>(dataDictionary, "face");
            Dictionary<string, object> faces = DictionaryHelpers.GetAsDictionary(dataDictionary, "face");

            Dictionary<string, int[]> facesDict = new Dictionary<string, int[]>();
            foreach (var kvp in faces)
            {
                if (kvp.Value is JArray jArray)
                {
                    int[] faceArray = jArray.ToObject<int[]>();
                    facesDict[kvp.Key] = faceArray;
                }
                else
                {
                    Debug.LogWarning($"Mesh.FromData: Invalid face data for key {kvp.Key}. Expected JArray.");
                }
            }

            Debug.Log("JOE LOOK FOR ME HERE faces" + JsonConvert.SerializeObject(facesDict));
            var faceData = DictionaryHelpers.GetSafeDictionary(dataDictionary, "face_data");

            int maxFace = dataDictionary.TryGetValue("max_face", out var maxFaceObj) ? Convert.ToInt32(maxFaceObj) : 0;
            int maxVertex = dataDictionary.TryGetValue("max_vertex", out var maxVertexObj) ? Convert.ToInt32(maxVertexObj) : 0;
            Debug.Log("JOE LOOK FOR ME HERE maxFace" + JsonConvert.SerializeObject(maxFaceObj));
            Debug.Log("JOE LOOK FOR ME HERE maxVertex" + JsonConvert.SerializeObject(maxVertexObj));

            Dictionary<string, object> vertexDataDict = DictionaryHelpers.GetAsDictionary(dataDictionary, "vertex");
            Dictionary<string, Vertex> vertex = new Dictionary<string, Vertex>();

            if (vertexDataDict.Count == 0)
            {
                Debug.LogWarning("Mesh.FromData: Vertex data not found or invalid. Setting to empty dictionary.");
            }
            else
            {
                foreach (var kvp in vertexDataDict)
                {
                    Dictionary<string, object> individualVertexData = DictionaryHelpers.GetAsDictionary(vertexDataDict, kvp.Key.ToString());
                    if (individualVertexData is Dictionary<string, object>)
                    {
                        double x = DataConverters.ConvertNumericDataToDouble(individualVertexData["x"]);
                        double y = DataConverters.ConvertNumericDataToDouble(individualVertexData["y"]);
                        double z = DataConverters.ConvertNumericDataToDouble(individualVertexData["z"]);
                        vertex[kvp.Key] = new Vertex(x, y, z);
                    }
                    else
                    {
                        Debug.LogWarning($"Mesh.FromData: Invalid vertex data for key {kvp.Key}. Expected Dictionary<string, object>.");
                    }
                }
            }
            return new CompasMesh(attributes, defaultEdgeAttributes, defaultFaceAttributes, defaultVertexAttributes, facesDict, faceData, maxFace, maxVertex, vertex);
        }

        public void CalculateTriangles()
        {
            List<int> triangles = new List<int>();

            foreach (var face in Faces)
            {
                // Assuming each face is a triangle (consists of three vertices)
                if (face.Value.Length != 3)
                {
                    throw new System.InvalidOperationException("Each face must have exactly 3 vertices to calculate triangles.");
                }

                triangles.AddRange(face.Value);
            }

            tris = triangles.ToArray();
        }
        public GameObject GenerateMeshFromRHMesh(string? meshName = "CompasMesh")
        {
            if (Vertex == null || Faces == null)
            {
                Debug.LogError("Vertices or faces data is missing.");
                return null;
            }

            // Convert vertices to Unity Vector3 format
            Vector3[] vertices = new Vector3[Vertex.Count];
            for (int i = 0; i < Vertex.Count; i++)
            {

                Vector3 rhVec = ObjectTransformations.GetPositionFromRightHand(Vertex[i.ToString()].Values);
                vertices[i] = rhVec;
            }

            // Calculate triangles if not already done
            if (tris == null || tris.Length == 0)
            {
                CalculateTriangles();
            }

            // Create the new mesh
            UnityEngine.Mesh mesh = new UnityEngine.Mesh
            {
                name = meshName,
                vertices = vertices,
                triangles = this.tris,
                normals = this.normals,
                uv = this.uv
            };

            // Recalculate bounds and normals if not provided
            if (this.normals == null || this.normals.Length == 0)
            {
                mesh.RecalculateNormals();
            }
            mesh.RecalculateBounds();

            // Create a new game object and add necessary components
            GameObject meshObject = new GameObject(meshName);
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            // Assign the mesh to the mesh filter
            meshFilter.mesh = mesh;

            return meshObject;
        }

        public GameObject GenerateMeshFromRHMeshDebug()
        {
            Debug.Log("GenerateMeshFromRHMeshDebug: Starting mesh generation...");

            if (Vertex == null || Vertex.Count == 0)
            {
                Debug.LogError("GenerateMeshFromRHMeshDebug: Vertex dictionary is null or empty.");
                return null;
            }

            if (Faces == null || Faces.Count == 0)
            {
                Debug.LogError("GenerateMeshFromRHMeshDebug: Faces dictionary is null or empty.");
                return null;
            }

            Debug.Log($"GenerateMeshFromRHMeshDebug: Found {Vertex.Count} vertices and {Faces.Count} faces.");

            // Sample log the first 3 vertices
            for (int i = 0; i < Mathf.Min(3, Vertex.Count); i++)
            {
                string key = i.ToString();
                if (Vertex.ContainsKey(key))
                {
                    Debug.Log($"Vertex[{key}] = ({Vertex[key].X}, {Vertex[key].Y}, {Vertex[key].Z})");
                }
                else
                {
                    Debug.LogWarning($"Vertex key '{key}' not found in Vertex dictionary.");
                }
            }

            // Sample log the first 3 faces
            int faceIndex = 0;
            foreach (var face in Faces)
            {
                Debug.Log($"GenerateMeshFromRHMeshDebug: Face[{face.Key}] = {string.Join(", ", face.Value)}");
                faceIndex++;
                if (faceIndex >= 3) break;
            }

            // Convert vertices to Unity Vector3 format
            Vector3[] vertices = new Vector3[Vertex.Count];
            for (int i = 0; i < Vertex.Count; i++)
            {
                string key = i.ToString();
                if (!Vertex.ContainsKey(key))
                {
                    Debug.LogError($"GenerateMeshFromRHMeshDebug: Missing vertex with key {key}");
                    return null;
                }

                Vector3 rhVec = ObjectTransformations.GetPositionFromRightHand(Vertex[key].Values);
                vertices[i] = rhVec;
            }

            // Calculate triangles if not already done
            if (tris == null || tris.Length == 0)
            {
                try
                {
                    CalculateTriangles();
                    Debug.Log($"GenerateMeshFromRHMeshDebug: Triangles calculated. Count: {tris.Length / 3} faces.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"GenerateMeshFromRHMeshDebug: Error calculating triangles: {ex.Message}");
                    return null;
                }
            }

            UnityEngine.Mesh mesh = new UnityEngine.Mesh
            {
                name = "TESTING OBJECT",
                vertices = vertices,
                triangles = this.tris,
                normals = this.normals,
                uv = this.uv
            };

            if (this.normals == null || this.normals.Length == 0)
            {
                Debug.Log("GenerateMeshFromRHMeshDebug: Recalculating normals...");
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            Debug.Log("GenerateMeshFromRHMeshDebug: Mesh bounds recalculated.");

            // Create GameObject
            GameObject meshObject = new GameObject("TESTING OBJECT");
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = mesh;

            // Optional sanity cube
            GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.transform.position = Vector3.zero;
            debugCube.name = "DEBUG CUBE - Scene Check";

            Debug.Log("GenerateMeshFromRHMesh: Mesh generation complete.");

            return meshObject;
        }

    }

    public class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public float[] Values { get; set; }

        public Vertex(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            Values = new float[] { (float)x, (float)y, (float)z };
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


    /////////////Classes for Assembly Desearialization./////////////// 
    [System.Serializable]
    public class Node
    {
        /*
        * Node : A class to define the structure of a node in the assembly data structure.
        * This class is used to define the structure of a node in the assembly data structure.
        * It is based off the Compas data structure for a node.
        */
        public Part part { get; set; }
        public string type_data { get; set; }
        public string type_id { get; set; }
        public Attributes attributes { get; set; }
        public static Node Parse(string key, object jsondata)
        {
            /*
            * Method to create an instance of a the Node class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            Node node = FromData(jsonDataDict, key);
            return node;
        }
        public static Node FromData(Dictionary<string, object> jsonDataDict, string key)
        {
            /*
            * Method to create an instance of a the Node class from a dictionary.
            */
            Node node = new Node();
            node.part = new Part();
            node.attributes = new Attributes();
            node.type_id = key;
            DtypeGeometryDesctiptionSelector(node, jsonDataDict);
            return node;
        }
        private static void DtypeGeometryDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to select the correct desearialization method based on the dtype of the part in the assembly.
            * It is used to parse the data from the dictionary and set the values of the node class.
            */

            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            string dtype = (string)partDict["dtype"];

            switch (dtype)
            {
                case "compas.geometry/Cylinder":

                    node.part.frame = Frame.Parse(dataDict["frame"]);
                    node.part.dtype = dtype;

                    float height = Convert.ToSingle(dataDict["height"]);
                    float radius = Convert.ToSingle(dataDict["radius"]);
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;

                    break;

                case "compas.geometry/Box":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(dataDict["frame"]);

                    float xsize = Convert.ToSingle(dataDict["xsize"]);
                    float ysize = Convert.ToSingle(dataDict["ysize"]);
                    float zsize = Convert.ToSingle(dataDict["zsize"]);
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;

                    break;

                case "compas.geometry/Frame":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.FromData(dataDict);

                    //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas.datastructures/Mesh":

                    node.part.dtype = dtype;

                    Dictionary<string, object> frameDict;
                    if (jsonDataDict.TryGetValue("frame", out object frameObject))
                    {
                        frameDict = jsonDataDict["frame"] as Dictionary<string, object>;
                        Dictionary<string, object> frameDataDict = frameDict["data"] as Dictionary<string, object>;
                        node.part.frame = Frame.FromData(frameDataDict);
                    }
                    else
                    {
                        node.part.frame = Frame.RhinoWorldXY();
                    }

                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas_timber.parts/Beam":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(dataDict["frame"]);

                    float objLength = Convert.ToSingle(dataDict["length"]);
                    float objWidth = Convert.ToSingle(dataDict["width"]);
                    float objHeight = Convert.ToSingle(dataDict["height"]);
                    node.attributes.length = objLength;
                    node.attributes.width = objWidth;
                    node.attributes.height = objHeight;

                    break;

                case string connectionType when connectionType.StartsWith("compas_timber.connections"):
                    //TODO: Set dtype to only compas_timber.connections so It can be checked in valid node without .StartsWith
                    node.part.dtype = "compas_timber.connections";
                    break;

                case "compas.datastructures/Part":
                    PartDesctiptionSelector(node, jsonDataDict);
                    break;

                default:
                    Debug.LogError($"DtypeGeometryDesctiptionSelector: No Deserilization type for dtype {dtype}.");
                    break;
            }
        }
        private static void PartDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to select the correct desearialization method based on the dtype of the part in the assembly.
            * It is used to parse the data from the dictionary and set the values of the node class.
            * This method is specifically used to parse the data for the part dtype "compas.datastructures/Part".
            */
            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            Dictionary<string, object> attributesDict = dataDict["attributes"] as Dictionary<string, object>;
            Dictionary<string, object> shapeDict = attributesDict["shape"] as Dictionary<string, object>;
            Dictionary<string, object> shapeDataDict = shapeDict["data"] as Dictionary<string, object>;
            string dtype = (string)shapeDict["dtype"];

            switch (dtype)
            {
                case "compas.geometry/Cylinder":

                    node.part.frame = Frame.Parse(shapeDataDict["frame"]);
                    node.part.dtype = dtype;

                    float height = Convert.ToSingle(shapeDataDict["height"]);
                    float radius = Convert.ToSingle(shapeDataDict["radius"]);
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;

                    break;

                case "compas.geometry/Box":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(shapeDataDict["frame"]);

                    float xsize = Convert.ToSingle(shapeDataDict["xsize"]);
                    float ysize = Convert.ToSingle(shapeDataDict["ysize"]);
                    float zsize = Convert.ToSingle(shapeDataDict["zsize"]);
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;

                    break;

                case "compas.datastructures/Mesh":

                    node.part.dtype = dtype;

                    Dictionary<string, object> frameDict;
                    if (jsonDataDict.TryGetValue("frame", out object frameObject))
                    {
                        frameDict = jsonDataDict["frame"] as Dictionary<string, object>;
                        Dictionary<string, object> frameDataDict = frameDict["data"] as Dictionary<string, object>;
                        node.part.frame = Frame.FromData(frameDataDict);
                    }
                    else
                    {
                        node.part.frame = Frame.RhinoWorldXY();
                    }

                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas.geometry/Frame":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.FromData(shapeDataDict);

                    //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    if (attributesDict.TryGetValue("name", out object name))
                    {
                        string nameString = name.ToString();
                        if (nameString.StartsWith("QR_"))
                        {
                            node.part.dtype = "compas_xr/QRCode";
                        }
                    }

                    break;

                default:
                    Debug.LogError($"PartDesctiptionSelector: No Part Deserilization type for dtype {dtype}.");
                    break;

            }
        }
        public bool IsValidNode()
        {
            /*
            * Method to check if the node contains all valid information.
            */
            if (!string.IsNullOrEmpty(type_id) &&
                !string.IsNullOrEmpty(part.dtype) &&
                part != null &&
                part.frame != null)
            {
                if (part.dtype == "compas_timber.connections")
                {
                    Debug.Log("This is a timbers Joint and should be ignored");
                    return false;
                }
                else if (part.dtype != "compas.geometry/Frame" ||
                        part.dtype != "compas.datastructures/Mesh" ||
                        part.dtype != "compas_xr/QRCode")
                {
                    if (attributes != null &&
                        attributes?.length != null &&
                        attributes?.width != null &&
                        attributes?.height != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    public class Part
    {
        /*
        * Part : A class to define the structure of a part in the assembly data structure.
        * It is based off the Compas data structure for a part, but not a direct corelation.
        */
        public Frame frame { get; set; }
        public string dtype { get; set; }

    }

    [System.Serializable]
    public class Attributes
    {
        /*
        * Attributes : A class to define the attributes of the node item.
        */
        public float length { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    }

    [System.Serializable]
    public class Frame
    {
        /*
        * Frame : A class to define the structure of a frame in the assembly data structure.
        * It is based off the Compas data structure for a frame.
        */
        public float[] point { get; set; }
        public float[] xaxis { get; set; }
        public float[] yaxis { get; set; }

        public static Frame Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Frame class from a json string.
            */
            Dictionary<string, object> frameDataDict = jsondata as Dictionary<string, object>; ;
            return FromData(frameDataDict);
        }
        public static Frame FromData(Dictionary<string, object> frameDataDict)
        {
            /*
            * Method to create an instance of a the Frame class from a dictionary.
            */
            Frame frame = new Frame();
            Debug.Log("FrameParse: Parsing Frame Data " + JsonConvert.SerializeObject(frameDataDict));
            float[] point = DataConverters.ConvertDatatoFloatArray(frameDataDict["point"]);
            float[] xaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["xaxis"]);
            float[] yaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["yaxis"]);

            if (point == null || xaxis == null || yaxis == null)
            {
                Debug.LogError("FrameParse: One or more arrays is null.");
            }
            else
            {
                frame.point = point;
                frame.xaxis = xaxis;
                frame.yaxis = yaxis;
            }

            return frame;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method to return the frame data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "point", point },
                { "xaxis", xaxis },
                { "yaxis", yaxis }
            };
        }
        public static Frame RhinoWorldXY()
        {
            /*
            * Returns a frame that represents the world XY plane in Rhino coordinates.
            */
            Frame frame = new Frame();
            frame.point = new float[] { 0.0f, 0.0f, 0.0f };
            frame.xaxis = new float[] { 1.0f, 0.0f, 0.0f };
            frame.yaxis = new float[] { 0.0f, 1.0f, 0.0f };
            return frame;
        }

        //TODO: Robotic Territories Testing ///////////////////////////////////////////////////////////////////////////////////
        public static List<Frame> _parseFramesData(List<Dictionary<string, object>> framesData)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<Frame> frames = new List<Frame>();
            foreach (Dictionary<string, object> frameData in framesData)
            {
                frames.Add(Frame.Parse(JsonConvert.SerializeObject(frameData)));
            }
            return frames;
        }
        public bool IsSameAs(Frame other, float posTolerance = 1e-4f, float axisToleranceDeg = 0.5f)
        {
            if (other == null) return false;

            // Compare point positions
            if (!AreVectorsClose(this.point, other.point, posTolerance)) return false;

            // Compare axes by angle
            if (!AreAxesClose(this.xaxis, other.xaxis, axisToleranceDeg)) return false;
            if (!AreAxesClose(this.yaxis, other.yaxis, axisToleranceDeg)) return false;

            return true;
        }
        private bool AreVectorsClose(float[] a, float[] b, float tol)
        {
            if (a == null || b == null || a.Length < 3 || b.Length < 3) return false;
            float dx = a[0] - b[0];
            float dy = a[1] - b[1];
            float dz = a[2] - b[2];
            return (dx * dx + dy * dy + dz * dz) <= tol * tol;
        }
        private bool AreAxesClose(float[] a, float[] b, float angTolDeg)
        {
            if (a == null || b == null || a.Length < 3 || b.Length < 3) return false;
            var A = new UnityEngine.Vector3(a[0], a[1], a[2]).normalized;
            var B = new UnityEngine.Vector3(b[0], b[1], b[2]).normalized;
            if (A.sqrMagnitude < 1e-12f || B.sqrMagnitude < 1e-12f) return false;

            float dot = Mathf.Clamp(Vector3.Dot(A, B), -1f, 1f);
            float ang = Mathf.Acos(dot) * Mathf.Rad2Deg;
            return ang <= angTolDeg;
        }

        //TODO: Robotic Territories Testing ///////////////////////////////////////////////////////////////////////////////////

    }

    /////////////// Classes For Building Plan Desearialization///////////////////

    [System.Serializable]
    public class BuildingPlanData
    {
        /*
        * BuildingPlanData : A class to define the structure of a building plan in the assembly data structure.
        * It is based off the Compas data structure for a building plan.
        * The building plan contains a dictionary of steps required for assembly, and the last built index.
        */
        public string LastBuiltIndex { get; set; }
        public Dictionary<string, Step> steps { get; set; }
        public Dictionary<string, List<string>> PriorityTreeDictionary { get; set; }
        public static BuildingPlanData Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the BuildingPlanData class from a json
            * Returns BuildingPlanData Class
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            BuildingPlanData buildingPlanData = BuildingPlanData.FromData(jsonDataDict);
            return buildingPlanData;
        }
        public static BuildingPlanData FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the BuildingPlanData class from a dictionary.
            * Returns BuildingPlan Class instance & PriorityTreeDictionary
            */
            BuildingPlanData buidingPlanData = new BuildingPlanData();
            buidingPlanData.steps = new Dictionary<string, Step>();
            buidingPlanData.PriorityTreeDictionary = new Dictionary<string, List<string>>();
            if (jsonDataDict.TryGetValue("LastBuiltIndex", out object last_built_index))
            {
                Debug.Log($"Last Built Index Fetched From database: {last_built_index.ToString()}");
                buidingPlanData.LastBuiltIndex = last_built_index.ToString();
            }
            else
            {
                buidingPlanData.LastBuiltIndex = null;
            }
            List<object> stepsList = jsonDataDict["steps"] as List<object>;
            for (int i = 0; i < stepsList.Count; i++)
            {
                string key = i.ToString();
                var json_data = stepsList[i];
                Step step_data = Step.Parse(json_data);

                if (step_data.IsValidStep())
                {
                    buidingPlanData.steps[key] = step_data;
                    Debug.Log($"FromData: BuildingPlan Step {key} successfully added to the building plan dictionary");

                    if (buidingPlanData.PriorityTreeDictionary.ContainsKey(step_data.data.priority.ToString()))
                    {
                        buidingPlanData.PriorityTreeDictionary[step_data.data.priority.ToString()].Add(key);
                    }
                    else
                    {
                        buidingPlanData.PriorityTreeDictionary[step_data.data.priority.ToString()] = new List<string>();
                        buidingPlanData.PriorityTreeDictionary[step_data.data.priority.ToString()].Add(key);
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
                }
            }
            return buidingPlanData;
        }
    }

    [System.Serializable]
    public class Step
    {
        /*
        * Step : A class to define the structure of a step in the building plan data structure.
        * It is based off the Compas data structure for a step.
        * The step contains the data required for a single step of the building process
        */
        public Data data { get; set; }
        public string dtype { get; set; }
        public string guid { get; set; }

        public static Step Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Step class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Step FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the Step class from a dictionary.
            */
            Step step = new Step();
            step.dtype = (string)jsonDataDict["dtype"];
            step.guid = (string)jsonDataDict["guid"];

            Dictionary<string, object> dataDict = jsonDataDict["data"] as Dictionary<string, object>;
            step.data = Data.FromData(dataDict);
            return step;
        }
        public static bool AreEqualSteps(Step step, Step NewStep)
        {
            /*
            * Method to compare two steps and check if they are equal.
            */
            if (step != null &&
                NewStep != null &&
                step.data.device_id == NewStep.data.device_id &&
                step.data.element_ids == step.data.element_ids &&
                step.data.actor == NewStep.data.actor &&
                step.data.location.point.SequenceEqual(NewStep.data.location.point) &&
                step.data.location.xaxis.SequenceEqual(NewStep.data.location.xaxis) &&
                step.data.location.yaxis.SequenceEqual(NewStep.data.location.yaxis) &&
                step.data.geometry == NewStep.data.geometry &&
                step.data.is_built == NewStep.data.is_built &&
                step.data.is_planned == NewStep.data.is_planned &&
                step.data.priority == NewStep.data.priority)
            {
                return true;
            }
            return false;
        }
        public bool IsValidStep()
        {
            /*
            * Method to check if the step contains all valid information.
            */
            if (data != null &&
                data.element_ids != null &&
                !string.IsNullOrEmpty(data.actor) &&
                data.location != null &&
                data.geometry != null &&
                data.is_built != null &&
                data.is_planned != null &&
                data.priority != null)
            {
                return true;
            }
            return false;
        }

    }

    [System.Serializable]
    public class Data
    {
        /*
        * Data : A class to define the structure of a data in the building plan data structure.
        * data contains the information required for coordinating the building process
        */
        public string device_id { get; set; }
        public string[] element_ids { get; set; }
        public string actor { get; set; }
        public Frame location { get; set; }
        public string geometry { get; set; }
        public bool is_built { get; set; }
        public bool is_planned { get; set; }
        public int priority { get; set; }

        public static Data Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Data class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Data FromData(Dictionary<string, object> dataDict)
        {
            /*
            * Method to create an instance of a the Data class from a dictionary.
            */
            Data data = new Data();
            Dictionary<string, object> locationDataDict = dataDict["location"] as Dictionary<string, object>;
            data.location = Frame.FromData(locationDataDict);

            if (dataDict.TryGetValue("device_id", out object device_id))
            {
                data.device_id = device_id.ToString();
            }
            else
            {
                data.device_id = null;
            }
            data.actor = (string)dataDict["actor"];
            data.geometry = (string)dataDict["geometry"];
            data.is_built = (bool)dataDict["is_built"];
            data.is_planned = (bool)dataDict["is_planned"];
            data.priority = (int)(long)dataDict["priority"];

            List<object> element_ids = dataDict["element_ids"] as List<object>;
            if (element_ids != null)
            {
                data.element_ids = element_ids.Select(x => x.ToString()).ToArray();
            }
            else
            {
                Debug.Log("FromData (Data): Element_ID's list is empty.");
            }
            return data;
        }

    }

    ////////////////Classes for User Current Informatoin/////////////////////

    [System.Serializable]
    public class UserCurrentInfo
    {
        /*
        * UserCurrentInfo : A class to define the structure of a user current information in the building plan data structure.
        * UserCurrentInfo contains the information required for tracking multiple users across the building process
        */
        public string currentStep { get; set; }
        public string timeStamp { get; set; }
        public static UserCurrentInfo Parse(object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static UserCurrentInfo FromData(Dictionary<string, object> jsonDataDict)
        {
            //Create class instances of node elements
            UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
            userCurrentInfo.currentStep = (string)jsonDataDict["currentStep"];
            userCurrentInfo.timeStamp = (string)jsonDataDict["timeStamp"];
            return userCurrentInfo;
        }

    }

    [System.Serializable]
    public class UserZoneInfo
    {
        public bool IsPerforming { get; set; }
        public string CurrentZone { get; set; }
        public List<string> CurrentSelectedRobots { get; set; }
        public static UserZoneInfo Parse(object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public Dictionary<string, object> GetData()
        {
            //Convert the UserZoneInfo class to a dictionary
            return new Dictionary<string, object>
            {
                { "CurrentZone", CurrentZone },
                { "CurrentSelectedRobots", CurrentSelectedRobots }
            };
        }

        public static UserZoneInfo FromData(Dictionary<string, object> jsonDataDict)
        {
            //Create class instances of node elements
            UserZoneInfo userZoneInfo = new UserZoneInfo();
            userZoneInfo.CurrentZone = (string)jsonDataDict["CurrentZone"];
            userZoneInfo.CurrentSelectedRobots = jsonDataDict["CurrentSelectedRobots"] as List<string>;
            return userZoneInfo;
        }
    }


}