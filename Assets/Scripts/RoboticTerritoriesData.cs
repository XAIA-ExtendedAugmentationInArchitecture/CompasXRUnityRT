using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompasXR.Core.Data;
using System.Linq;
using System;
using CompasXR.Core;
using Newtonsoft.Json;

namespace CompasXR.RoboticTerritories.Data
{   
    /*
    */
    public class ProjectZones
    {
        
        public CurrentZoneMode CurrentZone { get; set; }
        public Dictionary<string, Zone> MimicZones { get; set; }
        public Dictionary<string, Zone> InferenceZones { get; set; }
        public Dictionary<string, Zone> TelemimicZones { get; set; }
        public Dictionary<string, Zone> BoundaryZone { get; set; }
        public MimicZoneMode CurrentMimicMode { get; set; }

        public ProjectZones()
        {
            MimicZones = new Dictionary<string, Zone>();
            InferenceZones = new Dictionary<string, Zone>();
            TelemimicZones = new Dictionary<string, Zone>();
            BoundaryZone = new Dictionary<string, Zone>();
            CurrentZone = CurrentZoneMode.None;
            CurrentMimicMode = MimicZoneMode.UserInitiated;
        }

        public void Clear()
        {
            MimicZones.Clear();
            InferenceZones.Clear();
            TelemimicZones.Clear();
            BoundaryZone.Clear();
        }
        public enum CurrentZoneMode
        {
            None,
            Inference,
            Mimic,
            Telemimic,
        }

        public enum MimicZoneMode
        {
            UserInitiated,
            RealtimeMimic
        }
    }

    public class Zone
    {
        public string Name { get; set; }
        public Box Box { get; set; }
        public GameObject ZoneObject { get; set; }
        public Material ZoneActiveMaterial { get; set; }
        public Material ZoneInactiveMaterial { get; set; }

        public static Zone FromData(string Name, Dictionary<string, object> jsonDataDict)
        {
            Zone zone = new Zone();
            zone.Name = Name;
            zone.Box = Box.FromData(jsonDataDict as Dictionary<string, object>);
            // zone.ZoneObject = zone.CreateZoneObject();
            return zone;
        }

        public static Zone Parse(string Name, object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(Name, jsonDataDict);
        }
        public GameObject CreateZoneObject()
        {
            GameObject zoneObject = Box.CreateBoxObject();
            zoneObject.name = Name;    
            return zoneObject;
        }

    }

    [System.Serializable]
    public class ObservedGeometry
    {
        /*
        * ObservedGeometry : A class to define the structure of the observed geometry in the assembly data structure.
        * It is based off the Compas data structure for a geometry.
        */
        public Box Box { get; set; }
        public string MarkerType { get; set; }
        public GameObject GeometryObject { get; set; }

        public static ObservedGeometry Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the ObservedGeometry class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static ObservedGeometry FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the ObservedGeometry class from a dictionary.
            */
            ObservedGeometry observedGeometry = new ObservedGeometry();
            Dictionary<string, object> boxData = jsonDataDict["box"] as Dictionary<string, object>;
            Debug.Log($"ObservedGeometry FromData: {JsonConvert.SerializeObject(boxData)}" );
            Dictionary<string, object> Data = jsonDataDict["data"] as Dictionary<string, object>;
            observedGeometry.Box = Box.FromData(Data);
            observedGeometry.MarkerType = jsonDataDict["marker_type"] as string;
            return observedGeometry;
        }
    }


    [System.Serializable]
    public class Box
    {
        /*
        * Box : A class to define the structure of a box in the assembly data structure.
        * It is based off the Compas data structure for a box.
        */
        public Frame frame { get; set; }
        public float xsize { get; set; }
        public float ysize { get; set; }
        public float zsize { get; set; }
        public GameObject BoxObject { get; set; }
        public static Box Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Box class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Box FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the Box class from a dictionary.
            */
            Box box = new Box();
            box.frame = Frame.FromData(jsonDataDict["frame"] as Dictionary<string, object>);
            box.xsize = Convert.ToSingle(jsonDataDict["xsize"]);
            box.ysize = Convert.ToSingle(jsonDataDict["ysize"]);
            box.zsize = Convert.ToSingle(jsonDataDict["zsize"]);
            return box;
        }
        public GameObject CreateObject()
        {
            GameObject boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxObject.transform.localScale = new Vector3(xsize, ysize, zsize);
            ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(boxObject, frame.point, frame.xaxis, frame.yaxis, false, false);            
            return boxObject;
        }

        public GameObject CreateBoxObject()
        {
            /*
            * Method is used to instantiate the object from the right hand frame data
            * based on the point, x-axis, y-axis, and z-axis data.
            * This method serves as a simplified version of the placeElement method. And only requires a frame.
            * It loads the object, instantiates it at the correct place and then destroys the loaded object.
            */
            GameObject boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxObject.name = "Box";
            boxObject.transform.position = Vector3.zero;
            boxObject.transform.rotation = Quaternion.identity;
            boxObject.transform.localScale = new Vector3(xsize, zsize, ysize);
            
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(frame.point);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(frame.xaxis, frame.yaxis);
            Quaternion rotationQuaternion;

            rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            if(rotationQuaternion == null)
            {
                Debug.LogError("placeElement: Cannot assign object rotation because it is null");
            }

            //ADD Collider to the object            
            BoxCollider boxCollider = boxObject.AddComponent<BoxCollider>();
            Vector3 boxColliderSize = new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z);
            // Vector3 boxColliderSize = new Vector3(boxCollider.size.x*1.1f, boxCollider.size.y*1.2f, boxCollider.size.z*1.2f);
            boxCollider.size = boxColliderSize;

            //Assign the position and rotation to the object
            boxObject.transform.position = positionData;
            boxObject.transform.rotation = rotationQuaternion;
            
            return boxObject;
        }
    }
}
