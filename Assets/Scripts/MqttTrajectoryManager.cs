using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using CompasXR.Core;
using CompasXR.UI;
using CompasXR.Robots.MqttData;
using CompasXR.Robots.MqttData.RoboticTerritories;
using CompasXR.RoboticTerritories.Data;
using System.IO;
using CompasXR.Core.Extentions;

namespace CompasXR.Robots
{
    /*
    * CompasXR.Robots : Is the namespace for all Classes that
    * controll the primary functionalities releated to the use of robots in the CompasXR Application.
    * Functionalities, such as robot communication, robot visualization, and robot interaction.
    */
    public class MqttTrajectoryManager : M2MqttUnityClient
    {
        /*
        * MqttTrajectoryManager : Class is used to manage the MQTT connection and configuration settings.
        * Additionally it is designed to handle the MQTT message events, and allow users to subscribe custom topics
        * The MQTTTrajectory manager manages all responses and conditions based on received messges for trajectory visualization.
        */
        [Header("MQTT Settings")]
        [Tooltip("Set the topic to publish")]
        public string controllerName = "MQTT Trajectory Controller";
        private string m_msg;
        public string msg
        {
            get { return m_msg; }
            set
            {
                if (m_msg == value) return;
                m_msg = value;
                OnMessageArrived?.Invoke(m_msg);
            }
        }
        public event OnMessageArrivedDelegate OnMessageArrived;
        public delegate void OnMessageArrivedDelegate(string newMsg);
        private List<string> eventMessages = new List<string>();
        public CompasXRTopics compasXRTopics;
        public ServiceManager serviceManager = new ServiceManager();
        public UIFunctionalities UIFunctionalities;
        public DatabaseManager databaseManager;
        public TrajectoryVisualizer trajectoryVisualizer;

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////

        public RoboticTerritoriesTopics roboticTerritoriesTopics;
        public GameObject mqttConnectionMessagePannel;
        public GameObject mqttConnectionFailedTextObjects;

        //TODO: Realtime mimic testing Pick or Place Watching........

        public RealtimeMimicPickandPlaceManager realtimeMimicPickandPlaceManager = new RealtimeMimicPickandPlaceManager();

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////// Monobehaviour Methods ////////////////////////////////////////////
        protected override void Start()
        {
            mqttConnectionMessagePannel = GameObject.Find("MQTTConnectingScreen");
            mqttConnectionFailedTextObjects = mqttConnectionMessagePannel.FindObject("FailedTextObjects");
            base.Start();
            // OnStartorRestartInitilization();
            OnStartorRestartInitilizationRoboticTerritories();
        }
        protected override void Update()
        {
            base.Update();
        }
        public void OnDestroy()
        {
            UnsubscribeFromRoboticTerritoriesTopics();
            RemoveConnectionEventListnersRoboticTerritories();
            Disconnect();
        }

        //////////////////////////////////////////// General Methods ////////////////////////////////////////////
        public void OnStartorRestartInitilization(bool Restart = false)
        {
            /*
            * Method is used to initialize the MQTT connection, find dependencies, and add listners
            * for subscriptions on connected.
            */
            if (!Restart)
            {
                UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
                databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
                trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();
            }
            Connect();
            AddConnectionEventListners();
        }

        //////////////////////////////////////////// Connection Managers ////////////////////////////////////////

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////
        public void OnStartorRestartInitilizationRoboticTerritories(bool Restart = false)
        {
            /*
            * Method is used to initialize the MQTT connection, find dependencies, and add listners
            * for subscriptions on connected.
            */
            if (!Restart)
            {
                UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
                databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
                trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();
            }
            Connect();
            AddConnectionEventListnersRoboticTerritories();
        }
        public void SetRoboticTerritoriesTopics(object source, ApplicationSettingsEventArgs e)
        {
            /*
            * Method is used to set the custom Compas XR Topics based on the Application Settings.
            * format: compas_xr/project_name/message_name
            */
            roboticTerritoriesTopics = new RoboticTerritoriesTopics(e.Settings.project_name);
        }

        public void AddConnectionEventListnersRoboticTerritories()
        {
            /*
            * Method is used to add event listners for the MQTT connection options.
            */
            ConnectionSucceeded += SubscribeToRoboticTerritoriesTopics;
            ConnectionFailed += UIFunctionalities.SignalMQTTConnectionFailed;
        }
        public void RemoveConnectionEventListnersRoboticTerritories()
        {
            /*
            * Method is used to remove event listners for the MQTT connection options.
            */
            ConnectionSucceeded -= SubscribeToRoboticTerritoriesTopics;
            ConnectionFailed -= UIFunctionalities.SignalMQTTConnectionFailed;
        }
        public void SubscribeToRoboticTerritoriesTopics()
        {
            /*
            * Method is used to subscribe to the custom Compas XR Topics.
            */
            Debug.Log("MQTT: SubscribeToRoboticTerritoriesTopics: Subscribing to Robotic Territories Topics");
            SubscribeToTopic(roboticTerritoriesTopics.subscribers.mimicResultTopic);
            SubscribeToTopic(roboticTerritoriesTopics.subscribers.realtimeMimicResultTopic);
            SubscribeToTopic(roboticTerritoriesTopics.subscribers.inferenceResultTopic);
            SubscribeToTopic(roboticTerritoriesTopics.subscribers.inferencePostInferenceTargetTrajectoryResultTopic);
        }
        public void UnsubscribeFromRoboticTerritoriesTopics()
        {
            /*
            * Method is used to unsubscribe from the custom Compas XR Topics.
            */
            UnsubscribeFromTopic(roboticTerritoriesTopics.subscribers.mimicResultTopic);
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            /*
            * Method is used to decode the message received from the MQTT broker.
            * The method will decode the message and call the appropriate message handler based on the topic.
            */
            msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("MQTT: DecodeMessage: Received: " + msg + " from topic: " + topic);
            RoboticTerritoriesIncomingMessageHandler(topic, msg);
            // CompasXRIncomingMessageHandler(topic, msg);
            StoreMessage(msg);
        }
        public void RoboticTerritoriesIncomingMessageHandler(string topic, string message)
        {
            /*
            * Method is used to handle the incoming messages from the MQTT broker based on the topic.
            */
            if (topic == roboticTerritoriesTopics.subscribers.mimicResultTopic)
            {
                Debug.Log("MQTT: MimicResult Message Handeling");
                MimicTrajectoryResultMessage mimicResultMessage = MimicTrajectoryResultMessage.Parse(message);
                MimicResultReceivedMessageHandler(mimicResultMessage);
            }
            else if (topic == roboticTerritoriesTopics.subscribers.realtimeMimicResultTopic)
            {
                //TODO: Implement Realtime Mimic Result Message Handler (Needs to find the object and delete it if it existis in the scene)
                Debug.Log("MQTT: RealtimeMimicResult Message Handeling");
                RealtimeMimicResultMessage realtimeMimicResultMessage = RealtimeMimicResultMessage.Parse(message);
                RealtimeMimicResultHandler(realtimeMimicResultMessage);
            }
            else if (topic == roboticTerritoriesTopics.subscribers.inferenceResultTopic)
            {
                //TODO: Implement Inference Result Message Handler (Needs to find the object and delete it if it existis in the scene)
                Debug.Log("MQTT: InferenceResult Message Handeling");
                InferenceResultMessage inferenceResultMessage = InferenceResultMessage.Parse(message);
                InferenceResultReceivedMessageHandler(inferenceResultMessage);
                Debug.Log($"MQTT: InferenceResult Message Handeling: Received {JsonConvert.SerializeObject(inferenceResultMessage)}");
            }
            else if (topic == roboticTerritoriesTopics.subscribers.inferencePostInferenceTargetTrajectoryResultTopic)
            {
                PostInferenceTrajectoryResultMessage postInferenceTrajectoryResultMessage = PostInferenceTrajectoryResultMessage.Parse(message);
                PostInferenceTargetTrajectoryResultReceivedMessageHandler(postInferenceTrajectoryResultMessage);
                Debug.Log($"MQTT: InferencePostInferenceTargetTrajectoryResult Message Handeling {JsonConvert.SerializeObject(postInferenceTrajectoryResultMessage)}");
            }
            else
            {
                Debug.LogWarning("MQTT: No message handler for topic: " + topic);
            }
        }

        public void RealtimeMimicResultHandler(RealtimeMimicResultMessage realtimeMimicResultMessage)
        {
            Debug.Log($"MQTT: RealtimeMimicResultHandler: Realtime Mimic Result Message Received Pick : {realtimeMimicResultMessage.WasPickRequest} Or Place : {realtimeMimicResultMessage.WasPlaceRequest}, PTIdx : {realtimeMimicResultMessage.PointIndex}, PlanningSuccess: {realtimeMimicResultMessage.PickOrPlacePlanningSucceeded}");
            if (realtimeMimicResultMessage.CorrectBackend == false)
            {
                Debug.LogWarning("MQTT: RealtimeMimicHandler: No Trajectories in the Mimic Result Message.");
                string warningMessage = "WARNING: The robotic controler is set to the incorrect backend. Please restart it to mimic in realtime.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.RealtimeMimicIncorrectBackendOnScreenMessage, "RealtimeMimicIncorrectBackendOnScreenMessage", UIFunctionalities.MessagesParent, warningMessage, "RealtimeMimicIncorrectBackend: Realtime Mimic Cannot be used because the backend is incorrect.");
                return;
            }
            if (realtimeMimicResultMessage.PointIndex == null)
            {
                Debug.LogError("MQTT: RealtimeMimicResult Message Handeling: Point Index is null in the Realtime Mimic Result Message. No action taken.");
            }
            else if(realtimeMimicResultMessage.WasPickRequest)
            {
                Debug.Log("MQTT: RealtimeMimicResult Message Handeling: This was a Pick Action.");
                if (realtimeMimicResultMessage.PickOrPlacePlanningSucceeded)
                {
                    realtimeMimicPickandPlaceManager.ObjectPicked = true;
                    realtimeMimicPickandPlaceManager.IgnoreObservedGeometries = true;
                    realtimeMimicPickandPlaceManager.IgnoreTargetGeometries = false;

                    Debug.Log("MQTT: RealtimeMimicResult Message Handeling: The Pick Planning Succeeded.");
                    UIFunctionalities.UpdateLastCompletedIndexForRealtimeMimicPoint(realtimeMimicResultMessage.PointIndex);
                    UIFunctionalities.RealtimeMimicResetSceneFromPlanningSuccess();
                }
                else
                {
                    Debug.LogWarning("MQTT: RealtimeMimicResult Message Handeling: The Pick Planning Failed. The Point will be deleted, but the Action will not be completed.");
                    string warningMessage = "WARNING: The robotic controler was unable to plan the pick action. Please adjust the robot and try again.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.RealtimeMimicPickPlanningFailedOnScreenMessage, "RealtimeMimicPickPlanningFailedOnScreenMessage", UIFunctionalities.MessagesParent, warningMessage, "RealtimeMimicResultHandler: Realtime Mimic Pick Planning Failed.");
                    UIFunctionalities.RealtimeMimicResetSceneFromPlanningFailure();
                }
            }
            else if(realtimeMimicResultMessage.WasPlaceRequest)
            {
                Debug.Log("MQTT: RealtimeMimicResult Message Handeling: This was a Pick Action.");
                if (realtimeMimicResultMessage.PickOrPlacePlanningSucceeded)
                {
                    realtimeMimicPickandPlaceManager.ObjectPicked = false;
                    realtimeMimicPickandPlaceManager.IgnoreObservedGeometries = false;
                    realtimeMimicPickandPlaceManager.IgnoreTargetGeometries = true;

                    Debug.Log("MQTT: RealtimeMimicResult Message Handeling: The Pick Planning Succeeded.");
                    UIFunctionalities.UpdateLastCompletedIndexForRealtimeMimicPoint(realtimeMimicResultMessage.PointIndex);
                    UIFunctionalities.RealtimeMimicResetSceneFromPlanningSuccess();
                }
                else
                {
                    Debug.LogWarning("MQTT: RealtimeMimicResult Message Handeling: The Pick Planning Failed. The Point will be deleted, but the Action will not be completed.");
                    string warningMessage = "WARNING: The robotic controler was unable to plan the pick action. Please adjust the robot and try again.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.RealtimeMimicPlacePlanningFailedOnScreenMessage, "RealtimeMimicPickPlanningFailedOnScreenMessage", UIFunctionalities.MessagesParent, warningMessage, "RealtimeMimicResultHandler: Realtime Mimic Pick Planning Failed.");
                    UIFunctionalities.RealtimeMimicResetSceneFromPlanningFailure();
                }
            }
            else if (realtimeMimicResultMessage.Configuration == null)
            {
                Debug.LogWarning("MQTT: RealtimeMimicResult Message Handeling: Configuration is null in the Realtime Mimic Result Message. THe Point will be deleted, but the Action will not be completed.");
            }
            else
            {
                // UIFunctionalities.UpdateRealtimeMimicPointFromMessage(pointIndex, trajectoryVisualizer.RealtimeMimicPoints, trajectoryVisualizer.RealtimeMimicLines);
                UIFunctionalities.UpdateLastCompletedIndexForRealtimeMimicPoint(realtimeMimicResultMessage.PointIndex);
            }
        }
        public void MimicResultReceivedMessageHandler(MimicTrajectoryResultMessage mimicResultMessage)
        {
            Debug.Log("MQTT: MimicResultReceivedMessageHandler: Mimic Result Message Received");
            Debug.Log($"MQTT: MimicResultReceivedMessageHandler: Mimic Result Message Received with {mimicResultMessage.Trajectories.Count} trajectories and {mimicResultMessage.CombinedTrajectoryPoints.Count} points");

            if (databaseManager.ProjectZones.CurrentZone != ProjectZones.CurrentZoneMode.Mimic)
            {
                Debug.LogWarning("MQTT: MimicResultReceivedMessageHandler: Current Zone is not Mimic. No action taken.");
                return;
            }
            else if (mimicResultMessage.Trajectories.Count <= 0)
            {
                Debug.LogWarning("MQTT: MimicResultReceivedMessageHandler: No Trajectories in the Mimic Result Message.");
                string message = "WARNING: The robotic controler replied with a null Trajectory. Please edit points or rerequest.";
                UIFunctionalities.SetUserInitiatedMimicControlsActivity(true, true, true, false, false);
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryNullWarningMessageObject, "TrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "MimicTrajectoryResultReceivedMessageHandler: Received trajectory is null");
                return;
            }
            else if (mimicResultMessage.CombinedTrajectoryPoints.Count <= 0)
            {
                Debug.LogWarning("MQTT: MimicResultReceivedMessageHandler: Combined Trajectory points are empty in the Mimic Result Message.");
                string message = "WARNING: The robotic controler replied with a null Trajectory. Please edit points or rerequest.";
                UIFunctionalities.SetUserInitiatedMimicControlsActivity(true, true, true, false, false);
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryNullWarningMessageObject, "TrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "MimicTrajectoryResultReceivedMessageHandler: Received trajectory is null");
                return;
            }
            else if (mimicResultMessage.RobotName != serviceManager.ActiveRobotName)  //TODO: THIS IS FROM COMPAS XR, BUT NEEDS TO BE THOUGHT ABOUT FOR ROBOT TERRITORIES
            {
                //Update Last Mimic Trajectory Result Message in the Service Manager
                if (UIFunctionalities.UserInitiatedMimicTrajectoryReviewSlider.value != 0)
                {
                    // UIFunctionalities.UserInitiatedMimicTrajectoryReviewSlider.value = 0;
                    UIFunctionalities.UserInitiatedMimicTrajectoryReviewSlider.SetValueWithoutNotify(0);
                }

                UIFunctionalities.SignalActiveRobotUpdateFromPlannerRoboticTerritories(
                    mimicResultMessage.RobotName,
                    serviceManager.ActiveRobotName,
                    () => trajectoryVisualizer.InstantateRobotFromMimicMessage(
                        mimicResultMessage,
                        trajectoryVisualizer.ActiveRobot,
                        trajectoryVisualizer.URDFLinkNames,
                        trajectoryVisualizer.ActiveTrajectoryParentObject,
                        true));

                //Update Last Mimic Trajectory Result Message in the Service Manager
                serviceManager.LastMimicTrajectoryResultMessage = mimicResultMessage; //TODO: This is a strategy from compas XR class, but needs to be cleaned up

                Debug.Log("MQTT: : MimicResultReceivedMessageHandler : Robot Name in the message is not the same as the active robot name signaling on screen control.");
                return;
            }
            else
            {
                //Update Last Mimic Trajectory Result Message in the Service Manager
                if (UIFunctionalities.UserInitiatedMimicTrajectoryReviewSlider.value != 0)
                {
                    UIFunctionalities.UserInitiatedMimicTrajectoryReviewSlider.SetValueWithoutNotify(0);
                    // UIFunctionalities.UserInitiatedMimicTrajectoryReviewSlider.value = 0;
                }

                //Update Last Mimic Trajectory Result Message in the Service Manager
                serviceManager.LastMimicTrajectoryResultMessage = mimicResultMessage; //TODO: This is a strategy from compas XR class, but needs to be cleaned up

                trajectoryVisualizer.InstantateRobotFromMimicMessage(
                    mimicResultMessage,
                    trajectoryVisualizer.ActiveRobot,
                    trajectoryVisualizer.URDFLinkNames,
                    trajectoryVisualizer.ActiveTrajectoryParentObject,
                    true);
                UIFunctionalities.SetUserInitiatedMimicControlsActivity(true, false, false, true, true);
                Debug.Log("MQTT: : MimicResultReceivedMessageHandler : Robot Name in the message is the same as the active robot name.");
            }
        }
        public void InferenceResultReceivedMessageHandler(InferenceResultMessage inferenceResultMessage) //TODO: Remember CurrentGoal is updated in instantiateObjects
        {
            //Set the inference containing exacutable trajectory to false by default
            serviceManager.InferenceContainsExacutableTrajectory = false;
            serviceManager.InferenceSuggestedTargetName = "None";
            UIFunctionalities.OnScreenWaitForInferenceMessage.SetActive(false);

            if (databaseManager.ProjectZones.CurrentZone != ProjectZones.CurrentZoneMode.Inference)
            {
                Debug.LogWarning("MQTT: InferenceResultMessageHandler: Current Zone is not Inference. No action taken.");
                string message = "WARNING: You received an inference result but are not in Inference mode.This will be ignored";
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.InferenceResultReceivedWhileInOtherModeOnScreenMessage, "InferenceModeDeselected", UIFunctionalities.MessagesParent, message, "InferenceResultReceivedMessageHandler: Inference result received while not in inference mode.");
                serviceManager.InferenceResultsMessages.Add(inferenceResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                return;
            }
            else if (inferenceResultMessage.InferenceGuess == null || inferenceResultMessage.SuggestedTargetName == null || inferenceResultMessage.CompletedGoals.Count <= 0)
            {
                Debug.LogWarning("MQTT: InferenceResultMessageHandler: No inference guess or suggested target or completed goals");
                string message = "WARNING: The inference planner was unable to infer a goal. Place more and request again.";
                UIFunctionalities.SetInferenceRequestUIControlsVisibilityandInteractibility(true, true, false, false, false);
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.InferenceUnableToInferGoalMessage, "InferenceUnableToInferGoal", UIFunctionalities.MessagesParent, message, "InferenceResultReceivedMessageHandler: Inference result has no guess or completed goals.");
                serviceManager.InferenceResultsMessages.Add(inferenceResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                return;
            }
            else if (inferenceResultMessage.Trajectories.Count <= 0)
            {
                Debug.LogWarning("MQTT: InferenceResultReceivedMessageHandler: No Trajectories in the Mimic Result Message.");
                string message = "WARNING: The robotic controler replied with a null Trajectory, but the inference planner thinks this is your goal.";
                serviceManager.InferenceResultsMessages.Add(inferenceResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                UIFunctionalities.ShowInferedGeometriesInSceeneWrapper(inferenceResultMessage.InferenceGuess, inferenceResultMessage.CompletedGoals, inferenceResultMessage.SuggestedTargetName);

                UIFunctionalities.SetInferenceRequestUIControlsVisibilityandInteractibility(true, false, true, true, false);
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.InferenceTrajectoryNullWarningMessageObject, "InferenceTrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "InferenceResultReceivedMessageHandler: Received trajectory is null");
                return;
            }
            else if (inferenceResultMessage.RobotName != serviceManager.ActiveRobotName)  //TODO: THIS IS FROM COMPAS XR, BUT NEEDS TO BE THOUGHT ABOUT FOR ROBOT TERRITORIES
            {
                //Update Last Mimic Trajectory Result Message in the Service Manager
                if (UIFunctionalities.InferenceReviewSlider.value != 0)
                {
                    UIFunctionalities.InferenceReviewSlider.SetValueWithoutNotify(0);
                    // UIFunctionalities.InferenceReviewSlider.value = 0;
                }

                UIFunctionalities.SignalActiveRobotUpdateFromPlannerRoboticTerritories(
                    inferenceResultMessage.RobotName,
                    serviceManager.ActiveRobotName,
                    () => trajectoryVisualizer.InstantateRobotFromInferenceResultMessage(
                        inferenceResultMessage,
                        trajectoryVisualizer.ActiveRobot,
                        trajectoryVisualizer.URDFLinkNames,
                        trajectoryVisualizer.ActiveTrajectoryParentObject,
                        true));

                //Update Last Mimic Trajectory Result Message in the Service Manager
                serviceManager.InferenceResultsMessages.Add(inferenceResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                serviceManager.InferenceContainsExacutableTrajectory = true;
                serviceManager.InferenceSuggestedTargetName = inferenceResultMessage.SuggestedTargetName;

                UIFunctionalities.ShowInferedGeometriesInSceeneWrapper(inferenceResultMessage.InferenceGuess, inferenceResultMessage.CompletedGoals, inferenceResultMessage.SuggestedTargetName);

                UIFunctionalities.SetInferenceRequestUIControlsVisibilityandInteractibility(true, false, true, true, true);
                Debug.Log("MQTT: InferenceResultReceivedMessageHandler : Robot Name in the message is not the same as the active robot name signaling on screen control.");
                return;
            }
            else
            {
                //Update Last Inference Result Message in the Service Manager //TODO: Soemtimes I get an error from this.
                if (UIFunctionalities.InferenceReviewSlider.value != 0)
                {
                    UIFunctionalities.InferenceReviewSlider.SetValueWithoutNotify(0);
                    // UIFunctionalities.InferenceReviewSlider.value = 0;
                }

                //Update Last Inference Result Message in the Service Manager
                trajectoryVisualizer.InstantateRobotFromInferenceResultMessage(
                    inferenceResultMessage,
                    trajectoryVisualizer.ActiveRobot,
                    trajectoryVisualizer.URDFLinkNames,
                    trajectoryVisualizer.ActiveTrajectoryParentObject,
                    true);

                serviceManager.InferenceResultsMessages.Add(inferenceResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                serviceManager.InferenceContainsExacutableTrajectory = true;
                serviceManager.InferenceSuggestedTargetName = inferenceResultMessage.SuggestedTargetName;

                UIFunctionalities.ShowInferedGeometriesInSceeneWrapper(inferenceResultMessage.InferenceGuess, inferenceResultMessage.CompletedGoals, inferenceResultMessage.SuggestedTargetName);
                UIFunctionalities.SetInferenceRequestUIControlsVisibilityandInteractibility(true, false, true, true, true);

                Debug.Log("MQTT: InferenceResultReceivedMessageHandler : Robot Name in the message is the same as the active robot name.");
            }

        }
        public void PostInferenceTargetTrajectoryResultReceivedMessageHandler(PostInferenceTrajectoryResultMessage postInferenceTrajectoryResultMessage) //TODO: Remember CurrentGoal is updated in instantiateObjects
        {
            Debug.Log("MQTT: PostInferenceTargetTrajectoryResultReceivedMessageHandler: Post Inference Target Trajectory Result Message Received");
            if (databaseManager.ProjectZones.CurrentZone != ProjectZones.CurrentZoneMode.Inference)
            {
                Debug.LogWarning("MQTT: PostInferenceTargetTrajectoryResultReceivedMessageHandler: Current Zone is not Inference. No action taken.");
                string message = "WARNING: You received an inference result but are not in Inference mode.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.InferenceResultReceivedWhileInOtherModeOnScreenMessage, "InferenceModeDeselected", UIFunctionalities.MessagesParent, message, "PostInferenceTargetTrajectoryResultReceivedMessageHandler: Inference result received while not in inference mode.");
                UIFunctionalities.SetInferenceUIPostInferenceSuccesState(true, true, true, false, false); //TODO: This will cause an error if the current selected IsSatsified.
                serviceManager.PostInferenceTrajectoryResultsMessages.Add(postInferenceTrajectoryResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                return;
            }
            else if (postInferenceTrajectoryResultMessage.InferenceGoalName == null || postInferenceTrajectoryResultMessage.TargetName == null)
            {
                Debug.LogWarning("MQTT: InferenceResultMessageHandler: No inference guess or suggested target or completed goals");
                string message = "WARNING: The inference planner was unable to infer a goal. Place more and request again.";
                UIFunctionalities.SetInferenceRequestUIControlsVisibilityandInteractibility(true, true, false, false, false);
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.PostInferenceMessageErrorOnScreenMessage, "PostInferenceTrajectoryResultError", UIFunctionalities.MessagesParent, message, "PostInferenceTargetTrajectoryResultReceivedMessageHandler: Inference result has no guess or completed goals.");
                UIFunctionalities.SetInferenceUIPostInferenceSuccesState(true, true, true, false, false); //TODO: This will cause an error if the current selected IsSatsified.
                serviceManager.PostInferenceTrajectoryResultsMessages.Add(postInferenceTrajectoryResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                return;
            }
            else if (postInferenceTrajectoryResultMessage.Trajectories.Count <= 0)
            {
                Debug.LogWarning("MQTT: InferenceResultReceivedMessageHandler: No Trajectories in the Mimic Result Message.");
                string message = "WARNING: The robotic controler replied with a null Trajectory, but the inference planner thinks this is your goal.";
                serviceManager.PostInferenceTrajectoryResultsMessages.Add(postInferenceTrajectoryResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                UIFunctionalities.SetInferenceUIPostInferenceSuccesState(true, true, true, false, false); //TODO: This will cause an error if the current selected IsSatsified.
                UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.PostInferencePlannerRepliedWithNullTrajectory, "PostInferenceTrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "PostInferenceTargetTrajectoryResultReceivedMessageHandler: Received trajectory is null");
                return;
            }
            else if (postInferenceTrajectoryResultMessage.RobotName != serviceManager.ActiveRobotName)  //TODO: THIS IS FROM COMPAS XR, BUT NEEDS TO BE THOUGHT ABOUT FOR ROBOT TERRITORIES
            {
                //Update Last Mimic Trajectory Result Message in the Service Manager
                if (UIFunctionalities.PostInferenceTrajectoryReviewSlider.value != 0)
                {
                    UIFunctionalities.PostInferenceTrajectoryReviewSlider.SetValueWithoutNotify(0);
                    // UIFunctionalities.PostInferenceTrajectoryReviewSlider.value = 0;
                }

                UIFunctionalities.SignalActiveRobotUpdateFromPlannerRoboticTerritories(
                    postInferenceTrajectoryResultMessage.RobotName,
                    serviceManager.ActiveRobotName,
                    () => trajectoryVisualizer.InstantateRobotFromPostInferenceResultMessage(
                        postInferenceTrajectoryResultMessage,
                        trajectoryVisualizer.ActiveRobot,
                        trajectoryVisualizer.URDFLinkNames,
                        trajectoryVisualizer.ActiveTrajectoryParentObject,
                        true));

                //Update Last Mimic Trajectory Result Message in the Service Manager
                serviceManager.PostInferenceTrajectoryResultsMessages.Add(postInferenceTrajectoryResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                UIFunctionalities.SetInferenceUIPostInferenceSuccesState(true, false, false, true, true);
                Debug.Log("MQTT: PostInferenceTargetTrajectoryResultReceivedMessageHandler : Robot Name in the message is not the same as the active robot name signaling on screen control.");
                return;
            }
            else
            {
                //Update Last Inference Result Message in the Service Manager //TODO: Soemtimes I get an error from this.
                if (UIFunctionalities.PostInferenceTrajectoryReviewSlider.value != 0)
                {
                    UIFunctionalities.PostInferenceTrajectoryReviewSlider.SetValueWithoutNotify(0);
                    // UIFunctionalities.PostInferenceTrajectoryReviewSlider.value = 0;
                }

                //Update Last Inference Result Message in the Service Manager
                trajectoryVisualizer.InstantateRobotFromPostInferenceResultMessage(
                    postInferenceTrajectoryResultMessage,
                    trajectoryVisualizer.ActiveRobot,
                    trajectoryVisualizer.URDFLinkNames,
                    trajectoryVisualizer.ActiveTrajectoryParentObject,
                    true);

                serviceManager.PostInferenceTrajectoryResultsMessages.Add(postInferenceTrajectoryResultMessage); //TODO: This is a strategy from compas XR class, but needs to be cleaned up
                UIFunctionalities.SetInferenceUIPostInferenceSuccesState(true, false, false, true, true);
                Debug.Log("MQTT: PostInferenceTargetTrajectoryResultReceivedMessageHandler : Robot Name in the message is the same as the active robot name.");
            }
        }

        //TODO: Robotic Territories Testing //////////////////////////////////////////////////////////////////////////

        protected override void OnConnected()
        {
            /*
            * Method is used to signal that the MQTT connection has been established.
            */
            HandleConnectionSucceededUIPanel();
            base.OnConnected();
            Debug.Log($"MQTT: Connected to broker: {brokerAddress} on Port: {brokerPort}.");
            // if (UIFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
            // {
            //     UserInterface.SetUIObjectColor(UIFunctionalities.MqttConnectButtonObject, Color.green);
            //     UIFunctionalities.UpdateConnectionStatusText(UIFunctionalities.MqttConnectionStatusObject, true);
            // }
        }
        protected override void OnDisconnected()
        {
            /*
            * Method is used to signal that the MQTT connection has been disconnected.
            */
            base.OnDisconnected();
            Debug.Log("MQTT: DISCONNECTED.");
            // if (UIFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
            // {
            //     UIFunctionalities.UpdateConnectionStatusText(UIFunctionalities.MqttConnectionStatusObject, false);
            // }
        }
        protected override void OnConnectionLost()
        {
            /*
            * Method is used to signal that the MQTT connection has been lost.
            */
            base.OnConnectionLost();
            string message = "WARNING: MQTT connection has been lost. Please check your internet connection and restart the application.";
            UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.MQTTConnectionLostMessageObject, "MQTTConnectionLostMessage", UIFunctionalities.MessagesParent, message, "OnConnectionLost: MQTT Connection Lost");
            Debug.Log("MQTT: CONNECTION LOST");
        }
        public async void DisconnectandReconnectAsyncRoutine()
        {
            /*
            * Method is used to disconnect from the MQTT broker and reconnect after a short delay.
            */
            Disconnect();
            StartCoroutine(ReconnectAfterDisconect());
        }
        private IEnumerator ReconnectAfterDisconect()
        {
            /*
            * Method is used to reconnect to the MQTT broker after a short delay.
            */
            yield return new WaitUntil(() => !mqttClientConnected);
            yield return new WaitForSeconds(0.5f);
            OnStartorRestartInitilization(true);
        }

        //////////////////////////////////////////// Topic Managers /////////////////////////////////////////////
        public void SetCompasXRTopics(object source, ApplicationSettingsEventArgs e)
        {
            /*
            * Method is used to set the custom Compas XR Topics based on the Application Settings.
            * format: compas_xr/project_name/message_name
            */
            compasXRTopics = new CompasXRTopics(e.Settings.project_name);
        }
        private void SubscribeToTopic(string topicToSubscribe)
        {
            /*
            * Method is used to subscribe to a custom topic.
            */
            if (!string.IsNullOrEmpty(topicToSubscribe) && client != null)
            {
                Debug.Log("MQTT: SubscribeToTopic: Subscribing to topic: " + topicToSubscribe);
                client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            else
            {
                Debug.LogError("MQTT: Topic to subscribe is empty or client is null.");
            }
        }
        private void UnsubscribeFromTopic(string topicToUnsubscribe)
        {
            /*
            * Method is used to unsubscribe from a custom topic.
            */
            if (!string.IsNullOrEmpty(topicToUnsubscribe) && client != null)
            {
                client.Unsubscribe(new string[] { topicToUnsubscribe });
                Debug.Log("MQTT: UnsubscribeFromTopic: Unsubscribed from topic: " + topicToUnsubscribe);
            }
            else
            {
                Debug.LogWarning("MQTT: Topic to unsubscribe is empty or client is null.");
            }
        }
        public void PublishToTopic(string publishingTopic, Dictionary<string, object> message)
        {
            /*
            * Method is used to publish a message to a custom topic.
            */
            if (client != null && client.IsConnected)
            {
                string messagePublish = JsonConvert.SerializeObject(message);
                client.Publish(publishingTopic, System.Text.Encoding.UTF8.GetBytes(messagePublish), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            }
            else
            {
                Debug.LogWarning("MQTT: PublishToTopic: Client is null or not connected. Cannot publish message.");
            }
        }
        public void SubscribeToCompasXRTopics()
        {
            /*
            * Method is used to subscribe to the custom Compas XR Topics.
            */
            Debug.Log("MQTT: SubscribeToCompasXRTopics: Subscribing to Compas XR Topics");
            SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryRequestTopic);
            SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);
            SubscribeToTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
            SubscribeToTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
        }
        public void UnsubscribeFromCompasXRTopics()
        {
            /*
            * Method is used to unsubscribe from the custom Compas XR Topics.
            */
            UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryRequestTopic);
            UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryResultTopic);
            UnsubscribeFromTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
            UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
        }

        //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
        private void CompasXRIncomingMessageHandler(string topic, string message)
        {
            /*
            * Method is used to handle the incoming messages from the MQTT broker based on the topic.
            */
            if (topic == compasXRTopics.subscribers.getTrajectoryRequestTopic)
            {
                Debug.Log("MQTT: GetTrajectoryRequest Message Handeling");
                GetTrajectoryRequest getTrajectoryRequestmessage = GetTrajectoryRequest.Parse(message);
                GetTrajectoryRequestReceivedMessageHandler(getTrajectoryRequestmessage);
            }
            else if (topic == compasXRTopics.subscribers.getTrajectoryResultTopic)
            {
                Debug.Log("MQTT: GetTrajectoryResult Message Handeling");
                GetTrajectoryResult getTrajectoryResultmessage = GetTrajectoryResult.Parse(message);
                Debug.Log("MQTT: Pick and place bool: " + getTrajectoryResultmessage.PickAndPlace.ToString());
                GetTrajectoryResultReceivedMessageHandler(getTrajectoryResultmessage);
            }
            else if (topic == compasXRTopics.subscribers.approveTrajectoryTopic)
            {
                Debug.Log("MQTT: ApproveTrajectory Message Handeling");
                ApproveTrajectory trajectoryApprovalMessage = ApproveTrajectory.Parse(message);
                ApproveTrajectoryMessageReceivedHandler(trajectoryApprovalMessage);
            }
            else if (topic == compasXRTopics.subscribers.approvalCounterRequestTopic)
            {
                ApprovalCounterRequest approvalCounterRequestMessage = ApprovalCounterRequest.Parse(message);
                ApprovalCounterRequestMessageReceivedHandler(approvalCounterRequestMessage);
            }
            else if (topic == compasXRTopics.subscribers.approvalCounterResultTopic)
            {
                ApprovalCounterResult approvalCounterResultMessage = ApprovalCounterResult.Parse(message);
                ApprovalCounterResultMessageReceivedHandler(approvalCounterResultMessage);
            }
            else
            {
                Debug.LogWarning("MQTT: No message handler for topic: " + topic);
            }

        }
        private void GetTrajectoryRequestReceivedMessageHandler(GetTrajectoryRequest getTrajectoryRequestmessage)
        {
            /*
            * Method is used to handle the GetTrajectoryRequest message received from the MQTT broker.
            */
            serviceManager.LastGetTrajectoryRequestMessage = getTrajectoryRequestmessage;
            serviceManager.GetTrajectoryRequestTimeOutCancelationToken = new CancellationTokenSource();
            _ = TrajectoryRequestTimeOut(getTrajectoryRequestmessage.ElementID, 240f, serviceManager.GetTrajectoryRequestTimeOutCancelationToken.Token);

            if (getTrajectoryRequestmessage.Header.DeviceID != SystemInfo.deviceUniqueIdentifier)
            {
                Debug.Log($"MQTT: GetTrajectoryRequest from user {getTrajectoryRequestmessage.Header.DeviceID}");
                serviceManager.TrajectoryRequestTransactionLock = true;
            }
            else
            {
                Debug.Log("MQTT: GetTrajectoryRequest this request came from me");
            }
        }
        private void GetTrajectoryResultReceivedMessageHandler(GetTrajectoryResult getTrajectoryResultmessage)
        {
            //Check if the message is dirty and should be ignored
            if (serviceManager.IsDirtyTrajectory)
            {
                if (serviceManager.IsDirtyGetTrajectoryRequestHeader.ResponseID == getTrajectoryResultmessage.Header.ResponseID &&
                serviceManager.IsDirtyGetTrajectoryRequestHeader.SequenceID + 1 == getTrajectoryResultmessage.Header.SequenceID)
                {
                    Debug.Log("MQTT: GetTrajectoryResult: This Message is dirty & Should be Ignored.");
                    return;
                }
            }

            if (serviceManager.GetTrajectoryRequestTimeOutCancelationToken != null)
            {
                Debug.Log("GetTrajectoryResultReceivedMessageHandler: The request time out should be cancled, because the result was received.");
                serviceManager.GetTrajectoryRequestTimeOutCancelationToken.Cancel();
            }

            serviceManager.LastGetTrajectoryResultMessage = getTrajectoryResultmessage;
            if (serviceManager.LastGetTrajectoryRequestMessage != null)
            {
                //First Check if the message is the same as the last request message and if the trajectory count is greater then zero
                if (getTrajectoryResultmessage.Header.ResponseID != serviceManager.LastGetTrajectoryRequestMessage.Header.ResponseID
                || getTrajectoryResultmessage.Header.SequenceID != serviceManager.LastGetTrajectoryRequestMessage.Header.SequenceID + 1
                || getTrajectoryResultmessage.ElementID != serviceManager.LastGetTrajectoryRequestMessage.ElementID)
                {
                    if (serviceManager.PrimaryUser)
                    {
                        Debug.LogWarning("MQTT: GetTrajectoryResult (PrimaryUser): ResponseID, SequenceID, or ElementID do not match the last GetTrajectoryRequestMessage. No action taken.");

                        string message = "WARNING: Trajectory Response did not match expectations. Returning to Request Service.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryResponseIncorrectWarningMessageObject, "TrajectoryResponseIncorrectWarningMessage", UIFunctionalities.MessagesParent, message, "GetTrajectoryResultReceivedMessageHandler: Message Structure incorrect.");

                        serviceManager.PrimaryUser = false;
                        serviceManager.currentService = ServiceManager.CurrentService.None;
                        UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                        return;
                    }
                    else
                    {
                        serviceManager.TrajectoryRequestTransactionLock = false;
                        if (UIFunctionalities.RobotToggleObject.GetComponent<Toggle>().isOn)
                        {
                            UIFunctionalities.SetRoboticUIElementsFromKey(UIFunctionalities.CurrentStep);
                        }

                        Debug.LogWarning("MQTT: GetTrajectoryResult (!PrimaryUser): ResponseID, SequenceID, or ElementID do not match the last GetTrajectoryRequestMessage. Ignoring Message.");
                        return;
                    }
                }
                else
                {

                    //Check if the count is greater then Zero and start the time out dependant on if I am primary user or not.
                    if (getTrajectoryResultmessage.Trajectory.Count > 0)
                    {
                        serviceManager.ApprovalTimeOutCancelationToken = new CancellationTokenSource();
                        float duration = 120; //DURATION FOR PRIMARY USER WAITING FOR APPROVALS.... NEEDS TO BE ADJUSTED W/ FABRICATION TIME.
                        if (!serviceManager.PrimaryUser)
                        {
                            duration = 240; //DURATION FOR NON PRIMARY USER WAITING FOR CONSENSUS.... NEEDS TO BE ADJUSTED W/ FABRICATION TIME.
                        }
                        _ = TrajectoryApprovalTimeout(getTrajectoryResultmessage.ElementID, duration, serviceManager.ApprovalTimeOutCancelationToken.Token);
                    }
                    else
                    {
                        Debug.Log("MQTT: GetTrajectoryResult: Trajectory count is zero. No time out started.");
                    }

                    //If I am not the primary user checks
                    if (!serviceManager.PrimaryUser)
                    {
                        if (getTrajectoryResultmessage.Trajectory.Count > 0)
                        {
                            serviceManager.TrajectoryRequestTransactionLock = false;


                            UIFunctionalities.SignalTrajectoryReviewRequest(
                                getTrajectoryResultmessage.ElementID,
                                getTrajectoryResultmessage.RobotName,
                                serviceManager.ActiveRobotName,
                                () => trajectoryVisualizer.VisualizeRobotTrajectoryFromResultMessage(
                                    getTrajectoryResultmessage,
                                    trajectoryVisualizer.URDFLinkNames,
                                    trajectoryVisualizer.ActiveRobot,
                                    trajectoryVisualizer.ActiveTrajectoryParentObject,
                                    true));

                            serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;
                            serviceManager.currentService = ServiceManager.CurrentService.ApproveTrajectory;

                            Debug.Log("GetTrajectoryResult (!PrimaryUser): Trajectory count is greater then zero. I am moving on to trajectory review.");
                        }
                        else
                        {
                            serviceManager.TrajectoryRequestTransactionLock = false;
                            if (UIFunctionalities.RobotToggleObject.GetComponent<Toggle>().isOn)
                            {
                                UIFunctionalities.SetRoboticUIElementsFromKey(UIFunctionalities.CurrentStep);
                            }

                            Debug.Log("GetTrajectoryResult (!PrimaryUser): Trajectory count is zero. I am free to request.");
                        }
                    }

                    else
                    {
                        //I am the primary user
                        if (getTrajectoryResultmessage.Trajectory.Count > 0)
                        {
                            SubscribeToTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                            UIFunctionalities.TrajectoryServicesUIControler(false, false, true, true, false, false);
                            serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;
                            serviceManager.currentService = ServiceManager.CurrentService.ApproveTrajectory;

                            if (getTrajectoryResultmessage.RobotName != serviceManager.ActiveRobotName)
                            {
                                UIFunctionalities.SignalActiveRobotUpdateFromPlanner(
                                    getTrajectoryResultmessage.ElementID,
                                    getTrajectoryResultmessage.RobotName,
                                    serviceManager.ActiveRobotName,
                                    () => trajectoryVisualizer.VisualizeRobotTrajectoryFromResultMessage(
                                        getTrajectoryResultmessage,
                                        trajectoryVisualizer.URDFLinkNames,
                                        trajectoryVisualizer.ActiveRobot,
                                        trajectoryVisualizer.ActiveTrajectoryParentObject,
                                        true));

                                Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Robot Name in the message is not the same as the active robot name signaling on screen control.");

                            }
                            else
                            {
                                trajectoryVisualizer.VisualizeRobotTrajectoryFromResultMessage(getTrajectoryResultmessage, trajectoryVisualizer.URDFLinkNames, trajectoryVisualizer.ActiveRobot, trajectoryVisualizer.ActiveTrajectoryParentObject, true);
                                Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Robot Name in the message is the same as the active robot name.");
                            }

                            PublishToTopic(compasXRTopics.publishers.approvalCounterRequestTopic, new ApprovalCounterRequest(UIFunctionalities.CurrentStep).GetData());
                        }
                        //If the trajectory count is zero reset Service Manger and Return to Request Trajectory Service
                        else
                        {
                            Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Trajectory count is zero resetting Service Manager and returning to Request Trajectory Service.");
                            serviceManager.PrimaryUser = false;
                            serviceManager.currentService = ServiceManager.CurrentService.None;
                            UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                            string message = "WARNING: The robotic controler replied with a Null trajectory. You will be returned to trajectory request.";
                            UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryNullWarningMessageObject, "TrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "GetTrajectoryResultReceivedMessageHandler: Received trajectory is null");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("MQTT: GetTrajectoryResult LastGetTrajectoryRequestMessage is null. A request must be made before this code works.");
            }
        }
        private void ApproveTrajectoryMessageReceivedHandler(ApproveTrajectory trajectoryApprovalMessage)
        {
            /*
            * Method is used to handle the ApproveTrajectory message received from the MQTT broker.
            * The method will handle the approval status and take appropriate actions based on the status.
            * ApprovalStatus: 0 = Trajectory Rejected, 1 = Trajectory Approved, 2 = Consensus, 3 = Cancelation
            */

            //ApproveTrajectoryMessage ApprovalStatus Trajectory rejected message received
            if (trajectoryApprovalMessage.ApprovalStatus == 0)
            {
                if (serviceManager.PrimaryUser)
                {
                    UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                    serviceManager.PrimaryUser = false;
                }

                if (serviceManager.ApprovalTimeOutCancelationToken != null)
                {
                    Debug.Log("ApproveTrajectoryMessageReceivedHandler: Time Out Cancled from Rejection Message.");
                    serviceManager.ApprovalTimeOutCancelationToken.Cancel();
                }

                if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                }
                else
                {
                    Debug.LogWarning("ApproveTrajectoryMessageReceivedHandler: ActiveTrajectoryParentObject is null or has no children.");
                }

                if (trajectoryVisualizer.ActiveRobot != null && !trajectoryVisualizer.ActiveRobot.activeSelf)
                {
                    trajectoryVisualizer.ActiveRobot.SetActive(true);
                }

                serviceManager.ApprovalCount.Reset();
                serviceManager.UserCount.Reset();
                serviceManager.CurrentTrajectory = null;
                serviceManager.currentService = ServiceManager.CurrentService.None;
                UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
            }
            //ApproveTrajectoryMessage ApprovalStatus Trajectory approved message received
            else if (trajectoryApprovalMessage.ApprovalStatus == 1)
            {
                Debug.Log($"MQTT: ApproveTrajectory User {trajectoryApprovalMessage.Header.DeviceID} approved trajectory {trajectoryApprovalMessage.TrajectoryID}");
                if (serviceManager.PrimaryUser)
                {
                    serviceManager.ApprovalCount.Increment();
                    Debug.Log($"MQTT: Message Handeling Counters UserCount == {serviceManager.UserCount.Value} and ApprovalCount == {serviceManager.ApprovalCount.Value}");
                    if (serviceManager.ApprovalCount.Value == serviceManager.UserCount.Value)
                    {
                        Debug.Log("MQTT: ApprovalCount == UserCount. Moving to Service 3 as Primary User.");
                        UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                        serviceManager.currentService = ServiceManager.CurrentService.ExacuteTrajectory;
                        UIFunctionalities.TrajectoryServicesUIControler(false, false, true, false, true, true);
                    }
                    else
                    {
                        Debug.Log($"MQTT: Message Handeling Counters UserCount == {serviceManager.UserCount.Value} and ApprovalCount == {serviceManager.ApprovalCount.Value}");
                    }
                }
            }
            //ApproveTrajectoryMessage ApprovalStatus Consensus message received
            else if (trajectoryApprovalMessage.ApprovalStatus == 2)
            {
                Debug.Log($"MQTT: ApproveTrajectory Consensus message received for trajectory {trajectoryApprovalMessage.TrajectoryID}");

                if (serviceManager.PrimaryUser)
                {
                    UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                    serviceManager.PrimaryUser = false;
                }

                if (serviceManager.ApprovalTimeOutCancelationToken != null)
                {
                    Debug.Log("ApproveTrajectoryMessageReceivedHandler: Time Out Cancled from Consensus Message.");
                    serviceManager.ApprovalTimeOutCancelationToken.Cancel();
                }

                serviceManager.ApprovalCount.Reset();
                serviceManager.UserCount.Reset();
                serviceManager.CurrentTrajectory = null;
                serviceManager.currentService = ServiceManager.CurrentService.None;
                UIFunctionalities.TrajectoryServicesUIControler(true, false, false, false, false, false);
            }
            //ApproveTrajectoryMessage ApprovalStatus Cancelation message received
            else if (trajectoryApprovalMessage.ApprovalStatus == 3)
            {
                Debug.Log($"MQTT: ApproveTrajectory Cancelation message received for trajectory {trajectoryApprovalMessage.TrajectoryID}");
                if (serviceManager.ApprovalTimeOutCancelationToken != null)
                {
                    Debug.Log("ApproveTrajectoryMessageReceivedHandler: Canceling Trajectory Approval from Cancelation Message.");
                    serviceManager.ApprovalTimeOutCancelationToken.Cancel();
                }
                if (!serviceManager.PrimaryUser)
                {
                    serviceManager.ApprovalCount.Reset();
                    serviceManager.UserCount.Reset();
                    serviceManager.CurrentTrajectory = null;
                    serviceManager.currentService = ServiceManager.CurrentService.None;
                    if (trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    }
                    if (trajectoryVisualizer.ActiveRobot != null && !trajectoryVisualizer.ActiveRobot.activeSelf)
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(true);
                    }

                    if (trajectoryApprovalMessage.Header.DeviceID != SystemInfo.deviceUniqueIdentifier)
                    {
                        string message = "WARNING : The trajectory approval has been canceled by another user. Returning to Request Trajectory Service.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryCancledMessage, "TrajectoryCancledMessage", UIFunctionalities.MessagesParent, message, "ApproveTrajectoryMessageReceivedHandler: Trajectory Cancled by another user.");
                    }
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    Debug.Log("MQTT: ApproveTrajectory Cancelation message received for trajectory, but I am the primary user. No action taken.");
                }
            }
            //ApprovalStatus not recognized.
            else
            {
                Debug.LogWarning("MQTT: Approval Status is not recognized. Approval Status: " + trajectoryApprovalMessage.ApprovalStatus);
            }
        }
        private void ApprovalCounterRequestMessageReceivedHandler(ApprovalCounterRequest approvalCounterRequestMessage)
        {
            /*
            * Method is used to handle the ApprovalCounterRequest message received from the MQTT broker.
            * The method will publish the ApprovalCounterResult message to the MQTT broker.
            */
            Debug.Log($"MQTT: ApprovalCounterRequset Message Received from User {approvalCounterRequestMessage.Header.DeviceID}");
            PublishToTopic(compasXRTopics.publishers.approvalCounterResultTopic, new ApprovalCounterResult(approvalCounterRequestMessage.ElementID).GetData());
        }
        private void ApprovalCounterResultMessageReceivedHandler(ApprovalCounterResult approvalCounterResultMessage)
        {
            /*
            * Method is used to handle the ApprovalCounterResult message received from the MQTT broker.
            * The method will increment the UserCount if the Primary User has approved the trajectory.
            */
            Debug.Log($"MQTT: ApprovalCounterResult Message Received from User{approvalCounterResultMessage.Header.DeviceID} for step {approvalCounterResultMessage.ElementID}");
            if (serviceManager.PrimaryUser)
            {
                serviceManager.UserCount.Increment();
            }
        }
        private void StoreMessage(string eventMsg)
        {
            /*
            * Method is used to store the last 50 messages received from the MQTT broker.
            */
            if (eventMessages.Count > 50) eventMessages.Clear();
            eventMessages.Add(eventMsg);
        }
        async Task TrajectoryApprovalTimeout(string elementID, float timeDurationSeconds, CancellationToken cancellationToken)
        {
            /*
            * Method is used to handle the Trajectory Approval Time Out.
            * The method will wait for the time duration and take appropriate actions based on the time out
            * if it is not cancled before timeout is reached.
            */
            Debug.Log($"MQTT: TrajectoryApprovalTimeout: Started with a duration of {timeDurationSeconds} seconds.");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeDurationSeconds), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (serviceManager.CurrentTrajectory != null)
                {
                    if (serviceManager.PrimaryUser && serviceManager.currentService.Equals(ServiceManager.CurrentService.ExacuteTrajectory))
                    {
                        Debug.Log("MQTT: TrajectoryApprovalTimeout: Primary User has already moved on to Service 3.");
                        return;
                    }
                    else
                    {
                        Debug.Log("MQTT: TrajectoryApprovalTimeout: Primary User has not moved on to service 3 or Other user reached time out : Services will be reset.");
                        PublishToTopic(compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(elementID, serviceManager.ActiveRobotName, serviceManager.CurrentTrajectory, 3).GetData());
                        if (serviceManager.PrimaryUser)
                        {
                            UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                        }

                        serviceManager.PrimaryUser = false;
                        serviceManager.currentService = ServiceManager.CurrentService.None;
                        serviceManager.ApprovalCount.Reset();
                        serviceManager.UserCount.Reset();

                        string message = "WARNING : Trajectory Approval has timed out. Returning to Request Trajectory Service.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryCancledMessage, "TrajectoryCancledMessage", UIFunctionalities.MessagesParent, message, "TrajectoryApprovalTimeout: Trajectory Approval Cancled by Timeout.");
                        UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                    }
                }
                else
                {
                    Debug.Log("MQTT: TrajectoryApprovalTimeout: Current Trajectory is null and this method should not have been called. Either Cancelation did not happen properly, or there is a null trajectory on timeout.");
                }

            }

            catch (TaskCanceledException)
            {
                Debug.Log("MQTT: TrajectoryApprovalTimeout: Task was cancled before the time out duration meaning everything proved to be successful or was purposfully cancled.");
            }
        }
        async Task TrajectoryRequestTimeOut(string elementID, float timeDurationSeconds, CancellationToken cancellationToken)
        {
            /*
            * Method is used to handle the Trajectory Request Time Out.
            * The method will wait for the time duration and take appropriate actions based on the time out
            * if it is not cancled before timeout is reached.
            */
            Debug.Log($"MQTT: TrajectoryRequestTimeOut: For element {elementID} Started with a duration of {timeDurationSeconds} seconds.");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeDurationSeconds), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (serviceManager.PrimaryUser)
                {
                    serviceManager.PrimaryUser = false;
                    serviceManager.currentService = ServiceManager.CurrentService.None;
                    serviceManager.ApprovalCount.Reset();
                    serviceManager.UserCount.Reset();

                    string message = "WARNING : Current Trajectory Request has timed out. Returning to Request Trajectory Service.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryRequestTimeoutMessage, "TrajectoryRequestTimeoutMessage", UIFunctionalities.MessagesParent, message, "TrajectoryRequestTimeoutMessage: Trajectory Request Cancled by Timeout.");
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    Debug.Log("MQTT: TrajectoryRequestTimeOut: Current Primary User has not received a trajectory result yet transaction lock removed.");
                    serviceManager.TrajectoryRequestTransactionLock = false;
                }

                serviceManager.IsDirtyTrajectory = true;
                serviceManager.IsDirtyGetTrajectoryRequestHeader = serviceManager.LastGetTrajectoryRequestMessage.Header;
            }

            catch (TaskCanceledException)
            {
                Debug.Log("MQTT: TrajectoryRequestTimeOut: Task was cancled before the time out duration");
            }
        }

        //////////////////////////////////////////// Event Handlers ////////////////////////////////////////////
        public void AddConnectionEventListners()
        {
            /*
            * Method is used to add event listners for the MQTT connection options.
            */
            ConnectionFailed += HandleConnectionFailedUIPanel;
            ConnectionSucceeded += SubscribeToCompasXRTopics;
            ConnectionFailed += UIFunctionalities.SignalMQTTConnectionFailed;
        }
        public void RemoveConnectionEventListners()
        {
            /*
            * Method is used to remove event listners for the MQTT connection options.
            */
            ConnectionFailed -= HandleConnectionFailedUIPanel;
            ConnectionSucceeded -= SubscribeToCompasXRTopics;
            ConnectionFailed -= UIFunctionalities.SignalMQTTConnectionFailed;
        }

        public void HandleConnectionSucceededUIPanel()
        {
            /*
            * Method is used to handle the MQTT connection succeeded event for the UI panel.
            */
            Debug.Log("MQTT: Connection to broker succeeded - HandleConnectionSucceededUIPanel");
            mqttConnectionMessagePannel.SetActive(false);
        }
        public void HandleConnectionFailedUIPanel()
        {
            /*
            * Method is used to handle the MQTT connection failed event for the UI panel.
            */
            Debug.LogError("MQTT: Connection to broker failed - HandleConnectionFailedUIPanel");
            mqttConnectionFailedTextObjects.SetActive(true);
        }

    }
}
