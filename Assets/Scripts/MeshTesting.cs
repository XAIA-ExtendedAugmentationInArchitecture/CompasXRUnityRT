using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using CompasXR.Core.Data;


//EVA //////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class MeshTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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


//EVA //////////////////////////////////////////////////////////////////////////////////


namespace MeshElementData
{
    [System.Serializable]
    public class Result
    {
        public string result { get; set; }
    }
    [System.Serializable]
    public class Dimensions
    {
        public float width { get; set; }
        public float height { get; set; }
        public float length { get; set; }
    }
    
    [System.Serializable]
    public class MeshData
    {
        public string name { get; set; }
        public Dimensions dimensions { get; set; }
        public float[] color { get; set; }

        [JsonConverter(typeof(IntArrayArrayConverter))]
        public int[][] faces { get; set; }

        [JsonConverter(typeof(Vector3ArrayConverter))]
        public Vector3[] vertices { get; set; }

        [JsonConverter(typeof(Vector3ArrayConverter))]
        public Vector3[] normals { get; set; }

        [JsonConverter(typeof(Vector2ArrayConverter))]
        public Vector2[] uv { get; set; }

        private int[] tris { get; set; }

        // Method to calculate triangles of the mesh
        public void CalculateTriangles()
        {
            List<int> triangles = new List<int>();

            foreach (var face in faces)
            {
                // Assuming each face is a triangle (consists of three vertices)
                if (face.Length != 3)
                {
                    throw new System.InvalidOperationException("Each face must have exactly 3 vertices to calculate triangles.");
                }

                triangles.AddRange(face);
            }

            tris = triangles.ToArray();
        }


        // Method to generate a mesh and attach it to a new game object
        public GameObject GenerateMesh()
        {
            if (vertices == null || faces == null)
            {
                Debug.LogError("Vertices or faces data is missing.");
                return null;
            }

            // Calculate triangles if not already done
            if (tris == null || tris.Length == 0)
            {
                CalculateTriangles();
            }

            // Create the new mesh
            Mesh mesh = new Mesh
            {
                name = this.name,
                vertices = this.vertices,
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
            GameObject meshObject = new GameObject(this.name);
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            // Assign the mesh to the mesh filter
            meshFilter.mesh = mesh;

            return meshObject;
        }

        // Method to assign a material to the generated game object
        public void AssignMaterial(GameObject meshObject, Material material)
        {
            if (meshObject == null)
            {
                Debug.LogError("The provided game object is null.");
                return;
            }

            MeshRenderer meshRenderer = meshObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }
            else
            {
                Debug.LogError("The game object does not have a MeshRenderer component.");
            }
        }
    
    }
}

/*
    Author: MESH AG - Eleni Vasiliki Alexi
    License: MIT
    Created on: 14/02/2024 12:00
    Updated on: 04/06/2024 12:00
    Author: Huma Lab - Eleni Vasiliki Alexi

    This Unity file contains custom JSON converters for int[], Vector3, Vector3[], and Dictionary<string, Vector3[]>.
    These converters are designed to facilitate serialization and deserialization of Vector3 data and int[] data  from JSON format.

    The IntArrayConverter class is responsible for handling: int[], like MeshFaces

    The Vector3Converter class is responsible for handling: Vector3

    The Vector3ArrayConverter class is responsible for handling: Vector3[]

    The Vector3DictionaryConverter class is responsible for handling: Dictionary<string, Vector3[]>

    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    All the converters SWAPS the y and z components of each Vector3 object to convert from a right-handed 
    coordinate system to a left-handed coordinate system.
    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

*/

public class IntArrayConverter : JsonConverter<int[]>
{
    public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        for (int i = 0; i < value.Length; i += 3)
        {
            if (i + 2 >= value.Length)
            {
                throw new JsonException("Invalid int[] length for writing JSON.");
            }
            // Reverse the order to switch the winding order from left-handed to right-handed
            writer.WriteValue(value[i]);
            writer.WriteValue(value[i + 2]);
            writer.WriteValue(value[i + 1]);
        }
        writer.WriteEndArray();
    }

    public override int[] ReadJson(JsonReader reader, Type objectType, int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        List<int> faceList = new List<int>();

        for (int i = 0; i < array.Count; i += 3)
        {
            if (i + 2 >= array.Count)
            {
                throw new JsonException("Invalid JSON array for int[] conversion");
            }

            // Reverse the order to switch the winding order from right-handed to left-handed
            faceList.Add((int)array[i]);
            faceList.Add((int)array[i + 2]);
            faceList.Add((int)array[i + 1]);
        }

        return faceList.ToArray();
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}

public class IntArrayArrayConverter : JsonConverter<int[][]>
{
    public override void WriteJson(JsonWriter writer, int[][] value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var innerArray in value)
        {
            writer.WriteStartArray();
            for (int i = 0; i < innerArray.Length; i += 3)
            {
                if (i + 2 >= innerArray.Length)
                {
                    throw new JsonException("Invalid inner int[] length for writing JSON.");
                }
                // Reverse the order to switch the winding order from left-handed to right-handed
                writer.WriteValue(innerArray[i]);
                writer.WriteValue(innerArray[i + 2]);
                writer.WriteValue(innerArray[i + 1]);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

    public override int[][] ReadJson(JsonReader reader, Type objectType, int[][] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray outerArray = JArray.Load(reader);
        List<int[]> resultList = new List<int[]>();

        foreach (var innerArray in outerArray.Children<JArray>())
        {
            List<int> innerList = new List<int>();
            for (int i = 0; i < innerArray.Count; i += 3)
            {
                if (i + 2 >= innerArray.Count)
                {
                    throw new JsonException("Invalid inner JSON array for int[][] conversion");
                }

                // Reverse the order to switch the winding order from right-handed to left-handed
                innerList.Add((int)innerArray[i]);
                innerList.Add((int)innerArray[i + 2]);
                innerList.Add((int)innerArray[i + 1]);
            }
            resultList.Add(innerList.ToArray());
        }

        return resultList.ToArray();
    }
}

public class Vector2Converter : JsonConverter<Vector2>
{
    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        writer.WriteValue(value.x);
        writer.WriteValue(value.y);
        writer.WriteEndArray();
    }

    public override Vector2 ReadJson(JsonReader reader, System.Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);

        if (array == null || array.Count < 2)
        {
            throw new JsonException("Invalid JSON array for Vector2 conversion");
        }

        return new Vector2((float)array[0], (float)array[1]);
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;
}

public class Vector2ArrayConverter : JsonConverter<Vector2[]>
{
    public override void WriteJson(JsonWriter writer, Vector2[] value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var vector in value)
        {
            writer.WriteStartArray();
            writer.WriteValue(vector.x);
            writer.WriteValue(vector.y);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

    public override Vector2[] ReadJson(JsonReader reader, System.Type objectType, Vector2[] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray outerArray = JArray.Load(reader);
        var resultList = new List<Vector2>();

        foreach (var innerArray in outerArray.Children<JArray>())
        {
            if (innerArray.Count != 2)
            {
                throw new JsonException("Invalid inner JSON array for Vector2 conversion. Each inner array must have exactly 2 elements.");
            }

            resultList.Add(new Vector2((float)innerArray[0], (float)innerArray[1]));
        }

        return resultList.ToArray();
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;
}

public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        writer.WriteValue(value.x);
        writer.WriteValue(value.z);
        writer.WriteValue(value.y);
        writer.WriteEndArray();
    }

    public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);

        if (array == null || array.Count < 3)
        {   
            throw new JsonException("Invalid JSON array for Vector3 conversion");
        }

        // Swapping y and z components to convert from right-handed to left-handed coordinate system
        return new Vector3((float)array[0], (float)array[2], (float)array[1]);
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;
}

public class Vector3ArrayConverter : JsonConverter<Vector3[]>
{
    
    public override void WriteJson(JsonWriter writer, Vector3[] value, JsonSerializer serializer)
    {
        writer.WriteStartArray();

        foreach (Vector3 vector in value)
        {
            writer.WriteStartArray();
            writer.WriteValue(vector.x);
            writer.WriteValue(vector.z);
            writer.WriteValue(vector.y);
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

    public override Vector3[] ReadJson(JsonReader reader, System.Type objectType, Vector3[] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        List<Vector3> vectorList = new();

        foreach (JArray innerArray in array)
        {
            if (innerArray.Count != 3 || innerArray.Any(item => item.Type != JTokenType.Float))
            {
                throw new JsonException("Invalid JSON array for Vector3 array conversion");
            }

            // Swapping y and z components to convert from right-handed to left-handed coordinate system
            vectorList.Add(new Vector3((float)innerArray[0], (float)innerArray[2], (float)innerArray[1]));
        }

        return vectorList.ToArray();
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;
}

public class Vector3DictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Dictionary<string, Vector3[]>));
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Dictionary<string, Vector3[]> dict = (Dictionary<string, Vector3[]>)value;

        writer.WriteStartObject();

        foreach (var pair in dict)
        {
            writer.WritePropertyName(pair.Key);
            writer.WriteStartArray();

            foreach (Vector3 vector in pair.Value)
            {
                writer.WriteStartArray();
                writer.WriteValue(vector.x);
                writer.WriteValue(vector.z);
                writer.WriteValue(vector.y);
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        Dictionary<string, Vector3[]> result = new ();

        foreach (var property in jsonObject.Properties())
        {
            string key = property.Name;
            JArray array = (JArray)property.Value;

            // Swapping y and z components to convert from right-handed to left-handed coordinate system
            Vector3[] vectorArray = array.Select(item => new Vector3((float)item[0],(float)item[2],(float)item[1])).ToArray();

            result.Add(key, vectorArray);
        }
        return result;
    }

    public override bool CanRead => true;

    public override bool CanWrite => true;
}
