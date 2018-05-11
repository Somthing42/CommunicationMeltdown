using UnityEngine;

public class ControllerScript : MonoBehaviour
{
	private SteamVR_TrackedObject trackedObj;
	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}

	public GameObject pointerPrefab;
	[HideInInspector]
	public GameObject pointerObject;
	[HideInInspector]
	public Transform pointerTransform;
	[HideInInspector]
	public Vector3 hitPoint;
	public Vector3 teleportDestination;
	public GameObject CameraRig;
    public GameObject Player;
	public GameObject temp;
    //public LayerMask teleportPointMask;

    private PlayerTeleportHandler PTH; 
    
	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}

	void Start()
	{
		pointerObject = Instantiate(pointerPrefab);
		pointerTransform = pointerObject.transform;
        PTH = CameraRig.GetComponent<PlayerTeleportHandler>();
	}

 
	void Update()
	{
        
		if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			//GameManager.Instance.showTeleportPoints();
		}


		if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
		{
			RaycastHit hit;

			if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100))
			{

                //if (hit.transform.gameObject.GetComponent<TeleportPoint>() != null)
                if(hit.collider.gameObject.tag == "Floor")
                {
                    temp = hit.transform.gameObject;
					//hit.transform.gameObject.GetComponent<TeleportPoint>().HighlightGreen();
					//Debug.Log("test");

                    teleportDestination = hit.point; // store where it sends you

                    
				}
				
				hitPoint = hit.point;
				EnablePointer(hit);
			}

		}
		else
		{
			pointerObject.SetActive(false);
		}

		// 3
		if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
		{
            GameManager.Instance.infoPanel.AddLine(temp.name + " was it with tag " + temp.tag);
            if (teleportDestination != Vector3.zero && PTH.CanTeleport)
			{
                //GameManager.Instance.infoPanel.AddLine("Teleporting");

                VRTK.VRTK_HeightAdjustTeleport TeleScript = CameraRig.GetComponent<VRTK.VRTK_HeightAdjustTeleport>();
                Quaternion Rot = Quaternion.identity;

                RotationZone RotZone = null;
                foreach (RotationZone rz in GameManager.Instance.RotationZones)
                {
                    if (rz.InsideZone(teleportDestination))
                    {
                        //GameManager.Instance.infoPanel.AddLine("RZ " + rz.name);
                        RotZone = rz;
                        break;
                    }
                }

                if (RotZone != null)
                {
                    //GameManager.Instance.infoPanel.AddLine("Got rot point");
                    if (RotZone.Type == RotationZone.ZoneType.AROUND_POINT)
                    {

                        Vector3 targetDir = RotZone.Orinator.transform.position - teleportDestination;
                        Vector3 newDir = Vector3.RotateTowards(CameraRig.transform.forward, targetDir, 1.0f, 0.0f);
                        newDir.y = 0;
                        Rot = Quaternion.LookRotation(newDir);
                        //GameManager.Instance.infoPanel.AddLine("Around point " + targetDir.ToString() + " " + 
                            //newDir.ToString() + " " + Rot.ToString());
                        //Rot.x = 0;
                        //Rot.z = 0;
                    }
                    else if (RotZone.Type == RotationZone.ZoneType.FORWARD_DIR)
                    {
                        GameManager.Instance.infoPanel.AddLine("Forward Dir");
                        Rot = RotZone.Orinator.transform.rotation;

                    }
                
                }
                //GameManager.Instance.infoPanel.AddLine("Got here");
                TeleScript.Teleport(temp.transform, teleportDestination, Rot, true);
                PTH.StartCoroutine("TeleportDelay", 1.0f);
            }
            //GameManager.Instance.resetTeleportPoint();
			teleportDestination = Vector3.zero;
		}



	}

	private void EnablePointer(RaycastHit hit)
	{
		pointerObject.SetActive(true);
		pointerTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, 0.5f);
		pointerTransform.LookAt(hitPoint);
		pointerTransform.localScale = new Vector3(pointerTransform.localScale.x, pointerTransform.localScale.y, hit.distance);
	}

}
