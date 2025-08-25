using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dummiesman;
using TMPro;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.AppSettings;
using CompasXR.RoboticTerritories.Data;
using Newtonsoft.Json;
using CompasXR.Robots;
using CompasXR.Robots.Data;
using Vuforia;
using RosSharp.RosBridgeClient.MessageTypes.ObjectRecognition;


namespace CompasXR.Core
{
    /*
    * CompasXR.Core : Is the Primary namespace for all Classes that
    * controll the primary functionalities of the CompasXR Application.
    */
    public class InstantiateObjects : MonoBehaviour
    {
        /*
        * InstantiateObjects : Class is used to manage the instantiation of objects
        * in the AR space, and control the visulization, coloring, & etc. of the objects based on the
        * user input, building plan data, assembly info, and event items.
        */

        //Other Sript Objects
        public DatabaseManager databaseManager;
        public UIFunctionalities UIFunctionalities;
        public ScrollSearchManager scrollSearchManager;


        //Object Materials
        public Material BuiltMaterial;
        public Material UnbuiltMaterial;
        public Material HumanBuiltMaterial;
        public Material HumanUnbuiltMaterial;
        public Material RobotBuiltMaterial;
        public Material RobotUnbuiltMaterial;
        public Material LockedObjectMaterial;
        public Material SearchedObjectMaterial;
        public Material ActiveRobotMaterial;
        public Material InactiveRobotMaterial;
        public Material OutlineMaterial;

        //Parent Objects
        public GameObject QRMarkers; 
        public GameObject Elements;
        public GameObject ActiveUserObjects;

        //Events
        public delegate void InitialElementsPlaced(object source, EventArgs e);
        public event InitialElementsPlaced PlacedInitialElements;

        //Make Initial Visulization controler
        public ModeControler visulizationController = new ModeControler();

        //Private in script use objects
        private GameObject IdxImage;
        private GameObject PriorityImage;
        public GameObject MyUserIndacator;
        private GameObject OtherUserIndacator;
        public GameObject ObjectLengthsTags;

        //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////

        Dictionary<string, GameObject> ZonesParentObjectsDict = new Dictionary<string, GameObject>();
        public GameObject ZonesParentObject;
        public GameObject BoundaryZoneParent;
        public GameObject InferenceZonesParent;
        public GameObject MimicZonesParent;
        public TrajectoryVisualizer trajectoryVisualizer;


        //AR Camera and Object
        public GameObject cameraPositionObject;
        public Camera arCamera;

        //Materials for Zones
        public Material HumanZoneMaterial;
        public Material RobotZoneMaterial;
        public Material CollaborationZoneMaterial;
        public Material PickZoneMaterial;
        public Material BoundaryMaterial;

        //TODO: Robotic Territories Materials //////////////////////////////////////////
        public Material ObservedGeometryMaterial;
        public Material AnchorCubeMaterial;

        //TODO: Robotic Territories Materials //////////////////////////////////////////


        //Mimic GameObjects
        public GameObject MimicHumanObjects;
        public GameObject MimicHumanPointsParent;
        public GameObject MimicHumanSystemProposedPointsParent;
        public GameObject MimicHumanLine;

        public GameObject MimicSystemProposedLineHuman;
        public GameObject MimicSystemProposedLineRobot;

        public GameObject MimicRobotObjects;
        public GameObject MimicRobotPointsParent;
        public GameObject MimicRobotSystemProposedPointsParent;
        public GameObject MimicRobotLine;
        public List<GameObject> MimicHumanPoints = new List<GameObject>();
        public List<GameObject> MimicHumanSystemProposedPoints = new List<GameObject>();
        public List<GameObject> MimicRobotPoints = new List<GameObject>();
        public List<GameObject> MimicRobotSystemProposedPoints = new List<GameObject>();

        //Zones AR Prefabs
        public GameObject ZonesARPrefabObjects;
        public GameObject AllGeometiresParentObjects;
        public GameObject TrackedGeometriesParentObject;

        public GameObject InferenceGoalsParentObject;
        public GoalManager InferenceGoalsManager { get; private set; }
        public GameObject MimicGoalsParentObject;
        public GoalManager MimicGoalsManager { get; private set; }

        //TODO: REALTIME MIMIC OBJECT TESTING
        public GameObject RealtimeMimicObjects;
        public GameObject RealtimeMimicHumanLine;
        public GameObject RealtimeMimicRobotLine;
        public GameObject RealtimeMimicHumanPointsParent;
        public GameObject RealtimeMimicRobotPointsParent;
        public List<GameObject> RealtimeMimicHumanPoints = new List<GameObject>();
        public List<GameObject> RealtimeMimicRobotPoints = new List<GameObject>();

        //Events
        public delegate void InitialZonesCreated(object source, EventArgs e);
        public event InitialZonesCreated InitialZonesPlaced;

        //TODO: VUFORIA TESTING
        DevicePoseBehaviour devicePoseBehavior;

        public delegate void InitialTrackedGeometryCreated(object source, EventArgs e);
        public event InitialTrackedGeometryCreated InitialTrackedGeometryPlaced;

        //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////// Monobehaviour Methods //////////////////////////////////////////
        public void Awake()
        {
            OnAwakeInitilization();

        }
        public void Update()
        {
            //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////

            if (devicePoseBehavior == null)
            {
                devicePoseBehavior = VuforiaBehaviour.Instance.DevicePoseBehaviour;
                if (devicePoseBehavior == null)
                {
                    Debug.LogError("DevicePoseBehaviour not found in the scene. Please check Vuforia setup.");
                }
            }

            //Update the position of the Mimic Lines if they are on.
            if (databaseManager.ProjectZones.CurrentZone == ProjectZones.CurrentZoneMode.Mimic)
            {

                if (databaseManager.ProjectZones.CurrentMimicMode == ProjectZones.MimicZoneMode.UserInitiated)
                {
                    UpdateLinePositionsByGameObjectPositionsList(MimicHumanPoints, MimicHumanLine);
                    UpdateLinePositionsByGameObjectPositionsList(MimicRobotPoints, MimicRobotLine);

                    //TODO: TESTING
                    UpdateLinePositionsByGameObjectPositionsList(MimicHumanSystemProposedPoints, MimicSystemProposedLineHuman);
                    UpdateLinePositionsByGameObjectPositionsList(MimicRobotSystemProposedPoints, MimicSystemProposedLineRobot);
                }
                else if (databaseManager.ProjectZones.CurrentMimicMode == ProjectZones.MimicZoneMode.RealtimeMimic)
                {
                    if (RealtimeMimicHumanPoints.Count > 1 && RealtimeMimicRobotPoints.Count > 1)
                    {
                        UpdateLinePositionsByGameObjectPositionsList(RealtimeMimicHumanPoints, RealtimeMimicHumanLine);
                        UpdateLinePositionsByGameObjectPositionsList(RealtimeMimicRobotPoints, RealtimeMimicRobotLine);
                        Debug.Log("UpdateLinePositionsByGameObjectPositionsList: Updating Realtime Mimic Lines");
                    }
                    else
                    {
                        Debug.LogWarning("UpdateLinePositionsByGameObjectPositionsList: Realtime Mimic Points are not greater then 1 for some reason.");
                    }
                }
            }
        }

        /////////////////////////////// INSTANTIATE OBJECTS //////////////////////////////////////////

        //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////
        public void OnObservedGeometriesFetched(object source, ObservedGeometriesFetchedEventArgs e)
        {
            /*
            * Method is used to handle the event when the observed geometries are fetched from the database.
            */
            Debug.Log("OnObservedGeometriesFetched: Observed Geometries Fetched");
            InstantiateObservedGeometries(e.ObservedGeometriesDict);
        }
        public void InstantiateObservedGeometries(Dictionary<string, ObservedGeometry> observedGeometriesDict)
        {
            /*
            * Method is used to instantiate the observed geometries in the AR space.
            */
            if (observedGeometriesDict != null && observedGeometriesDict.Count > 0)
            {
                Debug.Log("InstanteObservedGeometries: Instantiating Observed Geometries");
                foreach (KeyValuePair<string, ObservedGeometry> entry in observedGeometriesDict)
                {
                    ObservedGeometry observedGeometry = entry.Value;
                    observedGeometry.Name = entry.Key;
                    GameObject observedGeometryObject = observedGeometry.Box.CreateBoxObject();
                    observedGeometry.GeometryObject = observedGeometryObject;
                    observedGeometry.GeometryObject.name = observedGeometry.Name;
                    observedGeometryObject.transform.SetParent(TrackedGeometriesParentObject.transform, false);
                }
                ColorObservedGeometries(observedGeometriesDict);
            }
            else
            {
                Debug.LogWarning("InstanteObservedGeometries: Observed Geometries Dict is null or empty");
            }
        }
        public void ColorObservedGeometries(Dictionary<string, ObservedGeometry> observedGeometriesDict)
        {
            /*
            * Method is used to color the observed geometries in the AR space.
            */
            if (observedGeometriesDict != null && observedGeometriesDict.Count > 0)
            {
                Debug.Log("ColorObservedGeometries: Coloring Observed Geometries");
                foreach (KeyValuePair<string, ObservedGeometry> entry in observedGeometriesDict)
                {
                    ObservedGeometry observedGeometry = entry.Value;
                    if (observedGeometry.GeometryObject != null)
                    {
                        Renderer renderers = observedGeometry.GeometryObject.GetComponentInChildren<Renderer>();
                        if (renderers != null)
                        {
                            if (observedGeometry.Name != "AnchorCube")
                            {
                                renderers.material = ObservedGeometryMaterial;
                            }
                            else
                            {
                                renderers.material = AnchorCubeMaterial;
                                //TODO: Update in the EVENTS
                                UpdateGoalsBasedOnAnchorCubeObservedFrame(observedGeometry.Box.frame, ref MimicGoalsParentObject, ref InferenceGoalsParentObject);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"ColorObservedGeometries: Renderer for {observedGeometry.Name} is null");
                        }


                    }
                    else
                    {
                        Debug.LogWarning($"ColorObservedGeometries: Geometry Object for {observedGeometry.Name} is null");
                    }
                }

                OnInitialTrackedGeometryPlaced();
                
            }
            else
            {
                Debug.LogWarning("ColorObservedGeometries: Observed Geometries Dict is null or empty");
            }
        }
        public void UpdateGoalsBasedOnAnchorCubeObservedFrame(Frame anchorCubeFrame, ref GameObject mimicGoalsParent, ref GameObject inferenceGoalsParent)
        {
            /*
            * Method is used to update the goals based on the anchor cube observed frame.
            */
            if (anchorCubeFrame != null)
            {
                Debug.Log("UpdateGoalsBasedOnAnchorCubeObservedFrame: Updating Goals Based on Anchor Cube Observed Frame");
            }
            else
            {
                Debug.LogWarning("UpdateGoalsBasedOnAnchorCubeObservedFrame: Anchor Cube Frame is null");
                return;
            }

            // IF mimic and inferance parents are null, then return.
            if (mimicGoalsParent == null || inferenceGoalsParent == null)
            {
                Debug.LogWarning("UpdateGoalsBasedOnAnchorCubeObservedFrame: Mimic or Inference Goals Parent is null");
                return;
            }
            ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(mimicGoalsParent, anchorCubeFrame.point, anchorCubeFrame.xaxis, anchorCubeFrame.yaxis, false, false);
            ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(inferenceGoalsParent, anchorCubeFrame.point, anchorCubeFrame.xaxis, anchorCubeFrame.yaxis, false, false);
            Debug.Log($"UpdateObservedGeometryLocation: Updated geometry location to {anchorCubeFrame.point} with x-axis {anchorCubeFrame.xaxis} and y-axis {anchorCubeFrame.yaxis}");
        }
        public void OnZonesReceived(object source, ZonesInfoReceivedEventArgs e)
        {
            /*
            * Method is used to handle the event when the database is initialized
            */
            Debug.Log("OnZonesRecived: Zones Received");
            // SetZonesMaterials(); //TODO: This is stupid, but will hopefully work before the other one is finished
            PlaceAllProjectZones(e.Zones, ZonesParentObjectsDict);
        }  
        public void OnModeZonesUpdate(object source, ModeZonesUpdateEventArgs e)
        {
            /*
            * Method is used to handle the event when the database is initialized
            */
            Debug.Log($"OnModeZonesUpdate: Zones Changed for {e.Key}");
            
            if(e.Zones.Count > 0)
            {
                InstantiateChangedZones(e.Zones, e.Key);
            }
            else
            {
                Debug.LogWarning("OnModeZonesUpdate: Zones Dict is empty");
            }
        }
        public void InstantiateChangedZones(Dictionary<string, Zone> ZonesDict, string zoneKey)
        {
            Debug.Log($"InstantiateChangedKeys: {zoneKey} with the information from the ZonesDict {JsonConvert.SerializeObject(ZonesDict)}");
            switch (zoneKey)
            {
                case "mimic_zones":
                    GameObject mimicParent = ZonesParentObjectsDict["MimicZonesParent"];
                    ObjectInstantiaion.DestroyChildrenOfGameObject(mimicParent);
                    PlaceModeZones(ZonesDict, mimicParent);
                    break;
                case "inference_zones":
                    GameObject inferenceParent = ZonesParentObjectsDict["InferenceZonesParent"];
                    ObjectInstantiaion.DestroyChildrenOfGameObject(inferenceParent);
                    PlaceModeZones(ZonesDict, inferenceParent);                    
                    break;
                case "boundary_zone":
                    GameObject boundaryParent = ZonesParentObjectsDict["BoundaryZoneParent"];
                    ObjectInstantiaion.DestroyChildrenOfGameObject(boundaryParent);
                    PlaceModeZones(ZonesDict, boundaryParent);
                    break;
                default:
                    Debug.LogWarning($"InstantiateChangedKeys: Invalid Zone Type for key '{zoneKey}'. Not changing the key.");
                    break;
            }

            //Color all zones based on the current mode
            UIFunctionalities.ColorZonesBasedOnCurrentMode(databaseManager.ProjectZones.CurrentZone);

        }
        public void PlaceModeZones(Dictionary<string, Zone> ModeZone, GameObject ParentObject)
        {
            /*
            * Method is used to place the zones in the AR space
            */            
            if (ModeZone != null)
            {
                Debug.Log("PlaceZones: Placing Zones");
                foreach (KeyValuePair<string, Zone> entry in ModeZone)
                {
                    if (entry.Value != null)
                    {
                        PlaceZoneItem(entry.Value, ParentObject);
                    }
                }
            }
            else
            {
                Debug.LogWarning("PlaceZones: Project Zones is null");
            }
        }
        public void DestroyZoneObjects(Dictionary<string, Zone> ZonesDict)
        {
            /*
            * Method is used to destroy the zone objects in the AR space
            */
            if (ZonesDict != null)
            {
                Debug.Log("DestroyZones: Destroying Zones");
                foreach (KeyValuePair<string, Zone> entry in ZonesDict)
                {
                    if (entry.Value != null)
                    {
                        Destroy(entry.Value.ZoneObject);
                    }
                }
            }
            else
            {
                Debug.LogWarning("DestroyZones: Zones Dict is null");
            }
        }
        public void PlaceAllProjectZones(ProjectZones projectZones, Dictionary<string, GameObject> parentObjects, bool isInitial = true)
        {
            /*
            * Method is used to place the zones in the AR space
            */
            if (projectZones != null)
            {
                Debug.Log("PlaceZones: Placing Zones");
                foreach (KeyValuePair<string, Zone> entry in projectZones.BoundaryZone)
                {
                    if (entry.Value != null)
                    {
                        GameObject parentObject = parentObjects["BoundaryZoneParent"];
                        PlaceZoneItem(entry.Value, parentObject);
                    }
                }
                
                foreach (KeyValuePair<string, Zone> entry in projectZones.InferenceZones)
                {
                    if (entry.Value != null)
                    {
                        GameObject parentObject = parentObjects["InferenceZonesParent"];
                        PlaceZoneItem(entry.Value, parentObject);
                    }
                }

                foreach (KeyValuePair<string, Zone> entry in projectZones.MimicZones)
                {
                    if (entry.Value != null)
                    {
                        GameObject parentObject = parentObjects["MimicZonesParent"];
                        PlaceZoneItem(entry.Value, parentObject);
                    }
                }
                UIFunctionalities.ColorZonesBasedOnCurrentMode(projectZones.CurrentZone);

                //Event trigger for the first time through placing the zones.
                if(isInitial)
                {
                    OnInitialZonesPlaced();
                }
            }
            else
            {
                Debug.LogWarning("PlaceZones: Project Zones is null");
            }
        }
        public void PlaceZoneItem(Zone zone, GameObject ParentObject)
        {
            Debug.Log($"PlaceZone: {zone.Name} In parent Object: {ParentObject}");
            GameObject zoneObject = zone.CreateZoneObject();
            SetIndividualZoneMaterial(zone);
            zone.ZoneObject = zoneObject;
            CreateTextObjectBasedOnZone(zone);
            zoneObject.transform.SetParent(ParentObject.transform, false);
        }
        public void SetIndividualZoneMaterial(Zone zone)
        {
            switch (zone.Name)
            {   
                // case "tele_mimic_zone":
                //     zone.ZoneActiveMaterial = RobotZoneMaterial;
                //     zone.ZoneInactiveMaterial = BoundaryMaterial;
                //     break;
                case "human_zone":
                    zone.ZoneActiveMaterial = HumanZoneMaterial;
                    zone.ZoneInactiveMaterial = BoundaryMaterial;
                    break;
                case "robot_zone":
                    zone.ZoneActiveMaterial = RobotZoneMaterial;
                    zone.ZoneInactiveMaterial = BoundaryMaterial;
                    break;
                case "collaboration_zone":
                    zone.ZoneActiveMaterial = CollaborationZoneMaterial;
                    zone.ZoneInactiveMaterial = BoundaryMaterial;
                    break;
                case "pick_zone":
                        zone.ZoneActiveMaterial = PickZoneMaterial;
                        zone.ZoneInactiveMaterial = BoundaryMaterial;
                    break;
                case "boundary_zone":
                    zone.ZoneActiveMaterial = BoundaryMaterial;
                    zone.ZoneInactiveMaterial = BoundaryMaterial;
                    break;
                default:
                    Debug.LogWarning($"SetIndividualZoneMaterial: Couldn't Set material for '{zone.Name}'");
                    break;
            }        
        }
        protected virtual void OnInitialZonesPlaced()
        {
            /*
            * Method is used to raise the event when the initial objects are placed
            */
            InitialZonesPlaced(this, EventArgs.Empty);
        }
        protected virtual void OnInitialTrackedGeometryPlaced()
        {
            /*
            * Method is used to raise the event when the initial objects are placed
            */
            InitialTrackedGeometryPlaced(this, EventArgs.Empty);
        }
        public void CreateMimicPoints(GameObject humanZone, GameObject robotZone, ref List<GameObject> humanPoints, ref List<GameObject> robotPoints, GameObject humanLine, GameObject robotLine, GameObject humanParent, GameObject robotParent, bool Mirror = false)
        {
            /*
            * Method is used to create the mimic points in the AR space
            */

            // Vector3 position = cameraPositionObject.transform.position;
            if (devicePoseBehavior == null)
            {
                Debug.LogError("CreateMimicPoints: DevicePoseBehavior is null. Please check the Vuforia setup.");
                return;
            }
            Vector3 position = devicePoseBehavior.transform.position;

            Debug.Log($"CreateMimicPoint: Reference before adding Rotation {position} with rotation {cameraPositionObject.transform.rotation}");

            Quaternion rotation = AddAdditionalRotationForEndEffector(cameraPositionObject); //TODO: Check this
            float radius = 0.1f;
            Color humanColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            Color robotColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);

            if (trajectoryVisualizer.humanZoneMimicReachibility == null ||
                ObjectInstantiaion.IsPositionWithinObject(trajectoryVisualizer.humanZoneMimicReachibility, position))
            {
                CreateSpheresForMimic(humanZone, robotZone, ref humanPoints, ref robotPoints, humanParent, robotParent, position, rotation, radius, humanColor, robotColor, $"{humanPoints.Count}_MimicPoint", $"{robotPoints.Count}_MimicPoint", true, Mirror);
            }
            else
            {
                CreateSpheresForMimic(humanZone, robotZone, ref humanPoints, ref robotPoints,
                humanParent, robotParent, position, rotation,
                radius, humanColor, robotColor, $"{humanPoints.Count}_MimicPoint", $"{robotPoints.Count}_MimicPoint", true, Mirror);

                // //TODO: Quick test for the closest reachable point:
                // CreateSpheresForMimic(humanZone, robotZone, ref MimicHumanSystemProposedPoints, ref MimicRobotSystemProposedPoints, 
                // MimicSystemProposedLineHuman, MimicSystemProposedLineRobot, MimicHumanSystemProposedPointsParent, MimicRobotSystemProposedPointsParent, 
                // closestReachablePoint, rotation, radius, Color.red, Color.grey, $"{humanPoints.Count}_MimicPointProposal", $"{robotPoints.Count}_MimicPointProposal", false, Mirror);
                //TODO: TESTING...
                CreateSystemProposalPoints(humanZone, robotZone, trajectoryVisualizer.humanZoneMimicReachibility, ref humanPoints,
                ref MimicHumanSystemProposedPoints, ref MimicRobotSystemProposedPoints,
                MimicSystemProposedLineHuman, MimicHumanSystemProposedPointsParent, MimicSystemProposedLineRobot,
                MimicRobotSystemProposedPointsParent, Mirror);

                Debug.LogWarning("CreateMimicPoints: POINT IS WITHIN THE ZONE BUT NOT REACHABLE BY ROBOT.");
            }

            if (humanPoints.Count > 1 && robotPoints.Count > 1)
            {
                Debug.Log("CreateMimicPoints: Creating Mimic Points");
                DrawLineFromGameObjectList(humanPoints, humanLine, humanColor, 0.01f);
                DrawLineFromGameObjectList(robotPoints, robotLine, robotColor, 0.01f);
            }
            else
            {
                Debug.LogWarning("CreateMimicPoints: Human or Robot Positions are empty");
            }
        }
        public void CreateSystemProposalPoints(GameObject humanZone, GameObject robotZone, GameObject reachabilitySphere, ref List<GameObject> userSetHumanPointsList, 
        ref List<GameObject> systemProposedHumanPointsList, ref List<GameObject> systemProposedRobotPointsList,
        GameObject systemProposedHumanLine, GameObject systemProposedHumanPointsParent, GameObject systemProposedRobotLine, 
        GameObject systemProposedRobotPointsParent, bool Mirror=false)
        {
            /*
            * Method is used to create the mimic points in the AR space
            */

            if (systemProposedHumanPointsList != null || systemProposedRobotPointsList != null)
            {
                Debug.Log("CreateSystemProposalPoints: Destroying Previous System Proposed Points");
                ObjectInstantiaion.DestroyChildrenOfGameObject(systemProposedHumanPointsParent);
                ObjectInstantiaion.DestroyChildrenOfGameObject(systemProposedRobotPointsParent);
                systemProposedHumanPointsList.Clear();
                systemProposedRobotPointsList.Clear();
            }
            if (reachabilitySphere == null)
            {
                Debug.LogWarning("CreateSystemProposalPoints: Reachability Sphere is null.");
                return;
            }

            //TODO: Set Activity of the system proposed line and points.
            systemProposedHumanLine.SetActive(true);
            systemProposedHumanPointsParent.SetActive(true);
            systemProposedRobotLine.SetActive(true);
            systemProposedRobotPointsParent.SetActive(true);

            List<GameObject> pointsToCheck = new List<GameObject>(userSetHumanPointsList);

            float radius = 0.1f;

            for (int i = 0; i < pointsToCheck.Count; i++)
            {
                GameObject point = pointsToCheck[i];
                Vector3 position = point.transform.position;
                Quaternion rotation = point.transform.rotation;

                if (!ObjectInstantiaion.IsPositionWithinObject(reachabilitySphere, position))
                {
                    position = FindClosestReachablePoint(reachabilitySphere, position);
                }
                else
                {
                    Debug.LogWarning($"CreateSystemProposalPoints: Point {position} is inside the reachability sphere.");
                }

                //TODO: THIS IS HUGE, DOUBLE CHECK ROTATION ON THE PHONE.
                CreateSpheresForMimic(humanZone, robotZone, ref MimicHumanSystemProposedPoints, ref MimicRobotSystemProposedPoints, 
                systemProposedHumanPointsParent, systemProposedRobotPointsParent, 
                position, rotation, radius, Color.red, Color.red, $"{i}_MimicPoint", $"{i}_MimicPoint", true, Mirror);
            }

            if (systemProposedHumanPointsList.Count > 1 && systemProposedRobotPointsList.Count > 1)
            {
                Debug.Log("CreateSystemTrajectoryProposal: Creating Mimic Points");
                DrawLineFromGameObjectList(systemProposedHumanPointsList, systemProposedHumanLine, Color.red, 0.01f);
                DrawLineFromGameObjectList(systemProposedRobotPointsList, systemProposedRobotLine, Color.red, 0.01f);
            }
            else
            {
                Debug.LogWarning("CreateSystemTrajectoryProposal: Human or Robot Positions are empty");
            }

            UIFunctionalities.SignalMimicPointsRemaptoRobotReachabilityMessage();
        }
        public void MakeMimicPointsFromSystemProposedPoints(ref List<GameObject> humanPoints, ref List<GameObject> robotPoints, ref List<GameObject> systemProposedHumanPoints, ref List<GameObject> systemProposedRobotPoints, 
        GameObject humanPointsParent, GameObject robotPointsParent, GameObject systemProposedHumanParent, GameObject systemProposedRobotParent, 
        GameObject humanLine, GameObject robotLine, GameObject systemProposedHumanLine, GameObject systemProposedRobotLine)
        {
            /*
            * Method is used to create the mimic points from the system proposed points in the AR space
            */
            
            if (humanPoints.Count <= 0 || robotPoints.Count <= 0)
            {
                Debug.LogWarning("MakeMimicPointsFromSystemProposedPoints: human points or robot points are not greater then 0 for some reason");
            }
            else if (systemProposedHumanPoints.Count <= 0 || systemProposedRobotPoints.Count <= 0)
            {
                Debug.LogWarning("MakeMimicPointsFromSystemProposedPoints: System Proposed Points are not greater then 0 for some reason.");
            }
            else
            {
                Debug.Log("MakeMimicPointsFromSystemProposedPoints: Creating Mimic Points from System Proposed Points");
                MigrateSystemProposedMimicPointsToCurrentSelection(ref humanPoints, ref systemProposedHumanPoints, humanPointsParent, humanLine, systemProposedHumanLine, HumanBuiltMaterial, Color.yellow);
                MigrateSystemProposedMimicPointsToCurrentSelection(ref robotPoints, ref systemProposedRobotPoints, robotPointsParent, robotLine, systemProposedRobotLine, RobotBuiltMaterial, Color.cyan);

                if(systemProposedHumanPoints.Count > 0)
                {
                    ObjectInstantiaion.DestroyChildrenOfGameObject(systemProposedHumanParent);
                }
                if(systemProposedRobotPoints.Count > 0)
                {
                    ObjectInstantiaion.DestroyChildrenOfGameObject(systemProposedRobotParent);
                }
            }

        }
        public void MigrateSystemProposedMimicPointsToCurrentSelection(ref List<GameObject> pointListReferenceToSet, ref List<GameObject> systemProposedPointsList, 
        GameObject currentListParent, GameObject currentLine, GameObject systemProposedLine, Material materialToAssignSystemPoints, Color currentLineColor)
        {
            /*
            * Method is used to migrate the mimic points list by the system proposed points in the AR space
            */

            if (pointListReferenceToSet != null)
            {
                Debug.Log("MigrateSystemProposedMimicPointsToCurrentSelection: Destroying Previous System Proposed Points");
                ObjectInstantiaion.DestroyChildrenOfGameObject(currentListParent);
                pointListReferenceToSet.Clear();
            }
            else
            {
                Debug.LogWarning("MigrateSystemProposedMimicPointsToCurrentSelection: Point List Reference to Set is null.");
            }

            if (systemProposedPointsList.Count > 0)
            {
                Debug.Log("MigrateSystemProposedMimicPointsToCurrentSelection: Migrating System Proposed Points to Current Selection");

                foreach (GameObject point in systemProposedPointsList)
                {
                    point.transform.SetParent(currentListParent.transform, true);
                    point.GetComponentInChildren<Renderer>().material = materialToAssignSystemPoints;
                    pointListReferenceToSet.Add(point);
                }

                DrawLineFromGameObjectList(pointListReferenceToSet, currentLine, currentLineColor, 0.01f);
                systemProposedLine.SetActive(false);
                systemProposedPointsList.Clear();
            }
            else
            {
                Debug.LogWarning("MigrateSystemProposedMimicPointsToCurrentSelection: System Proposed Points are empty for some reason.");
            }
            
        }
        public void DestroySystemProposedMimicPoints(ref List<GameObject> systemProposedHumanPoints, ref List<GameObject> systemProposedRobotPoints, GameObject systemProposedHumanParent, GameObject systemProposedRobotParent, GameObject systemProposedHumanLine, GameObject systemProposedRobotLine)
        {
            /*
            * Method is used to destroy the mimic points in the AR space
            */
            if (systemProposedHumanPoints.Count > 0 && systemProposedRobotPoints.Count > 0)
            {
                Debug.Log("DestroySystemProposedMimicPoints: Destroying System Proposed Points");
                ObjectInstantiaion.DestroyChildrenOfGameObject(systemProposedHumanParent);
                ObjectInstantiaion.DestroyChildrenOfGameObject(systemProposedRobotParent);

                systemProposedHumanPoints.Clear();
                systemProposedRobotPoints.Clear();

                systemProposedHumanLine.GetComponentInChildren<LineRenderer>().positionCount = 0;
                systemProposedRobotLine.GetComponentInChildren<LineRenderer>().positionCount = 0;

                systemProposedHumanLine.SetActive(false);
                systemProposedRobotLine.SetActive(false);
            }
            else
            {
                Debug.LogWarning("DestroySystemProposedMimicPoints: System Proposed Points are empty for some reason.");
            }
        }
        public List<GameObject> ColorGameObjectListByInputMaterial(List<GameObject> gameObjects, Material material)
        {
            /*
            * Method is used to color the game objects in the AR space
            */
            if (gameObjects != null && gameObjects.Count > 0)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (gameObject != null)
                    {
                        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material = material;
                        }
                        else
                        {
                            Debug.LogWarning($"ColorGameObjectListByInputMaterial: Renderer is null for {gameObject.name}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("ColorGameObjectListByInputMaterial: Game Objects List is empty or null.");
            }
            return gameObjects;
        }
        public void CreateSpheresForMimic(GameObject humanZone, GameObject robotZone, ref List<GameObject> humanPoints, 
        ref List<GameObject> robotPoints, GameObject humanParent, 
        GameObject robotParent,
        Vector3 position, Quaternion rotation, float radius, Color humanColor, Color robotColor, string humanPointName, string robotPointName, bool addToPointsList =true, //TODO: ADDED THESE
        bool Mirror=false)
        {
            GameObject humanPoint = CreateSphereAtPositionAndRotation(position, rotation, radius, humanColor, humanPointName);//$"{humanPoints.Count}_MimicPoint");
            Debug.Log($"CreateSpheresForMimic: Human Point Created at {position} with rotation {rotation}");
            humanPoint.transform.SetParent(humanParent.transform, true);
            if(addToPointsList)
            {
                humanPoints.Add(humanPoint);
            }
            else
            {
                Debug.LogWarning("CreateSpheresForMimic: Human Point is not added to the list.");
            }

            Vector3 mappedRobotPosition = Vector3.zero;
            Quaternion mappedRotation = Quaternion.identity;
            
            if(!Mirror)
            {
                mappedRobotPosition = MapPointBetweenBoxes(humanZone, robotZone, position); //TODO: Check this
                mappedRotation = rotation; //TODO: Check this
            }
            else
            {
                Vector3 mirroredPosition = MirrorPositionAcrossBox(humanZone, position); //TODO: CHECK THIS IDK WHATS UP.
                // Vector3 mirroredPosition = MirrorPositionAcrossBox(humanZone, position, humanZone.transform.right); //TODO: CHECK THIS IDK WHATS UP.
                // Vector3 mirroredPosition = MirrorPositionAcrossBox(humanZone, position, humanZone.transform.forward); //TODO: CHECK THIS IDK WHATS UP.
                Quaternion mirrorRotation = MirrorQuaternion(rotation, humanZone.transform.right);
                mappedRobotPosition = MapPointBetweenBoxes(humanZone, robotZone, mirroredPosition);
                mappedRotation = mirrorRotation;
            }

            GameObject robotPoint = CreateSphereAtPositionAndRotation(mappedRobotPosition, rotation, radius, robotColor, robotPointName);//, $"{robotPoints.Count}_MimicPoint");
            robotPoint.transform.rotation = mappedRotation;
            robotPoint.transform.SetParent(robotParent.transform, true);
            if(addToPointsList)
            {
                robotPoints.Add(robotPoint);
            }
            else
            {
                Debug.LogWarning("CreateSpheresForMimic: Robot Point is not added to the list.");
            }
        }
        public Vector3 FindClosestReachablePoint(GameObject reachabilitySphere, Vector3 desiredPosition)
        {
            /*
            * Method is used to find the closest reachable point in the zone.
            */
            if (reachabilitySphere == null)
            {
                Debug.LogError("FindClosestReachablePoint: Zone GameObject is null.");
                return Vector3.zero;
            }

            Vector3 center = ObjectTransformations.FindGameObjectCenter(reachabilitySphere);

            // Use half of the scaled size if the sphere is scaled from a 1-unit diameter (default Unity sphere)
            float radius = 0.5f * reachabilitySphere.transform.lossyScale.x;

            Vector3 direction = desiredPosition - center;
            float distance = direction.magnitude;

            if (distance <= radius)
            {
                return desiredPosition;
            }

            return center + direction.normalized * radius;
        }
        public static Vector3 MapPointBetweenBoxes(GameObject sourceBox, GameObject targetBox, Vector3 pointPosition)
        {
            if (sourceBox == null || targetBox == null)
            {
                Debug.LogError("MapPointBetweenBoxes: One or both GameObjects are null.");
                return Vector3.zero;
            }

            BoxCollider sourceCollider = sourceBox.GetComponent<BoxCollider>();
            BoxCollider targetCollider = targetBox.GetComponent<BoxCollider>();

            if (sourceCollider == null || targetCollider == null)
            {
                Debug.LogError("MapPointBetweenBoxes: One or both GameObjects are missing a BoxCollider.");
                return Vector3.zero;
            }

            Vector3 localPosition = sourceBox.transform.InverseTransformPoint(pointPosition);
            Vector3 normalizedPosition = new Vector3(
                (localPosition.x - sourceCollider.center.x) / sourceCollider.size.x,
                (localPosition.y - sourceCollider.center.y) / sourceCollider.size.y,
                (localPosition.z - sourceCollider.center.z) / sourceCollider.size.z
            );
            Vector3 targetLocalPosition = new Vector3(
                targetCollider.center.x + (normalizedPosition.x * targetCollider.size.x),
                targetCollider.center.y + (normalizedPosition.y * targetCollider.size.y),
                targetCollider.center.z + (normalizedPosition.z * targetCollider.size.z)
            );
            Vector3 mappedWorldPosition = targetBox.transform.TransformPoint(targetLocalPosition);

            Debug.Log($"MapPointBetweenBoxes: Mapped {pointPosition} from {sourceBox.name} to {mappedWorldPosition} in {targetBox.name}");
            
            return mappedWorldPosition;
        }
        public void DestroyLastMimicPoint(ref List<GameObject> MimicHumanPoints, ref List<GameObject> MimicRobotPoints, ref GameObject MimicHumanLine, ref GameObject MimicRobotLine)
        {
            if (MimicHumanPoints.Count > 0 && MimicRobotPoints.Count > 0)
            {
                Debug.Log("DestroyLastMimicPoint: Destroying Last Mimic Point");
                Debug.Log("DestroyLastMimicPoint: Human Points Count: " + MimicHumanPoints.Count);
                Debug.Log("DestroyLastMimicPoint: Robot Points Count: " + MimicRobotPoints.Count);

                Destroy(MimicHumanPoints[MimicHumanPoints.Count - 1]);
                Destroy(MimicRobotPoints[MimicRobotPoints.Count - 1]);
                MimicHumanPoints.RemoveAt(MimicHumanPoints.Count - 1);
                MimicRobotPoints.RemoveAt(MimicRobotPoints.Count - 1);
                
                Color humanColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
                Color robotColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                DrawLineFromGameObjectList(MimicHumanPoints, MimicHumanLine, humanColor, 0.01f);
                DrawLineFromGameObjectList(MimicRobotPoints, MimicRobotLine, robotColor, 0.01f);
            }
            else
            {
                Debug.LogWarning("DestroyLastMimicPoint: Mimic Points are empty");
            }
        }
        public void DrawLineFromGameObjectList(List<GameObject> pointsList, GameObject lineObject, Color color, float lineWidth)
        {
            /*
            * Method is used to draw a line in the AR space
            */

            LineRenderer lineRenderer = lineObject.GetComponentInChildren<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.LogWarning("DrawLineFromGameObjectList: LineRenderer is null");
            }

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;

            if (pointsList == null || pointsList.Count < 2)
            {
                Debug.LogWarning("DrawLineFromGameObjectList: Not enough points to draw a line (need at least 2)");
                return;
            }

            lineRenderer.positionCount = pointsList.Count;
            for (int i = 0; i < pointsList.Count; i++)
            {
                GameObject point = pointsList[i];
                if (point != null)
                {
                    // Vector3 center = ObjectTransformations.FindGameObjectCenter(point);
                    Vector3 position = point.transform.position;
                    Vector3 localPosition = point.transform.localPosition;

                    Debug.Log($"DrawLineFromGameObjectList: Drawing Line from {position} With a local position of {localPosition}");
                    Debug.Log($"DrawLineFromGameObjectList: Drawing Line from {position}");
                    lineRenderer.SetPosition(i, position);
                }
                else
                {
                    Debug.LogWarning("DrawLineFromGameObjectList: Point is null");
                }
            }
        }
        public void UpdateLinePositionsByGameObjectPositionsList(List<GameObject> posGameObjectList, GameObject lineObject)
        {
            /*
            * Method is used to update the line positions in the AR space
            * based on the list of gameobject positions.
            */
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            int listLength = posGameObjectList.Count;
            if (listLength > 1)
            {
                lineRenderer.positionCount = listLength;
                for (int i = 0; i < listLength; i++)
                {
                    lineRenderer.SetPosition(i, posGameObjectList[i].transform.position);
                }
            }
            else
            {
                LineRenderer renderer = lineObject.GetComponentInChildren<LineRenderer>();
                if (renderer != null)
                {
                    renderer.positionCount = 0;
                }
                Debug.LogWarning("UpdateLinePositionsByGameObjectPositionsList: List length is 0.");
            }
        }
        public void DestroyUserInstatiatedMimicZoneObjects()
        {
            /*
            * Method is used to destroy the mimic zone objects in the AR space
            */
            if (MimicHumanPoints.Count > 0 && MimicRobotPoints.Count > 0)
            {
                ObjectInstantiaion.DestroyChildrenOfGameObject(MimicHumanPointsParent);
                ObjectInstantiaion.DestroyChildrenOfGameObject(MimicRobotPointsParent);

                MimicHumanPoints.Clear();
                MimicRobotPoints.Clear();

                MimicHumanLine.GetComponentInChildren<LineRenderer>().positionCount = 0;
                MimicRobotLine.GetComponentInChildren<LineRenderer>().positionCount = 0;

                MimicHumanObjects.SetActive(false);
                MimicRobotObjects.SetActive(false);
            }
            else
            {
                Debug.Log("DestroyMimicZoneObjects: Mimic Points are empty");
            }
        }
        public void DestroyRealtimeMimicZoneObjects()
        {
            /*
            * Method is used to destroy the mimic zone objects in the AR space
            */
            if (RealtimeMimicHumanPoints.Count > 0 && RealtimeMimicHumanPoints.Count > 0)
            {
                ObjectInstantiaion.DestroyChildrenOfGameObject(RealtimeMimicHumanPointsParent);
                ObjectInstantiaion.DestroyChildrenOfGameObject(RealtimeMimicRobotPointsParent);

                RealtimeMimicHumanPoints.Clear();
                RealtimeMimicRobotPoints.Clear();

                RealtimeMimicHumanLine.GetComponentInChildren<LineRenderer>().positionCount = 0;
                RealtimeMimicRobotLine.GetComponentInChildren<LineRenderer>().positionCount = 0;

                //TODO: CHECK IF I NEED THIS LINE....
                // RealtimeMimicObjects.SetActive(false);
            }
            else
            {
                Debug.Log("DestroyMimicZoneObjects: Mimic Points are empty");
            }
        }
        public static Quaternion AddAdditionalRotationForEndEffector(GameObject cameraObject)
        {
            if (cameraObject == null)
            {
                Debug.LogError("MapCameraRotationToRobotEndEffector: GameObject is null!");
                return Quaternion.identity;
            }

            // Use the GameObject's local X-axis
            Vector3 localXAxis = cameraObject.transform.right;

            // Create and apply the 90-degree rotation
            Quaternion rotate90XLocal = Quaternion.AngleAxis(90, localXAxis);
            Quaternion rotatedQuaternion = rotate90XLocal * cameraObject.transform.rotation;

            Debug.Log($"MapCameraRotationToRobotEndEffector: Original Rotation {cameraObject.transform.rotation.eulerAngles} -> Rotated {rotatedQuaternion.eulerAngles}");

            return rotatedQuaternion;
        }
        public void SetZoneOnlyCurrentZoneVisible(ProjectZones.CurrentZoneMode currentZone)
        {
            /*
            * Method is used to set the visibility of the current zone only
            */
            switch (currentZone)
            {
                case ProjectZones.CurrentZoneMode.None:
                    SetAllZonesVisible();                    
                    Debug.Log("SetZoneOnlyCurrentZoneVisible: No Zones to set visible");
                    break;
                case ProjectZones.CurrentZoneMode.Inference:
                    SetZoneVisiblity(databaseManager.ProjectZones.BoundaryZone, false);
                    SetZoneVisiblity(databaseManager.ProjectZones.InferenceZones, true);
                    SetZoneVisiblity(databaseManager.ProjectZones.MimicZones, false);
                    break;
                case ProjectZones.CurrentZoneMode.Mimic:
                    SetZoneVisiblity(databaseManager.ProjectZones.BoundaryZone, false);
                    SetZoneVisiblity(databaseManager.ProjectZones.InferenceZones, false);
                    SetZoneVisiblity(databaseManager.ProjectZones.MimicZones, true);
                    break;
                default:
                    Debug.LogWarning("SetZoneOnlyCurrentZoneVisible: Invalid Current Zone Mode");
                    break;
            }
        }
        public void SetAllZonesVisible()
        {
            //Sets all zones visible.
            SetZoneVisiblity(databaseManager.ProjectZones.BoundaryZone, true);
            SetZoneVisiblity(databaseManager.ProjectZones.InferenceZones, true);
            SetZoneVisiblity(databaseManager.ProjectZones.MimicZones, true);
        }
        public void SetZoneVisiblity(Dictionary<string, Zone> ZoneDict, bool Visiblity)
        {
            /*
            * Method is used to set the visibility of the zones in the AR space
            */
            if (ZoneDict != null)
            {
                Debug.Log("SetZoneVisiblity: Setting Zone Visiblity");
                foreach (KeyValuePair<string, Zone> entry in ZoneDict)
                {
                    if (entry.Value != null)
                    {
                        entry.Value.ZoneObject.SetActive(Visiblity);
                    }
                }
            }
            else
            {
                Debug.LogWarning("SetZoneVisiblity: Zones Dict is null");
            }
        }      
        public void CreateTextObjectBasedOnZone(Zone ZoneObject)
        {
            /*
            * Method is used to create a 3D text object on the instantiation of the gameobject
            * in the AR space.
            */

            float offset = 0.2f;
            float halfGameObjectHeight = ZoneObject.Box.zsize / 2 + offset;
            float fontSize = 0.75f;

            switch (ZoneObject.Name)
            {
                case "human_zone":
                    CreateZonesTextOnInstantiation(ZoneObject.ZoneObject, halfGameObjectHeight, "Human Zone", "HumanZoneText", fontSize);
                    break;
                case "robot_zone":
                    CreateZonesTextOnInstantiation(ZoneObject.ZoneObject, halfGameObjectHeight, "Robot Zone", "RobotZoneText", fontSize);
                    break;
                case "collaboration_zone":
                    CreateZonesTextOnInstantiation(ZoneObject.ZoneObject, halfGameObjectHeight, "Collaboration Zone", "CollaborationZoneText", fontSize);
                    break;
                case "pick_zone":
                    CreateZonesTextOnInstantiation(ZoneObject.ZoneObject, halfGameObjectHeight, "Pick Zone", "PickZoneText", fontSize);
                    break;
                case "boundary_zone":
                    CreateZonesTextOnInstantiation(ZoneObject.ZoneObject, halfGameObjectHeight, "Boundary Zone", "BoundaryZoneText", fontSize);
                    break;
                default:
                    Debug.LogWarning("CreateTextObjectBasedOnZone: Invalid Zone Name");
                    break;
            }
        }
        private void CreateZonesTextOnInstantiation(GameObject gameObject, float offsetDistance, string text, string textObjectName, float fontSize)
        {              
            /*
            * Method is used to create a 3D text object on the instantiation of the gameobject
            * in the AR space.
            */
            Vector3 center = ObjectTransformations.FindGameObjectCenter(gameObject);
            Vector3 offsetPosition = ObjectTransformations.OffsetPositionVectorByDistance(center, offsetDistance, "y");

            GameObject TextContainer = ObjectInstantiaion.CreateTextinARSpaceAsGameObject(
                text, textObjectName, fontSize,
                TextAlignmentOptions.Center, Color.white, offsetPosition,
                Quaternion.identity, true, true, gameObject);
        }  
        public static Vector3 MirrorPositionAcrossBox(GameObject box, Vector3 worldPoint)
    {
        var col = box.GetComponent<BoxCollider>();
        if (col == null) return worldPoint;

        // 1) Bring into the *local* coordinates of the box
        Vector3 local = box.transform.InverseTransformPoint(worldPoint);

        // 2) Reflect across the box’s center plane (X axis in local space)
        //    (if your “long” axis is different, use Y or Z instead)
        local.x = 2f * col.center.x - local.x;

        // 3) Send back out to world space
        return box.transform.TransformPoint(local);
    }

    public static Quaternion MirrorQuaternion(Quaternion pointRotation, Vector3 normal)
        {

            // Create a pure quaternion representing the plane normal
            Quaternion n = new Quaternion(normal.x, normal.y, normal.z, 0);

            Quaternion R = Quaternion.AngleAxis(180, normal);

            // Compute mirrored quaternion
            Quaternion mirrored = R * pointRotation * n;
            return mirrored;

        }
    public static Quaternion MirrorRotationAcrossCenter(Transform centerTransform, Quaternion pointARotation)
    {
        // Step 1: Compute relative rotation of pointA to center
        Quaternion relativeRotationA = Quaternion.Inverse(centerTransform.rotation) * pointARotation;

        // Step 2: Invert the relative rotation and apply it back to the center
        Quaternion mirroredRotationB = centerTransform.rotation * Quaternion.Inverse(relativeRotationA);

        Debug.Log($"MirrorRotationAcrossCenter: Original Rotation {pointARotation.eulerAngles} -> Mirrored Rotation {mirroredRotationB.eulerAngles}");

        return mirroredRotationB;
    }

        //TODO: TODO: TODO: TODO: TESTING GEOMETRY UPDATES UPDATEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
    public void OnObservedObjectsChangedWrapper(object source, UpdateObservedGeometryEventArgs e)
    {
        if(e.ObservedGeometryDict == null)
        {
            Debug.LogWarning("OnObservedObjectsChangedWrapper: ObservedGeometryDict is null.");
            return;
        }
        if(e.NewObservedGeometry == null)
        {
            Debug.LogWarning("OnObservedObjectsChangedWrapper: NewObservedGeometry is null.");
            return;
        }
        if(string.IsNullOrEmpty(e.Key))
        {
            Debug.LogWarning("OnObservedObjectsChangedWrapper: Key is null or empty.");
            return;
        }
        OnObservedGeometryUpdated(e.ObservedGeometryDict, e.NewObservedGeometry, e.Key);
    }
    public void OnObservedGeometryUpdated(Dictionary<string, ObservedGeometry> currentGeometryDict, ObservedGeometry observedGeometry, string key)
    {
        /*
        * Method is used to handle the observed geometry updates.
        */
        if (currentGeometryDict == null || observedGeometry == null || string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("OnObservedGeometryUpdated: Invalid parameters provided.");
            return;
        }

        if (observedGeometry == null)
        {
            if (currentGeometryDict.ContainsKey(key))
            {
                Debug.Log($"OnObservedGeometryUpdated: Removing geometry for key: {key}");
                currentGeometryDict.Remove(key);
                return;
            }
            else
            {
                Debug.LogWarning($"OnObservedGeometryUpdated: Key {key} not found in currentGeometry dictionary.");
                return;
            }
        }

            if (currentGeometryDict.TryGetValue(key, out var cur))
            {

                if (cur == null)
                {
                    Debug.LogWarning($"OnObservedGeometryUpdated: Current geometry for key {key} is null.");
                    return;
                }
                if (cur.Box?.frame != null && observedGeometry.Box?.frame != null)
                {
                    if (cur.Box.frame.IsSameAs(observedGeometry.Box.frame))
                    {
                        Debug.Log($"OnObservedGeometryUpdated JOEEEEE: {key} frame is unchanged and will not update.");
                        return; // skip update
                    }
                }

                // Update the existing object’s transform
                UpdateObservedGeometryLocation(cur, observedGeometry);

                // Merge data-only fields into the existing instance
                cur.Name = key;
                cur.Box = observedGeometry.Box;
                cur.Box.frame = observedGeometry.Box.frame;

                if (key == "AnchorCube")
                {
                    //Updating the position of the goals parents based on the anchor cube position.
                    ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(MimicGoalsParentObject, cur.Box.frame.point, cur.Box.frame.xaxis, cur.Box.frame.yaxis, false, false);
                    ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(InferenceGoalsParentObject, cur.Box.frame.point, cur.Box.frame.xaxis, cur.Box.frame.yaxis, false, false);
                    Debug.LogWarning($"OnObservedGeometryUpdated: AnchorCube position updated to {cur.Box.frame.point}");
                }
        }
            else
            {
                observedGeometry.Name = key;
                currentGeometryDict[key] = observedGeometry;

                // First-time creation: make the GameObject now
                var go = observedGeometry.Box.CreateBoxObject();
                go.name = observedGeometry.Name;
                go.transform.SetParent(TrackedGeometriesParentObject.transform, false);
                observedGeometry.GeometryObject = go;
            }


    }
    private void UpdateObservedGeometryLocation(ObservedGeometry currentGeometry, ObservedGeometry newGeometry)
    {
        /*
        * Method is used to update the observed geometry location.
        */
        if (currentGeometry == null || newGeometry == null)
        {
            Debug.LogWarning("UpdateObservedGeometryLocation: Invalid geometry provided.");
            return;
        }
        else
        {
            Debug.LogWarning($"JOETHISSHOULD MOVE: Updating geometry location for {currentGeometry.Name}");
        }
        if (currentGeometry.GeometryObject == null)
        {
            Debug.LogWarning("UpdateObservedGeometryLocation: Current geometry object is null.");
            return;
        }
        else
        {
            // currentGeometry.GeometryObject.GetComponentInChildren<Renderer>().material.color = Color.yellow; //TODO: TEMPORARY COLOR CHANGE FOR DEBUGGING
        }

        ObjectInstantiaion.UpdateExistingObjectFromRightHandFrameData(currentGeometry.GeometryObject, newGeometry.Box.frame.point, newGeometry.Box.frame.xaxis, newGeometry.Box.frame.yaxis, false, false);
        Debug.Log($"UpdateObservedGeometryLocation: Updated geometry location to {newGeometry.Box.frame.point} with x-axis {newGeometry.Box.frame.xaxis} and y-axis {newGeometry.Box.frame.yaxis}");
    }

    //TODO: //TODO: //TODO: //TODO: TEMPORARY ROBOTIC TERRITORIES TESTING REALTIME MIMIC
    public void CreateRealtimeMimicPointsBasicTEMPORARY(GameObject humanZoneObject, GameObject robotZoneObject, ref List<GameObject> realtimeMimicHumanPoints,
    ref List<GameObject> realtimeMimicRobotPoints, GameObject realtimeMimicHumanPointsParent, GameObject realtimeMimicRobotPointsParent,
    GameObject realtimeMimicHumanLine, GameObject realtimeMimicRobotLine, bool MimicMirrorToggle = false)
    {
        Vector3 position = cameraPositionObject.transform.position;
        Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: CAMERA Position FROM REALTIME MIMIC: {position}");
        Quaternion rotation = AddAdditionalRotationForEndEffector(cameraPositionObject); //TODO: Check this
        Color humanColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Color robotColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);


        CreateRealtimeMimicPointTEMPORARY(humanZoneObject, robotZoneObject, ref realtimeMimicHumanPoints,
        ref realtimeMimicRobotPoints, realtimeMimicHumanPointsParent, realtimeMimicRobotPointsParent, position,
        rotation, $"{realtimeMimicHumanPoints.Count}_MimicPoint", $"{realtimeMimicRobotPoints.Count}_MimicPoint", true, //TODO: ADDED THESE
        MimicMirrorToggle);


        // CreateSpheresForMimic(humanZone, robotZone, ref humanPoints, ref robotPoints, 
        // humanParent, robotParent, position, rotation, 
        // radius, humanColor, robotColor, $"{humanPoints.Count}_MimicPoint", $"{robotPoints.Count}_MimicPoint", true, Mirror);

        // // //TODO: Quick test for the closest reachable point:
        // // CreateSpheresForMimic(humanZone, robotZone, ref MimicHumanSystemProposedPoints, ref MimicRobotSystemProposedPoints, 
        // // MimicSystemProposedLineHuman, MimicSystemProposedLineRobot, MimicHumanSystemProposedPointsParent, MimicRobotSystemProposedPointsParent, 
        // // closestReachablePoint, rotation, radius, Color.red, Color.grey, $"{humanPoints.Count}_MimicPointProposal", $"{robotPoints.Count}_MimicPointProposal", false, Mirror);
        // //TODO: TESTING...
        // CreateSystemProposalPoints(humanZone, robotZone, trajectoryVisualizer.humanZoneMimicReachibility, ref humanPoints, 
        // ref MimicHumanSystemProposedPoints, ref MimicRobotSystemProposedPoints,
        // MimicSystemProposedLineHuman, MimicHumanSystemProposedPointsParent, MimicSystemProposedLineRobot, 
        // MimicRobotSystemProposedPointsParent, Mirror);

        Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Point is being set within the Robot Reachability.");

        if (realtimeMimicHumanPoints.Count > 1 && realtimeMimicRobotPoints.Count > 1)
        {
            Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Drawing Mimic Points Line");
            DrawLineFromGameObjectList(realtimeMimicHumanPoints, realtimeMimicHumanLine, humanColor, 0.01f);
            DrawLineFromGameObjectList(realtimeMimicRobotPoints, realtimeMimicRobotLine, robotColor, 0.01f);
        }
        else
        {
            Debug.LogWarning("CreateRealtimeMimicPointsBasicTEMPORARY: Realtime Mimic Points are empty or not enough points to draw a line.");
        }
    }

    public void CreateRealtimeMimicPointTEMPORARY(GameObject humanZone, GameObject robotZone, ref List<GameObject> humanPoints, 
    ref List<GameObject> robotPoints, GameObject humanParent, 
    GameObject robotParent,
    Vector3 position, Quaternion rotation, string humanPointName, string robotPointName, bool addToPointsList =true, //TODO: ADDED THESE
    bool Mirror=false)
    {

        Debug.Log("CreateRealtimeMimicPointTEMPORARY: Creating Realtime Mimic Point. Position: " + position + " Rotation: " + rotation.eulerAngles);
        GameObject humanPoint = new GameObject(humanPointName);
        humanPoint.transform.position = position; //TODO: CHECK IF THIS WORKED....
        humanPoint.transform.rotation = rotation;
        // GameObject humanPoint = CreateSphereAtPositionAndRotation(position, rotation, radius, humanColor, humanPointName);//$"{humanPoints.Count}_MimicPoint");
        humanPoint.transform.SetParent(humanParent.transform, true);
        if(addToPointsList)
        {
            humanPoints.Add(humanPoint);
        }
        else
        {
            Debug.LogWarning("CreateRealtimeMimicPointTEMPORARY: Human Point is not added to the list.");
        }

        Vector3 mappedRobotPosition = Vector3.zero;
        Quaternion mappedRotation = Quaternion.identity;
        
        if(!Mirror)
        {
            mappedRobotPosition = MapPointBetweenBoxes(humanZone, robotZone, position); //TODO: Check this
            mappedRotation = rotation; //TODO: Check this
        }
        else
        {
            // Vector3 mirroredPosition = MirrorPositionAcrossBox(humanZone, position, humanZone.transform.forward); //TODO: CHECK THIS IDK WHATS UP.
            Vector3 mirroredPosition = MirrorPositionAcrossBox(humanZone, position); //TODO: CHECK THIS IDK WHATS UP.
            // Vector3 mirroredPosition = MirrorPositionAcrossBox(humanZone, position, humanZone.transform.right); //TODO: CHECK THIS IDK WHATS UP.
            Quaternion mirrorRotation = MirrorQuaternion(rotation, humanZone.transform.right);
            mappedRobotPosition = MapPointBetweenBoxes(humanZone, robotZone, mirroredPosition);
            mappedRotation = mirrorRotation;
        }

        if (mappedRobotPosition == Vector3.zero)
        {
            Debug.LogWarning("CreateRealtimeMimicPointTEMPORARY: Mapped Robot Position is zero, cannot create robot point.");
            return;
        }

        GameObject robotPoint = new GameObject(robotPointName);
        robotPoint.transform.position = mappedRobotPosition;
        // GameObject robotPoint = CreateSphereAtPositionAndRotation(mappedRobotPosition, rotation, radius, robotColor, robotPointName);//, $"{robotPoints.Count}_MimicPoint");
        robotPoint.transform.rotation = mappedRotation;
        robotPoint.transform.SetParent(robotParent.transform, true);
        if(addToPointsList)
        {
            robotPoints.Add(robotPoint);
        }
        else
        {
            Debug.LogWarning("CreateRealtimeMimicPointTEMPORARY: Robot Point is not added to the list.");
        }
    }

    //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            /*
            * Method is used to initialize all the objects, variables, and dependencies
            * that are required for the instantiation of objects in the AR space.
            */

            //Find Additional Scripts.
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            scrollSearchManager = GameObject.Find("ScrollSearchManager").GetComponent<ScrollSearchManager>();
            trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();

            if (trajectoryVisualizer == null)
            {
                Debug.LogError("TrajectoryVisualizer is null");
            }

            if (scrollSearchManager == null)
            {
                Debug.LogWarning("ScrollSearchManager is null");
            }

            //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////

            //Find Parent Objects
            ZonesParentObject = GameObject.Find("ZonesParent");
            ZonesARPrefabObjects = GameObject.Find("ZonesARPrefabs");

            AllGeometiresParentObjects = GameObject.Find("Geometries");
            TrackedGeometriesParentObject = AllGeometiresParentObjects.FindObject("TrackedGeometriesParent");

            //TODO: TESTING MIMIC & INFERENCE GOALS
            InferenceGoalsParentObject = AllGeometiresParentObjects.FindObject("InferenceGoals");
            if (InferenceGoalsParentObject != null)
            {
                InferenceGoalsManager = new GoalManager(InferenceGoalsParentObject);
                foreach (GoalObject goal in InferenceGoalsManager.Goals)
                {
                    Debug.Log("GoalsManager Inference Goal: " + goal.Name);
                }
            }
            else
            {
                Debug.LogWarning("InferenceGoalsParentObject is null");
            }


            MimicGoalsParentObject = AllGeometiresParentObjects.FindObject("MimicGoals");
            if (MimicGoalsParentObject != null)
            {
                MimicGoalsManager = new GoalManager(MimicGoalsParentObject);
                Debug.Log("MimicGoalsManager initialized. With Goals Parent Object: " + MimicGoalsParentObject.name);
                foreach(GoalObject goal in MimicGoalsManager.Goals)
                {
                    Debug.Log("GoalsManager Mimic Goals: " + goal.Name);
                }
            }
            else
            {
                Debug.LogWarning("MimicGoalsParentObject is null");
            }

            BoundaryZoneParent = ZonesParentObject.FindObject("BoundaryZoneParent");
            InferenceZonesParent = ZonesParentObject.FindObject("InferenceZonesParent");
            MimicZonesParent = ZonesParentObject.FindObject("MimicZonesParent");
            
            ZonesParentObjectsDict.Add("BoundaryZoneParent", BoundaryZoneParent);
            ZonesParentObjectsDict.Add("InferenceZonesParent", InferenceZonesParent);
            ZonesParentObjectsDict.Add("MimicZonesParent", MimicZonesParent);

            //Finding Materials
            HumanZoneMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("HumanZone").GetComponentInChildren<Renderer>().material;
            RobotZoneMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("RobotZone").GetComponentInChildren<Renderer>().material;
            CollaborationZoneMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("CollaborationZone").GetComponentInChildren<Renderer>().material;
            PickZoneMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("PickZone").GetComponentInChildren<Renderer>().material;
            BoundaryMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("BoundaryMaterial").GetComponentInChildren<Renderer>().material;
            ObservedGeometryMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("ObservedBoxLocations").GetComponentInChildren<Renderer>().material;
            AnchorCubeMaterial = GameObject.Find("Materials").FindObject("RoboticTerritories").FindObject("AnchorCubeLocation").GetComponentInChildren<Renderer>().material;

            //FindObjects for Mimic Controls
            MimicHumanObjects = ZonesARPrefabObjects.FindObject("HumanObjects");
            MimicHumanPointsParent = MimicHumanObjects.FindObject("Points").FindObject("UserSetPoints");
            MimicHumanSystemProposedPointsParent = MimicHumanObjects.FindObject("Points").FindObject("SystemProposedPoints");
            MimicHumanLine = MimicHumanObjects.FindObject("HumanLine");
            MimicSystemProposedLineHuman = MimicHumanObjects.FindObject("HumanSystemProposedLine");
            MimicRobotObjects = ZonesARPrefabObjects.FindObject("RobotObjects");
            MimicRobotPointsParent = MimicRobotObjects.FindObject("Points").FindObject("UserSetPoints");
            MimicRobotSystemProposedPointsParent = MimicRobotObjects.FindObject("Points").FindObject("SystemProposedPoints");
            MimicRobotLine = MimicRobotObjects.FindObject("RobotLine");
            MimicSystemProposedLineRobot = MimicRobotObjects.FindObject("RobotSystemProposedLine");

            if(MimicHumanSystemProposedPointsParent == null || MimicRobotSystemProposedPointsParent == null)
            {
                Debug.LogWarning("MimicHumanSystemProposedPointsParent or MimicRobotSystemProposedPointsParent is null");
            }

            //TODO: TEMPORARY ROBOTIC TERRITORIES TESTING REALTIME MIMIC
            RealtimeMimicObjects = ZonesARPrefabObjects.FindObject("RealtimeMimicObjectsTEMPORARY");
            RealtimeMimicHumanLine = RealtimeMimicObjects.FindObject("HumanLine");
            RealtimeMimicRobotLine = RealtimeMimicObjects.FindObject("RobotLine");
            RealtimeMimicHumanPointsParent = RealtimeMimicObjects.FindObject("Points").FindObject("Human");
            RealtimeMimicRobotPointsParent = RealtimeMimicObjects.FindObject("Points").FindObject("Robot");

            if(RealtimeMimicHumanPointsParent == null || RealtimeMimicRobotPointsParent == null)
            {
                Debug.LogWarning("JOETESTING : RealtimeMimicHumanPointsParent or RealtimeMimicRobotPointsParent is null");
            }
            else if(RealtimeMimicHumanLine == null || RealtimeMimicRobotLine == null)
            {
                Debug.LogWarning("JOETESTING : RealtimeMimicHumanLine or RealtimeMimicRobotLine is null");
            }
            else if(RealtimeMimicHumanPointsParent == null || RealtimeMimicRobotPointsParent == null)
            {
                Debug.LogWarning("JOETESTING : RealtimeMimicHumanPointsParent or RealtimeMimicRobotPointsParent is null");
            }
            else
            {
                Debug.Log("JOE TESTING : RealtimeMimicHumanPointsParent and RealtimeMimicRobotPointsParent and Lines are not null");
            }

            //Find AR and system management items
            cameraPositionObject = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera");
            arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();


            //TODO: ROBOTIC TERRITORIES TESTING ////////////////////////////////////////////////////////////////////////////////////////

            //Find Parent Object to Store Our Items in.
            Elements = GameObject.Find("Elements");
            QRMarkers = GameObject.Find("QRMarkers");
            ActiveUserObjects = GameObject.Find("ActiveUserObjects");

            //Find Initial Materials
            BuiltMaterial = GameObject.Find("Materials").FindObject("Built").GetComponentInChildren<Renderer>().material;
            UnbuiltMaterial = GameObject.Find("Materials").FindObject("Unbuilt").GetComponentInChildren<Renderer>().material;
            HumanBuiltMaterial = GameObject.Find("Materials").FindObject("HumanBuilt").GetComponentInChildren<Renderer>().material;
            HumanUnbuiltMaterial = GameObject.Find("Materials").FindObject("HumanUnbuilt").GetComponentInChildren<Renderer>().material;
            RobotBuiltMaterial = GameObject.Find("Materials").FindObject("RobotBuilt").GetComponentInChildren<Renderer>().material;
            RobotUnbuiltMaterial = GameObject.Find("Materials").FindObject("RobotUnbuilt").GetComponentInChildren<Renderer>().material;
            LockedObjectMaterial = GameObject.Find("Materials").FindObject("LockedObjects").GetComponentInChildren<Renderer>().material;
            SearchedObjectMaterial = GameObject.Find("Materials").FindObject("SearchedObjects").GetComponentInChildren<Renderer>().material;
            ActiveRobotMaterial = GameObject.Find("Materials").FindObject("ActiveRobot").GetComponentInChildren<Renderer>().material;
            InactiveRobotMaterial = GameObject.Find("Materials").FindObject("InactiveRobot").GetComponentInChildren<Renderer>().material;
            OutlineMaterial = GameObject.Find("Materials").FindObject("OutlineMaterial").GetComponentInChildren<Renderer>().material;
            
            //Find GameObjects fo internal use
            IdxImage = GameObject.Find("ImageTagTemplates").FindObject("Circle");
            PriorityImage = GameObject.Find("ImageTagTemplates").FindObject("Triangle");
            MyUserIndacator = GameObject.Find("UserIndicatorPrefabs").FindObject("MyUserIndicatorPrefab");
            OtherUserIndacator = GameObject.Find("UserIndicatorPrefabs").FindObject("OtherUserIndicatorPrefab");
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");

        }
        public void PlaceElementFromStep(string Key, Step step)
        {
            /*
            * PlaceElementFromStep : Method is used to place an element in the AR space
            * based on the step information from the building plan data.
            */

            Debug.Log($"PlaceElement: {step.data.element_ids[0]} from Step: {Key}");

            //Load the correct object based on the step information
            GameObject geometry_object = gameobjectTypeSelector(step);
            if (geometry_object == null)
            {
                Debug.LogError($"PlaceElement: This key:{step.data.element_ids[0]} from Step: {Key} is null");
                return;
            }

            //Check if the object is suppose to be loaded as an .Obj file
            bool isObj = false;
            if (step.data.geometry == "2.ObjFile")
            {
                isObj = true;
            }

            //Instantiate the object in the AR space
            GameObject elementPrefab = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(geometry_object, step.data.location.point, step.data.location.xaxis, step.data.location.yaxis, isObj, databaseManager.z_remapped);
            StoreObjectLengthsPositionsOnInstantiation(Key, elementPrefab, databaseManager.ObjectLengthsDictionary);
            elementPrefab.transform.SetParent(Elements.transform, false);
            elementPrefab.name = Key;
            GameObject geometryObject = elementPrefab.FindObject(step.data.element_ids[0] + " Geometry");
            
            //Set AR Text objects for the element
            float heightOffset = getHeightOffsetByStepGeometryType(step, step.data.geometry);
            CreateTextForGameObjectOnInstantiation(elementPrefab, step.data.element_ids[0], heightOffset, $"{Key}", $"{elementPrefab.name}IdxText", 0.5f);
            CreateBackgroundImageForText(ref IdxImage, elementPrefab,  heightOffset, $"{elementPrefab.name}IdxImage", false);
            CreateTextForGameObjectOnInstantiation(elementPrefab, step.data.element_ids[0], heightOffset, $"{step.data.priority}", $"{elementPrefab.name}PriorityText", 0.5f);
            CreateBackgroundImageForText(ref PriorityImage, elementPrefab, heightOffset, $"{elementPrefab.name}PriorityImage", false);

            //Control color and visualization of the object
            ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, step, Key, geometryObject);
            if (UIFunctionalities.IDToggleObject.GetComponent<Toggle>().isOn)
            {
                elementPrefab.FindObject(elementPrefab.name + "IdxText").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "IdxImage").gameObject.SetActive(true);
            }
            if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
            {
                ColorObjectByPriority(UIFunctionalities.SelectedPriority, step.data.priority.ToString(), Key, geometryObject);
                elementPrefab.FindObject(elementPrefab.name + "PriorityText").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "PriorityImage").gameObject.SetActive(true);
            }
            if (Key == UIFunctionalities.CurrentStep)
            {
                ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
                UserIndicatorInstantiator(ref MyUserIndacator, elementPrefab, Key, Key, "ME", 0.25f);
            }
        }
        public float getHeightOffsetByStepGeometryType(Step step, string geometryType)
        {
            /*
            * Method is used to calculate the height offset of text and user objects
            * based on the geometry type of the step.
            */
            float heightOffset = 0.0f;
            switch (geometryType)
            {
                case "0.Cylinder":
                    heightOffset = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width * 3.0f;
                    break;
                case "1.Box":
                    heightOffset = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                    break;
                case "2.ObjFile":
                    heightOffset = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                    break;
                default:
                    heightOffset = 0.155f;
                    break;
            }
            return heightOffset;
        }
        public void placeElementsDict(Dictionary<string, Step> BuildingPlanDataDict)
        {
            /*
            * Method is used to place all the elements in the AR space
            * based on the building plan data dictionary.
            */
            if (BuildingPlanDataDict != null)
            {
                Debug.Log($"placeElementsDict: Number of key-value pairs in the dictionary = {BuildingPlanDataDict.Count}");
                foreach (KeyValuePair<string, Step> entry in BuildingPlanDataDict)
                {
                    if (entry.Value != null)
                    {
                        PlaceElementFromStep(entry.Key, entry.Value);
                    }

                }
                OnInitialObjectsPlaced();
            }
            else
            {
                Debug.LogWarning("The dictionary is null");
            }
        }   
        public GameObject gameobjectTypeSelector(Step step)
        {
            /*
            * Method is used to determine the type of gameobject to instantiate
            * based on the geometry type of the step.
            * Cylinder & Box will be recreated on the fly, while .Obj files will be loaded from the storage.
            */

            if (step == null)
            {
                Debug.LogWarning("Step is null. Cannot determine GameObject type.");
                return null;
            }

            GameObject element;

            switch (step.data.geometry)
                {
                    case "0.Cylinder":
                        element = new GameObject();
                        element.transform.position = Vector3.zero;
                        element.transform.rotation = Quaternion.identity;
                        float cylinderRadius = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                        float cylinderHeight = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.height;
                        Vector3 cylindersize = new Vector3(cylinderRadius*2, cylinderHeight/2, cylinderRadius*2);

                        GameObject cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        cylinderObject.transform.localScale = Vector3.one;
                        cylinderObject.transform.localScale = cylindersize;
                        cylinderObject.name = step.data.element_ids[0].ToString() + " Geometry";

                        BoxCollider cylinderCollider = cylinderObject.AddComponent<BoxCollider>();
                        Vector3 cylinderColliderSize = new Vector3(cylinderCollider.size.x*1.1f, cylinderCollider.size.y*1.2f, cylinderCollider.size.z*1.2f);
                        cylinderCollider.size = cylinderColliderSize;
                        cylinderObject.transform.SetParent(element.transform);
                        break;

                    case "1.Box":
                        element = new GameObject();
                        element.transform.position = Vector3.zero;
                        element.transform.rotation = Quaternion.identity;
                        Vector3 cubesize = new Vector3(databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.length, databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.height, databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width);
                        
                        GameObject boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        boxObject.transform.localScale = cubesize;
                        boxObject.name = step.data.element_ids[0].ToString() + " Geometry";

                        BoxCollider boxCollider = boxObject.AddComponent<BoxCollider>();
                        Vector3 boxColliderSize = new Vector3(boxCollider.size.x*1.1f, boxCollider.size.y*1.2f, boxCollider.size.z*1.2f);
                        boxCollider.size = boxColliderSize;
                        boxObject.transform.SetParent(element.transform);
                        break;

                    case "2.ObjFile":

                        string basepath = Application.persistentDataPath;
                        string folderpath = Path.Combine(basepath, "Object_Storage");
                        string filepath = Path.Combine(folderpath, step.data.element_ids[0]+".obj");

                        if (File.Exists(filepath))
                        {
                            element =  new OBJLoader().Load(filepath);
                        }
                        else
                        {
                            element = null;
                            Debug.LogError("gameobjectTypeSelector: ObjPrefab is null");
                        }
                        if (element!=null && element.transform.childCount > 0)
                        {
                            GameObject child_object = element.transform.GetChild(0).gameObject;
                            child_object.name = step.data.element_ids[0].ToString() + " Geometry";
                            BoxCollider collider = child_object.AddComponent<BoxCollider>();
                            Vector3 MeshSize = child_object.GetComponent<MeshRenderer>().bounds.size;
                            Vector3 colliderSize = new Vector3(MeshSize.x*1.1f, MeshSize.y*1.2f, MeshSize.z*1.2f);
                            collider.size = colliderSize;
                        }
                        break;

                    default:
                        Debug.LogWarning($"No element type found for type {step.data.geometry}");
                        return null;
                }

                Debug.Log($"gameobjectTypeSelector: Created Element of type {step.data.geometry}");
                return element;
            
        }
        private void CreateTextForGameObjectOnInstantiation(GameObject gameObject, string assemblyID, float offsetDistance, string text, string textObjectName, float fontSize)
        {              
            /*
            * Method is used to create a 3D text object on the instantiation of the gameobject
            * in the AR space.
            */
            GameObject childobject = gameObject.FindObject(assemblyID + " Geometry");
            Vector3 center = ObjectTransformations.FindGameObjectCenter(childobject);
            Vector3 offsetPosition = ObjectTransformations.OffsetPositionVectorByDistance(center, offsetDistance, "y");

            GameObject TextContainer = ObjectInstantiaion.CreateTextinARSpaceAsGameObject(
                text, textObjectName, fontSize,
                TextAlignmentOptions.Center, Color.white, offsetPosition,
                Quaternion.identity, true, false, gameObject);
        }
        private void CreateBackgroundImageForText(ref GameObject inputImg, GameObject parentObject, float verticalOffset,string imgObjectName, bool isVisible=true, bool isBillboard=true, bool storePositionData=true)
        {            
            /*
            * Method is used to create a background image for the 3D text object
            * in the AR space.
            */
            string elementID = databaseManager.BuildingPlanDataItem.steps[parentObject.name].data.element_ids[0];
            Vector3 centerPosition = ObjectTransformations.FindGameObjectCenter(parentObject.FindObject(elementID + " Geometry"));
            Vector3 offsetPosition = ObjectTransformations.OffsetPositionVectorByDistance(centerPosition, verticalOffset, "y");
            GameObject imgObject = ObjectInstantiaion.InstantiateObjectFromPrefabRefrence(ref inputImg, imgObjectName, offsetPosition, Quaternion.identity, parentObject);

            if (isBillboard)
            {
                HelpersExtensions.Billboard billboard = imgObject.AddComponent<HelpersExtensions.Billboard>();
            }
            if (storePositionData)
            {
                HelpersExtensions.ObjectPositionInfo positionData = imgObject.AddComponent<HelpersExtensions.ObjectPositionInfo>();
                positionData.StorePositionRotationScale(imgObject.transform.localPosition, imgObject.transform.localRotation, imgObject.transform.localScale);
            }
            imgObject.SetActive(isVisible);
        }
        public void UserIndicatorInstantiator(ref GameObject UserIndicator, GameObject parentObject, string stepKey, string namingBase, string inGameText, float fontSize)
        {            
            /*
            * Method is used to instantiate a user indicator object in the AR space
            * based on the step information from the building plan data.
            */
            if (UserIndicator == null)
            {
                Debug.LogError("Could Not find UserIndicator.");
                return;
            }

            Step step = databaseManager.BuildingPlanDataItem.steps[stepKey];
            GameObject element = Elements.FindObject(stepKey);
            GameObject geometryObject = element.FindObject(step.data.element_ids[0] + " Geometry");
            if (geometryObject == null)
            {
                Debug.LogError("Geometry Object not found.");
                return;
            }
            
            float heightOffset = getHeightOffsetByStepGeometryType(step, step.data.geometry);
            Vector3 objectCenter = ObjectTransformations.FindGameObjectCenter(geometryObject);
            Vector3 arrowOffset = ObjectTransformations.OffsetPositionVectorByDistance(objectCenter, heightOffset, "y");
            Quaternion rotationQuaternion = Quaternion.identity;

            GameObject newArrow = null;
            newArrow = ObjectInstantiaion.InstantiateObjectFromPrefabRefrence(ref UserIndicator, namingBase+" Arrow", arrowOffset, rotationQuaternion, parentObject);
            newArrow.AddComponent<HelpersExtensions.Billboard>();

            GameObject IndexTextContainer = ObjectInstantiaion.CreateTextinARSpaceAsGameObject(
                inGameText, $"{namingBase} UserText", fontSize,
                TextAlignmentOptions.Center, Color.white, newArrow.transform.position,
                newArrow.transform.rotation, true, true, newArrow);

            ObjectTransformations.OffsetGameObjectPositionByExistingObjectPosition(IndexTextContainer, newArrow, 0.12f , "y");
            newArrow.SetActive(true);
        }
        public void CreateNewUserObject(string UserInfoname, string itemKey)
        {
            /*
            * Method is used to create a new user object in the AR space
            * based on the user information.
            */
            GameObject userObject = new GameObject(UserInfoname);
            userObject.transform.SetParent(ActiveUserObjects.transform);
            userObject.transform.position = Vector3.zero;
            userObject.transform.rotation = Quaternion.identity;
            UserIndicatorInstantiator(ref OtherUserIndacator, userObject, itemKey, UserInfoname, UserInfoname, 0.15f);
        }
        public void StoreObjectLengthsPositionsOnInstantiation(string Key, GameObject gameObject, Dictionary<string, List<float>> ObjectLenthsDictionary)
        {
            /*
                Method is used to store the P1 and P2 positions of the element in the AR space
                Based on world Zero. This is used to calculate the position of the gameObject before
                the object is moved, rotated, or parent is set, as it is the most acurate in comparision to rhino.
            */

            if(ObjectLenthsDictionary.ContainsKey(Key))
            {
                ObjectLenthsDictionary.Remove(Key);
            }
            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2PositionsFromGameObjectStepKeyToWorldZero(gameObject, Key, false);
            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2PositionsFromGameObjectStepKeyToWorldZero(gameObject, Key, true);
            ObjectLengthsTags.FindObject("P1Tag").transform.position = P1Position;
            ObjectLengthsTags.FindObject("P2Tag").transform.position = P2Position;
            float P1distance = Vector3.Distance(P1Position, P1Adjusted);
            float P2distance = Vector3.Distance(P2Position, P2Adjusted);
            ObjectLenthsDictionary.Add(Key, new List<float> {P1distance, P2distance});
        }
        public (Vector3, Vector3) FindP1orP2PositionsFromGameObjectStepKeyToWorldZero(GameObject objectToMeasure, string key, bool isP2)
        {
            /*
            * Method is used to find the P1 or P2 positions of the element in the AR space
            * P1 is the center of the element - half of the height or length of the element
            * P2 is the center of the element + half of the height or length of the element
            */
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            Vector3 center = ObjectTransformations.FindGameObjectCenter(objectToMeasure);

            float offsetDistance;
            Vector3 offsetVector;
            if(step.data.geometry == "0.Cylinder")
            {
                offsetDistance =  databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.height;
                offsetVector = objectToMeasure.transform.up;
            }
            else
            {
                offsetDistance = databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.length;
                offsetVector = objectToMeasure.transform.right;
            }

            Vector3 ptPosition = new Vector3(0, 0, 0);
            if(!isP2)
            {                
                ptPosition = center + offsetVector * (offsetDistance / 2)* -1;
            }
            else
            {
                ptPosition = center + offsetVector * (offsetDistance / 2);
            }

            Vector3 worldZeroPosition = Vector3.zero;
            Vector3 ptPositionAdjusted = new Vector3(0,0,0);
            if (ptPosition != Vector3.zero)
            {
                ptPositionAdjusted = new Vector3(ptPosition.x, worldZeroPosition.y, ptPosition.z);
            }
            else
            {
                Debug.LogError("P1 or P2 Position is null.");
            }

            return (ptPosition, ptPositionAdjusted);
        }
        public (Vector3, Vector3) FindP1orP2Positions(string key, bool isP2)
        {
            /*
            * Method is used to find the P1 or P2 positions of the element in the AR space
            * P1 is the center of the element - half of the height or length of the element
            * P2 is the center of the element + half of the height or length of the element
            */

            GameObject element = Elements.FindObject(key);
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            Vector3 center = ObjectTransformations.FindGameObjectCenter(element.FindObject(step.data.element_ids[0] + " Geometry"));

            float offsetDistance;
            Vector3 offsetVector;
            if(step.data.geometry == "0.Cylinder")
            {
                offsetDistance =  databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.height;
                offsetVector = element.transform.up;
            }
            else
            {
                offsetDistance = databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.length;
                offsetVector = element.transform.right;
            }

            Vector3 ptPosition = new Vector3(0, 0, 0);
            if(!isP2)
            {                
                ptPosition = center + offsetVector * (offsetDistance / 2)* -1;
            }
            else
            {
                ptPosition = center + offsetVector * (offsetDistance / 2);
            }

            Vector3 ElementsPosition = Elements.transform.position;
            Vector3 ptPositionAdjusted = new Vector3(0,0,0);
            if (ptPosition != Vector3.zero)
            {
                ptPositionAdjusted = new Vector3(ptPosition.x, ElementsPosition.y, ptPosition.z);
            }
            else
            {
                Debug.LogError("P1 or P2 Position is null.");
            }

            return (ptPosition, ptPositionAdjusted);
        }
        public void CalculateandSetLengthPositions(string key)
        {
            /*
            * Method is used to calculate the P1 and P2 positions of the element in the AR space
            * and set the line positions and text for the object lengths.
            */

            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2Positions(key, false);
            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2Positions(key, true);
            ObjectLengthsTags.FindObject("P1Tag").transform.position = P1Position;
            ObjectLengthsTags.FindObject("P2Tag").transform.position = P2Position;
            if (ObjectLengthsTags.FindObject("P1Tag").GetComponent<HelpersExtensions.Billboard>() == null)
            {
                ObjectLengthsTags.FindObject("P1Tag").AddComponent<HelpersExtensions.Billboard>();
            }
            if (ObjectLengthsTags.FindObject("P2Tag").GetComponent<HelpersExtensions.Billboard>() == null)
            {
                ObjectLengthsTags.FindObject("P2Tag").AddComponent<HelpersExtensions.Billboard>();
            }

            float P1distance = Vector3.Distance(P1Position, P1Adjusted);
            float P2distance = Vector3.Distance(P2Position, P2Adjusted);
            LineRenderer P1Line = ObjectLengthsTags.FindObject("P1Tag").GetComponent<LineRenderer>();
            P1Line.useWorldSpace = true;
            P1Line.SetPosition(0, P1Position);
            P1Line.SetPosition(1, P1Adjusted);

            LineRenderer P2Line = ObjectLengthsTags.FindObject("P2Tag").GetComponent<LineRenderer>();
            P2Line.useWorldSpace = true;
            P2Line.SetPosition(0, P2Position);
            P2Line.SetPosition(1, P2Adjusted);

            UIFunctionalities.SetObjectLengthsTextFromStoredKey(key);
            // UIFunctionalities.SetObjectLengthsText(P1distance, P2distance);
        }
        public void UpdateObjectLengthsLines(string currentStep, GameObject p1LineObject, GameObject p2LineObject)
        {
            /*
            * Method is used to update the P1 and P2 positions of the element in the AR space
            * and set the line positions for the object lengths.
            */

            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2Positions(currentStep, false);
            List<Vector3> P1Positions = new List<Vector3> { P1Position, P1Adjusted };
            UpdateLinePositionsByVectorList(P1Positions, p1LineObject);

            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2Positions(currentStep, true);
            List<Vector3> P2Positions = new List<Vector3> { P2Position, P2Adjusted };
            UpdateLinePositionsByVectorList(P2Positions, p2LineObject);

        }
        public void CreateLineAndPointsForPriorityViewerToggle(string selectedPriority, ref GameObject lineObject, Color lineColor, float lineWidth, float ptRadius, Color ptColor, GameObject ptsParentObject)
        {
            /*
            * Method is used to create the priority viewer items in the AR space
            * based on the selected priority.
            */
            List<string> priorityList = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[selectedPriority];
            DrawLinefromKeyswithGameObjectReference(priorityList, ref lineObject, lineColor, lineWidth, true, ptColor, ptsParentObject);
        }
        public void DrawLinefromKeyswithGameObjectReference(List<string> keyslist, ref GameObject lineObject, Color lineColor, float lineWidth, bool createPoints=true, Color? ptColor=null, GameObject ptsParentObject=null)
        {
            /*
            * Method is used to draw a line in the AR space based on the list of keys
            * and create points if desired.
            */
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.Log("LineRenderer is null. for object: " + lineObject.name);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
            }

            if (ptsParentObject && ptsParentObject.transform.childCount > 0)
            {
                foreach (Transform child in ptsParentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            int listLength = keyslist.Count;
            if (listLength > 1)
            {
                lineRenderer.positionCount = keyslist.Count;

                for (int i = 0; i < keyslist.Count; i++)
                {
                    GameObject element = Elements.FindObject(keyslist[i]);
                    Vector3 center = ObjectTransformations.FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[keyslist[i]].data.element_ids[0] + " Geometry"));
                    lineRenderer.SetPosition(i, center);
                    if (createPoints)
                    {
                        if(ptColor != null)
                        {
                            CreateSphereForPriorityViewerFromGameObject(element, ptColor.Value, keyslist[i] + "Point", ptsParentObject);
                        }
                        else
                        {
                            Debug.Log("DrawLineFromKeys: Point Radius and Color not provided.");
                        }
                    }
                }
                lineObject.SetActive(true);
            }
            else
            {
                if(listLength != 0)
                {                        
                    if(createPoints)
                    {
                        if(ptColor != null)
                        {
                            lineObject.SetActive(false);
                            GameObject element = Elements.FindObject(keyslist[0]);
                            CreateSphereForPriorityViewerFromGameObject(element, ptColor.Value, keyslist[0] + "Point", ptsParentObject);
                        }
                        else
                        {
                            Debug.Log("DrawLineFromKeys: Point Radius and Color not provided.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("DrawLineFromKeys: List length is 0.");
                }
            }

        }
        public void UpdatePriorityLine(string selectedPriority, GameObject lineObject)
        {
            /*  
            * Method is used to update the priority line in the AR space
            * based on the selected priority.
            */
            Debug.Log($"UpdatingPriorityLine: priority {selectedPriority}");
            List<Vector3> priorityObjectPositions = GetPositionsFromPriorityGroup(selectedPriority);
            UpdateLinePositionsByVectorList(priorityObjectPositions, lineObject);
        }
        public void UpdateLinePositionsByVectorList(List<Vector3> posVectorList, GameObject lineObject)
        {
            /*
            * Method is used to update the line positions in the AR space
            * based on the list of vector positions.
            */
            LineRenderer lineRenderer = lineObject.GetComponentInChildren<LineRenderer>();
            int listLength = posVectorList.Count;
            if (listLength > 1)
            {

                for (int i = 0; i < posVectorList.Count; i++)
                {
                    lineRenderer.SetPosition(i, posVectorList[i]);
                }
            }
            else
            {
                Debug.LogWarning("UpdateLinePositionsByVectorList: List length is 0.");
            }
        }
        public GameObject CreateSphereForPriorityViewerFromGameObject(GameObject gameObject, Color color, string name=null, GameObject parentObject=null)
        {
            /*
            * Method is used to create a sphere object in the AR space
            * based on the the scale of the input game object.
            */
            Collider collider = gameObject.GetComponentInChildren<Collider>();
            Vector3 center = ObjectTransformations.FindGameObjectCenter(gameObject);
            float radius = collider.bounds.extents.magnitude;

            float scaleFactor;
            if(radius>0.75)
            {
                scaleFactor = 0.1f;
            }
            else if(radius<0.2)
            {
                scaleFactor = 0.3f;
            }
            else
            {
                scaleFactor = 0.2f;
            }

            float scaledRadius = radius * scaleFactor;
            GameObject sphere = CreateSphereAtPosition(center, scaledRadius, color, name, parentObject);
            return sphere;
        }
        public GameObject CreateSphereAtPosition(Vector3 position, float radius, Color color, string name=null, GameObject parentObject=null)
        {
            /*
            * Method is used to create a sphere object in the AR space
            * based on the position, radius, and color.
            */
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = new Vector3(radius, radius, radius);
            sphere.GetComponent<Renderer>().material.color = color;
            if (name != null)
            {
                sphere.name = name;
            }
            if (parentObject != null)
            {
                sphere.transform.SetParent(parentObject.transform);
            }
            return sphere;
        }
        public GameObject CreateSphereAtPositionAndRotation(Vector3 position, Quaternion rotation, float radius, Color color, string name=null, GameObject parentObject=null)
        {
            /*
            * Method is used to create a sphere object in the AR space
            * based on the position, radius, and color.
            */
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.rotation = rotation;
            sphere.transform.localScale = new Vector3(radius, radius, radius);
            sphere.GetComponent<Renderer>().material.color = color;
            if (name != null)
            {
                sphere.name = name;
            }
            if (parentObject != null)
            {
                sphere.transform.SetParent(parentObject.transform);
            }
            return sphere;
        }
        public void DestroyChildrenWithOutGeometryName(GameObject gameObject)
        {
            /*
            * Method is used to destroy the children of the game object without
            * name including "Geometry".
            */

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.name.Contains("Geometry") == false)
                {
                    Destroy(child.gameObject);
                }
            }
        }

    /////////////////////////////// POSITION AND ROTATION ////////////////////////////////////////
        public Quaternion GetQuaternionFromStepKey(string key)
        {
            /*
            * Method is used to get the quaternion from the step key
            * based on the x and y axis of the step data.
            */
            ObjectTransformations.Rotation rotationrh = ObjectTransformations.GetRotationFromRightHand(databaseManager.BuildingPlanDataItem.steps[key].data.location.xaxis, databaseManager.BuildingPlanDataItem.steps[key].data.location.yaxis); 
            ObjectTransformations.Rotation rotationlh = ObjectTransformations.RightHandToLeftHand(rotationrh.x , rotationrh.y);
            Quaternion rotationQuaternion = ObjectTransformations.GetQuaternion(rotationlh.y, rotationlh.z);
            return rotationQuaternion;
        }
        public List<Vector3> GetPositionsFromPriorityGroup(string priorityGroup)
        {
            /*
            * Method is used to get the positions from the game objects in a priority group
            * based on the priority group.
            */
            List<Vector3> positions = new List<Vector3>();
            List<string> keys = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[priorityGroup];
            foreach (string key in keys)
            {
                GameObject element = Elements.FindObject(key);
                Vector3 center = ObjectTransformations.FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0] + " Geometry"));
                positions.Add(center);
            }
            return positions;
        }

    /////////////////////////////// Material and colors ////////////////////////////////////////
        public void ObjectColorandTouchEvaluater(VisulizationMode visualizationMode, TouchMode touchMode, Step step, string key, GameObject geometryObject)
        {
            /*
            * Method is used to determine the color and touch of the object
            * based on the visulization mode and touch mode.
            */
            switch (visualizationMode)
            {
                case VisulizationMode.BuiltUnbuilt:
                    ColorBuiltOrUnbuilt(step.data.is_built, geometryObject);
                    break;
                case VisulizationMode.ActorView:
                    ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
                    break;
            }
            switch (touchMode)
            {
                case TouchMode.None:
                    break;
                case TouchMode.ElementEditSelection:
                    break;
            }
        }
        public void ColorObjectbyInputMaterial(GameObject gamobj, Material material)
        {
            /*
            * Method is used to color the object by the input material
            * based on the game object.
            */
            Renderer m_renderer= gamobj.GetComponentInChildren<MeshRenderer>();
            m_renderer.material = material; 
        }
        public void ColorBuiltOrUnbuilt(bool built, GameObject gamobj)
        {
            /*
            * Method is used to color the object based on the built status
            */
            Renderer m_renderer= gamobj.GetComponentInChildren<MeshRenderer>();

            if (built)
            {          
                m_renderer.material = BuiltMaterial; 
            }
            else
            {
                m_renderer.material = UnbuiltMaterial;
            }
        }
        public void ColorHumanOrRobot(string actor, bool builtStatus, GameObject gamobj)
        {
            /*
            * Method is used to color the object based on the actor and built status
            */            
            Renderer m_renderer= gamobj.GetComponentInChildren<Renderer>();
            if (actor == "HUMAN")
            {
                if(builtStatus)
                {
                    m_renderer.material = HumanBuiltMaterial;
                }
                else
                {
                    m_renderer.material = HumanUnbuiltMaterial; 
                }
            }
            else
            {
                if(builtStatus)
                {
                    m_renderer.material = RobotBuiltMaterial;
                }
                else
                {
                    m_renderer.material = RobotUnbuiltMaterial;
                }
            }
        }
        public void ColorObjectByPriority(string SelectedPriority, string StepPriority,string Key, GameObject gamobj)
        {
            /*
            * Method is used to color the object based on the selected priority
            * and the step priority.
            */
            Renderer m_renderer= gamobj.GetComponentInChildren<Renderer>();
            if (StepPriority != SelectedPriority)
            {
                m_renderer.material = OutlineMaterial;
            }
            else
            {
                Step step = databaseManager.BuildingPlanDataItem.steps[Key];
                string elementID = step.data.element_ids[0];
                ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, step, Key, gamobj.FindObject(elementID + " Geometry"));
            }
        }
        public void ApplyColorBasedOnBuildState()
        {
            /*
            * Method is used to apply color to objects based on their build state
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");

                    if (gameObject != null && geometryObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorBuiltOrUnbuilt(entry.Value.data.is_built, geometryObject);

                        //Check if other visibility options are on and need to be colored additionally.
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, geometryObject);
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                }
            }
        }
        public void ApplyColorBasedOnActor()
        {
            /*
            * Method is used to apply color to objects based on the actor
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");

                    if (gameObject != null && geometryObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorHumanOrRobot(entry.Value.data.actor, entry.Value.data.is_built, geometryObject);

                        //Check if other visibility options are on and need to be colored additionally.
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, geometryObject);
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                }
            }
        }
        public void ApplyColorBasedOnPriority(string SelectedPriority)
        {
            /*
            * Method is used to apply color to objects based on the selected priority
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");
                    if (gameObject != null && geometryObject != null)
                    {
                        if (entry.Key != UIFunctionalities.CurrentStep)
                        {
                            ColorObjectByPriority(SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry"));
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find object with key: {entry.Key}");
                    }
                }
            }
        }
        public void ApplyColortoPriorityGroup(string selectedPriorityGroup, string newPriorityGroup, bool newPriority=false)
        {
            /*
            * Method is used to apply color to objects based on the selected priority group
            */
            List<string> priorityList = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[selectedPriorityGroup];
            foreach (string key in priorityList)
            {
                GameObject gameObject = GameObject.Find(key);
                GameObject geometryObject = gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0] + " Geometry");

                if (gameObject != null && geometryObject != null)
                {
                    if (key != UIFunctionalities.CurrentStep)
                    {
                        if (newPriority)
                        {
                            ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, databaseManager.BuildingPlanDataItem.steps[key], key, geometryObject);
                            if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && key == scrollSearchManager.selectedCellStepIndex)
                            {
                                ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                            }
                        }
                        else
                        {
                            ColorObjectByPriority(newPriorityGroup, databaseManager.BuildingPlanDataItem.steps[key].data.priority.ToString(), key, geometryObject);
                            if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && key == scrollSearchManager.selectedCellStepIndex)
                            {
                                ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find object with key: {key}");
                }
            }
        }
        public void ApplyColorBasedOnAppModes()
        {
            /*
            * Method is used to apply color to objects based on the app modes
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");

                    if (gameObject != null && geometryObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, entry.Value, entry.Key, geometryObject);
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, geometryObject);
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                }
            }
        }

    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////
        public void OnDatabaseInitializedDict(object source, BuildingPlanDataDictEventArgs e)
        {
            /*
            * Method is used to handle the event when the database is initialized
            */
            Debug.Log("OnDatabaseInitializedDict: Database is loaded." + " " + "Number of Steps in the BuildingPlan " + e.BuildingPlanDataItem.steps.Count);
            placeElementsDict(e.BuildingPlanDataItem.steps);
        }
        public void OnDatabaseUpdate(object source, UpdateDataItemsDictEventArgs eventArgs)
        {
            /*
            * Method is used to handle the event when the database is updated
            */
            Debug.Log("OnDatabaseUpdate:" + " " + "Key of Step updated = " + eventArgs.Key);
            if (eventArgs.NewValue == null)
            {
                ObjectInstantiaion.DestroyGameObjectByName(eventArgs.Key);
                
                if(databaseManager.ObjectLengthsDictionary.ContainsKey(eventArgs.Key))
                {
                    databaseManager.ObjectLengthsDictionary.Remove(eventArgs.Key);
                }
            }
            else
            {
                InstantiateChangedKeys(eventArgs.NewValue, eventArgs.Key);
            }

        }
        public void OnUserInfoUpdate(object source, UserInfoDataItemsDictEventArgs eventArgs)
        {
            /*
            * Method is used to handle the event when the user info is updated
            */
            if (eventArgs.UserInfo == null)
            {
                Debug.Log($"OnUserInfoUpdate: User Info is null {eventArgs.Key} will be removed");
                ObjectInstantiaion.DestroyGameObjectByName(eventArgs.Key);
            }
            else
            {
                if (GameObject.Find(eventArgs.Key) != null)
                {
                    Debug.Log($"OnUserInfoUpdate: User {eventArgs.Key} updated their current step.");
                    ObjectInstantiaion.DestroyGameObjectByName(eventArgs.Key + " Arrow");
                    UserIndicatorInstantiator(ref OtherUserIndacator, GameObject.Find(eventArgs.Key), eventArgs.UserInfo.currentStep, eventArgs.Key, eventArgs.Key, 0.15f);
                }
                else
                {
                    Debug.Log($"OnUserInfoUpdate: New user joined and {eventArgs.Key} now join the assembly party :)");
                    CreateNewUserObject(eventArgs.Key, eventArgs.UserInfo.currentStep);
                }
            }
        }
        private void InstantiateChangedKeys(Step newValue, string key)
        {
            /*
            * Method is used to instantiate the changed keys on database events
            */
            if (GameObject.Find(key) != null)
            {
                Debug.Log("InstantiateChangedKeys: Deleting old object with key: " + key);
                GameObject oldObject = GameObject.Find(key);
                Destroy(oldObject);
            }
            else
            {
                Debug.Log( $"InstantiateChangedKeys: Could Not find Object with key: {key}");
            }
            PlaceElementFromStep(key, newValue);
        }
        protected virtual void OnInitialObjectsPlaced()
        {
            /*
            * Method is used to raise the event when the initial objects are placed
            */
            PlacedInitialElements(this, EventArgs.Empty);
            databaseManager.FindInitialElement();
        }
    }

    public static class ObjectInstantiaion
    {
        /*
        * ObjectInstantiation class is used to facilitate the placement of objects in the AR space
        * Class is used to handle the object instantiation in the AR space
        * It contains methods for the creation of 3D objects, text, and etc. in the AR space
        */
        public static GameObject CreateTextinARSpaceAsGameObject(string text, string gameObjectName, float fontSize, TextAlignmentOptions textAlignment, Color textColor, Vector3 position, Quaternion rotation, bool isBillboard, bool isVisible, GameObject parentObject=null, bool storePositionData=true)
        {
            GameObject textContainer = new GameObject(gameObjectName);
            textContainer.transform.position = position;
            textContainer.transform.rotation = rotation;

            TextMeshPro textMesh = textContainer.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.autoSizeTextContainer = true;
            textMesh.alignment = textAlignment;
            textMesh.color = textColor;

            if (isBillboard)
            {
                textContainer.AddComponent<HelpersExtensions.Billboard>();
            }
            if (parentObject != null)
            {
                textContainer.transform.SetParent(parentObject.transform);
            }
            if (storePositionData)
            {
                HelpersExtensions.ObjectPositionInfo positionData = textContainer.AddComponent<HelpersExtensions.ObjectPositionInfo>();
                positionData.StorePositionRotationScale(textContainer.transform.localPosition, textContainer.transform.localRotation, textContainer.transform.localScale);
            }
            textContainer.SetActive(isVisible);
            return textContainer;
        }
        public static GameObject InstantiateObjectFromPrefabRefrence(ref GameObject prefabReference, string gameObjectName, Vector3 position, Quaternion rotation, GameObject parentObject=null)
        {
            /*
            * Method is used to instantiate the object from the prefab reference
            */
            GameObject instantiatedObject = GameObject.Instantiate(prefabReference, position, rotation);
            instantiatedObject.name = gameObjectName;
            if (parentObject != null)
            {
                instantiatedObject.transform.SetParent(parentObject.transform);
            }
            return instantiatedObject;
        }
        public static GameObject InstantiateObjectFromRightHandFrameData(GameObject gameObject, float[] pointData, float[] xAxisData, float[] yAxisData, bool isObj, bool z_remapped)
        {
            /*
            * Method is used to instantiate the object from the right hand frame data
            * based on the point, x-axis, y-axis, and z-axis data.
            * This method serves as a simplified version of the placeElement method. And only requires a frame.
            * It loads the object, instantiates it at the correct place and then destroys the loaded object.
            */
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(pointData);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(xAxisData, yAxisData);
            Quaternion rotationQuaternion;

            if(isObj)
            {
                rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForObj(rotationData, z_remapped);
            }
            else
            {
                rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            }

            if(rotationQuaternion == null)
            {
                Debug.LogError("placeElement: Cannot assign object rotation because it is null");
            }

            GameObject elementPrefab = GameObject.Instantiate(gameObject, positionData, rotationQuaternion);
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
            return elementPrefab;
        }
        public static void DestroyGameObjectByName(string gameObjectName)
        {
            /*
            * Destroy the gameobject by input gameObjectName
            */

            if (GameObject.Find(gameObjectName) != null)
            {
                GameObject oldObject = GameObject.Find(gameObjectName);
                GameObject.Destroy(oldObject);
            }
            else
            {
                Debug.LogWarning($"DestroyGameObjectByName: Could Not find Object with key: {gameObjectName}");
            }
        }
        public static void DestroyChildrenOfGameObject(GameObject gameObject)
        {
            /*
            * Destroy the children of the gameobject
            */
            foreach (Transform child in gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        //TODO: RoboticTerritories Testing ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static GameObject UpdateExistingObjectFromRightHandFrameData(GameObject existingGameObject, float[] pointData, float[] xAxisData, float[] yAxisData, bool isObj, bool z_remapped)
        {
            /*
            * Method is used to instantiate the object from the right hand frame data
            * based on the point, x-axis, y-axis, and z-axis data.
            * This method serves as a simplified version of the placeElement method. And only requires a frame.
            * It loads the object, instantiates it at the correct place and then destroys the loaded object.
            */
            if (existingGameObject == null)
            {
                Debug.LogError("UpdateExistingObjectFromRightHandFrameData: Existing game object is null.");
                return null;
            }
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(pointData);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(xAxisData, yAxisData);
            Quaternion rotationQuaternion;

            if (isObj)
            {
                rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForObj(rotationData, z_remapped);
            }
            else
            {
                rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            }

            if (rotationQuaternion == null)
            {
                Debug.LogError("placeElement: Cannot assign object rotation because it is null");
            }

            existingGameObject.transform.localPosition = positionData;
            existingGameObject.transform.localRotation = rotationQuaternion;
            return existingGameObject;
        }
        public static bool IsPositionWithinObject(GameObject targetObject, Vector3 positionToCheck)
        {
            if (targetObject == null)
            {
                Debug.LogError("IsPositionWithinObject: Target object is null.");
                return false;
            }

            Collider collider = targetObject.GetComponent<Collider>();
            if (collider != null)
            {
                // Check if the position is within the collider's bounds
                return collider.bounds.Contains(positionToCheck);
            }
            else
            {
                Debug.LogError($"The target object {targetObject.name} does not have a collider.");
                return false;
            }
        }
        public static bool AllGameObjectsInListsPositionsAreWithinAnotherObject(List<GameObject> gameObjects, GameObject targetObject)
        {
            if (gameObjects == null || gameObjects.Count == 0)
            {
                Debug.LogError("AllGameObjectsInListsPositionsAreWithinAnotherObject: The list of game objects is null or empty.");
                return false;
            }

            foreach (GameObject gameObject in gameObjects)
            {
                if (!IsPositionWithinObject(targetObject, gameObject.transform.position))
                {
                    return false; // If any object is not within the target object, return false
                }
            }
            return true; // All objects are within the target object
            }
        public static bool Vector3sAreCloserThenThreshold(Vector3 pointA, Vector3 pointB, float threshold)
        {
            
            if (threshold <= 0)
            {
                Debug.LogError("Vector3sAreCloserThenThreshold: Threshold must be greater than zero.");
                return false;
            }
            else if (pointA == null || pointB == null)
            {
                Debug.LogError("Vector3sAreCloserThenThreshold: One or both of the points are null.");
                return false;
            }
            else if (pointA == pointB)
            {
                return true; // If the points are the same, they are definitely within the threshold
            }
            float distance = Vector3.Distance(pointA, pointB);
            return distance < threshold;
        }
    }
}