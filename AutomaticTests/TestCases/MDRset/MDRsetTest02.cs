using Godot;
using MD;

[MDAutoRegister]
public class MDRsetTest02Base : AutomaticTestBase
{
    [Remote]
    public int PublicIntField;

    [Remote]
    protected int ProtectedIntProperty { get; set; }

    [Remote]
    private int PrivateIntField;

    public void SetPrivateIntField(int Value)
    {
        PrivateIntField = 3;
    }

    public int GetPrivateIntField()
    {
        return PrivateIntField;
    }
}

/// <summary>
/// Tests calling RPC methods that are on the base class
/// </summary>
[MDTestClass]
[MDAutoRegister]
public class MDRsetTest02 : MDRsetTest02Base
{
    [MDTest]
    protected void TestMDRsetPublicIntField()
    {
        this.MDRset(nameof(PublicIntField), 1);
    }

    protected void TestMDRsetPublicIntFieldValidate()
    {
        if (PublicIntField != 1)
        {
            LogError($"PublicIntField is {PublicIntField} but expected 1");
        }
    }

    [MDTest]
    protected void TestMDRsetProtectedIntProperty()
    {
        this.MDRset(nameof(ProtectedIntProperty), 2);
    }

    protected void TestMDRsetProtectedIntPropertyValidate()
    {
        if (ProtectedIntProperty != 2)
        {
            LogError($"ProtectedIntProperty is {ProtectedIntProperty} but expected 2");
        }
    }

    [MDTest]
    protected void TestMDRsetPrivateIntField()
    {
        this.MDRset("PrivateIntField", 3);
    }

    protected void TestMDRsetPrivateIntFieldValidate()
    {
        if (GetPrivateIntField() != 3)
        {
            LogError($"PrivateIntField is {GetPrivateIntField()} but expected 3");
        }
    }
}