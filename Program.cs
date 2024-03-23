

// See https://aka.ms/new-console-template for more information
using CosmosDBPartialUpdateTypeConverter;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Reflection;


Console.WriteLine("Hello, World!");


//Not done yet

namespace CosmosDBPartialUpdateTypeConverter
{
    public static class PatchOperationListExtension
    {
        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, string path, object? value)
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
            where T : notnull
        {
            patchOperations.AddRange(typeof(T).GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(entity))));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
            where T : notnull
        {
            patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, params T[] entities)
            where T : notnull
        {
            patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(property.Name, property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            patchOperations.AddRange(value.GetType()
                .GetProperties()
                .Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
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

        public static List<PatchOperation> AddAppend(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Add($"{path}/`", value));
            return patchOperations;
        }

        public static List<PatchOperation> AddIncrement(this List<PatchOperation> patchOperations, string path, long value)
        {
            patchOperations.Add(PatchOperation.Increment(path, value));
            return patchOperations;
        }

        public static List<PatchOperation> AddIncrement(this List<PatchOperation> patchOperations, string path, double value)
        {
            patchOperations.Add(PatchOperation.Increment(path, value));
            return patchOperations;
        }

        public static List<PatchOperation> AddMove(this List<PatchOperation> patchOperations, string from, string path)
        {
            patchOperations.Add(PatchOperation.Move(from, path));
            return patchOperations;
        }

        public static List<PatchOperation> AddRemove(this List<PatchOperation> patchOperations, string path)
        {
            patchOperations.Add(PatchOperation.Remove(path));
            return patchOperations;
        }

        public static List<PatchOperation> AddReplace(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Replace(path, value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Set(path, value));
            return patchOperations;
        }

        //TODO
        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, JObject value)
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

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, T entity)
        {
            patchOperations.AddRange(typeof(T).GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(entity))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
        {
            patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, params T[] entities)
        {
            patchOperations.AddRange(entities.SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, object? value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            patchOperations.AddRange(value.GetType()
                .GetProperties()
                .Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
                .Select(property => PatchOperation.Set(property.Name, property.GetValue(value))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, IEnumerable<object> values)
        {
            patchOperations.AddRange(values.SelectMany(item => item.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(item)))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, params object[] values)
        {
            patchOperations.AddRange(values.SelectMany(item => item.GetType().GetProperties().Select(property => PatchOperation.Set(property.Name, property.GetValue(item)))));
            return patchOperations;
        }
    }

    public class PatchOperationListBuilder(List<PatchOperation> _patchOperations)
    {
        public PatchOperationListBuilder() : this(new List<PatchOperation>()) { }
        public PatchOperationListBuilder(IEnumerable<PatchOperation> patchOperations) : this(patchOperations.ToList()) { }
        public PatchOperationListBuilder(PatchOperation patchOperation) : this(new List<PatchOperation> { patchOperation }) { }
        public PatchOperationListBuilder(params PatchOperation[] patchOperations) : this(patchOperations.ToList()) { }

        public PatchOperationListBuilder WithAdd(string path, object value)
        {
            _patchOperations.Add(path, value);
            return this;
        }

        public PatchOperationListBuilder WithAdd(JObject value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.Add(value);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(T entity)
            where T : notnull
        {
            _patchOperations.Add(entity);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(IEnumerable<T> entities)
            where T : notnull
        {
            _patchOperations.Add(entities);
            return this;
        }

        public PatchOperationListBuilder WithAdd<T>(params T[] entities)
            where T : notnull
        {
            _patchOperations.Add(entities);
            return this;
        }

        public PatchOperationListBuilder WithAdd(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            _patchOperations.Add(value, propertyInfoFilter);
            return this;
        }

        public PatchOperationListBuilder WithAddAppend(string path, object? value)
        {
            _patchOperations.AddAppend(path, value);
            return this;
        }

        public PatchOperationListBuilder WithIncrement(string path, long value)
        {
            _patchOperations.AddIncrement(path, value);
            return this;
        }

        public PatchOperationListBuilder WithMove(string from, string path)
        {
            _patchOperations.AddMove(from, path);
            return this;
        }

        public PatchOperationListBuilder WithRemove(string path)
        {
            _patchOperations.AddRemove(path);
            return this;
        }

        public PatchOperationListBuilder WithReplace(string path, object? value)
        {
            _patchOperations.AddReplace(path, value);
            return this;
        }

        public PatchOperationListBuilder WithSet(string path, object? value)
        {
            _patchOperations.AddSet(path, value);
            return this;
        }

        public PatchOperationListBuilder WithSet(JObject value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.AddSet(value);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(T entity)
            where T : notnull
        {
            _patchOperations.AddSet(entity);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(IEnumerable<T> entities)
            where T : notnull
        {
            _patchOperations.AddSet(entities);
            return this;
        }

        public PatchOperationListBuilder WithSet<T>(params T[] entities)
            where T : notnull
        {
            _patchOperations.AddSet(entities);
            return this;
        }

        public PatchOperationListBuilder WithSet(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            _patchOperations.AddSet(value, propertyInfoFilter);
            return this;
        }

        public List<PatchOperation> Build()
        {
            return _patchOperations;
        }
    }
}