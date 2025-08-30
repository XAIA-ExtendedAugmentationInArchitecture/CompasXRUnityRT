using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using CompasXR.Core;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.Robots.MqttData;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Firebase.Extensions;
using CompasXR.Robots.MqttData.RoboticTerritories;
using CompasXR.Robots.Data;
using CompasXR.RoboticTerritories.Data;

namespace CompasXR.Robots
{
    /*
    * CompasXR.Robots : Is the namespace for all Classes that
    * controll the primary functionalities releated to the use of robots in the CompasXR Application.
    * Functionalities, such as robot communication, robot visualization, and robot interaction.
    */
    public class TrajectoryVisualizer : MonoBehaviour
    {
        /*
        The TrajectoryVisualizer class is responsible for managing the active robot in the scene,
        instantiating and visualizing robot trajectories, and controling placing active robot objects in the scene.
        */

        //Other script objects
        private InstantiateObjects instantiateObjects;
        private MqttTrajectoryManager mqttTrajectoryManager;
        private UIFunctionalities uiFunctionalities;

        //GameObjects for storing the active robot objects in the scene
        public GameObject ActiveRobotObjects;
        public GameObject ActiveRobot;
        public GameObject ActiveTrajectoryParentObject;
        private GameObject BuiltInRobotsParent;

        //Dictionary for storing URDFLinkNames associated with JointNames. Updated by recursive method from updating robot.
        public Dictionary<string, string> URDFLinkNames = new Dictionary<string, string>();
        public int? previousTrajectoryReviewSliderValue;
        public Dictionary<string, string> URDFRenderComponents = new Dictionary<string, string>();

        //List of available robots
        public List<string> RobotPreFabList = new List<string> {"UR3", "UR5", "UR10e", "UR20", "ETHZurichRFL"};

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////////////////////////////
        public Frame databaseRobotBaseFrame;
        public DatabaseManager databaseManager;
        public GameObject humanZoneMimicReachibility;

        public int? previousTrajectoryIndex;
        public int? previousConfigIndex;
            
        ////////////////////////////////////////// Monobehaviour Methods ////////////////////////////////////////////////////////
        void Start()
        {
            OnStartInitilization();
        }

        ////////////////////////////////////////// Initilization & Selection //////////////////////////////////////////////////////
        private void OnStartInitilization()
        {
            /*
            OnStartInitilization is called at the start of the script,
            and is responsible for finding and setting the necessary dependencies to objects that exist in the scene.
            */
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            uiFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            BuiltInRobotsParent = GameObject.Find("RobotPrefabs");
            ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");
        }
        public void SetActiveRobotFromDropdown(string robotName, bool yRotation, bool visibility = true)
        {
            /*
            SetActiveRobotFromDropdown is called from the UI Dropdown and is responsible for setting the active robot in the scene.
            */
            if(URDFLinkNames.Count > 0)
            {
                URDFLinkNames.Clear();
            }
            if(URDFRenderComponents.Count > 0)
            {
                URDFRenderComponents.Clear();
            }
            if(humanZoneMimicReachibility != null)
            {
                Destroy(humanZoneMimicReachibility);
            }

            SetActiveRobot(BuiltInRobotsParent, robotName, yRotation, ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectoryParentObject, instantiateObjects.InactiveRobotMaterial, visibility);
        
            //TODO: Updated for Robotic Territories /////////////////////////////////////////////////////////////////////////////////////////////////////
            if(databaseRobotBaseFrame != null)
            {
                URDFManagement.SetRobotLocalPositionandRotationFromFrame(databaseRobotBaseFrame, ActiveRobot);

                //TODO: This is a hot fix, but the code should probably be restructured.
                if(databaseManager.ProjectZones.CurrentZone == ProjectZones.CurrentZoneMode.Mimic)
                {
                    bool reachVisibiility = uiFunctionalities.ReachabilityToggleObject.GetComponent<Toggle>().isOn;
                    AddReachabilitlyToHumanZone(ActiveRobot.FindObject(mqttTrajectoryManager.serviceManager.ActiveRobotName), databaseManager.ProjectZones.MimicZones["human_zone"].ZoneObject, databaseManager.ProjectZones.MimicZones["robot_zone"].ZoneObject, reachVisibiility);
                }
            }
            else
            {
                Debug.Log("SetActiveRobotFromDropdown: Robot Base Frame is null.");
            }
        }
        private void SetActiveRobot(GameObject BuiltInRobotsParent, string robotName, bool yRotation, GameObject ActiveRobotObjectsParent, ref GameObject ActiveRobot, ref GameObject ActiveTrajectoryParentObject, Material material, bool visibility)
        {
            /*
            SetActiveRobot is responsible for setting the active robot in the scene.
            */
            GameObject selectedRobot = BuiltInRobotsParent.FindObject(robotName);

            if(selectedRobot != null)
            {
                if(ActiveRobot != null)
                {
                    Destroy(ActiveRobot);
                }
                if(ActiveTrajectoryParentObject != null)
                {
                    Destroy(ActiveTrajectoryParentObject);
                }
                GameObject temporaryRobot = Instantiate(selectedRobot, ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                temporaryRobot.name = selectedRobot.name;
                if(yRotation)
                {
                    temporaryRobot.transform.Rotate(0, 90, 0);
                }

                ActiveRobot = Instantiate(new GameObject(), ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                ActiveRobot.name = "ActiveRobot";
                ActiveRobot.transform.SetParent(ActiveRobotObjectsParent.transform);
                ActiveTrajectoryParentObject = Instantiate(new GameObject(), ActiveRobot.transform.position, ActiveRobot.transform.rotation);
                ActiveTrajectoryParentObject.name = "ActiveTrajectory";
                ActiveTrajectoryParentObject.transform.SetParent(ActiveRobotObjectsParent.transform);

                mqttTrajectoryManager.serviceManager.ActiveRobotName = robotName; //TODO: THIS IS FROM COMPAS XR, BUT NEEDS TO BE THOUGHT ABOUT FOR ROBOT TERRITORIES

                if(uiFunctionalities.ReachabilityToggleObject.GetComponent<Toggle>().isOn)
                {
                    SetReachabilityActive(temporaryRobot, visibility);
                }

                temporaryRobot.transform.SetParent(ActiveRobot.transform);
                URDFManagement.ColorURDFGameObject(temporaryRobot, material, ref URDFRenderComponents);
                temporaryRobot.SetActive(visibility);
            }
            else
            {
                Debug.Log($"SetActiveRobot: Robot {robotName} not found in the BuiltInRobotsParent.");

                string message = "WARNING: Active Robot could not be found. Confirm with planner which Robot is in use, or load robot.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ActiveRobotCouldNotBeFoundWarningMessage, "ActiveRobotCouldNotBeFoundWarningMessage", uiFunctionalities.MessagesParent, message, $"SetActiveRobot: Robot {robotName} could not be found");
            }
        }
        public void SetReachabilityActive(GameObject robotObject, bool visibility)
        {
            /*
            SetReachabilityActive is responsible for setting the reachability of the robot in the scene.
            */
            if(robotObject != null)
            {

                GameObject reachabilityObject = null;
                for (int i = 0; i < robotObject.transform.childCount; i++)
                {
                    GameObject child = robotObject.transform.GetChild(i).gameObject;
                    Debug.Log($"SetReachabilityActive: Checking child {i} with name {child.name}.");
                    if(child.name.Contains("Reachability"))
                    {
                        Debug.Log("SetReachabilityActive: Reachability object found in the robot object.");
                        reachabilityObject = child;
                    }
                }

                if(reachabilityObject != null)
                {
                    Debug.Log("SetReachabilityActive: Reachability object found in the robot object.");
                    reachabilityObject.SetActive(visibility);
                    if(databaseManager.ProjectZones.CurrentZone == ProjectZones.CurrentZoneMode.Mimic)
                    {
                        AddReachabilitlyToHumanZone(robotObject, databaseManager.ProjectZones.MimicZones["human_zone"].ZoneObject, databaseManager.ProjectZones.MimicZones["robot_zone"].ZoneObject, visibility);
                    }
                }
                else
                {
                    Debug.Log("SetReachabilityActive: Reachability object not found in the robot object.");
                }
            }
            else
            {
                Debug.Log("SetReachabilityActive: Robot object is null.");
            }
        }

        //TODO: Reachibility Sphere needs to be thought about a bit more in terms of positioning.
        //TODO: ZONES IN GENERAL NEED SOME MORE THOUGHT.... MAYBE THERE SHOULD BE MORE REFERENCE TO WHAT THE CARTESIAN SPACE IS LIKE.
        public void AddReachabilitlyToHumanZone(GameObject robotObject, GameObject humanZoneObject, GameObject robotZoneObject, bool visibility)
        {
            /*
            AddReachabilitlyToHumanZone is responsible for adding reachability to the human zone in the scene.
            */
            Debug.Log("AddReachabilitlyToHumanZone: Adding reachability to the human zone.");
            if(robotObject != null)
            {
                GameObject reachabilityObject = null;
                for (int i = 0; i < robotObject.transform.childCount; i++)
                {
                    GameObject child = robotObject.transform.GetChild(i).gameObject;
                    Debug.Log($"AddReachabilitlyToHumanZone: Checking child {i} with name {child.name}.");
                    if(child.name.Contains("Reachability"))
                    {
                        Debug.Log("AddReachabilitlyToHumanZone: Reachability object found in the robot object.");
                        reachabilityObject = child;
                    }
                }
                if(humanZoneObject != null && reachabilityObject != null)
                {
                    GameObject humanZoneReachability = instantiateObjects.ZonesARPrefabObjects.FindObject("MimicObjects").FindObject("HumanZoneMimicReachibility"); 
                    if(humanZoneReachability == null)
                    {
                        Debug.Log("AddReachabilitlyToHumanZone: HumanZoneMimicReachibility does not exist.");
                        humanZoneReachability = Instantiate(reachabilityObject, reachabilityObject.transform.position, reachabilityObject.transform.rotation);
                        humanZoneReachability.name = "HumanZoneMimicReachibility";
                        humanZoneReachability.transform.SetParent(instantiateObjects.ZonesARPrefabObjects.FindObject("MimicObjects").transform, true);
                        humanZoneMimicReachibility = humanZoneReachability;
                    }
                    else
                    {
                        Debug.Log("AddReachabilitlyToHumanZone: HumanZoneMimicReachibility already exists.");
                    }

                    Vector3 reachibilitysphereScale = ScaleReachibilitySphereProportionally(robotZoneObject, reachabilityObject, humanZoneObject);
                    humanZoneReachability.transform.localScale = reachibilitysphereScale;
                    
                    Vector3 tempPos = reachabilityObject.transform.position;
                    if(uiFunctionalities.UserInitiatedMimicMirrorToggle.isOn)
                    {
                        tempPos = InstantiateObjects.MirrorPositionAcrossBox(robotZoneObject, reachabilityObject.transform.position); //TODO: CHECK THIS IDK WHATS UP.
                        // tempPos = InstantiateObjects.MirrorPositionAcrossBox(robotZoneObject, reachabilityObject.transform.position, robotZoneObject.transform.right); //TODO: CHECK THIS IDK WHATS UP.
                    }
                    Vector3 position = InstantiateObjects.MapPointBetweenBoxes(robotZoneObject, humanZoneObject, tempPos);
                    humanZoneReachability.transform.position = position;
                    humanZoneReachability.SetActive(visibility);

                }
                else
                {
                    Debug.Log("AddReachabilitlyToHumanZone: HumanZoneObject or ReachabilityObject is null.");
                }
            }
            else
            {
                Debug.Log("AddReachabilitlyToHumanZone: Robot object is null.");
            }

        }
        public Vector3 ScaleReachibilitySphereProportionally(GameObject referenceBox, GameObject referenceSphere, GameObject targetBox)
        {
            if (referenceBox == null || referenceSphere == null || targetBox == null)
            {
                Debug.LogWarning("ScaleReachibilitySphereProportionally: One or more objects are not assigned.");
                return Vector3.zero;
            }

            // Get heights (assuming scale.y represents height)
            float referenceBoxHeight = referenceBox.transform.localScale.y;
            float referenceSphereHeight = referenceSphere.transform.localScale.y;

            // Compute the size ratio
            float sizeRatio = referenceSphereHeight / referenceBoxHeight;

            // Get the new height of Sphere 2 based on Box 2's height
            float targetBoxHeight = targetBox.transform.localScale.y;
            float newSphereHeight = targetBoxHeight * sizeRatio;

            // Apply the new scale to Sphere 2 (assuming uniform scale)
            Vector3 reachabilitySphere = new Vector3(newSphereHeight, newSphereHeight, newSphereHeight);
            return reachabilitySphere;
        }

        ////////////////////////////////////////// Robot Object Management ////////////////////////////////////////////////////////

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////////////////////////////

        //TODO: TEST UPDATED THIS FOR THE MULTIPLE TRAJECTORY PARSING.
        public void InstantateRobotFromInferenceResultMessage(InferenceResultMessage inferenceResultMessage, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            List<JointTrajectoryPoint> trajectoryPointsList = inferenceResultMessage.CombinedTrajectoryPoints;
            List<Trajectory> trajectoryList = inferenceResultMessage.Trajectories;
            Debug.Log($"InstantateRobotFromInferenceResultMessage: {trajectoryPointsList.Count} configurations.");
            
            if (trajectoryPointsList.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
            {
                InstatintaiteRobotFromTrajectoryList(trajectoryList, inferenceResultMessage.RobotBaseFrame, robotToConfigure, URDFLinks, parentObject, visibility);
                // InstantiateRobotTrajectoryFromJointTrajectoryPoints(trajectoryPointsList, mimicResult.RobotBaseFrame, robotToConfigure, URDFLinks, parentObject, visibility);
                // AttachedCollisionMesh attachedCollisionMesh = mimicResult.Trajectories[0].AttachedCollisionMeshes[0];
                // attachedCollisionMesh.CollisionMesh.Mesh.GenerateMeshFromRHMesh();
            }
            else
            {
                
                Debug.LogError("InstantateRobotFromInferenceResultMessage: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
        }

        public void InstantateRobotFromMimicMessage(MimicTrajectoryResultMessage mimicResult, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            List<JointTrajectoryPoint> trajectoryPointsList = mimicResult.CombinedTrajectoryPoints;
            List<Trajectory> trajectoryList = mimicResult.Trajectories;
            Debug.Log($"InstantiateRobotFromConfigList: {trajectoryPointsList.Count} configurations.");

            if (trajectoryPointsList.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
            {
                InstatintaiteRobotFromTrajectoryList(trajectoryList, mimicResult.RobotBaseFrame, robotToConfigure, URDFLinks, parentObject, visibility);
                // InstantiateRobotTrajectoryFromJointTrajectoryPoints(trajectoryPointsList, mimicResult.RobotBaseFrame, robotToConfigure, URDFLinks, parentObject, visibility);
                // AttachedCollisionMesh attachedCollisionMesh = mimicResult.Trajectories[0].AttachedCollisionMeshes[0];
                // attachedCollisionMesh.CollisionMesh.Mesh.GenerateMeshFromRHMesh();
            }
            else
            {

                Debug.LogError("InstantiateRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
        }
        public void InstatintaiteRobotFromTrajectoryList(List<Trajectory> trajectories, Frame robotBaseFrame, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            Debug.Log($"InstantiateRobotFromConfigList: {trajectories.Count} configurations.");
            
            if (trajectories.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
            {
                int trajectoryCount = trajectories.Count;
                for (int i = 0; i < trajectoryCount; i++)
                {
                    Debug.Log($"InstantiateRobotTrajectory: Instantiating Trajectory {trajectories.Count}.");
                    GameObject trajectoryParent = Instantiate(new GameObject());
                    trajectoryParent.name = $"Trajectory{i}";
                    trajectoryParent.transform.SetParent(parentObject.transform, false);
                    InstantiateRobotTrajectoryFromJointTrajectoryPoints(trajectories[i].Points, robotBaseFrame, robotToConfigure, URDFLinks, trajectoryParent, visibility);
                
                    if(trajectories[i].AttachedCollisionMeshes.Count > 0)
                    {
                        foreach (AttachedCollisionMesh attachedCollisionMesh in trajectories[i].AttachedCollisionMeshes)
                        {
                            GameObject acmGameObject = attachedCollisionMesh.CollisionMesh.Mesh.GenerateMeshFromRHMesh(attachedCollisionMesh.CollisionMesh.Id);
                            if(acmGameObject != null)
                            {
                                acmGameObject.transform.GetComponentInChildren<MeshRenderer>().material = instantiateObjects.InactiveRobotMaterial;
                                Debug.Log(attachedCollisionMesh.CollisionMesh.Id);
                                acmGameObject.name = attachedCollisionMesh.CollisionMesh.Id;
                                AttachCollisionMeshToTrajectoryConfigs(attachedCollisionMesh, trajectories[i], acmGameObject, trajectoryParent);
                            }
                            else
                            {
                                Debug.Log("InstatintaiteRobotFromTrajectoryList: Attached Collision Mesh is null.");
                            }
                            Destroy(acmGameObject);
                        }
                    }
                    else
                    {
                        Debug.Log("InstatintaiteRobotFromTrajectoryList: No attached collision meshes found.");
                    }
                }

                robotToConfigure.SetActive(false);
            }
            else
            {
                Debug.LogWarning("InstantiateRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
        }
        public void AttachCollisionMeshToTrajectoryConfigs(AttachedCollisionMesh acm, Trajectory trajectory, GameObject attachedCollisionMeshObject, GameObject trajectoryParent)
        {
            /*
            AttachCollisionMeshToTrajectoryConfigs is responsible for attaching the collision mesh to the trajectory configs.
            */

            Debug.Log($"AttachCollisionMeshToTrajectoryConfigs: For {trajectory} with {trajectory.Points.Count} configurations.");

            for (int i = 0; i < trajectory.Points.Count; i++)
            {
                Debug.Log($"AttachCollisionMeshToTrajectoryConfigs: Config {i} with {trajectory.Points[i].JointValues.Count} joints.");
                GameObject trajectoryConfig = trajectoryParent.FindObject($"Config {i}");
                if(trajectoryConfig != null)
                {
                    Debug.Log($"AttachCollisionMeshToTrajectoryConfigs: Trajectory Config {trajectoryConfig.name} found.");
                    GameObject linkToAttachTo = trajectoryConfig.FindObject(acm.LinkName);
                    if(linkToAttachTo != null)
                    {
                        attachedCollisionMeshObject = Instantiate(attachedCollisionMeshObject, linkToAttachTo.transform.position, linkToAttachTo.transform.rotation);
                        attachedCollisionMeshObject.name = $"{acm.CollisionMesh.Id}";
                        attachedCollisionMeshObject.transform.SetParent(linkToAttachTo.transform, true);
                    }
                    else
                    {
                        Debug.Log($"AttachCollisionMeshToTrajectoryConfigs: Link {acm.LinkName} not found in trajectory config {i}.");
                    }
                }
                else
                {
                    Debug.Log($"AttachCollisionMeshToTrajectoryConfigs: Trajectory Config {i} not found.");
                }
            }
        }
        public void InstantiateRobotTrajectoryFromJointTrajectoryPoints(List<JointTrajectoryPoint> points, Frame robotBaseFrame, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            Debug.Log($"InstantiateRobotFromConfigList: {points.Count} configurations.");
            
            if (points.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
            {
                int trajectoryCount = points.Count;
                for (int i = 0; i < trajectoryCount; i++)
                {
                    Debug.Log($"InstantiateRobotTrajectory: Config {i} with {points[i].JointValues.Count} joints.");

                    GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);

                    //TODO: This is hardcoded to turn off reachability once the robot is active.
                    if(uiFunctionalities.ReachabilityToggleObject.GetComponent<Toggle>().isOn)
                    {
                        SetReachabilityActive(temporaryRobot.transform.GetChild(0).gameObject, false);
                    }

                    temporaryRobot.name = $"Config {i}";

                    SetRobotConfigfromDictWrapper(points[i].JointsDict, $"Config {i}", temporaryRobot, URDFLinkNames);
                    temporaryRobot.transform.SetParent(parentObject.transform);
                    
                    URDFManagement.SetRobotLocalPositionandRotationFromFrame(robotBaseFrame, temporaryRobot);
                    temporaryRobot.SetActive(visibility);
                }

                robotToConfigure.SetActive(false);

                //TODO: This will get replaces with the collision mesh parsing
                // if(result.PickAndPlace)
                // {    
                //     StartCoroutine(AttachElementAfterDelay(result, parentObject, 0.2f));
                // }
            }
            else
            {
                
                Debug.LogError("InstantiateRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
        }

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////////////////////////////
        public void InstantiateRobotTrajectoryFromJointsDict(GetTrajectoryResult result, List<Dictionary<string, float>> TrajectoryConfigs, Frame robotBaseFrame, string trajectoryID, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            Debug.Log($"InstantiateRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");
            
            if (TrajectoryConfigs.Count > 0 && robotToConfigure != null && URDFLinks.Count > 0 || parentObject != null)
            {
                int trajectoryCount = TrajectoryConfigs.Count;
                for (int i = 0; i < trajectoryCount; i++)
                {
                    Debug.Log($"InstantiateRobotTrajectory: Config {i} with {TrajectoryConfigs[i].Count} joints.");

                    GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                    temporaryRobot.name = $"Config {i}";

                    SetRobotConfigfromDictWrapper(TrajectoryConfigs[i], $"Config {i}", temporaryRobot, URDFLinkNames);
                    temporaryRobot.transform.SetParent(parentObject.transform);
                    
                    URDFManagement.SetRobotLocalPositionandRotationFromFrame(robotBaseFrame, temporaryRobot);
                    temporaryRobot.SetActive(visibility);
                }

                if(result.PickAndPlace)
                {    
                    StartCoroutine(AttachElementAfterDelay(result, parentObject, 0.2f));
                }
            }
            else
            {
                
                Debug.LogError("InstantiateRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
            
        }
        public void VisualizeRobotTrajectoryFromResultMessage(GetTrajectoryResult result, Dictionary<string,string> URDFLinkNames, GameObject robotToConfigure, GameObject parentObject, bool visibility)
        {
            /*
            VisualizeRobotTrajectoryFromJointsDict is responsible for visualizing the robot trajectory in the scene.
            */

            Debug.Log($"VisualizeRobotTrajectory: For {result.TrajectoryID} with {result.Trajectory} configurations.");
            if(!ActiveRobot.transform.GetChild(0).gameObject.activeSelf)
            {
                ActiveRobot.transform.GetChild(0).gameObject.SetActive(true);
            }
            ActiveRobot.SetActive(false);
            InstantiateRobotTrajectoryFromJointsDict(result, result.Trajectory, result.RobotBaseFrame, result.TrajectoryID, robotToConfigure, URDFLinkNames, parentObject, visibility);
        }
        IEnumerator AttachElementAfterDelay(GetTrajectoryResult result, GameObject parentObject, float delay = 0.1f)
        {
            yield return new WaitForSeconds(delay);
            AttachElementToTrajectoryEndEffectorLinks(result.ElementID, parentObject.name, result.RobotName, result.EndEffectorLinkName, result.PickIndex.Value, result.Trajectory.Count);
        }
        public void AttachElementToTrajectoryEndEffectorLinks(string stepID, string trajectoryParentName, string robotName, string endEffectorLinkName, int pickIndex, int trajectoryCount)
        {
            /*
            AttachElementToTrajectoryEndEffectorLinks is responsible for attaching an element to the end effector link in the trajectory GameObject.
            */

            int lastConfigIndex = trajectoryCount - 1;
            GameObject stepElement = GameObject.Find(stepID);

            GameObject TrajectoryParent = GameObject.Find(trajectoryParentName);
            GameObject endEffectorLink = TrajectoryParent.FindObject($"Config {lastConfigIndex}").FindObject(robotName).FindObject(endEffectorLinkName);
            GameObject newStepElement = Instantiate(stepElement);

            //Remove all children from the stepElement
            Renderer stepChildRenderer = newStepElement.GetComponentInChildren<MeshRenderer>();
            stepChildRenderer.material = instantiateObjects.InactiveRobotMaterial;
            instantiateObjects.DestroyChildrenWithOutGeometryName(newStepElement);
            newStepElement.name = $"AttachedElement{lastConfigIndex}";

            newStepElement.transform.SetParent(endEffectorLink.transform, true);
            newStepElement.transform.position = stepElement.transform.position;
            newStepElement.transform.rotation = stepElement.transform.rotation;

            Vector3 position = newStepElement.transform.localPosition;
            Quaternion rotation = newStepElement.transform.localRotation;

            for (int i = lastConfigIndex - 1; i >= pickIndex; i--)
            {
                GameObject currentEndEffectorLink = TrajectoryParent.FindObject($"Config {i}").FindObject(endEffectorLinkName);
                GameObject attachedStepElement = Instantiate(newStepElement);
                attachedStepElement.transform.SetParent(currentEndEffectorLink.transform, true);
                instantiateObjects.DestroyChildrenWithOutGeometryName(attachedStepElement);
                attachedStepElement.name = $"AttachedElement{i}";
                attachedStepElement.transform.localPosition = position;
                attachedStepElement.transform.localRotation = rotation;
            }

        }
        public void DestroyActiveRobotObjects()
        {
            /*
            DestroyActiveRobotObjects is responsible for destroying the active robot objects in the scene.
            */

            if(ActiveRobot != null)
            {
                Destroy(ActiveRobot);
            }
            if(ActiveTrajectoryParentObject != null)
            {
                Destroy(ActiveTrajectoryParentObject);
            }
        }
        public void DestroyActiveTrajectoryChildren()
        {
            /*
            DestroyActiveTrajectoryChildren is responsible for destroying child objects in the trajectory parent.
            */
            if(ActiveTrajectoryParentObject != null)
            {
                foreach (Transform child in ActiveTrajectoryParentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        public void SetRobotConfigfromDictWrapper(Dictionary<string, float> config, string configName, GameObject robotToConfigure, Dictionary<string, string> urdfLinkNames)
        {
            /*
            SetRobotConfigfromDictWrapper is responsible for setting the robot configuration from a dictionary.
            */

            Debug.Log($"SetRobotConfigfromDictWrapper: Visulizing robot configuration for gameObject {robotToConfigure.name}.");
            
            if (urdfLinkNames.Count == 0)
            {
                URDFManagement.FindLinkNamesFromJointNames(robotToConfigure.transform, config, ref urdfLinkNames);
            }
            if(URDFManagement.ConfigJointsEqualURDFLinks(config, urdfLinkNames))
            {
                URDFManagement.SetRobotConfigfromJointsDict(config, robotToConfigure, urdfLinkNames);
            }
            else
            {
                if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject == null)
                {
                    string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visualized.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, "ConfigDoesNotMatchURDFStructureWarningMessage", uiFunctionalities.MessagesParent, message, "SetRobotConfigfromDictWrapper: Config does not match URDF");
                }
                else if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject.activeSelf == false)
                {
                    string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visualized.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, "ConfigDoesNotMatchURDFStructureWarningMessage", uiFunctionalities.MessagesParent, message, "SetRobotConfigfromDictWrapper: Config does not match URDF");
                }

                Debug.LogWarning($"SetRobotConfigfromDictWrapper: Config dict {config.Count} (Count) and LinkNames dict {urdfLinkNames.Count} (Count) for search do not match.");
            }
        }
        public void ColorRobotConfigfromSliderInput(int sliderValue, Material inactiveMaterial, Material activeMaterial, ref int? previousTrajectoryReviewSliderValue)
        {
            /*
            ColorRobotConfigfromSlider is responsible for coloring the robot configuration from the slider input for trajectory review.
            */
            Debug.Log($"ColorRobotConfigfromSlider: Coloring robot config {sliderValue} for active trajectory.");
            if(previousTrajectoryReviewSliderValue != null)
            {
                GameObject previousRobotGameObject = ActiveTrajectoryParentObject.FindObject($"Config {previousTrajectoryReviewSliderValue}");
                URDFManagement.ColorURDFGameObject(previousRobotGameObject, inactiveMaterial, ref URDFRenderComponents);

                //Attached GameObject
                GameObject previousAttachedGameObject = previousRobotGameObject.FindObject($"AttachedElement{previousTrajectoryReviewSliderValue}");
                if(previousAttachedGameObject != null)
                {
                    previousAttachedGameObject.GetComponentInChildren<Renderer>().material = inactiveMaterial;
                }
            }

            GameObject robotGameObject = ActiveTrajectoryParentObject.FindObject($"Config {sliderValue}");
            if (robotGameObject == null)
            {
                Debug.Log($"ColorRobotConfigfromSlider: Robot GameObject not found for Config {sliderValue}.");
            }
            URDFManagement.ColorURDFGameObject(robotGameObject, activeMaterial, ref URDFRenderComponents);

            //Attached GameObject
            GameObject attachedGameObject = robotGameObject.FindObject($"AttachedElement{sliderValue}");
            if(attachedGameObject != null)
            {
                attachedGameObject.GetComponentInChildren<Renderer>().material = activeMaterial;
            }

            previousTrajectoryReviewSliderValue = sliderValue;
        }
        public void ColorRobotConfigfromSliderInputCompoundTrajectories(int trajectoryIndex, int configIndex, List<Trajectory> trajectories, Material inactiveMaterial, Material activeMaterial, ref int? previousConfigIndex, ref int? previoustrajectoryIndex)
        {
            /*
            ColorRobotConfigfromSlider is responsible for coloring the robot configuration from the slider input for trajectory review.
            */
            Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Coloring robot Trajectory {trajectoryIndex} config {configIndex} for active trajectory.");
            if(previousConfigIndex != null && previoustrajectoryIndex != null)
            {
                GameObject previousRobotGameObject = ActiveTrajectoryParentObject.FindObject($"Trajectory{previousTrajectoryIndex.Value}").FindObject($"Config {previousConfigIndex.Value}");
                URDFManagement.ColorURDFGameObject(previousRobotGameObject, inactiveMaterial, ref URDFRenderComponents);

                //Attached GameObject
                Trajectory previousTrajectory = trajectories[previoustrajectoryIndex.Value];
                if(previousTrajectory == null)
                {
                    Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Previous Trajectory {previoustrajectoryIndex} is null.");
                }


                //TODO: FIX ME... THIS IS NOT WORKING PROPERLY.
                List<AttachedCollisionMesh> previousAttachedCollisionMeshes = previousTrajectory.AttachedCollisionMeshes;
                if (previousAttachedCollisionMeshes.Count > 0)
                {
                    foreach (AttachedCollisionMesh previousattachedCollisionMesh in previousAttachedCollisionMeshes)
                    {
                        GameObject previousAttachedGameObject = previousRobotGameObject.FindObject(previousattachedCollisionMesh.CollisionMesh.Id);
                        if (previousAttachedGameObject != null)
                        {
                            previousAttachedGameObject.GetComponentInChildren<Renderer>().material = inactiveMaterial;
                        }
                        else
                        {
                            Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Attached GameObject not found for Trajectory {previoustrajectoryIndex} Config {previousConfigIndex} id {previousattachedCollisionMesh.CollisionMesh.Id}.");
                        }
                    }
                }
                else
                {
                    Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Attached GameObject not found for Trajectory {previoustrajectoryIndex} Config {previousConfigIndex}.");
                }
            }

            GameObject robotGameObject = ActiveTrajectoryParentObject.FindObject($"Trajectory{trajectoryIndex}").FindObject($"Config {configIndex}");
            if (robotGameObject == null)
            {
                Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Robot GameObject not found for Trajectory {trajectoryIndex} Config {configIndex}.");
            }
            URDFManagement.ColorURDFGameObject(robotGameObject, activeMaterial, ref URDFRenderComponents);

            //Attached GameObject
            Trajectory trajectory = trajectories[trajectoryIndex];
            List<AttachedCollisionMesh> attachedCollisionMeshes = trajectory.AttachedCollisionMeshes;
            if(attachedCollisionMeshes != null)
            {
                foreach (AttachedCollisionMesh attachedCollisionMesh in attachedCollisionMeshes)
                {
                    GameObject attachedGameObject = robotGameObject.FindObject(attachedCollisionMesh.CollisionMesh.Id);
                    if(attachedGameObject != null)
                    {
                        attachedGameObject.GetComponentInChildren<Renderer>().material = activeMaterial;
                    }
                    else
                    {
                        Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Attached GameObject not found for Trajectory {previoustrajectoryIndex} Config {previousConfigIndex} id {attachedCollisionMesh.CollisionMesh.Id}.");
                    }
                }
            }
            else
            {
                Debug.Log($"ColorRobotConfigfromSliderInputCompoundTrajectories: Attached GameObject not found for Trajectory {trajectoryIndex} Config {configIndex}.");
            }

            previoustrajectoryIndex = trajectoryIndex;
            previousConfigIndex = configIndex;
        }

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////////////////////////////

        //TODO: This needs to be updaeted to fix robots based on the children (based on the robot name.)
        public void OnRobotBaseFrameReceived(object source, RobotBaseFrameReceivedEventArgs e)
        {
            /*
            OnRobotBaseFrameReceived is responsible for setting the robot base frame in the scene.
            */
            if (uiFunctionalities.SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
            {
                URDFManagement.SetRobotLocalPositionandRotationFromFrame(e.RobotBaseFrame, ActiveRobot);
                if (ActiveRobot != null)
                {
                    //TODO: ADDED FOR TESTING...
                    //TODO: THiS SERVICE MANAGER NEEDS TO BE UPDATED AS THE COMPAS XR ONE DOES...
                    if (databaseManager.ProjectZones.CurrentZone == ProjectZones.CurrentZoneMode.Mimic)
                    {
                        AddReachabilitlyToHumanZone(ActiveRobot.FindObject(mqttTrajectoryManager.serviceManager.ActiveRobotName),
                        databaseManager.ProjectZones.MimicZones["human_zone"].ZoneObject,
                        databaseManager.ProjectZones.MimicZones["robot_zone"].ZoneObject,
                        uiFunctionalities.ReachabilityToggleObject.GetComponentInChildren<Toggle>().isOn);
                    }
                }
                databaseRobotBaseFrame = e.RobotBaseFrame;
            }
            else
            {
                databaseRobotBaseFrame = e.RobotBaseFrame;
                Debug.Log("OnRobotBaseFrameReceived: SetActiveRobotToggle is not on but robot baseframe is updates.");
            }
        }
    }



    //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////////////////////////////

    public static class URDFManagement
    {
        /*
        * URDFManagement : Is a static class that contains methods for managing URDF objects in the scene.
        * URDFManagement is responsible for finding, setting, coloring, and visualizing robot configurations in the scene.
        */
        public static void SetRobotConfigfromList(List<float> config, GameObject URDFGameObject, List<string> jointNames)
        {
            /*
            SetRobotConfigfromList is responsible for setting the robot configuration to the URDF from a list of joint values.
            */
            Debug.Log($"SetRobotConfigFromList: Visulizing robot configuration for gameObject {URDFGameObject.name}.");
            int configCount = config.Count;

            for (int i = 0; i < configCount; i++)
            {
                GameObject joint = URDFGameObject.FindObject(jointNames[i]);
                if (joint)
                {
                    JointStateWriter jointStateWriter = joint.GetComponent<JointStateWriter>();
                    UrdfJoint urdfJoint = joint.GetComponent<UrdfJoint>();
                    if (!jointStateWriter)
                    {
                        jointStateWriter = joint.AddComponent<JointStateWriter>();    
                    }
                    
                    jointStateWriter.Write(config[i]);
                }  
                else
                {
                    Debug.Log($"SetRobotConfigfromList: Joint {joint.name} not found in the robotToConfigure.");
                }
            }

        }
        public static void SetRobotConfigfromJointsDict(Dictionary<string, float> config, GameObject URDFGameObject, Dictionary<string, string> linkNamesStorageDict)
        {
            /*
            SetRobotConfigfromJointsDict is responsible for setting the robot configuration to the URDF from a dictionary of joint values.
            */
            Debug.Log($"SetRobotConfigFromDict: Visulizing robot configuration for gameObject {URDFGameObject.name}.");    

            List<Task> setConfigTaskList = new List<Task>();

            foreach (KeyValuePair<string, float> jointDescription in config)
            {
                string jointName = jointDescription.Key;
                float jointValue = jointDescription.Value;
                string urdfLinkName = linkNamesStorageDict[jointName];
                GameObject urdfLinkObject = URDFGameObject.FindObject(urdfLinkName);

                if (urdfLinkObject)
                {
                    JointStateWriter jointStateWriter = urdfLinkObject.GetComponent<JointStateWriter>();
                    if (!jointStateWriter)
                    {
                        jointStateWriter = urdfLinkObject.AddComponent<JointStateWriter>(); 
                    }
                    jointStateWriter.Write(jointValue);
                }  
                else
                {
                    Debug.LogWarning($"SetRobotConfigfromDict: URDF Link {urdfLinkObject.name} not found in the robotToConfigure.");
                }
            }
        }
        public static void FindAllMeshRenderersInURDFGameObject(Transform currentTransform, Dictionary<string,string> URDFRenderComponents)
        {
            /*
            * FindAllMeshRenderersInURDFGameObject is responsible for finding all MeshRenderers in the URDF GameObject.
            * The method is called recursively to search through all children of the URDF GameObject due to its nested structure.
            */

            Debug.Log($"FindMeshRenderers: Searching for Mesh Renderer in {currentTransform.gameObject.name}.");
            MeshRenderer meshRenderer = currentTransform.GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
            {
                int instanceID = meshRenderer.GetInstanceID();
                if (!URDFRenderComponents.ContainsKey(instanceID.ToString()))
                {
                    meshRenderer.gameObject.name = meshRenderer.gameObject.name + $"_{instanceID.ToString()}";
                    URDFRenderComponents.Add(instanceID.ToString(), meshRenderer.gameObject.name);
                }
            }
            if (currentTransform.childCount > 0)
            {
                foreach (Transform child in currentTransform)
                {
                    FindAllMeshRenderersInURDFGameObject(child, URDFRenderComponents);
                }
            }
        }
        public static void SetRobotLocalPositionandRotationFromFrame(Frame robotBaseFrame, GameObject robotToPosition)
        {
            /*
            * SetRobotPosition is responsible for setting the robot position and rotation from the robot baseframe from a RightHanded Plane.
            */

            Debug.Log($"SetRobotPosition: Setting the robot {robotToPosition.name} to position and rotation from robot baseframe.");
            
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(robotBaseFrame.point);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(robotBaseFrame.xaxis, robotBaseFrame.yaxis);
            Quaternion rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            robotToPosition.transform.localPosition = positionData;
            robotToPosition.transform.localRotation = rotationQuaternion;
        }
        public static void ColorURDFGameObject(GameObject RobotParent, Material material, ref Dictionary<string, string> URDFRenderComponentsStorageDict)
        {
            /*
            * ColorURDFGameObject is responsible for coloring the URDF GameObject with a material.
            * If the URDFRenderComponentsStorageDict is empty, the method will search through the URDF GameObject to find all MeshRenderers.
            */
            if (URDFRenderComponentsStorageDict.Count == 0)
            {
                foreach (Transform child in RobotParent.transform)
                {
                    URDFManagement.FindAllMeshRenderersInURDFGameObject(child, URDFRenderComponentsStorageDict);
                }
            }

            foreach (KeyValuePair<string, string> component in URDFRenderComponentsStorageDict)
            {
                string gameObjectName = component.Value;
                GameObject gameObject = RobotParent.FindObject(gameObjectName);

                if (gameObject)
                {
                    MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer)
                    {
                        meshRenderer.material = material;
                    }
                    else
                    {
                        Debug.Log($"ColorRobot: MeshRenderer not found for {gameObject} when searching through URDF list.");
                    }
                }
            }
        }
        public static void FindLinkNamesFromJointNames(Transform currentTransform, Dictionary<string, float> config, ref Dictionary<string,string> URDFLinkNamesStorageDict)
        {
            /*
            * FindLinkNamesFromJointNames is responsible for finding the URDF Link names from the Joint names in the URDF GameObject.
            * The method is called recursively to search through all children of the URDF GameObject due to its nested structure.
            */
            UrdfJoint urdfJoint = currentTransform.GetComponent<UrdfJoint>();
            if (urdfJoint != null)
            {
                if(config.ContainsKey(urdfJoint.JointName) && !URDFLinkNamesStorageDict.ContainsKey(urdfJoint.JointName))
                {
                    Debug.Log($"FindLinkNames: Found UrdfJointName {urdfJoint.JointName} in URDF on GameObject {currentTransform.gameObject.name}.");
                    URDFLinkNamesStorageDict.Add(urdfJoint.JointName, currentTransform.gameObject.name);
                }
            }
            if (currentTransform.childCount > 0)
            {
                foreach (Transform child in currentTransform)
                {
                    FindLinkNamesFromJointNames(child, config, ref URDFLinkNamesStorageDict);
                }
            }
            else
            {
                Debug.Log($"FindLinkNames: No UrdfJoint found in URDF on GameObject {currentTransform.gameObject.name}");
            }

        }
        public static bool ConfigJointsEqualURDFLinks(Dictionary<string, float> config, Dictionary<string,string> URDFLinkNamesDict)
        {
            
            /*
            * ConfigJointsEqualURDFLinks is responsible for checking if the joint names in the config dictionary match the URDF Link names.
            */
            bool isEqual = true;
            foreach (KeyValuePair<string, float> joint in config)
            {
                string jointName = joint.Key;
                if(URDFLinkNamesDict.ContainsKey(jointName))
                {
                    Debug.Log($"ConfigJointsEqualURDFLinks: Found joint {jointName} in URDFLinkNames.");
                }
                else
                {
                    Debug.Log($"ConfigJointsEqualURDFLinks: Joint {jointName} not found in URDFLinkNames.");
                    isEqual = false;
                }
            }
            return isEqual;
        }
    }
}
