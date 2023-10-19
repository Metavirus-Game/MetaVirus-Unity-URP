// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: protos/WorldMessages/common/PBFormationData.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace MetaVirus.Net.Messages.Common {

  /// <summary>Holder for reflection information generated from protos/WorldMessages/common/PBFormationData.proto</summary>
  public static partial class PBFormationDataReflection {

    #region Descriptor
    /// <summary>File descriptor for protos/WorldMessages/common/PBFormationData.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PBFormationDataReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjFwcm90b3MvV29ybGRNZXNzYWdlcy9jb21tb24vUEJGb3JtYXRpb25EYXRh",
            "LnByb3RvEh1NZXRhVmlydXMuTmV0Lk1lc3NhZ2VzLkNvbW1vbiKYAQoPUEJG",
            "b3JtYXRpb25EYXRhEhgKC2Zvcm1hdGlvbklkGAEgASgFSACIAQESHAoPZm9y",
            "bWF0aW9uRGF0YUlkGAIgASgFSAGIAQESEQoEbmFtZRgDIAEoCUgCiAEBEg0K",
            "BXNsb3RzGAQgAygFQg4KDF9mb3JtYXRpb25JZEISChBfZm9ybWF0aW9uRGF0",
            "YUlkQgcKBV9uYW1lQjwKJGNvbS5tZXRhdmlydXMubmV0Lm1lc3NhZ2VzLnBi",
            "LmNvbW1vbkISRm9ybWF0aW9uRGF0YVBiT3V0UAFiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::MetaVirus.Net.Messages.Common.PBFormationData), global::MetaVirus.Net.Messages.Common.PBFormationData.Parser, new[]{ "FormationId", "FormationDataId", "Name", "Slots" }, new[]{ "FormationId", "FormationDataId", "Name" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBFormationData : pb::IMessage<PBFormationData>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBFormationData> _parser = new pb::MessageParser<PBFormationData>(() => new PBFormationData());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBFormationData> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::MetaVirus.Net.Messages.Common.PBFormationDataReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBFormationData() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBFormationData(PBFormationData other) : this() {
      _hasBits0 = other._hasBits0;
      formationId_ = other.formationId_;
      formationDataId_ = other.formationDataId_;
      name_ = other.name_;
      slots_ = other.slots_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBFormationData Clone() {
      return new PBFormationData(this);
    }

    /// <summary>Field number for the "formationId" field.</summary>
    public const int FormationIdFieldNumber = 1;
    private int formationId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int FormationId {
      get { if ((_hasBits0 & 1) != 0) { return formationId_; } else { return 0; } }
      set {
        _hasBits0 |= 1;
        formationId_ = value;
      }
    }
    /// <summary>Gets whether the "formationId" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasFormationId {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "formationId" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearFormationId() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "formationDataId" field.</summary>
    public const int FormationDataIdFieldNumber = 2;
    private int formationDataId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int FormationDataId {
      get { if ((_hasBits0 & 2) != 0) { return formationDataId_; } else { return 0; } }
      set {
        _hasBits0 |= 2;
        formationDataId_ = value;
      }
    }
    /// <summary>Gets whether the "formationDataId" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasFormationDataId {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "formationDataId" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearFormationDataId() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 3;
    private string name_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Name {
      get { return name_ ?? ""; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "name" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasName {
      get { return name_ != null; }
    }
    /// <summary>Clears the value of the "name" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearName() {
      name_ = null;
    }

    /// <summary>Field number for the "slots" field.</summary>
    public const int SlotsFieldNumber = 4;
    private static readonly pb::FieldCodec<int> _repeated_slots_codec
        = pb::FieldCodec.ForInt32(34);
    private readonly pbc::RepeatedField<int> slots_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<int> Slots {
      get { return slots_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBFormationData);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBFormationData other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (FormationId != other.FormationId) return false;
      if (FormationDataId != other.FormationDataId) return false;
      if (Name != other.Name) return false;
      if(!slots_.Equals(other.slots_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasFormationId) hash ^= FormationId.GetHashCode();
      if (HasFormationDataId) hash ^= FormationDataId.GetHashCode();
      if (HasName) hash ^= Name.GetHashCode();
      hash ^= slots_.GetHashCode();
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
      if (HasFormationId) {
        output.WriteRawTag(8);
        output.WriteInt32(FormationId);
      }
      if (HasFormationDataId) {
        output.WriteRawTag(16);
        output.WriteInt32(FormationDataId);
      }
      if (HasName) {
        output.WriteRawTag(26);
        output.WriteString(Name);
      }
      slots_.WriteTo(output, _repeated_slots_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasFormationId) {
        output.WriteRawTag(8);
        output.WriteInt32(FormationId);
      }
      if (HasFormationDataId) {
        output.WriteRawTag(16);
        output.WriteInt32(FormationDataId);
      }
      if (HasName) {
        output.WriteRawTag(26);
        output.WriteString(Name);
      }
      slots_.WriteTo(ref output, _repeated_slots_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (HasFormationId) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(FormationId);
      }
      if (HasFormationDataId) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(FormationDataId);
      }
      if (HasName) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      size += slots_.CalculateSize(_repeated_slots_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBFormationData other) {
      if (other == null) {
        return;
      }
      if (other.HasFormationId) {
        FormationId = other.FormationId;
      }
      if (other.HasFormationDataId) {
        FormationDataId = other.FormationDataId;
      }
      if (other.HasName) {
        Name = other.Name;
      }
      slots_.Add(other.slots_);
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
            FormationId = input.ReadInt32();
            break;
          }
          case 16: {
            FormationDataId = input.ReadInt32();
            break;
          }
          case 26: {
            Name = input.ReadString();
            break;
          }
          case 34:
          case 32: {
            slots_.AddEntriesFrom(input, _repeated_slots_codec);
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
            FormationId = input.ReadInt32();
            break;
          }
          case 16: {
            FormationDataId = input.ReadInt32();
            break;
          }
          case 26: {
            Name = input.ReadString();
            break;
          }
          case 34:
          case 32: {
            slots_.AddEntriesFrom(ref input, _repeated_slots_codec);
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
