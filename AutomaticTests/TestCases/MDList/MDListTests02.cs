using Godot;
using System;
using System.Collections.Generic;
using MD;

[MDAutoRegister]
public class MDListTests02 : AutomaticTestBase
{
	private const string TEST_STRING01 = "TestString01";
    private const string TEST_STRING02 = "TestString02";

	[MDReplicated]
	[MDReplicatedSetting(MDReplicatedMember.Settings.OnValueChangedEvent, nameof(DoValidation))]
	MDList<CustomClassWithStrings> StringList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
    
	protected void Test1()
	{
		StringList.Add(new CustomClassWithStrings(TEST_STRING01, TEST_STRING01));
	}

	protected void ValidateTest1()
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
		TestFinished();
	}

	protected void Test2()
	{
		StringList[0].StringValue = TEST_STRING02;
	}

	protected void ValidateTest2()
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
		TestFinished();
	}
}
