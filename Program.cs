// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Reflection;

Console.WriteLine("Hello, World!");

//Not done yet
public static class PatchOperationListExtension
{
    public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, string path, object value)
    {
        patchOperations.Add(PatchOperation.Add(path, value));
        return patchOperations;
    }

    //TODO
    public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, JObject value)
    {
        //foreach (var property in value.Properties())
        //{
        //    var propertyValue = property.Value.Type == JTokenType.Object || property.Value.Type == JTokenType.Array
        //        ? property.Value.ToString() // For nested objects or arrays, convert to string
        //        : (object)property.Value.ToObject(typeof(object)); // Convert simple values to their native types

        //    patchOperations.Add(PatchOperation.Add(property.Path, propertyValue));
        //}

        return patchOperations;
    }

    public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, T entity)
    {
        patchOperations.AddRange(typeof(T).GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(entity))));
        return patchOperations;
    }

    public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
    {
        patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(entity)))));
        return patchOperations;
    }

    public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, params T[] entities)
    {
        patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(entity)))));
        return patchOperations;
    }

    public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
    {      
        patchOperations.AddRange(value.GetType()
            .GetProperties()
            .Where(propertyInfoFilter is not null ? propertyInfoFilter : _=>true)
            .Select(property => PatchOperation.Add(property.Name, property.GetValue(value))));
        return patchOperations;
    }

    public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, IEnumerable<object> values)
    {
        patchOperations.AddRange(values.SelectMany(item => item.GetType().GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(item)))));
        return patchOperations;
    }

    public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, params object[] values)
    {
        patchOperations.AddRange(values.SelectMany(item => item.GetType().GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(item)))));
        return patchOperations;
    }

    public static List<PatchOperation> Append(this List<PatchOperation> patchOperations, string path, object value)
    {
        patchOperations.Add(PatchOperation.Add($"{path}/`", value));
        return patchOperations;
    }

    public static List<PatchOperation> Increment(this List<PatchOperation> patchOperations, string path, long value)
    {
        patchOperations.Add(PatchOperation.Increment(path, value));
        return patchOperations;
    }

    public static List<PatchOperation> Move(this List<PatchOperation> patchOperations, string from, string path)
    {
        patchOperations.Add(PatchOperation.Move(from, path));
        return patchOperations;
    }

    public static List<PatchOperation> Remove(this List<PatchOperation> patchOperations, string path)
    {
        patchOperations.Add(PatchOperation.Remove(path));
        return patchOperations;
    }

    public static List<PatchOperation> Replace(this List<PatchOperation> patchOperations, string path, object value)
    {
        patchOperations.Add(PatchOperation.Replace(path, value));
        return patchOperations;
    }

    public static List<PatchOperation> Set(this List<PatchOperation> patchOperations, string path, object value)
    {
        patchOperations.Add(PatchOperation.Set(path, value));
        return patchOperations;
    }

    //TODO
    public static List<PatchOperation> Set(this List<PatchOperation> patchOperations, JObject value)
    {
        //foreach (var property in value.Properties())
        //{
        //    var propertyValue = property.Value.Type == JTokenType.Object || property.Value.Type == JTokenType.Array
        //        ? property.Value.ToString() // For nested objects or arrays, convert to string
        //        : (object)property.Value.ToObject(typeof(object)); // Convert simple values to their native types

        //    patchOperations.Add(PatchOperation.Add(property.Path, propertyValue));
        //}

        return patchOperations;
    }

    public static List<PatchOperation> Set<T>(this List<PatchOperation> patchOperations, T entity)
    {
        patchOperations.AddRange(typeof(T).GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(entity))));
        return patchOperations;
    }

    public static List<PatchOperation> Set<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
    {
        patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(entity)))));
        return patchOperations;
    }

    public static List<PatchOperation> Set<T>(this List<PatchOperation> patchOperations, params T[] entities)
    {
        patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(entity)))));
        return patchOperations;
    }

    public static List<PatchOperation> Set(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
    {
        patchOperations.AddRange(value.GetType()
            .GetProperties()
            .Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
            .Select(property => PatchOperation.Set(property.Name, property.GetValue(value))));
        return patchOperations;
    }

    public static List<PatchOperation> Set(this List<PatchOperation> patchOperations, IEnumerable<object> values)
    {
        patchOperations.AddRange(values.SelectMany(item => item.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(item)))));
        return patchOperations;
    }

    public static List<PatchOperation> Set(this List<PatchOperation> patchOperations, params object[] values)
    {
        patchOperations.AddRange(values.SelectMany(item => item.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(item)))));
        return patchOperations;
    }
}