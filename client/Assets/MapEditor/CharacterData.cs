//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: CharacterData.proto
namespace CharacterData
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CharacterDatatable")]
  public partial class CharacterDatatable : global::ProtoBuf.IExtensible
  {
    public CharacterDatatable() {}
    
    private string _tname = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"tname", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string tname
    {
      get { return _tname; }
      set { _tname = value; }
    }
    private readonly global::System.Collections.Generic.List<CharacterData> _tlist = new global::System.Collections.Generic.List<CharacterData>();
    [global::ProtoBuf.ProtoMember(2, Name=@"tlist", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<CharacterData> tlist
    {
      get { return _tlist; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CharacterData")]
  public partial class CharacterData : global::ProtoBuf.IExtensible
  {
    public CharacterData() {}
    
    private int _ID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"ID", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int ID
    {
      get { return _ID; }
      set { _ID = value; }
    }
    private string _characterName = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"characterName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string characterName
    {
      get { return _characterName; }
      set { _characterName = value; }
    }
    private int _actorType = default(int);
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"actorType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int actorType
    {
      get { return _actorType; }
      set { _actorType = value; }
    }
    private int _country = default(int);
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"country", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int country
    {
      get { return _country; }
      set { _country = value; }
    }
    private int _battleInfo = default(int);
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"battleInfo", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int battleInfo
    {
      get { return _battleInfo; }
      set { _battleInfo = value; }
    }
    private int _battleAttr = default(int);
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"battleAttr", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int battleAttr
    {
      get { return _battleAttr; }
      set { _battleAttr = value; }
    }
    private int _PositionIndex = default(int);
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"PositionIndex", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int PositionIndex
    {
      get { return _PositionIndex; }
      set { _PositionIndex = value; }
    }
    private bool _isShow = default(bool);
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"isShow", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(default(bool))]
    public bool isShow
    {
      get { return _isShow; }
      set { _isShow = value; }
    }
    private int _SelectSound = default(int);
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"SelectSound", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int SelectSound
    {
      get { return _SelectSound; }
      set { _SelectSound = value; }
    }
    private int _EnterStageSound = default(int);
    [global::ProtoBuf.ProtoMember(10, IsRequired = false, Name=@"EnterStageSound", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int EnterStageSound
    {
      get { return _EnterStageSound; }
      set { _EnterStageSound = value; }
    }
    private int _VictorySound = default(int);
    [global::ProtoBuf.ProtoMember(11, IsRequired = false, Name=@"VictorySound", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int VictorySound
    {
      get { return _VictorySound; }
      set { _VictorySound = value; }
    }
    private int _DieSound = default(int);
    [global::ProtoBuf.ProtoMember(12, IsRequired = false, Name=@"DieSound", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int DieSound
    {
      get { return _DieSound; }
      set { _DieSound = value; }
    }
    private int _AdeptType = default(int);
    [global::ProtoBuf.ProtoMember(13, IsRequired = false, Name=@"AdeptType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int AdeptType
    {
      get { return _AdeptType; }
      set { _AdeptType = value; }
    }
    private int _Appellation = default(int);
    [global::ProtoBuf.ProtoMember(14, IsRequired = false, Name=@"Appellation", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int Appellation
    {
      get { return _Appellation; }
      set { _Appellation = value; }
    }
    private string _Des = "";
    [global::ProtoBuf.ProtoMember(15, IsRequired = false, Name=@"Des", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Des
    {
      get { return _Des; }
      set { _Des = value; }
    }
    private float _Height = default(float);
    [global::ProtoBuf.ProtoMember(16, IsRequired = false, Name=@"Height", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float Height
    {
      get { return _Height; }
      set { _Height = value; }
    }
    private float _scale = default(float);
    [global::ProtoBuf.ProtoMember(17, IsRequired = false, Name=@"scale", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float scale
    {
      get { return _scale; }
      set { _scale = value; }
    }
    private float _BodyRadius = default(float);
    [global::ProtoBuf.ProtoMember(18, IsRequired = false, Name=@"BodyRadius", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float BodyRadius
    {
      get { return _BodyRadius; }
      set { _BodyRadius = value; }
    }
    private int _Quality = default(int);
    [global::ProtoBuf.ProtoMember(19, IsRequired = false, Name=@"Quality", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int Quality
    {
      get { return _Quality; }
      set { _Quality = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}