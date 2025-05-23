using System.Collections;
using System.Collections.Generic;
using CompasXR.Core;
using CompasXR.Core.Data;
using CompasXR.RoboticTerritories.Data;
using UnityEngine;
using UnityEngine.UI;
using CompasXR.Core.Extentions;


public class ZoneHapticsManager : MonoBehaviour
{
    public DatabaseManager databaseManager;

    public ProjectZones projectZones;
    public AudioClip entrySound;
    public AudioClip exitSound;
    public bool isInsideHumanMimicZoneNow = false;

    public bool wasInsideHumanMimicZoneLastFrame = false;
    AudioSource MimicZoneAudioSource;
    public Camera arCamera;

    // Start is called before the first frame update
    void Start()
    {
        OnStartInitilization();
    }

    // Update is called once per frame
    void Update()
    {
        if (databaseManager.ProjectZones.CurrentZone == ProjectZones.CurrentZoneMode.Mimic)
        {
            ZoneHapticsUpdateMethod();
        }
    }
    public void OnStartInitilization()
    {
        //Get the database manager component from the EventManager
        databaseManager = GameObject.Find("DatabaseManager").GetComponentInChildren<DatabaseManager>();

        //Get the mimic zone audio source
        MimicZoneAudioSource = GetComponentInChildren<AudioSource>();

        GameObject ZoneHapticsManagerObject = GameObject.Find("ZoneHapticsManager");
        entrySound = ZoneHapticsManagerObject.FindObject("ZoneEntry").GetComponent<AudioSource>().clip;
        exitSound = ZoneHapticsManagerObject.FindObject("ZoneEntry").GetComponent<AudioSource>().clip;

        if (entrySound == null)
        {
            Debug.LogWarning("ZoneHapticsManager: Entry sound is null. Cannot play sound.");
        }
        if (exitSound == null)
        {
            Debug.LogWarning("ZoneHapticsManager: Exit sound is null. Cannot play sound.");
        }

        //Get the arCamera
        arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();
    }

    public void ZoneHapticsUpdateMethod()
    {

        if (databaseManager == null)
        {
            Debug.Log("ZoneHapticsManager: DatabaseManager is null. Cannot update zones.");
            return;
        }
        if (databaseManager.ProjectZones == null)
        {
            Debug.Log("ZoneHapticsManager: ProjectZones is null. Cannot update zones.");
            return;
        }
        if (databaseManager.ProjectZones.MimicZones == null)
        {
            Debug.Log("ZoneHapticsManager: MimicZones is null. Cannot update zones. MimicZones Count: " + databaseManager.ProjectZones.MimicZones.Count);
            return;
        }

        var mimicZones = databaseManager.ProjectZones.MimicZones;

        if (mimicZones.TryGetValue("human_zone", out Zone humanZone))
        {
            GameObject humanZoneObject = humanZone.ZoneObject;

            // Additional logic for both zones can go here
            Vector3 cameraPositionObject = arCamera.transform.position;
            SoundControlerForZoneEntry(cameraPositionObject, humanZoneObject, ref isInsideHumanMimicZoneNow, ref wasInsideHumanMimicZoneLastFrame);
        }
        else
        {
            Debug.LogError("SetMimicPoint: 'human_zone' key not found in MimicZones.");
        }

    }

    public void SoundControlerForZoneEntry(Vector3 cameraPosition, GameObject zoneObject, ref bool isInsideZone, ref bool wasInsideHumanMimicZoneLastFrame)
    {
        // Play sound for zone entry
        // Debug.Log("Playing sound for zone entry at position: " + position);
        isInsideZone = ObjectInstantiaion.IsPositionWithinObject(zoneObject, cameraPosition);


        if (isInsideZone && !wasInsideHumanMimicZoneLastFrame)
        {
            // Play sound for entering the zone
            Debug.Log("ZoneHapticsManager: Entered zone. Playing entry sound.");
            // Add your sound playing logic here
            PlayEntrySoundFromAudioSource(ref MimicZoneAudioSource, entrySound);
        }
        else if (!isInsideZone && wasInsideHumanMimicZoneLastFrame)
        {
            // Play sound for exiting the zone
            Debug.Log("ZoneHapticsManager: Exited zone. Playing exit sound.");
            // Add your sound playing logic here
            PlayEntrySoundFromAudioSource(ref MimicZoneAudioSource, exitSound);
        }

        wasInsideHumanMimicZoneLastFrame = isInsideZone;
    }

    public void PlayEntrySoundFromAudioSource(ref AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
        else
        {
            Debug.LogError("ZoneHapticsControler: AudioSource is null. Cannot play sound.");
        }
    }

}
