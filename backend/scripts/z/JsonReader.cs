using Newtonsoft.Json;

namespace Z;

public static class JsonReader
{
    public static List<T> LoadListedData<T, TWrapper>(string path, Func<TWrapper, T> factory)
    {
        // Read the json file into a string
        string jsonPath = GetFullJsonPath(path);
        string jsonString = File.ReadAllText(jsonPath);
        // Load data in wrapper list
        List<TWrapper> wrappers = JsonConvert.DeserializeObject<List<TWrapper>>(jsonString) ?? throw new Exception("Failed to load data from: " + jsonPath);

        // Check if actual data is loaded
        if (wrappers == null || wrappers.Count == 0)
            throw new Exception("Failed to load station from: " + jsonPath);

        // Convert wrapper list to object list
        List<T> objects = [];
        foreach (TWrapper entry in wrappers)
        {
            objects.Add(factory(entry));
        }

        return objects;
    }

    public static List<T> LoadNestedListData<T, TRoot, TItem>(string path, Func<TRoot, List<TItem>> extractList, Func<TItem, T> factory)
    {
        // Read the json file into a string
        string jsonPath = GetFullJsonPath(path);
        string jsonString = File.ReadAllText(jsonPath);

        // Deserialize the root wrapper
        TRoot root = JsonConvert.DeserializeObject<TRoot>(jsonString) ?? throw new Exception("Failed to load data from: " + jsonPath);
        List<TItem> items = extractList(root);

        if (items == null || items.Count == 0)
            throw new Exception("No nested list items found in: " + jsonPath);

        List<T> result = [.. items.Select(factory)];
        return result;
    }

    private static string GetFullJsonPath(string path)
    {
        string startPath = AppContext.BaseDirectory;
        DirectoryInfo directory = new(startPath);

        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new Exception("Cant find git folder");
        }

        //Finds the git root folder
        string fullPath = directory.FullName;
        fullPath = Path.Combine(fullPath, path);
        return fullPath.Replace('\\', '/');
    }
}