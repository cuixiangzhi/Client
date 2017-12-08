using UnityEngine;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {
        private void Awake()
        {
            Matrix4x4 sca = Matrix4x4.Scale(Vector3.one);
            Matrix4x4 eul = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0,60,0)), Vector3.one);
            Matrix4x4 pos = Matrix4x4.Translate(Vector3.one);

            Matrix4x4 v1 = Matrix4x4.TRS(Vector3.one, Quaternion.Euler(new Vector3(0, 60, 0)), Vector3.one);
            Matrix4x4 v2 = sca * eul * pos;
            int i = 0;
        }
    }
}
