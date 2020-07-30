BetterDP
========

[![Build-AppVeyor](https://ci.appveyor.com/api/projects/status/github/DigiDNA/BetterDP?svg=true)](https://ci.appveyor.com/project/DigiDNA/BetterDP)
[![Issues](http://img.shields.io/github/issues/DigiDNA/BetterDP.svg?style=flat)](https://github.com/DigiDNA/BetterDP/issues)
![Status](https://img.shields.io/badge/status-active-brightgreen.svg?style=flat)
![License](https://img.shields.io/badge/license-mit-brightgreen.svg?style=flat)
[![Contact](https://img.shields.io/badge/contact-@DigiDNA-blue.svg?style=flat)](https://twitter.com/DigiDNA)  

Simpler declaration for WPF Dependency Properties
-------------------------------------------------

About
-----

[Dependency Properties](https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/dependency-properties-overview) are a neat feature of WPF, allowing powerful data binding mechanisms.

However, declaring custom properties is not really straightforward and leads to a lot of boilerplate code.  
As an example:

```cs
using System.Windows;

public class Foo: DependencyObject
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register
    (
        "Bar",
        typeof( string ),
        typeof( Foo ),
        new PropertyMetadata()
    );

    public string Bar
    {
        get => this.GetValue( BarProperty ) as string;
        set => this.SetValue( BarProperty, value );
    }
}
```

This package provides a way to reduce the amount of code needed to declare such a property, by introducing a custom `[DP]` attribute.  
The new syntax is then:

```cs
using System.Windows;
using BetterDP;

public class Foo: DependencyObject
{
    static Foo()
    {
        DP.InitializeProperties( typeof( Foo ) );
    }

    [DP]
    public string Bar
    {
        get => this.Get< string >();
        set => this.Set( value );
    }
}
```

### Initialization

**BetterDP** will automatically create dependency properties for every property marked with the `DP` attribute.  
However, this unfortunately cannot be done lazily, as dependency properties usually need to be available before constructing an object, especially when using XAML bindings.

This is why the example above uses a static constructor.  
Calling `DP.InitializeProperties` will ensure every dependency property is properly initialized.

### Default values

A default value can be provided directly within the `[DP]` attribute, such as:

```cs
[DP( DefaultValue = "hello, world" )]
public string Bar
{
    get => this.Get< string >();
    set => this.Set( value );
}
```

This is the equivalent of setting a default value with `PropertyMetadata` or `FrameworkPropertyMetadata`.

### Change Handlers

In order to be notified when a property has changed, you may implement the following method in your class:

```cs
protected virtual void DependencyPropertyDidChange( string name, object value )
{}
```

It will automatically be called when a property marked with `[DP]` has changed.

License
-------

**BetterDP** is released under the terms of the MIT License.

Repository Infos
----------------

    Owner:          DigiDNA
    Web:            www.digidna.net
    Blog:           imazing.com/blog
    Twitter:        @DigiDNA
    GitHub:         github.com/DigiDNA
