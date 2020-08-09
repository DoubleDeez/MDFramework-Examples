using Godot;
using System;
using MD;

/// <summary>
/// Simple test for Rset
/// Tests sending a String & Custom class and sending null for both as well
/// </summary>
[MDTestClass]
public class MDRsetTest01 : AutomaticTestBase
{
    private const string TEST_STRING = "TestString";

    [Remote]
    String MyString = "";

    [Remote]
    CustomClassWithStrings MyCustomClass = null;

    [Remote]
    MDList<CustomClassWithList> MyMDList = null;

    [MDTest]
    protected void TestMDRsetWithCustomClass()
    {
        MyCustomClass = new CustomClassWithStrings(TEST_STRING, TEST_STRING);
        this.MDRset(nameof(MyCustomClass), MyCustomClass);
    }

    protected void TestMDRsetWithCustomClassValidate()
    {
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
    protected void TestMDRsetCustomClassWithNull()
    {
        this.MDRset(nameof(MyCustomClass), null);
        this.MDRset(nameof(MyString), TEST_STRING);
    }

    protected void TestMDRsetCustomClassWithNullValidate()
    {
        if (MyCustomClass != null)
        {
            LogError("Custom class is not null");
        }
        if (MyString != TEST_STRING)
        {
            LogError($"String is {MyString} it should be {TEST_STRING}");
        }
    }

    [MDTest]
    protected void TestMDRsetCustomClassContainingOtherCustomClasses()
    {
        MDList<CustomClassWithList> TmpList = new MDList<CustomClassWithList>();
        CustomClassWithList item = new CustomClassWithList();
        CustomClassWithList item2 = new CustomClassWithList();
        item.RecursiveList.Add(item2);
        TmpList.Add(item);
        TmpList.Add(new CustomClassWithList());
        this.MDRset(nameof(MyMDList), TmpList);
    }

    protected void TestMDRsetCustomClassContainingOtherCustomClassesValidate()
    {
        if (MyMDList == null)
        {
            LogError("List is null");
            return;
        }
        else if (MyMDList.Count != 2)
        {
            LogError($"List should have 2 items but has {MyMDList.Count}");
            return;
        }
        if (MyMDList[0] == null)
        {
            LogError($"First item in list is null");
        }
        else if (MyMDList[0].RecursiveList == null)
        {
            LogError($"Recursive list is null");
        }
        else if (MyMDList[0].RecursiveList.Count != 1)
        {
            LogError($"Recursive list is should have one value, it has {MyMDList[0].RecursiveList.Count}");
        }
        else if (MyMDList[0].RecursiveList[0] == null)
        {
            LogError($"First item in recurisve list is not initialized");
        }
        if (MyMDList[1] == null)
        {
            LogError($"Second item in list is null");
        }
    }

    [MDTest]
    protected void TestMDRsetMDListAsNull()
    {
        this.MDRset(nameof(MyMDList), null);
    }

    protected void TestMDRsetMDListAsNullValidate()
    {
        if (MyMDList != null)
        {
            LogError("List is not null");
        }
    }
}
