﻿using VRTK;

public class SetupPlayerAvatar : Photon.MonoBehaviour {

	void Awake () {
        if (!photonView.isMine) {
            return;
        }
        // Move the camera rig to where the player was spawned
        VRTK_SDKManager sdk = VRTK_SDKManager.instance;
        sdk.loadedSetup.actualBoundaries.transform.position = transform.position;
        sdk.loadedSetup.actualBoundaries.transform.rotation = transform.rotation;
    }
}
