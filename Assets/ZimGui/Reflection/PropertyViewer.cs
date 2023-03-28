using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Str=System.ReadOnlySpan<char>;
namespace ZimGui.Reflection {
  //  public   delegate void PropertyViewer(Str text, object instance,bool isReadOnly);
    public static class PropertyView {
        static Dictionary<(Type, string), (int,Func<object, object>)> GetterCache=new (16);
        public static  bool ViewProperty(this object o, string fieldName) {
            var type=o.GetType();
            
            var propertyInfo = type.GetProperty(fieldName);
            var propertyType = propertyInfo.PropertyType;
            return false;
        }
        public static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return null;
#if !UNITY_EDITOR&&ENABLE_IL2CPP
                return propertyInfo.GetValue;
#else 
            if (propertyInfo.GetGetMethod(true).IsStatic)  {
                Expression body = Expression.Convert(Expression.MakeMemberAccess(null, propertyInfo), typeof(object));
                var lambda = Expression.Lambda<Func<object>>(body).Compile();
                return _ => lambda();
      
            }
            if (propertyInfo.DeclaringType != null)
            {
                var objParam = Expression.Parameter(typeof(object), "obj");
                var tParam = Expression.Convert(objParam, propertyInfo.DeclaringType);
                Expression body = Expression.Convert(Expression.MakeMemberAccess(tParam, propertyInfo), typeof(object));
                return Expression.Lambda<Func<object, object>>(body, objParam).Compile();
            }
            return null;
#endif    
        }
    }
}