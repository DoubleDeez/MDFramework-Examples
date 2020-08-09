using Godot;
using System;
using MD;

/// <summary>
/// Simple test for Rpc
/// Tests sending a String & Custom class and sending null for both as well
/// </summary>
[MDTestClass]
public class MDRpcTest01 : AutomaticTestBase
{
    private const string TEST_STRING = "TestString";

    CustomClassWithStrings MyCustomClass = null;

    String MyString = null;

    private bool CustomClassMethodCalled = false;

    private bool BasicMethodCalled = false;

    private bool InvokedOnServer = false;

    protected void CheckIfCustomMethodWasCalled()
    {
        if (!CustomClassMethodCalled)
        {
            LogError("Custom method was not called");
        }
        CustomClassMethodCalled = false;
    }

    protected void CheckIfBasicMethodWasCalled()
    {
        if (!BasicMethodCalled)
        {
            LogError("Basic method was not called");
        }
        BasicMethodCalled = false;
    }

    protected void MDRpcInvokeOnServer()
    {
        InvokedOnServer = true;
    }

    [Remote]
    private void MDRpcCustomClass(CustomClassWithStrings CustomClass)
    {
        this.MyCustomClass = CustomClass;
        CustomClassMethodCalled = true;
    }

    [Remote]
    private void MDRpcBasicString(String StringValue)
    {
        this.MyString = StringValue;
        BasicMethodCalled = true;
    }

    [MDTest]
    protected void TestSendStringOverMDRpc()
    {
        MyString = TEST_STRING;
        this.MDRpc(nameof(MDRpcBasicString), MyString);

        // Extra test for server
        this.MDRpcId(MDStatics.GetPeerId(), nameof(MDRpcInvokeOnServer));
        if (!InvokedOnServer)
        {
            AddError("Failed to invoke with MDRpcId on local client");
        }
    }

    protected void TestSendStringOverMDRpcValidate()
    {
        CheckIfBasicMethodWasCalled();
        if (MyString == null)
        {
            LogError("String is not set");
        }
        else if (MyString != TEST_STRING)
        {
            LogError($"String value is not correct, it is '{MyString}' while it should be '{TEST_STRING}'");
        }
    }

    [MDTest]
    protected void TestSendNullOverMDRpc()
    {
        this.MDRpc(nameof(MDRpcBasicString), null);
    }

    protected void TestSendNullOverMDRpcValidate()
    {
        CheckIfBasicMethodWasCalled();
        if (MyString != null)
        {
            LogError("String is not null");
        }
    }

    [MDTest]
    protected void TestSendCustomClassOverMDRpc()
    {
        MyCustomClass = new CustomClassWithStrings(TEST_STRING, TEST_STRING);
        this.MDRpc(nameof(MDRpcCustomClass), MyCustomClass);
    }

    protected void TestSendCustomClassOverMDRpcValidate()
    {
        CheckIfCustomMethodWasCalled();
        if (MyCustomClass == null)
        {
            LogError("Custom class is not set");
        }
        else if (MyCustomClass.StringValue != TEST_STRING)
        {
            LogError($"Custom class string value is not correct, it is '{MyCustomClass.StringValue}' while it should be '{TEST_STRING}'");
        }
        else if (MyCustomClass.StringProperty != TEST_STRING)
        {
            LogError($"Custom class string property is not correct, it is '{MyCustomClass.StringProperty}' while it should be '{TEST_STRING}'");
        }
    }

    [MDTest]
    protected void TestSendNullToCustomClassMethodOverMDRpc()
    {
        this.MDRpc(nameof(MDRpcCustomClass), null);
    }

    protected void TestSendNullToCustomClassMethodOverMDRpcValidate()
    {
        CheckIfCustomMethodWasCalled();
        if (MyCustomClass != null)
        {
            LogError("Custom class is not null");
        }
    }
}
