using Godot;
using System;
using MD;

internal class CustomClassWithManyValues
{
    [MDReplicated]
    public string StringValue;

    [MDReplicated]
    public int IntValue;

    [MDReplicated]
    public float FloatValue;

    [MDReplicated]
    public Decimal DecimalValue;

    [MDReplicated]
    public long LongValue;

    [MDReplicated]
    public double DoubleValue;

    [MDReplicated]
    public bool BoolValue;

    [MDReplicated]
    public Vector2 Vector2Value;

    [MDReplicated]
    public Vector3 Vector3Value;

    [MDReplicated]
    public Quat QuatValue;

    [MDReplicated]
    public Rect2 Rect2Value;

    [MDReplicated]
    public Transform2D Transform2DValue;

    [MDReplicated]
    public Transform TransformValue;

    [MDReplicated]
    public Plane PlaneValue;

    [MDReplicated]
    public AABB AABBValue;

    [MDReplicated]
    public Basis BasisValue;

    [MDReplicated]
    public Color ColorValue;

    [MDReplicated]
    public NodePath NodePathValue;


    public CustomClassWithManyValues()
    {
    }
}

/// <summary>
/// This test class aims to test all Godot value types along with basic C# types
/// </summary>
[MDTestClass]
[MDAutoRegister]
public class CustomClassTest03 : AutomaticTestBase
{
    [MDReplicated]
    private CustomClassWithManyValues MyCustomClass;

    private Vector2 Vector2Value = new Vector2(1,1);
    private Vector3 Vector3Value = new Vector3(1,1,1);
    private decimal DecimalValue = 1.2344874378M;
    private double DoubleValue = 2.44482823;
    private float FloatValue = 3.34213424f;
    private int IntValue = 23324;
    private long LongValue = 423434334;
    private string StringValue = "Some/node/path";

    [MDTest]
    protected void TestCreateCustomClassWithManyValueTypes()
    {
        MyCustomClass = new CustomClassWithManyValues();
    }

    protected void TestCreateCustomClassWithManyValueTypesValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
        }
    }

    [MDTest]
    protected void TestSetAABBValue()
    {
        MyCustomClass.AABBValue = new AABB(Vector3Value, Vector3Value);
    }

    protected void TestSetAABBValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.AABBValue == null)
        {
            LogError("AABBValue is null");
        }
        else if (MyCustomClass.AABBValue.Position != Vector3Value)
        {
            LogError($"AABBValue position is {MyCustomClass.AABBValue.Position}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.AABBValue.Size != Vector3Value)
        {
            LogError($"AABBValue size is {MyCustomClass.AABBValue.Size}, it should be {Vector3Value}");
        }
    }

    [MDTest]
    protected void TestSetBasisValue()
    {
        MyCustomClass.BasisValue = new Basis(Vector3Value, Vector3Value, Vector3Value);
    }

    protected void TestSetBasisValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.BasisValue == null)
        {
            LogError("Basis is null");
        }
        else if (MyCustomClass.BasisValue.x != Vector3Value)
        {
            LogError($"Basis x is {MyCustomClass.BasisValue.x}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.BasisValue.y != Vector3Value)
        {
            LogError($"Basis y is {MyCustomClass.BasisValue.y}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.BasisValue.z != Vector3Value)
        {
            LogError($"Basis y is {MyCustomClass.BasisValue.z}, it should be {Vector3Value}");
        }
    }

    [MDTest]
    protected void TestSetColorValue()
    {
        MyCustomClass.ColorValue = Colors.Red;
    }

    protected void TestSetColorValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.ColorValue == null)
        {
            LogError("Color is null");
        }
        else if (MyCustomClass.ColorValue != Colors.Red)
        {
            LogError($"Color is {MyCustomClass.ColorValue}, it should be {Colors.Red}");
        }
    }

    [MDTest]
    protected void TestSetCSharpBaseTypeValues()
    {
        // Test most C# base types
        MyCustomClass.DecimalValue = DecimalValue;
        MyCustomClass.DoubleValue = DoubleValue;
        MyCustomClass.FloatValue = FloatValue;
        MyCustomClass.IntValue = IntValue;
        MyCustomClass.LongValue = LongValue;
        MyCustomClass.StringValue = StringValue;
        MyCustomClass.BoolValue = true;
    }

    protected void TestSetCSharpBaseTypeValuesValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.DecimalValue != DecimalValue)
        {
            LogError($"Decimal is {MyCustomClass.DecimalValue}, it should be {DecimalValue}");
        }
        if (MyCustomClass.DoubleValue != DoubleValue)
        {
            LogError($"Double is {MyCustomClass.DoubleValue}, it should be {DoubleValue}");
        }
        if (MyCustomClass.FloatValue != FloatValue)
        {
            LogError($"Float is {MyCustomClass.FloatValue}, it should be {FloatValue}");
        }
        if (MyCustomClass.IntValue != IntValue)
        {
            LogError($"Float is {MyCustomClass.IntValue}, it should be {IntValue}");
        }
        if (MyCustomClass.StringValue != StringValue)
        {
            LogError($"Float is {MyCustomClass.StringValue}, it should be {StringValue}");
        }
        if (MyCustomClass.BoolValue != true)
        {
            LogError($"Float is {MyCustomClass.BoolValue}, it should be {true}");
        }
        if (MyCustomClass.LongValue != LongValue)
        {
            LogError($"Float is {MyCustomClass.LongValue}, it should be {LongValue}");
        }
    }

    [MDTest]
    protected void TestSetNodePathValue()
    {
        MyCustomClass.NodePathValue = new NodePath(StringValue);
    }

    protected void TestSetNodePathValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.NodePathValue == null)
        {
            LogError("NodePath is null");
        }
        else if (!MyCustomClass.NodePathValue.ToString().Equals(StringValue))
        {
            LogError($"NodePath is {MyCustomClass.NodePathValue.ToString()}, it should be {StringValue}");
        }
    }

    [MDTest]
    protected void TestSetPlaneValue()
    {
        MyCustomClass.PlaneValue = new Plane(Vector3Value, FloatValue);
    }

    protected void TestSetPlaneValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.PlaneValue == null)
        {
            LogError("Plane is null");
        }
        else if (MyCustomClass.PlaneValue.Normal != Vector3Value)
        {
            LogError($"Plane normal is {MyCustomClass.PlaneValue.Normal}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.PlaneValue.D != FloatValue)
        {
            LogError($"Plane D is {MyCustomClass.PlaneValue.D}, it should be {FloatValue}");
        }
    }

    [MDTest]
    protected void TestSetQuatValue()
    {
        MyCustomClass.QuatValue = new Quat(FloatValue, FloatValue, FloatValue, FloatValue);
    }

    protected void TestSetQuatValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.QuatValue == null)
        {
            LogError("Quat is null");
        }
        else if (MyCustomClass.QuatValue.x != FloatValue)
        {
            LogError($"Quat x is {MyCustomClass.QuatValue.x}, it should be {FloatValue}");
        }
        else if (MyCustomClass.QuatValue.y != FloatValue)
        {
            LogError($"Quat y is {MyCustomClass.QuatValue.x}, it should be {FloatValue}");
        }
        else if (MyCustomClass.QuatValue.z != FloatValue)
        {
            LogError($"Quat z is {MyCustomClass.QuatValue.x}, it should be {FloatValue}");
        }
        else if (MyCustomClass.QuatValue.w != FloatValue)
        {
            LogError($"Quat w is {MyCustomClass.QuatValue.x}, it should be {FloatValue}");
        }
    }

    [MDTest]
    protected void TestSetRect2Value()
    {
        MyCustomClass.Rect2Value = new Rect2(Vector2Value, Vector2Value);
    }

    protected void TestSetRect2ValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.Rect2Value == null)
        {
            LogError("Rect is null");
        }
        else if (MyCustomClass.Rect2Value.Position != Vector2Value)
        {
            LogError($"Rect position is {MyCustomClass.Rect2Value.Position}, it should be {Vector2Value}");
        }
        else if (MyCustomClass.Rect2Value.Size != Vector2Value)
        {
            LogError($"Rect size is {MyCustomClass.Rect2Value.Size}, it should be {Vector2Value}");
        }
    }

    [MDTest]
    protected void TestSetTransform2DValue()
    {
        MyCustomClass.Transform2DValue = new Transform2D(Vector2Value, Vector2Value, Vector2Value);
    }

    protected void TestSetTransform2DValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.Transform2DValue == null)
        {
            LogError("Transform2D is null");
        }
        else if (MyCustomClass.Transform2DValue.origin != Vector2Value)
        {
            LogError($"Transform2D origin is {MyCustomClass.Transform2DValue.origin}, it should be {Vector2Value}");
        }
        else if (MyCustomClass.Transform2DValue.x != Vector2Value)
        {
            LogError($"Transform2D x is {MyCustomClass.Transform2DValue.origin}, it should be {Vector2Value}");
        }
        else if (MyCustomClass.Transform2DValue.y != Vector2Value)
        {
            LogError($"Transform2D y is {MyCustomClass.Transform2DValue.origin}, it should be {Vector2Value}");
        }
    }

    [MDTest]
    protected void TestSetTransformValue()
    {
        MyCustomClass.TransformValue = new Transform(Vector3Value, Vector3Value, Vector3Value, Vector3Value);
    }

    protected void TestSetTransformValueValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.TransformValue == null)
        {
            LogError("Transform is null");
        }
        else if (MyCustomClass.TransformValue.origin != Vector3Value)
        {
            LogError($"Transform origin is {MyCustomClass.TransformValue.origin}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.TransformValue.basis.x != Vector3Value)
        {
            LogError($"Transform basis.x is {MyCustomClass.TransformValue.origin}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.TransformValue.basis.y != Vector3Value)
        {
            LogError($"Transform basis.y is {MyCustomClass.TransformValue.origin}, it should be {Vector3Value}");
        }
        else if (MyCustomClass.TransformValue.basis.z != Vector3Value)
        {
            LogError($"Transform basis.z is {MyCustomClass.TransformValue.origin}, it should be {Vector3Value}");
        }
    }

    [MDTest]
    protected void TestSetVectorValues()
    {
        MyCustomClass.Vector2Value = Vector2Value;
        MyCustomClass.Vector3Value = Vector3Value;
    }

    protected void TestSetVectorValuesValidate()
    {
        if (MyCustomClass == null)
        {
            LogError("Custom class is null");
            return;
        }
        if (MyCustomClass.Vector2Value == null)
        {
            LogError("Vector2 is null");
        }
        else if (MyCustomClass.Vector2Value != Vector2Value)
        {
            LogError($"Vector2 is {MyCustomClass.Vector2Value}, it should be {Vector2Value}");
        }
        if (MyCustomClass.Vector3Value == null)
        {
            LogError("Vector3 is null");
        }
        else if (MyCustomClass.Vector3Value != Vector3Value)
        {
            LogError($"Vector3 is {MyCustomClass.Vector3Value}, it should be {Vector3Value}");
        }
    }
}
