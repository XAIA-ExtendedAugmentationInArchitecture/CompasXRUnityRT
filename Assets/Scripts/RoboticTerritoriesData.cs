using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompasXR.Core.Data;
using System.Linq;
using System;
using CompasXR.Core;
using Newtonsoft.Json;
using UnityEngine.InputSystem;

namespace CompasXR.RoboticTerritories.Data
{   
    /*
    */
    public class ProjectZones
    {
        
        public CurrentZoneMode CurrentZone { get; set; }
        public Dictionary<string, Zone> MimicZones { get; set; }
        public Dictionary<string, Zone> InferenceZones { get; set; }
        public Dictionary<string, Zone> BoundaryZone { get; set; }
        public MimicZoneMode CurrentMimicMode { get; set; }

        public ProjectZones()
        {
            MimicZones = new Dictionary<string, Zone>();
            InferenceZones = new Dictionary<string, Zone>();
            BoundaryZone = new Dictionary<string, Zone>();
            CurrentZone = CurrentZoneMode.None;
            CurrentMimicMode = MimicZoneMode.UserInitiated;
        }

        public void Clear()
        {
            MimicZones.Clear();
            InferenceZones.Clear();
            BoundaryZone.Clear();
        }
        public enum CurrentZoneMode
        {
            None,
            Inference,
            Mimic,
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

    public class GoalObject
    {
        public string Name { get; private set; }
        public GameObject GoalGameObject { get; private set; }
        public Dictionary<string, GoalObjectComponent> GoalObjectComponentsDict { get; private set; }
        public GoalObject(string name, GameObject parentObject)
        {
            Name = name;
            GoalGameObject = parentObject;
            GoalObjectComponentsDict = BuildComponentsDict(parentObject);
        }

        // Internal method to build the dictionary from child objects
        private Dictionary<string, GoalObjectComponent> BuildComponentsDict(GameObject parent)
        {
            var dict = new Dictionary<string, GoalObjectComponent>();
            Debug.Log("Building components dictionary for Goal: " + parent.name);
            foreach (Transform child in parent.transform)
            {
                if (!dict.ContainsKey(child.name))
                {
                    GoalObjectComponent goalComponent = new GoalObjectComponent(child.name, child.gameObject);
                    dict.Add(goalComponent.Name, goalComponent);
                }
                else
                {
                    Debug.LogWarning($"Duplicate child name '{child.name}' under {parent.name}");
                }
            }

            return dict;
        }
    }

    public class GoalManager
    {
        public List<GoalObject> Goals { get; private set; }
        public string ParentObjectName { get; private set; }
        public GoalObject CurrentGoal { get; set; }
        public GoalStateObserver GoalStatusObserver { get; set; }
        public GoalManager(GameObject parentObject)
        {
            Goals = new List<GoalObject>();
            ParentObjectName = parentObject.name;
            CurrentGoal = null;

            foreach (Transform child in parentObject.transform)
            {
                Debug.Log("Adding Goal: " + child.name);
                Goals.Add(new GoalObject(child.name, child.gameObject));
            }
        }

        // Accessors
        public GoalObject GetByIndex(int index) => Goals[index];

        public GoalObject GetByName(string name) =>
            Goals.Find(g => g.Name == name);

        public void UpdateCurrentGoal(GoalObject newGoal)
        {
            CurrentGoal = newGoal;

            if (GoalStatusObserver != null)
            {
                if (GoalStatusObserver.Active)
                {
                    GoalStatusObserver.InitializeComponentStates(newGoal);
                }
                else
                {
                    Debug.LogWarning("GoalManager: GoalStateObserver is not active. Cannot initialize component states.");
                }
            }
            else
            {
                Debug.LogWarning("GoalManager: GoalStateObserver is null. Cannot initialize component states.");
            }
        }
    }
    public class GoalObjectComponent
        {
            public string Name { get; private set; }
            public bool IsSatisfied { get; set; }
            public GameObject ComponentGameObject { get; private set; }
            public ObservedGeometry SatisfyingObservedGeometry { get; set; }

            public GoalObjectComponent(string name, GameObject componentObject)
            {
                Name = name;
                ComponentGameObject = componentObject;
                IsSatisfied = false;
                SatisfyingObservedGeometry = null;
            }
        }

    public class GoalStateObserver //TODO: Integrate this class within the GoalManager Class. (Active when zones switch (For Mimic), but When Inference, active when inference is complete)
    {
        public Dictionary<string, GoalObjectComponent> ComponentStates { get; private set; }
        public bool Active { get; set; }
        public bool AllComponentsSatisfied
        {
            get
            {
                return ComponentStates.Values.All(g => g.IsSatisfied);
            }
        }

        public GoalStateObserver(GoalObject goalObject=null)
        {
            ComponentStates = new Dictionary<string, GoalObjectComponent>();
            if (goalObject != null)
            {
                InitializeComponentStates(goalObject);
            }

            Active = false;
        }

        public void InitializeComponentStates(GoalObject goal)
        {
            ComponentStates.Clear();
            foreach (var component in goal.GoalObjectComponentsDict)
            {
                if (!ComponentStates.ContainsKey(component.Key))
                {
                    ComponentStates.Add(component.Key, component.Value);
                }
                else
                {
                    Debug.LogWarning($"GoalStateObserver: Duplicate component name '{component.Key}' in goal '{goal.Name}'");
                }
            }
        }
        public void ColorGoalComponentbySatisfaction(bool isSatisfied, Material satisfiedMaterial, Material unsatisfiedMaterial)
        {
            foreach (var component in ComponentStates.Values)
            {
                Renderer renderer = component.ComponentGameObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = component.IsSatisfied ? satisfiedMaterial : unsatisfiedMaterial;
                }
                else
                {
                    Debug.LogWarning($"GoalStateObserver: No Renderer found on component '{component.Name}'");
                }
            }
        }
        public void ClearComponentStates()
        {
            ComponentStates.Clear();
        }
        public void CheckAllGoalsStatesFromObservedGeometriesDict(
            Dictionary<string, ObservedGeometry> observedGeometriesDict,
            Material satisfiedMaterial,
            Material unsatisfiedMaterial,
            float positionTolerance = 0.03f,
            float rotationTolerance = 3f)
        {
            if (ComponentStates == null || ComponentStates.Count == 0) return;

            foreach (var kvp in ComponentStates.ToList())
            {
                var component = kvp.Value;

                // 1) overwrite/reset at the start of THIS component's evaluation
                component.IsSatisfied = false;
                component.SatisfyingObservedGeometry = null;

                // 2) scan observed; break on first match
                foreach (var observed in observedGeometriesDict.Values)
                {
                    // Use the SAME destructive checker you already have
                    var updated = CheckGoalStateFromObservedGeometry(
                        observed, component.Name, positionTolerance, rotationTolerance);

                    if (updated != null && updated.IsSatisfied)
                    {
                        updated.SatisfyingObservedGeometry = observed;   // explicit bind (ok but optional)
                        ComponentStates[kvp.Key] = updated;               // explicit reassign (optional; it's a class)
                        Debug.Log($"GoalStateObserver: Component '{kvp.Key}' satisfied by '{observed.Name}'");
                        break;
                    }
                }
            }

            // 3) color once at the end (final states only)
            ColorGoalComponentbySatisfaction(true, satisfiedMaterial, unsatisfiedMaterial);
        }

        // TODO: THIS METHOD IS THE KEY TO OPTITRACK UPDAETS.
        public void ApplySingleObservedGeometryOverwrite(
            ObservedGeometry observed, Material satisfiedMaterial, Material unsatisfiedMaterial, float positionTolerance = 0.03f, float rotationToleranceDeg = 3f)
        {
            if (observed == null || ComponentStates == null || ComponentStates.Count == 0)
            {
                Debug.LogWarning("GoalStateObserver: ApplySingleObservedGeometryOverwrite: missing observed or components.");
                return;
            }

            // 1) Find if this observed is currently assigned to any component
            GoalObjectComponent previouslyAssigned = null;
            foreach (var comp in ComponentStates.Values)
            {
                if (ReferenceEquals(comp.SatisfyingObservedGeometry, observed))
                {
                    previouslyAssigned = comp;
                    break;
                }
            }

            // 2) Find the BEST matching component (within tolerance) for this observed (no mutation yet)
            GoalObjectComponent bestComp = null;
            float bestScore = float.MaxValue;

            foreach (var comp in ComponentStates.Values)
            {
                float posErr = (observed.GeometryObject.transform.position - comp.ComponentGameObject.transform.position).magnitude;
                if (posErr > positionTolerance) continue;

                float rotErr = Quaternion.Angle(
                    observed.GeometryObject.transform.rotation,
                    comp.ComponentGameObject.transform.rotation);
                if (rotErr > rotationToleranceDeg) continue;

                // simple combined score; tweak weights as needed
                float score = posErr + 0.02f * rotErr; // 0.02 scales degrees roughly vs meters
                if (score < bestScore)
                {
                    bestScore = score;
                    bestComp = comp;
                }
            }

            // 3) Overwrite states for only the involved components
            //    Case A: no component in tolerance -> unsatisfy previous (if any)
            if (bestComp == null)
            {
                if (previouslyAssigned != null)
                {
                    previouslyAssigned.IsSatisfied = false;
                    previouslyAssigned.SatisfyingObservedGeometry = null;

                    var rPrev = previouslyAssigned.ComponentGameObject?.GetComponentInChildren<Renderer>();
                    if (rPrev != null) rPrev.material = unsatisfiedMaterial;

                    Debug.Log($"GoalStateObserver: '{observed.Name}' no longer satisfies '{previouslyAssigned.Name}'.");
                }
                return;
            }

            //    Case B: we have a best match -> assign it; unsatisfy previous if different
            if (previouslyAssigned != null && !ReferenceEquals(previouslyAssigned, bestComp))
            {
                previouslyAssigned.IsSatisfied = false;
                previouslyAssigned.SatisfyingObservedGeometry = null;

                var rPrev = previouslyAssigned.ComponentGameObject?.GetComponentInChildren<Renderer>();
                if (rPrev != null) rPrev.material = unsatisfiedMaterial;

                Debug.Log($"GoalStateObserver: '{observed.Name}' moved from '{previouslyAssigned.Name}' to '{bestComp.Name}'.");
            }

            // Assign observed to best component (overwrite semantics)
            bestComp.IsSatisfied = true;
            bestComp.SatisfyingObservedGeometry = observed;

            var rBest = bestComp.ComponentGameObject?.GetComponentInChildren<Renderer>();
            if (rBest != null) rBest.material = satisfiedMaterial;

            // Optional: if previouslyAssigned == bestComp and it stayed in tolerance, this just reaffirms the link.
            Debug.Log($"GoalStateObserver: '{observed.Name}' satisfies '{bestComp.Name}' (score {bestScore:F4}).");
        }
        public void DebugLogComponentsStates()
        {
            foreach (var component in ComponentStates.Values)
            {
                Debug.Log($"Component: {component.Name}, IsSatisfied: {component.IsSatisfied}, SatisfyingObservedGeometry: {(component.SatisfyingObservedGeometry != null ? component.SatisfyingObservedGeometry.Name : "None")}");
            }
        }
        public void DebugLogAllComponentStatesAsDictionary()
        {
            var dict = new Dictionary<string, (bool isSatisfied, string observedName)>();
            foreach (var component in ComponentStates.Values)
            {
                dict[component.Name] = (component.IsSatisfied, component.SatisfyingObservedGeometry != null ? component.SatisfyingObservedGeometry.Name : "None");
            }
            Debug.Log("Component States Dictionary: " + JsonConvert.SerializeObject(dict, Formatting.Indented));
        }
        public GoalObjectComponent CheckGoalStateFromObservedGeometry(ObservedGeometry observedGeometry, string componentName, float positionTolerance = 0.03f, float rotationTolerance = 3f)
        {
            GoalObjectComponent component = ComponentStates.ContainsKey(componentName) ? ComponentStates[componentName] : null;
            if (component != null)
            {
                bool isSatisfied = ObjectInstantiaion.IsWithinPoseTolerance(observedGeometry.GeometryObject, component.ComponentGameObject, positionTolerance, rotationTolerance);
                component.IsSatisfied = isSatisfied;
                component.SatisfyingObservedGeometry = isSatisfied ? observedGeometry : null;
                return component;
            }
            else
            {
                Debug.LogWarning($"GoalStateObserver: Component '{componentName}' not found in the current goal.");
                return null;
            }
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
        public String Name { get; set; }
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
            Dictionary<string, object> Data = boxData["data"] as Dictionary<string, object>;
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
