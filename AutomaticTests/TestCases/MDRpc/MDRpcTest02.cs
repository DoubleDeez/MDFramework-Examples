using Godot;
using MD;

[MDAutoRegister]
public class MDRpcTest02Base : AutomaticTestBase
{
    public int PublicRPCValue;
    public int ProtectedVirtualRPCValue;
    public int PrivateRPCValue;

    [Remote]
    public void PublicRPC(int Value)
    {
        PublicRPCValue = Value;
    }

    [Remote]
    protected virtual void ProtectedVirtualRPC(int Value)
    {
        ProtectedVirtualRPCValue = Value;
    }

    [Remote]
    private void PrivateRPC(int Value)
    {
        PrivateRPCValue = Value;
    }
}

/// <summary>
/// Tests calling RPC methods that are on the base class
/// </summary>
[MDTestClass]
[MDAutoRegister]
public class MDRpcTest02 : MDRpcTest02Base
{
    [MDTest]
    protected void TestCallPublicInheritedRpcMethod()
    {
        this.MDRpc(nameof(PublicRPC), 1);
    }

    protected void TestCallPublicInheritedRpcMethodValidate()
    {
        if (PublicRPCValue != 1)
        {
            LogError($"PublicRPCValue is {PublicRPCValue} but expected 1");
        }
    }

    [MDTest]
    protected void TestCallProtectedVirtualInheritedRpcMethod()
    {
        this.MDRpc(nameof(ProtectedVirtualRPC), 2);
    }

    protected void TestCallProtectedVirtualInheritedRpcMethodValidate()
    {
        if (ProtectedVirtualRPCValue != 2)
        {
            LogError($"ProtectedVirtualRPCValue is {ProtectedVirtualRPCValue} but expected 2");
        }
    }

    [MDTest]
    protected void TestCallPrivateMDRpcMethod()
    {
        this.MDRpc("PrivateRPC", 3);
    }

    protected void TestCallPrivateMDRpcMethodValidate()
    {
        if (PrivateRPCValue != 3)
        {
            LogError($"PrivateRPCValue is {PrivateRPCValue} but expected 3");
        }
    }
}