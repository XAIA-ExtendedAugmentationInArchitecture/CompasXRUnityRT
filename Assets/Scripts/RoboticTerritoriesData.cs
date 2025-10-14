using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompasXR.Core.Data;
using System.Linq;
using System;
using CompasXR.Core;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using CompasXR.Core.Extentions;
using Unity.VisualScripting;

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
        public GameObject ZoneLineRenderer { get; set; }

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

        public void ColorEntireGoal(GoalObject goal, Material material, bool visibility)
        {
            if (goal == null || material == null)
            {
                Debug.LogWarning("GoalManager: ColorEntireGoal: goal or material is null.");
                return;
            }

            foreach (var component in goal.GoalObjectComponentsDict.Values)
            {
                Renderer renderer = component.ComponentGameObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
                else
                {
                    Debug.LogWarning($"GoalManager: ColorEntireGoal: No Renderer found on component '{component.Name}'");
                }
            }

            goal.GoalGameObject.SetActive(visibility);
        }
    }
    public class GoalObjectComponent
    {
        public string Name { get; private set; }
        public bool IsSatisfied { get; set; }
        public GameObject ComponentGameObject { get; set; }
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
        public GoalStateObserver(GoalObject goalObject = null)
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

    // Helper: never add a null/empty key to `changed`
    private static void TryRecordChanged(
        Dictionary<string, GoalObjectComponent> changed,
        GoalObjectComponent comp,
        string contextForLog)
    {
        if (comp == null) return;

        // Prefer explicit component name; fall back to GO name if needed
        string key = !string.IsNullOrEmpty(comp.Name)
            ? comp.Name
            : comp.ComponentGameObject != null ? comp.ComponentGameObject.name : null;

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning($"ApplySingleObservedGeometryOverwrite: Skipping changed entry with null/empty key (context: {contextForLog}).");
            return;
        }

        changed[key] = comp;
    }

    public Dictionary<string, GoalObjectComponent> ApplySingleObservedGeometryOverwrite(
        ObservedGeometry observed,
        Material satisfiedMaterial,
        Material unsatisfiedMaterial,
        float positionTolerance = 0.03f,
        float rotationToleranceDeg = 3f)
    {
        var changed = new Dictionary<string, GoalObjectComponent>();

        if (observed == null || ComponentStates == null || ComponentStates.Count == 0)
        {
            Debug.LogWarning("GoalStateObserver: ApplySingleObservedGeometryOverwrite: missing observed or components.");
            return changed;
        }

        var observedGO = observed.GeometryObject;
        if (observedGO == null)
        {
            Debug.LogWarning($"ApplySingleObservedGeometryOverwrite: observed '{observed.Name}' has null GeometryObject (likely race: update before instantiation).");
            return changed;
        }
        var obsTransform = observedGO.transform;

        // 1) who is currently satisfied by THIS observed GO?
        GoalObjectComponent previouslyAssigned = null;
        foreach (var comp in ComponentStates.Values)
        {
            var sat = comp?.SatisfyingObservedGeometry;
            if (sat != null && sat.GeometryObject == observedGO)
            {
                previouslyAssigned = comp;
                break;
            }
        }

        // 2) best in-tolerance match
        GoalObjectComponent bestComp = null;
        float bestScore = float.MaxValue;

        foreach (var comp in ComponentStates.Values)
        {
            if (comp == null) continue;

            var compGO = comp.ComponentGameObject;
            if (compGO == null)
            {
                Debug.LogWarning($"ApplySingleObservedGeometryOverwrite: Component '{comp.Name}' has null GameObject — skipping.");
                continue;
            }

            float posErr = (obsTransform.position - compGO.transform.position).magnitude;
            if (posErr > positionTolerance) continue;

            float rotErr = Quaternion.Angle(obsTransform.rotation, compGO.transform.rotation);
            if (rotErr > rotationToleranceDeg) continue;

            float score = posErr + 0.02f * rotErr;
            if (score < bestScore)
            {
                bestScore = score;
                bestComp = comp;
            }
        }

        // 3) no match: unsatisfy previous
        if (bestComp == null)
        {
            if (previouslyAssigned != null)
            {
                previouslyAssigned.IsSatisfied = false;
                previouslyAssigned.SatisfyingObservedGeometry = null;

                var rPrev = previouslyAssigned.ComponentGameObject?.GetComponentInChildren<Renderer>();
                if (rPrev != null && unsatisfiedMaterial != null) rPrev.material = unsatisfiedMaterial;

                Debug.Log($"GoalStateObserver: '{observed.Name}' no longer satisfies '{previouslyAssigned.Name}'.");
                TryRecordChanged(changed, previouslyAssigned, "no-match unsatisfy previous");
            }
            return changed;
        }

        // 4) single-owner: unlink others pointing to this observed GO
        foreach (var comp in ComponentStates.Values)
        {
            if (comp == null || ReferenceEquals(comp, bestComp)) continue;
            var sat = comp.SatisfyingObservedGeometry;
            if (sat != null && sat.GeometryObject == observedGO)
            {
                comp.IsSatisfied = false;
                comp.SatisfyingObservedGeometry = null;

                var r = comp.ComponentGameObject?.GetComponentInChildren<Renderer>();
                if (r != null && unsatisfiedMaterial != null) r.material = unsatisfiedMaterial;

                Debug.Log($"GoalStateObserver: '{observed.Name}' unlinked from '{comp.Name}' (reassigning).");
                TryRecordChanged(changed, comp, "single-owner unlink");
            }
        }

        // 5) moved from a different component
        if (previouslyAssigned != null && !ReferenceEquals(previouslyAssigned, bestComp))
        {
            previouslyAssigned.IsSatisfied = false;
            previouslyAssigned.SatisfyingObservedGeometry = null;

            var rPrev = previouslyAssigned.ComponentGameObject?.GetComponentInChildren<Renderer>();
            if (rPrev != null && unsatisfiedMaterial != null) rPrev.material = unsatisfiedMaterial;

            Debug.Log($"GoalStateObserver: '{observed.Name}' moved from '{previouslyAssigned.Name}' to '{bestComp.Name}'.");
            TryRecordChanged(changed, previouslyAssigned, "moved unsatisfy previous");
        }

        // 6) assign best (this is the "becomes satisfied again" path)
        bestComp.IsSatisfied = true;
        bestComp.SatisfyingObservedGeometry = observed;

        var rBest = bestComp.ComponentGameObject?.GetComponentInChildren<Renderer>();
        if (rBest != null && satisfiedMaterial != null) rBest.material = satisfiedMaterial;

        Debug.Log($"GoalStateObserver: '{observed.Name}' satisfies '{bestComp.Name}' (score {bestScore:F4}).");

        // CRITICAL: don’t add null key if Name is missing
        TryRecordChanged(changed, bestComp, "assign best");

        return changed;
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
        public List<string> GetCompletedComponentsNames()
        {
            return ComponentStates.Values.Where(c => c.IsSatisfied).Select(c => c.Name).ToList();
        }
        public List<string> GetSatisfyingGeometryNames()
        {
            return ComponentStates.Values.Where(c => c.IsSatisfied && c.SatisfyingObservedGeometry != null).Select(c => c.SatisfyingObservedGeometry.Name).ToList();
        }
        public List<string> GetIncompleteComponentsNames()
        {
            return ComponentStates.Values.Where(c => !c.IsSatisfied).Select(c => c.Name).ToList();
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
    public class MimicMirroredGeometryManager
    {
        public GameObject MirroredGeometriesParentObject { get; set; }
        public Dictionary<string, ObservedGeometry> MimicObservedGeometriesDict { get; set; }
        public Dictionary<string, GoalObjectComponent> MimicGoalComponentsDict { get; set; }

        public GameObject TrackedGeometriesParent { get; set; }
        public GameObject GoalsParent { get; set; }

        public string CurrentMimicGoalName { get; set; }
        bool Active
        { get; set; }

        public MimicMirroredGeometryManager(ref GameObject MirroredGeometriesParentObject, Dictionary<string, ObservedGeometry> CurrentObservedGeometriesDict, Dictionary<string, GoalObjectComponent> CurrentGoalComponentsDict, string currentMimicGoalName)
        {
            this.MirroredGeometriesParentObject = MirroredGeometriesParentObject;
            MimicObservedGeometriesDict = _CreateObservedGeometriesDict(MirroredGeometriesParentObject, CurrentObservedGeometriesDict);

            TrackedGeometriesParent = MirroredGeometriesParentObject.FindObject("TrackedGeometriesParent");
            GoalsParent = MirroredGeometriesParentObject.FindObject("MimicGoals");

            CurrentMimicGoalName = currentMimicGoalName;
            MimicGoalComponentsDict = _CreateGoalCompoenetsDict(MirroredGeometriesParentObject, CurrentGoalComponentsDict, currentMimicGoalName);
        }

        private Dictionary<string, ObservedGeometry> _CreateObservedGeometriesDict(GameObject MirroredGeometriesParentObject, Dictionary<string, ObservedGeometry> CurrentObservedGeometriesDict)
        {
            Dictionary<string, ObservedGeometry> observedGeometriesDict = new Dictionary<string, ObservedGeometry>();
            GameObject trackedGeometriesObject = MirroredGeometriesParentObject.FindObject("TrackedGeometriesParent");
            if (trackedGeometriesObject == null)
            {
                Debug.LogWarning($"TrackedGeometriesParentObject not found under {MirroredGeometriesParentObject.name}");
                return observedGeometriesDict;
            }
            foreach (KeyValuePair<string, ObservedGeometry> kvp in CurrentObservedGeometriesDict)
            {
                ObservedGeometry observedGeometry = new ObservedGeometry();
                GameObject child = trackedGeometriesObject.FindObject(kvp.Key);
                if (child == null)
                {
                    Debug.LogWarning($"Observed geometry '{kvp.Key}' not found under {trackedGeometriesObject.name}");
                    continue;
                }
                observedGeometry.Name = kvp.Key;
                observedGeometry.Box = kvp.Value.Box;
                observedGeometry.Box.BoxObject = child;
                observedGeometry.MarkerType = kvp.Value.MarkerType;
                observedGeometry.GeometryObject = child;
                if (observedGeometry.GeometryObject != null)
                {
                    observedGeometriesDict.Add(observedGeometry.Name, observedGeometry);
                }
                else
                {
                    Debug.LogWarning($"Observed geometry '{kvp.Key}' not found under {MirroredGeometriesParentObject.name}");
                }
            }
            return observedGeometriesDict;
        }
        public Dictionary<string, GoalObjectComponent> _CreateGoalCompoenetsDict(
            GameObject MirroredGeometriesParentObject,
            Dictionary<string, GoalObjectComponent> CurrentGoalComponentsDict,
            string CurrentMimicGoalName)
        {
            var goalComponentsDict = new Dictionary<string, GoalObjectComponent>();

            // Validate inputs
            if (MirroredGeometriesParentObject == null)
            {
                Debug.LogWarning("_CreateGoalCompoenetsDict: MirroredGeometriesParentObject is null.");
                return goalComponentsDict;
            }
            if (CurrentGoalComponentsDict == null || CurrentGoalComponentsDict.Count == 0)
            {
                Debug.LogWarning("_CreateGoalCompoenetsDict: CurrentGoalComponentsDict is null or empty.");
                return goalComponentsDict;
            }
            if (string.IsNullOrEmpty(CurrentMimicGoalName))
            {
                Debug.LogWarning("_CreateGoalCompoenetsDict: CurrentMimicGoalName is null or empty.");
                return goalComponentsDict;
            }

            // Locate goal parents
            var goalParent = MirroredGeometriesParentObject.FindObject("MimicGoals");
            if (goalParent == null)
            {
                Debug.LogWarning($"GoalParentObject 'MimicGoals' not found under {MirroredGeometriesParentObject.name}");
                return goalComponentsDict;
            }

            var currentGoalObject = goalParent.FindObject(CurrentMimicGoalName);
            if (currentGoalObject == null)
            {
                Debug.LogWarning($"Current Mimic Goal '{CurrentMimicGoalName}' not found under {goalParent.name}");
                return goalComponentsDict;
            }

            // Local helper: map an observed geometry into the mirrored dict by name first, then by GO
            ObservedGeometry MapObserved(ObservedGeometry src)
            {
                if (src == null) return null;
                if (MimicObservedGeometriesDict == null || MimicObservedGeometriesDict.Count == 0) return null;

                // 1) By name (preferred)
                var n = src.Name;
                if (!string.IsNullOrEmpty(n) && MimicObservedGeometriesDict.TryGetValue(n, out var namedHit))
                    return namedHit;

                // 2) By GO reference / instance id (fallback for when Name is set later)
                var go = src.GeometryObject;
                if (go != null)
                {
                    int id = go.GetInstanceID();
                    foreach (var kv in MimicObservedGeometriesDict)
                    {
                        var v = kv.Value;
                        if (v?.GeometryObject == go) return v;
                        if (v?.GeometryObject && v.GeometryObject.GetInstanceID() == id) return v;
                    }
                }
                return null;
            }

            // Build mirrored components
            foreach (var kvp in CurrentGoalComponentsDict)
            {
                var srcName = kvp.Key;
                var srcComp = kvp.Value;
                if (srcComp == null)
                {
                    Debug.LogWarning($"_CreateGoalCompoenetsDict: Source component '{srcName}' is null.");
                    continue;
                }

                // Find the mirrored child GameObject for this component
                var child = currentGoalObject.FindObject(srcName);
                if (child == null)
                {
                    Debug.LogWarning($"Goal component '{srcName}' not found under {currentGoalObject.name}");
                    continue;
                }

                // Create mirrored component with same name and GO
                var goalComponent = new GoalObjectComponent(srcName, child)
                {
                    IsSatisfied = srcComp.IsSatisfied
                };

                // Map satisfying observed geometry from SOURCE (bug fix: don't check goalComponent.SatisfyingObservedGeometry here)
                var srcSat = srcComp.SatisfyingObservedGeometry;
                if (srcSat != null)
                {
                    var mapped = MapObserved(srcSat);
                    goalComponent.SatisfyingObservedGeometry = mapped;

                    // Optional: debug to understand mapping outcomes
                    if (mapped == null)
                    {
                        var srcSatName = string.IsNullOrEmpty(srcSat.Name) ? "(null)" : srcSat.Name;
                        var srcGoName = srcSat.GeometryObject ? srcSat.GeometryObject.name : "(null GO)";
                        Debug.LogWarning(
                            $"_CreateGoalCompoenetsDict: Could not map satisfying observed geometry for '{srcName}'. " +
                            $"src.Name={srcSatName}, src.GO={srcGoName}."
                        );
                    }
                }
                else
                {
                    goalComponent.SatisfyingObservedGeometry = null;
                }

                // Add to mirrored dict (warn on duplicates)
                if (!goalComponentsDict.TryAdd(goalComponent.Name, goalComponent))
                {
                    Debug.LogWarning($"_CreateGoalCompoenetsDict: Duplicate component key '{goalComponent.Name}' — overwriting.");
                    goalComponentsDict[goalComponent.Name] = goalComponent;
                }
            }

            return goalComponentsDict;
        }
        public void SetGoalComponentsDictFromCurrentGoal(Dictionary<string, GoalObjectComponent> CurrentGoalComponentsDict, string currentMimicGoalName)
        {
            if (MimicGoalComponentsDict == null)
            {
                MimicGoalComponentsDict = new Dictionary<string, GoalObjectComponent>();
            }
            MimicGoalComponentsDict = _CreateGoalCompoenetsDict(MirroredGeometriesParentObject, CurrentGoalComponentsDict, currentMimicGoalName);
        }
        public void UpdateCurrentGoal(string goalName)
        {
            //TODO: Mimic Parent Find Goal by name and update
            GameObject CurrentMimicGoal = MirroredGeometriesParentObject.FindObject("MimicGoals").FindObject(CurrentMimicGoalName);
            if (CurrentMimicGoal != null)
            {
                if (CurrentMimicGoal.activeSelf)
                {
                    //TODO: Logic for coloring them all back again.
                    CurrentMimicGoal.SetActive(false);
                }
            }
            GameObject NewMimicGoal = MirroredGeometriesParentObject.FindObject("MimicGoals").FindObject(goalName);
            if (NewMimicGoal != null)
            {
                //TODO: Logic for coloring....
                if (!NewMimicGoal.activeSelf)
                {
                    NewMimicGoal.SetActive(true);
                }
                CurrentMimicGoalName = goalName;
                UpdateMimicGoalComponentsDictGameObjects(goalName);
            }
            else
            {
                Debug.LogWarning($"Current Mimic Goal '{goalName}' not found under MimicGoals");
            }
        }
        public void UpdateMimicGoalComponentsDictGameObjects(string newGoalName)
        {
            if (MimicGoalComponentsDict == null || MimicGoalComponentsDict.Count == 0)
            {
                Debug.LogWarning("MimicGoalComponentsDict is null or empty");
                return;
            }
            GameObject NewMimicGoal = MirroredGeometriesParentObject.FindObject("MimicGoals").FindObject(newGoalName);
            if (NewMimicGoal == null)
            {
                Debug.LogWarning($"New Mimic Goal '{newGoalName}' not found under MimicGoals");
                return;
            }
            foreach (KeyValuePair<string, GoalObjectComponent> kvp in MimicGoalComponentsDict)
            {
                GameObject child = NewMimicGoal.FindObject(kvp.Key);
                if (child == null)
                {
                    Debug.LogWarning($"Goal component '{kvp.Key}' not found under {NewMimicGoal.name}");
                    continue;
                }
                kvp.Value.ComponentGameObject = child;
            }
        }
        public void UpdateObservedGeometry(string observedGeometryName, ObservedGeometry observedGeometryActual)
        {
            //TODO: Mimic Parent Find Observed Geometry by name and update
            if (!MimicObservedGeometriesDict.ContainsKey(observedGeometryName))
            {
                Debug.LogWarning($"Observed Geometry '{observedGeometryName}' not found in MimicObservedGeometriesDict");
                return;
            }
            ObservedGeometry observedGeometry = MimicObservedGeometriesDict[observedGeometryName];
            if (observedGeometry == null)
            {
                Debug.LogWarning($"Observed Geometry '{observedGeometryName}' is null in MimicObservedGeometriesDict");
                return;
            }
            GameObject observedGeometryObject = observedGeometry.GeometryObject;
            if (observedGeometryObject == null)
            {
                Debug.LogWarning($"Observed Geometry GameObject for '{observedGeometryName}' is null");
                return;
            }
            observedGeometry.Box.frame = observedGeometryActual.Box.frame;
            observedGeometry.MarkerType = observedGeometryActual.MarkerType;
            ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(observedGeometryObject, observedGeometryActual.Box.frame.point, observedGeometryActual.Box.frame.xaxis, observedGeometryActual.Box.frame.yaxis, false, false);
        }
        public void UpdateGoalLocationPosition(GameObject mimicGoalsParentObject, Frame anchorCubeFrame) //TODO: I am not sure if this will actual work.
        {
            if (MirroredGeometriesParentObject == null)
            {
                Debug.LogWarning("MirroredGeometriesParentObject is null");
                return;
            }
            if (mimicGoalsParentObject == null)
            {
                Debug.LogWarning("mimicGoalsParentObject is null");
                return;
            }
            ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(mimicGoalsParentObject, anchorCubeFrame.point, anchorCubeFrame.xaxis, anchorCubeFrame.yaxis, false, false);
        }
        public void DebugLogAllComponentStatesAsDictionary()
        {
            var dict = new Dictionary<string, (bool isSatisfied, string observedName)>();
            foreach (var component in MimicGoalComponentsDict.Values)
            {
                dict[component.Name] = (component.IsSatisfied, component.SatisfyingObservedGeometry != null ? component.SatisfyingObservedGeometry.Name : "None");
            }
            Debug.Log("Mimic Goal Component States Dictionary: " + JsonConvert.SerializeObject(dict, Formatting.Indented));
        }
        public void UpdateCompomponentState(string componentName, GoalObjectComponent satisfyingGoalComponentActual)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogWarning("UpdateCompomponentState: componentName is null or empty — skipping.");
                return;
            }
            if (satisfyingGoalComponentActual == null)
            {
                Debug.LogWarning($"UpdateCompomponentState('{componentName}'): satisfyingGoalComponentActual is null — skipping.");
                return;
            }
            if (MimicGoalComponentsDict == null)
            {
                Debug.LogWarning("UpdateCompomponentState: MimicGoalComponentsDict is null.");
                return;
            }

            if (!MimicGoalComponentsDict.TryGetValue(componentName, out var component) || component == null)
            {
                Debug.LogWarning($"Component '{componentName}' not found or null in MimicGoalComponentsDict");
                return;
            }

            var componentObject = component.ComponentGameObject;
            if (componentObject == null)
            {
                Debug.LogWarning($"Component GameObject for '{componentName}' is null");
                return;
            }

            // 1) Update IsSatisfied flag
            component.IsSatisfied = satisfyingGoalComponentActual.IsSatisfied;

            // 2) Map observed geometry by name (if present)
            var satisfyingObservedGeometry = satisfyingGoalComponentActual.SatisfyingObservedGeometry;
            if (satisfyingObservedGeometry != null)
            {
                var satisfyingGeometryName = satisfyingObservedGeometry.Name;
                if (!string.IsNullOrEmpty(satisfyingGeometryName) &&
                    MimicObservedGeometriesDict != null &&
                    MimicObservedGeometriesDict.TryGetValue(satisfyingGeometryName, out var mappedObserved))
                {
                    component.SatisfyingObservedGeometry = mappedObserved;
                }
                else
                {
                    component.SatisfyingObservedGeometry = null;
                }
            }
            else
            {
                component.SatisfyingObservedGeometry = null;
            }

            // 3) Get the target renderer (include inactive children so hidden goals still work)
            var targetRenderer = componentObject.GetComponentInChildren<Renderer>(true);
            if (!targetRenderer)
            {
                Debug.LogWarning($"Renderer not found on component '{componentName}' (includeInactive:true)");
                return;
            }

            // 4) Get the source renderer from the satisfying component
            Renderer sourceRenderer = null;
            var srcGO = satisfyingGoalComponentActual.ComponentGameObject;
            if (srcGO)
            {
                sourceRenderer = srcGO.GetComponentInChildren<Renderer>(true);
            }
            if (!sourceRenderer)
            {
                Debug.LogWarning($"Source renderer not found on satisfying component '{satisfyingGoalComponentActual.Name}'");
                return;
            }

            // 5) Copy materials directly from source to target
            var srcMats = sourceRenderer.sharedMaterials;
            if (srcMats != null && srcMats.Length > 0)
            {
                targetRenderer.sharedMaterials = srcMats;
                Debug.Log($"Copied {srcMats.Length} materials from '{satisfyingGoalComponentActual.Name}' to '{componentName}'");
            }
            else
            {
                Debug.LogWarning($"Source renderer on '{satisfyingGoalComponentActual.Name}' has no materials");
            }
        }

        public void ColorAllGoalComponentsBasedOnState(Material satisfiedMaterial, Material unsatisfiedMaterial)
        {
            if (MimicGoalComponentsDict == null || MimicGoalComponentsDict.Count == 0)
            {
                Debug.LogWarning("ColorAllGoalComponentsBasedOnState: MimicGoalComponentsDict is null or empty.");
                return;
            }

            foreach (var kvp in MimicGoalComponentsDict)
            {
                var componentName = kvp.Key;
                var component = kvp.Value;
                if (component == null)
                {
                    Debug.LogWarning($"ColorAllGoalComponentsBasedOnState: Component '{componentName}' is null — skipping.");
                    continue;
                }

                var componentObject = component.ComponentGameObject;
                if (componentObject == null)
                {
                    Debug.LogWarning($"ColorAllGoalComponentsBasedOnState: Component GameObject for '{componentName}' is null — skipping.");
                    continue;
                }

                var renderer = componentObject.GetComponentInChildren<Renderer>(true);
                if (renderer == null)
                {
                    Debug.LogWarning($"ColorAllGoalComponentsBasedOnState: Renderer not found on component '{componentName}' — skipping.");
                    continue;
                }

                // Apply material based on IsSatisfied state
                if (component.IsSatisfied)
                {
                    if (satisfiedMaterial != null)
                    {
                        renderer.material = satisfiedMaterial;
                        Debug.Log($"ColorAllGoalComponentsBasedOnState: Applied satisfiedMaterial to '{componentName}'.");
                    }
                    else
                    {

                        Debug.Log($"Compoent name : {componentName}, Component parent name: {componentObject.transform.parent.name}, Component grandparent name: {componentObject.transform.parent.parent.name}, Component great-grandparent name: {componentObject.transform.parent.parent.parent.name}");
                        Debug.LogWarning($"ColorAllGoalComponentsBasedOnState: satisfiedMaterial is null — cannot apply to '{componentName}'.");
                    }
                }
                else
                {
                    if (unsatisfiedMaterial != null)
                    {
                        renderer.material = unsatisfiedMaterial;
                        Debug.Log($"ColorAllGoalComponentsBasedOnState: Applied unsatisfiedMaterial to '{componentName}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"ColorAllGoalComponentsBasedOnState: unsatisfiedMaterial is null — cannot apply to '{componentName}'.");
                    }
                }
            }
        }
        public void UpdateAllComponentStates(Dictionary<string, GoalObjectComponent> currentGoalComponentsDict)
        {
            foreach (KeyValuePair<string, GoalObjectComponent> kvp in currentGoalComponentsDict)
            {
                UpdateCompomponentState(kvp.Key, kvp.Value);
            }
        }
        public void UpdateAllObservedGeometries(Dictionary<string, ObservedGeometry> currentObservedGeometriesDict)
        {
            foreach (KeyValuePair<string, ObservedGeometry> kvp in currentObservedGeometriesDict)
            {
                UpdateObservedGeometry(kvp.Key, kvp.Value);
            }
        }
        public void DestroyMirroredGeometriesParentObject()
        {
            if (MirroredGeometriesParentObject != null)
            {
                GameObject.Destroy(MirroredGeometriesParentObject);
                MirroredGeometriesParentObject = null;
            }
        }
        public void Clear()
        {
            MimicObservedGeometriesDict.Clear();
            MimicGoalComponentsDict.Clear();
            CurrentMimicGoalName = null;
            TrackedGeometriesParent = null;
            GoalsParent = null;
            DestroyMirroredGeometriesParentObject();
            Active = false;
        }

    }

    public class UserInitiatedMimicPickandPlaceManger
    {
        public bool ObjectPicked { get; set; }
        public string PickedObjectName { get; set; }

        //TODO: Visualization of the picked object for both human and robot
        public GameObject PickedObjectVisualizationRobot { get; set; }
        public GameObject PickedObjectVisualizationHuman { get; set; }
        public bool ObjectPlaced { get; set; }
        public string TargetPositionName { get; set; }

        //TODO: Visualization of the target position for both human and robot
        public GameObject TargetPositionVisualizationHuman { get; set; }
        public GameObject TargetPositionVisualizationRobot { get; set; }
        public int PickPointTrajectoryIndex { get; set; }
        public int PlacePointTrajectoryIndex { get; set; }

        public UserInitiatedMimicPickandPlaceManger()
        {
            ObjectPicked = false;
            PickedObjectName = null;
            PickedObjectVisualizationHuman = null;
            PickedObjectVisualizationRobot = null;
            ObjectPlaced = false;
            TargetPositionName = null;
            TargetPositionVisualizationHuman = null;
            TargetPositionVisualizationRobot = null;
            PickPointTrajectoryIndex = -1;
            PlacePointTrajectoryIndex = -1;
        }
        public void Reset()
        {
            ObjectPicked = false;
            PickedObjectName = null;

            if (PickedObjectVisualizationHuman != null && PickedObjectVisualizationHuman.activeSelf)
            {
                PickedObjectVisualizationHuman.SetActive(false);
            }
            if (PickedObjectVisualizationRobot != null && PickedObjectVisualizationRobot.activeSelf)
            {
                PickedObjectVisualizationRobot.SetActive(false);
            }
            PickedObjectVisualizationHuman = null;
            PickedObjectVisualizationRobot = null;

            ObjectPlaced = false;
            TargetPositionName = null;

            if (TargetPositionVisualizationRobot != null && TargetPositionVisualizationRobot.activeSelf)
            {
                TargetPositionVisualizationRobot.SetActive(false);
            }
            if (TargetPositionVisualizationHuman != null && TargetPositionVisualizationHuman.activeSelf)
            {
                TargetPositionVisualizationHuman.SetActive(false);
            }
            TargetPositionVisualizationRobot = null;
            TargetPositionVisualizationHuman = null;

            PickPointTrajectoryIndex = -1;
            PlacePointTrajectoryIndex = -1;
        }
        public void ResetPickObjects()
        {
            ObjectPicked = false;
            PickedObjectName = null;

            if (PickedObjectVisualizationHuman != null && PickedObjectVisualizationHuman.transform.parent.gameObject.activeSelf)
            {
                PickedObjectVisualizationHuman.transform.parent.gameObject.SetActive(false);
            }
            if (PickedObjectVisualizationRobot != null && PickedObjectVisualizationRobot.transform.parent.gameObject.activeSelf)
            {
                PickedObjectVisualizationRobot.transform.parent.gameObject.SetActive(false);
            }
            PickedObjectVisualizationHuman = null;
            PickedObjectVisualizationRobot = null;

            PickPointTrajectoryIndex = -1;
        }
        public void ResetPlaceObjects()
        {
            ObjectPlaced = false;
            TargetPositionName = null;

            if (TargetPositionVisualizationRobot != null && TargetPositionVisualizationRobot.transform.parent.gameObject.activeSelf)
            {
                TargetPositionVisualizationRobot.transform.parent.gameObject.SetActive(false);
            }
            if (TargetPositionVisualizationHuman != null && TargetPositionVisualizationHuman.transform.parent.gameObject.activeSelf)
            {
                TargetPositionVisualizationHuman.transform.parent.gameObject.SetActive(false);
            }
            TargetPositionVisualizationRobot = null;
            TargetPositionVisualizationHuman = null;

            PlacePointTrajectoryIndex = -1;
        }
        public List<int> CreateIOControlIndeciesFromMimicPointCount(int mimicPointCount)
        {
            List<int> ioControlIndecies = new List<int>();

            for (int i = 0; i < mimicPointCount; i++)
            {
                if (PickPointTrajectoryIndex != -1 && i == PickPointTrajectoryIndex)
                {
                    ioControlIndecies.Add(1); //Pick
                }
                else if (PlacePointTrajectoryIndex != -1 && i == PlacePointTrajectoryIndex)
                {
                    ioControlIndecies.Add(2); //Place
                }
                else
                {
                    ioControlIndecies.Add(0); //No Action
                }
            }
            return ioControlIndecies;
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
