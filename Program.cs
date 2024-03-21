// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;

Console.WriteLine("Hello, World!");

//Not done yet
public static class PatchOperationsBuilder
{
    public static List<PatchOperation> Add(this List<PatchOperation> patchOperations, string path, object value)
    {
        patchOperations.Add(PatchOperation.Add(path, value));
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
}