namespace NetVRTK
{
	using UnityEditor;

	[CustomEditor(typeof(NetworkGrabManager))]
    [CanEditMultipleObjects]
    public class NetworkGrabManagerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            NetworkGrabManager ngm = (NetworkGrabManager)target;
            EditorGUILayout.LabelField("Grab Owner", ngm.currentGrabOwner.ToString());
        }
    }
}
