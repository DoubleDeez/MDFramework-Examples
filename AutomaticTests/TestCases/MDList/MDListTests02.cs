using Godot;
using System;
using System.Collections.Generic;
using MD;

/// <summary>
/// This tests the following
/// 1 - Create a custom class with strings inside a list
/// 2 - Modify the strings
/// </summary>
[MDTestClass]
[MDAutoRegister]
public class MDListTests02 : AutomaticTestBase
{
    private const string TEST_STRING01 = "TestString01";
    private const string TEST_STRING02 = "TestString02";

    [MDReplicated]
    MDList<CustomClassWithStrings> StringList = null;

    [MDTest]
    protected void TestAddCustomClassToMDList()
    {
        StringList.Add(new CustomClassWithStrings(TEST_STRING01, TEST_STRING01));
    }

    protected void TestAddCustomClassToMDListValidate()
    {
        if (StringList.Count != 1)
        {
            LogError("List contains no items");
        }
        else if (StringList[0] == null)
        {
            LogError($"List item is null");
        }
        else if (StringList[0].StringValue != TEST_STRING01)
        {
            LogError($"List value is not correct, it is {StringList[0].StringValue}");
        }
        else if (StringList[0].StringProperty != TEST_STRING01)
        {
            LogError($"List property is not correct, it is {StringList[0].StringProperty}");
        }
    }

    [MDTest]
    protected void TestSetValuesOnCustomClassInsideMDList()
    {
        StringList[0].StringValue = TEST_STRING02;
        StringList[0].StringProperty = TEST_STRING02;
    }

    protected void TestSetValuesOnCustomClassInsideMDListValidate()
    {
        if (StringList.Count != 1)
        {
            LogError("List contains no items");
        }
        else if (StringList[0] == null)
        {
            LogError($"List item is null");
        }
        else if (StringList[0].StringValue != TEST_STRING02)
        {
            LogError($"List value is not correct, it is {StringList[0].StringValue}");
        }
        else if (StringList[0].StringProperty != TEST_STRING02)
        {
            LogError($"List property is not correct, it is {StringList[0].StringProperty}");
        }
    }
}
