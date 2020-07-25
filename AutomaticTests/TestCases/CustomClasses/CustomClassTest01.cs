using Godot;
using System;
using System.Collections.Generic;
using MD;

internal class CustomClassWithStrings
{
	[MDReplicated]
	public string StringValue;

	[MDReplicated]
	public string StringProperty { get; set; }

    public CustomClassWithStrings()
	{
	}
	
	public CustomClassWithStrings(string Value, string PropValue)
	{
		this.StringValue = Value;
		this.StringProperty = PropValue;
	}
}

/// <summary>
/// This tests the following
/// 1 - Creating a custom class
/// 2 - Changing a value and property within the custom class
/// 3 - Setting a value and property to null
/// 4 - Setting the custom class itself to null
/// </summary>
[MDAutoRegister]
public class CustomClassTest01 : AutomaticTestBase
{
	private const string STRING_VALUE01 = "This is a string";
	private const string STRING_VALUE02 = "This is a different string";

	[MDReplicated]
	private CustomClassWithStrings MyCustomClass;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	protected void Test1()
	{
		MyCustomClass = new CustomClassWithStrings(STRING_VALUE01, STRING_VALUE02);
	}

	protected void ValidateTest1()
	{
		if (MyCustomClass == null)
		{
			LogError("Custom class is null");
		}
		else if (MyCustomClass.StringValue != STRING_VALUE01)
		{
			LogError($"String value should be '{STRING_VALUE01}' but is '{MyCustomClass.StringValue}'");
		}
		else if (MyCustomClass.StringProperty != STRING_VALUE02)
		{
			LogError($"String property should be '{STRING_VALUE02}' but is '{MyCustomClass.StringProperty}'");
		}
		TestFinished();
	}

	protected void Test2()
	{
		MyCustomClass.StringProperty = STRING_VALUE01;
		MyCustomClass.StringValue = STRING_VALUE02;
	}

	protected void ValidateTest2()
	{
		if (MyCustomClass == null)
		{
			LogError("Custom class is null");
		}
		else if (MyCustomClass.StringValue != STRING_VALUE02)
		{
			LogError($"String value should be '{STRING_VALUE02}' but is '{MyCustomClass.StringValue}'");
		}
		else if (MyCustomClass.StringProperty != STRING_VALUE01)
		{
			LogError($"String property should be '{STRING_VALUE01}' but is '{MyCustomClass.StringProperty}'");
		}
		TestFinished();
	}

	protected void Test3()
	{
		MyCustomClass.StringProperty = null;
		MyCustomClass.StringValue = null;
	}

	protected void ValidateTest3()
	{
		if (MyCustomClass == null)
		{
			LogError("Custom class is null");
		}
		else if (MyCustomClass.StringValue != null)
		{
			LogError($"String value should be null but is '{MyCustomClass.StringValue}'");
		}
		else if (MyCustomClass.StringProperty != null)
		{
			LogError($"String property should be null but is '{MyCustomClass.StringProperty}'");
		}
		TestFinished();
	}

	protected void Test4()
	{
		MyCustomClass = null;
	}

	protected void ValidateTest4()
	{
		if (MyCustomClass != null)
		{
			LogError("Custom class is not null");
		}
		TestFinished();
	}
}
