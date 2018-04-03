# Code Style

The following is a general guide of how to structure your code and some of the guidelines we have in place when contributing to Quaver. 

**NOTE: Some portions of the codebase may not follow this exactly, but as we rewrite and work on things, we hope to follow these guidelines throughout the entire codebase.**

This code style guide was largely taken from the official [MonoGame](https://github.com/MonoGame/MonoGame) repository, as we believe their code style guide is great.

### Useful Links ###
* [C# Coding Conventions (MSDN)](http://msdn.microsoft.com/en-us/library/ff926074.aspx)

# Guidelines

### Tabs & Indenting ###

Tab characters should **never** be used. All indentation should be done with 4 space characters.

#### Braces ####

Open Braces should **always** be on a new line. Furthermore, **one-liners** do not need to have braces.

**Example:**
```cs
if (someExpression)
{
   DoSomething();
   DoAnotherThing();
}
else
   DoSomethingElse();
```

#### Switches ####

`case` statements should be indented from the switch statement.

**Example:**
```cs
switch (someExpression) 
{
   case 0:
      DoSomething();
      break;

   case 1:
      DoSomethingElse();
      break;
}
```

#### Single Statements ####

Braces are not used for single statement blocks immediately following a `for`, `foreach`, `if`, `do`, etc. The single statement block should always be on the following line and indented by four spaces. This increases code readability and maintainability.

**Example:**
```cs
for (int i = 0; i < 100; ++i)
    DoSomething(i);
```


#### Single line Property Statements ####

Single line property statements can have braces that begin and end on the same line. This should only be used for simple property statements. Add a single space before and after the braces.  

**Example:**
```cs
internal class Foo
{
   internal int Bar { get; set; } = 10;
}
```

#### Multi-Line Property Statements ####

Multi-line property statements must have braces on new lines.

**Example:**
```cs
internal class Foo
{
   internal int Bar
   {
      get => Bar * 2
      set { bar = value; }
   }
}
```

### Commenting ###

Comments should be used to describe intention, algorithmic overview, and/or logical flow.  It would be ideal, if from reading the comments alone,someone other than the author could understand a functions intended behavior and general operation. While there are no minimum comment requirements and certainly some very small routines need no commenting at all, it is hoped that most routines will have comments reflecting the programmers intent and approach.

Comments must provide added value or explanation to the code. Simply describing the code is not helpful or useful.

**Example:**
```cs
    // Wrong
    // Set count to 1
    count = 1;

    // Right
    // Set the initial reference count so it isn't cleaned up next frame
    count = 1;
```

### XML Documentation ###

We **REQUIRE** All methods, properties, and fields to have property XML documentation with a summary of what they do. This is so that we can all be on the same page and navigate the code easier.

**Example:**
```cs
/// <summary>
/// Generates A random float between 2 numbers.
/// </summary>
/// <param name="min"></param>
/// <param name="max"></param>
/// <returns></returns>
internal static float Random(float min, float max)
{
    var random = new Random();

    // If min > max for some reason
    if (min > max)
    {
        var temp = min;
        max = min;
        min = temp;
    }

    //Generate the random number
    var randNum = random.Next(0, 1000) / 1000f;

    //Return the random number in the given range
    return (randNum * (max - min)) + min;
}
```

### Spacing ###

Spaces improve readability by decreasing code density. Here are some guidelines for the use of space characters within code:

* Do use a single space after a comma between function arguments.
```cs
Console.In.Read(myChar, 0, 1);  // Right
Console.In.Read(myChar,0,1);    // Wrong
```
* Do not use a space after the parenthesis and function arguments
```cs
CreateFoo(myChar, 0, 1)         // Right
CreateFoo( myChar, 0, 1 )       // Wrong
```
* Do not use spaces between a function name and parenthesis.
```cs
CreateFoo()                     // Right
CreateFoo ()                    // Wrong
```
* Do not use spaces inside brackets.
```cs
x = dataArray[index];           // Right
x = dataArray[ index ];         // Wrong
```
* Do use a single space before flow control statements
```cs
while (x == y)                  // Right
while(x==y)                     // Wrong
```
* Do use a single space before and after binary operators
```cs
if (x == y)                     // Right
if (x==y)                       // Wrong
```
* Do not use a space between a unary operator and the operand
```cs
++i;                            // Right
++ i;                           // Wrong
```
* Do not use a space before a semi-colon. Do use a space after a semi-colon if there is more on the same line
```cs
for (int i = 0; i < 100; ++i)   // Right
for (int i=0 ; i<100 ; ++i)     // Wrong
```

### Naming ###

Follow all .NET Framework Design Guidelines for both internal and external members. Highlights of these include:
* Do not use Hungarian notation
* Do use an underscore prefix for **private field member variables if they have an associated property** , e.g. `_foo`
* Do use camelCasing for member variables (first word all lowercase, subsequent words initial uppercase)
* Do use camelCasing for parameters
* Do use camelCasing for local variables
* Do use PascalCasing for function, property, event, and class names (all words initial uppercase)
* Do prefix interfaces names with **I**
* Do not prefix enums, classes, or delegates with any letter