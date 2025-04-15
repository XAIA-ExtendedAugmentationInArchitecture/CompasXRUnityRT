using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using CompasXR.Core.Data;
using System.IO;


//EVA //////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.MiniJSON;
using CompasXR.Core;

public class MeshTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // string filePath = @"C:\Users\jk6372\Desktop\00_princeton_projects\00_robotic_territories\02_Production\02_Unity\01_mesh_parsing_tests\box_mesh.json";
        // string filePath = @"C:\Users\jk6372\Desktop\00_princeton_projects\00_robotic_territories\02_Production\02_Unity\01_mesh_parsing_tests\box_tri_mesh.json";
        // string filePath = @"C:\Users\jk6372\Desktop\00_princeton_projects\00_robotic_territories\02_Production\02_Unity\01_mesh_parsing_tests\mesh_tri.json";
        // string filePath = @"C:\Users\jk6372\Desktop\00_princeton_projects\00_robotic_territories\02_Production\02_Unity\01_mesh_parsing_tests\mesh.json";
        string filePath = @"C:\Users\jk6372\Desktop\00_princeton_projects\00_robotic_territories\02_Production\02_Unity\01_mesh_parsing_tests\trajectory_acm_test_detailed.json";

        if (File.Exists(filePath))
        {
            // Read the JSON file
            string jsonText = File.ReadAllText(filePath);

            Debug.Log("JOE LOOK FOR ME" + jsonText);
            
            /////////////////////////////////////////////// MESH TESTING /////////////////////////////////////////////////////////////////////
            // Parse JSON into a Dictionary
            // Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);

            // Print to console
            // Debug.Log(JsonConvert.SerializeObject(jsonData, Formatting.Indented));

            
            // Mesh mesh = Mesh.Parse(jsonText);
            // mesh.GenerateMeshFromRHMesh();
            /////////////////////////////////////////////// MESH TESTING /////////////////////////////////////////////////////////////////////
            
            /////////////////////////////////////////////// ACM TESTING /////////////////////////////////////////////////////////////////////
            var rootDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);
            var dataDict = ((JObject)rootDict["data"]).ToObject<Dictionary<string, object>>();
            var acmListRaw = (JArray)dataDict["attached_collision_meshes"];

            foreach (var item in acmListRaw)
            {
                string acmJson = item.ToString();
                AttachedCollisionMesh acm = AttachedCollisionMesh.Parse(acmJson);

                Debug.Log("JOE LOOK FOR ME Parsed Link Name: " + acm.LinkName);
                Debug.Log("JOE LOOK FOR ME Parsed Collision Mesh ID: " + acm.CollisionMesh.Id);
                Debug.Log("JOE LOOK FOR ME Parsed Collision Mesh Root Name: " + acm.CollisionMesh.RootName);
                Debug.Log("JOE LOOK FOR ME Parsed Collision Mesh Frame: " + acm.CollisionMesh.Frame.GetData());
                Debug.Log("JOE LOOK FOR ME Parsed Collision Mesh Vertices: " + acm.CollisionMesh.Mesh.Vertex.Count);

                acm.CollisionMesh.Mesh.GenerateMeshFromRHMesh();

            }

            /////////////////////////////////////////////// ACM TESTING /////////////////////////////////////////////////////////////////////

        }
        else
        {
            Debug.LogError("JSON file not found: " + filePath);
        }
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

        public static AttachedCollisionMesh Parse(string jsonData)
        {
            Dictionary<string, object> jsonDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            Debug.Log("JOE LOOK FOR ME HERE jsonDataDict" + JsonConvert.SerializeObject(jsonDataDict));
            if (jsonDataDict == null)
            {
                throw new ArgumentNullException(nameof(jsonDataDict), "Input data cannot be null.");
            }
            return FromData(jsonDataDict);
        }

        public static AttachedCollisionMesh FromData(Dictionary<string, object> jsonDataDict)
        {
            Dictionary<string, object> collisionMeshDict = DictionaryHelpers.GetAsDictionary(jsonDataDict, "collision_mesh");
            CollisionMesh collisionMesh = CollisionMesh.FromData(collisionMeshDict);
            string linkName = jsonDataDict["link_name"] as string;
            List<string> touchLinks = jsonDataDict["touch_links"] as List<string>;
            double weight = Convert.ToDouble(jsonDataDict["weight"]);

            return new AttachedCollisionMesh(collisionMesh, linkName, touchLinks, weight);
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
            Debug.Log("JOSEEPHHHH" + JsonConvert.SerializeObject(jsonDataDict));
            Debug.Log("JOSEEPHHHH" + jsonDataDict.GetType());
            var frameDict = DictionaryHelpers.GetAsDictionary(jsonDataDict, "frame");
            Frame frame = Frame.FromData(frameDict);
            string id = jsonDataDict["id"] as string;
            Dictionary<string, object> meshDict = DictionaryHelpers.GetAsDictionary(jsonDataDict, "mesh");
            Debug.Log("JOSEEPHHHH MESH DICT" + JsonConvert.SerializeObject(meshDict));

            Mesh mesh = Mesh.FromData(meshDict);
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
        
        //TODO: ADDED FOR EASE DO NOT KNOW IF I NEED THEM...
        private int[] tris { get; set; }
        public Vector3[] normals { get; set; }
        public Vector2[] uv { get; set; }

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
            return new Mesh(attributes, defaultEdgeAttributes, defaultFaceAttributes, defaultVertexAttributes, facesDict, faceData, maxFace, maxVertex, vertex);
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


        public GameObject GenerateMeshFromRHMesh()
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
                name = "TESTING OBJECT",
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
            GameObject meshObject = new GameObject("TESTING OBJECT");
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            // Assign the mesh to the mesh filter
            meshFilter.mesh = mesh;

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
            if (jsonDataDict.TryGetValue(key, out var obj))
            {
                if (obj is JObject jObj)
                {
                    return jObj.ToObject<Dictionary<string, object>>();
                }

                if (obj is Dictionary<string, object> dict)
                {
                    return dict;
                }
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

    }
