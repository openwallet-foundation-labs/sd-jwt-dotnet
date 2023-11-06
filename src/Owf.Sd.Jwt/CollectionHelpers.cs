namespace Owf.Sd.Jwt;

public static class CollectionHelpers
{

    public static Dictionary<string, object>? ConvertToDictionary(object obj)
    {
        if (obj == null)
        {
            return new(); // Handle null input if needed
        }

        // Check if the input is a Dictionary<string, object>
        if (obj is Dictionary<string, object> dictionaryObject)
        {
            return dictionaryObject;
        }

        // Check if the input is a Dictionary<string, string> and convert it
        if (obj is Dictionary<string, string> dictionaryString)
        {
            return dictionaryString.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }

        // Check if the input is a Dictionary<string, int> and convert it
        if (obj is Dictionary<string, int> dictionaryInt)
        {
            return dictionaryInt.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }

        // Handle other cases or throw an exception if the type is not supported
        throw new ArgumentException("Unsupported dictionary type");
    }

    public static List<object> ConvertToList(object obj)
    {
        if (obj is List<object> listObject)
        {
            // obj is already a List<object>
            return listObject;
        }
        else if (obj is IEnumerable<object> enumerable)
        {
            // obj is an IEnumerable<object>, so convert it to List<object>
            return enumerable.ToList();
        }
        else
        {
            // Handle the case where the object can't be converted to a List<object>.
            throw new InvalidOperationException("Object cannot be converted to List<object>.");
        }
    }

    public static bool IsDictionaryType(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        Type type = obj.GetType();

        // Check if the type implements IDictionary<string, T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type[] genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 2 && genericArgs[0] == typeof(string))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsListType(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        Type type = obj.GetType();
        return IsListType(type);
    }

    public static bool IsListType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return true;
        }

        return false;
    }
}