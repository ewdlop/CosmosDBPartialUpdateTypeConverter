﻿using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace CosmosDBPartialUpdateTypeConverter
{
    /// <summary>
    /// Need more work
    /// </summary>
    public static class PatchOperationExtension
    {
        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, Microsoft.AspNetCore.JsonPatch.JsonPatchDocument jsonPatchDocument)
        {
            ArgumentNullException.ThrowIfNull(jsonPatchDocument, nameof(jsonPatchDocument));
            //From Microsoft:
            //Azure Cosmos DB partial document update is inspired by JSON Patch RFC 6902.
            //There are other features such as Conditional Patch
            //while some of the features of JSON Patch RFC 6902 such as (Copy, Test) have not been implemented.

            //No-op for this case would be just don't do anything I suppose
            foreach (var operation in jsonPatchDocument.Operations)
            {
                switch (operation.OperationType)
                {
                    case OperationType.Add:
                        patchOperations.Add(PatchOperation.Add(operation.path, operation.value));
                        break;
                    case OperationType.Copy:
                        break;
                    case OperationType.Move:
                        patchOperations.Add(PatchOperation.Move(operation.from, operation.path));
                        break;
                    case OperationType.Remove:
                        patchOperations.Add(PatchOperation.Remove(operation.path));
                        break;
                    case OperationType.Replace:
                        patchOperations.Add(PatchOperation.Replace(operation.path, operation.value));
                        break;
                    case OperationType.Test:
                        break;
                    case OperationType.Invalid: //create an invalid patch operation[?]
                        break;
                    default:
                        break;
                }
            }
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, List<PatchOperation> operations)
        {
            patchOperations.AddRange(operations);
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, List<PatchOperation<T>> operations)
        {
            patchOperations.AddRange(operations);
            return patchOperations;
        }

        public static List<PatchOperation<T>> Add<T>(this List<PatchOperation<T>> patchOperations, List<PatchOperation<T>> operations)
        {
            patchOperations.AddRange(operations);
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, params PatchOperation[] operations)
        {
            patchOperations.AddRange(operations);
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Add(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, string path, T? value)
        {
            patchOperations.Add(PatchOperation.Add(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject, nameof(jObject));
            foreach (var property in jObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                patchOperations.Add(PatchOperation.Add(BuildPath(key), value));
            }
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, T entity)
            where T : class
        {
            patchOperations.AddRange(entity switch
            {
                string => Enumerable.Empty<PatchOperation>(),
                _ => typeof(T).GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, List<T> entities)
            where T : class
        {
            patchOperations.AddRange(entities.Where(entity => entity is not string)
                .SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
            where T : class
        {
            patchOperations.AddRange(entities.Where(entity => entity is not string)
                .SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> Add<T>(this List<PatchOperation> patchOperations, params T[] entities)
            where T : class
        {
            patchOperations.AddRange(entities switch
            {
            [string item1, string item2] => [PatchOperation.Add(BuildPath(item1), item2)],
                _ => entities.Where(entity => entity is not string).SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(entity))))
            });
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            patchOperations.AddRange(value switch
            {
                (string item1, object item2) => [PatchOperation.Add(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Add(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties().Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
                    .Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(value)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, IEnumerable<object> values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Add(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Add(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(value)))
            }));
            return patchOperations;
        }

        public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, params object[] values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Add(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Add(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Add(BuildPath(property.Name), property.GetValue(value)))
            }));
            return patchOperations;
        }

        //For Appending to an json array
        //the path must exist first
        //need more testing for the corner cases
        public static List<PatchOperation> AddAppend(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Add($"{BuildPath(path)}/`", value));
            return patchOperations;
        }

        public static List<PatchOperation> AddIncrement(this List<PatchOperation> patchOperations, string path, long value)
        {
            patchOperations.Add(PatchOperation.Increment(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddIncrement(this List<PatchOperation> patchOperations, string path, double value)
        {
            patchOperations.Add(PatchOperation.Increment(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddMove(this List<PatchOperation> patchOperations, string from, string path)
        {
            patchOperations.Add(PatchOperation.Move(BuildPath(from), BuildPath(path)));
            return patchOperations;
        }

        public static List<PatchOperation> AddRemove(this List<PatchOperation> patchOperations, string path)
        {
            patchOperations.Add(PatchOperation.Remove(BuildPath(path)));
            return patchOperations;
        }

        public static List<PatchOperation> AddReplace(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Replace(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, string path, object? value)
        {
            patchOperations.Add(PatchOperation.Set(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, string path, T? value)
        {
            patchOperations.Add(PatchOperation.Set(BuildPath(path), value));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, T entity)
            where T : class
        {
            patchOperations.AddRange(entity switch
            {
                string => Enumerable.Empty<PatchOperation>(),
                _ => typeof(T).GetProperties().Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(entity)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject, nameof(jObject));
            foreach (var property in jObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                patchOperations.Add(PatchOperation.Set(BuildPath(key), value));
            }
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, IEnumerable<T> entities)
            where T : class
        {
            patchOperations.AddRange(entities.Where(entity => entity is not string)
                .SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(entity)))));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet<T>(this List<PatchOperation> patchOperations, params T[] entities)
            where T : class
        {
            patchOperations.AddRange(entities switch
            {
            [string item1, string item2] => [PatchOperation.Set(BuildPath(item1), item2)],
                _ => entities.Where(entity => entity is not string).SelectMany(entity => entity.GetType().GetProperties().Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(entity))))
            });
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, object value, Func<PropertyInfo, bool>? propertyInfoFilter = null)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            patchOperations.AddRange(value switch
            {
                (string item1, object item2) => [PatchOperation.Set(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Set(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties().Where(propertyInfoFilter is not null ? propertyInfoFilter : _ => true)
                    .Select(property => PatchOperation.Set(property.Name, property.GetValue(value)))
            });
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, IEnumerable<object> values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Set(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Set(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Set(property.Name, property.GetValue(value)))
            }));
            return patchOperations;
        }

        public static List<PatchOperation> AddSet(this List<PatchOperation> patchOperations, params object[] values)
        {
            patchOperations.AddRange(values.SelectMany(value => value switch
            {
                (string item1, object item2) => [PatchOperation.Set(BuildPath(item1), item2)],
                (object item1, object item2) => [PatchOperation.Set(BuildPath(item1.ToString()), item2)], //need to check null or do something
                int or long or float or double or decimal or string or bool or null => Enumerable.Empty<PatchOperation>(),
                _ => value.GetType().GetProperties()
                    .Select(property => PatchOperation.Set(BuildPath(property.Name), property.GetValue(value)))
            }));
            return patchOperations;
        }

        //Todo
        //Should use regex to check if string is of the form ^\/[A-Za-z0-9]+$ 
        //or of the form ^\/[A-Za-z0-9]+\/[A-Za-z0-9]+$, recursively for more than 2 level
        //for path such as /address/city
        public static string BuildPath(string path)
        {
            return path.StartsWith('/') ? path : $"/{path}";
        }

        public static PatchOperation ToPatchOperation<T>(this PatchOperation<T> patchOperation)
        {
            return patchOperation.OperationType switch
            {
                PatchOperationType.Add => PatchOperation.Add(patchOperation.Path, patchOperation.Value),
                PatchOperationType.Remove => PatchOperation.Remove(patchOperation.Path),
                PatchOperationType.Replace => PatchOperation.Replace(patchOperation.Path, patchOperation.Value),
                PatchOperationType.Set => PatchOperation.Set(patchOperation.Path, patchOperation.Value),
                PatchOperationType.Increment => patchOperation switch
                {
                    PatchOperation<long> longPatchOperation => PatchOperation.Increment(patchOperation.Path, longPatchOperation.Value),
                    PatchOperation<double> doublePatchOperation => PatchOperation.Increment(patchOperation.Path, doublePatchOperation.Value),
                    _ => throw new NotSupportedException("Unsupported PatchOperation type")
                },
                PatchOperationType.Move => PatchOperation.Move(patchOperation.From, patchOperation.Path),
                _ => throw new NotSupportedException("Unsupported PatchOperation type")
            };
        }

        public static PatchOperation ToPatchOperation(this Operation operation)
        {
            return operation.OperationType switch
            {
                OperationType.Add => PatchOperation.Add(operation.path, operation.value),
                OperationType.Remove => PatchOperation.Remove(operation.path),
                OperationType.Replace => PatchOperation.Replace(operation.path, operation.value),
                OperationType.Move => PatchOperation.Move(operation.from, operation.path),
                OperationType.Copy => throw new NotSupportedException("Copy operation is not supported"),
                OperationType.Test => throw new NotSupportedException("Test operation is not supported"),
                _ => throw new NotSupportedException("Unsupported OperationType")
            };
        }

        public static PatchOperation ToPatchOperation<T>(this Operation<T> operation) where T : class
        {
            return operation.OperationType switch
            {
                OperationType.Add => PatchOperation.Add(operation.path, operation.value),
                OperationType.Remove => PatchOperation.Remove(operation.path),
                OperationType.Replace => PatchOperation.Replace(operation.path, operation.value),
                OperationType.Move => PatchOperation.Move(operation.from, operation.path),
                OperationType.Copy => throw new NotSupportedException("Copy operation is not supported"),
                OperationType.Test => throw new NotSupportedException("Test operation is not supported"),
                _ => throw new NotSupportedException("Unsupported OperationType")
            };
        }

        public static PatchOperation ToPatchOperation(this JsonPatchOperation jsonPatchOperation)
        {
            return jsonPatchOperation.Operation switch
            {
                Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add => PatchOperation.Add(jsonPatchOperation.Path, jsonPatchOperation.Value),
                Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Remove => PatchOperation.Remove(jsonPatchOperation.Path),
                Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Replace => PatchOperation.Replace(jsonPatchOperation.Path, jsonPatchOperation.Value),
                Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Move => PatchOperation.Move(jsonPatchOperation.From, jsonPatchOperation.Path),
                Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Copy => throw new NotSupportedException("Copy operation is not supported"),
                Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Test => throw new NotSupportedException("Test operation is not supported"),
                _ => throw new NotSupportedException("Unsupported OperationType")
            };
        }
    }
}