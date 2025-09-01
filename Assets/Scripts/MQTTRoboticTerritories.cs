using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using RosSharp.RosBridgeClient.MessageTypes.Rosapi;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using Newtonsoft.Json;
using RosSharp.Urdf;
using CompasXR.Core.Data;
using CompasXR.Core;
using Google.MiniJSON;
using CompasXR.Robots.Data;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;



namespace CompasXR.Robots.MqttData.RoboticTerritories
{
    [System.Serializable]
    public class RoboticTerritoriesTopics
    {
        /*
        * CompasXRTopics : Class is used to manage the MQTT topics for Compas XR communication.
        * It is designed to store the publishers and subscribers for the specific project.
        */
        public RTPublishers publishers { get; set; }
        public RTSubscribers subscribers { get; set; }
        public RoboticTerritoriesTopics(string projectName)
        {
            publishers = new RTPublishers(projectName);
            subscribers = new RTSubscribers(projectName);
        }
    }

    [System.Serializable]
    public class RTPublishers
    {
        /*
        * Publishers : Class is used to manage the MQTT publishers for Compas XR communication.
        * It is designed to store the specific topics to publish to.
        */
        public string mimicRequestTopic { get; set; }
        public string mimicExecuteTrajectoryRequestTopic { get; set; }
        public string realtimeMimicRequestTopic { get; set; }
        public string realtimeMimicIOToggleRequestTopic { get; set; }

        public string inferenceRequestTopic { get; set; }
        public string inferenceUserReplyTopic { get; set; }

        public string inferencePostInferenceRequestTarget { get; set; }
        public string inferencePostInferenceExecuteTargetTopic { get; set; }
        public RTPublishers(string projectName)
        {
            mimicRequestTopic = $"robotic_territories/mimic_request/{projectName}";
            mimicExecuteTrajectoryRequestTopic = $"robotic_territories/mimic_execute_trajectory/{projectName}";
            realtimeMimicRequestTopic = $"robotic_territories/real_time_mimic_request/{projectName}";
            realtimeMimicIOToggleRequestTopic = $"robotic_territories/real_time_mimic_io_toggle_request/{projectName}";

            inferenceRequestTopic = $"robotic_territories/inference_request/{projectName}";
            inferenceUserReplyTopic = $"robotic_territories/inference_user_reply/{projectName}";
            inferencePostInferenceRequestTarget = $"robotic_territories/post_inference_request_target/{projectName}";
            inferencePostInferenceExecuteTargetTopic = $"robotic_territories/post_inference_execute_target/{projectName}";
        }

    }

    [System.Serializable]
    public class RTSubscribers
    {
        /*
        * Subscribers : Class is used to manage the MQTT subscribers for Compas XR communication.
        * It is designed to store the specific topics to subscribe to.
        */
        public string mimicResultTopic { get; set; }
        public string realtimeMimicResultTopic { get; set; }
        public string inferenceResultTopic { get; set; }
        public string inferencePostInferenceTargetTrajectoryResultTopic { get; set; }

        //Constructer for subscribers that takes an input project name
        public RTSubscribers(string projectName)
        {
            mimicResultTopic = $"robotic_territories/mimic_result/{projectName}";
            realtimeMimicResultTopic = $"robotic_territories/real_time_mimic_result/{projectName}";

            inferenceResultTopic = $"robotic_territories/inference_result/{projectName}";
            inferencePostInferenceTargetTrajectoryResultTopic = $"robotic_territories/post_inference_target_result/{projectName}";
        }
    }

    // Message classes ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public static class MessageHandelingExtensions
    {
        public static Frame _getBaseFrameFromMessage(Dictionary<string, object> jsonObject)
        {
            var robotBaseFrameData = jsonObject["robot_base_frame"] as JObject;
            if (robotBaseFrameData != null)
            {
                var robotBaseFrameDict = robotBaseFrameData.ToObject<Dictionary<string, object>>();

                if (robotBaseFrameDict != null && robotBaseFrameDict.ContainsKey("data"))
                {
                    var robotBaseFrameInnerDict = robotBaseFrameDict["data"] as JObject;
                    if (robotBaseFrameInnerDict != null)
                    {
                        var frameDataDict = robotBaseFrameInnerDict.ToObject<Dictionary<string, object>>();
                        Debug.Log($"MimicTrajectoryResultMessage: Parse: robotBaseFrameDict: {JsonConvert.SerializeObject(frameDataDict)}");
                        Frame robotBaseFrame = Frame.FromData(frameDataDict);
                        return robotBaseFrame;
                    }
                    else
                    {
                        Debug.LogError("robotBaseFrameInnerDict is null.");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError("Key 'data' not found in robot_base_frame.");
                    return null;
                }
            }
            else
            {
                Debug.LogError("robot_base_frame is not a JObject.");
                return null;
            }
        }
        public static List<Dictionary<string, object>> _getTrajectoriesDataFromList(List<Trajectory> Trajectories)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<Dictionary<string, object>> trajectoriesData = new List<Dictionary<string, object>>();
            foreach (Trajectory trajectory in Trajectories)
            {
                trajectoriesData.Add(trajectory.GetData());
            }
            return trajectoriesData;
        }

        public static List<Frame> _parseDataFromFramesList(List<Dictionary<string, object>> framesData)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<Frame> frames = new List<Frame>();
            foreach (Dictionary<string, object> frameData in framesData)
            {
                frames.Add(Frame.Parse(JsonConvert.SerializeObject(frameData)));
            }
            return frames;
        }

        public static List<JointTrajectoryPoint> _getCombinedTrajectoryPointsFromTrajectoryList(List<Trajectory> Trajectories)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<JointTrajectoryPoint> combinedTrajectoryPoints = new List<JointTrajectoryPoint>();
            foreach (Trajectory trajectory in Trajectories)
            {
                combinedTrajectoryPoints.AddRange(trajectory.Points);
            }
            return combinedTrajectoryPoints;
        }
        public static List<Dictionary<string, object>> _getDataFromJointTrajectoryPointList(List<JointTrajectoryPoint> jointTrajectoryPoints)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<Dictionary<string, object>> jointTrajectoryPointsData = new List<Dictionary<string, object>>();
            foreach (JointTrajectoryPoint jointTrajectoryPoint in jointTrajectoryPoints)
            {
                jointTrajectoryPointsData.Add(jointTrajectoryPoint.GetData());
            }
            return jointTrajectoryPointsData;
        }

        public static List<JointTrajectoryPoint> _parseJointTrajectoryPointFromDataList(List<Dictionary<string, object>> jointTrajectoryPointsData)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<JointTrajectoryPoint> jointTrajectoryPoints = new List<JointTrajectoryPoint>();
            foreach (Dictionary<string, object> jointTrajectoryPointData in jointTrajectoryPointsData)
            {
                jointTrajectoryPoints.Add(JointTrajectoryPoint.Parse(JsonConvert.SerializeObject(jointTrajectoryPointData)));
            }
            return jointTrajectoryPoints;
        }

        public static List<Dictionary<string, object>> _getDataFromFramesList(List<Frame> Frames)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<Dictionary<string, object>> framesData = new List<Dictionary<string, object>>();
            foreach (Frame frame in Frames)
            {
                framesData.Add(frame.GetData());
            }
            return framesData;
        }

        public static Dictionary<string, Dictionary<string, object>> _getDataFromFramesDictionary(Dictionary<string, Frame> FramesDictionary)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            Dictionary<string, Dictionary<string, object>> framesData = new Dictionary<string, Dictionary<string, object>>();
            foreach (KeyValuePair<string, Frame> frame in FramesDictionary)
            {
                framesData.Add(frame.Key, frame.Value.GetData());
            }
            return framesData;
        }

        public static List<Frame> _parseFramesFromDataList(List<Dictionary<string, object>> framesData)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<Frame> frames = new List<Frame>();
            foreach (Dictionary<string, object> frameData in framesData)
            {
                frames.Add(Frame.Parse(JsonConvert.SerializeObject(frameData)));
            }
            return frames;
        }

    }

    [System.Serializable]
    public class Header
    {
        /*
        * Header : Class is used to manage the header for Compas XR communication.
        * It is designed to store the sequence ID, response ID, device ID, and timestamp for each message.
        */
        public string DeviceID { get; private set; }
        public string TimeStamp { get; private set; }
        public Header(string deviceID = null, string timeStamp = null)
        {
            if (deviceID != null && timeStamp != null)
            {
                DeviceID = deviceID;
                TimeStamp = timeStamp;
            }
            else
            {
                DeviceID = GetDeviceID();
                TimeStamp = GetTimeStamp();
            }
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the header data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "device_id", DeviceID },
                { "time_stamp", TimeStamp }
            };
        }
        private static string GetDeviceID()
        {
            /*
            * Method is used to retrieve the device ID for the current device.
            */
            return SystemInfo.deviceUniqueIdentifier;
        }
        private static string GetTimeStamp()
        {
            /*
            * Method is used to retrieve the current timestamp.
            */
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        public static Header Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var deviceID = jsonObject["device_id"].ToString();
            var timeStamp = jsonObject["time_stamp"].ToString();

            //Update message counters based on the received information if needed
            return new Header(deviceID, timeStamp);
        }
    }

    [System.Serializable]
    public class MimicTrajectoryRequestMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public List<Frame> HumanFrames { get; private set; }
        public List<Frame> RobotFrames { get; private set; }
        public string RobotName { get; private set; }
        public MimicTrajectoryRequestMessage(List<Frame> humanFrames, List<Frame> robotFrames, string robotName, Header header = null)
        {
            Header = header ?? new Header();
            HumanFrames = humanFrames;
            RobotFrames = robotFrames;
            RobotName = robotName;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "human_frames", MessageHandelingExtensions._getDataFromFramesList(HumanFrames) },
                { "robot_frames", MessageHandelingExtensions._getDataFromFramesList(RobotFrames) },
                { "robot_name", RobotName }
            };
        }
        public static MimicTrajectoryRequestMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var humanFramesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["human_frames"].ToString());
            List<Frame> humanFrames = Frame._parseFramesData(humanFramesData);

            var robotFramesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["human_frames"].ToString());
            List<Frame> robotFrames = Frame._parseFramesData(robotFramesData);

            var robotName = jsonObject["robot_name"].ToString();

            return new MimicTrajectoryRequestMessage(humanFrames, robotFrames, robotName, header);
        }
    }

    [System.Serializable]
    public class MimicTrajectoryResultMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public List<Trajectory> Trajectories { get; private set; }
        public Frame RobotBaseFrame { get; private set; }
        public List<JointTrajectoryPoint> CombinedTrajectoryPoints { get; private set; }
        public string RobotName { get; private set; }
        public MimicTrajectoryResultMessage(List<Trajectory> trajectories, Frame robotBaseFrame, string robotName, Header header = null)
        {
            Header = header ?? new Header();
            Trajectories = trajectories;
            CombinedTrajectoryPoints = _GetJointTrajectoryPoints(trajectories);
            RobotBaseFrame = robotBaseFrame;
            RobotName = robotName;
        }

        public static List<JointTrajectoryPoint> _GetJointTrajectoryPoints(List<Trajectory> trajectories)
        {
            if (trajectories.Count == 0)
            {
                Debug.LogWarning("MimicTrajectoryResultMessage: No trajectories found in the message returning null list.");
                return new List<JointTrajectoryPoint>();
            }
            else
            {
                Debug.Log($"MimicTrajectoryResultMessage: Found {trajectories.Count} trajectories in the message.");
                return MessageHandelingExtensions._getCombinedTrajectoryPointsFromTrajectoryList(trajectories);
            }
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "trajectories", MessageHandelingExtensions._getTrajectoriesDataFromList(Trajectories) },
                { "robot_base_frame", RobotBaseFrame.GetData() },
                { "robot_name", RobotName }
            };
        }
        public static MimicTrajectoryResultMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */

            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var trajectoriesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["trajectories"].ToString());
            List<Trajectory> trajectories = new List<Trajectory>();
            if (trajectoriesData.Count > 0)
            {
                foreach (Dictionary<string, object> trajectoryData in trajectoriesData)
                {
                    if (trajectoryData.TryGetValue("data", out var trajectoryDataValue))
                    {
                        var trajectoryJson = JsonConvert.SerializeObject(trajectoryDataValue);
                        var trajectoryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(trajectoryJson);
                        trajectories.Add(Trajectory.FromData(trajectoryDict));
                    }
                    else
                    {
                        Debug.LogWarning("MimicTrajectoryResultMessage: Parse: Trajectory data not found in the message.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("MimicTrajectoryResultMessage: Parse: No trajectories found in the message.");
            }

            Frame robotBaseFrame = MessageHandelingExtensions._getBaseFrameFromMessage(jsonObject);
            if (robotBaseFrame == null)
            {
                Debug.LogWarning("MimicTrajectoryResultMessage: Parse: Robot base frame not found in the message.");
            }
            else
            {
                Debug.Log($"MimicTrajectoryResultMessage: Robot Base Frame Parsed Successfully: {JsonConvert.SerializeObject(robotBaseFrame)}");
            }

            //Parse the robot name
            var robotName = jsonObject["robot_name"].ToString();

            return new MimicTrajectoryResultMessage(trajectories, robotBaseFrame, robotName, header);
        }

    }


    [System.Serializable]
    public class ExacuteMimicTrajectoryRequestMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public List<Trajectory> Trajectories { get; private set; }
        public List<JointTrajectoryPoint> CombinedTrajectoryPoints { get; private set; }
        public string RobotName { get; private set; }
        public Frame RobotBaseFrame { get; private set; }
        public ExacuteMimicTrajectoryRequestMessage(List<Trajectory> trajectories, List<JointTrajectoryPoint> combinedTrajectoryPoints, string robotName, Frame robotBaseFrame, Header header = null)
        {
            Header = header ?? new Header();
            Trajectories = trajectories;
            CombinedTrajectoryPoints = combinedTrajectoryPoints;
            RobotName = robotName;
            RobotBaseFrame = robotBaseFrame;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "trajectories", MessageHandelingExtensions._getTrajectoriesDataFromList(Trajectories) },
                { "combined_trajectory_points", MessageHandelingExtensions._getDataFromJointTrajectoryPointList(CombinedTrajectoryPoints) },
                { "robot_name", RobotName },
                { "robot_base_frame", RobotBaseFrame.GetData() }
            };
        }
        public static ExacuteMimicTrajectoryRequestMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var trajectoriesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["trajectories"].ToString());
            List<Trajectory> trajectories = new List<Trajectory>();
            foreach (Dictionary<string, object> trajectoryData in trajectoriesData)
            {
                if (trajectoryData.TryGetValue("data", out var trajectoryDataValue))
                {
                    var trajectoryJson = JsonConvert.SerializeObject(trajectoryDataValue);
                    var trajectoryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(trajectoryJson);
                    trajectories.Add(Trajectory.FromData(trajectoryDict));
                }
                else
                {
                    Debug.LogWarning("ExacuteMimicTrajectoryRequestMessage: Parse: Trajectory data not found in the message.");
                }
            }

            var combinedTrajectoryPointsData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["combined_trajectory_points"].ToString());
            List<JointTrajectoryPoint> combinedTrajectoryPoints = MessageHandelingExtensions._parseJointTrajectoryPointFromDataList(combinedTrajectoryPointsData);

            Frame robotBaseFrame = MessageHandelingExtensions._getBaseFrameFromMessage(jsonObject);
            if (robotBaseFrame == null)
            {
                Debug.LogWarning("ExacuteMimicTrajectoryRequestMessage: Parse: Robot base frame not found in the message.");
            }
            else
            {
                Debug.Log($"ExacuteMimicTrajectoryRequestMessage: Robot Base Frame Parsed Successfully: {JsonConvert.SerializeObject(robotBaseFrame)}");
            }

            var robotName = jsonObject["robot_name"].ToString();

            return new ExacuteMimicTrajectoryRequestMessage(trajectories, combinedTrajectoryPoints, robotName, robotBaseFrame, header);
        }

    }

    [System.Serializable]
    public class RealtimeMimicRequestMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }

        public Frame RequestedRobotFrame { get; private set; }

        // public List<Frame> HumanFrames { get; private set; }
        // public List<Frame> RobotFrames { get; private set; }
        public string RobotName { get; private set; }
        public string Message { get; private set; }
        public bool InitialRequest { get; private set; }
        public RealtimeMimicRequestMessage(Frame requestedRobotFrame, string robotName, string message, Header header = null, bool initialRequest = false)
        {
            Header = header ?? new Header();
            RequestedRobotFrame = requestedRobotFrame;
            // HumanFrames = humanFrames;
            // RobotFrames = robotFrames;
            RobotName = robotName;
            Message = message;
            InitialRequest = initialRequest;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "requested_robot_frame", RequestedRobotFrame.GetData() },
                // { "human_frames", MessageHandelingExtensions._getDataFromFramesList(HumanFrames) },
                // { "robot_frames", MessageHandelingExtensions._getDataFromFramesList(RobotFrames) },
                { "robot_name", RobotName },
                { "message", Message },
                { "initial_request", InitialRequest }
            };
        }
        public static RealtimeMimicRequestMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            Dictionary<string, object> requestedRobotFrameDict = jsonObject["requested_robot_frame"] as Dictionary<string, object>;
            Frame requestedFrame = Frame.FromData(requestedRobotFrameDict);
            // var humanFramesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["human_frames"].ToString());
            // List<Frame> humanFrames = Frame._parseFramesData(humanFramesData);

            // var robotFramesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["robot_frames"].ToString());
            // List<Frame> robotFrames = Frame._parseFramesData(robotFramesData);

            var robotName = jsonObject["robot_name"].ToString();
            var message = jsonObject["message"].ToString();
            var initialRequest = Convert.ToBoolean(jsonObject["initial_request"]);
            return new RealtimeMimicRequestMessage(requestedFrame, robotName, message, header, initialRequest);
        }
    }

    [System.Serializable]
    public class RealtimeMimicResultMessage //TODO: UPDATE ME.
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        // public List<Trajectory> Trajectories { get; private set; }
        // public Frame RobotBaseFrame { get; private set; }
        // public List<Configuration> Configurations { get; private set; }
        // public List<AttachedCollisionMesh> AttachedCollisionMeshes { get; private set; }
        public string RobotName { get; private set; }
        public string ReturnMessage { get; private set; }
        public RealtimeMimicResultMessage(string robotName, string returnMessage, Header header = null) //List<Trajectory> trajectories, Frame robotBaseFrame, string robotName, Header header=null)
        {
            Header = header ?? new Header();
            // Trajectories = trajectories;
            // RobotBaseFrame = robotBaseFrame;
            RobotName = robotName;
            ReturnMessage = returnMessage;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                // { "trajectories", MessageHandelingExtensions._getTrajectoriesDataFromList(Trajectories) },
                // { "robot_base_frame", RobotBaseFrame.GetData() },
                { "robot_name", RobotName },
                { "return_message", ReturnMessage }
            };
        }
        public static RealtimeMimicResultMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var robotName = jsonObject["robot_name"].ToString();
            var message = jsonObject["return_message"].ToString();
            return new RealtimeMimicResultMessage(robotName, message, header);

            // var trajectoriesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["trajectories"].ToString());
            // List<Trajectory> trajectories = new List<Trajectory>();
            // foreach (Dictionary<string, object> trajectoryData in trajectoriesData)
            // {
            //     if (trajectoryData.TryGetValue("data", out var trajectoryDataValue))
            //     {
            //         var trajectoryJson = JsonConvert.SerializeObject(trajectoryDataValue);
            //         var trajectoryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(trajectoryJson);
            //         trajectories.Add(Trajectory.FromData(trajectoryDict));
            //     }
            //     else
            //     {
            //         Debug.LogWarning("ExacuteMimicTrajectoryRequestMessage: Parse: Trajectory data not found in the message.");
            //     }
            // }
            // Frame robotBaseFrame = MessageHandelingExtensions._getBaseFrameFromMessage(jsonObject);
            // if(robotBaseFrame == null)
            // {
            //     Debug.LogWarning("ExacuteMimicTrajectoryRequestMessage: Parse: Robot base frame not found in the message.");
            // }
            //     else
            //     {
            //     Debug.Log($"ExacuteMimicTrajectoryRequestMessage: Robot Base Frame Parsed Successfully: {JsonConvert.SerializeObject(robotBaseFrame)}");
        }
    }

    [System.Serializable]
    public class RealtimeMimicIOToggleRequestMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public int Signal { get; private set; }
        public int Value { get; private set; }
        public RealtimeMimicIOToggleRequestMessage(int signal, bool gripperToggle, Header header = null) //List<Trajectory> trajectories, Frame robotBaseFrame, string robotName, Header header=null)
        {
            Header = header ?? new Header();
            Signal = signal;
            if (gripperToggle)
            {
                Value = 1; // 1 for gripper open
            }
            else
            {
                Value = 0; // 0 for gripper close
            }
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "signal", Signal },
                { "value", Value }
            };
        }
        public static RealtimeMimicIOToggleRequestMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);
            var signal = Convert.ToInt32(jsonObject["signal"]);
            var value = Convert.ToInt32(jsonObject["value"]);
            bool gripperToggle = false;
            if (value != 0 && value != 1)
            {
                Debug.LogError("RealtimeMimicIOToggleRequestMessage: Parse: Value must be either 0 or 1.");
                return null;
            }
            else if (signal == 0)
            {
                gripperToggle = false;
            }
            else if (signal == 1)
            {
                gripperToggle = true;
            }
            return new RealtimeMimicIOToggleRequestMessage(signal, gripperToggle, header);
        }
    }

    [System.Serializable]
    public class InferenceRequestMessage
    {
        /*
        * InferenceRequestMessage : Class is used to manage the InferenceRequestMessage message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public Dictionary<string, Frame> CurrentGeometryFrames { get; private set; }
        public string RobotName { get; private set; }
        public bool InitialRequest { get; private set; }
        public InferenceRequestMessage(Dictionary<string, Frame> currentGeometryFrames, bool initialRequest, string robotName, Header header = null)
        {
            Header = header ?? new Header();
            CurrentGeometryFrames = currentGeometryFrames;
            RobotName = robotName;
            InitialRequest = initialRequest;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "geometry_frames",  MessageHandelingExtensions._getDataFromFramesDictionary(CurrentGeometryFrames) },
                { "robot_name", RobotName },
                { "initial_request", InitialRequest }
            };
        }


        public static InferenceRequestMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // geometry_frames: now a dictionary: name -> frameData
            var geometryFrames = new Dictionary<string, Frame>();

            //TODO: Get initial request boolean
            var initialRequest = Convert.ToBoolean(jsonObject["initial_request"]);

            if (jsonObject.TryGetValue("geometry_frames", out var framesObj) && framesObj != null)
            {
                // Expecting: { "geometry_frames": { "frameA": {...}, "frameB": {...}, ... } }
                var framesDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(
                    framesObj.ToString()
                );

                if (framesDict != null)
                {
                    foreach (var kv in framesDict)
                    {
                        var frameName = kv.Key;
                        var frameData = kv.Value; // Dictionary<string, object> for one frame

                        try
                        {
                            geometryFrames[frameName] = Frame.FromData(frameData);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"InferenceRequestMessage.Parse: Skipping frame '{frameName}': {ex.Message}");
                        }
                    }
                }
            }


            var robotName = jsonObject["robot_name"].ToString();
            return new InferenceRequestMessage(geometryFrames, initialRequest, robotName, header);

        }
    }

    [System.Serializable] //TODO: CHECK THIS.
    public class InferenceResultMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }

        //TODO: Make Nullable.
        public List<Trajectory> Trajectories { get; private set; }

        //TODO: Make Nullable.
        public Frame RobotBaseFrame { get; private set; }

        //TODO: Make Nullable.
        public List<string> CompletedGoals { get; private set; }
        //TODO: Make Nullable.
        public List<JointTrajectoryPoint> CombinedTrajectoryPoints { get; private set; }
        public string RobotName { get; private set; }
        public string InferenceGuess { get; set; }
        public string SuggestedTargetName { get; set; }

        public InferenceResultMessage(List<string> completedGoals, List<Trajectory> trajectories, string inferenceGuess = null, string suggestedTargetName = null, Frame robotBaseFrame = null, string robotName = null, Header header = null)
        {
            Header = header ?? new Header();
            Trajectories = trajectories;
            CompletedGoals = completedGoals;
            if (trajectories.Count > 0)
            {
                CombinedTrajectoryPoints = _GetJointTrajectoryPoints(trajectories);
            }
            else
            {
                CombinedTrajectoryPoints = new List<JointTrajectoryPoint>();
            }
            RobotBaseFrame = robotBaseFrame;
            RobotName = robotName;
            InferenceGuess = inferenceGuess;
            SuggestedTargetName = suggestedTargetName;
        }

        public static List<JointTrajectoryPoint> _GetJointTrajectoryPoints(List<Trajectory> trajectories)
        {
            if (trajectories.Count == 0)
            {
                Debug.LogWarning("InferenceResultMessage: No trajectories found in the message returning null list.");
                return new List<JointTrajectoryPoint>();
            }
            else
            {
                Debug.Log($"InferenceResultMessage: Found {trajectories.Count} trajectories in the message.");
                return MessageHandelingExtensions._getCombinedTrajectoryPointsFromTrajectoryList(trajectories);
            }
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "trajectories", MessageHandelingExtensions._getTrajectoriesDataFromList(Trajectories) },
                { "robot_base_frame", RobotBaseFrame.GetData() },
                { "robot_name", RobotName },
                { "inference_guess", InferenceGuess },
                { "suggested_target_name", SuggestedTargetName },
                { "completed_goals", CompletedGoals }
            };
        }
        public static InferenceResultMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */

            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var inferenceGuess = (jsonObject.TryGetValue("inference_guess", out var ig) && ig != null)
                ? ig.ToString()
                : null;

            if (string.IsNullOrEmpty(inferenceGuess))
            {
                Debug.LogWarning("InferenceResultMessage: Parse: Inference guess is null or empty.");
                return new InferenceResultMessage(new List<string>(), new List<Trajectory>(),
                                                null, null, null, null, header);
            }

            //Parse the robot name
            var robotName = jsonObject["robot_name"].ToString();
            var completedGoals = DictionaryHelpers.GetStringListFromDict(jsonObject, "completed_goals");
            Debug.Log($"Completed goals: {JsonConvert.SerializeObject(completedGoals)}"); // ["G0","G1"]
            Debug.Log($"Completed goals type: {completedGoals.GetType()}");

            var suggestedTargetName = jsonObject["suggested_target_name"] != null ? jsonObject["suggested_target_name"].ToString() : null;
            if (string.IsNullOrEmpty(suggestedTargetName))
            {
                Debug.LogWarning("InferenceResultMessage: Parse: Suggested target name is null or empty.");
            }
            else
            {
                Debug.Log($"InferenceResultMessage: Suggested Target Name Parsed Successfully: {suggestedTargetName}");
            }

            var trajectoriesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["trajectories"].ToString());
            List<Trajectory> trajectories = new List<Trajectory>();
            if (trajectoriesData.Count > 0)
            {
                int i = 0;
                foreach (Dictionary<string, object> trajectoryData in trajectoriesData)
                {
                    if (trajectoryData.TryGetValue("data", out var trajectoryDataValue))
                    {
                        var trajectoryJson = JsonConvert.SerializeObject(trajectoryDataValue);
                        var trajectoryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(trajectoryJson);
                        trajectories.Add(Trajectory.FromData(trajectoryDict));
                        Debug.Log($"JOE : InferenceResultMessage: Parse: Trajectory {i} parsed successfully.");
                        i++;
                    }
                    else
                    {
                        Debug.LogWarning("InferenceResultMessage: Parse: Trajectory data not found in the message.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("InferenceResultMessage: Parse: No trajectories found in the message.");
            }

            if (trajectories.Count <= 0)
            {
                Debug.LogWarning("InferenceResultMessage: Parse: No trajectories found in the message.");
                return new InferenceResultMessage(completedGoals, new List<Trajectory>(), inferenceGuess, suggestedTargetName, null, robotName, header);
            }

            Frame robotBaseFrame = null;
            try
            {
                robotBaseFrame = MessageHandelingExtensions._getBaseFrameFromMessage(jsonObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"InferenceResultMessage: Failed to parse Robot Base Frame. Using empty. Error: {ex.Message}");
            }
            if (robotBaseFrame == null)
            {
                Debug.LogWarning("InferenceResultMessage: Robot base frame is null (allowed).");
            }
            else
            {
                Debug.Log($"InferenceResultMessage: Robot Base Frame Parsed Successfully: {JsonConvert.SerializeObject(robotBaseFrame, Formatting.Indented)}");
            }

            //TODO: ALL OF THESE NEED TO DUMP IF IT IS NULL. NOT REACH SOME SORT OF EXCEPTION.
            return new InferenceResultMessage(completedGoals, trajectories, inferenceGuess, suggestedTargetName, robotBaseFrame, robotName, header);
        }
    }

    public enum GoalStatusReplyEnum
    {
        RejectGoalandTarget = 0,
        AcceptTargetRejectGoal = 1,
        AcceptTargetandGoal = 2
    }
    [System.Serializable]
    public class InferenceUserReplyMessage
    {
        /*
        * InferenceRequestMessage : Class is used to manage the InferenceRequestMessage message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public GoalStatusReplyEnum GoalStatusReply { get; private set; }

        public string RobotName { get; private set; }
        public bool IncludesExecutableTrajectory { get; private set; }
        public string CurrentGoalName { get; set; }
        public string SuggestedTargetName { get; set; }

        public InferenceUserReplyMessage(GoalStatusReplyEnum goalStatusReply, string currentGoalName, string suggestedTargetName, string robotName, bool includesExecutableTrajectory, Header header = null)
        {
            Header = header ?? new Header();
            GoalStatusReply = goalStatusReply;
            RobotName = robotName;
            IncludesExecutableTrajectory = includesExecutableTrajectory;
            CurrentGoalName = currentGoalName;
            SuggestedTargetName = suggestedTargetName;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "goal_status_reply", (int)GoalStatusReply },
                { "robot_name", RobotName },
                { "includes_executable_trajectory", IncludesExecutableTrajectory },
                { "current_goal_name", CurrentGoalName },
                { "suggested_target_name", SuggestedTargetName }
            };
        }
        public static InferenceUserReplyMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            int rawValue = Convert.ToInt32(jsonObject["goal_status_reply"]);
            GoalStatusReplyEnum goalStatusReply = (GoalStatusReplyEnum)rawValue;

            var robotName = jsonObject["robot_name"].ToString();
            var includesExacutableTrajectory = Convert.ToBoolean(jsonObject["includes_executable_trajectory"]);
            var currentGoalName = jsonObject["current_goal_name"] != null ? jsonObject["current_goal_name"].ToString() : null;
            var suggestedTargetName = jsonObject["suggested_target_name"] != null ? jsonObject["suggested_target_name"].ToString() : null;

            return new InferenceUserReplyMessage(goalStatusReply, currentGoalName, suggestedTargetName, robotName, includesExacutableTrajectory, header);
        }
    }

    [System.Serializable] //TODO: Work in Progress....
    public class PostInferenceTargetRequestMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public string InferenceGoalName { get; set; }
        public string TargetName { get; set; }
        public Dictionary<string, Frame> CurrentGeometryFrames { get; private set; }
        public List<string> CompletedGoals { get; private set; }
        public string RobotName { get; private set; }

        public PostInferenceTargetRequestMessage(List<string> completedGoals, string inferenceGoalName, string targetName, string robotName, Dictionary<string, Frame> currentGeometryFrames, Header header = null)
        {
            Header = header ?? new Header();
            CompletedGoals = completedGoals;
            InferenceGoalName = inferenceGoalName;
            TargetName = targetName;
            CurrentGeometryFrames = currentGeometryFrames;
            RobotName = robotName;
        }
        public static List<JointTrajectoryPoint> _GetJointTrajectoryPoints(List<Trajectory> trajectories)
        {
            if (trajectories.Count == 0)
            {
                Debug.LogWarning("InferenceResultMessage: No trajectories found in the message returning null list.");
                return new List<JointTrajectoryPoint>();
            }
            else
            {
                Debug.Log($"InferenceResultMessage: Found {trajectories.Count} trajectories in the message.");
                return MessageHandelingExtensions._getCombinedTrajectoryPointsFromTrajectoryList(trajectories);
            }
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "inference_goal_name", InferenceGoalName },
                { "target_name", TargetName },
                { "geometry_frames",  MessageHandelingExtensions._getDataFromFramesDictionary(CurrentGeometryFrames) },
                { "robot_name", RobotName },
                { "completed_goals", CompletedGoals }
            };
        }

        public static PostInferenceTargetRequestMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */

            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var robotName = jsonObject["robot_name"].ToString();
            var completedGoals = DictionaryHelpers.GetStringListFromDict(jsonObject, "completed_goals");
            Debug.Log($"PostInferenceTargetRequestMessage : Completed goals: {JsonConvert.SerializeObject(completedGoals)}"); // ["G0","G1"]
            Debug.Log($"PostInferenceTargetRequestMessage : Completed goals type: {completedGoals.GetType()}");

            var inferenceGoalName = (jsonObject.TryGetValue("inference_goal_name", out var ig) && ig != null)
                ? ig.ToString()
                : null;

            if (string.IsNullOrEmpty(inferenceGoalName))
            {
                Debug.LogError("PostInferenceTargetRequestMessage: Parse: Inference guess is null or empty.");
            }

            var targetName = jsonObject["target_name"] != null ? jsonObject["target_name"].ToString() : null;
            if (string.IsNullOrEmpty(targetName))
            {
                Debug.LogWarning("PostInferenceTargetRequestMessage: Parse: Suggested target name is null or empty.");
            }
            else
            {
                Debug.Log($"PostInferenceTargetRequestMessage: Suggested Target Name Parsed Successfully: {targetName}");
            }

            // geometry_frames: now a dictionary: name -> frameData
            var geometryFrames = new Dictionary<string, Frame>();
            if (jsonObject.TryGetValue("geometry_frames", out var framesObj) && framesObj != null)
            {
                // Expecting: { "geometry_frames": { "frameA": {...}, "frameB": {...}, ... } }
                var framesDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(
                    framesObj.ToString()
                );

                if (framesDict != null)
                {
                    foreach (var kv in framesDict)
                    {
                        var frameName = kv.Key;
                        var frameData = kv.Value; // Dictionary<string, object> for one frame

                        try
                        {
                            geometryFrames[frameName] = Frame.FromData(frameData);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"InferenceRequestMessage.Parse: Skipping frame '{frameName}': {ex.Message}");
                        }
                    }
                }
            }
            return new PostInferenceTargetRequestMessage(completedGoals, inferenceGoalName, targetName, robotName, geometryFrames, header);
        }
    }


    [System.Serializable] //TODO: CHECK THIS.
    public class PostInferenceTrajectoryResultMessage
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public List<Trajectory> Trajectories { get; private set; }
        public Frame RobotBaseFrame { get; private set; }
        public List<JointTrajectoryPoint> CombinedTrajectoryPoints { get; private set; }
        public string RobotName { get; private set; }
        public string InferenceGoalName { get; set; }
        public string TargetName { get; set; }

        public PostInferenceTrajectoryResultMessage(List<Trajectory> trajectories, string inferenceGoalName = null, string targetName = null, Frame robotBaseFrame = null, string robotName = null, Header header = null)
        {
            Header = header ?? new Header();
            Trajectories = trajectories;
            if (trajectories.Count > 0)
            {
                CombinedTrajectoryPoints = _GetJointTrajectoryPoints(trajectories);
            }
            else
            {
                CombinedTrajectoryPoints = new List<JointTrajectoryPoint>();
            }
            RobotBaseFrame = robotBaseFrame;
            RobotName = robotName;
            InferenceGoalName = inferenceGoalName;
            TargetName = targetName;
        }

        public static List<JointTrajectoryPoint> _GetJointTrajectoryPoints(List<Trajectory> trajectories)
        {
            if (trajectories.Count == 0)
            {
                Debug.LogWarning("InferenceResultMessage: No trajectories found in the message returning null list.");
                return new List<JointTrajectoryPoint>();
            }
            else
            {
                Debug.Log($"InferenceResultMessage: Found {trajectories.Count} trajectories in the message.");
                return MessageHandelingExtensions._getCombinedTrajectoryPointsFromTrajectoryList(trajectories);
            }
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "trajectories", MessageHandelingExtensions._getTrajectoriesDataFromList(Trajectories) },
                { "robot_base_frame", RobotBaseFrame.GetData() },
                { "robot_name", RobotName },
                { "inference_goal_name", InferenceGoalName },
                { "target_name", TargetName }
            };
        }

        //TODO: WORK IN PROGRESS...PLEASE FINISH ME
        public static PostInferenceTrajectoryResultMessage Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */

            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var inferenceGoalName = (jsonObject.TryGetValue("inference_goal_name", out var ig) && ig != null)
                ? ig.ToString()
                : null;

            if (string.IsNullOrEmpty(inferenceGoalName))
            {
                Debug.LogError("PostInferenceTrajectoryResultMessage: Parse: Inference guess is null or empty.");
            }

            //Parse the robot name
            var robotName = jsonObject["robot_name"].ToString();
            var targetName = jsonObject["target_name"] != null ? jsonObject["target_name"].ToString() : null;
            if (string.IsNullOrEmpty(targetName))
            {
                Debug.LogWarning("PostInferenceTrajectoryResultMessage: Parse: Target name is null or empty.");
            }
            else
            {
                Debug.Log($"PostInferenceTrajectoryResultMessage: Target Name Parsed Successfully: {targetName}");
            }

            var trajectoriesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonObject["trajectories"].ToString());
            List<Trajectory> trajectories = new List<Trajectory>();
            if (trajectoriesData.Count > 0)
            {
                int i = 0;
                foreach (Dictionary<string, object> trajectoryData in trajectoriesData)
                {
                    if (trajectoryData.TryGetValue("data", out var trajectoryDataValue))
                    {
                        var trajectoryJson = JsonConvert.SerializeObject(trajectoryDataValue);
                        var trajectoryDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(trajectoryJson);
                        trajectories.Add(Trajectory.FromData(trajectoryDict));
                        Debug.Log($"PostInferenceTrajectoryResultMessage: InferenceResultMessage: Parse: Trajectory {i} parsed successfully.");
                        i++;
                    }
                    else
                    {
                        Debug.LogWarning("PostInferenceTrajectoryResultMessage: Parse: Trajectory data not found in the message.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("PostInferenceTrajectoryResultMessage: Parse: No trajectories found in the message.");
            }

            if (trajectories.Count <= 0)
            {
                Debug.LogWarning("PostInferenceTrajectoryResultMessage: Parse: No trajectories found in the message.");
                return new PostInferenceTrajectoryResultMessage(new List<Trajectory>(), inferenceGoalName, targetName, null, robotName, header);
            }

            Frame robotBaseFrame = null;
            try
            {
                robotBaseFrame = MessageHandelingExtensions._getBaseFrameFromMessage(jsonObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"InferenceResultMessage: Failed to parse Robot Base Frame. Using empty. Error: {ex.Message}");
            }
            if (robotBaseFrame == null)
            {
                Debug.LogWarning("InferenceResultMessage: Robot base frame is null (allowed).");
            }
            else
            {
                Debug.Log($"InferenceResultMessage: Robot Base Frame Parsed Successfully: {JsonConvert.SerializeObject(robotBaseFrame, Formatting.Indented)}");
            }

            //TODO: ALL OF THESE NEED TO DUMP IF IT IS NULL. NOT REACH SOME SORT OF EXCEPTION.
            return new PostInferenceTrajectoryResultMessage(trajectories, inferenceGoalName, targetName, robotBaseFrame, robotName, header);
        }
    }

}
