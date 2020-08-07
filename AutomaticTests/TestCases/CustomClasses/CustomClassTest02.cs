using Godot;
using System;
using System.Collections.Generic;
using MD;

internal class CustomClassWithList
{
    [MDReplicated]
    public MDList<CustomClassWithStrings> ListValue;

    [MDReplicated]
    public MDList<CustomClassWithList> RecursiveList {get; set;}

    public CustomClassWithList()
    {
        if (MDStatics.IsServer())
        {
            ListValue = new MDList<CustomClassWithStrings>();
            RecursiveList = new MDList<CustomClassWithList>();
        }
    }
}

/// <summary>
/// This tests the following
/// 1 - Creating a custom class with lists
/// 2 - Add a new custom class to the list
/// 3 - Remove from the list
/// 4 - Add a recursive custom class to the recrusive list
/// 5 - Add another recursive custom class to the list inside the custom class added in the last step
/// 6 - Set both lists to null
/// </summary>
[MDTestClass]
[MDAutoRegister]
public class CustomClassTest02 : AutomaticTestBase
{
    private const string STRING_VALUE01 = "This is a string";
    private const string STRING_VALUE02 = "This is a different string";

    [MDReplicated]
    private CustomClassWithList MyCustomClass;

    [MDTest]
    protected void TestCreateCustomClassWithList()
    {
        MyCustomClass = new CustomClassWithList();
    }

    protected void TestCreateCustomClassWithListValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
        }
        else if (MyCustomClass.ListValue == null)
        {
            LogError($"List is null");
        }
        else if (MyCustomClass.RecursiveList == null)
        {
            LogError($"List is null");
        }
    }

    [MDTest]
    protected void TestCreateCustomClassInsideCustomClass()
    {
        CustomClassWithStrings CustomClass = new CustomClassWithStrings(STRING_VALUE01, STRING_VALUE02);
        MyCustomClass.ListValue.Add(CustomClass);
    }

    protected void TestCreateCustomClassInsideCustomClassValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
        }
        else if (MyCustomClass.ListValue == null)
        {
            LogError($"List is null");
        }
        else if (MyCustomClass.ListValue.Count == 0)
        {
            LogError($"List has no entries");
        }
        else if (MyCustomClass.ListValue[0].StringValue != STRING_VALUE01)
        {
            LogError($"String value should be '{STRING_VALUE01}' but is '{MyCustomClass.ListValue[0].StringValue}'");
        }
        else if (MyCustomClass.ListValue[0].StringProperty != STRING_VALUE02)
        {
            LogError($"String property should be '{STRING_VALUE02}' but is '{MyCustomClass.ListValue[0].StringProperty}'");
        }
    }

    [MDTest]
    protected void TestRemoveFromListInsideCustomClass()
    {
        MyCustomClass.ListValue.RemoveAt(0);
    }

    protected void TestRemoveFromListInsideCustomClassValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
        }
        else if (MyCustomClass.ListValue == null)
        {
            LogError($"List is null");
        }
        else if (MyCustomClass.ListValue.Count != 0)
        {
            LogError($"List has entries");
        }
    }

    [MDTest]
    protected void TestAddRecursiveClassInsideCustomClass()
    {
        CustomClassWithList CustomClass = new CustomClassWithList();
        MyCustomClass.RecursiveList.Add(CustomClass);
    }

    protected void TestAddRecursiveClassInsideCustomClassValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
        }
        else if (MyCustomClass.RecursiveList == null)
        {
            LogError($"List is null");
        }
        else if (MyCustomClass.RecursiveList.Count == 0)
        {
            LogError($"List has no entries");
        }
        else if (MyCustomClass.RecursiveList[0] == null)
        {
            LogError($"List entry is null");
        }
    }

    [MDTest]
    protected void TestAddRecursiveClassInsideCustomClass2()
    {
        CustomClassWithList CustomClass = new CustomClassWithList();
        MyCustomClass.RecursiveList[0].RecursiveList.Add(CustomClass);
    }

    protected void TestAddRecursiveClassInsideCustomClass2Validate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
        }
        else if (MyCustomClass.RecursiveList == null)
        {
            LogError($"List is null");
        }
        else if (MyCustomClass.RecursiveList.Count == 0)
        {
            LogError($"List has no entries");
        }
        else if (MyCustomClass.RecursiveList[0] == null)
        {
            LogError($"List entry is null");
        }
        else if (MyCustomClass.RecursiveList[0].RecursiveList == null)
        {
            LogError($"Inner list is null");
        }
        else if (MyCustomClass.RecursiveList[0].RecursiveList.Count == 0)
        {
            LogError($"Inner list has no entries");
        }
        else if (MyCustomClass.RecursiveList[0].RecursiveList[0] == null)
        {
            LogError($"Inner list entry is null");
        }
    }

    [MDTest]
    protected void TestSetListsAndRecursiveClassToNull()
    {
        MyCustomClass.ListValue = null;
        MyCustomClass.RecursiveList = null;
    }

    protected void TestSetListsAndRecursiveClassToNullValidate()
    {
        if (MyCustomClass.ListValue != null)
        {
            LogError("Value list is not null");
        }
        if (MyCustomClass.RecursiveList != null)
        {
            LogError("Recursive list is not null");
        }
    }
}
