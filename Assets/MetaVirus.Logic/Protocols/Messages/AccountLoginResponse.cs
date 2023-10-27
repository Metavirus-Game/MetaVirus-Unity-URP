// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: WorldMessages/user/account_login_response.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace MetaVirus.Net.Messages.User {

  /// <summary>Holder for reflection information generated from WorldMessages/user/account_login_response.proto</summary>
  public static partial class AccountLoginResponseReflection {

    #region Descriptor
    /// <summary>File descriptor for WorldMessages/user/account_login_response.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AccountLoginResponseReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Ci9Xb3JsZE1lc3NhZ2VzL3VzZXIvYWNjb3VudF9sb2dpbl9yZXNwb25zZS5w",
            "cm90bxIbTWV0YVZpcnVzLk5ldC5NZXNzYWdlcy5Vc2VyIm8KEkFjY291bnRM",
            "b2dpblBiUmVzcBIPCgdyZXRDb2RlGAEgASgFEhQKB21lc3NhZ2UYAiABKAlI",
            "AIgBARIXCgpzZXNzaW9uS2V5GAMgASgJSAGIAQFCCgoIX21lc3NhZ2VCDQoL",
            "X3Nlc3Npb25LZXlCPQoiY29tLm1ldGF2aXJ1cy5uZXQubWVzc2FnZXMucGIu",
            "dXNlckIVQWNjb3VudExvZ2luUGJSZXNwT3V0UAFiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::MetaVirus.Net.Messages.User.AccountLoginPbResp), global::MetaVirus.Net.Messages.User.AccountLoginPbResp.Parser, new[]{ "RetCode", "Message", "SessionKey" }, new[]{ "Message", "SessionKey" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class AccountLoginPbResp : pb::IMessage<AccountLoginPbResp>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<AccountLoginPbResp> _parser = new pb::MessageParser<AccountLoginPbResp>(() => new AccountLoginPbResp());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<AccountLoginPbResp> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::MetaVirus.Net.Messages.User.AccountLoginResponseReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public AccountLoginPbResp() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public AccountLoginPbResp(AccountLoginPbResp other) : this() {
      retCode_ = other.retCode_;
      message_ = other.message_;
      sessionKey_ = other.sessionKey_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public AccountLoginPbResp Clone() {
      return new AccountLoginPbResp(this);
    }

    /// <summary>Field number for the "retCode" field.</summary>
    public const int RetCodeFieldNumber = 1;
    private int retCode_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int RetCode {
      get { return retCode_; }
      set {
        retCode_ = value;
      }
    }

    /// <summary>Field number for the "message" field.</summary>
    public const int MessageFieldNumber = 2;
    private string message_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Message {
      get { return message_ ?? ""; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "message" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasMessage {
      get { return message_ != null; }
    }
    /// <summary>Clears the value of the "message" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearMessage() {
      message_ = null;
    }

    /// <summary>Field number for the "sessionKey" field.</summary>
    public const int SessionKeyFieldNumber = 3;
    private string sessionKey_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string SessionKey {
      get { return sessionKey_ ?? ""; }
      set {
        sessionKey_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "sessionKey" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasSessionKey {
      get { return sessionKey_ != null; }
    }
    /// <summary>Clears the value of the "sessionKey" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearSessionKey() {
      sessionKey_ = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as AccountLoginPbResp);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(AccountLoginPbResp other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (RetCode != other.RetCode) return false;
      if (Message != other.Message) return false;
      if (SessionKey != other.SessionKey) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (RetCode != 0) hash ^= RetCode.GetHashCode();
      if (HasMessage) hash ^= Message.GetHashCode();
      if (HasSessionKey) hash ^= SessionKey.GetHashCode();
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
      if (RetCode != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(RetCode);
      }
      if (HasMessage) {
        output.WriteRawTag(18);
        output.WriteString(Message);
      }
      if (HasSessionKey) {
        output.WriteRawTag(26);
        output.WriteString(SessionKey);
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
      if (RetCode != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(RetCode);
      }
      if (HasMessage) {
        output.WriteRawTag(18);
        output.WriteString(Message);
      }
      if (HasSessionKey) {
        output.WriteRawTag(26);
        output.WriteString(SessionKey);
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
      if (RetCode != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(RetCode);
      }
      if (HasMessage) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      if (HasSessionKey) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(SessionKey);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(AccountLoginPbResp other) {
      if (other == null) {
        return;
      }
      if (other.RetCode != 0) {
        RetCode = other.RetCode;
      }
      if (other.HasMessage) {
        Message = other.Message;
      }
      if (other.HasSessionKey) {
        SessionKey = other.SessionKey;
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
            RetCode = input.ReadInt32();
            break;
          }
          case 18: {
            Message = input.ReadString();
            break;
          }
          case 26: {
            SessionKey = input.ReadString();
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
            RetCode = input.ReadInt32();
            break;
          }
          case 18: {
            Message = input.ReadString();
            break;
          }
          case 26: {
            SessionKey = input.ReadString();
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
