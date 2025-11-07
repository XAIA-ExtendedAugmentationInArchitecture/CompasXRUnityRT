using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using CompasXR.Core;
using CompasXR.Systems;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.AppSettings;
using CompasXR.Robots;
using CompasXR.Robots.MqttData;
using Unity.VisualScripting;
using CompasXR.RoboticTerritories.Data;
using CompasXR.Robots.MqttData.RoboticTerritories;
using Unity.XR.CoreUtils;
using CompasXR.Robots.Data;
using RosSharp.RosBridgeClient;
using System.Collections;
using Vuforia;
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;
using UnityEngine.Analytics;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace CompasXR.UI
{
    /*
    * CompasXR.UI : Is the namespace for all Classes that
    * controll the primary functionalities releated to the User Interface in the CompasXR Application.
    * Functionalities, such as UI interaction, UI element creation, and UI element control.
    */
    public class UIFunctionalities : MonoBehaviour
    {
        /*
        * UIFunctionalities : Class is used to manage the User Interface and User Interface elements.
        * This class is designed to handle the all UI elements and are primariarily divided into 3 sections.
        * 1. Primary UI Elements: These are UI elements used to control the primary functionalities of the application (constantly on the screen).
        * 2. Visualizer Menu Elements: These are UI elements used to control the various visualization functionalities of the application.
        * 3. Menu Elements: These are UI elements used to control additional functionalities of the application. Ex. Info, Reload, etc.
        */

        //Other Scripts for inuse objects
        public DatabaseManager databaseManager;
        public InstantiateObjects instantiateObjects;
        public EventManager eventManager;
        public MqttTrajectoryManager mqttTrajectoryManager;
        public TrajectoryVisualizer trajectoryVisualizer;
        public RosConnectionManager rosConnectionManager;
        public ScrollSearchManager scrollSearchManager;

        //Primary UI Objects
        private GameObject VisibilityMenuObject;
        private GameObject MenuButtonObject;
        private GameObject EditorToggleObject;
        public GameObject CanvasObject;
        public GameObject ConstantUIPanelObjects;
        public GameObject NextGeometryButtonObject;
        public GameObject PreviousGeometryButtonObject;
        public GameObject PreviewGeometrySliderObject;
        public Slider PreviewGeometrySlider;
        public GameObject IsBuiltPanelObjects;
        public GameObject IsBuiltButtonObject;
        public GameObject IsbuiltButtonImage;
    

        //On Screen Messages
        public GameObject MessagesParent;
        public GameObject OnScreenErrorMessagePrefab;
        public GameObject OnScreenInfoMessagePrefab;
        private GameObject PriorityIncompleteWarningMessageObject;
        private GameObject PriorityIncorrectWarningMessageObject;
        private GameObject PriorityCompleteMessageObject;
        public GameObject MQTTFailedToConnectMessageObject;
        public GameObject MQTTConnectionLostMessageObject;
        public GameObject ErrorFetchingDownloadUriMessageObject;
        public GameObject ErrorDownloadingObjectMessageObject;
        public GameObject TrajectoryReviewRequestMessageObject;
        public GameObject TrajectoryCancledMessage;
        public GameObject TrajectoryRequestTimeoutMessage;
        public GameObject SearchItemNotFoundWarningMessageObject;
        public GameObject ActiveRobotIsNullWarningMessageObject;
        public GameObject TransactionLockActiveWarningMessageObject;
        public GameObject ActiveRobotCouldNotBeFoundWarningMessage;
        public GameObject ActiveRobotUpdatedFromPlannerMessageObject;
        public GameObject OnScreenWaitForInferenceMessage;

        //TODO: Robotic Territories Testing ///////////////////////////////////////////////////////////////////////////////////
        public GameObject MimicRemapPointsToRobotReachabilityMessage;
        public GameObject TrajectoryResponseIncorrectWarningMessageObject;
        public GameObject ConfigDoesNotMatchURDFStructureWarningMessageObject;
        public GameObject TrajectoryNullWarningMessageObject;

        public GameObject RealtimeMimicIncorrectBackendOnScreenMessage;
        public GameObject RealtimeMimicPickPlanningFailedOnScreenMessage;
        public GameObject RealtimeMimicPlacePlanningFailedOnScreenMessage;

        //Visualizer Menu Objects
        private GameObject VisualzierBackground;
        private GameObject PreviewActorToggleObject;
        public GameObject IDToggleObject;
        public GameObject RobotToggleObject;
        public GameObject ObjectLengthsToggleObject;
        private GameObject ObjectLengthsUIPanelObjects;
        private Vector3 ObjectLengthsUIPanelPosition;
        private TMP_Text ObjectLengthsText;
        private GameObject ObjectLengthsTags;
        public GameObject ScrollSearchToggleObject;
        private GameObject ScrollSearchObjects;
        public GameObject PriorityViewerToggleObject;
        public GameObject NextPriorityButtonObject;
        public GameObject PreviousPriorityButtonObject;
        public GameObject PriorityViewerBackground;
        public GameObject SelectedPriorityTextObject;
        public TMP_Text SelectedPriorityText;

        //Menu Toggle Button Objects
        private GameObject MenuBackground;
        private GameObject ReloadButtonObject;
        private GameObject InfoToggleObject;
        private GameObject InfoPanelObject;
        public GameObject CommunicationToggleObject;
        private GameObject CommunicationPanelObject;

        //Editor Toggle Objects
        private GameObject EditorBackground;
        private GameObject BuilderEditorButtonObject;
        private GameObject BuildStatusButtonObject;
        
        //Communication Specific Objects
        private TMP_InputField MqttBrokerInputField;
        private TMP_InputField MqttPortInputField;
        private GameObject MqttUpdateConnectionMessage;
        public GameObject MqttConnectionStatusObject;
        public GameObject MqttConnectButtonObject;
        public GameObject RosConnectButtonObject;
        private TMP_InputField RosHostInputField;
        private TMP_InputField RosPortInputField;
        private GameObject RosUpdateConnectionMessage;
        public GameObject RosConnectionStatusObject;

        //Trajectory Review UI Controls
        public GameObject ReviewTrajectoryObjects;
        public GameObject RequestTrajectoryButtonObject;
        public GameObject ApproveTrajectoryButtonObject;
        public GameObject RejectTrajectoryButtonObject;
        public GameObject TrajectoryReviewSliderObject;
        public Slider TrajectoryReviewSlider;
        public GameObject ExecuteTrajectoryButtonObject;
        public GameObject RobotSelectionControlObjects;
        public GameObject RobotSelectionDropdownObject;
        public TMP_Dropdown RobotSelectionDropdown;
        public GameObject SetActiveRobotToggleObject;

        //Object Colors
        private Color Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        private Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color TranspWhite = new Color(1.0f, 1.0f, 1.0f, 0.4f);
        private Color TranspGrey = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.4f);

        //Parent Objects for gameObjects
        public GameObject Elements;
        public GameObject QRMarkers;
        public GameObject UserObjects;

        //AR Camera and Touch GameObjects & Occlusion Objects
        public GameObject arCameraObject;
        public Camera arCamera;
        private GameObject activeGameObject;
        private GameObject temporaryObject; 
        private ARRaycastManager rayManager;
        public CompasXR.Systems.OperatingSystem currentOperatingSystem;
        private AROcclusionManager occlusionManager;
        private GameObject OcclusionToggleObject;

        //On Screen Text
        public GameObject CurrentStepTextObject;
        public GameObject EditorSelectedTextObject;
        public TMP_Text CurrentStepText;
        public TMP_Text LastBuiltIndexText;
        public TMP_Text CurrentPriorityText;
        public TMP_Text EditorSelectedText;

        //In script use variables
        public string CurrentStep = null;
        public string SearchedElementStepID;
        public string SelectedPriority = "None";
        public bool IDTagIsOffset = false;
        public bool PriorityTagIsOffset = false;

        //TODO: Robotic Territories Testing ///////////////////////////////////////////////////////////////////////////////////
        public GameObject RoboticTerritoriesCanvasItems;
        public GameObject ZonesParentGlobal;
        public GameObject ZonesARPrefabObjects;
        public TMP_Text CurrentModeTextObject;

        //TODO: Updating Canvas to the new one..........................................................
        public GameObject RoboticTerritoriesUpdatedCanvas;
        public GameObject ModeSelectionControlsObject;
        public GameObject ModeSelectionDropdownObject;
        public TMP_Dropdown ModeSelectionDropdown;
        public GameObject RoboticTerritoriesInferenceControlsObject;

        //TODO: Inference Controls
        bool GOALINFERRED = false;
        bool INITIALINFERENCEREQUEST = true;
        public GameObject InferenceRequestControlsObject;
        public GameObject InferenceRequestTrajectoryCalculationControlsObject;
        public GameObject RequestInferenceButtonObject;
        public GameObject ReviewInferenceControlsParentObject;

        //TODO: Inference Review Before Guessing the Goal
        public GameObject InferenceRejectGoalandTargetButton;
        public GameObject InferenceAcceptTargetButton;
        public GameObject InferenceAcceptGoalButton;
        public GameObject InferenceReviewSliderParentGameObject;
        public GameObject InferenceReviewSliderObject;
        public Slider InferenceReviewSlider;

        //TODO: After inference success, target selection and planning controls
        public GameObject InferenceSelectTargetParentObject;
        public GameObject InferenceNextTargetButtonObject;
        public GameObject InferencePreviousTargetButtonObject;
        public GameObject InferenceRequestTargetButtonObject;

        public GameObject PostInferenceReviewTrajectoryParentObject;
        public GameObject PostInferenceRobotExecuteButton;
        public GameObject PostInferenceRobotRejectTrajectoryButton;
        public GameObject PostInferenceTrajectoryReviewSliderObject;
        public Slider PostInferenceTrajectoryReviewSlider;

        //TODO: Inference OnScreen Messages
        public GameObject InferenceActiveRobotNullMessage;
        public GameObject PostInferenceActiveRobotNullMessage;
        public GameObject PostInferenceCurrentGoalNull;
        public GameObject InferenceResultReceivedWhileInOtherModeOnScreenMessage;
        public GameObject PostInferenceMessageErrorOnScreenMessage;
        public GameObject PostInferencePlannerRepliedWithNullTrajectory;
        public GameObject InferenceUnableToInferGoalMessage;
        public GameObject InferenceTrajectoryNullWarningMessageObject;

        //TODO: User Initiated Mimic Target On Screen Messages
        public GameObject UserInitiatedMimicCannotBeSetInAnchorOnScreenMsg;
        public GameObject UserInitiatedMimicCannotSetDuplicatePickOnScreenMsg;
        public GameObject UserInitiatedMimicCannotFindTargetOnScreenMsg;
        public GameObject UserInitiatedMimicCannotBeSetInSatisfiedTargetOnScreenMsg;

        //TODO: Updating Canvas to the new one..........................................................


        //ROBOT ITEMS
        public GameObject ReachabilityToggleObject;


        public List<string> ZoneMenuItemsTest = new List<string> {"None", "Inference", "Mimic"};
        public string CurrentZone = "None";
        public int CurrentZoneIndex = 0;
        public int PreviousZoneIndex;
        public GameObject NextZoneButtonObject;
        public GameObject PreviousZoneButtonObject;
        public GameObject RoboticTerritoriesConstantUIObjects;
        public GameObject CorrectionButtonObject;
        public GameObject ToggleZoneVisibilityObject;

        //Mimic Controls
        public GameObject UserInitiatedMimicControlsSetPointsUIObjects;
        public GameObject MimicControlsParent;

        //TODO : Draw Zone Lines Toggle
        public GameObject DrawZonesAsLinesToggleObject;
        public Toggle DrawZonesAsLinesToggle;

        public GameObject MimicSelectGoalsUI;
        public GameObject MimicNextGoalButtonObject;
        public GameObject MimicPreviousGoalsButtonObject;
        public TMP_Text CurrentSelectedGoalTextObject;

        public GameObject ChangeMimicModeControlsObject;
        public GameObject MimicNextModeButtonObject;
        public GameObject MimicPreviousModeButtonObject;

        public GameObject UserInitiatedMimicControls;
        public GameObject MimicSetPointsButtonObject;
        public GameObject UserInitiatedMimicControlsUndoPointButtonObject;
        public GameObject UserInitiatedMimicRequestTrajectoryButtonObject;
        public GameObject UserInitiatedMimicMirrorToggleObject;
        public Toggle UserInitiatedMimicMirrorToggle;

        public GameObject UserInitiatedMimicControlsReviewAndExecuteTrajectoryUIObjects;

        //TODO: Testing Set Target Button.
        public GameObject UserInitiatedSetTargetButtonObject;
        public CompasXRButtonHeldEvent UserInitiatedSetTargetButtonHeldEventComponent;
        public GameObject UserInitiatedMimicExecuteTrajectoryButtonObject;
        public GameObject UserInitiatedMimicTrajectoryReviewSliderObject;
        public Slider UserInitiatedMimicTrajectoryReviewSlider;
        
        //Mimic OnScreen Messages
        public GameObject MimicSetPointOutsideOfHumanZone;
        public GameObject RealtimeMimicActiveRobotNull;
        public GameObject MimicPointsTooFewMessage;

        //TODO: Mimic Pick and Place OnScreen Messages
        public GameObject MimicPickAndPlaceNotInReachabilityMessage;

        // Pick and Place Mimic Helpers...
        public GameObject MimicObjectPickedButNotPlaced;
        public GameObject MimicObjectPlacedButNotPicked;

        public GameObject MimicUnabletoExecuteTrajectory;

        public GameObject UserInitiatedMimicSetPointGreenScreen;
        public GameObject UserInitiatedMimicUndoPointRedScreen;
        public float MimicSetandUndoFlashDuration = 0.1f;

        public GameObject FollowMeButton;
        public GameObject FollowMeButtonCover; //TODO: Set Active False when request is sent, and reactive based on controls...

        public CompasXRButtonHeldEvent FollowMeButtonHeldEventComponent;
        public GameObject RealtimeMimicControlsParent;
        public GameObject RealtimeMimicEditorTestToggleObject;
        public int REALTIMEMIMICINDEXCOUNTER = 0;
        public int REALTIMEMIMICLASTSENTINDEX = -1;
        public int REALTIMEMIMICLASTCOMPLETEDINDEX = -1;

        public GameObject RealtimeMimicIOToggleGameObject;
        public GameObject RealtimeMimicMirrorToggleGameObject;
        public Toggle RealtimeMimicMirrorToggle;

        private bool _syncingMirrorToggles = false;

        //TODO: TESTING
        DevicePoseBehaviour devicePoseBehavior;

        //Mimic Mode and Goal Selection Objects
        public List<string> MimicModesList = new List<string> {"UserInitiated", "RealtimeMimic" };
        public int CurrentMimicModeIndex = 0;

        public int mimicCurrentSelectedGoalIndex = 0;
        public string CurrentSelectedGoalName = "Goal00";

        public bool USERINSTIANTEDTRAJECTORYEXECUTED = false;

        //TODO: Robotic Territories Testing ///////////////////////////////////////////////////////////////////////////////////

        //TODO : REALTIME MIMCIC PICK AND ONSCREEN MESSAGES
        public GameObject RealtimeMimicCannotFindTargetOnScreenMsg;

        // public bool REALTIMEMIMICIGNOREISHELDEVENT = false;
        public GameObject RealtimeMimicAcceptPickMessage;
        public GameObject RealtimeMimicAcceptPlaceMessage;
        public GameObject RealTimeMimicPickRequestedWithNoMessage;
        public GameObject RealTimeMimicPlaceRequestedWithNoMessage;

        /////////////////////////////////// Monobehaviour Methods ///////////////////////////////////////////////////////////        
        void Start()
        {
            /*
            * Start : Method is used to initialize the UI elements and set up the UI Object & Script Dependencies on start.
            */
            // OnAwakeInitilization();
            OnAwakeInitilizationRoboticTerritories();
            devicePoseBehavior = VuforiaBehaviour.Instance.DevicePoseBehaviour;
        }
        void Update()
        {
            /*
            * Update : Method is used to update the UI elements and check for touch option activation.
            */
            // TouchSearchControler();
            RealtimeMimicFollowMeEventWatcher();
        }

        /////////////////////////////////// UI Control & OnStart methods ////////////////////////////////////////////////////

        //TODO: Robotic Territories Testing ///////////////////////////////////////////////////////////////////////////////////
        public void RealtimeMimicFollowMeEventWatcher()
        {
            if (FollowMeButtonHeldEventComponent.isHeld)
            {
                // if (REALTIMEMIMICIGNOREISHELDEVENT)
                if (FollowMeButtonHeldEventComponent.ignore)
                {
                    Debug.Log("RealtimeMimicFollowMeEventWatcher: Ignoring isHeld event.");
                    return;
                }
                SetRealtimeMimicPointBasicTEMPORARY();
            }
            else
            {
                REALTIMEMIMICINDEXCOUNTER = 0;
            }
        }
        public IEnumerator PauseForDurationSeconds(float duration)
        {
            yield return new WaitForSeconds(duration);
            Debug.Log($"Paused for {duration} seconds.");
        }
        private void OnAwakeInitilizationRoboticTerritories()
        {
            /*
            * OnAwakeInitilization : Method is used to initialize the UI elements and set up the 
            * UI Objects, Script Dependencies, & set up relationships on start.
            */

            //Find Other Scripts
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();
            rosConnectionManager = GameObject.Find("RosManager").GetComponent<RosConnectionManager>();
            scrollSearchManager = GameObject.Find("ScrollSearchManager").GetComponent<ScrollSearchManager>();

            //Find Global use GameObjects
            ZonesParentGlobal = GameObject.Find("ZonesParent");
            QRMarkers = GameObject.Find("QRMarkers");
            CanvasObject = GameObject.Find("Canvas");

            //Find AR and system management items
            arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();
            rayManager = FindObjectOfType<ARRaycastManager>();
            currentOperatingSystem = OperatingSystemManager.GetCurrentOS();

            //Find OnScreeen Message Prefabs
            RoboticTerritoriesCanvasItems = CanvasObject.FindObject("RoboticTerritories");
            MessagesParent = RoboticTerritoriesCanvasItems.FindObject("OnScreenMessages");
            OnScreenErrorMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenErrorMessagePrefab");
            OnScreenInfoMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenInfoMessagePrefab");
            ActiveRobotUpdatedFromPlannerMessageObject = MessagesParent.FindObject("Prefabs").FindObject("ActiveRobotUpdatedFromPlannerMessage");
            OnScreenWaitForInferenceMessage = MessagesParent.FindObject("Prefabs").FindObject("OnScreenWaitForInference");
            if (OnScreenWaitForInferenceMessage == null)
            {
                Debug.LogWarning("UIFunctionalities: OnScreenWaitForInferenceMessage is null. Cannot show wait for inference message.");
            }

            //TODO: Working updates
            RoboticTerritoriesUpdatedCanvas = CanvasObject.FindObject("RoboticTerritoriesUpdated");
            SetModeSelectionItemsOnStart();

            RoboticTerritoriesInferenceControlsObject = RoboticTerritoriesUpdatedCanvas.FindObject("InferenceControls");
            SetInferenceControlsOnStart();

            //TODO: Mimic remap testing : I think this can go away, but keep for now. ////////////////////////////////////////////////////////////////////////////////////////
            MimicRemapPointsToRobotReachabilityMessage = MessagesParent.FindObject("Prefabs").FindObject("RemapMimicPointsMessage");
            Button RemapButton = MimicRemapPointsToRobotReachabilityMessage.FindObject("YesButton").GetComponent<Button>();
            Button NoButton = MimicRemapPointsToRobotReachabilityMessage.FindObject("NoButton").GetComponent<Button>();
            RemapButton.GetComponent<Button>().onClick.AddListener(RemapMimicPointsToRobotReachabilityButtonMethod);
            NoButton.GetComponent<Button>().onClick.AddListener(DestroySystemProposedMimicPointsButtonMethod);
            //TODO: Mimic remap testing : I think this can go away, but keep for now. ////////////////////////////////////////////////////////////////////////////////////////

            DrawZonesAsLinesToggleObject = RoboticTerritoriesUpdatedCanvas.FindObject("DrawZoneLInes");
            DrawZonesAsLinesToggle = DrawZonesAsLinesToggleObject.GetComponentInChildren<Toggle>();
            DrawZonesAsLinesToggle.onValueChanged.AddListener(DrawLinesToggleMethod);

            //Set robotic items on start
            SetRoboticMenuItemsOnStart();

            //Set Mimic Mode Selection Items
            MimicControlsParent = RoboticTerritoriesUpdatedCanvas.FindObject("MimicControls");
            SetMimicModeSelectionItemsOnStart();

            //Set User Initiated Mimic Controls
            SetMimicUserInitiatedMimicControlsOnStart();

            //Set Realtime Mimic Controls
            SetRealtimeMimicControlsOnStart();

            //Set Realtime Mimic Accept pick or place message contorols
            SetRealtimeMimicPickAndPlaceOnscreenMessageOnStart();

            //Set Mimic Goal Selection UI
            SetMimicGoalSelectionUIOnStart();
        }
        public void SetMimicModeSelectionItemsOnStart()
        {
            /*
            * Method is used to set up the Mimic Mode Selection UI elements on start.
            * Mimic Mode Selection UI elements constitute the UI elements that are used to control the mode selection functionalities
            * of the application.
            */

            //Find The Dropdown for Mode Selection
            ChangeMimicModeControlsObject = MimicControlsParent.FindObject("ChangeMimicModeControls");

            //Find SetPointsButton Objects
            UserInterface.FindButtonandSetOnClickAction(
            ChangeMimicModeControlsObject,
            ref MimicNextModeButtonObject,
            "NextModeButton", NextMimicModeButtonMethod);

            //Find UndoPointsButton ObjectsU
            UserInterface.FindButtonandSetOnClickAction(
            ChangeMimicModeControlsObject,
            ref MimicPreviousModeButtonObject,
            "PreviousModeButton", PreviousMimicModeButtonMethod);
        }
        public void DrawLinesToggleMethod(bool toggle)
        {
            /*
            * Method is used to toggle the drawing of zone lines.
            */
            Debug.Log($"DrawLinesToggleMethod: Toggling Draw Zone Lines to {toggle}");
            instantiateObjects.SetZoneOnlyCurrentZoneVisible(databaseManager.ProjectZones.CurrentZone);
        }
        //TODO: Mimic UI Mode and Goal Selecton Controls
        public void NextMimicModeButtonMethod()
        {
            /*
            * Method is used to set the next mimic mode based on the current mode.
            */
            if (CurrentMimicModeIndex < MimicModesList.Count - 1)
            {
                string previousMimicMode = MimicModesList[CurrentMimicModeIndex];
                CurrentMimicModeIndex += 1;
                string newMimicMode = MimicModesList[CurrentMimicModeIndex];

                if (newMimicMode == "RealtimeMimic")
                {
                    databaseManager.ProjectZones.CurrentMimicMode = ProjectZones.MimicZoneMode.RealtimeMimic;
                    SetMimicControlsBasedOnCurrentMimicMode(databaseManager.ProjectZones.CurrentMimicMode);
                    instantiateObjects.DestroyUserInstatiatedMimicZoneObjects();

                    //TODO: This was not 100% Correct. It kills the active Robot when it shouldn't.
                    if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        // trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                        trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                    }
                    Debug.Log($"NextMimicModeButtonMethod: Changed Mimic Mode from {previousMimicMode} to {newMimicMode}");
                }
                else
                {
                    Debug.Log($"NextMimicModeButtonMethod: NOT SURE HOW... Changed Mimic Mode from {previousMimicMode} to {newMimicMode}");
                }
            }
            else
            {
                Debug.Log("NextMimicModeButtonMethod: Reached end of Mimic Modes List Not Changing");
            }
        }
        public void PreviousMimicModeButtonMethod()
        {
            /*
            * Method is used to set the previous mimic mode based on the current mode.
            */

            if (CurrentMimicModeIndex > 0)
            {
                string previousMimicMode = MimicModesList[CurrentMimicModeIndex];
                CurrentMimicModeIndex -= 1;
                string newMimicMode = MimicModesList[CurrentMimicModeIndex];

                if (newMimicMode == "UserInitiated")
                {
                    databaseManager.ProjectZones.CurrentMimicMode = ProjectZones.MimicZoneMode.UserInitiated;
                    SetMimicControlsBasedOnCurrentMimicMode(databaseManager.ProjectZones.CurrentMimicMode);
                    instantiateObjects.DestroyRealtimeMimicZoneObjects();
                    Debug.Log($"PreviousMimicModeButtonMethod: Changed Mimic Mode from {previousMimicMode} to {newMimicMode}");
                }
                else
                {
                    Debug.Log($"PreviousMimicModeButtonMethod: NOT SURE HOW... Changed Mimic Mode from {previousMimicMode} to {newMimicMode}");
                }
            }
            else
            {
                Debug.Log("PreviousMimicModeButtonMethod: Reached start of Mimic Modes List Not Changing");
            }

            Debug.Log("PreviousMimicModeButtonMethod: Previous Mimic Mode Button Pressed");
        }
        public void MimicSelectNextGoalButtonMethod() //TODO: Working on this now...
        {
            //Do nothing for now
            if (instantiateObjects.MimicGoalsManager.Goals.Count == 0)
            {
                Debug.LogWarning("MimicSelectNextGoalButtonMethod: No Goals Found in the Mimic Goals Manager");
                return;
            }
            else
            {
                if (mimicCurrentSelectedGoalIndex < instantiateObjects.MimicGoalsManager.Goals.Count - 1)
                {
                    GameObject previousSelectedGoal = instantiateObjects.MimicGoalsManager.Goals[mimicCurrentSelectedGoalIndex].GoalGameObject;
                    previousSelectedGoal.SetActive(false);

                    mimicCurrentSelectedGoalIndex += 1;

                    //TODO:Testing the addition of removing everything associated with the previous set pick and place
                    if (databaseManager.ProjectZones.CurrentMimicMode == ProjectZones.MimicZoneMode.UserInitiated)
                    {
                        if (instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPicked || instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPlaced)
                        {
                            instantiateObjects.DestroyUserInstatiatedMimicZoneObjects(true);
                            instantiateObjects.userIniatedMimicPickandPlaceManager.Reset();
                        }
                    }

                    SetMimicGoalFromIndex(mimicCurrentSelectedGoalIndex);

                    Debug.Log($"MimicSelectNextGoalButtonMethod: Changed Selected Goal to {CurrentSelectedGoalName}");
                }
                else
                {
                    Debug.Log("MimicSelectNextGoalButtonMethod: Reached end of Goals List Not Changing");
                }
            }
        }
        public void SetMimicGoalFromIndex(int selectedGoalIndex)
        {
            CurrentSelectedGoalName = instantiateObjects.MimicGoalsManager.Goals[selectedGoalIndex].Name;
            CurrentSelectedGoalTextObject.text = CurrentSelectedGoalName;
            GoalObject newSelectedGoal = instantiateObjects.MimicGoalsManager.Goals[selectedGoalIndex];
            GameObject newSelectedGoalGameObject = newSelectedGoal.GoalGameObject;
            instantiateObjects.MimicGoalsManager.UpdateCurrentGoal(newSelectedGoal);
            GoalStateObserver goalStateObserver = instantiateObjects.MimicGoalsManager.GoalStatusObserver;
            goalStateObserver.CheckAllGoalsStatesFromObservedGeometriesDict(databaseManager.observedGeometriesDict, instantiateObjects.GoalSatisfiedMaterial, instantiateObjects.GoalUnsatisfiedMaterial, instantiateObjects.OBJECT_TRACKING_POSITION_SATISFACTION_TOLERANCE, instantiateObjects.OBJECT_TRACKING_ROTATION_SATISFACTION_TOLERANCE);
            goalStateObserver.DebugLogAllComponentStatesAsDictionary();
            newSelectedGoalGameObject.SetActive(true);

            //TODO: THERE IS A BUG HERE. The MIrrored Geometry does not render properly. When switching goals.
            if (instantiateObjects.MimicMirroredGeometryManagerImplementation != null)
            {
                Debug.Log($"SetMimicGoalFromIndex: Updating Mimic Mirrored Geometry Manager with new selected goal {newSelectedGoal.Name}");
                instantiateObjects.MimicMirroredGeometryManagerImplementation.UpdateCurrentGoal(newSelectedGoal.Name);
                instantiateObjects.MimicMirroredGeometryManagerImplementation.UpdateAllComponentStates(instantiateObjects.MimicGoalsManager.GoalStatusObserver.ComponentStates);
                instantiateObjects.MimicMirroredGeometryManagerImplementation.DebugLogAllComponentStatesAsDictionary();
                goalStateObserver.DebugLogAllComponentStatesAsDictionary();
            }
        }
        public void MimicSelectPreviousGoalButtonMethod() //TODO: Working on this now...
        {
            //Do nothing for now
            Debug.Log("MimicSelectPreviousGoalButtonMethod: Previous Goal Button Pressed - Do Nothing for now");

            if (instantiateObjects.MimicGoalsManager.Goals.Count == 0)
            {
                Debug.LogWarning("MimicSelectPreviousGoalButtonMethod: No Goals Found in the Mimic Goals Manager");
                return;
            }
            else
            {
                if (mimicCurrentSelectedGoalIndex > 0)
                {
                    GameObject previousSelectedGoal = instantiateObjects.MimicGoalsManager.Goals[mimicCurrentSelectedGoalIndex].GoalGameObject;
                    previousSelectedGoal.SetActive(false);

                    //TODO:Testing the addition of removing everything associated with the previous set pick and place
                    if (databaseManager.ProjectZones.CurrentMimicMode == ProjectZones.MimicZoneMode.UserInitiated)
                    {
                        if (instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPicked || instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPlaced)
                        {
                            instantiateObjects.DestroyUserInstatiatedMimicZoneObjects(true);
                            instantiateObjects.userIniatedMimicPickandPlaceManager.Reset();
                        }
                    }

                    mimicCurrentSelectedGoalIndex -= 1;
                    SetMimicGoalFromIndex(mimicCurrentSelectedGoalIndex);
                    Debug.Log($"MimicSelectPreviousGoalButtonMethod: Changed Selected Goal to {CurrentSelectedGoalName}");
                }
                else
                {
                    Debug.Log("MimicSelectPreviousGoalButtonMethod: Reached start of Goals List Not Changing");
                }
            }
        }

        //TODO: Mimic UI Mode and Goal Selecton Controls
        public void SetMimicGoalSelectionUIOnStart()
        {
            MimicSelectGoalsUI = MimicControlsParent.FindObject("SelectGoalUI");

            //Find SetPointsButton Objects
            UserInterface.FindButtonandSetOnClickAction(
            MimicSelectGoalsUI,
            ref MimicNextGoalButtonObject,
            "NextGoalButton", MimicSelectNextGoalButtonMethod);

            //Find UndoPointsButton ObjectsU
            UserInterface.FindButtonandSetOnClickAction(
            MimicSelectGoalsUI,
            ref MimicPreviousGoalsButtonObject,
            "PreviousGoal", MimicSelectPreviousGoalButtonMethod);

            CurrentSelectedGoalTextObject = MimicSelectGoalsUI.FindObject("GoalNameText").GetComponentInChildren<TMP_Text>();
            SetMimicGoalFromIndex(mimicCurrentSelectedGoalIndex);
        }
        public void PrintobservedGeometryDictInformation()
        {
            /*
            * Method is used to print the observed geometry dictionary information.
            */
            Debug.Log("JOETESTING : Observed Geometry Dictionary Information:");
            foreach (KeyValuePair<string, ObservedGeometry> item in databaseManager.observedGeometriesDict)
            {
                // Debug.Log($"Key: {item.Key}, Value: {JsonConvert.SerializeObject(item.Value)}");
                if (item.Value.GeometryObject != null)
                {
                    Debug.Log($"PrintobservedGeometryDictInformation JOEEE: {item.Key}, Value: {item.Value.GeometryObject.name}");
                    item.Value.GeometryObject.GetComponentInChildren<Renderer>().material.color = Color.green;
                }
                else
                {
                    Debug.LogWarning($"PrintobservedGeometryDictInformation JOEEE: {item.Key}, Value: null");
                }
            }
        }
        public void ToggleIOForRealtimeMimicMethod(bool toggle)
        {
            /*
            * Method is used to toggle the IO for the Realtime Mimic.
            */
            Debug.Log($"ToggleIOForRealtimeMimicMethod: Toggling IO for Realtime Mimic to {toggle}");
            if (RealtimeMimicIOToggleGameObject != null)
            {
                int signal = 1;
                bool gripperToggle;
                if (toggle)
                {
                    gripperToggle = true;
                }
                else
                {
                    gripperToggle = false;
                }
                RealtimeMimicIOToggleRequestMessage realtimeMimmicIOToggleMessage = new RealtimeMimicIOToggleRequestMessage
                (
                    signal,
                    gripperToggle
                );
                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicIOToggleRequestTopic, realtimeMimmicIOToggleMessage.GetData());
                Debug.Log($"ToggleIOForRealtimeMimicMethod: Sending Realtime Mimic IO Toggle Request with signal: {signal}, gripperToggle: {gripperToggle}");

            }
        }
        public void SetInferenceControlsOnStart()
        {
            /*
            * Method is used to set up the Inference Controls UI elements on start.
            * Inference Controls UI elements constitute the UI elements that are used to control the inference functionalities
            * of the application.
            */
            //Find Objects for Inference Controls
            InferenceRequestControlsObject = RoboticTerritoriesInferenceControlsObject.FindObject("InferenceRequestControls");
            UserInterface.FindButtonandSetOnClickAction(InferenceRequestControlsObject, ref RequestInferenceButtonObject, "RequestInferenceButton", RequestInferenceButtonMethod);

            ReviewInferenceControlsParentObject = InferenceRequestControlsObject.FindObject("InferenceReviewControls");
            UserInterface.FindButtonandSetOnClickActionDebug(ReviewInferenceControlsParentObject, ref InferenceRejectGoalandTargetButton, "RejectGoalButton", InferenceReviewRejectGoalAndTargetButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(ReviewInferenceControlsParentObject, ref InferenceAcceptTargetButton, "AcceptTargetButton", InferenceAcceptTargetRejectGoalButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(ReviewInferenceControlsParentObject, ref InferenceAcceptGoalButton, "AcceptGoalButton", InferenceAcceptGoalButtonMethod);

            InferenceReviewSliderParentGameObject = ReviewInferenceControlsParentObject.FindObject("InferenceReviewTrajectorySlider");
            UserInterface.FindSliderandSetOnValueChangeAction(InferenceReviewSliderParentGameObject, ref InferenceReviewSliderObject, ref InferenceReviewSlider, "TrajectoryReviewSlider", (value) => InferenceReviewSliderReviewCompoundTrajectories(value));

            //TODO: This is to select a goal and plan after success in guessing the correct goal.
            InferenceRequestTrajectoryCalculationControlsObject = RoboticTerritoriesInferenceControlsObject.FindObject("RobotTrajectoryCalculationControls");
            InferenceSelectTargetParentObject = InferenceRequestTrajectoryCalculationControlsObject.FindObject("SelectTargetControls");
            UserInterface.FindButtonandSetOnClickAction(InferenceSelectTargetParentObject, ref InferenceNextTargetButtonObject, "NextTargetButton", PostInferenceNextTargetButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(InferenceSelectTargetParentObject, ref InferencePreviousTargetButtonObject, "PreviousTargetButton", PostInferencePreviousTargetButtonMethod);
            UserInterface.FindButtonandSetOnClickActionDebug(InferenceSelectTargetParentObject, ref InferenceRequestTargetButtonObject, "RequestTargetButton", PostInferenceRequestTargetButtonMethod);

            //TODO: This is for trajectory review after inference success.
            PostInferenceReviewTrajectoryParentObject = InferenceRequestTrajectoryCalculationControlsObject.FindObject("ReviewAndExecuteTrajectoryUI");
            UserInterface.FindButtonandSetOnClickAction(PostInferenceReviewTrajectoryParentObject, ref PostInferenceRobotExecuteButton, "ExecuteTrajectory", PostInferenceExecuteTrajectoryButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(PostInferenceReviewTrajectoryParentObject, ref PostInferenceRobotRejectTrajectoryButton, "RejectTrajectory", PostInferenceRejectTrajectoryButtonMethod);
            UserInterface.FindSliderandSetOnValueChangeAction(InferenceRequestTrajectoryCalculationControlsObject, ref PostInferenceTrajectoryReviewSliderObject, ref PostInferenceTrajectoryReviewSlider, "TrajectoryReviewSlider", (value) => PostInferenceTrajectoryReviewSliderMethod(value));

            //TODO: This is incorrect because it not set to this mode needs to be called in the set mode.
            SetInferanceUIBasedOnInferenceState(GOALINFERRED);
        }
        public void SetInferanceUIBasedOnInferenceState(bool goalInfered)
        {
            /*
            * Method is used to set the inference goals based on the inference state.
            * If the goal is inferred, the inference controls are set to visible and interactable.
            * If the goal is not inferred, the inference controls are set to not visible and not interactable.
            */
            Debug.Log($"SetInferanceGoalsBasedOnInferenceState: Setting Inference UI based on Inference State. Goal Inferred: {goalInfered}");
            if (goalInfered)
            {

                SetInferenceUIPostInferenceSuccesState(true, true, true, false, false);
                SetInferenceRequestUIControlsVisibilityandInteractibility(false, false, false, false, false);
                Debug.Log("SetInferanceGoalsBasedOnInferenceState: Goal Inferred, setting inference controls to visible and interactable.");
            }
            else
            {
                SetInferenceUIPostInferenceSuccesState(false, false, false, false, false);
                SetInferenceRequestUIControlsVisibilityandInteractibility(true, true, false, false, false);
                Debug.Log("SetInferanceGoalsBasedOnInferenceState: Goal Not Inferred, setting inference controls to not visible and not interactable.");
            }

        }
        public void SetInferenceUIPostInferenceSuccesState(bool selectTargetControlsVisibility, bool selectTargetControlsInteractibility, bool requestTargetInteractable, bool trajectoryReviewControlsVisibility, bool trajectoryReviewControlsInteractibility)
        {
            /*
            * Method is used to set the inference UI controls post inference success state.
            */
            if (InferenceRequestTrajectoryCalculationControlsObject != null)
            {
                bool anyVisible = selectTargetControlsVisibility || trajectoryReviewControlsVisibility;
                InferenceRequestTrajectoryCalculationControlsObject.SetActive(anyVisible);
            }
            else
            {
                Debug.LogWarning("SetInferenceUIPostInferenceSuccesState: InferenceRequestTrajectoryCalculationControlsObject is null.");
            }

            if (InferenceSelectTargetParentObject != null)
            {
                InferenceSelectTargetParentObject.SetActive(selectTargetControlsVisibility);
                InferenceNextTargetButtonObject.GetComponentInChildren<Button>().interactable = selectTargetControlsInteractibility;
                InferencePreviousTargetButtonObject.GetComponentInChildren<Button>().interactable = selectTargetControlsInteractibility;
                InferenceRequestTargetButtonObject.GetComponentInChildren<Button>().interactable = requestTargetInteractable;
            }
            if (PostInferenceReviewTrajectoryParentObject != null)
            {
                PostInferenceReviewTrajectoryParentObject.SetActive(trajectoryReviewControlsVisibility);
                PostInferenceRobotExecuteButton.GetComponentInChildren<Button>().interactable = trajectoryReviewControlsInteractibility;
                PostInferenceRobotRejectTrajectoryButton.GetComponentInChildren<Button>().interactable = trajectoryReviewControlsInteractibility;
                PostInferenceTrajectoryReviewSlider.GetComponentInChildren<Slider>().interactable = trajectoryReviewControlsInteractibility;
            }
        }
        public void SetInferenceRequestUIControlsVisibilityandInteractibility(bool requestInferenceControlsVisibility, bool requestInferenceControlsInteractibility, bool reviewInferenceControlsVisibility, bool reviewInferenceControlsInteractibility, bool reviewInferenceTrajectoryControlsExecutionInteractability)
        {
            /*
            * Method is used to set the visibility and interactibility of the Inference Controls UI elements.
            */
            if (InferenceRequestControlsObject != null)
            {
                // Parent is visible if ANY visibility flag is true
                bool anyVisible = requestInferenceControlsVisibility || reviewInferenceControlsVisibility;
                InferenceRequestControlsObject.SetActive(anyVisible);
            }

            if (RequestInferenceButtonObject != null)
            {
                RequestInferenceButtonObject.SetActive(requestInferenceControlsVisibility);

                // If the button should be visible, set interactability accordingly
                var button = RequestInferenceButtonObject.GetComponentInChildren<Button>();
                if (button != null)
                    button.interactable = requestInferenceControlsInteractibility;
            }

            if (ReviewInferenceControlsParentObject != null)
            {
                ReviewInferenceControlsParentObject.SetActive(reviewInferenceControlsVisibility);

                InferenceRejectGoalandTargetButton.GetComponentInChildren<Button>().interactable = reviewInferenceControlsInteractibility;

                // Handles both execute/not-execute cases
                InferenceAcceptGoalButton.GetComponentInChildren<Button>().interactable   = reviewInferenceControlsInteractibility;
                InferenceAcceptTargetButton.GetComponentInChildren<Button>().interactable = reviewInferenceControlsInteractibility;

                InferenceReviewSliderParentGameObject.GetComponentInChildren<Slider>().interactable =
                    reviewInferenceTrajectoryControlsExecutionInteractability;
            }

        }
        public void TEMPORARYToggleRealtimeMimicIsPressedTestingMethodTEMPORARY(bool toggle)
        {
            /*
            * Method is used to test the Realtime Mimic Is Pressed Toggle.
            */
            if (toggle)
            {
                Debug.Log("ToggleRealtimeMimicIsPressedTestingMethod: Realtime Mimic Is Pressed Toggle is On.");
                FollowMeButtonHeldEventComponent.isHeld = true;
            }
            else
            {
                Debug.Log("ToggleRealtimeMimicIsPressedTestingMethod: Realtime Mimic Is Pressed Toggle is Off.");
                FollowMeButtonHeldEventComponent.isHeld = false;
            }
        }
        public void SetRoboticMenuItemsOnStart()
        {
            /*
            * Method is used to set up the Robotic Menu UI elements on start.
            * Robotic Menu UI elements constitute the UI elements that are used to control the robotic functionalities
            * of the application.
            */

            //Find Objects for active robot selection
            RobotSelectionControlObjects = RoboticTerritoriesUpdatedCanvas.FindObject("RobotSelectionControls");
            RobotSelectionDropdownObject = RobotSelectionControlObjects.FindObject("RobotSelectionDropdown");
            RobotSelectionDropdown = RobotSelectionDropdownObject.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> robotOptions = UserInterface.SetDropDownOptionsFromStringList(RobotSelectionDropdown ,trajectoryVisualizer.RobotPreFabList);
            RobotSelectionDropdown.onValueChanged.AddListener(RobotSelectionDropdownValueChanged);

            GameObject ReachabilityToggleParent = RobotSelectionControlObjects.FindObject("Reachability");
            UserInterface.FindToggleandSetOnValueChangedAction(RobotSelectionControlObjects, ref ReachabilityToggleObject, "ReachabilityToggle", ReachabilityToggleMethod);

            //Find Active Robot Toggle Objects
            if(RobotSelectionControlObjects == null)
            {
                Debug.Log("Robot Selection Control Objects is null.");

            }
            else if (RobotSelectionDropdownObject == null)
            {
                Debug.Log("Robot Selection Dropdown Object is null.");
            }
            else
            {
                RobotSelectionDropdown.options = robotOptions;
            }
            UserInterface.FindToggleandSetOnValueChangedAction(RobotSelectionControlObjects, ref SetActiveRobotToggleObject, "SetActiveRobotToggle", RoboticTerritoriesSetActiveRobotToggleMethod);
        }
        public void SignalMimicPointsRemaptoRobotReachabilityMessage() //TODO: Static and not interchangeable
        {
            /*
            * Method is used to signal the user that the mimic points are being remapped to the robot reachability.
            */
            Debug.Log("SignalMimicPointsRemaptoRobotReachabilityMessage: Signaling user to remap mimic points to robot reachability.");
            MimicRemapPointsToRobotReachabilityMessage.SetActive(true);
            SetUserInitiatedMimicControlsActivity(true, false, false, false, false);
        }
        public void RemapMimicPointsToRobotReachabilityButtonMethod()
        {
            /*
            * Method is used to remap the mimic points to the robot reachability.
            */
            Debug.Log("RemapMimicPointsToRobotReachabilityMethod: Remapping Mimic Points to Robot Reachability.");
            
            //TODO: CHECK THIS... IT IS CRAZY...
            instantiateObjects.MakeMimicPointsFromSystemProposedPoints(ref instantiateObjects.MimicHumanPoints, ref instantiateObjects.MimicRobotPoints, ref instantiateObjects.MimicHumanSystemProposedPoints, ref instantiateObjects.MimicRobotSystemProposedPoints, instantiateObjects.MimicHumanPointsParent, instantiateObjects.MimicRobotPointsParent, 
            instantiateObjects.MimicHumanSystemProposedPointsParent, instantiateObjects.MimicRobotSystemProposedPointsParent, instantiateObjects.MimicHumanLine, instantiateObjects.MimicRobotLine, instantiateObjects.MimicSystemProposedLineHuman, instantiateObjects.MimicSystemProposedLineRobot);

            SetUserInitiatedMimicControlsActivity(true, true, true, false, false);
            MimicRemapPointsToRobotReachabilityMessage.SetActive(false);
        }
        public void DestroySystemProposedMimicPointsButtonMethod()
        {
            /*
            * Method is used to destroy the system proposed mimic points.
            */
            Debug.Log("DestroySystemProposedMimicPointsMethod: Destroying System Proposed Mimic Points.");
            instantiateObjects.DestroySystemProposedMimicPoints(ref instantiateObjects.MimicHumanSystemProposedPoints, ref instantiateObjects.MimicRobotSystemProposedPoints, 
            instantiateObjects.MimicHumanSystemProposedPointsParent, instantiateObjects.MimicRobotSystemProposedPointsParent, instantiateObjects.MimicSystemProposedLineHuman, 
            instantiateObjects.MimicSystemProposedLineRobot);

            SetUserInitiatedMimicControlsActivity(true, true, true, false, false);
            MimicRemapPointsToRobotReachabilityMessage.SetActive(false);
        }
        public void RoboticTerritoriesSetActiveRobotToggleMethod(Toggle toggle)
        {
            /*
            * Method is used to set the active robot based on the toggle value.
            * Additionally it controls UI elements based on the toggle value.
            */
            if(toggle!=null && toggle.isOn)
            {
                Debug.Log($"SettingActiveRobotButtonMethod: Setting Active Robot based on input {RobotSelectionDropdown.options[RobotSelectionDropdown.value].text}");
                string robotName = RobotSelectionDropdown.options[RobotSelectionDropdown.value].text;
                // bool visibility = false;
                // if(CurrentStep != null && RobotToggleObject.GetComponent<Toggle>().isOn)
                // {
                //     if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.actor == "ROBOT")
                //     {
                //         visibility = true;
                //     }
                // }
                trajectoryVisualizer.SetActiveRobotFromDropdown(robotName, true, toggle.isOn); //TODO: Changed this bool.
                SetActiveRobotToggleObject.FindObject("Image").SetActive(true);
            }
            else
            {
                Debug.Log("SettingActiveRobotButtonMethod: Destroying Current Active Robot");
                if(trajectoryVisualizer.ActiveRobotObjects.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveRobotObjects();
                }
                if(trajectoryVisualizer.humanZoneMimicReachibility != null)
                {
                    Destroy(trajectoryVisualizer.humanZoneMimicReachibility);
                }
                mqttTrajectoryManager.serviceManager.ActiveRobotName = null;  //TODO: THIS IS FROM COMPAS XR, BUT NEEDS TO BE THOUGHT ABOUT FOR ROBOT TERRITORIES
                SetActiveRobotToggleObject.FindObject("Image").SetActive(false);           
            }
        }
        public void SetModeSelectionItemsOnStart()
        {
            /*
            * Method is used to set up the Mode Selection UI elements on start.
            * Mode Selection UI elements constitute the UI elements that are used to control the mode selection functionalities
            * of the application.
            */

            //Find The Dropdown for Mode Selection
            ModeSelectionControlsObject = RoboticTerritoriesUpdatedCanvas.FindObject("ModeSelectionControls");
            if(ModeSelectionControlsObject == null)
            {
                Debug.LogWarning("SetModeSelectionItemsOnStart: ModeSelectionControlsObject is null.");
            }
            if(RoboticTerritoriesUpdatedCanvas == null)
            {
                Debug.LogWarning("SetModeSelectionItemsOnStart: RoboticTerritoriesUpdatedCanvas is null.");
            }
            ModeSelectionDropdownObject = ModeSelectionControlsObject.FindObject("SetModeDropdown");
            if(ModeSelectionDropdownObject == null)
            {
                Debug.LogWarning("SetModeSelectionItemsOnStart: ModeSelectionDropdownObject is null.");
            }
            ModeSelectionDropdown = ModeSelectionDropdownObject.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> modeOptions = UserInterface.SetDropDownOptionsFromStringList(ModeSelectionDropdown , ZoneMenuItemsTest);
            ModeSelectionDropdown.onValueChanged.AddListener(SetCurrentZoneFromDropdown);

            // RobotSelectionDropdown.onValueChanged.AddListener(RobotSelectionDropdownValueChanged);

        }
        public void SetCurrentZoneFromDropdown(int dropDownValue) //TODO: Some Hacky things in here for resetting inference state.
        {
            /*
            * Method is used to set the current zone based on the dropdown value.
            */
            if(dropDownValue >= 0 && dropDownValue < ZoneMenuItemsTest.Count)
            {

                PreviousZoneIndex = CurrentZoneIndex;
                CurrentZoneIndex = dropDownValue;

                //TODO: This is a hacky way to reset the goal inferred state when switching away from the inference zone.
                if (PreviousZoneIndex == 1 && CurrentZoneIndex != 1)
                {
                    GOALINFERRED = false;
                    INITIALINFERENCEREQUEST = true;
                    ResetInferenceGameObjectsInScene();
                    instantiateObjects.PostInferenceResetSelectedGoalComponent();
                    instantiateObjects.InferenceGoalsManager.GoalStatusObserver.Active = false;
                }
                if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }

                CurrentZone = ZoneMenuItemsTest[CurrentZoneIndex];
                databaseManager.ProjectZones.CurrentZone = (ProjectZones.CurrentZoneMode)CurrentZoneIndex; //TODO: THIS NEEDS TO REMAIN THE SAME AS THE OTHER ONE
                LogService.Log($"SetCurrentZoneFromDropdown: Setting Current Zone to {CurrentZone} at index {CurrentZoneIndex}");

                //Control Zone Coloring, UI Objects, and AR Objects
                ColorZonesBasedOnCurrentMode(databaseManager.ProjectZones.CurrentZone);
                // SetUIObjectsFromCurrentMode(databaseManager.ProjectZones.CurrentZone);
                ControlARZoneObjectsBasedOnCurrentMode(databaseManager.ProjectZones.CurrentZone);
                SetUIObjectsFromCurrentMode(databaseManager.ProjectZones.CurrentZone);
                instantiateObjects.SetZoneOnlyCurrentZoneVisible(databaseManager.ProjectZones.CurrentZone);

                Debug.Log($"SetCurrentZoneFromDropdown: Attempting to push data to database {CurrentZone}");
                DataHandlers.PushStringDataToDatabaseReference(databaseManager.dbReferenceCurrentMode, JsonConvert.SerializeObject(CurrentZone));
            }
            else
            {
                Debug.LogWarning("SetCurrentZoneFromDropdown: Dropdown value is out of range.");
            }
        }
        public void SetRealtimeMimicControlsOnStart()
        {
            //Find FollowMe Button and then add event trigger componnet to it.
            RealtimeMimicControlsParent = MimicControlsParent.FindObject("RealtimeMimicControls");
            FollowMeButtonCover = RealtimeMimicControlsParent.FindObject("FollowMeButtonCover");
            FollowMeButton = RealtimeMimicControlsParent.FindObject("FollowMeButton");
            FollowMeButton.AddComponent<CompasXRButtonHeldEvent>();
            FollowMeButtonHeldEventComponent = FollowMeButton.GetComponent<CompasXRButtonHeldEvent>();

            RealtimeMimicIOToggleGameObject = RealtimeMimicControlsParent.FindObject("IOToggle");
            Toggle RealtimeMimicIOToggle = RealtimeMimicIOToggleGameObject.GetComponentInChildren<Toggle>();
            RealtimeMimicIOToggle.onValueChanged.AddListener(ToggleIOForRealtimeMimicMethod);

            RealtimeMimicMirrorToggleGameObject = RealtimeMimicControlsParent.FindObject("Mirror");
            RealtimeMimicMirrorToggle = RealtimeMimicMirrorToggleGameObject.GetComponentInChildren<Toggle>();
            RealtimeMimicMirrorToggle.onValueChanged.AddListener(RealtimeMimicMirrorToggleMethod);

            if (FollowMeButtonHeldEventComponent == null)
            {
                Debug.LogError("JOETESTING : FollowMeButtonHeldEventComponent is null.");
            }
            else
            {
                Debug.Log("JOETESTING : FollowMeButtonHeldEventComponent is not null.");
            }

            if (RealtimeMimicControlsParent == null)
            {
                Debug.LogError("JOETESTING : RealtimeMimicControls is null.");
            }
            else
            {
                Debug.Log("JOETESTING : RealtimeMimicControls is not null.");
            }

            if (FollowMeButton == null)
            {
                Debug.Log("JOETESTING : FollowMeButton is null.");
            }
            else
            {
                Debug.Log("JOETESTING : FollowMeButton is not null.");
            }

            //TODO: //TODO: //TODO: //TODO: //TODO: THIS IS LITERALLY JUST FOR TESTING PURPOSES IN THE REALTIME MIMIC.
            RealtimeMimicEditorTestToggleObject = RealtimeMimicControlsParent.FindObject("TestingToggle");
            RealtimeMimicEditorTestToggleObject = RealtimeMimicControlsParent.FindObject("TestingToggle");
            Toggle RealtimeMimicTestingToggle = RealtimeMimicEditorTestToggleObject.GetComponentInChildren<Toggle>();
            RealtimeMimicTestingToggle.onValueChanged.AddListener(TEMPORARYToggleRealtimeMimicIsPressedTestingMethodTEMPORARY);


        }
        public void ResetInferenceGameObjectsInScene()
        {
            /*
            * Method is used to reset the inference game objects.
            */
            Debug.Log("ResetInferenceGameObjects: Resetting Inference Game Objects.");
            if (OnScreenWaitForInferenceMessage.activeSelf)
            {
                OnScreenWaitForInferenceMessage.SetActive(false);
            }
            if (instantiateObjects.InferenceGoalsManager.CurrentGoal != null)
            {
                instantiateObjects.InferenceGoalsManager.ColorEntireGoal(instantiateObjects.InferenceGoalsManager.CurrentGoal, instantiateObjects.GoalUnsatisfiedMaterial, false);
            }
        }

        //TODO: This is inference button methods and testing
        public void RequestInferenceButtonMethod()
        {
            /*
            * Method is used to request inference from the robot.
            */
            Debug.Log("RequestInferenceButtonMethod: Requesting Inference from the Robot.");
            if (trajectoryVisualizer.ActiveRobot == null)
            {
                Debug.Log("RequestInferenceButtonMethods: Active Robot is null");
                string message = "WARNING: Active Robot is currently null. An active robot must be set before requesting inference.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref InferenceActiveRobotNullMessage, "ActiveRobotNullWarningMessage", MessagesParent, message, "MimicRequestTrajectoryButtonMethod: Active Robot is null.");
                return;
            }
            else
            {
                //TODO: This would be better to be a direct instance of the box object rather then the geometry frame, but it will just be the center frame information
                Dictionary<string, Frame> currentGeometryFramesAsDict = databaseManager.GetObservedGeometryFramesAsDict();

                InferenceRequestMessage inferenceRequestMessage = new InferenceRequestMessage
                (
                    currentGeometryFramesAsDict,
                    INITIALINFERENCEREQUEST,
                    mqttTrajectoryManager.serviceManager.ActiveRobotName
                );
                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceRequestTopic, inferenceRequestMessage.GetData());
                Debug.Log($"RequestInferenceButtonMethod: Published Inference Request Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceRequestTopic} with data: {inferenceRequestMessage.GetData()}");
                OnScreenWaitForInferenceMessage.SetActive(true);
                SetInferenceRequestUIControlsVisibilityandInteractibility(true, false, false, false, false);
                if (INITIALINFERENCEREQUEST)
                {
                    INITIALINFERENCEREQUEST = false;
                    Debug.Log("RequestInferenceButtonMethod: Setting INITIALINFERENCEREQUEST to false because this should only happend on the first request.");
                }

                //Adding Logging Service for Inference Request
                LogService.Log($"RequestInferenceButtonMethod: Published Inference Request Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceRequestTopic}");
                LogService.LogDict(inferenceRequestMessage.GetData(), "Inference Request Message Data:");
            }
        }
        public void InferenceReviewSliderReviewCompoundTrajectories(float value)
        {
            if (mqttTrajectoryManager.serviceManager.InferenceResultsMessages.Count > 0)
            {
                if (mqttTrajectoryManager.serviceManager.InferenceResultsMessages[mqttTrajectoryManager.serviceManager.InferenceResultsMessages.Count-1].Trajectories.Count <= 0)
                {
                    Debug.LogWarning("InferenceReviewSliderReviewCompoundTrajectories: Using InferenceResultsMessages for Trajectories.");
                    return;
                }

                List<Trajectory> trajectories = mqttTrajectoryManager.serviceManager.InferenceResultsMessages[mqttTrajectoryManager.serviceManager.InferenceResultsMessages.Count-1].Trajectories;
                List<(int start, int end)> trajectoryRanges = new List<(int, int)>();
                int configCount = 0;

                foreach (Trajectory trajectory in trajectories)
                {
                    int count = trajectory.Points.Count;
                    trajectoryRanges.Add((configCount, configCount + count - 1));
                    configCount += count;
                }

                float SliderValue = value;
                float SliderMin = 0f;
                float SliderMax = 1f;
                int targetGlobalIndex = Mathf.RoundToInt(
                    HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0f, configCount - 1)
                );

                int selectedTrajectoryIndex = -1;
                int localIndex = -1;

                for (int i = 0; i < trajectoryRanges.Count; i++)
                {
                    var (start, end) = trajectoryRanges[i];
                    if (targetGlobalIndex >= start && targetGlobalIndex <= end)
                    {
                        selectedTrajectoryIndex = i;
                        localIndex = targetGlobalIndex - start;
                        break;
                    }
                }

                if (selectedTrajectoryIndex >= 0 && localIndex >= 0)
                {
                    Debug.Log($"Slider = {SliderValue:0.000} → Global Config #{targetGlobalIndex}");
                    Debug.Log($"Belongs to Trajectory #{selectedTrajectoryIndex}, Local Config #{localIndex}");
                    //TODO: CHECK THIS WITH UI MIMIC SLIDER IF THE PREVIOUS AND CURRENT INDEX ARE WORKING PROPERLY.
                    trajectoryVisualizer.ColorRobotConfigfromSliderInputCompoundTrajectories(selectedTrajectoryIndex, localIndex, trajectories, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial, ref trajectoryVisualizer.previousConfigIndex, ref trajectoryVisualizer.previousTrajectoryIndex);

                    LogService.Log($"InferenceReviewSliderReviewCompoundTrajectories: Reviewed Compound Trajectories at Global Config Index {targetGlobalIndex}, Trajectory Index {selectedTrajectoryIndex}, Local Config Index {localIndex}");
                }
                else
                {
                    Debug.LogWarning("Could not map slider to trajectory index.");
                }

            }
            else
            {
                Debug.Log("InferenceReviewSliderReviewCompoundTrajectories: Current Trajectory is null.");
            }
        }
        public void InferenceReviewRejectGoalAndTargetButtonMethod()
        {
            /*
            * Method is used to reject the infered goal and target.
            */
            InferenceUserReplyMessage inferenceReplyMessage = new InferenceUserReplyMessage
            (
                GoalStatusReplyEnum.RejectGoalandTarget,
                instantiateObjects.InferenceGoalsManager.CurrentGoal.Name,
                mqttTrajectoryManager.serviceManager.InferenceSuggestedTargetName,
                mqttTrajectoryManager.serviceManager.ActiveRobotName,
                mqttTrajectoryManager.serviceManager.InferenceContainsExacutableTrajectory
            );
            Debug.Log("InferenceRejectGoalAndTargetButtonMethod: Sending Inference Reject Goal and Target Message. Msg: " + JsonConvert.SerializeObject(inferenceReplyMessage.GetData()));
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceUserReplyTopic, inferenceReplyMessage.GetData());
            Debug.Log($"InferenceRejectGoalAndTargetButtonMethod: Rejecting Infered Goal {instantiateObjects.InferenceGoalsManager.CurrentGoal.Name} and Target {mqttTrajectoryManager.serviceManager.InferenceSuggestedTargetName}.");
            SetInferenceRequestUIControlsVisibilityandInteractibility(true, true, false, false, false);
            instantiateObjects.ResetInferenceGoalsAndTargets();

            if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
            }
            else
            {
                Debug.LogWarning("InferenceReviewRejectGoalAndTargetButtonMethod: Active Robot is null, cannot set interactable state.");
            }
            //TODO: Destroy Trajectory if it exists, and set active robot active again

            //Adding Logging Service for Inference Reject Goal and Target
            LogService.Log($"InferenceRejectGoalAndTargetButtonMethod: Published Inference Reject Goal and Target Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceUserReplyTopic}");
            LogService.LogDict(inferenceReplyMessage.GetData(), "Inference Reject Goal and Target Message Data:");
        }
        public void InferenceAcceptTargetRejectGoalButtonMethod()
        {
            /*
            * Method is used to reject the infered goal and target.
            */
            InferenceUserReplyMessage inferenceReplyMessage = new InferenceUserReplyMessage
            (
                GoalStatusReplyEnum.AcceptTargetRejectGoal,
                instantiateObjects.InferenceGoalsManager.CurrentGoal.Name,
                mqttTrajectoryManager.serviceManager.InferenceSuggestedTargetName,
                mqttTrajectoryManager.serviceManager.ActiveRobotName,
                mqttTrajectoryManager.serviceManager.InferenceContainsExacutableTrajectory
            );

            Debug.Log("InferenceAcceptTargetRejectGoalButtonMethod: Sending Inference Reject Goal and Accept Target Message. Msg: " + JsonConvert.SerializeObject(inferenceReplyMessage.GetData()));
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceUserReplyTopic, inferenceReplyMessage.GetData());
            Debug.Log($"InferenceAcceptTargetRejectGoalButtonMethod: Rejecting Infered Goal {instantiateObjects.InferenceGoalsManager.CurrentGoal.Name} and Target {mqttTrajectoryManager.serviceManager.InferenceSuggestedTargetName}.");
            SetInferenceRequestUIControlsVisibilityandInteractibility(true, true, false, false, false);
            instantiateObjects.ResetInferenceGoalsAndTargets();

            if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
            }
            else
            {
                Debug.LogWarning("InferenceReviewRejectGoalAndTargetButtonMethod: Active Robot is null, cannot set interactable state.");
            }
            //TODO: Destroy Trajectory if it exists, and set active robot active again???? Or Wait a bit???

            //Adding Logging Service for Inference Reject Goal and Accept Target
            LogService.Log($"InferenceAcceptTargetRejectGoalButtonMethod: Published Inference Reject Goal and Accept Target Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceUserReplyTopic}");
            LogService.LogDict(inferenceReplyMessage.GetData(), "Inference Reject Goal and Accept Target Message Data:");
        }
        public void InferenceAcceptGoalButtonMethod()
        {
            /*
            * Method is used to accept the infered goal.
            */
            InferenceUserReplyMessage inferenceReplyMessage = new InferenceUserReplyMessage
            (
                GoalStatusReplyEnum.AcceptTargetandGoal,
                instantiateObjects.InferenceGoalsManager.CurrentGoal.Name,
                mqttTrajectoryManager.serviceManager.InferenceSuggestedTargetName,
                mqttTrajectoryManager.serviceManager.ActiveRobotName,
                mqttTrajectoryManager.serviceManager.InferenceContainsExacutableTrajectory
            );
            Debug.Log("InferenceAcceptGoalButtonMethod: Sending Inference Accept Goal and Target Message. Msg: " + JsonConvert.SerializeObject(inferenceReplyMessage.GetData()));
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceUserReplyTopic, inferenceReplyMessage.GetData());
            Debug.Log($"InferenceAcceptGoalButtonMethod: Accepting Infered Goal {instantiateObjects.InferenceGoalsManager.CurrentGoal.Name}.");
            SetInferenceRequestUIControlsVisibilityandInteractibility(false, false, false, false, false);
            GOALINFERRED = true;

            instantiateObjects.InferenceGoalsManager.GoalStatusObserver.Active = true;

            instantiateObjects.InferenceGoalsManager.GoalStatusObserver.InitializeComponentStates(instantiateObjects.InferenceGoalsManager.CurrentGoal);
            instantiateObjects.InferenceGoalsManager.GoalStatusObserver.CheckAllGoalsStatesFromObservedGeometriesDict(databaseManager.observedGeometriesDict, instantiateObjects.GoalSatisfiedMaterial, instantiateObjects.GoalUnsatisfiedMaterial, instantiateObjects.OBJECT_TRACKING_POSITION_SATISFACTION_TOLERANCE, instantiateObjects.OBJECT_TRACKING_ROTATION_SATISFACTION_TOLERANCE);

            instantiateObjects.PostInferenceSetFirstUnsatisfiedInferenceGoalAsCurrent(instantiateObjects.InferenceGoalsManager.GoalStatusObserver.ComponentStates, instantiateObjects.InferenceSelectedTargetMaterialUnbuilt, instantiateObjects.InferenceSelectedTargetMaterialBuilt);
            instantiateObjects.InferenceGoalsManager.GoalStatusObserver.DebugLogAllComponentStatesAsDictionary();
            SetInferenceUIPostInferenceSuccesState(true, true, false, false, false);

            //Adding Logging Service for Inference Accept Goal and Target
            LogService.Log($"InferenceAcceptGoalButtonMethod: Published Inference Accept Goal and Target Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferenceUserReplyTopic}");
            LogService.LogDict(inferenceReplyMessage.GetData(), "Inference Accept Goal and Target Message Data:");
        }

        //TODO: Post Inference Button Methods
        public void PostInferenceNextTargetButtonMethod()
        {
            /*
            * Method is used to select the next target in the inference targets list.
            */
            if (instantiateObjects.CurrentSelectedGoalComponet == null)
            {
                Debug.LogWarning("PostInferenceNextTargetButtonMethod: CurrentSelectedGoalComponet is null, cannot select next target.");
                return;
            }
            if (instantiateObjects.CurrentSelectedGoalName == "None")
            {
                Debug.LogWarning("PostInferenceNextTargetButtonMethod: CurrentSelectedGoalName is 'None', cannot select next target.");
                return;
            }
            if (trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                if (SetActiveRobotToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                else
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    Debug.LogWarning("PostInferenceNextTargetButtonMethod: Active Robot is null, cannot set interactable state.");
                }
            }
            GoalObjectComponent currentGoalComponent = instantiateObjects.CurrentSelectedGoalComponet;
            string currentGoalComponentName = instantiateObjects.CurrentSelectedGoalName;
            Dictionary<string, GoalObjectComponent> states = instantiateObjects.InferenceGoalsManager.GoalStatusObserver.ComponentStates;

            var m = Regex.Match(currentGoalComponentName, @"^(?<prefix>[A-Za-z]+)(?<num>\d+)$");
            if (m.Success)
            {
                string prefix  = m.Groups["prefix"].Value;
                string numStr  = m.Groups["num"].Value;
                int    width   = numStr.Length;
                int    num     = int.Parse(numStr);
                string nextKey = prefix + (num + 1).ToString($"D{width}");
                if (num == states.Count - 1)
                {
                    Debug.Log("PostInferenceNextTargetButtonMethod: Reached the end of the list, not moving anymore.");
                    return;
                }
                if (states.ContainsKey(nextKey))
                {
                    GoalObjectComponent nextGoalComponent = states[nextKey];
                    instantiateObjects.PostInferenceSetGoalComponentForSelection(nextGoalComponent, instantiateObjects.InferenceSelectedTargetMaterialUnbuilt, instantiateObjects.InferenceSelectedTargetMaterialBuilt);
                    Debug.Log($"PostInferenceNextTargetButtonMethod: Selecting Next Target {nextKey} in the Inference Targets List.");

                    //Adding Logging Service for Post Inference Next Target Selection
                    LogService.Log($"PostInferenceNextTargetButtonMethod: Selecting Next Target {nextKey} in the Inference Targets List.");

                    return;
                }
            }
            else
            {
                Debug.LogWarning("PostInferenceNextTargetButtonMethod: CurrentSelectedGoalName does not match expected pattern, cannot select next target.");
                return;
            }
        }
        public void PostInferencePreviousTargetButtonMethod()
        {
            /*
            * Method is used to select the previous target in the inference targets list.
            */
            /*
            * Method is used to select the next target in the inference targets list.
            */
            if (instantiateObjects.CurrentSelectedGoalComponet == null)
            {
                Debug.LogWarning("PostInferenceNextTargetButtonMethod: CurrentSelectedGoalComponet is null, cannot select next target.");
                return;
            }
            if (instantiateObjects.CurrentSelectedGoalName == "None")
            {
                Debug.LogWarning("PostInferenceNextTargetButtonMethod: CurrentSelectedGoalName is 'None', cannot select next target.");
                return;
            }
            if (trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                if (SetActiveRobotToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                else
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    Debug.LogWarning("PostInferenceNextTargetButtonMethod: Active Robot is null, cannot set interactable state.");
                }
            }

            GoalObjectComponent currentGoalComponent = instantiateObjects.CurrentSelectedGoalComponet;
            string currentGoalComponentName = instantiateObjects.CurrentSelectedGoalName;
            Dictionary<string, GoalObjectComponent> states = instantiateObjects.InferenceGoalsManager.GoalStatusObserver.ComponentStates;

            var m = Regex.Match(currentGoalComponentName, @"^(?<prefix>[A-Za-z]+)(?<num>\d+)$");
            if (m.Success)
            {
                string prefix  = m.Groups["prefix"].Value;
                string numStr  = m.Groups["num"].Value;
                int    width   = numStr.Length;
                int    num     = int.Parse(numStr);
                string prevKey = prefix + (num - 1).ToString($"D{width}");
                if (num == 0)
                {
                    Debug.Log("PostInferenceNextTargetButtonMethod: Reached the start of the list, not moving anymore.");
                    return;
                }
                if (states.ContainsKey(prevKey))
                {
                    GoalObjectComponent prevGoalComponent = states[prevKey];
                    instantiateObjects.PostInferenceSetGoalComponentForSelection(prevGoalComponent, instantiateObjects.InferenceSelectedTargetMaterialUnbuilt, instantiateObjects.InferenceSelectedTargetMaterialBuilt);
                    Debug.Log($"PostInferenceNextTargetButtonMethod: Selecting Next Target {prevKey} in the Inference Targets List.");

                    //Adding Logging Service for Post Inference Previous Target Selection
                    LogService.Log($"PostInferenceNextTargetButtonMethod: Selecting Next Target {prevKey} in the Inference Targets List.");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("PostInferenceNextTargetButtonMethod: CurrentSelectedGoalName does not match expected pattern, cannot select next target.");
                return;
            }          
        }
        public void PostInferenceRequestTargetButtonMethod()
        {
            /*
            * Method is used to request a target for the current goal.
            */
            Debug.Log("PostInferenceRequestTargetButtonMethod: Requesting Target for Current Goal.");

            if (trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                if (SetActiveRobotToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                else
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    Debug.LogWarning("PostInferenceRequestTargetButtonMethod: Active Robot is null, cannot set interactable state.");
                }
            }

            if (instantiateObjects.InferenceGoalsManager.CurrentGoal == null)
            {
                Debug.LogWarning("PostInferenceRequestTargetButtonMethod: Current Goal is null, cannot request target.");
                string message = "WARNING: Current Goal is currently null. A current goal must be set before requesting a target.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PostInferenceCurrentGoalNull, "CurrentGoalNullWarningMessage", MessagesParent, message, "PostInferenceRequestTargetButtonMethod: Current Goal is null.");
                return;
            }
            else
            {
                Debug.LogWarning("PostInferenceRequestTargetButtonMethod: Current Goal is null, cannot request target.");
                if (instantiateObjects.CurrentSelectedGoalComponet.IsSatisfied)
                {
                    Debug.LogWarning("PostInferenceRequestTargetButtonMethod: Current Goal is already satisfied, cannot request target.");
                    return;
                }
            }

            if (trajectoryVisualizer.ActiveRobot == null)
            {
                Debug.LogWarning("PostInferenceRequestTargetButtonMethod: Active Robot is null, cannot request target.");
                string message = "WARNING: Active Robot is currently null. An active robot must be set before requesting a target.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PostInferenceActiveRobotNullMessage, "PostInferenceActiveRobotNullWarningMessage", MessagesParent, message, "PostInferenceRequestTargetButtonMethod: Active Robot is null.");
                return;
            }

            List<string> satisfiedGoals = instantiateObjects.InferenceGoalsManager.GoalStatusObserver.GetCompletedComponentsNames();
            List<string> completedObjectNames = instantiateObjects.InferenceGoalsManager.GoalStatusObserver.GetSatisfyingGeometryNames();
            Dictionary<string, Frame> currentGeometryFramesAsDict = databaseManager.GetObservedGeometryFramesAsDict();

            Debug.Log($"JOEEEEEEE: Completed Object Names: {JsonConvert.SerializeObject(completedObjectNames)}");

            PostInferenceTargetRequestMessage postInferenceTargetRequestMessage = new PostInferenceTargetRequestMessage
            (
                satisfiedGoals,
                completedObjectNames,
                instantiateObjects.InferenceGoalsManager.CurrentGoal.Name,
                instantiateObjects.CurrentSelectedGoalComponet.Name,
                mqttTrajectoryManager.serviceManager.ActiveRobotName,
                currentGeometryFramesAsDict
            );

            Debug.Log($"PostInferenceRequestTargetButtonMethod: Topic : {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferencePostInferenceRequestTarget}  Msg: {JsonConvert.SerializeObject(postInferenceTargetRequestMessage.GetData())}");
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferencePostInferenceRequestTarget, postInferenceTargetRequestMessage.GetData());

            SetInferenceUIPostInferenceSuccesState(true, false, false, false, false);
            //Adding Logging Service for Post Inference Request Target
            LogService.Log($"PostInferenceRequestTargetButtonMethod: Published Post Inference Request Target Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferencePostInferenceRequestTarget}");
            LogService.LogDict(postInferenceTargetRequestMessage.GetData(), "Post Inference Request Target Message Data:");

        }
        public void PostInferenceExecuteTrajectoryButtonMethod()
        {
            if (trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                if (SetActiveRobotToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                else
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    Debug.LogWarning("PostInferenceExecuteTrajectoryButtonMethod: Active Robot is null, cannot set interactable state.");
                }
            }
            if (instantiateObjects.CurrentSelectedGoalComponet == null)
            {
                Debug.LogWarning("PostInferenceExecuteTrajectoryButtonMethod: CurrentSelectedGoalComponet is null, cannot execute trajectory.");
                return;
            }
            if (instantiateObjects.CurrentSelectedGoalName == "None")
            {
                Debug.LogWarning("PostInferenceExecuteTrajectoryButtonMethod: CurrentSelectedGoalName is 'None', cannot execute trajectory.");
                return;
            }
            if (mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages.Count == 0)
            {
                Debug.LogWarning("PostInferenceExecuteTrajectoryButtonMethod: No Post Inference Trajectory Results Messages, cannot execute trajectory.");
                return;
            }
            if (mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages[mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages.Count - 1].Trajectories.Count == 0)
            {
                Debug.LogWarning("PostInferenceExecuteTrajectoryButtonMethod: No Trajectories in the last Post Inference Trajectory Results Message, cannot execute trajectory.");
                return;
            }

            List<Trajectory> trajectories = mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages[mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages.Count - 1].Trajectories;

            PostInferenceExecuteTrajectoryMessage postInferenceExecuteTrajectoryMessage = new PostInferenceExecuteTrajectoryMessage
            (
                instantiateObjects.InferenceGoalsManager.CurrentGoal.Name,
                instantiateObjects.CurrentSelectedGoalComponet.Name,
                mqttTrajectoryManager.serviceManager.ActiveRobotName
            );

            Debug.Log("PostInferenceExecuteTrajectoryButtonMethod: Sending Post Inference Execute Trajectory Message. Msg: " + JsonConvert.SerializeObject(postInferenceExecuteTrajectoryMessage.GetData()));
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferencePostInferenceExecuteTargetTopic, postInferenceExecuteTrajectoryMessage.GetData());
            SetInferenceUIPostInferenceSuccesState(true, true, true, false, false);

            //Adding Logging Service for Post Inference Execute Trajectory
            LogService.Log($"PostInferenceExecuteTrajectoryButtonMethod: Published Post Inference Execute Trajectory Message to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.inferencePostInferenceExecuteTargetTopic}");
            LogService.LogDict(postInferenceExecuteTrajectoryMessage.GetData(), "Post Inference Execute Trajectory Message Data:");

        }
        public void PostInferenceRejectTrajectoryButtonMethod()
        {
            if (trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
            {
                if (SetActiveRobotToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                else
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    Debug.LogWarning("PostInferenceRejectTrajectoryButtonMethod: Active Robot is null, cannot set interactable state.");
                }
            }
            SetInferenceUIPostInferenceSuccesState(true, true, true, false, false);
            Debug.Log("PostInferenceRejectTrajectoryButtonMethod: Rejecting Current Trajectory.");

            //Adding Logging Service for Post Inference Reject Trajectory
            LogService.Log("PostInferenceRejectTrajectoryButtonMethod: Rejected Current Trajectory.");
        }
        public void PostInferenceTrajectoryReviewSliderMethod(float value)
        { 
            if (mqttTrajectoryManager.serviceManager.InferenceResultsMessages.Count > 0)
            {
                if (mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages[mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages.Count-1].Trajectories.Count <= 0)
                {
                    Debug.LogWarning("InferenceReviewSliderReviewCompoundTrajectories: Using InferenceResultsMessages for Trajectories.");
                    return;
                }

                List<Trajectory> trajectories = mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages[mqttTrajectoryManager.serviceManager.PostInferenceTrajectoryResultsMessages.Count-1].Trajectories;
                List<(int start, int end)> trajectoryRanges = new List<(int, int)>();
                int configCount = 0;

                foreach (Trajectory trajectory in trajectories)
                {
                    int count = trajectory.Points.Count;
                    trajectoryRanges.Add((configCount, configCount + count - 1));
                    configCount += count;
                }

                float SliderValue = value;
                float SliderMin = 0f;
                float SliderMax = 1f;
                int targetGlobalIndex = Mathf.RoundToInt(
                    HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0f, configCount - 1)
                );

                int selectedTrajectoryIndex = -1;
                int localIndex = -1;

                for (int i = 0; i < trajectoryRanges.Count; i++)
                {
                    var (start, end) = trajectoryRanges[i];
                    if (targetGlobalIndex >= start && targetGlobalIndex <= end)
                    {
                        selectedTrajectoryIndex = i;
                        localIndex = targetGlobalIndex - start;
                        break;
                    }
                }

                if (selectedTrajectoryIndex >= 0 && localIndex >= 0)
                {
                    Debug.Log($"Slider = {SliderValue:0.000} → Global Config #{targetGlobalIndex}");
                    Debug.Log($"Belongs to Trajectory #{selectedTrajectoryIndex}, Local Config #{localIndex}");
                    //TODO: CHECK THIS WITH UI MIMIC SLIDER IF THE PREVIOUS AND CURRENT INDEX ARE WORKING PROPERLY.
                    trajectoryVisualizer.ColorRobotConfigfromSliderInputCompoundTrajectories(selectedTrajectoryIndex, localIndex, trajectories, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial, ref trajectoryVisualizer.previousConfigIndex, ref trajectoryVisualizer.previousTrajectoryIndex);

                    //Adding Logging Service for Post Inference Trajectory Review
                    LogService.Log($"PostInferenceTrajectoryReviewSliderMethod: Reviewed Compound Trajectories at Global Config Index {targetGlobalIndex}, Trajectory Index {selectedTrajectoryIndex}, Local Config Index {localIndex}");
                }
                else
                {
                    Debug.LogWarning("Could not map slider to trajectory index.");
                }

            }
            else
            {
                Debug.Log("InferenceReviewSliderReviewCompoundTrajectories: Current Trajectory is null.");
            }
        }

        //TODO: Other methods
        public void RealtimeMimicMirrorToggleMethod(bool value)
        {
            /*
            * Method is used to set the mirror mode for the Realtime Mimic.
            */
            Debug.Log($"RealtimeMimicMirrorToggleMethod: Setting Realtime Mimic Mirror Mode to {value}");
            OnMirrorToggleChanged(value);
        }
        public void SetMimicUserInitiatedMimicControlsOnStart()
        {

            //Find Mimic Control Objects
            UserInitiatedMimicControls = MimicControlsParent.FindObject("UserInitiatedMimicControls");
            UserInitiatedMimicControlsSetPointsUIObjects = UserInitiatedMimicControls.FindObject("SetPointsUI");
            UserInitiatedMimicControlsReviewAndExecuteTrajectoryUIObjects = UserInitiatedMimicControls.FindObject("ReviewAndExecuteTrajectoryUI");

            //Find SetPointsButton Objects
            UserInterface.FindButtonandSetOnClickAction(
            UserInitiatedMimicControlsSetPointsUIObjects,
            ref MimicSetPointsButtonObject,
            "SetPointButton", SetUserInitiatedMimicPointButtonMethod);

            //Find UndoPointsButton ObjectsU
            UserInterface.FindButtonandSetOnClickAction(
            UserInitiatedMimicControlsSetPointsUIObjects,
            ref UserInitiatedMimicControlsUndoPointButtonObject,
            "UndoPointButton", UndoUserInitiatedMimicPointButtonMethod);

            //Find UndoPointsButton Objects
            UserInterface.FindButtonandSetOnClickAction(
            UserInitiatedMimicControlsSetPointsUIObjects,
            ref UserInitiatedMimicRequestTrajectoryButtonObject,
            "RequestTrajectoryButton", UserInitiatedMimicRequestTrajectoryButtonMethod);

            //Find Execute Button Objects
            UserInterface.FindButtonandSetOnClickAction(
            UserInitiatedMimicControlsReviewAndExecuteTrajectoryUIObjects,
            ref UserInitiatedMimicExecuteTrajectoryButtonObject,
            "ExecuteTrajectoryButton", UserInitiatedMimicExecuteTrajectoryButtonMethod);

            //TODO: Set Target Button.
            UserInitiatedSetTargetButtonObject = UserInitiatedMimicControlsSetPointsUIObjects.FindObject("SetTargetButton");
            UserInitiatedSetTargetButtonObject.AddComponent<CompasXRButtonHeldEvent>();
            UserInitiatedSetTargetButtonHeldEventComponent = UserInitiatedSetTargetButtonObject.GetComponent<CompasXRButtonHeldEvent>();
            UserInitiatedSetTargetButtonHeldEventComponent.vibrate = false;

            //Find Slider Objects
            UserInterface.FindSliderandSetOnValueChangeAction(
            UserInitiatedMimicControlsReviewAndExecuteTrajectoryUIObjects, ref UserInitiatedMimicTrajectoryReviewSliderObject,
            ref UserInitiatedMimicTrajectoryReviewSlider, "TrajectoryReviewSlider", value => UserInitiatedMimicTrajectorySliderReviewCompoundTrajectories(value));

            //Set Mirror Toggle Object
            UserInitiatedMimicMirrorToggleObject = UserInitiatedMimicControlsSetPointsUIObjects.FindObject("Mirror");
            UserInitiatedMimicMirrorToggle = UserInitiatedMimicMirrorToggleObject.GetComponentInChildren<Toggle>();
            UserInitiatedMimicMirrorToggle.onValueChanged.AddListener(UserInitiatedMimicMirrorToggleMethod);

            //Set Mimic OnScreen Messages
            UserInitiatedMimicSetPointGreenScreen = UserInitiatedMimicControlsSetPointsUIObjects.FindObject("SetPointGreenScreen");
            UserInitiatedMimicUndoPointRedScreen = UserInitiatedMimicControlsSetPointsUIObjects.FindObject("UndoPointRedScreen");
        }
        public void ControlARZoneObjectsBasedOnCurrentMode(ProjectZones.CurrentZoneMode currentMode)
        {
            /*
            * Method is used to control the AR Zone Objects based on the current mode.
            */
            Debug.Log($"ControlARZoneObjectsBasedOnCurrentMode: Controlling AR Zone Objects based on the current mode {currentMode}");
            switch (currentMode)
            {
                case ProjectZones.CurrentZoneMode.None:
                    Debug.Log("ControlARZoneObjectsBasedOnCurrentMode: Controlling AR Zone Objects for None Mode.");

                    //Setting the Goal Objects To be not visible.
                    if (instantiateObjects.InferenceGoalsParentObject != null)
                    {
                        instantiateObjects.InferenceGoalsParentObject.SetActive(false);
                    }
                    if (instantiateObjects.MimicGoalsParentObject != null)
                    {
                        instantiateObjects.MimicGoalsParentObject.SetActive(false);
                    }
                    instantiateObjects.InferenceGoalsManager.GoalStatusObserver.Active = false;
                    instantiateObjects.MimicGoalsManager.GoalStatusObserver.Active = false;
                    if(trajectoryVisualizer.ActiveTrajectoryParentObject!= null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                    }
                    if(trajectoryVisualizer.humanZoneMimicReachibility != null)
                    {
                        Destroy(trajectoryVisualizer.humanZoneMimicReachibility);
                    }
                    if(instantiateObjects.MimicMirroredGeometryManagerImplementation != null)
                    {
                        instantiateObjects.MimicMirroredGeometryManagerImplementation.Clear();  
                    }
                    break;
                case ProjectZones.CurrentZoneMode.Inference:
                    instantiateObjects.DestroyUserInstatiatedMimicZoneObjects();
                    if(trajectoryVisualizer.ActiveTrajectoryParentObject!= null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                        // trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    }
                    if(trajectoryVisualizer.humanZoneMimicReachibility != null)
                    {
                        Destroy(trajectoryVisualizer.humanZoneMimicReachibility);
                    }
                    Debug.Log("ControlARZoneObjectsBasedOnCurrentMode: Controlling AR Zone Objects for Inference Mode.");

                    //Setting the Goal Objects To be not visible.
                    if (instantiateObjects.InferenceGoalsParentObject != null)
                    {
                        instantiateObjects.InferenceGoalsParentObject.SetActive(true);
                    }
                    if (instantiateObjects.MimicGoalsParentObject != null)
                    {
                        instantiateObjects.MimicGoalsParentObject.SetActive(false);
                    }
                    instantiateObjects.MimicGoalsManager.GoalStatusObserver.Active = false;
                    if(instantiateObjects.MimicMirroredGeometryManagerImplementation != null)
                    {
                        instantiateObjects.MimicMirroredGeometryManagerImplementation.Clear();  
                    }
                    break;
                case ProjectZones.CurrentZoneMode.Mimic:
                    Debug.Log("ControlARZoneObjectsBasedOnCurrentMode: Controlling AR Zone Objects for Mimic Mode.");
                    ControlRobotVisibilityBasedOnMode(ProjectZones.CurrentZoneMode.Mimic);

                    instantiateObjects.InferenceGoalsManager.GoalStatusObserver.Active = false;
                    instantiateObjects.MimicGoalsManager.GoalStatusObserver.Active = true;
                    instantiateObjects.MimicGoalsManager.GoalStatusObserver.CheckAllGoalsStatesFromObservedGeometriesDict(databaseManager.observedGeometriesDict, instantiateObjects.GoalSatisfiedMaterial, instantiateObjects.GoalUnsatisfiedMaterial, instantiateObjects.OBJECT_TRACKING_POSITION_SATISFACTION_TOLERANCE, instantiateObjects.OBJECT_TRACKING_ROTATION_SATISFACTION_TOLERANCE);
                    if (instantiateObjects.MimicMirroredGeometryManagerImplementation != null)
                    {
                        instantiateObjects.MimicMirroredGeometryManagerImplementation.UpdateAllComponentStates(instantiateObjects.MimicGoalsManager.GoalStatusObserver.ComponentStates);
                    }
                    if (trajectoryVisualizer.ActiveRobot != null)
                    {
                        if (trajectoryVisualizer.humanZoneMimicReachibility == null)
                        {
                            trajectoryVisualizer.AddReachabilitlyToHumanZone(trajectoryVisualizer.ActiveRobot.FindObject(mqttTrajectoryManager.serviceManager.ActiveRobotName),
                            databaseManager.ProjectZones.MimicZones["human_zone"].ZoneObject, databaseManager.ProjectZones.MimicZones["robot_zone"].ZoneObject, ReachabilityToggleObject.GetComponentInChildren<Toggle>().isOn);
                        }
                        else
                        {
                            Debug.LogWarning("ControlARZoneObjectsBasedOnCurrentMode: Reachability Toggle is not on.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ControlARZoneObjectsBasedOnCurrentMode: Active Robot is null.");
                    }

                    if (instantiateObjects.InferenceGoalsParentObject != null)
                    {
                        instantiateObjects.InferenceGoalsParentObject.SetActive(false);
                    }
                    if (instantiateObjects.MimicGoalsParentObject != null)
                    {
                        instantiateObjects.MimicGoalsParentObject.SetActive(true);
                        instantiateObjects.userIniatedMimicPickandPlaceManager.Reset();
                        instantiateObjects.CreateDuplicatGeometriesForMimic(ref instantiateObjects.AllGeometiresParentObjects, ref instantiateObjects.MirroredGeometriesParentObject);
                    }

                    break;
                default:
                    Debug.LogWarning("ControlARZoneObjectsBasedOnCurrentMode: Current Zone Mode is not set.");
                    break;
            }
        }
        public void ControlRobotVisibilityBasedOnMode(ProjectZones.CurrentZoneMode currentZoneMode)
        {
            /*
            * Method is used to control the robot visibility based on the current zone mode.
            */
            Debug.Log($"ControlRobotVisibilityBasedOnMode: Controlling Robot Visibility based on the current zone mode {currentZoneMode}");
            switch (currentZoneMode)
            {
                case ProjectZones.CurrentZoneMode.None:
                    Debug.Log("ControlRobotVisibilityBasedOnMode: Controlling Robot Visibility for None Mode.");
                    break;
                case ProjectZones.CurrentZoneMode.Inference:
                    Debug.Log("ControlRobotVisibilityBasedOnMode: Controlling Robot Visibility for Inference Mode.");
                    break;
                case ProjectZones.CurrentZoneMode.Mimic:
                    if (mqttTrajectoryManager.serviceManager.ActiveRobotName != null)
                    {
                        if(trajectoryVisualizer.ActiveRobot != null && SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                        {
                            Debug.Log("ControlRobotVisibilityBasedOnMode: Controlling Robot Visibility for Mimic Mode.");
                            string robotName = RobotSelectionDropdown.options[RobotSelectionDropdown.value].text;
                            if(robotName == mqttTrajectoryManager.serviceManager.ActiveRobotName)
                            {
                                Debug.Log("ControlRobotVisibilityBasedOnMode: Active Robot Name matches the selected robot name.");
                                trajectoryVisualizer.ActiveRobot.SetActive(true);
                            }
                            else
                            {
                                Debug.LogWarning("ControlRobotVisibilityBasedOnMode: Active Robot Name does not match the selected robot name.");
                            }

                        }
                        else
                        {
                            Debug.LogWarning("ControlRobotVisibilityBasedOnMode: Active Robot Toggle is not on.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ControlRobotVisibilityBasedOnMode: Active Robot Name is null.");
                    }
                    break;
                default:
                    Debug.LogWarning("ControlRobotVisibilityBasedOnMode: Current Zone Mode is not set.");
                    break;
            }
        }
        public void ColorZonesBasedOnCurrentMode(ProjectZones.CurrentZoneMode cuttentMode)
        {
            /*
            * Method is used to color the zones based on the current mode.
            */
            Debug.Log($"ColorZonesBasedOnCurrentMode: Coloring Zones based on the current mode {cuttentMode}");
            switch (cuttentMode)
            {
                case ProjectZones.CurrentZoneMode.None:
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.MimicZones, false);
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.InferenceZones, false);
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.BoundaryZone, false);
                    break;
                case ProjectZones.CurrentZoneMode.Inference:
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.MimicZones, false);
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.InferenceZones, true);
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.BoundaryZone, false);
                    break;
                case ProjectZones.CurrentZoneMode.Mimic:
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.MimicZones, true);
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.InferenceZones, false);
                    ColorZoneBasedOnActivity(databaseManager.ProjectZones.BoundaryZone, false);
                    break;
                default:
                    Debug.LogWarning("ColorZonesBasedOnCurrentMode: Current Zone Mode is not set.");
                    break;
            }
            
        }
        public void ColorZoneBasedOnActivity(Dictionary<string, Zone> zoneDict, bool isActive)
        {
            /*
            * Method is used to color the zones based on the activity.
            */
            foreach (KeyValuePair<string, Zone> zone in zoneDict)
            {
                if (isActive)
                {
                    zone.Value.ZoneObject.GetComponent<MeshRenderer>().material = zone.Value.ZoneActiveMaterial;
                }
                else
                {
                    zone.Value.ZoneObject.GetComponent<MeshRenderer>().material = zone.Value.ZoneInactiveMaterial;
                }
            }
        }
        public void SetRealtimeMimicPointBasicTEMPORARY() //TODO: JOEEEEE CONTINUE HERE....
        {
            /*
            * Method is used to set the mimic point based on the human and robot zone objects.
            */
            Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Mimic Zone Objects: " +databaseManager.ProjectZones.MimicZones + "Type of Mimic Zones: " + databaseManager.ProjectZones.MimicZones.GetType());
    
            var mimicZones = databaseManager.ProjectZones.MimicZones;

            if (mimicZones.TryGetValue("human_zone", out Zone humanZone))
            {
                if (mimicZones.TryGetValue("robot_zone", out Zone robotZone))
                {
                    GameObject humanZoneObject = humanZone.ZoneObject;
                    GameObject robotZoneObject = robotZone.ZoneObject;

                    // Additional logic for both zones can go here
                    Vector3 cameraPositionObjectPosition = arCamera.transform.position;
                    Quaternion cameraRotationObjectRotation = arCamera.transform.rotation;
                    Vector3 devicePosePosition = devicePoseBehavior.transform.position;


                    if (ObjectInstantiaion.IsPositionWithinBox(humanZoneObject, cameraPositionObjectPosition))
                    {
                        if (trajectoryVisualizer.ActiveRobot == null)
                        {
                            Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Active Robot is Null.");
                            string message = "WARNING: You must select an active robot in order to mimic realtime.";
                            UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref RealtimeMimicActiveRobotNull, "RealtimeMimicActiveRobotNull", MessagesParent, message, "SetRealtimeMimicPoint: ActiveRobot is null.");
                            return;
                        }

                        if (trajectoryVisualizer.humanZoneMimicReachibility == null)
                        {
                            Debug.LogError("CreateRealtimeMimicPointsBasicTEMPORARY: Human Zone Reachability is null for some weird reason.");
                            return;
                        }
                        else
                        {
                            if (!ObjectInstantiaion.IsPositionWithinObject(trajectoryVisualizer.humanZoneMimicReachibility, cameraPositionObjectPosition))
                            {
                                Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Camera Position is not within the Human Zone Object.");
                                string message = "WARNING: This mimic point is outside of the Robots reachability.";
                                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref RealtimeMimicActiveRobotNull, "RealtimeMimicActiveRobotNull", MessagesParent, message, "SetRealtimeMimicPoint: ActiveRobot is null.");
                                return;
                            }
                        }

                        //TODO: ADD METHOD TO RETURN IF THRESHOLD IS NOT MET.
                        float DRAWINGTHRESHOLD = instantiateObjects.REALTIMEMIMICDRAWRINGTHRESHOLD; //TODO: TESTING....
                        float ROTTHRESHOLD = 3.0f; //TODO: THIS IS TEMPORARY AND NEEDS TO BE CHANGED.
                        if (REALTIMEMIMICINDEXCOUNTER >= 1)
                        {
                            Vector3 lastRealtimeMimicPointPosition = instantiateObjects.RealtimeMimicHumanPoints[instantiateObjects.RealtimeMimicHumanPoints.Count - 1].transform.position;
                            if (lastRealtimeMimicPointPosition == null)
                            {
                                Debug.LogError("CreateRealtimeMimicPointsBasicTEMPORARY: Last Realtime Mimic Point is null.");
                                return;
                            }
                            else
                            {
                                Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Last Realtime Mimic Point is not null.");
                            }

                            //TODO: I think this needs to be rot and position.
                            Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Last Realtime Mimic Point Position: {lastRealtimeMimicPointPosition} Camera Position: {cameraPositionObjectPosition} Distance: {Vector3.Distance(cameraPositionObjectPosition, lastRealtimeMimicPointPosition)} Bool Value: {ObjectInstantiaion.Vector3sAreCloserThenThreshold(cameraPositionObjectPosition, lastRealtimeMimicPointPosition, DRAWINGTHRESHOLD)}");
                            if (ObjectInstantiaion.RotationsAreCloserThanThreshold(cameraRotationObjectRotation, instantiateObjects.LASTSENTCAMERAROTATIONFORREALTIMEMIMIC, ROTTHRESHOLD) && ObjectInstantiaion.Vector3sAreCloserThenThreshold(cameraPositionObjectPosition, lastRealtimeMimicPointPosition, DRAWINGTHRESHOLD))
                            {
                                Debug.LogWarning("CreateRealtimeMimicPointsBasicTEMPORARY: Rotations and Positions are closer than threshold, not creating new points.");
                                return;
                            }
                            else
                            {
                                Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Camera Position is within the Human Zone Object and Points are not closer than threshold point will be set for Index {REALTIMEMIMICINDEXCOUNTER}.");
                                //Set Lines active and Points active
                                instantiateObjects.RealtimeMimicObjects.SetActive(true);

                                instantiateObjects.CreateRealtimeMimicPointsBasicTEMPORARY(humanZoneObject, robotZoneObject,
                                ref instantiateObjects.RealtimeMimicHumanPoints, ref instantiateObjects.RealtimeMimicRobotPoints,
                                instantiateObjects.RealtimeMimicHumanPointsParent, instantiateObjects.RealtimeMimicRobotPointsParent,
                                instantiateObjects.RealtimeMimicHumanLine, instantiateObjects.RealtimeMimicRobotLine, REALTIMEMIMICINDEXCOUNTER, RealtimeMimicMirrorToggle.isOn);

                                //TODO: CONVERT TO FRAME FROM LAST GAMEOBJECT IN ROBOT POINTS LIST.
                                GameObject lastRealtimeMimicPointTest = instantiateObjects.RealtimeMimicRobotPoints[instantiateObjects.RealtimeMimicRobotPoints.Count - 1];
                                Frame lastMimicPointFrame = ObjectTransformations.ConvertGameObjectToRightHandFrameDataRoboticTerritories(lastRealtimeMimicPointTest, instantiateObjects.ZonesARPrefabObjects);
                                int pointIndex = REALTIMEMIMICINDEXCOUNTER; //TODO: THIS IS TEMPORARY AND NEEDS TO BE CHANGED.
                                string robotName = RobotSelectionDropdown.options[RobotSelectionDropdown.value].text;

                                //TODO: Testing ADD here.....
                                List<string> colliderObjectHits = instantiateObjects.GetAllCollidersNamesAtPoint(devicePosePosition);
                                (int pickState, string pickOrPlaceItemName) = instantiateObjects.DetermineOrAndPlaceObjectFromColliderHitsRTMimic(colliderObjectHits, mqttTrajectoryManager.realtimeMimicPickandPlaceManager);

                                if (pickState == 0)
                                {
                                    Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: This point should be made as normal and published {pickOrPlaceItemName}");
                                }
                                else if (pickState == 1)
                                {
                                    Debug.LogWarning($"CreateRealtimeMimicPointsBasicTEMPORARY: This point should not be made..... for some reason {pickOrPlaceItemName}");
                                    //TODO: Signal On screen message that point cannot be made.
                                    return;
                                }
                                else if (pickState == 2)
                                {
                                    Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: This point should be a pick point {pickOrPlaceItemName}");
                                    //TODO: Signal On screen message that pick could be made. The only thing is that I need to be sure the published value has correct index.
                                    string currentGoalName = instantiateObjects.MimicMirroredGeometryManagerImplementation.CurrentMimicGoalName;
                                    GameObject pickFrameGameObject = instantiateObjects.FindHumanandRobotTargetInformationRTMimic(pickOrPlaceItemName, instantiateObjects.TrackedGeometriesParentObject, instantiateObjects.MimicGoalsManager.CurrentGoal.GoalGameObject, instantiateObjects.MimicMirroredGeometryManagerImplementation.TrackedGeometriesParent, instantiateObjects.MimicMirroredGeometryManagerImplementation.GoalsParent.FindObject(currentGoalName));
                                    Frame pickFrame = ObjectTransformations.ConvertGameObjectToRightHandFrameDataRoboticTerritories(pickFrameGameObject, instantiateObjects.ZonesARPrefabObjects);
                                    if (pickFrameGameObject == null || pickFrame == null) //TODO: This should return a big Error Onscreen... I think....
                                    {
                                        Debug.LogWarning("CreateRealtimeMimicPointsBasicTEMPORARY : Cannot find PICK frame game object.");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"CreateRealtimeMimicPointsBasicTEMPORARY: Found PICK frame game object {pickFrameGameObject.name}.");
                                    }

                                    RealtimeMimicRequestMessage realtimeMimicRequestPickMessage = new RealtimeMimicRequestMessage
                                    (
                                        lastMimicPointFrame,
                                        robotName,
                                        pointIndex,
                                        isPick: true,
                                        isPlace: false,
                                        geometryFrame: pickFrame
                                    );
                                    //TODO: This is storing to publish later based on the user interaction.....
                                    mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish = realtimeMimicRequestPickMessage;
                                    Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Storing RT Mimic Pick for {robotName} with pointIndex {pointIndex} and {JsonConvert.SerializeObject(realtimeMimicRequestPickMessage.GetData())} : Awaiting user confirmation to publish.");
                                    RealtimeMimicSignalOnscreenMessageForPick();
                                    // REALTIMEMIMICLASTSENTINDEX = pointIndex; //TODO: This is important and needs to be added to the yes or no pick and place.
                                    REALTIMEMIMICLASTSENTINDEX = pointIndex;
                                    return;
                                }
                                else if (pickState == 3)
                                {
                                    Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: This point should be a place point {pickOrPlaceItemName}.");
                                    string currentGoalName = instantiateObjects.MimicMirroredGeometryManagerImplementation.CurrentMimicGoalName;
                                    GameObject placeFrameGameObject = instantiateObjects.FindHumanandRobotTargetInformationRTMimic(pickOrPlaceItemName, instantiateObjects.TrackedGeometriesParentObject, instantiateObjects.MimicGoalsManager.CurrentGoal.GoalGameObject, instantiateObjects.MimicMirroredGeometryManagerImplementation.TrackedGeometriesParent, instantiateObjects.MimicMirroredGeometryManagerImplementation.GoalsParent.FindObject(currentGoalName));
                                    Frame placeFrame = ObjectTransformations.ConvertGameObjectToRightHandFrameDataRoboticTerritories(placeFrameGameObject, instantiateObjects.ZonesARPrefabObjects);

                                    if (placeFrameGameObject == null || placeFrame == null) //TODO: This should return a big Error Onscreen... I think....
                                    {
                                        Debug.LogWarning("CreateRealtimeMimicPointsBasicTEMPORARY : Cannot find PLACE frame game object.");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"CreateRealtimeMimicPointsBasicTEMPORARY: Found PLACE frame game object {placeFrameGameObject.name}.");
                                    }

                                    RealtimeMimicRequestMessage realtimeMimicRequestPlaceMessage = new RealtimeMimicRequestMessage
                                    (
                                        lastMimicPointFrame,
                                        robotName,
                                        pointIndex,
                                        isPick: false,
                                        isPlace: true,
                                        geometryFrame: placeFrame
                                    );
                                    //TODO: This is storing to publish later based on the user interaction.....
                                    mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish = realtimeMimicRequestPlaceMessage;
                                    Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Storing RT Mimic Place for {robotName} with pointIndex {pointIndex} and {JsonConvert.SerializeObject(realtimeMimicRequestPlaceMessage.GetData())} : Awaiting user confirmation to publish.");
                                    // REALTIMEMIMICLASTSENTINDEX = pointIndex; //TODO: This is important and needs to be added to the yes or no pick and place.
                                    RealtimeMimicSignalOnscreenMessageForPlace();
                                    return;
                                }
                                else
                                {
                                    Debug.LogError("CreateRealtimeMimicPointsBasicTEMPORARY: This point should not be made.....");
                                    return;
                                }

                                RealtimeMimicRequestMessage realtimeMimicRequestMessage = new RealtimeMimicRequestMessage
                                (
                                    lastMimicPointFrame,
                                    robotName,
                                    pointIndex
                                );
                                Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Sending message to {robotName} with pointIndex {pointIndex} with structure {JsonConvert.SerializeObject(realtimeMimicRequestMessage.GetData())} to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic}");
                                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic, realtimeMimicRequestMessage.GetData());
                                REALTIMEMIMICLASTSENTINDEX = pointIndex;
                                REALTIMEMIMICINDEXCOUNTER++;

                            }
                        }
                        else
                        {
                            Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Camera Position is Outside of the Threshold and Point Will Be Set for Index {REALTIMEMIMICINDEXCOUNTER}");
                            //Set Lines active and Points active
                            instantiateObjects.RealtimeMimicObjects.SetActive(true);
                            //TODO: Set the the last two sent pick and place messages to null just to be safe.
                            mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish = null;
                            mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish = null;

                            //TODO: This needs to have an index as and input.
                            instantiateObjects.CreateRealtimeMimicPointsBasicTEMPORARY(humanZoneObject, robotZoneObject,
                            ref instantiateObjects.RealtimeMimicHumanPoints, ref instantiateObjects.RealtimeMimicRobotPoints,
                            instantiateObjects.RealtimeMimicHumanPointsParent, instantiateObjects.RealtimeMimicRobotPointsParent,
                            instantiateObjects.RealtimeMimicHumanLine, instantiateObjects.RealtimeMimicRobotLine, REALTIMEMIMICINDEXCOUNTER, RealtimeMimicMirrorToggle.isOn);

                            //TODO: CONVERT TO FRAME FROM LAST GAMEOBJECT IN ROBOT POINTS LIST.
                            GameObject lastRealtimeMimicPoint = instantiateObjects.RealtimeMimicRobotPoints[instantiateObjects.RealtimeMimicRobotPoints.Count - 1];
                            Frame lastMimicPointFrame = ObjectTransformations.ConvertGameObjectToRightHandFrameDataRoboticTerritories(lastRealtimeMimicPoint, instantiateObjects.ZonesARPrefabObjects);
                            int pointIndex = REALTIMEMIMICINDEXCOUNTER; //TODO: THIS IS TEMPORARY AND NEEDS TO BE CHANGED.

                            string robotName = RobotSelectionDropdown.options[RobotSelectionDropdown.value].text;
                            RealtimeMimicRequestMessage realtimeMimicRequestMessage = new RealtimeMimicRequestMessage
                            (
                                lastMimicPointFrame,
                                robotName,
                                pointIndex,
                                initialRequest: true
                            );
                            Debug.Log($"CreateRealtimeMimicPointsBasicTEMPORARY: Sending message to {robotName} with point Index {pointIndex} with structure {JsonConvert.SerializeObject(realtimeMimicRequestMessage.GetData())} to topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic}");
                            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic, realtimeMimicRequestMessage.GetData());
                            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT != null || instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN != null)
                            {
                                if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.activeSelf)
                                {
                                    instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.SetActive(false);
                                }
                                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT = null;

                                if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.activeSelf)
                                {
                                    instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.SetActive(false);
                                }
                                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN = null;
                            }
                            REALTIMEMIMICLASTSENTINDEX = pointIndex;
                            REALTIMEMIMICINDEXCOUNTER++;
                        }
                    }
                    else
                    {
                        Debug.Log("CreateRealtimeMimicPointsBasicTEMPORARY: Camera Position is not within the Human Zone Object.");
                        string message = "WARNING: This Point cannot be set because it is not within the human editing zone.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MimicSetPointOutsideOfHumanZone, "MimicPointOutsideOfBounds", MessagesParent, message, "RealtimeMimicRequest: Point outside of human zone.");
                    }
                }
                else
                {
                    Debug.LogError("CreateRealtimeMimicPointsBasicTEMPORARY: 'robot_zone' key not found in MimicZones.");
                }
            }
            else
            {
                Debug.LogError("CreateRealtimeMimicPointsBasicTEMPORARY: 'human_zone' key not found in MimicZones.");
            }
        }
        public void UpdateLastCompletedIndexForRealtimeMimicPoint(int lastCompletedIndex)
        {
            if (lastCompletedIndex >= 0)
            {
                if (lastCompletedIndex > REALTIMEMIMICLASTCOMPLETEDINDEX)
                {
                    Debug.Log($"UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index {lastCompletedIndex} is greater than current Last Completed Index {REALTIMEMIMICLASTCOMPLETEDINDEX}");
                    REALTIMEMIMICLASTCOMPLETEDINDEX = lastCompletedIndex;
                }
                else
                {
                    Debug.LogWarning($"UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index {lastCompletedIndex} is less than current Last Completed Index {REALTIMEMIMICLASTCOMPLETEDINDEX}");
                    return;
                }

                if (REALTIMEMIMICLASTCOMPLETEDINDEX == REALTIMEMIMICLASTSENTINDEX && REALTIMEMIMICINDEXCOUNTER != 0)
                {
                    Debug.Log("UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index is equal to Last Sent Index and Index Counter is not 0, User Is still Holding the Button.");
                }
                else if (REALTIMEMIMICLASTCOMPLETEDINDEX != REALTIMEMIMICLASTSENTINDEX && REALTIMEMIMICINDEXCOUNTER == 0)
                {
                    FollowMeButton.GetComponentInChildren<Button>().interactable = false;
                    Debug.LogWarning("UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index is still less then sent, but user has let go of the button.");
                }
                //TODO: This will break if one gets lost in transit....
                else if (REALTIMEMIMICLASTCOMPLETEDINDEX == REALTIMEMIMICLASTSENTINDEX && REALTIMEMIMICINDEXCOUNTER == 0)
                {
                    Debug.Log("UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index is equal to Last Sent Index and Index Counter is 0, User has let go of the button.");
                    FollowMeButton.GetComponentInChildren<Button>().interactable = true;
                    REALTIMEMIMICLASTCOMPLETEDINDEX = -1;
                    REALTIMEMIMICLASTSENTINDEX = -1;
                    REALTIMEMIMICINDEXCOUNTER = 0;
                    instantiateObjects.DestroyRealtimeMimicZoneObjects();
                }
                else
                {
                    Debug.Log("UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index is not equal to Last Sent Index or Index Counter is 0, not sending next point.");
                }
            }
            else
            {
                Debug.LogWarning("UpdateLastCompletedIndexForRealtimeMimicPoint: Last Completed Index is less than 0.");
            }
        }
        public void SetUserInitiatedMimicPointButtonMethod()
        {
            /*
            * Method is used to set the mimic point based on the human and robot zone objects.
            */
            Debug.Log("SetMimicPoint: Setting Mimic Point based on Human and Robot Zone Objects.");
            Debug.Log("SetMimicPoint: Mimic Zone Objects: " + databaseManager.ProjectZones.MimicZones + "Type of Mimic Zones: " + databaseManager.ProjectZones.MimicZones.GetType());

            if (USERINSTIANTEDTRAJECTORYEXECUTED)
            {
                Debug.LogWarning("SetMimicPoint: User has already executed a trajectory, cannot set more points.");
                instantiateObjects.DestroyUserInstatiatedMimicZoneObjects();
                USERINSTIANTEDTRAJECTORYEXECUTED = false;
                //TODO: Needs to reset the PickandPlaceStateManager
                if(trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
            }

            var mimicZones = databaseManager.ProjectZones.MimicZones;

            if (mimicZones.TryGetValue("human_zone", out Zone humanZone))
            {
                if (mimicZones.TryGetValue("robot_zone", out Zone robotZone))
                {
                    GameObject humanZoneObject = humanZone.ZoneObject;
                    GameObject robotZoneObject = robotZone.ZoneObject;

                    // Additional logic for both zones can go here
                    Vector3 cameraPositionObject = arCamera.transform.position;
                    Vector3 devicePosePosition = devicePoseBehavior.transform.position; //TODO: IMPLEMENT THIS CHANGE.
                    Debug.Log("SetMimicPoint: Camera Position Object: " + cameraPositionObject + "Rotation: " + arCamera.transform.rotation);

                    if (ObjectInstantiaion.IsPositionWithinBox(humanZoneObject, cameraPositionObject)) //TODO: Write method to create mimic points etc.
                    {
                        Debug.Log("SetMimicPoint: Camera Position is within the Human Zone Object.");

                        //TODO: THIS NEEDS TO TAKE CODE FROM THE OLD APPLICATION THAT CHECKS IF THE COLDIDERS AR ON OR NOT...
                        List<string> colliderObjectHits = instantiateObjects.GetAllCollidersNamesAtPoint(devicePosePosition);

                        // (int pickState, string pickOrPlaceItemName) = instantiateObjects.DeterminePickAndPlaceStateFromColliderHitsPickThenPlace(colliderObjectHits);
                        (int pickState, string pickOrPlaceItemName) = instantiateObjects.DeterminePickAndPlaceStateFromColliderHitsPickorPlace(colliderObjectHits);
                        Debug.Log($"SetMimicPoint: Pick State: {pickState}, Pick or Place Object Name: {pickOrPlaceItemName}");

                        if (pickState == 1)
                        {
                            Debug.Log("SetMimicPoint: Point Should not be made, for some reason. This should be notified in the previous function already.");
                            return;
                        }
                        else if (pickState == 2 || pickState == 3)
                        {
                            Debug.Log("SetMimicPoint: Point is a Place Point or Pick Point.");
                            instantiateObjects.MimicHumanObjects.SetActive(true);
                            instantiateObjects.MimicRobotObjects.SetActive(true);
                            string currentGoalName = instantiateObjects.MimicMirroredGeometryManagerImplementation.CurrentMimicGoalName;
                            instantiateObjects.FindHumanandRobotTargetInformation(pickOrPlaceItemName, instantiateObjects.TrackedGeometriesParentObject, instantiateObjects.MimicGoalsManager.CurrentGoal.GoalGameObject, instantiateObjects.MimicMirroredGeometryManagerImplementation.TrackedGeometriesParent, instantiateObjects.MimicMirroredGeometryManagerImplementation.GoalsParent.FindObject(currentGoalName));
                            StartCoroutine(HelpersExtensions.FlashOnScreenObjectRoutine(UserInitiatedMimicSetPointGreenScreen, MimicSetandUndoFlashDuration));
                        }
                        else
                        {
                            //Set Lines active and Points active
                            Debug.Log("SetMimicPoint: Point is a Normal Point and will be set as a trajectory goal point.");
                            instantiateObjects.MimicHumanObjects.SetActive(true);
                            instantiateObjects.MimicRobotObjects.SetActive(true);
                            instantiateObjects.CreateMimicPoints(humanZoneObject, robotZoneObject, ref instantiateObjects.MimicHumanPoints, ref instantiateObjects.MimicRobotPoints, instantiateObjects.MimicHumanLine, instantiateObjects.MimicRobotLine, instantiateObjects.MimicHumanPointsParent, instantiateObjects.MimicRobotPointsParent, UserInitiatedMimicMirrorToggle.isOn);
                            StartCoroutine(HelpersExtensions.FlashOnScreenObjectRoutine(UserInitiatedMimicSetPointGreenScreen, MimicSetandUndoFlashDuration));
                        }

                    }
                    else
                    {
                        Debug.Log("SetMimicPoint: Camera Position is not within the Human Zone Object.");
                        string message = "WARNING: This Point cannot be set because it is not within the human editing zone.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenInfoMessagePrefab, ref MimicSetPointOutsideOfHumanZone, "MimicPointOutsideOfBounds", MessagesParent, message, "RequestTrajectoryButtonMethod: Transaction Lock Active Warning.");

                    }
                }
                else
                {
                    Debug.LogError("SetMimicPoint: 'robot_zone' key not found in MimicZones.");
                }
            }
            else
            {
                Debug.LogError("SetMimicPoint: 'human_zone' key not found in MimicZones.");
            }
        }
        public void UndoUserInitiatedMimicPointButtonMethod()
        {
            /*
            * Method is used to undo the last set mimic point.
            */
            if (USERINSTIANTEDTRAJECTORYEXECUTED)
            {
                Debug.LogWarning("SetMimicPoint: User has already executed a trajectory, cannot set more points.");
                instantiateObjects.DestroyUserInstatiatedMimicZoneObjects();
                //TODO: Needs to reset the PickandPlaceStateManager

                USERINSTIANTEDTRAJECTORYEXECUTED = false;
                if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                return;
            }

            Debug.Log("UndoMimicPoint: Undoing Last Mimic Point.");
            if(instantiateObjects.MimicHumanPoints.Count > 0 && instantiateObjects.MimicRobotPoints.Count > 0)
            {
                if (instantiateObjects.MimicHumanPoints.Count != instantiateObjects.MimicRobotPoints.Count)
                {
                    Debug.LogError("UndoMimicPoint: Mimic Human Points and Mimic Robot Points count do not match.");
                    return;
                }

                if (instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPicked || instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPlaced)
                {
                    if(instantiateObjects.MimicHumanPoints.Count - 1 == instantiateObjects.userIniatedMimicPickandPlaceManager.PickPointTrajectoryIndex)
                    {
                        Debug.Log("UndoMimicPoint: Last Point is a Pick Point, resetting Pick State.");
                        instantiateObjects.userIniatedMimicPickandPlaceManager.ResetPickObjects();
                        //TODO: DO NOT DESTROY JUST REMOVE...
                        instantiateObjects.DestroyOrRemoveLastMimicPoint(ref instantiateObjects.MimicHumanPoints, ref instantiateObjects.MimicRobotPoints, ref instantiateObjects.MimicHumanLine, ref instantiateObjects.MimicRobotLine, false);
                        StartCoroutine(HelpersExtensions.FlashOnScreenObjectRoutine(UserInitiatedMimicUndoPointRedScreen, MimicSetandUndoFlashDuration));
                        return;
                    }
                    else if (instantiateObjects.MimicHumanPoints.Count - 1 == instantiateObjects.userIniatedMimicPickandPlaceManager.PlacePointTrajectoryIndex)
                    {
                        Debug.Log("UndoMimicPoint: Last Point is a Place Point, resetting Place State.");
                        //TODO: DO NOT DESTROY JUST REMOVE...
                        instantiateObjects.userIniatedMimicPickandPlaceManager.ResetPlaceObjects();
                        instantiateObjects.DestroyOrRemoveLastMimicPoint(ref instantiateObjects.MimicHumanPoints, ref instantiateObjects.MimicRobotPoints, ref instantiateObjects.MimicHumanLine, ref instantiateObjects.MimicRobotLine, false);
                        StartCoroutine(HelpersExtensions.FlashOnScreenObjectRoutine(UserInitiatedMimicUndoPointRedScreen, MimicSetandUndoFlashDuration));
                        return;
                    }
                    else
                    {
                        Debug.Log("UndoMimicPoint: Last Point is not a Pick or Place Point, no need to reset Pick and Place State.");
                    }
                }

                instantiateObjects.DestroyOrRemoveLastMimicPoint(ref instantiateObjects.MimicHumanPoints, ref instantiateObjects.MimicRobotPoints, ref instantiateObjects.MimicHumanLine, ref instantiateObjects.MimicRobotLine);
                StartCoroutine(HelpersExtensions.FlashOnScreenObjectRoutine(UserInitiatedMimicUndoPointRedScreen, MimicSetandUndoFlashDuration));
            }
            else
            {
                Debug.LogWarning("UndoMimicPoint: No Mimic Points to Undo.");
            }
        }
        public void ReachabilityToggleMethod(Toggle toggle)
        {
            /*
            ReachabilityToggleMethod is called from the UI Toggle and is responsible for toggling the reachability of the active robot in the scene.
            */
            bool visibility = toggle.GetComponent<Toggle>().isOn;
            if(trajectoryVisualizer.ActiveRobot != null)
            {
                trajectoryVisualizer.SetReachabilityActive(trajectoryVisualizer.ActiveRobot.transform.GetChild(0).gameObject, visibility);
                print("ReachabilityToggleMethod: Reachability is set to " + visibility);
            }
            else
            {
                Debug.Log("ReachabilityToggleMethod: ActiveRobot is null.");
            }
        }
        public void ShowInferedGeometriesInSceeneWrapper(string inferedGoal, List<string> completedTargets, string suggestedTarget)
        {
            /*
            * Method is used to show the infered geometries in the scene.
            */
            if (inferedGoal != null && completedTargets != null && suggestedTarget != null)
            {
                Debug.Log($"ShowInferedGeometriesInSceene: Showing Infered Geometries {inferedGoal} with suggested target {suggestedTarget} and completed goals {JsonConvert.SerializeObject(completedTargets)} in Scene.");
                instantiateObjects.ShowInferedGeometriesInSceene(inferedGoal, completedTargets, suggestedTarget, instantiateObjects.InferenceSuggestedTargetMaterial, instantiateObjects.InferenceInferedGoalMaterial, instantiateObjects.InferenceCompletedItemsMaterial, ref instantiateObjects.InferenceGoalsParentObject);
            }
            else
            {
                Debug.LogWarning("ShowInferedGeometriesInSceene: Infered Goal, Completed Targets or Suggested Target is null.");
            }
        }

        //TODO: Realtime Mimic Execute Pick and Execute Place Onscreen Message Methods
        public void SetRealtimeMimicPickAndPlaceOnscreenMessageOnStart()
        {
            //TODO: Setup message structures and button methods for realtime mimic pick and place.
            RealtimeMimicAcceptPickMessage = MessagesParent.FindObject("Prefabs").FindObject("RealtimeMimicPickObjectMessage");
            Button YesPickButton = RealtimeMimicAcceptPickMessage.FindObject("YesButton").GetComponent<Button>();
            Button NoPickButton = RealtimeMimicAcceptPickMessage.FindObject("NoButton").GetComponent<Button>();
            YesPickButton.GetComponent<Button>().onClick.AddListener(RealtimeMimicPublishAcceptPickButtonMethod);
            NoPickButton.GetComponent<Button>().onClick.AddListener(RealtimeMimicDeclinePickButtonMethod);

            RealtimeMimicAcceptPlaceMessage = MessagesParent.FindObject("Prefabs").FindObject("RealtimeMimicPlaceObjectMessage");
            Button YesPlaceButton = RealtimeMimicAcceptPlaceMessage.FindObject("YesButton").GetComponent<Button>();
            Button NoPlaceButton = RealtimeMimicAcceptPlaceMessage.FindObject("NoButton").GetComponent<Button>();
            YesPlaceButton.GetComponent<Button>().onClick.AddListener(RealtimeMimicPublishAcceptPlaceButtonMethod);
            NoPlaceButton.GetComponent<Button>().onClick.AddListener(RealtimeMimicDeclinePlaceButtonMethod);
        }
        public void RealtimeMimicPublishAcceptPickButtonMethod()
        {
            RealtimeMimicRequestMessage realtimeMimicPickRequestMessage = mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish;
            if (realtimeMimicPickRequestMessage == null)
            {
                Debug.LogError("RealtimeMimicPublishAcceptPickButtonMethod: Realtime Mimic PICK Message to Publish is null, cannot publish.");
                return;
            }
            REALTIMEMIMICLASTSENTINDEX = realtimeMimicPickRequestMessage.PointIndex;
            Debug.Log($"RealtimeMimicPublishAcceptPickButtonMethod: User ACCEPTED PICK Point, Publishing MSG : {realtimeMimicPickRequestMessage.GetData()} to Topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic}.");
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic, realtimeMimicPickRequestMessage.GetData());
            RealtimeMimicAcceptPickMessage.SetActive(false);
        }
        public void RealtimeMimicDeclinePickButtonMethod()
        {
            Debug.Log("RealtimeMimicPublishAcceptPickButtonMethod: User DECLINED Pick Point, Publishing Pick Message.");
            ControlFollowMeButtonInteractability(true);
            if (mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish != null)
            {
                mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish = null;
            }
            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.activeSelf)
            {
                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.SetActive(false);
            }
            instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN = null;
            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.activeSelf)
            {
                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.SetActive(false);
            }
            instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT = null;
            instantiateObjects.DestroyRealtimeMimicZoneObjects();
            RealtimeMimicAcceptPickMessage.SetActive(false);
        }
        public void RealtimeMimicResetSceneFromPlanningFailure()
        {
            Debug.Log("RealtimeMimicResetSceneFromPlanningFailure: Reset scene from planning failure (hiding objects and resetting everything).");
            ControlFollowMeButtonInteractability(true);
            if (mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish != null)
            {
                mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish = null;
            }
            if(mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish != null)
            {
                mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish = null;
            }
            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.activeSelf)
            {
                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.SetActive(false);
            }
            instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN = null;
            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.activeSelf)
            {
                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.SetActive(false);
            }
            instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT = null;
            instantiateObjects.DestroyRealtimeMimicZoneObjects();
        }
        public void RealtimeMimicResetSceneFromPlanningSuccess()
        {
            Debug.Log("RealtimeMimicPublishAcceptPickButtonMethod: User DECLINED Pick Point, Publishing Pick Message.");
            ControlFollowMeButtonInteractability(true);
            if (mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish != null)
            {
                mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish = null;
            }
            if(mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish != null)
            {
                mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish = null;
            }
            instantiateObjects.DestroyRealtimeMimicZoneObjects();
        }
        public void RealtimeMimicPublishAcceptPlaceButtonMethod()
        {
            RealtimeMimicRequestMessage realtimeMimicPlaceRequestMessage = mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish;
            if (realtimeMimicPlaceRequestMessage == null)
            {
                Debug.LogError("RealtimeMimicPublishAcceptPlaceButtonMethod: Realtime Mimic PICK Message to Publish is null, cannot publish.");
                return;
            }
            REALTIMEMIMICLASTSENTINDEX = realtimeMimicPlaceRequestMessage.PointIndex;
            Debug.Log($"RealtimeMimicPublishAcceptPlaceButtonMethod: User ACCEPTED PLACE Point, Publishing MSG : {realtimeMimicPlaceRequestMessage.GetData()} to Topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic}.");
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.realtimeMimicRequestTopic, realtimeMimicPlaceRequestMessage.GetData());
            RealtimeMimicAcceptPlaceMessage.SetActive(false);
        }
        public void RealtimeMimicDeclinePlaceButtonMethod()
        {
            Debug.Log("RealtimeMimicDeclinePlaceButtonMethod: User DECLINED Place Point, Publishing Place Message.");
            ControlFollowMeButtonInteractability(true);
            if (mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish != null)
            {
                mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish = null;
            }
            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.activeSelf)
            {
                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN.SetActive(false);
            }
            instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONHUMAN = null;
            if (instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.activeSelf)
            {
                instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT.SetActive(false);
            }
            instantiateObjects.REALTIMEMIMICPICKORPLACEINFORMATIONROBOT = null;
            instantiateObjects.DestroyRealtimeMimicZoneObjects();
            RealtimeMimicAcceptPlaceMessage.SetActive(false);
        }
        public void RealtimeMimicSignalOnscreenMessageForPick()
        {
            if (mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PickMessageToPublish != null)
            {
                Debug.Log("SignalOnscreenMessageForPick: Signaling Onscreen Message for Pick Point.");

                ControlFollowMeButtonInteractability(false);

                if (RealtimeMimicEditorTestToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    RealtimeMimicEditorTestToggleObject.GetComponentInChildren<Toggle>().isOn = false;
                }
                RealtimeMimicAcceptPickMessage.SetActive(true);
            }
            else
            {
                Debug.LogWarning("SignalOnscreenMessageForPick: No Pick Message to Publish.");
                string message = "WARNING: A pick was requested, however there is nothing to publish to the planner please try this again.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref RealTimeMimicPickRequestedWithNoMessage, "RealtimeMimicPickRequestedWithNoMessage", MessagesParent, message, "RealtimeMimicSignalOnscreenMessageForPick: Realtime Mimic Pick was requested with no message.");
            }
        }
        public void RealtimeMimicSignalOnscreenMessageForPlace()
        {
            if (mqttTrajectoryManager.realtimeMimicPickandPlaceManager.PlaceMessageToPublish != null)
            {
                Debug.Log("RealtimeMimicSignalOnscreenMessageForPlace: Signaling Onscreen Message for Pick Point.");
                ControlFollowMeButtonInteractability(false);
                if (RealtimeMimicEditorTestToggleObject.GetComponentInChildren<Toggle>().isOn)
                {
                    RealtimeMimicEditorTestToggleObject.GetComponentInChildren<Toggle>().isOn = false;
                }
                RealtimeMimicAcceptPlaceMessage.SetActive(true);
            }
            else
            {
                Debug.LogWarning("RealtimeMimicSignalOnscreenMessageForPlace: No Pick Message to Publish.");
                string message = "WARNING: A place was requested, however there is nothing to publish to the planner please try this again.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref RealTimeMimicPlaceRequestedWithNoMessage, "RealtimeMimicPlaceRequestedWithNoMessage", MessagesParent, message, "RealtimeMimicSignalOnscreenMessageForPlace: Realtime Mimic Pick was requested with no message.");
            }
        }
        public void ControlFollowMeButtonInteractability(bool interactable)
        {
            /*
            * Method is used to control the interactability of the Follow Me Button.
            */
            if (!interactable)
            {
                FollowMeButton.GetComponentInChildren<Button>().interactable = false;
                FollowMeButtonHeldEventComponent.isHeld = false;
                FollowMeButtonHeldEventComponent.ignore = true; //TODO: Make this false when the message replys or the informatoin is cleared.
            }
            else
            {
                FollowMeButtonHeldEventComponent.ignore = false;
                FollowMeButton.GetComponentInChildren<Button>().interactable = true;
            }
        }

        //UI Control Methods //TODO: I think that all of the updated methods for Mimic are working, but needs to be tested.
        public void SetUIObjectsFromCurrentMode(ProjectZones.CurrentZoneMode mode)
        {
            /*
            * Method is used to set the UI objects based on the current mode.
            */
            switch (mode)
            {
                case ProjectZones.CurrentZoneMode.None:
                    Debug.Log("SetUIObjectsFromCurrentMode: Setting UI Objects for None Mode.");
                    SetUserInitiatedMimicControlsActivity(false, false, false, false, false);
                    SetRealtimeMimicControlsActivity(false, false);

                    SetInferenceUIPostInferenceSuccesState(false, false, false, false, false);
                    SetInferenceRequestUIControlsVisibilityandInteractibility(false, false, false, false, false);

                    MimicControlsParent.gameObject.SetActive(false);
                    RoboticTerritoriesInferenceControlsObject.SetActive(false);

                    break;

                case ProjectZones.CurrentZoneMode.Inference:
                    Debug.Log("SetUIObjectsFromCurrentMode: Setting UI Objects for Inference Mode.");
                    MimicControlsParent.gameObject.SetActive(false);
                    SetUserInitiatedMimicControlsActivity(false, false, false, false, false);
                    SetRealtimeMimicControlsActivity(false, false);

                    RoboticTerritoriesInferenceControlsObject.SetActive(true);
                    SetInferanceUIBasedOnInferenceState(GOALINFERRED);
                    break;

                case ProjectZones.CurrentZoneMode.Mimic:
                    MimicControlsParent.gameObject.SetActive(true);
                    SetMimicControlsBasedOnCurrentMimicMode(databaseManager.ProjectZones.CurrentMimicMode);

                    //Reset Inference Controls
                    RoboticTerritoriesInferenceControlsObject.SetActive(false);
                    SetInferenceUIPostInferenceSuccesState(false, false, false, false, false);
                    SetInferenceRequestUIControlsVisibilityandInteractibility(false, false, false, false, false);

                    Debug.Log("SetUIObjectsFromCurrentMode: Setting Active Controls for Mimic Mode.");
                    break;

                default:
                    SetUserInitiatedMimicControlsActivity(false, false, false, false, false);

                    //Reset Inference Controls
                    SetInferenceUIPostInferenceSuccesState(false, false, false, false, false);
                    SetInferenceRequestUIControlsVisibilityandInteractibility(false, false, false, false, false);

                    //Set both parents to false.
                    MimicControlsParent.gameObject.SetActive(false);
                    RoboticTerritoriesInferenceControlsObject.SetActive(true);

                    Debug.LogWarning("SetUIObjectsFromCurrentMode: Current Zone Mode is not set.");
                    break;
            }
        }
        public void SetUserInitiatedMimicControlsActivity(bool setControlsActive, bool setControlsInteractive, bool requestInteractable, bool reviewActive, bool reviewInteractive)
        {
            /*
            * Method is used to set the Mimic Controls activity based on the input.
            */
            UserInitiatedMimicControlsSetPointsUIObjects.SetActive(setControlsActive);
            MimicSetPointsButtonObject.GetComponentInChildren<Button>().interactable = setControlsInteractive;
            UserInitiatedMimicControlsUndoPointButtonObject.GetComponentInChildren<Button>().interactable = setControlsInteractive;
            UserInitiatedMimicRequestTrajectoryButtonObject.GetComponentInChildren<Button>().interactable = requestInteractable;
            // UserInitiatedMimicMirrorToggleObject.GetComponentInChildren<Toggle>().interactable = setControlsInteractive;

            UserInitiatedMimicControlsReviewAndExecuteTrajectoryUIObjects.SetActive(reviewActive);
            UserInitiatedMimicExecuteTrajectoryButtonObject.GetComponentInChildren<Button>().interactable = reviewInteractive;
            UserInitiatedMimicTrajectoryReviewSliderObject.GetComponentInChildren<Slider>().interactable = reviewInteractive;
        }
        public void SetMimicControlsBasedOnCurrentMimicMode(ProjectZones.MimicZoneMode mode)
        {
            /*
            * Method is used to set the Mimic Controls based on the current mimic mode.
            */
            Debug.Log($"SetMimicControlsBasedOnCurrentMimicMode: Setting Mimic Controls based on the current mimic mode {mode}");
            switch (mode)
            {
                case ProjectZones.MimicZoneMode.UserInitiated:
                    SetUserInitiatedMimicControlsActivity(true, true, true, false, false);
                    SetRealtimeMimicControlsActivity(false, false);
                    AdaptObjectCollidersForMimicMode(ProjectZones.MimicZoneMode.UserInitiated, instantiateObjects.MimicGoalsManager, instantiateObjects.MimicMirroredGeometryManagerImplementation);
                    //TODO: Line Testing
                    if (instantiateObjects.MimicHumanLine != null)
                    {
                        instantiateObjects.MimicHumanLine.gameObject.SetActive(true);
                    }
                    if (instantiateObjects.MimicRobotLine != null)
                    {
                        instantiateObjects.MimicRobotLine.gameObject.SetActive(true);
                    }
                    if(instantiateObjects.RealtimeMimicHumanLine != null)
                    {
                        instantiateObjects.RealtimeMimicHumanLine.gameObject.SetActive(false);
                    }
                    if(instantiateObjects.RealtimeMimicRobotLine != null)
                    {
                        instantiateObjects.RealtimeMimicRobotLine.gameObject.SetActive(false);
                    }
                    //TODO: End Line Testing

                    Debug.Log("SetMimicControlsBasedOnCurrentMimicMode: Setting UI Objects for None Mode.");
                    break;
                case ProjectZones.MimicZoneMode.RealtimeMimic:
                    SetUserInitiatedMimicControlsActivity(false, false, false, false, false);
                    SetRealtimeMimicControlsActivity(true, true);
                    AdaptObjectCollidersForMimicMode(ProjectZones.MimicZoneMode.RealtimeMimic, instantiateObjects.MimicGoalsManager, instantiateObjects.MimicMirroredGeometryManagerImplementation);

                    //TODO: RealtimeMimic Pick and Place Management Testing
                    mqttTrajectoryManager.realtimeMimicPickandPlaceManager.Reset();

                    //TODO: Line Testing
                    if (instantiateObjects.MimicHumanLine != null)
                    {
                        instantiateObjects.MimicHumanLine.gameObject.SetActive(false);
                    }
                    if (instantiateObjects.MimicRobotLine != null)
                    {
                        instantiateObjects.MimicRobotLine.gameObject.SetActive(false);
                    }
                    if(instantiateObjects.RealtimeMimicHumanLine != null)
                    {
                        instantiateObjects.RealtimeMimicHumanLine.gameObject.SetActive(true);
                    }
                    if(instantiateObjects.RealtimeMimicRobotLine != null)
                    {
                        instantiateObjects.RealtimeMimicRobotLine.gameObject.SetActive(true);
                    }
                    //TODO: End Line Testing

                    Debug.Log("SetMimicControlsBasedOnCurrentMimicMode: Setting UI Objects for SetPoints Mode.");
                    break;
                default:
                    AdaptObjectCollidersForMimicMode(ProjectZones.MimicZoneMode.UserInitiated, instantiateObjects.MimicGoalsManager, instantiateObjects.MimicMirroredGeometryManagerImplementation);
                    SetUserInitiatedMimicControlsActivity(false, false, false, false, false);
                    SetRealtimeMimicControlsActivity(false, false);
                    Debug.LogWarning("SetMimicControlsBasedOnCurrentMimicMode: Current Mimic Mode is not set.");
                    break;
            }
        }
        public void SetRealtimeMimicControlsActivity(bool setControlsActive, bool setControlsInteractive)
        {
            /*
            * Method is used to set the Realtime Mimic Controls activity based on the input.
            */
            if (RealtimeMimicControlsParent == null)
            {
                Debug.LogError("SetRealtimeMimicControlsActivity: RealtimeMimicControlsParent is null.");
                return;
            }
            if (RealtimeMimicControlsParent != null)
            {
                RealtimeMimicControlsParent.SetActive(setControlsActive);
            }

            if (RealtimeMimicControlsParent.transform.childCount == 0)
            {
                Debug.LogWarning("SetRealtimeMimicControlsActivity: RealtimeMimicControlsParent has no children.");
                return;
            }
            else
            {
                //TODO: JOEEEEE CHEEECCCCCKKKKKK HEREEEEEEE..... 
                // foreach (Transform child in RealtimeMimicControlsParent.transform)
                // {
                //     var go = child.gameObject;

                //     // Show/hide
                //     go.SetActive(setControlsActive);

                //     // If it's a Button
                //     if (go.TryGetComponent<Button>(out var button))
                //         button.interactable = setControlsInteractive;

                //     // If it's a Toggle
                //     if (go.TryGetComponent<Toggle>(out var toggle))
                //         toggle.interactable = setControlsInteractive;
                // }
            }
        }
        public void UserInitiatedMimicRequestTrajectoryButtonMethod()
        {
            Debug.Log($"MimicRequestTrajectoryButtonMethod: Requesting Trajectory for {instantiateObjects.MimicHumanPoints.Count} points.");

            if (USERINSTIANTEDTRAJECTORYEXECUTED)
            {
                Debug.LogWarning("SetMimicPoint: User has already executed a trajectory, cannot set more points.");
                instantiateObjects.DestroyUserInstatiatedMimicZoneObjects();
                //TODO: Needs to reset the PickandPlaceStateManager : JOEEEEE I THINK THIS NEEDS TO BE DONE....

                USERINSTIANTEDTRAJECTORYEXECUTED = false;
                if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryandShowRobot();
                }
                return;
            }
            if (instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPicked || instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPlaced)
            {
                if (instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPicked && !instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPlaced)
                {
                    Debug.Log("MimicRequestTrajectoryButton: There is not enough mimic points to request a trajectory");
                    string message = "WARNING: You need at least 2 points to request a trajectory for mimicry.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MimicObjectPickedButNotPlaced, "MimicObjectPickedButNotPlaced", MessagesParent, message, "MimicRequestTrajectoryButtonMethod: An object has been specified to be picked, but not to be placed.");
                    return;
                }
                else if (!instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPicked && instantiateObjects.userIniatedMimicPickandPlaceManager.ObjectPlaced)
                {
                    Debug.Log("MimicRequestTrajectoryButton: There is a no pick but a place which makes no sense, You need to request with both a pick and a place.");
                    string message = "WARNING: There is a place but not a pick, you are unable to request without a pick and a place specified.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MimicObjectPlacedButNotPicked, "MimicObjectPlacedButNotPicked", MessagesParent, message, "MimicRequestTrajectoryButtonMethod: An object has been specified to be placed, but not to be picked.");
                    return;
                }
                else
                {
                    Debug.Log("MimicRequestTrajectoryButton: There is a pick and a place specified, proceeding with trajectory request.");
                }
            }
            if (instantiateObjects.MimicHumanPoints.Count < 2)
            {
                Debug.Log("MimicRequestTrajectoryButton: There is not enough mimic points to request a trajectory");
                string message = "WARNING: You need at least 2 points to request a trajectory for mimicry.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MimicPointsTooFewMessage, "MimicPointsTooFewMessage", MessagesParent, message, "MimicRequestTrajectoryButtonMethod: There is no enough mimic points.");
                return;
            }
            else if (trajectoryVisualizer.ActiveRobot == null)
            {
                Debug.Log("MimicRequestTrajectoryButton: Active Robot is null");
                string message = "WARNING: Active Robot is currently null. An active robot must be set before visulizing robotic information.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref ActiveRobotIsNullWarningMessageObject, "ActiveRobotNullWarningMessage", MessagesParent, message, "MimicRequestTrajectoryButtonMethod: Active Robot is null.");
                return;
            }
            //TODO: This was the old one : else if (!ObjectInstantiaion.AllGameObjectsInListsPositionsAreWithinAnotherObject(instantiateObjects.MimicHumanPoints, trajectoryVisualizer.humanZoneMimicReachibility))
            else if (!ObjectInstantiaion.AllGameObjectsInsideSphereButIgnoreIndexes(instantiateObjects.MimicHumanPoints, trajectoryVisualizer.humanZoneMimicReachibility, new List<int> { instantiateObjects.userIniatedMimicPickandPlaceManager.PickPointTrajectoryIndex, instantiateObjects.userIniatedMimicPickandPlaceManager.PlacePointTrajectoryIndex })) //TODO: CHANGED THIS
            {

                var mimicZones = databaseManager.ProjectZones.MimicZones;

                if (mimicZones.TryGetValue("human_zone", out Zone humanZone))
                {
                    if (mimicZones.TryGetValue("robot_zone", out Zone robotZone))
                    {
                        GameObject humanZoneObject = humanZone.ZoneObject;
                        GameObject robotZoneObject = robotZone.ZoneObject;
                        instantiateObjects.CreateSystemProposalPoints(humanZoneObject, robotZoneObject, trajectoryVisualizer.humanZoneMimicReachibility, ref instantiateObjects.MimicHumanPoints,
                        ref instantiateObjects.MimicHumanSystemProposedPoints, ref instantiateObjects.MimicRobotSystemProposedPoints,
                        instantiateObjects.MimicSystemProposedLineHuman, instantiateObjects.MimicHumanSystemProposedPointsParent, instantiateObjects.MimicSystemProposedLineRobot,
                        instantiateObjects.MimicRobotSystemProposedPointsParent, UserInitiatedMimicMirrorToggleObject.GetComponentInChildren<Toggle>().isOn);
                    }
                    else
                    {
                        Debug.LogError("MimicRequestTrajectoryButton: 'robot_zone' key not found in MimicZones.");
                    }
                }
                else
                {
                    Debug.LogError("MimicRequestTrajectoryButton: 'human_zone' key not found in MimicZones.");
                }
            }
            else if (!instantiateObjects.PickAndPlaceTargetsAreWithinReachability(instantiateObjects.userIniatedMimicPickandPlaceManager.PickPointTrajectoryIndex, instantiateObjects.userIniatedMimicPickandPlaceManager.PlacePointTrajectoryIndex, trajectoryVisualizer.humanZoneMimicReachibility))
            {
                Debug.Log("MimicRequestTrajectoryButton: Pick and Place Points are not within reachability of the active robot.");
                string message = "WARNING: The pick and/or place points are not within the reachability of the active robot. Please adjust the points or the robot.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MimicPickAndPlaceNotInReachabilityMessage, "MimicPickAndPlaceNotInReachabilityMessage", MessagesParent, message, "MimicRequestTrajectoryButtonMethod: Pick and/or Place Points are not within reachability of the active robot.");
                return;
            }
            else
            {
                List<Frame> humanFrames = ObjectTransformations.ConvertGameObjectListToRightHandFrameDataRoboticTerritories(instantiateObjects.MimicHumanPoints, instantiateObjects.ZonesARPrefabObjects);
                List<Frame> robotFrames = ObjectTransformations.ConvertGameObjectListToRightHandFrameDataRoboticTerritories(instantiateObjects.MimicRobotPoints, instantiateObjects.ZonesARPrefabObjects);
                List<int> ioControlIndexes = instantiateObjects.userIniatedMimicPickandPlaceManager.CreateIOControlIndeciesFromMimicPointCount(instantiateObjects.MimicHumanPoints.Count);

                if (humanFrames == null || robotFrames == null || ioControlIndexes == null)
                {
                    Debug.LogError("MimicRequestTrajectoryButton: Human or Robot Frames or IO Control Indexes are null.");
                    return;
                }
                if (instantiateObjects.userIniatedMimicPickandPlaceManager.ReverseConfigurations)
                {
                    humanFrames.Reverse();
                    robotFrames.Reverse();
                    ioControlIndexes.Reverse();
                    Debug.Log("MimicRequestTrajectoryButton: Reversing Configurations as specified by the user.");
                }

                if (humanFrames.Count != robotFrames.Count || humanFrames.Count != ioControlIndexes.Count)
                {
                    Debug.LogError("MimicRequestTrajectoryButton: Human and Robot Frames count do not match or IO Control Index Count Does not match.");
                    return;
                }

                MimicTrajectoryRequestMessage requestMessage = new MimicTrajectoryRequestMessage(humanFrames, robotFrames, mqttTrajectoryManager.serviceManager.ActiveRobotName, ioControlIndexes); //TODO: ROBOT NAME NEEDS TO BE CHANGED FOR SURE...
                Debug.Log($"MimicRequestTrajectoryButton: Publishing Mimic request {JsonConvert.SerializeObject(requestMessage.GetData())} on topic {mqttTrajectoryManager.roboticTerritoriesTopics.publishers.mimicRequestTopic}");

                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.mimicRequestTopic, requestMessage.GetData());
                SetUserInitiatedMimicControlsActivity(true, false, false, true, false);
            }
        }
        public void SignalActiveRobotUpdateFromPlannerRoboticTerritories(string robotName, string activeRobotName, Action visualizeRobotMethod)
        {
            /*
            * Method is used to signal an active robot update from another user on message request.
            * This method is used for a custom active robot update because it has more requirements then message requests.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.Log($"SignalActiveRobotUpdateFromPlannerRoboticTerrirories: Updating Active Robot from dropdown list .");
            TMP_Text messageComponent = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();
            string message = $"WARNING: You requested for {activeRobotName} but reply Trajectory is for {robotName}. ACTIVE ROBOT UPDATED.";
            int robotSelection = RobotSelectionDropdown.options.FindIndex(option => option.text == robotName);

            if(robotSelection != -1)
            {            
                if(SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                {
                    SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
                }
                RobotSelectionDropdown.value = robotSelection;
                SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = true;
            }
            else
            {
                Debug.LogError("SignalActiveRobotUpdateFromPlanner: Could not find robot in dropdown options.");
            }

            if(messageComponent != null && message != null && ActiveRobotUpdatedFromPlannerMessageObject != null)
            {
                UserInterface.SignalOnScreenMessageWithButton(ActiveRobotUpdatedFromPlannerMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Could not find message object or message component.");
            }

            GameObject AcknowledgeButton = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("AcknowledgeButton");
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => SetUserInitiatedMimicControlsActivity(true, false, true, true, true));
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Something Is messed up with on click event listner.");
            }

        }
        public void MimicTrajectorySliderReviewMethod(float value)
        {
            if (mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints != null)
            {
                if (mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints.Count > 0)
                {
                    float SliderValue = value;
                    int TrajectoryConfigurationsCount = mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints.Count;
                    float SliderMax = 1;
                    float SliderMin = 0;
                    float SliderValueRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0, TrajectoryConfigurationsCount-1); 
                    Debug.Log($"MimicTrajectorySliderReviewMethod: Slider Value Changed is value {value} and the item is {JsonConvert.SerializeObject(mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints[(int)SliderValueRemaped])}");
                    trajectoryVisualizer.ColorRobotConfigfromSliderInput((int)SliderValueRemaped, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial,ref trajectoryVisualizer.previousTrajectoryReviewSliderValue);
                }
                else
                {
                    Debug.Log("MimicTrajectorySliderReviewMethod: Current Trajectory Count is 0.");
                }
            }
            else
            {
                Debug.Log("MimicTrajectorySliderReviewMethod: Current Trajectory is null.");
            }
        }
        public void UserInitiatedMimicTrajectorySliderReviewCompoundTrajectories(float value)
        {
            if (mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.Trajectories != null)
            {
                if (mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.Trajectories.Count > 0)
                {

                    List<Trajectory> trajectories = mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.Trajectories;
                    List<(int start, int end)> trajectoryRanges = new List<(int, int)>();
                    int configCount = 0;

                    foreach (Trajectory trajectory in trajectories)
                    {
                        int count = trajectory.Points.Count;
                        trajectoryRanges.Add((configCount, configCount + count - 1));
                        configCount += count;
                    }

                    float SliderValue = value;
                    float SliderMin = 0f;
                    float SliderMax = 1f;
                    int targetGlobalIndex = Mathf.RoundToInt(
                        HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0f, configCount - 1)
                    );

                    int selectedTrajectoryIndex = -1;
                    int localIndex = -1;

                    for (int i = 0; i < trajectoryRanges.Count; i++)
                    {
                        var (start, end) = trajectoryRanges[i];
                        if (targetGlobalIndex >= start && targetGlobalIndex <= end)
                        {
                            selectedTrajectoryIndex = i;
                            localIndex = targetGlobalIndex - start;
                            break;
                        }
                    }

                    if (selectedTrajectoryIndex >= 0 && localIndex >= 0)
                    {
                        Debug.Log($"Slider = {SliderValue:0.000} → Global Config #{targetGlobalIndex}");
                        Debug.Log($"Belongs to Trajectory #{selectedTrajectoryIndex}, Local Config #{localIndex}");
                        trajectoryVisualizer.ColorRobotConfigfromSliderInputCompoundTrajectories(selectedTrajectoryIndex, localIndex, trajectories, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial, ref trajectoryVisualizer.previousConfigIndex, ref trajectoryVisualizer.previousTrajectoryIndex);

                    }
                    else
                    {
                        Debug.LogWarning("Could not map slider to trajectory index.");
                    }
                }
            }
            else
            {
                Debug.Log("MimicTrajectorySliderReviewMethod: Current Trajectory is null.");
            }
        }
        public void UserInitiatedMimicExecuteTrajectoryButtonMethod()
        {
            Debug.Log("MimicExecuteTrajectoryButton: Executing Mimic Trajectory.");
            if (mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints == null
            || mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints.Count <= 0
            || mqttTrajectoryManager.serviceManager.ActiveRobotName != mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.RobotName)
            {
                Debug.Log("MimicExecuteTrajectoryButton: Current Trajectory is null or empty.");
                string message = "WARNING: There is no trajectory to execute or the active robot does not match the robot trajectory.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MimicUnabletoExecuteTrajectory, "MimicPointsTooFewMessage", MessagesParent, message, "MimicExecuteTrajectoryButton: No Trajectory to Execute.");
            }
            else
            {
                Debug.Log("MimicExecuteTrajectoryButton: Publising Mimic Exacution Message Mimic Trajectory.");
                ExacuteMimicTrajectoryRequestMessage exacuteMimicRequestMessage = new ExacuteMimicTrajectoryRequestMessage
                (
                    mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.Trajectories,
                    mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.CombinedTrajectoryPoints,
                    mqttTrajectoryManager.serviceManager.ActiveRobotName,
                    mqttTrajectoryManager.serviceManager.LastMimicTrajectoryResultMessage.RobotBaseFrame
                );
                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.roboticTerritoriesTopics.publishers.mimicExecuteTrajectoryRequestTopic, exacuteMimicRequestMessage.GetData());
                SetUserInitiatedMimicControlsActivity(true, true, true, false, false);
                USERINSTIANTEDTRAJECTORYEXECUTED = true;
            }
        }
        public void UserInitiatedMimicMirrorToggleMethod(bool value)
        {
            /*
            * Method is used to set the mimic mirror based on the toggle value.
            */
            Debug.Log($"MimicMirrorToggleMethod: Setting Mimic Mirror to {value}");
            OnMirrorToggleChanged(value);
        }
        private void OnMirrorToggleChanged(bool value) //TODO: Kind of a hack, but keeps the toggles in sync.
        {
            if (_syncingMirrorToggles) return;

            Debug.Log($"OnMirrorToggleChanged : Mirror toggles changed to {value}");

            _syncingMirrorToggles = true;
            if (UserInitiatedMimicMirrorToggle.isOn != value)
                UserInitiatedMimicMirrorToggle.isOn = value;
            if (RealtimeMimicMirrorToggle.isOn != value)
                RealtimeMimicMirrorToggle.isOn = value;
            _syncingMirrorToggles = false;


            if (ReachabilityToggleObject.GetComponentInChildren<Toggle>().isOn)
            {
                Debug.Log("Reachability Toggle is on.");
                trajectoryVisualizer.AddReachabilitlyToHumanZone(
                    trajectoryVisualizer.ActiveRobot.FindObject(mqttTrajectoryManager.serviceManager.ActiveRobotName),
                    databaseManager.ProjectZones.MimicZones["human_zone"].ZoneObject,
                    databaseManager.ProjectZones.MimicZones["robot_zone"].ZoneObject,
                    ReachabilityToggleObject.GetComponent<Toggle>().isOn);
            }
            else
            {
                Debug.LogWarning("Reachability Toggle is not on.");
            }
        }
        public void AdaptObjectCollidersForMimicMode(ProjectZones.MimicZoneMode mimicMode, GoalManager mimicGoalManager, MimicMirroredGeometryManager mimicMirroredGeometryManager)
        {
            List<GoalObject> allGoalObjects = mimicGoalManager.Goals;
            GameObject mirroredGoalsParent = mimicMirroredGeometryManager.GoalsParent;
            Dictionary<string, ObservedGeometry> currentObservedGeometryDict = databaseManager.observedGeometriesDict;
            Dictionary<string, ObservedGeometry> mimicMirroredObservedGeometries = mimicMirroredGeometryManager.MimicObservedGeometriesDict;

            if (allGoalObjects == null || allGoalObjects.Count == 0)
            {
                Debug.LogWarning("AdaptObjectCollidersForMimicMode: No Goal Objects found in Mimic Goal Manager.");
                return;
            }
            if (mirroredGoalsParent == null || mirroredGoalsParent.transform.childCount == 0)
            {
                Debug.LogWarning("AdaptObjectCollidersForMimicMode: No Mirrored Goals found in Mimic Mirrored Geometry Manager.");
                return;
            }
            if (currentObservedGeometryDict == null || currentObservedGeometryDict.Count == 0)
            {
                Debug.LogWarning("AdaptObjectCollidersForMimicMode: No Observed Geometries found in Database Manager.");
                return;
            }
            if (mimicMirroredObservedGeometries == null || mimicMirroredObservedGeometries.Count == 0)
            {
                Debug.LogWarning("AdaptObjectCollidersForMimicMode: No Mimic Mirrored Observed Geometries found in Mimic Mirrored Geometry Manager.");
                return;
            }

            switch (mimicMode)
            {
                case ProjectZones.MimicZoneMode.UserInitiated:
                    {
                        Debug.Log("AdaptObjectCollidersForMimicMode: Setting Colliders for User Initiated Mimic Mode.");
                        // float yScale = 0.3f;
                        // float yCenterValue = 0.0f;
                        float yScale = 1.0f;
                        float yCenterValue = 0.0f;
                        instantiateObjects.ModifyCollidersForMimicModes(allGoalObjects, mirroredGoalsParent, currentObservedGeometryDict, mimicMirroredObservedGeometries, yScale, yCenterValue);
                        break;
                    }

                case ProjectZones.MimicZoneMode.RealtimeMimic:
                    {
                        Debug.Log("AdaptObjectCollidersForMimicMode: Setting Colliders for Realtime Mimic Mode.");
                        float yScale = 2.33f;
                        float yCenterValue = 0.35f;
                        instantiateObjects.ModifyCollidersForMimicModes(allGoalObjects, mirroredGoalsParent, currentObservedGeometryDict, mimicMirroredObservedGeometries, yScale, yCenterValue);
                        break;
                    }

                default:
                    {
                        Debug.LogWarning("AdaptObjectCollidersForMimicMode: Current Mimic Mode is not set.");
                        float yScale = 1.0f;
                        float yCenterValue = 0.0f;
                        instantiateObjects.ModifyCollidersForMimicModes(allGoalObjects, mirroredGoalsParent, currentObservedGeometryDict, mimicMirroredObservedGeometries, yScale, yCenterValue);
                        break;
                    }
            }
        }

        //TODO: RoboticTerritories Testing ///////////////////////////////////////////////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            /*
            * OnAwakeInitilization : Method is used to initialize the UI elements and set up the 
            * UI Objects, Script Dependencies, & set up relationships on start.
            */

            //Find Other Scripts
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();
            rosConnectionManager = GameObject.Find("RosManager").GetComponent<RosConnectionManager>();
            scrollSearchManager = GameObject.Find("ScrollSearchManager").GetComponent<ScrollSearchManager>();

            //Find Global use GameObjects
            Elements = GameObject.Find("Elements");
            QRMarkers = GameObject.Find("QRMarkers");
            CanvasObject = GameObject.Find("Canvas").FindObject("CompasXR");
            UserObjects = GameObject.Find("ActiveUserObjects");

            //Find AR and system management items
            arCameraObject = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera");
            arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();
            rayManager = FindObjectOfType<ARRaycastManager>();
            currentOperatingSystem = OperatingSystemManager.GetCurrentOS();

            //Find Constant UI Pannel
            ConstantUIPanelObjects = CanvasObject.FindObject("ConstantUIPanel");

            //Set up UI Objects and buttons on start
            SetPrimaryUIItemsOnStart();
            SetVisualizerMenuItemsOnStart();
            SetMenuItemsOnStart();
            SetCommunicationItemsOnStart();
        }
        private void SetPrimaryUIItemsOnStart()
        {
            /*
            * SetPrimaryUIItemsOnStart : Method is used to set up the primary UI elements on start.
            * Primary UI elements constitute the UI elements that are constantly on the screen
            * & control basic fundimental functionalities of the application.
            */

            //Find OnScreen UI Objects
            UserInterface.FindButtonandSetOnClickAction(ConstantUIPanelObjects, ref NextGeometryButtonObject, "Next_Geometry", NextStepButton);
            UserInterface.FindButtonandSetOnClickAction(ConstantUIPanelObjects, ref PreviousGeometryButtonObject, "Previous_Geometry", PreviousStepButton);
            UserInterface.FindSliderandSetOnValueChangeAction(CanvasObject, ref PreviewGeometrySliderObject, ref PreviewGeometrySlider, "GeometrySlider", PreviewGeometrySliderSetVisibilty);
            IsBuiltPanelObjects = ConstantUIPanelObjects.FindObject("IsBuiltPanel"); 
            UserInterface.FindButtonandSetOnClickAction(IsBuiltPanelObjects, ref IsBuiltButtonObject, "IsBuiltButton", () => ModifyStepBuildStatus(CurrentStep));
            IsbuiltButtonImage = IsBuiltButtonObject.FindObject("Image");
            UserInterface.FindToggleandSetOnValueChangedAction(CanvasObject, ref MenuButtonObject, "Menu_Toggle", ToggleMenu);
            UserInterface.FindToggleandSetOnValueChangedAction(CanvasObject, ref VisibilityMenuObject, "Visibility_Editor", ToggleVisibilityMenu);

            //Find Text Objects
            CurrentStepTextObject = GameObject.Find("Current_Index_Text");
            CurrentStepText = CurrentStepTextObject.GetComponent<TMPro.TMP_Text>();
            GameObject LastBuiltIndexTextObject = GameObject.Find("LastBuiltElement_Text");
            LastBuiltIndexText = LastBuiltIndexTextObject.GetComponent<TMPro.TMP_Text>();
            GameObject CurrentPriorityTextObject = GameObject.Find("CurrentPriority_Text");
            CurrentPriorityText = CurrentPriorityTextObject.GetComponent<TMPro.TMP_Text>();
            EditorSelectedTextObject = CanvasObject.FindObject("Editor_Selected_Text");
            EditorSelectedText = EditorSelectedTextObject.GetComponent<TMPro.TMP_Text>();
            
            //Find Background Images for Toggles
            VisualzierBackground = VisibilityMenuObject.FindObject("Background_Visualizer");
            MenuBackground = MenuButtonObject.FindObject("Background_Menu");

            //Find OnScreeen Message Prefabs
            MessagesParent = CanvasObject.FindObject("OnScreenMessages");
            OnScreenErrorMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenErrorMessagePrefab");
            OnScreenInfoMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenInfoMessagePrefab");

            //OnScreen Messages with custom acknowledgement events.
            ActiveRobotUpdatedFromPlannerMessageObject = MessagesParent.FindObject("Prefabs").FindObject("ActiveRobotUpdatedFromPlannerMessage");
            TrajectoryReviewRequestMessageObject = MessagesParent.FindObject("Prefabs").FindObject("TrajectoryReviewRequestReceivedMessage");
        }
        private void SetVisualizerMenuItemsOnStart()
        {
            /*
            * SetVisualizerMenuItemsOnStart : Method is used to set up the Visualizer Menu UI elements on start.
            * Visualizer Menu UI elements constitute the UI elements that are used to control various visualization
            * functionalities of the application.
            */

            //Find Visualizer Menu Objects
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref PreviewActorToggleObject, "PreviewActorToggle", TogglePreviewActor);
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref IDToggleObject, "ID_Toggle", ToggleID);
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref RobotToggleObject, "RobotToggle", ToggleRobot);
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref ScrollSearchToggleObject, "ScrollSearchToggle", ToggleScrollSearch);
            ScrollSearchObjects = ScrollSearchToggleObject.FindObject("ScrollSearchObjects");

            //Find Robot toggle and Objects
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref PriorityViewerToggleObject, "PriorityViewer", TogglePriority);
            PriorityViewerBackground = PriorityViewerToggleObject.FindObject("BackgroundPriorityViewer");
            SelectedPriorityTextObject = PriorityViewerToggleObject.FindObject("SelectedPriorityText");
            SelectedPriorityText = SelectedPriorityTextObject.GetComponent<TMP_Text>();
            UserInterface.FindButtonandSetOnClickAction(PriorityViewerToggleObject, ref NextPriorityButtonObject, "NextPriorityButton", SetNextPriorityGroup);
            UserInterface.FindButtonandSetOnClickAction(PriorityViewerToggleObject, ref PreviousPriorityButtonObject, "PreviousPriorityButton", SetPreviousPriorityGroup);

            //Find Object Lengths Toggle and Objects
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref ObjectLengthsToggleObject, "ObjectLength_Button", ToggleObjectLengths);
            ObjectLengthsUIPanelObjects = CanvasObject.FindObject("ObjectLengthsPanel");
            ObjectLengthsUIPanelPosition = ObjectLengthsUIPanelObjects.transform.localPosition;
            ObjectLengthsText = ObjectLengthsUIPanelObjects.FindObject("LengthsText").GetComponent<TMP_Text>();
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
        }
        private void SetMenuItemsOnStart()
        {
            /*
            * SetMenuItemsOnStart : Method is used to set up the Menu UI elements on start.
            * Menu UI elements constitute the UI elements that are used to control additional functionalities
            * of the application.
            */

            //Find Toggle Objects and set up on value changed actions
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref InfoToggleObject, "Info_Button", ToggleInfo);
            UserInterface.FindButtonandSetOnClickAction(MenuButtonObject, ref ReloadButtonObject, "Reload_Button", ReloadApplication);
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref CommunicationToggleObject, "Communication_Button", ToggleCommunication);
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref EditorToggleObject, "Editor_Toggle", ToggleEditor);
            UserInterface.FindButtonandSetOnClickAction(EditorToggleObject, ref BuilderEditorButtonObject, "Builder_Editor_Button", TouchModifyActor);
            UserInterface.FindButtonandSetOnClickAction(EditorToggleObject, ref BuildStatusButtonObject, "Build_Status_Editor", TouchModifyBuildStatus);
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref CommunicationToggleObject, "Communication_Button", ToggleCommunication);

            //Find Panel Objects used for Info and communication
            InfoPanelObject = CanvasObject.FindObject("InfoPanel");
            CommunicationPanelObject = CanvasObject.FindObject("CommunicationPanel");

            //Find Background Images for Toggles
            EditorBackground = EditorToggleObject.FindObject("Background_Editor");
        }
        private void SetCommunicationItemsOnStart()
        {
            /*
            * SetCommunicationItemsOnStart : Method is used to set up the Communication UI elements on start.
            * Communication UI elements constitute the UI elements that are used to control the communication
            * functionalities of the application. Such as Trajectory Review, Connection Management, & Robot Selection.
            */

            //Find Pannel Objects used for connecting to a different MQTT broker
            MqttBrokerInputField = CommunicationPanelObject.FindObject("MqttBrokerInputField").GetComponent<TMP_InputField>();
            MqttPortInputField = CommunicationPanelObject.FindObject("MqttPortInputField").GetComponent<TMP_InputField>();
            MqttUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsMQTTReconnectMessage");
            MqttConnectionStatusObject = CommunicationPanelObject.FindObject("MqttConnectionStatusObject");
            UserInterface.FindButtonandSetOnClickAction(CommunicationPanelObject, ref MqttConnectButtonObject, "MqttConnectButton", UpdateMqttConnectionFromUserInputs);

            //Find Pannel Objects used for connecting to a different ROS host
            RosHostInputField = CommunicationPanelObject.FindObject("ROSHostInputField").GetComponent<TMP_InputField>();
            RosPortInputField = CommunicationPanelObject.FindObject("ROSPortInputField").GetComponent<TMP_InputField>();
            RosUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsROSReconnectMessage");
            RosConnectionStatusObject = CommunicationPanelObject.FindObject("ROSConnectionStatusObject");
            UserInterface.FindButtonandSetOnClickAction(CommunicationPanelObject, ref RosConnectButtonObject, "ROSConnectButton", UpdateRosConnectionFromUserInputs);

            //Find Control Objects and set up events
            GameObject TrajectoryControlObjects = GameObject.Find("TrajectoryReviewUIControls");
            ReviewTrajectoryObjects = TrajectoryControlObjects.FindObject("ReviewTrajectoryControls");

            //Find Object, request button and add event listner for on click method
            UserInterface.FindButtonandSetOnClickAction(TrajectoryControlObjects, ref RequestTrajectoryButtonObject, "RequestTrajectoryButton", RequestTrajectoryButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(ReviewTrajectoryObjects, ref ApproveTrajectoryButtonObject, "ApproveTrajectoryButton", ApproveTrajectoryButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(ReviewTrajectoryObjects, ref RejectTrajectoryButtonObject, "RejectTrajectoryButton", RejectTrajectoryButtonMethod);
            UserInterface.FindSliderandSetOnValueChangeAction(ReviewTrajectoryObjects, ref TrajectoryReviewSliderObject, ref TrajectoryReviewSlider, "TrajectoryReviewSlider", TrajectorySliderReviewMethod);
            UserInterface.FindButtonandSetOnClickAction(TrajectoryControlObjects, ref ExecuteTrajectoryButtonObject, "ExecuteTrajectoryButton", ExecuteTrajectoryButtonMethod);
        }
        public void SetOcclusionFromOS(ref AROcclusionManager occlusionManager, CompasXR.Systems.OperatingSystem currentOperatingSystem)
        {
            /*  
            * Method used to set the occlusion manager based on the current operating system.
            * This method is used to enable occlusion on iOS devices and disable it on other devices.
            */
            if(currentOperatingSystem == CompasXR.Systems.OperatingSystem.iOS)
            {
                occlusionManager = FindObjectOfType<AROcclusionManager>(true);
                occlusionManager.enabled = true;

                Debug.Log("AROcclusion: will be activated because current platform is ios");
            }
            else
            {
                Debug.Log("AROcclusion: will not be activated because current system is not ios");
            }
        }

        /////////////////////////////////////// Primary UI Functions //////////////////////////////////////////////
        public void ToggleVisibilityMenu(Toggle toggle)
        {
            /*
            * Method is used to toggle the visibility of the Visualizer Menu items.
            */
            if (VisualzierBackground != null && PreviewActorToggleObject != null && RobotToggleObject != null && ObjectLengthsToggleObject != null && IDToggleObject != null && PriorityViewerToggleObject != null)
            {    
                if (toggle.isOn)
                {             
                    VisualzierBackground.SetActive(true);
                    PreviewActorToggleObject.SetActive(true);
                    RobotToggleObject.SetActive(true);
                    ObjectLengthsToggleObject.SetActive(true);
                    IDToggleObject.SetActive(true);
                    PriorityViewerToggleObject.SetActive(true);
                    ScrollSearchToggleObject.SetActive(true);
                    UserInterface.SetUIObjectColor(VisibilityMenuObject, Yellow);

                }
                else
                {
                    VisualzierBackground.SetActive(false);
                    PreviewActorToggleObject.SetActive(false);
                    RobotToggleObject.SetActive(false);
                    ObjectLengthsToggleObject.SetActive(false);
                    IDToggleObject.SetActive(false);
                    PriorityViewerToggleObject.SetActive(false);
                    ScrollSearchToggleObject.SetActive(false);
                    UserInterface.SetUIObjectColor(VisibilityMenuObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find one of the buttons in the Visualizer Menu.");
            }   
        }
        public void ToggleMenu(Toggle toggle)
        {
            /*
            * Method is used to toggle the visibility of the Menu items.
            */
            if (MenuBackground != null && InfoToggleObject != null && ReloadButtonObject != null && CommunicationToggleObject != null && EditorToggleObject != null)
            {    
                if (toggle.isOn)
                {             
                    MenuBackground.SetActive(true);
                    InfoToggleObject.SetActive(true);
                    ReloadButtonObject.SetActive(true);
                    CommunicationToggleObject.SetActive(true);
                    EditorToggleObject.SetActive(true);
                    UserInterface.SetUIObjectColor(MenuButtonObject, Yellow);

                }
                else
                {
                    if(EditorToggleObject.GetComponent<Toggle>().isOn){
                        EditorToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(InfoToggleObject.GetComponent<Toggle>().isOn){
                        InfoToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(CommunicationToggleObject.GetComponent<Toggle>().isOn){
                        CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    MenuBackground.SetActive(false);
                    InfoToggleObject.SetActive(false);
                    ReloadButtonObject.SetActive(false);
                    CommunicationToggleObject.SetActive(false);
                    EditorToggleObject.SetActive(false);
                    UserInterface.SetUIObjectColor(MenuButtonObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find one of the buttons in the Menu.");
            }   
        }
        public void NextStepButton()
        {
            /*
            * Method is used to move to the next step in the building plan.
            */
            if(CurrentStep != null)
            {
                int CurrentStepInt = Convert.ToInt16(CurrentStep);
                if(CurrentStepInt < databaseManager.BuildingPlanDataItem.steps.Count - 1)
                {
                    SetCurrentStep((CurrentStepInt + 1).ToString());
                }  
            }
        }
        public void SetCurrentStep(string key)
        {
            /*
            * Method is used to set the current step in the building plan.
            * This method is really used to do a lot of UI control.
            * It handles the coloring, control, and coordination of many items
            * both in the UI and in AR space.
            */

            //If the current step is not null, remove the arrow from the previous step
            if(CurrentStep != null)
            {
                ObjectInstantiaion.DestroyGameObjectByName($"{CurrentStep} Arrow");
                GameObject previousStepElement = Elements.FindObject(CurrentStep);

                if(previousStepElement != null)
                {
                    Step PreviousStep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
                    string elementID = PreviousStep.data.element_ids[0];
                    instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, PreviousStep, key, previousStepElement.FindObject(elementID + " Geometry"));
                    if (PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                    {
                        instantiateObjects.ColorObjectByPriority(SelectedPriority, PreviousStep.data.priority.ToString(), CurrentStep, previousStepElement.FindObject(elementID + " Geometry"));
                    }
                }
            }

            //Set the current step to the new key
            CurrentStep = key;
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            GameObject element = Elements.FindObject(key);
            if(element != null)
            {
                instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject(step.data.element_ids[0] + " Geometry"));
                Debug.Log($"SetCurrentStep: Current Step is now {CurrentStep}");
            }
            CurrentStepText.text = CurrentStep;
            
            //Write current step information to the database
            instantiateObjects.UserIndicatorInstantiator(ref instantiateObjects.MyUserIndacator, element, CurrentStep, CurrentStep, "ME", 0.25f);
            UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
            userCurrentInfo.currentStep = CurrentStep;
            userCurrentInfo.timeStamp = (System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss"));
            databaseManager.UserCurrentStepDict[SystemInfo.deviceUniqueIdentifier] = userCurrentInfo;
            DataHandlers.PushStringDataToDatabaseReference(databaseManager.dbReferenceUsersCurrentSteps.Child(SystemInfo.deviceUniqueIdentifier), JsonConvert.SerializeObject(userCurrentInfo));

            //Check other additional UI elements and set them  based on their toggle status
            if(ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
            {
                instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
            }
            if(RobotToggleObject.GetComponent<Toggle>().isOn)
            {
                SetRoboticUIElementsFromKey(CurrentStep);
                if(trajectoryVisualizer.ActiveRobot != null)
                {
                    if(step.data.actor == "ROBOT")
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(true);
                        trajectoryVisualizer.ActiveRobot.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(false);
                    }
                    if(trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    }
                }
                else
                {
                    Debug.LogWarning("SetCurrentStep: Active Robot is null.");
                }
            }

            //Update preview geometry and is built graphics
            PreviewGeometrySliderSetVisibilty(PreviewGeometrySlider.value);
            IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
        }
        public void PreviousStepButton()
        {
            /*
            * Method is used to move to the previous step in the building plan on button press.
            */
            if(CurrentStep != null)
            {
                int CurrentStepInt = Convert.ToInt16(CurrentStep);
                if(CurrentStepInt > 0)
                {
                    SetCurrentStep((CurrentStepInt - 1).ToString());
                }  
            }       

        }
        public void PreviewGeometrySliderSetVisibilty(float value)
        {
            /*
            * Method is used to set the visibility of geometry in the scene based on the slider value.
            */
            if (CurrentStep != null)
            {
                int min = Convert.ToInt16(CurrentStep);
                float SliderValue = value;
                int ElementsTotal = databaseManager.BuildingPlanDataItem.steps.Count;
                float SliderMax = 1;
                float SliderMin = 0;
                float SliderRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, min, ElementsTotal); 

                foreach(int index in Enumerable.Range(min, ElementsTotal))
                {
                    string elementName = index.ToString();
                    int InstanceNumber = Convert.ToInt16(elementName);
                    GameObject element = Elements.FindObject(elementName);
                    if (element != null)
                    {
                        if (InstanceNumber > SliderRemaped)
                        {
                            element.SetActive(false); 
                        }
                        else
                        {
                            element.SetActive(true);
                        }
                    }
                }
            }
        }
        public void IsBuiltButtonGraphicsControler(bool builtStatus, int stepPriority)
        {
            /*
            * Method is used to control the graphics of the is built button based on the built status of the step.
            */
            if (IsBuiltPanelObjects.activeSelf)
            {
                if (builtStatus)
                {
                    IsbuiltButtonImage.SetActive(true);
                    IsBuiltButtonObject.GetComponent<UnityEngine.UI.Image>().color = TranspGrey;
                }
                else
                {
                    IsbuiltButtonImage.SetActive(false);
                    IsBuiltButtonObject.GetComponent<UnityEngine.UI.Image>().color = TranspWhite;
                }
            }
        }
        public bool LocalPriorityChecker(Step step)
        {
            /*
            * Method is used to check the priority of the step and determine if the step can be built.
            * This method is used to check the priority of the step and determine if the step can be built
            * based on the current set priority and the step of the priority attempting to be built.
            */
            if(databaseManager.CurrentPriority == null)
            {
                Debug.LogError("LocalPriorityChecker: Current Priority is null.");
                return false;
            }
            else if (databaseManager.CurrentPriority == step.data.priority.ToString())
            {
                return true;
            }
            else if (Convert.ToInt16(databaseManager.CurrentPriority) > step.data.priority) //TODO: THIS ONLY WORKS BECAUSE WE PUSH EVERYTHING.
            {
                for(int i = Convert.ToInt16(step.data.priority) + 1; i < databaseManager.BuildingPlanDataItem.PriorityTreeDictionary.Count; i++)
                {
                    List<string> PriorityDataItem = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[i.ToString()];
                    foreach(string key in PriorityDataItem)
                    {
                        Step stepToUnbuild = databaseManager.BuildingPlanDataItem.steps[key];
                        if(stepToUnbuild.data.is_built)
                        {                        
                            stepToUnbuild.data.is_built = false;
                        }
                        instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, stepToUnbuild, key, Elements.FindObject(key).FindObject(stepToUnbuild.data.element_ids[0] + " Geometry"));
                    }
                }
                return true;
            }
            else
            {
                if(step.data.priority != Convert.ToInt16(databaseManager.CurrentPriority) + 1)
                {
                    string message = $"WARNING: This elements priority is incorrect. It is priority {step.data.priority.ToString()} and next priority to build is {Convert.ToInt16(databaseManager.CurrentPriority) + 1}";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PriorityIncorrectWarningMessageObject, "PriorityIncorrectWarningMessage", MessagesParent, message, "LocalPriorityChecker: Priority Incorrect Warning");
                    return false;
                }
                else
                {   
                    List<string> UnbuiltElements = new List<string>();
                    List<string> PriorityDataItem = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[databaseManager.CurrentPriority];

                    foreach(string element in PriorityDataItem)
                    {
                        Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[element];
                        if(!stepToCheck.data.is_built)
                        {
                            UnbuiltElements.Add(element);
                        }
                    }
                    if(UnbuiltElements.Count == 0)
                    {
                        Debug.Log($"Priority Check: Current Priority is complete. Unlocking Next Priority.");
                        string message = $"The previous priority {databaseManager.CurrentPriority} is complete you are now moving on to priority {step.data.priority.ToString()}.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenInfoMessagePrefab, ref PriorityCompleteMessageObject, "PriorityCompleteMessage", MessagesParent, message, "LocalPriorityChecker: Priority Complete Message");
                        SetCurrentPriority(step.data.priority.ToString());

                        if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.priority.ToString() == databaseManager.CurrentPriority)
                        {    
                            IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                        }
                        return false;
                    }
                    else
                    {
                        string message = $"WARNING: This element cannot build because the following elements from Current Priority {databaseManager.CurrentPriority} are not built: {string.Join(", ", UnbuiltElements)}";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PriorityIncompleteWarningMessageObject, "PriorityIncompleteWarningMessage", MessagesParent, message, "LocalPriorityChecker: Priority Incomplete Warning");
                        return false;
                    }
                }
            }
        }
        public void ModifyStepBuildStatus(string key)
        {
            /*
            * Method is used to modify the build status of the step based on the key.
            * If the step is being unbuilt, it will unbuild all steps of a higher priority,
            * and it will prevent building of steps with higher priority then the current priority.
            */
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            if (LocalPriorityChecker(step))
            {
                if(step.data.is_built)
                {
                    step.data.is_built = false;
                    int StepInt = Convert.ToInt16(key);
                    for(int i = StepInt; i >= 0; i--)
                    {
                        Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[i.ToString()];
                        if(StepInt == 0)
                        {
                            SetCurrentPriority(stepToCheck.data.priority.ToString());
                            break;   
                        }
                        if(stepToCheck.data.is_built)
                        {
                            databaseManager.BuildingPlanDataItem.LastBuiltIndex = i.ToString();
                            SetLastBuiltText(i.ToString());
                            SetCurrentPriority(stepToCheck.data.priority.ToString());
                            break;
                        }
                    }
                }
                else
                {
                    step.data.is_built = true;
                    databaseManager.BuildingPlanDataItem.LastBuiltIndex = key;
                    SetLastBuiltText(key);
                    SetCurrentPriority(step.data.priority.ToString());
                }

                instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
                if(key == CurrentStep)
                {    
                    IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                }
                databaseManager.PushAllDataBuildingPlan(key);
            }
            else
            {
                Debug.Log("ModifyStepBuildStatus: Priority Check will not allow this step to be built.");
            }
        }
        public void SetLastBuiltText(string key)
        {
            /*
            * Method is used to set the last built text on the screen.
            */
            LastBuiltIndexText.text = $"Last Built Element : {key}";
        }
        public void SetCurrentPriority(string Priority)
        {        
            /*
            * Method is used to set the current priority of the building plan.
            * This method is used to set the current priority of the building plan
            * and update the UI elements based on the current priority.
            */
            if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && databaseManager.CurrentPriority != Priority)
            {
                instantiateObjects.ApplyColorBasedOnPriority(Priority);
                SelectedPriority = Priority;
                PriorityViewerUIGraphicsController(true, Priority);
            }
            databaseManager.CurrentPriority = Priority;

            if(RobotToggleObject.GetComponent<Toggle>().isOn)
            {
                SetRoboticUIElementsFromKey(CurrentStep);
            }
            CurrentPriorityText.text = $"Current Priority : {Priority}";
            Debug.Log($"SetCurrentPriority: Current Priority set to {Priority} ");
        }
        public void SetActiveRobotToggleMethod(Toggle toggle)
        {
            /*
            * Method is used to set the active robot based on the toggle value.
            * Additionally it controls UI elements based on the toggle value.
            */
            if(toggle!=null && toggle.isOn)
            {
                Debug.Log($"SettingActiveRobotButtonMethod: Setting Active Robot based on input {RobotSelectionDropdown.options[RobotSelectionDropdown.value].text}");
                string robotName = RobotSelectionDropdown.options[RobotSelectionDropdown.value].text;
                bool visibility = false;
                if(CurrentStep != null && RobotToggleObject.GetComponent<Toggle>().isOn)
                {
                    if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.actor == "ROBOT")
                    {
                        visibility = true;
                    }
                }
                trajectoryVisualizer.SetActiveRobotFromDropdown(robotName, true, visibility);
                SetActiveRobotToggleObject.FindObject("Image").SetActive(true);
            }
            else
            {
                Debug.Log("SettingActiveRobotButtonMethod: Destroying Current Active Robot");
                if(trajectoryVisualizer.ActiveRobotObjects.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveRobotObjects();
                }
                mqttTrajectoryManager.serviceManager.ActiveRobotName = null;  //TODO: THIS IS FROM COMPAS XR, BUT NEEDS TO BE THOUGHT ABOUT FOR ROBOT TERRITORIES
                SetActiveRobotToggleObject.FindObject("Image").SetActive(false);           
            }
        }
        public void RobotSelectionDropdownValueChanged(int dropDownValue)
        {
            /*
            * Method is used to set the active robot based on the dropdown value.
            * Additionally it controls UI elements based on the dropdown value.
            */
            Debug.Log($"RobotSelectionDropdownValueChanged: Robot Selection Dropdown Value Changed to {dropDownValue}. Setting Current Active Robot to False.");
            SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
        }

        /////////////////////////////////////// On Screen Message Functions //////////////////////////////////////////////
        public void SignalTrajectoryReviewRequest(string key, string robotName, string activeRobotName, Action visualizeRobotMethod)
        {
            /*
            * Method is used to signal a trajectory review request from another user.
            * This method is used for a custom review request because it has more requirements then message requests.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.Log($"Trajectory Review Request: Other User is Requesting review of Trajectory for Step {key} .");
            TMP_Text messageComponent = TrajectoryReviewRequestMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();
            string message = null;
            if(activeRobotName != robotName)
            {
                message = $"REQUEST : Trajectory Review requested for step: {key} with Robot: {robotName}. YOUR ACTIVE ROBOT UPDATED.";
                int robotSelection = RobotSelectionDropdown.options.FindIndex(option => option.text == robotName);
                if(robotSelection != -1)
                {            
                    if(SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    RobotSelectionDropdown.value = robotSelection;
                    SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = true;
                }
                else
                {
                    Debug.LogError($"Trajectory Review Request Message: Could not find robot {robotName} in dropdown options.");
                }
            }
            else
            {
                message = $"REQUEST : Trajectory Review requested for step: {key} with Robot: {robotName}.";
            }
            
            if(TransactionLockActiveWarningMessageObject != null && TransactionLockActiveWarningMessageObject.activeSelf)
            {
                TransactionLockActiveWarningMessageObject.SetActive(false);
            }

            if(messageComponent != null && message != null && TrajectoryReviewRequestMessageObject != null)
            {
                UserInterface.SignalOnScreenMessageWithButton(TrajectoryReviewRequestMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("Trajectory Review Request Message: Could not find message object or message component.");
            }

            GameObject AcknowledgeButton = TrajectoryReviewRequestMessageObject.FindObject("AcknowledgeButton");
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentStep(key));
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => 
                {
                    if(!RobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        RobotToggleObject.GetComponent<Toggle>().isOn = true;
                    }
                });
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => TrajectoryServicesUIControler(false, false, true, true, false, false));
            }
        }
        public void SignalActiveRobotUpdateFromPlanner(string key, string robotName, string activeRobotName, Action visualizeRobotMethod)
        {
            /*
            * Method is used to signal an active robot update from another user on message request.
            * This method is used for a custom active robot update because it has more requirements then message requests.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.Log($"SignalActiveRobotUpdateFromPlanner: Other User is Requesting review of Trajectory for Step {key} .");
            TMP_Text messageComponent = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();
            string message = $"WARNING: You requested for {activeRobotName} but reply Trajectory is for {robotName}. ACTIVE ROBOT UPDATED.";
            int robotSelection = RobotSelectionDropdown.options.FindIndex(option => option.text == robotName);

            if(robotSelection != -1)
            {            
                if(SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                {
                    SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
                }
                RobotSelectionDropdown.value = robotSelection;
                SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = true;
            }
            else
            {
                Debug.LogError("SignalActiveRobotUpdateFromPlanner: Could not find robot in dropdown options.");
            }

            if(messageComponent != null && message != null && ActiveRobotUpdatedFromPlannerMessageObject != null)
            {
                UserInterface.SignalOnScreenMessageWithButton(ActiveRobotUpdatedFromPlannerMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Could not find message object or message component.");
            }

            GameObject AcknowledgeButton = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("AcknowledgeButton");
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => TrajectoryServicesUIControler(false, false, true, true, false, false));
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Something Is messed up with on click event listner.");
            }

        }
        public void SignalMQTTConnectionFailed()
        {
            /*
            * Method is used to signal a MQTT connection failure to the user.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.LogWarning("MQTT: MQTT Connection Failed.");
            // if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
            // {
            //     CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
            // }
            string message = $"WARNING: MQTT Failed to connect to broker: {mqttTrajectoryManager.brokerAddress} on port: {mqttTrajectoryManager.brokerPort}. Please check your internet and try again.";
            UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MQTTFailedToConnectMessageObject, "MQTTConnectionFailedMessage", MessagesParent, message, "SignalMQTTConnectionFailed: MQTT Connection Failed.");
        }

        /////////////////////////////////////// Communication Buttons //////////////////////////////////////////////
        public void UpdateConnectionStatusText(GameObject connectionStatusObject, bool connectionStatus)
        {
            /*
            * Method is used to update the connection status text based on the connection status of communication protocols.
            */
            TMP_Text connectionStatusText = connectionStatusObject.FindObject("StatusText").GetComponent<TMP_Text>();
            if(RosConnectionStatusObject == null)
            {
                Debug.LogWarning("ConnectionStatusText is null for " + connectionStatusObject.name);
            }

            if(connectionStatus)
            {
                connectionStatusText.text = "CONNECTED";
                connectionStatusText.color = Color.green;
            }
            else
            {
                connectionStatusText.text = "DISCONNECTED";
                connectionStatusText.color = Color.red;
            }
        }
        public void UpdateMqttConnectionFromUserInputs()
        {
            /*
            * Method is used to update the MQTT connection based on the user inputs.
            * This allows the user to input various MQTT broker and port addresses.
            * Allowing for custom and even encrypted connections.
            */
            UserInterface.SetUIObjectColor(MqttConnectButtonObject, White);
            string newMqttBroker = MqttBrokerInputField.text;
            if (string.IsNullOrWhiteSpace(newMqttBroker))
            {
                newMqttBroker = "broker.hivemq.com";
            }

            string newMqttPort = MqttPortInputField.text;
            if (string.IsNullOrWhiteSpace(newMqttPort))
            {
                newMqttPort = "1883";
            }

            if (newMqttBroker != mqttTrajectoryManager.brokerAddress || Convert.ToInt32(newMqttPort) != mqttTrajectoryManager.brokerPort)
            {
                mqttTrajectoryManager.RemoveConnectionEventListners();
                mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();
                mqttTrajectoryManager.brokerAddress = newMqttBroker;
                mqttTrajectoryManager.brokerPort = Convert.ToInt32(newMqttPort);
                mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();
            }
            else
            {
                Debug.Log("MQTT: Broker and Port are the same as the current one. Not updating connection.");
                MqttUpdateConnectionMessage.SetActive(true);
            }
        }
        public void UpdateRosConnectionFromUserInputs()
        {
            Debug.Log($"UpdateRosConnectionFromUserInputs: Attempting ROS Connection to ws://{RosHostInputField.text}:{RosPortInputField.text} from User Inputs.");
            UserInterface.SetUIObjectColor(RosConnectButtonObject, White);
            string rosHostInput = RosHostInputField.text;
            string rosPortInput = RosPortInputField.text;

            if (string.IsNullOrWhiteSpace(rosHostInput) || string.IsNullOrWhiteSpace(rosPortInput))
            {
                rosHostInput = "localhost";
                rosPortInput = "9090";
            }
            string newRosBridgeAddress = $"ws://{rosHostInput}:{rosPortInput}";

            Debug.Log("UpdateRosConnectionFromUserInputs: New ROS Bridge Address: " + newRosBridgeAddress);
            if (newRosBridgeAddress != rosConnectionManager.RosBridgeServerUrl || !rosConnectionManager.IsConnectedToRos)
            {
                if(rosConnectionManager.IsConnectedToRos)
                {
                    rosConnectionManager.RosSocket.Close();
                }
                rosConnectionManager.RosBridgeServerUrl = newRosBridgeAddress;
                rosConnectionManager.ConnectAndWait();
            }
            else
            {
                Debug.Log("UpdateRosConnectionFromUserInputs: ROS Host and Port are the same as our current and we are connected. Not updating connection.");
                RosUpdateConnectionMessage.SetActive(true);
            }
        }
        public void TrajectoryServicesUIControler(bool requestTrajectoryVisability, bool requestTrajectoryInteractable, bool trajectoryReviewVisibility, bool trajectoryReviewInteractable, bool executeTrajectoryVisability, bool executeTrajectoryInteractable)
        {
            /*
            * Method is used to control the UI elements of the trajectory services.
            * This method is used to control the visibility and interactibility of UI elements for trajectory services
            * based on the current service and the current user.
            */
            RequestTrajectoryButtonObject.SetActive(requestTrajectoryVisability);
            RequestTrajectoryButtonObject.GetComponent<Button>().interactable = requestTrajectoryInteractable;

            ReviewTrajectoryObjects.SetActive(trajectoryReviewVisibility);
            ApproveTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;
            RejectTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;
            if (trajectoryReviewVisibility)
            {
                if(TrajectoryReviewSlider.value != 0)
                {
                    TrajectoryReviewSlider.value = 0;
                    trajectoryVisualizer.previousTrajectoryReviewSliderValue = 0;
                    trajectoryVisualizer.previousConfigIndex = 0;
                    trajectoryVisualizer.previousTrajectoryIndex = 0;
                }
            }

            ExecuteTrajectoryButtonObject.SetActive(executeTrajectoryVisability);
            ExecuteTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;

            if (executeTrajectoryInteractable)
            {
                RejectTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;
            }
            if ( trajectoryReviewVisibility || executeTrajectoryVisability)
            {
                RobotToggleObject.GetComponent<Toggle>().interactable = false;

                //Next and previous button not interactable based on service
                NextGeometryButtonObject.GetComponent<Button>().interactable = false;
                PreviousGeometryButtonObject.GetComponent<Button>().interactable = false;
                
            }
            else if (requestTrajectoryVisability)
            {
                RobotToggleObject.GetComponent<Toggle>().interactable = true;
                NextGeometryButtonObject.GetComponent<Button>().interactable = true;
                PreviousGeometryButtonObject.GetComponent<Button>().interactable = true;
            }
        }
        public void RequestTrajectoryButtonMethod()
        {
            /*
            * Method is used to request a trajectory for the current step.
            * This method is used to request a trajectory for the current step,
            * set UI elements and publish the request on the particular request topic.
            */
            Debug.Log($"RequestTrajectoryButtonMethod: Requesting Trajectory for Step {CurrentStep}");

            if (mqttTrajectoryManager.serviceManager.TrajectoryRequestTransactionLock)
            {
                Debug.Log("RequestTrajectoryButtonMethod : You cannot request because transaction lock is active");
                string message = "WARNING: You are currently prevented from requesting because another active user is awaiting a Trajectory Result.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref TransactionLockActiveWarningMessageObject, "TransactionLockActiveWarningMessage", MessagesParent, message, "RequestTrajectoryButtonMethod: Transaction Lock Active Warning.");
                return;
            }
            else if (trajectoryVisualizer.ActiveRobot == null)
            {
                Debug.Log("RequestTrajectoryButtonMethod: Active Robot is null");
                string message = "WARNING: Active Robot is currently null. An active robot must be set before visulizing robotic information.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref ActiveRobotIsNullWarningMessageObject, "ActiveRobotNullWarningMessage", MessagesParent, message, "RequestTrajectoryButtonMethod: Active Robot is null.");
                return;
            }
            else
            {    
                mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopic, new GetTrajectoryRequest(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName).GetData());
                mqttTrajectoryManager.serviceManager.PrimaryUser = true;
                mqttTrajectoryManager.serviceManager.currentService = ServiceManager.CurrentService.GetTrajectory;
                TrajectoryServicesUIControler(true, false, false, false, false, false);
            }
        }
        public void ApproveTrajectoryButtonMethod()
        {
            /*
            * Method is used to approve a trajectory for the current step.
            * This method is used to approve a trajectory for the current step,
            * set UI elements and publish the approval on the particular approval topic.
            */
            Debug.Log($"ApproveTrajectoryButtonMethod: Approving Trajectory for Step {CurrentStep}");
            TrajectoryServicesUIControler(false, false, true, false, false, false);
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 1).GetData());
        }
        public void RejectTrajectoryButtonMethod()
        {
            /*
            * Method is used to reject a trajectory for the current step.
            * This method is used to reject a trajectory for the current step,
            * set UI elements and publish the rejection on the particular approval topic.
            */
            Debug.Log($"RejectTrajectoryButtonMethod: Rejecting Trajectory for Step {CurrentStep}");
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 0).GetData());
            TrajectoryServicesUIControler(false, false, true, false, false, false);
        }
        public void TrajectorySliderReviewMethod(float value)
        {
            if (mqttTrajectoryManager.serviceManager.CurrentTrajectory != null)
            {
                if (mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count > 0)
                {
                    float SliderValue = value;
                    int TrajectoryConfigurationsCount = mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count; 
                    float SliderMax = 1;
                    float SliderMin = 0;
                    float SliderValueRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0, TrajectoryConfigurationsCount-1); 
                    Debug.Log($"TrajectorySliderReviewMethod: Slider Value Changed is value {value} and the item is {JsonConvert.SerializeObject(mqttTrajectoryManager.serviceManager.CurrentTrajectory[(int)SliderValueRemaped])}"); //TODO:CHECK SLIDER REMAP
                    trajectoryVisualizer.ColorRobotConfigfromSliderInput((int)SliderValueRemaped, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial,ref trajectoryVisualizer.previousTrajectoryReviewSliderValue);
                }
                else
                {
                    Debug.Log("TrajectorySliderReviewMethod: Current Trajectory Count is 0.");
                }
            }
            else
            {
                Debug.Log("TrajectorySliderReviewMethod: Current Trajectory is null.");
            }
        }
        public void ExecuteTrajectoryButtonMethod()
        {
            /*
            * Method is used to execute a trajectory for the current step.
            * It sets UI elements and publish the execution on the particular approval topic to the CAD.
            */
            Debug.Log($"ExecuteTrajectoryButtonMethod: Executing Trajectory for Step {CurrentStep}");
            Dictionary<string, object> sendTrajectoryMessage = new SendTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory).GetData();
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.sendTrajectoryTopic, sendTrajectoryMessage);
            TrajectoryServicesUIControler(false, false, false, false, true, false);
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.ActiveRobotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 2).GetData());
        }

        ////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
        public void TogglePreviewActor(Toggle toggle)
        {
            /*
            * Method is used to toggle the preview actor view in the scene.
            * Additionally it will color all elements in the scene based on their actor.
            */
            Debug.Log("TogglePreviewActor: Preview Builder Toggle Pressed value set to " + toggle.GetComponent<Toggle>().isOn);
            if(toggle.isOn)
            {
                instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.ActorView;
                instantiateObjects.ApplyColorBasedOnActor();
                UserInterface.SetUIObjectColor(PreviewActorToggleObject, Yellow);
            }
            else
            {
                instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.BuiltUnbuilt;
                instantiateObjects.ApplyColorBasedOnAppModes();
                UserInterface.SetUIObjectColor(PreviewActorToggleObject, White);
            }
        }
        public void ToggleID(Toggle toggle)
        {
            /*
            * Method is used to toggle the ID tags in the scene.
            * Additionally it will reposition the ID tags based on priority viewer the toggle value.
            */
            Debug.Log("ToggleID: ID Toggle Pressed value set to " + toggle.GetComponent<Toggle>().isOn);
            if (toggle != null && IDToggleObject != null)
            {
                if(toggle.isOn)
                {
                    ARSpaceTextControler(true, "IdxText", ref IDTagIsOffset, "IdxImage", PriorityViewerToggleObject.GetComponent<Toggle>().isOn, 0.155f); //bool verticlReposition, float distance
                    UserInterface.SetUIObjectColor(IDToggleObject, Yellow);
                }
                else
                {
                    ARSpaceTextControler(false, "IdxText", ref IDTagIsOffset, "IdxImage");
                    if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && PriorityTagIsOffset)
                    {
                        ARSpaceTextControler(true, "PriorityText", ref PriorityTagIsOffset, "PriorityImage");
                    }
                    UserInterface.SetUIObjectColor(IDToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleID: Could not find ID Toggle or ID Toggle Object.");
            }
        }
        public void ARSpaceTextControler(bool Visibility, string textObjectBaseName, ref bool tagIsOffset, string imageObjectBaseName = null, bool verticalReposition = false, float? verticalOffset = null)
        {
            /*
            * Method is used to control the visibility of AR Space Text in the scene.
            * Additionally it will reposition the AR Space Text based on the visibility and the vertical reposition value.
            */
            if (instantiateObjects != null && instantiateObjects.Elements != null)
            {
                foreach (Transform child in instantiateObjects.Elements.transform)
                {
                    Transform textChild = child.Find(child.name + textObjectBaseName);
                    if (textChild != null)
                    {
                        textChild.gameObject.SetActive(Visibility);
                    }
                    if (verticalReposition)
                    {
                        Vector3 objectposition = textChild.transform.position;
                        Vector3 newPosition = ObjectTransformations.OffsetPositionVectorByDistance(objectposition, verticalOffset.GetValueOrDefault(0.0f), "y");
                        textChild.position = newPosition;
                    }
                    else
                    {
                        HelpersExtensions.ObjectPositionInfo instantiationPosition = textChild.GetComponent<HelpersExtensions.ObjectPositionInfo>();
                        textChild.localPosition = instantiationPosition.position;
                    }
                    if(imageObjectBaseName != null)
                    {
                        Transform imageChild = child.Find(child.name + imageObjectBaseName);
                        if (imageChild != null)
                        {
                            imageChild.gameObject.SetActive(Visibility);
                        }

                        if (verticalReposition)
                        {
                            Vector3 objectposition = imageChild.transform.position;
                            Vector3 newPosition = ObjectTransformations.OffsetPositionVectorByDistance(objectposition, verticalOffset.GetValueOrDefault(0.0f), "y");
                            imageChild.position = newPosition;
                            tagIsOffset = true;
                        }
                        else
                        {
                            HelpersExtensions.ObjectPositionInfo instantiationPosition = imageChild.GetComponent<HelpersExtensions.ObjectPositionInfo>();
                            imageChild.localPosition = instantiationPosition.position;
                            tagIsOffset = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("ARSpaceTextControler: InstantiateObjects script or Elements object not set.");
            }
        }
        public void ToggleObjectLengths(Toggle toggle)
        {
            /*
            * Method is used to toggle the object lengths in the scene.
            * Additionally it will calculate the object lengths and set the P1 & P2 elements based on the current step.
            */
            Debug.Log($"ToggleObjectLengths: Object Lengths Toggle set to {toggle.GetComponent<Toggle>().isOn}");
            if (ObjectLengthsUIPanelObjects != null && ObjectLengthsText != null && ObjectLengthsTags != null)
            {    
                if (toggle.isOn)
                {             
                    if (RobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        Vector3 offsetPosition = new Vector3(ObjectLengthsUIPanelPosition.x, ObjectLengthsUIPanelPosition.y - 300, ObjectLengthsUIPanelPosition.z);
                        ObjectLengthsUIPanelObjects.transform.localPosition = offsetPosition; 
                    }
                    else
                    {
                        ObjectLengthsUIPanelObjects.transform.localPosition = ObjectLengthsUIPanelPosition;
                    }
                    ObjectLengthsUIPanelObjects.SetActive(true);
                    ObjectLengthsTags.FindObject("P1Tag").SetActive(true);
                    ObjectLengthsTags.FindObject("P2Tag").SetActive(true);

                    if (CurrentStep != null)
                    {    
                        instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
                    }
                    else
                    {
                        Debug.LogWarning("ToggleObjectLengths: Current Step is null.");
                    }
                    UserInterface.SetUIObjectColor(ObjectLengthsToggleObject, Yellow);

                }
                else
                {
                    ObjectLengthsUIPanelObjects.SetActive(false);
                    ObjectLengthsTags.FindObject("P1Tag").SetActive(false);
                    ObjectLengthsTags.FindObject("P2Tag").SetActive(false);
                    UserInterface.SetUIObjectColor(ObjectLengthsToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleObjectLengths: Could not find Object Lengths Objects.");
            }
        }
        public void SetObjectLengthsText(float P1distance, float P2distance)
        {
            /*
            * Method is used to set the object lengths text in the scene.
            */
            ObjectLengthsText.text = $"P1 | {(float)Math.Round(P1distance, 2)}     P2 | {(float)Math.Round(P2distance, 2)}";
        }
        public void SetObjectLengthsTextFromStoredKey(string key)
        {
            /*
            * Method is used to set the object lengths text on the AR Canvas
            * based on the stored list information in the database.
            */
            List<float> objectLengths = databaseManager.ObjectLengthsDictionary[key];
            float p1Distance = (float)Math.Round(objectLengths[0], 2);
            float p2Distance = (float)Math.Round(objectLengths[1], 2);
            ObjectLengthsText.text = $"P1 | {p1Distance}     P2 | {p2Distance}";
        }
        public void ToggleRobot(Toggle toggle) //TODO: ADAPT FOR ROBOTIC TERRITORIES
        {
            /*
            * Method is used to toggle the robot in the scene.
            * Additionally it will set the visibility of the robot and the UI elements based on the toggle value,
            * and step actor.
            */
            Debug.Log($"ToggleRobot: Robot Toggle Pressed value set to {toggle.GetComponent<Toggle>().isOn}");
            if(toggle.isOn && RequestTrajectoryButtonObject != null)
            {
                if (ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
                {
                    Vector3 offsetPosition = new Vector3(ObjectLengthsUIPanelPosition.x, ObjectLengthsUIPanelPosition.y - 300, ObjectLengthsUIPanelPosition.z);
                    ObjectLengthsUIPanelObjects.transform.localPosition = offsetPosition; 
                }

                if(trajectoryVisualizer.ActiveRobot && CurrentStep != null)
                {
                    trajectoryVisualizer.ActiveRobot.SetActive(true);
                }
                else
                {
                    Debug.Log("ToggleRobot: Active Robot or CurrentStep is null not setting any visibility.");
                }

                RobotSelectionDropdownObject.SetActive(true);
                SetActiveRobotToggleObject.SetActive(true);
                if(CurrentStep != null)
                {
                    SetRoboticUIElementsFromKey(CurrentStep);
                }
                else
                {
                    Debug.LogWarning("ToggleRobot: Current Step is null.");
                }
                UserInterface.SetUIObjectColor(RobotToggleObject, Yellow);
            }
            else
            {            
                if (RequestTrajectoryButtonObject.activeSelf)
                {
                    TrajectoryServicesUIControler(false, false, false, false, false, false);
                }
                RobotSelectionDropdownObject.SetActive(false);
                SetActiveRobotToggleObject.SetActive(false);
                            
                if(trajectoryVisualizer.ActiveRobotObjects.transform.childCount > 0)
                {
                    if(trajectoryVisualizer.ActiveRobot.activeSelf)
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(false);
                    }
                    else if(trajectoryVisualizer.ActiveTrajectoryParentObject.activeSelf)
                    {
                        trajectoryVisualizer.ActiveTrajectoryParentObject.SetActive(false);
                    }
                }
                UserInterface.SetUIObjectColor(RobotToggleObject, White);
            }
        }
        public void SetRoboticUIElementsFromKey(string key)
        {
            /*
            * Method is used to set the robotic UI elements based on the key.
            * Additionally it will set the visibility of the robot and the UI elements based on the key,
            * and step actor.
            */
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            if(step.data.actor == "ROBOT")
            {
                if (!step.data.is_built && step.data.priority.ToString() == databaseManager.CurrentPriority)
                {    
                    TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    TrajectoryServicesUIControler(true, false, false, false, false, false);
                }
            }
            else
            {
                RobotSelectionDropdownObject.SetActive(true);
                SetActiveRobotToggleObject.SetActive(true);
                TrajectoryServicesUIControler(false, false, false, false, false, false);
            }
        }
        public void TogglePriority(Toggle toggle)
        {
            /*
            * Method is used to toggle the priority viewer in the scene.
            * Additionally it will set the visibility of the priority viewer UI elements and gameObjects based on the toggle value.
            */
            Debug.Log($"TogglePriority: Priority Toggle Pressed the value is now set to {toggle.GetComponent<Toggle>().isOn}");
            if(toggle.isOn && PriorityViewerToggleObject != null)
            {
                //Set the preview geometry slider to 1
                if(PreviewGeometrySlider.value != 1)
                {
                    PreviewGeometrySlider.value = 1;
                }
                PreviewGeometrySlider.interactable = false;
                ARSpaceTextControler(true, "PriorityText", ref PriorityTagIsOffset, "PriorityImage", IDToggleObject.GetComponent<Toggle>().isOn, 0.155f);
                PriorityViewerUIGraphicsController(true, databaseManager.CurrentPriority);
                SelectedPriority = databaseManager.CurrentPriority;
                instantiateObjects.ApplyColorBasedOnPriority(databaseManager.CurrentPriority);
                UserInterface.SetUIObjectColor(PriorityViewerToggleObject, Yellow);
            }
            else
            {
                
                PreviewGeometrySlider.interactable = true;
                instantiateObjects.ApplyColorBasedOnAppModes();
                SelectedPriority = "None";
                ARSpaceTextControler(false, "PriorityText", ref PriorityTagIsOffset, "PriorityImage");

                if(IDToggleObject.GetComponent<Toggle>().isOn && IDTagIsOffset)
                {
                    ARSpaceTextControler(true, "IdxText", ref IDTagIsOffset, "IdxImage");
                }
                PriorityViewerUIGraphicsController(false);
                UserInterface.SetUIObjectColor(PriorityViewerToggleObject, White);
            }
        }
        public void PriorityViewerUIGraphicsController(bool? isVisible, string selectedPrioritytext=null)
        {
            /*
            * Method is used to control the updating of the priority viewer UI elements.
            */
            if(isVisible.HasValue)
            {
                NextPriorityButtonObject.SetActive(isVisible.Value);
                PreviousPriorityButtonObject.SetActive(isVisible.Value);
                SelectedPriorityTextObject.SetActive(isVisible.Value);
                PriorityViewerBackground.SetActive(isVisible.Value);
            }
            if(selectedPrioritytext != null)
            {
                SelectedPriorityText.text = selectedPrioritytext;
            }
        }
        public void SetNextPriorityGroup()
        {
            /*
            * Method is used to set the next priority group in the priority viewer.
            * Additionally it will color the elements of the next priority group and set the text based on the next priority group.
            */
            Debug.Log("SetNextPriorityGroup: Next Priority Button Pressed");
            if(SelectedPriority != "None")
            {
                int SelectedPriorityInt = Convert.ToInt16(SelectedPriority);
                int newPriorityGroupInt = SelectedPriorityInt + 1;

                if(newPriorityGroupInt <= databaseManager.BuildingPlanDataItem.PriorityTreeDictionary.Count - 1)
                {                
                    instantiateObjects.ApplyColortoPriorityGroup(SelectedPriorityInt.ToString(), newPriorityGroupInt.ToString());
                    instantiateObjects.ApplyColortoPriorityGroup(newPriorityGroupInt.ToString(), newPriorityGroupInt.ToString(), true);
                    PriorityViewerUIGraphicsController(true, newPriorityGroupInt.ToString());
                    SelectedPriority = (SelectedPriorityInt + 1).ToString();
                }
                else
                {
                    Debug.Log("SetNextPriorityGroup: We have reached the priority groups limit.");
                }
            }
            else
            {
                Debug.LogWarning("SetNextPriorityGroup: Selected Priority is null.");
            }
        }
        public void SetPreviousPriorityGroup()
        {
            /*
            * Method is used to set the previous priority group in the priority viewer.
            * Additionally it will color the elements of the previous priority group and set the text based on the previous priority group.
            */
            Debug.Log("SetPreviousPriorityGroup: setting the previous Priority group");
            if(SelectedPriority != "None")
            {
                int SelectedPriorityInt = Convert.ToInt16(SelectedPriority);
                int newPriorityGroupInt = SelectedPriorityInt - 1;

                if(newPriorityGroupInt >= 0)
                {
                    instantiateObjects.ApplyColortoPriorityGroup(SelectedPriorityInt.ToString(), newPriorityGroupInt.ToString());
                    instantiateObjects.ApplyColortoPriorityGroup(newPriorityGroupInt.ToString(), newPriorityGroupInt.ToString(), true);
                    PriorityViewerUIGraphicsController(true, newPriorityGroupInt.ToString());
                    SelectedPriority = (SelectedPriorityInt - 1).ToString();
                }
                else
                {
                    Debug.Log("SetPreviousPriorityGroup: We have reached the zero priority group.");
                }
            }
            else
            {
                Debug.LogWarning("SetPreviousPriorityGroup: Selected Priority is null.");
            }
        }
        public void ToggleScrollSearch(Toggle toggle)
        {
            /*
            * Method is used to toggle the scroll search in the scene.
            * Additionally it will set the visibility of the scroll search UI elements based on the toggle value.
            */
            if (toggle.isOn)
            {             
                ScrollSearchObjects.SetActive(true);
                scrollSearchManager.CreateCellsFromPrefab(ref scrollSearchManager.cellPrefab, scrollSearchManager.cellSpacing, scrollSearchManager.cellsParent, databaseManager.BuildingPlanDataItem.steps.Count, ref scrollSearchManager.cellsExist);
                UserInterface.SetUIObjectColor(ScrollSearchToggleObject, Yellow);
            }
            else
            {
                ScrollSearchObjects.SetActive(false);
                scrollSearchManager.ResetScrollSearch(ref scrollSearchManager.cellsExist);
                UserInterface.SetUIObjectColor(ScrollSearchToggleObject, White);
            }
        }

        ////////////////////////////////////////// Menu Buttons ///////////////////////////////////////////////////
        private void ToggleInfo(Toggle toggle)
        {
            /*
            * Method is used to toggle the information panel in the scene.
            */
            if(InfoPanelObject != null)
            {
                Debug.Log($"ToggleInfo: Info Toggle Pressed value is now set to {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
                    {
                        CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    InfoPanelObject.SetActive(true);
                    UserInterface.SetUIObjectColor(InfoToggleObject, Yellow);
                }
                else
                {
                    InfoPanelObject.SetActive(false);
                    UserInterface.SetUIObjectColor(InfoToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleInfo: Could not find Info Panel.");
            }
        }
        private void ToggleCommunication(Toggle toggle)
        {
            /*
            * Method is used to toggle the communication panel in the scene.
            */
            if(CommunicationPanelObject != null)
            {
                Debug.Log($"ToggleCommunication: Communication Toggle Pressed value is now {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    if(InfoToggleObject.GetComponent<Toggle>().isOn)
                    {
                        InfoToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    CommunicationPanelObject.SetActive(true);
                    UpdateConnectionStatusText(MqttConnectionStatusObject, mqttTrajectoryManager.mqttClientConnected);
                    UpdateConnectionStatusText(RosConnectionStatusObject, rosConnectionManager.IsConnectedToRos);
                    UserInterface.SetUIObjectColor(CommunicationToggleObject, Yellow);
                }
                else
                {
                    if(MqttUpdateConnectionMessage.activeSelf)
                    {
                        MqttUpdateConnectionMessage.SetActive(false);
                    }
                    
                    CommunicationPanelObject.SetActive(false);
                    UserInterface.SetUIObjectColor(CommunicationToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleCommunication: Could not find Communication Panel.");
            }
        }
        private void ReloadApplication()
        {
            /*
            * Method is used to reload the application.
            * This method will reset all current information and then pull all information again.
            */
            Debug.Log("ReloadApplication: Attempting to reload all information from the database");
            databaseManager.RemoveListners();
            if (Elements.transform.childCount > 0)
            {
                foreach (Transform child in Elements.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            if (QRMarkers.transform.childCount > 0)
            {        
                foreach (Transform child in QRMarkers.transform)
                {
                    child.transform.position = Vector3.zero;
                    child.transform.rotation = Quaternion.identity;
                }
            }

            if (UserObjects.transform.childCount > 0)
            {
                foreach (Transform child in UserObjects.transform)
                {
                    Destroy(child.gameObject);
                }
            }        

            databaseManager.BuildingPlanDataItem.steps.Clear();
            databaseManager.AssemblyDataDict.Clear();
            databaseManager.QRCodeDataDict.Clear();
            databaseManager.UserCurrentStepDict.Clear();
            databaseManager.BuildingPlanDataItem.PriorityTreeDictionary.Clear();
            databaseManager.ObjectLengthsDictionary.Clear();

            mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();
            mqttTrajectoryManager.RemoveConnectionEventListners();

            databaseManager.FetchSettingsData(eventManager.dbReferenceSettings);
            mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();

        }
        public void ToggleEditor(Toggle toggle)
        {
            /*
            * Method is used to toggle the editor panel in the scene.
            * Additionally it toggles touch input for objects in space.
            */
            if (EditorBackground != null && BuilderEditorButtonObject != null && BuildStatusButtonObject != null)
            {    
                Debug.Log($"ToggleEditor: Editor Toggle Pressed Value now set to {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    EditorBackground.SetActive(true);
                    BuilderEditorButtonObject.SetActive(true);
                    BuildStatusButtonObject.SetActive(true);
                    CurrentStepTextObject.SetActive(false);
                    EditorSelectedTextObject.SetActive(true);

                    TouchSearchModeController(TouchMode.ElementEditSelection);
                    UserInterface.SetUIObjectColor(EditorToggleObject, Yellow);
                }
                else
                {
                    EditorBackground.SetActive(false);
                    BuilderEditorButtonObject.SetActive(false);
                    BuildStatusButtonObject.SetActive(false);
                    EditorSelectedTextObject.SetActive(false);
                    CurrentStepTextObject.SetActive(true);

                    TouchSearchModeController(TouchMode.None);

                    if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.ActorView)
                    {
                        instantiateObjects.ApplyColorBasedOnActor();
                    }
                    else if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.BuiltUnbuilt)
                    {
                        instantiateObjects.ApplyColorBasedOnBuildState();
                    }
                    else if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                    {
                        instantiateObjects.ApplyColorBasedOnPriority(SelectedPriority);
                    }
                    else
                    {
                        Debug.LogWarning("ToggleEditor: Could not find Visulization Mode.");
                    }
                    UserInterface.SetUIObjectColor(EditorToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleEditor: Could not find one of the buttons in the Editor Menu.");
            }
        }
        public void ToggleAROcclusion(Toggle toggle)
        {
            /*
            * Method is used to toggle the AR Occlusion in the scene.
            */
            if (OcclusionToggleObject != null && occlusionManager != null)
            {
                Debug.Log("ToggleAROcclusion: Occlusion Toggle Pressed value is now set to " + toggle.GetComponent<Toggle>().isOn);
                if (toggle.isOn)
                { 
                    occlusionManager.enabled = true;            
                    UserInterface.SetUIObjectColor(OcclusionToggleObject, Yellow);
                }
                else
                {
                    occlusionManager.enabled = false;
                    UserInterface.SetUIObjectColor(OcclusionToggleObject, White);            
                }
            }
            else
            {
                Debug.LogWarning("ToggleAROcclusion: Could not find Occlusion Toggle Object.");
            }
        }

        ////////////////////////////////////////// Editor Buttons /////////////////////////////////////////////////
        public void TouchSearchModeController(TouchMode modetype)
        {
            /*
            * Method is used to control the touch search mode in the scene.
            * it will set the touch mode based on the input mode type.
            */

            instantiateObjects.visulizationController.TouchMode = modetype;

            if (modetype == TouchMode.ElementEditSelection)
            {
                Debug.Log ("***TouchMode: ELEMENT EDIT MODE***");
            }
            else if(modetype == TouchMode.None)
            {
                DestroyBoundingBoxFixElementColor();
                activeGameObject = null;
                Debug.Log ("***TouchMode: NONE***");
            }
            else
            {
                Debug.LogWarning("TouchSearchModeController: Could not find Touch Mode.");
            }

        }
        private void TouchSearchControler()
        {
            /*
            Touch search controler is used to control the touch input modes for the application.
            */
            
            if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
            {
                SearchInput();
            }
        }
        private void ColliderControler()
        {
            /*
            * Collider controler is used to control the collider for the elements in the scene.
            * It will enable or disable the collider based on the current step.
            * Additionally it will color elements that are not suppose to be interacted with.
            */
            Step Currentstep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
            for (int i =0 ; i < databaseManager.BuildingPlanDataItem.steps.Count; i++)
            {
                Step step = databaseManager.BuildingPlanDataItem.steps[i.ToString()];
                GameObject element = Elements.FindObject(i.ToString()).FindObject(step.data.element_ids[0] + " Geometry");
                Collider ElementCollider = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Collider>();
                Renderer ElementRenderer = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Renderer>();

                if(ElementCollider != null)
                {
                    if(step.data.priority == Currentstep.data.priority)
                    {
                        ElementCollider.enabled = true;
                    }
                    else
                    {
                        ElementCollider.enabled = false;
                        ElementRenderer.material = instantiateObjects.LockedObjectMaterial;
                    }
                }
            }
        }
        private GameObject SelectedObject(GameObject activeGameObject = null)
        {
            /*
            * Selected object is used to find the selected object in the scene.
            * It will allow you to select an object both in the editor and on the device.
            */
            if (Application.isEditor)
            {
                Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    if (hitObject.collider.tag != "plane")
                    {
                        activeGameObject = hitObject.collider.gameObject;
                        Debug.Log($"SelectedObject: hit object is {activeGameObject.name}");
                    }
                }
            }
            else
            {
                Touch touch = Input.GetTouch(0);
                if (Input.touchCount == 1 && touch.phase == TouchPhase.Ended)
                {
                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    rayManager.Raycast(touch.position, hits);
                    if (hits.Count > 0)
                    {
                        Ray ray = arCamera.ScreenPointToRay(touch.position);
                        RaycastHit hitObject;
                        if (Physics.Raycast(ray, out hitObject))
                        {
                            if (hitObject.collider.tag != "plane")
                            {
                                activeGameObject = hitObject.collider.gameObject;
                                Debug.Log($"SelectedObject: hit object is {activeGameObject.name}");
                            }
                        }
                    }
                }
            }
            return activeGameObject;
        }
        private void SearchInput()
        {
            /*
            * Search input is used to control the search input for the application.
            * It will allow you to search for objects in the scene and select them.
            * Additionally it is configured to work in both the editor and on the device.
            */
            if (Application.isEditor)
            {   
                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
                    {
                        Debug.Log("*** ELEMENT SELECTION MODE : Editor ***");
                        EditMode();
                    }

                    else
                    {
                        Debug.Log("Press a button to initialize a mode");
                    }
                }
            }
            else
            {
                SearchTouch();
            }
        }
        private void SearchTouch()
        {
            /*
            * Search touch is used to control the search touch for the application.
            * It allows for the touch selection of objects in space.
            */
            if (Input.touchCount > 0)         
            {
                if (PhysicRayCastBlockedByUi(Input.GetTouch(0).position))
                {
                    if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
                    {
                        Debug.Log("*** SearchTouch: ELEMENT SELECTION MODE: Touch ***");
                        EditMode();                     
                    }
                    else
                    {
                        Debug.Log("SearchTouch: Press a button to initialize a mode");
                    }
                }
            }
        }
        private bool PhysicRayCastBlockedByUi(Vector2 touchPosition)
        {
            /*
            * Physics ray cast blocked by UI is used to check if the physics ray cast is blocked by the UI.
            * It will return a boolean value based on the touch position.
            */
            if (HelpersExtensions.IsPointerOverUIObject(touchPosition))
            {
                return false;
            }
            return true;
        }
        private void EditMode()
        {
            /*
            * Edit mode is used to control the edit mode for the application.
            * It will allow you to touch select any object in the screen.
            */
            activeGameObject = SelectedObject();
            
            if (Input.touchCount == 1)
            {
                activeGameObject = SelectedObject();
            }

            if (activeGameObject != null)
            {
                EditorSelectedText.text = activeGameObject.transform.parent.name;
                temporaryObject = activeGameObject;
                string activeGameObjectParentname = activeGameObject.transform.parent.name;
                instantiateObjects.ColorHumanOrRobot(databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.actor, databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.is_built, activeGameObject);
                addBoundingBox(temporaryObject);
            }
            else
            {
                if (GameObject.Find("BoundingArea") != null)
                {
                    DestroyBoundingBoxFixElementColor();
                }
            }
        }
        private void addBoundingBox(GameObject gameObj)
        {
            /*
            * Add bounding box is used to add a bounding box to the selected object.
            * It will create a bounding box around the object and color the object based on the object.
            */
            DestroyBoundingBoxFixElementColor();

            GameObject boundingArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boundingArea.name = "BoundingArea";
            boundingArea.GetComponent<Renderer>().material = instantiateObjects.HumanUnbuiltMaterial;
            
            Collider collider = gameObj.GetComponent<Collider>();
            Vector3 center = collider.bounds.center;
            float radius = collider.bounds.extents.magnitude;

            if (boundingArea.GetComponent<Rigidbody>() != null)
            {
                Destroy(boundingArea.GetComponent<BoxCollider>());
            }
            if (boundingArea.GetComponent<Collider>() != null)
            {
                Destroy(boundingArea.GetComponent<BoxCollider>());
            }

            boundingArea.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
            boundingArea.transform.localPosition = center;
            boundingArea.transform.rotation = gameObj.transform.rotation;

            var stepParent = gameObj.transform.parent;
            boundingArea.transform.SetParent(stepParent);
        }
        private void DestroyBoundingBoxFixElementColor()
        {
            /*
            * Destroy bounding box fix element color is used to destroy the bounding box and fix the element color.
            */
            if (GameObject.Find("BoundingArea") != null)
            {
                GameObject Box = GameObject.Find("BoundingArea");
                var element = Box.transform.parent;
                GameObject elementGameobject = Box.transform.parent.gameObject;

                if (element != null && elementGameobject != null)
                {
                    if (CurrentStep != null)
                    {
                        if (element.name != CurrentStep)
                        {
                            Step step = databaseManager.BuildingPlanDataItem.steps[element.name];
                            if(step != null)
                            {
                                instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, step, element.name, elementGameobject.FindObject(step.data.element_ids[0] + " Geometry"));                        
                            }
                            else
                            {
                                Debug.LogWarning("DestroyBoundingBoxFixElementColor: Fix Element Color: Step is null.");
                            }                        
                        }
                    }

                }

                Destroy(GameObject.Find("BoundingArea"));
            }

        }
        public void ModifyStepActor(string key)
        {
            /*
            * Modify step actor is used to modify the actor of the step.
            * It will change the actor from human to robot or robot to human.
            */
            Debug.Log($"ModifyStepActor: Modifying Actor of: {key}");
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            if(step.data.actor == "HUMAN")
            {
                step.data.actor = "ROBOT";
            }
            else
            {
                step.data.actor = "HUMAN";
            }

            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
            databaseManager.PushAllDataBuildingPlan(key);
        }
        private void TouchModifyBuildStatus()
        {
            /*
            * Touch modify build status is used to modify the build status of the step.
            * It will change the build status from built to unbuilt or unbuilt to built.
            */
            Debug.Log("TouchModifyBuildStatus: Build Status Button Pressed");
            if (activeGameObject != null)
            {
                ModifyStepBuildStatus(activeGameObject.transform.parent.name);
            }
        }
        private void TouchModifyActor()
        {
            /*
            * Touch modify actor is used to modify the actor of the step.
            * It will change the actor from human to robot or robot to human.
            */
            Debug.Log("TouchModifyActor: Actor Modifier Button Pressed");
            if (activeGameObject != null)
            {
                ModifyStepActor(activeGameObject.transform.parent.name);
            }
        }
    }

    public static class UserInterface
    {
        public static void SetUIObjectColor(GameObject Button, Color color)
        {
            /*
            * Set UI Object Color is used to set the color of the UI object.
            */
            Button.GetComponent<UnityEngine.UI.Image>().color = color;
        }
        public static void FindButtonandSetOnClickAction(GameObject searchObject, ref GameObject buttonParentObjectReference, string unityObjectName, UnityAction customAction)
        {
            /*
            * Find Button and Set On Click Action is used to find the button and set the on click action.
            * This method is used throughout the CompasXR Application, and serves as a simple way to set on click actions.
            */
            if (searchObject != null)
            {
                buttonParentObjectReference = searchObject.FindObject(unityObjectName);
                Button buttonComponent = buttonParentObjectReference.GetComponent<Button>();
                buttonComponent.onClick.AddListener(customAction);
            }
            else
            {
                Debug.LogError($"FindButtonandSetOnClickAction: Could not Set OnClick Action because search object is null for {unityObjectName}");
            }
        }

        public static void FindButtonandSetOnClickActionDebug(
        GameObject searchObject,
        ref GameObject buttonParentObjectReference,
        string unityObjectName,
        UnityAction customAction)
        {
            if (searchObject == null)
            {
                Debug.LogError($"FindButtonandSetOnClickAction: searchObject is null (looking for '{unityObjectName}')");
                return;
            }

            buttonParentObjectReference = searchObject.FindObject(unityObjectName);

            if (buttonParentObjectReference == null)
            {
                Debug.LogError(
                    $"FindButtonandSetOnClickAction: Could not find '{unityObjectName}' under '{GetHierarchyPath(searchObject.transform)}'. " +
                    $"Check the exact name/casing or the parent you’re searching within.");
                return;
            }

            var buttonComponent = buttonParentObjectReference.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError(
                    $"FindButtonandSetOnClickAction: '{GetHierarchyPath(buttonParentObjectReference.transform)}' exists but has no <Button> component.");
                return;
            }

            if (customAction == null)
            {
                Debug.LogWarning($"FindButtonandSetOnClickAction: customAction is null for '{unityObjectName}'.");
                return;
            }

            // Optional: prevent duplicate listeners if this runs more than once
            // buttonComponent.onClick.RemoveAllListeners();

            buttonComponent.onClick.AddListener(customAction);
        }

        // Small debug helper
        private static string GetHierarchyPath(Transform t)
        {
            var path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
        public static void FindToggleandSetOnValueChangedAction(GameObject searchObject, ref GameObject toggleParentObjectReference, string unityObjectName, UnityAction<Toggle> customAction)
        {
            /*
            * Find Toggle and Set On Value Changed Action is used to find the toggle and set the on value changed action.
            * This method is used throughout the CompasXR Application, and serves as a simple way to set on value changed actions.
            */
            if (searchObject != null)
            {
                toggleParentObjectReference = searchObject.FindObject(unityObjectName);
                Toggle toggleComponent = toggleParentObjectReference.GetComponent<Toggle>();
                toggleComponent.onValueChanged.AddListener(value => customAction(toggleComponent));
            }
            else
            {
                Debug.LogError($"Toggle Constructer: Could not Set OnValueChanged Action because search object is null for {unityObjectName}");
            }
        }
        public static void FindSliderandSetOnValueChangeAction(GameObject searchObject, ref GameObject sliderParentObjectReference, ref Slider sliderObjectReference, string unityObjectName, UnityAction<float> customAction)
        {
            /*
            * Find Slider and Set On Value Change Action is used to find the slider and set the on value change action.
            * This method is used throughout the CompasXR Application, and serves as a simple way to set on value change actions.
            */
            if (searchObject != null)
            {
                sliderParentObjectReference = searchObject.FindObject(unityObjectName);
                sliderObjectReference = sliderParentObjectReference.GetComponent<Slider>();
                sliderObjectReference.onValueChanged.AddListener(customAction);
            }
            else
            {
                Debug.LogError($"Slider Constructer: Could not Set OnValueChanged Action because search object is null for {unityObjectName}");
            }
        }
        public static void PrintStringOnClick(string Text)
        {
            /*
            * Print String On Click is used to print a string to the console from a button.
            */
            Debug.Log(Text);
        }
        public static List<TMP_Dropdown.OptionData> SetDropDownOptionsFromStringList(TMP_Dropdown dropDown, List<string> stringList)
        {
            /*
            * Set Drop Down Options From String List is used to set the add a list to a drop down item.
            */
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string stringItem in stringList)
            {
                options.Add(new TMP_Dropdown.OptionData(stringItem));
            }
            return options;
        }
        public static TMP_Dropdown.OptionData AddOptionDataToDropdown(string option, TMP_Dropdown dropDown)
        {
            /*
            * Add Option Data To Dropdown is used to add an option to a drop down UI item.
            */
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(option);
            dropDown.options.Add(newOption);
            return newOption;
        }
        public static void SignalOnScreenMessageFromPrefab(ref GameObject prefabReference, ref GameObject messageObjectReference, string activeMessageGameObjectName, GameObject activeMessageParent, string message, string logMessageName)
        {
            /*
            * Signal On Screen Message From Prefab is used to signal an on screen message from a prefab.
            * This method is used to create a message from a prefab and set the message text.
            * Additionally it is dependent on the structure of the prefab.
            */
            Debug.Log($"SignalOnScreenMessageFromPrefab: {logMessageName}: Signal On Screen Message.");
            if (messageObjectReference == null)
            {
                messageObjectReference = GameObject.Instantiate(prefabReference);
                messageObjectReference.transform.SetParent(activeMessageParent.transform, false);
                messageObjectReference.name = activeMessageGameObjectName;
            }
            TMP_Text messageTextComponent = messageObjectReference.FindObject("MessageText").GetComponent<TMP_Text>();

            //TODO: THIS WAS UPDATED FOR ROBOTIC TERRITORIES.
            if (messageTextComponent != null && message != null && messageObjectReference != null && messageObjectReference.activeSelf == false)
            {
                SignalOnScreenMessageWithButton(messageObjectReference, messageTextComponent, message);
            }
            else if (messageObjectReference != null && messageObjectReference.activeSelf == true)
            {
                Debug.LogWarning($"SignalOnScreenMessageFromPrefab: {logMessageName}: Message is already active.");
            }
            else
            {
                Debug.LogWarning($"SignalOnScreenMessageFromPrefab: {logMessageName}: Could not find message object or message component.");
            }
        }
        public static void CreateCenterAlignedSelfDestructiveMessageInstance(string messageGameObjectName, float messageHeight, float messageWidth, Color messagePanelColor, TextAlignmentOptions textAlignment, float textBoarderOffset, Color textColor, string message, float buttonHeight, float buttonWidth, Color buttonColor, float buttonTextBoarderOffset, string buttonText, Color buttonTextColor)
        {
            /*
            * Create an instance of an On Screen Message with a button that will destroy itself when clicked.
            * This instance is used for messages in the library where
            * you cannot find instances of message objects in other classes.
            */

            GameObject newCanvas = new GameObject($"{messageGameObjectName}Canvas");
            Canvas canvas = newCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.AddComponent<CanvasScaler>();
            newCanvas.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject($"{messageGameObjectName}Panel");
            panel.AddComponent<CanvasRenderer>();
            UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = messagePanelColor;
            panel.transform.SetParent(newCanvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(messageWidth, messageHeight);
            panelRect.anchoredPosition = new Vector2(0, 0);

            GameObject textObject = new GameObject($"{messageGameObjectName}Text");
            textObject.transform.SetParent(newCanvas.transform, false);
            TMPro.TextMeshProUGUI messageText = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            RectTransform textRectObject = textObject.GetComponent<RectTransform>();
            float textWidth = messageWidth - textBoarderOffset * 2;
            float textHeight = messageHeight - textBoarderOffset * 3 - buttonHeight;
            textRectObject.sizeDelta = new Vector2(textWidth, textHeight);

            GameObject randomItems = GameObject.Instantiate(textObject, newCanvas.transform, false);
            textRectObject.anchoredPosition = new Vector2(0, (textBoarderOffset * 2 + buttonHeight) / 2);

            messageText.alignment = textAlignment;
            messageText.color = textColor;
            messageText.text = message;
            messageText.enableAutoSizing = true;
            messageText.fontSizeMin = 1;
            messageText.fontSizeMax = 100;

            GameObject buttonObject = new GameObject($"{messageGameObjectName}Button");
            buttonObject.transform.SetParent(newCanvas.transform, false);
            float buttonYLocation = (messageHeight / 2 - textBoarderOffset - buttonHeight / 2) * -1;
            RectTransform buttonRectObject = buttonObject.AddComponent<RectTransform>();
            buttonRectObject.anchoredPosition = new Vector2(0, buttonYLocation);
            buttonRectObject.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            Button buttonComponent = buttonObject.AddComponent<Button>();
            UnityEngine.UI.Image buttonImage = buttonObject.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = buttonColor;

            GameObject buttonTextObject = new GameObject($"{messageGameObjectName}ButtonText");
            buttonTextObject.transform.SetParent(buttonObject.transform, false);
            TMPro.TextMeshProUGUI buttonTextComponent = buttonTextObject.AddComponent<TMPro.TextMeshProUGUI>();
            RectTransform buttontextRectObject = buttonTextComponent.GetComponent<RectTransform>();
            buttontextRectObject.anchoredPosition = new Vector2(0, 0);
            buttontextRectObject.sizeDelta = new Vector2(buttonWidth - buttonTextBoarderOffset, buttonHeight - buttonTextBoarderOffset);

            buttonTextComponent.enableAutoSizing = true;
            buttonTextComponent.fontSizeMin = 1;
            buttonTextComponent.fontSizeMax = 100;
            buttonTextComponent.text = buttonText;
            buttonTextComponent.color = buttonTextColor;
            buttonTextComponent.alignment = TextAlignmentOptions.Center;

            buttonComponent.onClick.AddListener(() => GameObject.Destroy(newCanvas));
        }
        public static void SignalOnScreenMessageFromReference(ref GameObject messageObjectReference, string message, string logMessageName)
        {
            /*
            * Signal On Screen Message From Reference is used to signal an on screen message from a reference.
            * This method is used to create a message from a reference and set the message text.
            * Additionally it is dependent on the structure of the prefab.
            */
            Debug.Log($"SignalOnScreenMessageFromReference: {logMessageName}: Signal On Screen Message.");
            TMP_Text messageTextComponent = messageObjectReference.FindObject("MessageText").GetComponent<TMP_Text>();

            if (messageTextComponent != null && message != null && messageObjectReference != null)
            {
                SignalOnScreenMessageWithButton(messageObjectReference, messageTextComponent, message);
            }
            else
            {
                Debug.LogWarning($"SignalOnScreenMessageFromReference: {logMessageName}: Could not find message object or message component.");
            }
        }
        public static void SignalOnScreenMessageWithButton(GameObject messageGameObject, TMP_Text messageComponent = null, string message = "None")
        {
            /*
            * Signal On Screen Message With Button is used to signal an on screen message with a button.
            * This method is used to set the message text and activate the message object.
            * Additionally it is dependent on the structure of the prefab.
            */
            if (messageGameObject != null)
            {
                if (message != "None" && messageComponent != null)
                {
                    messageComponent.text = message;
                }
                messageGameObject.SetActive(true);
                GameObject AcknowledgeButton = messageGameObject.FindObject("AcknowledgeButton");

                if (AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() == 0)
                {
                    AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => messageGameObject.SetActive(false));
                }
            }
            else
            {
                Debug.LogWarning($"Message: Could not find message object or message component inside of GameObject {messageGameObject.name}.");
            }
        }

    }
}

