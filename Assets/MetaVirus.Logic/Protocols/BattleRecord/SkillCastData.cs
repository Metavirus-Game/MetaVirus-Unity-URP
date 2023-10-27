// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: SkillCastData.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace MetaVirus.Battle.Record {

  /// <summary>Holder for reflection information generated from SkillCastData.proto</summary>
  public static partial class SkillCastDataReflection {

    #region Descriptor
    /// <summary>File descriptor for SkillCastData.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SkillCastDataReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChNTa2lsbENhc3REYXRhLnByb3RvEhdNZXRhVmlydXMuYmF0dGxlLnJlY29y",
            "ZCKaAQoPU2tpbGxDYXN0RGF0YVBiEhAKCHRhcmdldElkGAEgASgFEhIKCnNr",
            "aWxsU3RhdGUYAiABKAUSFgoOc2tpbGxBdHRyaWJ1dGUYAyABKAUSEgoKZWZm",
            "ZWN0QXR0chgEIAEoBRITCgtlZmZlY3RWYWx1ZRgFIAEoBRIRCgl2YWx1ZVR5",
            "cGUYBiABKAUSDQoFaW5kZXgYByABKAVCMwobY29tLm1ldGF2aXJ1cy5iYXR0",
            "bGUucmVjb3JkQhJTa2lsbENhc3REYXRhUEJPdXRQAWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::MetaVirus.Battle.Record.SkillCastDataPb), global::MetaVirus.Battle.Record.SkillCastDataPb.Parser, new[]{ "TargetId", "SkillState", "SkillAttribute", "EffectAttr", "EffectValue", "ValueType", "Index" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class SkillCastDataPb : pb::IMessage<SkillCastDataPb>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SkillCastDataPb> _parser = new pb::MessageParser<SkillCastDataPb>(() => new SkillCastDataPb());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SkillCastDataPb> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::MetaVirus.Battle.Record.SkillCastDataReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SkillCastDataPb() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SkillCastDataPb(SkillCastDataPb other) : this() {
      targetId_ = other.targetId_;
      skillState_ = other.skillState_;
      skillAttribute_ = other.skillAttribute_;
      effectAttr_ = other.effectAttr_;
      effectValue_ = other.effectValue_;
      valueType_ = other.valueType_;
      index_ = other.index_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SkillCastDataPb Clone() {
      return new SkillCastDataPb(this);
    }

    /// <summary>Field number for the "targetId" field.</summary>
    public const int TargetIdFieldNumber = 1;
    private int targetId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int TargetId {
      get { return targetId_; }
      set {
        targetId_ = value;
      }
    }

    /// <summary>Field number for the "skillState" field.</summary>
    public const int SkillStateFieldNumber = 2;
    private int skillState_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int SkillState {
      get { return skillState_; }
      set {
        skillState_ = value;
      }
    }

    /// <summary>Field number for the "skillAttribute" field.</summary>
    public const int SkillAttributeFieldNumber = 3;
    private int skillAttribute_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int SkillAttribute {
      get { return skillAttribute_; }
      set {
        skillAttribute_ = value;
      }
    }

    /// <summary>Field number for the "effectAttr" field.</summary>
    public const int EffectAttrFieldNumber = 4;
    private int effectAttr_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int EffectAttr {
      get { return effectAttr_; }
      set {
        effectAttr_ = value;
      }
    }

    /// <summary>Field number for the "effectValue" field.</summary>
    public const int EffectValueFieldNumber = 5;
    private int effectValue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int EffectValue {
      get { return effectValue_; }
      set {
        effectValue_ = value;
      }
    }

    /// <summary>Field number for the "valueType" field.</summary>
    public const int ValueTypeFieldNumber = 6;
    private int valueType_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int ValueType {
      get { return valueType_; }
      set {
        valueType_ = value;
      }
    }

    /// <summary>Field number for the "index" field.</summary>
    public const int IndexFieldNumber = 7;
    private int index_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Index {
      get { return index_; }
      set {
        index_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SkillCastDataPb);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SkillCastDataPb other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (TargetId != other.TargetId) return false;
      if (SkillState != other.SkillState) return false;
      if (SkillAttribute != other.SkillAttribute) return false;
      if (EffectAttr != other.EffectAttr) return false;
      if (EffectValue != other.EffectValue) return false;
      if (ValueType != other.ValueType) return false;
      if (Index != other.Index) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (TargetId != 0) hash ^= TargetId.GetHashCode();
      if (SkillState != 0) hash ^= SkillState.GetHashCode();
      if (SkillAttribute != 0) hash ^= SkillAttribute.GetHashCode();
      if (EffectAttr != 0) hash ^= EffectAttr.GetHashCode();
      if (EffectValue != 0) hash ^= EffectValue.GetHashCode();
      if (ValueType != 0) hash ^= ValueType.GetHashCode();
      if (Index != 0) hash ^= Index.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (TargetId != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(TargetId);
      }
      if (SkillState != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(SkillState);
      }
      if (SkillAttribute != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(SkillAttribute);
      }
      if (EffectAttr != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(EffectAttr);
      }
      if (EffectValue != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(EffectValue);
      }
      if (ValueType != 0) {
        output.WriteRawTag(48);
        output.WriteInt32(ValueType);
      }
      if (Index != 0) {
        output.WriteRawTag(56);
        output.WriteInt32(Index);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (TargetId != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(TargetId);
      }
      if (SkillState != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(SkillState);
      }
      if (SkillAttribute != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(SkillAttribute);
      }
      if (EffectAttr != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(EffectAttr);
      }
      if (EffectValue != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(EffectValue);
      }
      if (ValueType != 0) {
        output.WriteRawTag(48);
        output.WriteInt32(ValueType);
      }
      if (Index != 0) {
        output.WriteRawTag(56);
        output.WriteInt32(Index);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (TargetId != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(TargetId);
      }
      if (SkillState != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(SkillState);
      }
      if (SkillAttribute != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(SkillAttribute);
      }
      if (EffectAttr != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(EffectAttr);
      }
      if (EffectValue != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(EffectValue);
      }
      if (ValueType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ValueType);
      }
      if (Index != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Index);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SkillCastDataPb other) {
      if (other == null) {
        return;
      }
      if (other.TargetId != 0) {
        TargetId = other.TargetId;
      }
      if (other.SkillState != 0) {
        SkillState = other.SkillState;
      }
      if (other.SkillAttribute != 0) {
        SkillAttribute = other.SkillAttribute;
      }
      if (other.EffectAttr != 0) {
        EffectAttr = other.EffectAttr;
      }
      if (other.EffectValue != 0) {
        EffectValue = other.EffectValue;
      }
      if (other.ValueType != 0) {
        ValueType = other.ValueType;
      }
      if (other.Index != 0) {
        Index = other.Index;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            TargetId = input.ReadInt32();
            break;
          }
          case 16: {
            SkillState = input.ReadInt32();
            break;
          }
          case 24: {
            SkillAttribute = input.ReadInt32();
            break;
          }
          case 32: {
            EffectAttr = input.ReadInt32();
            break;
          }
          case 40: {
            EffectValue = input.ReadInt32();
            break;
          }
          case 48: {
            ValueType = input.ReadInt32();
            break;
          }
          case 56: {
            Index = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            TargetId = input.ReadInt32();
            break;
          }
          case 16: {
            SkillState = input.ReadInt32();
            break;
          }
          case 24: {
            SkillAttribute = input.ReadInt32();
            break;
          }
          case 32: {
            EffectAttr = input.ReadInt32();
            break;
          }
          case 40: {
            EffectValue = input.ReadInt32();
            break;
          }
          case 48: {
            ValueType = input.ReadInt32();
            break;
          }
          case 56: {
            Index = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
