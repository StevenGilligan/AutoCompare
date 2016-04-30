[![Build status](https://ci.appveyor.com/api/projects/status/ay77ykijskpiuiy8?svg=true)](https://ci.appveyor.com/project/StevenGilligan/autocompare) [![codecov.io](http://codecov.io/github/StevenGilligan/AutoCompare/coverage.svg?branch=dev)](http://codecov.io/github/StevenGilligan/AutoCompare?branch=dev)

AutoCompare
===========

What is AutoCompare?
--------------------

AutoCompare is a simple library with the goal of making it effortless to compare two objects of the same type to generate the list of modified properties.

Why use AutoCompare?
--------------------

The main goal of AutoCompare is making it easy to get differences between two objects of the same type. AutoCompare builds an Expression Tree the first time you compare a type so the first call is always slower. Once the Expression Tree is built and compiled, comparing two objects becomes extremely fast. 

How to use AutoCompare in your project
--------------------------------------

Include [AutoCompare](https://www.nuget.org/packages/AutoCompare/) in your project using [NuGet](https://www.nuget.org/)

From the package manager console : 

    PM> Install-Package AutoCompare

Features
--------

* Works on any object type, and does deep compare (compares child objects)
* Reflection is only used the first time a type is compared, successive calls are executing a compiled lambda and are extremely fast
* Detects circular references so it won't throw a StackOverflowException if you have `Parent.Child.Parent`
* Strongly typed fluent configuration using lambdas
* Supports `IEnumerable<>` and `IDictionary<,>` properties, although some configuration might be necessary
* Supports DI/IoC frameworks with the `IComparerEngine` interface, initialize using a `new Engine()`
* Supports having multiple configuration for the same type, using separate `IComparerEngine` instances

Code examples
-------------

Compare two objects
```c#
var differences = AutoCompare.Comparer.Compare<MyObjectType>(objA, objB);
```

Configure properties that shouldn't be compared
```c#
AutoCompare.Comparer.Configure<MyObjectType>()
    .For(x => x.IgnoredProperty, x => x.Ignore()) 
    .Ignore(x => x.AnotherIgnoredProperty); // Alternative way to ignore a property
```

Configure a IEnumerable (array, list, hashmap, etc.) property to perform a deep compare. To deeply compare lists, you must specify the property to be used as a key or ID. 
```c#
AutoCompare.Comparer.Configure<MyObjectType>()
    .For(x => x.ListProperty, x => x.MatchUsing(y => y.ID));
```

Precompile a type to make sure the first comparison is not slowed down by the compilation process
```c#
AutoCompare.Comparer.Configure<MyObjectType>()
	.Compile.Now(); // .Compile.Async() also available
```

Calling `Configure<Type>()` is optional and AutoCompare will default to comparing every public property. 

Please note that you must call `Configure<Type>()` only once per type, and call it before any call to `Compare<Type>()` is made or it will throw an exception.

More examples can be found in AutoCompare.Tests

Licence
-------

AutoCompare is Copyright &copy; 2015 [Steven Gilligan](http://steven.gilligan.io) and other contributors under the [Apache 2.0 license](LICENSE.txt).
