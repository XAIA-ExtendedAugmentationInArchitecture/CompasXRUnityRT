using System;
using UnityEngine;
using Firebase.Database;
using CompasXR.Database.FirebaseManagment;
using CompasXR.Robots;

namespace CompasXR.Core
{
    /*
    * CompasXR.Core : Is the Primary namespace for all Classes that
    * controll the primary functionalities of the CompasXR Application.
    */
    public class EventManager : MonoBehaviour
    {
        /*
        * EventManager : Class is used to manage global event listeners and subscriptions and has 2 primary functions.
        * 1. To initialize the application and establish the start up routine for passing information between scripts.
        * 2. To manage the global event listeners and throughout interaction and infomtion change.
        */

        //GameObjects for Script Storage
        public GameObject databaseManagerObject;
        public GameObject instantiateObjectsObject;
        public GameObject checkFirebaseObject;
        public GameObject qrLocalizationObject;
        public GameObject mqttTrajectoryReceiverObject;
        public GameObject trajectoryVisualizerObject;
        public GameObject zoneHapticsManagerObject;

        //Settings Database Reference
        public DatabaseReference dbReferenceSettings;

        //Other Script Components
        public DatabaseManager databaseManager;

        public InstantiationCoordinator instantiationCoordinator = new InstantiationCoordinator();

        //////////////////////////// Monobehaviour Methods //////////////////////////////
        void Awake()
        {
            //Initilization functionalities for the application.
            Caching.ClearCache();
            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            dbReferenceSettings = FirebaseDatabase.DefaultInstance.GetReference("ApplicationSettings");

            //Add script components to objects in the scene
            databaseManager = databaseManagerObject.AddComponent<DatabaseManager>();
            InstantiateObjects instantiateObjects = instantiateObjectsObject.AddComponent<InstantiateObjects>();
            CheckFirebase checkFirebase = checkFirebaseObject.AddComponent<CheckFirebase>();
            QRLocalization qrLocalization = qrLocalizationObject.GetComponent<QRLocalization>();
            MqttTrajectoryManager mqttTrajectoryReceiver = mqttTrajectoryReceiverObject.GetComponent<MqttTrajectoryManager>();
            TrajectoryVisualizer trajectoryVisualizer = trajectoryVisualizerObject.GetComponent<TrajectoryVisualizer>();

            //Establish Global Event Listeners
            checkFirebase.FirebaseInitialized += DBInitializedFetchSettings;
            databaseManager.ApplicationSettingUpdate += databaseManager.FetchRoboticTerritoriesData;
            databaseManager.ApplicationSettingUpdate += mqttTrajectoryReceiver.SetRoboticTerritoriesTopics;
            databaseManager.TrackingDictReceived += qrLocalization.OnTrackingInformationReceived;

            //TODO: Robotic Territories Testing ////////////////////////////////////////////////////////////////////////////////////////////////////
            databaseManager.RobotBaseFrameReceived += trajectoryVisualizer.OnRobotBaseFrameReceived;
            instantiateObjects.InitialZonesPlaced += addZoneHapticsManager;
            //TODO: Robotic Territories Testing ////////////////////////////////////////////////////////////////////////////////////////////////////

            databaseManager.ZonesInfoReceived += instantiateObjects.OnZonesReceived;
            instantiateObjects.InitialZonesPlaced += instantiationCoordinator.OnInitialZonesPlaced;
            instantiateObjects.InitialTrackedGeometryPlaced += instantiationCoordinator.OnInitialTrackedGeometryPlaced;
            instantiationCoordinator.Ready += databaseManager.AddListenersRoboticTerritories;


            databaseManager.ModeZonesUpdate += instantiateObjects.OnModeZonesUpdate;
            databaseManager.FetchedObservedGeometries += instantiateObjects.OnObservedGeometriesFetched;
            databaseManager.UpdateObvservedGeometry += instantiateObjects.OnObservedObjectsChangedWrapper;

            // databaseManager.ApplicationSettingUpdate += databaseManager.FetchData;
            // databaseManager.ApplicationSettingUpdate += mqttTrajectoryReceiver.SetCompasXRTopics;
            // databaseManager.DatabaseInitializedDict += instantiateObjects.OnDatabaseInitializedDict;
            // instantiateObjects.PlacedInitialElements += databaseManager.AddListeners;
            // databaseManager.DatabaseUpdate += instantiateObjects.OnDatabaseUpdate;
            // databaseManager.UserInfoUpdate += instantiateObjects.OnUserInfoUpdate;
        }

        //////////////////////////// Event Methods //////////////////////////////////////
        public void DBInitializedFetchSettings(object sender, EventArgs e)
        {
            /*
            * Method is used to fetch the settings data from the Firebase Database
            * once the connection has been initilized.
            */
            databaseManager.FetchSettingsData(dbReferenceSettings);
        }

        public void addZoneHapticsManager(object sender, EventArgs e)
        {
            /*
            * Method is used to add the zone haptics manager to the event manager
            * once the zones have been placed.
            */
            ZoneHapticsManager zoneHapticsManager = zoneHapticsManagerObject.AddComponent<ZoneHapticsManager>();
        }

        public class InstantiationCoordinator
        {
            private bool _zonesPlaced;
            private bool _otherReady;

            public event EventHandler Ready;

            public void OnInitialZonesPlaced(object sender, EventArgs e)
            {
                _zonesPlaced = true;
                TryFire();
            }

            public void OnInitialTrackedGeometryPlaced(object sender, EventArgs e)
            {
                _otherReady = true;
                TryFire();
            }

            private void TryFire()
            {
                if (_zonesPlaced && _otherReady)
                {
                    Ready?.Invoke(this, EventArgs.Empty);
                    // optional: reset or unsubscribe if you only want it once
                }
            }
        }


    }
}

