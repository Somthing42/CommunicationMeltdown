using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AvatarCameraRigSync))]
public class PlayerModel : MonoBehaviour {

	void Start () 
	{
		RootMotion.FinalIK.VRIK VRIK = GetComponent<RootMotion.FinalIK.VRIK> ();

		GameObject Player = GameObject.FindGameObjectWithTag ("Player");
		PlayerRig Rig = Player.GetComponent<PlayerRig> ();
		AvatarCameraRigSync ACRS = GetComponent<AvatarCameraRigSync> ();

		VRIK.solver.spine.headTarget = Rig.Head;
		ACRS.AvatarHead = Rig.Head.gameObject;
		VRIK.solver.leftArm.target = Rig.Left;
		ACRS.LeftHand = Rig.Left.gameObject;
		VRIK.solver.rightArm.target = Rig.Right;
		ACRS.RightHand = Rig.Right.gameObject; 
	}
}
