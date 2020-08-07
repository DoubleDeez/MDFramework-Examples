using Godot;
using System;
using System.Collections.Generic;
using MD;

/// <summary>
/// This tests the following
/// 1 - Add a string to a MDList
/// 2 - Remove the string from the MDList
/// 3 - Add a lot of strings to an MDList
/// 4 - Sort an MDList
/// TODO: Test more MDList features
/// </summary>
[MDTestClass]
[MDAutoRegister]
public class MDListTests01 : AutomaticTestBase
{
    private const string TEST_STRING = "TestString";
    private const int TEST03_STRING_COUNT = 20;

    [MDReplicated]
    MDList<string> StringList = null;
    
    [MDTest]
    protected void AddStringToMDList()
    {
        StringList.Add(TEST_STRING);
    }

    protected void AddStringToMDListValidate()
    {
        if (StringList.Count != 1)
        {
            LogError("List contains no items");
        }
        else if (StringList[0] != TEST_STRING)
        {
            LogError($"List text is not correct, it is {StringList[0]}");
        }
    }

    [MDTest]
    protected void TestRemoveStringFromMDList()
    {
        StringList.Remove(TEST_STRING);
    }

    protected void TestRemoveStringFromMDListValidate()
    {
        if (StringList.Count != 0)
        {
            LogError("List still contain items");
        }
    }

    [MDTest]
    protected void TestAddMultipleStringsToMDList()
    {
        for (int i = TEST03_STRING_COUNT; i > 0; i--)
        {
            StringList.Add($"{TEST_STRING}{i}");
        }
    }

    protected void TestAddMultipleStringsToMDListValidate()
    {
        // Wait for all strings to arrive
        if (StringList.Count < TEST03_STRING_COUNT)
        {
            AddError($"List has  '{StringList.Count} items, it is supposed to have {TEST03_STRING_COUNT}");
        }
        else
        {
            for (int i = 0; i < TEST03_STRING_COUNT; i++)
            {
                if (StringList[i] != $"{TEST_STRING}{TEST03_STRING_COUNT-i}")
                {
                    AddError($"List string wrong, expected '{TEST_STRING}{TEST03_STRING_COUNT-i}' instead we got '{StringList[i]}'");
                }
            }
        }
    }
}
