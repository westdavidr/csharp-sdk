using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MultiMcpServer.Tools;

/// <summary>
/// Tools for the math server instance
/// </summary>
[McpServerToolType]
public class MathTools
{
    [McpServerTool(Name = "add"), Description("Add two numbers")]
    public static double Add(double a, double b)
    {
        return a + b;
    }

    [McpServerTool(Name = "subtract"), Description("Subtract two numbers")]
    public static double Subtract(double a, double b)
    {
        return a - b;
    }

    [McpServerTool(Name = "multiply"), Description("Multiply two numbers")]
    public static double Multiply(double a, double b)
    {
        return a * b;
    }

    [McpServerTool(Name = "divide"), Description("Divide two numbers")]
    public static double Divide(double a, double b)
    {
        if (b == 0)
            throw new InvalidOperationException("Cannot divide by zero");
        return a / b;
    }

    [McpServerTool(Name = "power"), Description("Calculate a number raised to a power")]
    public static double Power(double baseNumber, double exponent)
    {
        return Math.Pow(baseNumber, exponent);
    }

    [McpServerTool(Name = "sqrt"), Description("Calculate square root of a number")]
    public static double SquareRoot(double number)
    {
        if (number < 0)
            throw new InvalidOperationException("Cannot calculate square root of negative number");
        return Math.Sqrt(number);
    }
}