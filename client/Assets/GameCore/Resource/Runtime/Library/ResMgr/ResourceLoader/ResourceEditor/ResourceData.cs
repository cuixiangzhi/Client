namespace com.cyou.plugin.resource.loader.editor
{
    public class ResourceData
    {
        //

        public UnityEngine.Object resObject = null;
        public UnityEngine.Object[] resObjectArray = null;
        //
        public bool remove = false;
        //
        public EditorResConfig config = null;
        public EditorResConfig[] configArray = null;
        public bool isInstantiate = false;
        public bool isActive = false;
        public int index = -1;

        private bool Valid = true;
        public bool isValid()
        {
            return Valid;
        }
        public ResourceData Valided()
        {
            this.Valid = true;
            return this;
        }
        public void Release()
        {
            remove = false;
            Valid = false;
            resObject = null;
            resObjectArray = null;
            config = null;
            configArray = null;
            isInstantiate = false;
            isActive = false;
            index = -1;
        }
    }

}
