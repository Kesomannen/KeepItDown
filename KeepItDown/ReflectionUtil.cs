using System.Reflection;

namespace KeepItDown; 

internal static class ReflectionUtil {
    const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    
    public static T GetPropertyValue<T>(this object obj, string propertyName, BindingFlags flags = DefaultFlags) {
        var property = obj.GetType().GetProperty(propertyName, flags);
        return (T) property?.GetValue(obj);
    }
    
    public static T GetFieldValue<T>(this object obj, string fieldName, BindingFlags flags = DefaultFlags) {
        var field = obj.GetType().GetField(fieldName, flags);
        return (T) field?.GetValue(obj);
    }
}