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
[MDAutoRegister]
public class MDRpcTest02 : MDRpcTest02Base
{
    protected void Test1()
    {
        this.MDRpc(nameof(PublicRPC), 1);
    }

    protected void ValidateTest1()
    {
        if (PublicRPCValue != 1)
        {
            LogError($"PublicRPCValue is {PublicRPCValue} but expected 1");
        }
    }
    
    protected void Test2()
    {
        this.MDRpc(nameof(ProtectedVirtualRPC), 2);
    }

    protected void ValidateTest2()
    {
        if (ProtectedVirtualRPCValue != 2)
        {
            LogError($"ProtectedVirtualRPCValue is {ProtectedVirtualRPCValue} but expected 2");
        }
    }
    
    protected void Test3()
    {
        this.MDRpc("PrivateRPC", 3);
    }

    protected void ValidateTest3()
    {
        if (PrivateRPCValue != 3)
        {
            LogError($"PrivateRPCValue is {PrivateRPCValue} but expected 3");
        }
    }
}