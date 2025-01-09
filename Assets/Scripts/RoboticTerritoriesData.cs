using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompasXR.Core.Data;
using System.Linq;
using System;
using CompasXR.Core;

namespace CompasXR.RoboticTerritories.Data
{   
    /*
    */
    //TODO: FIX BOX INSTANTIATION.

    public class ProjectZones
    {
        public Dictionary<string, Zone> MimicZones { get; set; }
        public Dictionary<string, Zone> InferenceZones { get; set; }
        public Dictionary<string, Zone> TelemimicZones { get; set; }
        public Dictionary<string, Zone> BoundaryZone { get; set; }

        public ProjectZones()
        {
            MimicZones = new Dictionary<string, Zone>();
            InferenceZones = new Dictionary<string, Zone>();
            TelemimicZones = new Dictionary<string, Zone>();
            BoundaryZone = new Dictionary<string, Zone>();
        }

        public void Clear()
        {
            MimicZones.Clear();
            InferenceZones.Clear();
            TelemimicZones.Clear();
            BoundaryZone.Clear();
        }
    }

    public class Zone
    {
        public string Name { get; set; }
        public Box Box { get; set; }
        public GameObject ZoneObject { get; set; }
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
            GameObject zoneObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zoneObject.name = Name;
            zoneObject.transform.position = new Vector3(Box.frame.point[0], Box.frame.point[1], Box.frame.point[2]);
            zoneObject.transform.localScale = new Vector3(Box.xsize, Box.ysize, Box.zsize);    
            return zoneObject;
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
    }
}
