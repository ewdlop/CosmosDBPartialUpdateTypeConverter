using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch;

namespace CosmosDBPartialUpdateTypeConverter
{
    /// <summary>
    ///Not done yet
    ///Maybe to limit to 10 PatchOperations is not a good idea
    ///Need more testing
    ///Not thread safe
    ///https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-faq
    /// </summary>
    public class PatchOperationList(List<PatchOperation> _patchOperations) : IList<PatchOperation>, IReadOnlyList<PatchOperation>
    {

        public static implicit operator PatchOperationList(List<PatchOperation> patchOperations) => new PatchOperationList([..patchOperations]); //shadow copy to prevent modifications to the original list
        public static implicit operator PatchOperationList(PatchOperation[] patchOperations) => new PatchOperationList([..patchOperations]); //shadow copy to prevent modifications to the original list
        public static implicit operator PatchOperationList(PatchOperation patchOperation) => new PatchOperationList(patchOperation);
        public PatchOperationList() : this([]) { }

        public PatchOperationList(PatchOperationList patchOperationList) : this([.. patchOperationList]) { } //shadow copy to prevent modifications to the original list

        public PatchOperationList(IList<PatchOperation> patchOperations) : this([.. patchOperations]) { } //shadow copy to prevent modifications to the original list

        public PatchOperationList(IEnumerable<PatchOperation> patchOperations) : this(patchOperations.ToList()) { } //shadow a copy to prevent modifications to the original list

        public PatchOperationList(PatchOperation patchOperation) : this([patchOperation]) { }

        public int Count => _patchOperations.Count;
        
        public PatchOperation this[int index]
        {
            get => _patchOperations[index];
            set => _patchOperations[index] = value;
        }

        public PatchOperationListBuilder Builder => new(_patchOperations);

        public bool IsReadOnly => false;

        PatchOperation IList<PatchOperation>.this[int index] 
        {
            get => (_patchOperations as IList<PatchOperation>)[index];
            set => (_patchOperations as IList<PatchOperation>)[index] = value;
        }

        public void Add(string path, object? value) => _patchOperations.Add(path, value);

        public void Add(PatchOperation item) => _patchOperations.Add(item);

        public void Add(JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject, nameof(jObject));
            _patchOperations.Add(jObject);
        }

        public void Add(JsonPatchDocument jsonPatchDocument)
        {
            ArgumentNullException.ThrowIfNull(jsonPatchDocument, nameof(jsonPatchDocument));
            _patchOperations.Add(jsonPatchDocument);
        }

        public void Add(List<PatchOperation> operations) => _patchOperations.Add(operations);

        public void Add<T>(T entity)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _patchOperations.Add(entity);
        }

        public void Add<T>(IEnumerable<T> entities)
            where T : class => _patchOperations.Add(entities);

        public void Add<T>(List<T> entities)
            where T : class => _patchOperations.Add(entities);

        public void Add<T>(params T[] entities)
            where T : class
        {
            if(entities is PatchOperation[] patchOperations)
            {
                _patchOperations.Add(patchOperations);
            }
            else
            {
                _patchOperations.Add(entities);
            }
        }

        public void Add(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.Add(value, propertyInfoFilter);
        }

        public void AddAppend(string path, object? value) => _patchOperations.AddAppend(path, value);

        public void AddIncrement(string path, long value) => _patchOperations.AddIncrement(path, value);

        public void AddIncrement(string path, double value) => _patchOperations.AddIncrement(path, value);

        public void AddMove(string from, string path) => _patchOperations.AddMove(from, path);

        public void AddRemove(string path) => _patchOperations.AddRemove(path);

        public void AddReplace(string path, object? value) => _patchOperations.AddReplace(path, value);

        public void AddSet(string path, object? value) => _patchOperations.AddSet(path, value);

        public void AddSet(JObject value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _patchOperations.AddSet(value);
        }

        public void AddSet<T>(T entity)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _patchOperations.AddSet(entity);
        }

        public void AddSet<T>(IEnumerable<T> entities)
            where T : class => _patchOperations.AddSet(entities);

        public void AddSet<T>(params T[] entities)
            where T : class => _patchOperations.AddSet(entities);

        public void AddSet(object value, Func<PropertyInfo, bool>? propertyInfoFilter = null) => _patchOperations.AddSet(value, propertyInfoFilter);

        public void AddSet(IEnumerable<object> values) => _patchOperations.AddSet(values);

        public void AddSet(params object[] values) => _patchOperations.AddSet(values);

        public int IndexOf(PatchOperation item) => _patchOperations.IndexOf(item);

        public void Insert(int index, PatchOperation item) => _patchOperations.Insert(index, item);

        public void RemoveAt(int index) => _patchOperations.RemoveAt(index);

        public void Clear() => _patchOperations.Clear();

        public bool Contains(PatchOperation item) => _patchOperations.Contains(item);

        public void CopyTo(PatchOperation[] array, int arrayIndex) => _patchOperations.CopyTo(array, arrayIndex);

        public bool Remove(PatchOperation item) => _patchOperations.Remove(item);

        public IEnumerator<PatchOperation> GetEnumerator() => _patchOperations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ForEach(Action<PatchOperation> action) => _patchOperations.ForEach(action);

        public IReadOnlyList<PatchOperation> AsReadOnly() => _patchOperations.AsReadOnly();
    }
}