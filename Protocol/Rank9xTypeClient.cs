//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: Rank9xTypeClient.proto
// Note: requires additional types generated from: CommonData.proto
// Note: requires additional types generated from: MessageData.proto
// Note: requires additional types generated from: ServerData.proto
namespace DataContract
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_GetRankList_RET_RankList__")]
  public partial class __RPC_Rank_GetRankList_RET_RankList__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_GetRankList_RET_RankList__() {}
    

    private DataContract.RankList _ReturnValue = null;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ReturnValue", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public DataContract.RankList ReturnValue
    {
      get { return _ReturnValue; }
      set { _ReturnValue = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_GetRankList_ARG_int32_serverId_int32_rankType__")]
  public partial class __RPC_Rank_GetRankList_ARG_int32_serverId_int32_rankType__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_GetRankList_ARG_int32_serverId_int32_rankType__() {}
    

    private int _ServerId = default(int);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ServerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ServerId
    {
      get { return _ServerId; }
      set { _ServerId = value; }
    }

    private int _RankType = default(int);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"RankType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int RankType
    {
      get { return _RankType; }
      set { _RankType = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_GMRank_RET_int32__")]
  public partial class __RPC_Rank_GMRank_RET_int32__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_GMRank_RET_int32__() {}
    

    private int _ReturnValue = default(int);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ReturnValue", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ReturnValue
    {
      get { return _ReturnValue; }
      set { _ReturnValue = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_GMRank_ARG_string_commond__")]
  public partial class __RPC_Rank_GMRank_ARG_string_commond__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_GMRank_ARG_string_commond__() {}
    

    private string _Commond = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"Commond", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Commond
    {
      get { return _Commond; }
      set { _Commond = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_ApplyServerActivityData_RET_ServerActivityDatas__")]
  public partial class __RPC_Rank_ApplyServerActivityData_RET_ServerActivityDatas__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_ApplyServerActivityData_RET_ServerActivityDatas__() {}
    

    private DataContract.ServerActivityDatas _ReturnValue = null;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ReturnValue", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public DataContract.ServerActivityDatas ReturnValue
    {
      get { return _ReturnValue; }
      set { _ReturnValue = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_ApplyServerActivityData_ARG_int32_serverId__")]
  public partial class __RPC_Rank_ApplyServerActivityData_ARG_int32_serverId__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_ApplyServerActivityData_ARG_int32_serverId__() {}
    

    private int _ServerId = default(int);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ServerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ServerId
    {
      get { return _ServerId; }
      set { _ServerId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_GetFightRankList_RET_RankList__")]
  public partial class __RPC_Rank_GetFightRankList_RET_RankList__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_GetFightRankList_RET_RankList__() {}
    

    private DataContract.RankList _ReturnValue = null;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ReturnValue", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public DataContract.RankList ReturnValue
    {
      get { return _ReturnValue; }
      set { _ReturnValue = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"__RPC_Rank_GetFightRankList_ARG_int32_serverId_int32_rankType__")]
  public partial class __RPC_Rank_GetFightRankList_ARG_int32_serverId_int32_rankType__ : global::ProtoBuf.IExtensible
  {
    public __RPC_Rank_GetFightRankList_ARG_int32_serverId_int32_rankType__() {}
    

    private int _ServerId = default(int);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"ServerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ServerId
    {
      get { return _ServerId; }
      set { _ServerId = value; }
    }

    private int _RankType = default(int);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"RankType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int RankType
    {
      get { return _RankType; }
      set { _RankType = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}