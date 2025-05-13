using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using Unity.VisualScripting;


public class URDFParser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        GameObject urdfObject = new GameObject("URDFObject");
        urdfObject.transform.position = new Vector3(0, 0, 0);
        urdfObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        UrdfRobot urdfGameObjectScript = urdfObject.AddComponent<UrdfRobot>();
        urdfGameObjectScript.SetRigidbodiesIsKinematic(true);
        urdfGameObjectScript.SetRigidbodiesUseGravity(false);
        urdfGameObjectScript.SetUseUrdfInertiaData(false);

        GameObject jointObject = new GameObject("JointObject");
        jointObject.transform.position = new Vector3(0, 0, 0);
        jointObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        UrdfJoint urdfJointScript = jointObject.AddComponent<UrdfJointRevolute>();
        UrdfJointRevolute urdfJointRevoluteScript = jointObject.GetComponent<UrdfJointRevolute>();
        // urdfJointRevoluteScript.
        jointObject.transform.parent = urdfObject.transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
