using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using Logger.Common.Base.ObjectModel.Exceptions;
using Logger.Common.Base.Properties;




namespace Logger.Common.Base.DataTypes
{
    public static class TypeExtensions
    {
        #region Static Methods

        public static object CallMethod (this Type type, string name, BindingFlags bindingFlags, Type returnType, Type[] parameterTypes, object[] parameterValues)
        {
            return type.CallMethod(null, name, bindingFlags, returnType, parameterTypes, parameterValues);
        }

        public static object CallMethod (this Type type, object instance, string name, BindingFlags bindingFlags, Type returnType, Type[] parameterTypes, object[] parameterValues)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            if (returnType == typeof(void))
            {
                returnType = null;
            }

            if (parameterValues == null)
            {
                parameterValues = new object[0];
            }

            if (parameterTypes == null)
            {
                List<Type> tempTypes = new List<Type>();
                foreach (object parameterValue in parameterValues)
                {
                    tempTypes.Add(parameterValue == null ? typeof(object) : parameterValue.GetType());
                }
                parameterTypes = tempTypes.ToArray();
            }

            if (parameterTypes.Length > 0)
            {
                if (parameterTypes.Length != parameterValues.Length)
                {
                    throw new ArgumentException(Resources.TypeExtensions_ParameterTypeCountMismatchParameterValueCount, nameof(parameterTypes));
                }
            }

            MethodInfo[] methods = type.FindMethods(name, bindingFlags, returnType, parameterTypes);

            if (methods.Length == 0)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_NoMatchingMembers, name));
            }

            if (methods.Length != 1)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_TooManyMatchingMembers, name));
            }

            return methods[0].Invoke(instance, parameterValues);
        }

        public static bool CanConvertTo (this Type type, Type targetType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (type.ImplementsInterface(targetType))
            {
                return true;
            }

            Type[] implementedTypes = type.GetTypeInheritance(true);

            foreach (Type implementedType in implementedTypes)
            {
                if (implementedType.Equals(targetType))
                {
                    return true;
                }
            }

            return targetType.IsAssignableFrom(type);
        }

        public static object CreateDefaultValue (this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsInterface || type.IsClass)
            {
                return null;
            }

            return type.CreateInstance(false);
        }

        public static object CreateInstance (this Type type, bool includeNonPublic, params object[] constructorParameters)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (constructorParameters == null)
            {
                constructorParameters = new object[0];
            }

            BindingFlags bindingFlags = BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;

            if (includeNonPublic)
            {
                bindingFlags |= BindingFlags.NonPublic;
            }

            return Activator.CreateInstance(type, bindingFlags, null, constructorParameters, CultureInfo.InvariantCulture);
        }

        public static PropertyInfo[] FindIndexer (this Type type, string name, BindingFlags bindingFlags, Type propertyType, Type[] indexerTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            if (indexerTypes == null)
            {
                indexerTypes = new Type[0];
            }

            MemberInfo[] members = type.FindMembers(MemberTypes.Property, bindingFlags, (memberInfo, criteria) =>
            {
                if (string.Equals(memberInfo.Name, name, StringComparison.InvariantCulture))
                {
                    if (memberInfo is PropertyInfo)
                    {
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;

                        ParameterInfo[] indexer = propertyInfo.GetIndexParameters();

                        if (indexer.Length == 0)
                        {
                            return false;
                        }

                        if (( propertyType == null ) && ( indexerTypes.Length == 0 ))
                        {
                            return true;
                        }

                        if (propertyType != null)
                        {
                            if (!propertyInfo.PropertyType.IsAssignableFrom(propertyType) && !propertyType.IsAssignableFrom(propertyInfo.PropertyType))
                            {
                                return false;
                            }
                        }

                        bool indexerMatch = true;

                        if (indexer.Length == indexerTypes.Length)
                        {
                            for (int i1 = 0; i1 < indexer.Length; i1++)
                            {
                                if (!indexer[i1].ParameterType.IsAssignableFrom(indexerTypes[i1]))
                                {
                                    indexerMatch = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            indexerMatch = false;
                        }

                        if (!indexerMatch)
                        {
                            return false;
                        }

                        return true;
                    }
                }

                return false;
            }, name);

            List<PropertyInfo> properties = new List<PropertyInfo>();

            foreach (MemberInfo member in members)
            {
                if (member is PropertyInfo)
                {
                    properties.Add((PropertyInfo)member);
                }
            }

            return properties.ToArray();
        }

        public static MethodInfo[] FindMethods (this Type type, string name, BindingFlags bindingFlags, Type returnValue, Type[] parameterTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            if (returnValue == typeof(void))
            {
                returnValue = null;
            }

            if (parameterTypes == null)
            {
                parameterTypes = new Type[0];
            }

            MemberInfo[] members = type.FindMembers(MemberTypes.Method, bindingFlags, (memberInfo, criteria) =>
            {
                if (string.Equals(memberInfo.Name, name, StringComparison.InvariantCulture))
                {
                    if (memberInfo is MethodInfo)
                    {
                        MethodInfo methodInfo = memberInfo as MethodInfo;

                        Type currentReturnType = methodInfo.ReturnType;
                        if (currentReturnType == typeof(void))
                        {
                            currentReturnType = null;
                        }

                        bool returnTypeMatches = false;
                        if (( currentReturnType == null ) && ( returnValue == null ))
                        {
                            returnTypeMatches = true;
                        }
                        else if (( currentReturnType != null ) && ( returnValue != null ))
                        {
                            returnTypeMatches = returnValue.IsAssignableFrom(currentReturnType);
                        }
                        else if (returnValue == null)
                        {
                            returnTypeMatches = true;
                        }

                        if (returnTypeMatches)
                        {
                            ParameterInfo[] parameters = methodInfo.GetParameters();

                            bool parametersMatch = true;

                            if (parameters.Length == parameterTypes.Length)
                            {
                                for (int i1 = 0; i1 < parameters.Length; i1++)
                                {
                                    if (!parameters[i1].ParameterType.IsAssignableFrom(parameterTypes[i1]))
                                    {
                                        parametersMatch = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                parametersMatch = false;
                            }

                            if (parametersMatch)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }, name);

            List<MethodInfo> methods = new List<MethodInfo>();

            foreach (MemberInfo member in members)
            {
                if (member is MethodInfo)
                {
                    methods.Add((MethodInfo)member);
                }
            }

            return methods.ToArray();
        }

        public static PropertyInfo[] FindProperties (this Type type, string name, BindingFlags bindingFlags, Type propertyType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            MemberInfo[] members = type.FindMembers(MemberTypes.Property, bindingFlags, (memberInfo, criteria) =>
            {
                if (string.Equals(memberInfo.Name, name, StringComparison.InvariantCulture))
                {
                    if (memberInfo is PropertyInfo)
                    {
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;

                        if (propertyType == null)
                        {
                            return true;
                        }

                        if (propertyInfo.PropertyType.IsAssignableFrom(propertyType) || propertyType.IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }, name);

            List<PropertyInfo> properties = new List<PropertyInfo>();

            foreach (MemberInfo member in members)
            {
                if (member is PropertyInfo)
                {
                    properties.Add((PropertyInfo)member);
                }
            }

            return properties.ToArray();
        }

        public static object GetIndex (this Type type, string name, object[] indexer, BindingFlags bindingFlags, Type propertyType, Type[] indexerTypes)
        {
            return type.GetIndex(null, name, indexer, bindingFlags, propertyType, indexerTypes);
        }

        public static object GetIndex (this Type type, object instance, string name, object[] indexer, BindingFlags bindingFlags, Type propertyType, Type[] indexerTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            if (indexer == null)
            {
                indexer = new object[0];
            }

            if (indexerTypes == null)
            {
                List<Type> tempTypes = new List<Type>();
                foreach (object parameterValue in indexer)
                {
                    tempTypes.Add(parameterValue == null ? typeof(object) : parameterValue.GetType());
                }
                indexerTypes = tempTypes.ToArray();
            }

            if (indexerTypes.Length > 0)
            {
                if (indexerTypes.Length != indexer.Length)
                {
                    throw new ArgumentException(Resources.TypeExtensions_ParameterTypeCountMismatchParameterValueCount, nameof(indexerTypes));
                }
            }

            PropertyInfo[] properties = type.FindIndexer(name, bindingFlags, propertyType, indexerTypes);

            if (properties.Length == 0)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_NoMatchingMembers, name));
            }

            if (properties.Length != 1)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_TooManyMatchingMembers, name));
            }

            return properties[0].GetValue(instance, indexer);
        }

        public static object GetProperty (this Type type, string name, BindingFlags bindingFlags, Type propertyType)
        {
            return type.GetProperty(null, name, bindingFlags, propertyType);
        }

        public static object GetProperty (this Type type, object instance, string name, BindingFlags bindingFlags, Type propertyType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            PropertyInfo[] properties = type.FindProperties(name, bindingFlags, propertyType);

            if (properties.Length == 0)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_NoMatchingMembers, name));
            }

            if (properties.Length != 1)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_TooManyMatchingMembers, name));
            }

            return properties[0].GetValue(instance, null);
        }

        public static Type[] GetTypeInheritance (this Type type, bool includeSelf)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            List<Type> types = new List<Type>();

            if (includeSelf)
            {
                types.Add(type);
            }

            while (type.BaseType != null)
            {
                type = type.BaseType;
                types.Add(type);
            }

            return types.ToArray();
        }

        public static bool ImplementsInterface (this Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            Type[] implementedInterfaces = type.GetInterfaces();

            foreach (Type implementedInterface in implementedInterfaces)
            {
                if (implementedInterface.Equals(interfaceType))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetIndex (this Type type, string name, object[] indexer, BindingFlags bindingFlags, Type propertyType, Type[] indexerTypes, object value)
        {
            type.SetIndex(null, name, indexer, bindingFlags, propertyType, indexerTypes, value);
        }

        public static void SetIndex (this Type type, object instance, string name, object[] indexer, BindingFlags bindingFlags, Type propertyType, Type[] indexerTypes, object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            if (propertyType == null)
            {
                propertyType = value == null ? typeof(object) : value.GetType();
            }

            if (indexer == null)
            {
                indexer = new object[0];
            }

            if (indexerTypes == null)
            {
                List<Type> tempTypes = new List<Type>();
                foreach (object parameterValue in indexer)
                {
                    tempTypes.Add(parameterValue == null ? typeof(object) : parameterValue.GetType());
                }
                indexerTypes = tempTypes.ToArray();
            }

            if (indexerTypes.Length > 0)
            {
                if (indexerTypes.Length != indexer.Length)
                {
                    throw new ArgumentException(Resources.TypeExtensions_ParameterTypeCountMismatchParameterValueCount, nameof(indexerTypes));
                }
            }

            PropertyInfo[] properties = type.FindIndexer(name, bindingFlags, propertyType, indexerTypes);

            if (properties.Length == 0)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_NoMatchingMembers, name));
            }

            if (properties.Length != 1)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_TooManyMatchingMembers, name));
            }

            properties[0].SetValue(instance, value, indexer);
        }

        public static void SetProperty (this Type type, string name, BindingFlags bindingFlags, Type propertyType, object value)
        {
            type.SetProperty(null, name, bindingFlags, propertyType, value);
        }

        public static void SetProperty (this Type type, object instance, string name, BindingFlags bindingFlags, Type propertyType, object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(name));
            }

            if (propertyType == null)
            {
                propertyType = value == null ? typeof(object) : value.GetType();
            }

            PropertyInfo[] properties = type.FindProperties(name, bindingFlags, propertyType);

            if (properties.Length == 0)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_NoMatchingMembers, name));
            }

            if (properties.Length != 1)
            {
                throw new AmbiguousMatchException(string.Format(CultureInfo.InvariantCulture, Resources.TypeExtensions_TooManyMatchingMembers, name));
            }

            properties[0].SetValue(instance, value, null);
        }

        #endregion
    }
}
