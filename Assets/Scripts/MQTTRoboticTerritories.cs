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
using Google.MiniJSON;
using CompasXR.Robots.Data;
using Newtonsoft.Json.Linq;



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
        public string executeTrajectoryRequestTopic { get; set; }
        public RTPublishers(string projectName)
        {
            mimicRequestTopic = $"robotic_territories/mimic_request/{projectName}";
            executeTrajectoryRequestTopic = $"robotic_territories/execute_trajectory/{projectName}";
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

        //Constructer for subscribers that takes an input project name
        public RTSubscribers(string projectName)
        {
            mimicResultTopic = $"robotic_territories/mimic_result/{projectName}";
        }
    }

    // Message classes ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class Header
    {
        /*
        * Header : Class is used to manage the header for Compas XR communication.
        * It is designed to store the sequence ID, response ID, device ID, and timestamp for each message.
        */
        public string DeviceID { get; private set; }
        public string TimeStamp { get; private set; }
        public Header(string deviceID=null, string timeStamp=null)
        {   
            if(deviceID != null && timeStamp != null)
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
        public MimicTrajectoryRequestMessage(List<Frame> humanFrames, List<Frame> robotFrames, string robotName, Header header=null)
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
                { "human_frames", _getFramesData(HumanFrames) },
                { "robot_frames", _getFramesData(RobotFrames) },
                { "robot_name", RobotName }
            };
        }

        public List<Dictionary<string, object>> _getFramesData(List<Frame> Frames)
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

        public List<Frame> _parseFramesData(List<Dictionary<string, object>> framesData)
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

    [System.Serializable] //TODO: CHECK THIS.
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
        public MimicTrajectoryResultMessage(List<Trajectory> trajectories, Frame robotBaseFrame, string robotName, Header header=null)
        {
            Header = header ?? new Header();
            Trajectories = trajectories;
            CombinedTrajectoryPoints = _getCombinedTrajectoryPoints(trajectories);
            RobotBaseFrame = robotBaseFrame;
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
                { "trajectories", _getTrajectoriesData(Trajectories) },
                { "robot_base_frame", RobotBaseFrame.GetData() },
                { "robot_name", RobotName }
            };
        }
        private List<JointTrajectoryPoint> _getCombinedTrajectoryPoints(List<Trajectory> Trajectories)
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            List<JointTrajectoryPoint> combinedTrajectoryPoints = new List<JointTrajectoryPoint>();
            foreach (Trajectory trajectory in Trajectories)
            {
                combinedTrajectoryPoints.AddRange(trajectory.Points); //TODO: THE ERROR HAPPENS HERE.
            }
            return combinedTrajectoryPoints;
        }
        private List<Dictionary<string, object>> _getTrajectoriesData(List<Trajectory> Trajectories)
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

            Frame robotBaseFrame = _getBaseFrameFromMessage(jsonObject);
            if(robotBaseFrame == null)
            {
                Debug.LogWarning("MimicTrajectoryResultMessage: Parse: Robot base frame not found in the message.");
            }
            else
            {
                Debug.Log($"MimicTrajectoryResultMessage: Robot Base Frame Parsed Successfully: {JsonConvert.SerializeObject(robotBaseFrame)}");
            }

            //Parse the robot name
            var robotName = jsonObject["robot_name"].ToString();

            //TODO: Return the message
            return new MimicTrajectoryResultMessage(trajectories, robotBaseFrame, robotName, header);
        }

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
    }

}
