
using UnityEngine;


namespace GameCore
{
    public class UIEvent : MonoBehaviour
    {
        [SerializeField]
        public int id;
        [SerializeField]
        public Collider col = null;
        [SerializeField]
        public UIFrame frame = null;
        protected void Awake() { if (col == null) col = GetComponent<BoxCollider>(); }
        protected void OnClick() { UIMgr.Instance.OnClick(frame, gameObject, id); }
        protected void OnPress(bool press) { UIMgr.Instance.OnPress(frame, gameObject, press, id); }
        protected void OnSelect(bool select) { UIMgr.Instance.OnSelect(frame, gameObject, select, id); }
        protected void OnDragStart() { UIMgr.Instance.OnDragStart(frame, gameObject, id); }
        protected void OnDrag(Vector2 delta) { UIMgr.Instance.OnDrag(frame, gameObject, delta, id); }
        protected void OnDragEnd() { UIMgr.Instance.OnDragEnd(frame, gameObject, id); }
        protected void OnDoubleClick() { UIMgr.Instance.OnDoubleClick(frame, gameObject, id); }
    }
}
