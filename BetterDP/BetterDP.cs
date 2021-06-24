
/*******************************************************************************
 * The MIT License (MIT)
 * 
 * Copyright (c) 2020 DigiDNA - www.digidna.net
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ******************************************************************************/

/*!
 * @file        BetterDP.cs
 * @copyright   (c) 2020, DigiDNA - www.digidna.net
 * @author      Jean-David Gadina - www.digidna.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace BetterDP
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DP: Attribute
    {
        public object DefaultValue
        {
            get;
            set;
        }

        public static void InitializeProperties( Type type )
        {
            DependencyObjectExtensions.InitializeProperties( type );
        }
    }

    public static partial class DependencyObjectExtensions
    {
        private class PropertyInfo
        {
            public DependencyProperty Property
            {
                get;
                set;
            }

            public PropertyInfo( Type ownerType, string name, Type type, object defaultValue = null )
            {
                this.Property = DependencyProperty.Register
                (
                    name,
                    type,
                    ownerType,
                    new FrameworkPropertyMetadata
                    (
                        ( type.IsValueType && defaultValue == null ) ? Activator.CreateInstance( type ) : defaultValue,
                        ( o, e ) =>
                        {
                            Type       t;
                            MethodInfo i;

                            t = o.GetType();
                            i = t.GetMethod( "DidChangeDependencyProperty", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );

                            i?.Invoke( o, new object[] { name, e.NewValue } );
                        }
                    )
                );
            }
        }

        private static readonly object                                                   Lock   = new object();
        private static readonly List< string >                                           Inited = new List< string >();
        private static readonly Dictionary< string, Dictionary< string, PropertyInfo > > Props  = new Dictionary< string, Dictionary< string, PropertyInfo > >();

        private static PropertyInfo PropertyForOwnerType< T >( string name, Type ownerType )
        {
            return PropertyForOwnerType( name, typeof( T ), ownerType );
        }


        private static PropertyInfo PropertyForOwnerType( string name, Type type, Type ownerType, object defaultValue = null )
        {
            lock( Lock )
            {
                if( Props.ContainsKey( ownerType.FullName ) && Props[ ownerType.FullName ].ContainsKey( name ) )
                {
                    return Props[ ownerType.FullName ][ name ];
                }

                if( ownerType.BaseType is Type baseType )
                {
                    return PropertyForOwnerType( name, type.BaseType, baseType );
                }

                throw new Exception( "Cannot find dependency property '" + name + "' for type '" + ownerType.FullName + "'. Make sure to call InitializeProperties() before using a property for this type." );
            }
        }

        public static T Get< T >( this DependencyObject o, [ CallerMemberName ] string name = "" )
        {
            PropertyInfo prop;

            prop = PropertyForOwnerType<T>( name, o.GetType() );

            return ( T )( o.GetValue( prop.Property ) );
        }

        public static void Set< T >( this DependencyObject o, T value, [ CallerMemberName ] string name = "" )
        {
            PropertyInfo prop;

            prop = PropertyForOwnerType<T>( name, o.GetType() );

            o.SetValue( prop.Property, value );
        }

        public static void InitializeProperties( this DependencyObject o )
        {
            InitializeProperties( o.GetType() );
        }

        public static void InitializeProperties( Type type )
        {
            lock( Lock )
            {
                if( Inited.Contains( type.FullName ) )
                {
                    return;
                }

                Inited.Add( type.FullName );
                Props.Add( type.FullName, new Dictionary<string, PropertyInfo>() );

                foreach( System.Reflection.PropertyInfo prop in type.GetProperties() )
                {
                    foreach( object p in prop.GetCustomAttributes( true ) )
                    {
                        if( p is DP dp && prop.DeclaringType == type )
                        {
                            Props[ type.FullName ].Add( prop.Name, new PropertyInfo( type, prop.Name, prop.PropertyType, dp.DefaultValue ) );
                        }
                    }
                }
            }
        }
    }
}
