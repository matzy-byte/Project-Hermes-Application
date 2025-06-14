using Newtonsoft.Json;

namespace Json;

public static class JsonReader
{
    /// <summary>
    /// Loads and deserializes a JSON file into an object of type T.
    /// </summary>
    public static T LoadData<T>(string path)
    {
        // Read the json file into a string
        string jsonPath = GetFullPath(path);
        string jsonString = File.ReadAllText(jsonPath);
        return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new Exception("Failed to load data from: " + jsonPath);
    }

    /// <summary>
    /// Loads and deserializes a JSON file into a list of objects of type T using a wrapper type and factory function.
    /// </summary>
    public static List<T> LoadListedData<T, TWrapper>(string path, Func<TWrapper, T> factory)
    {
        // Read the json file into a string
        string jsonPath = GetFullPath(path);
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

    /// <summary>
    /// Loads and deserializes a JSON file into a root object and extracts a nested list from it.
    /// </summary>
    public static List<T> LoadNestedListData<TRoot, T>(string path, Func<TRoot, List<T>> extractList)
    {
        // Read the json file into a string
        string jsonPath = GetFullPath(path);
        string jsonString = File.ReadAllText(jsonPath);

        // Deserialize the root wrapper
        TRoot root = JsonConvert.DeserializeObject<TRoot>(jsonString) ?? throw new Exception("Failed to load data from: " + jsonPath);
        List<T> items = extractList(root);

        if (items == null || items.Count == 0)
            throw new Exception("No nested list items found in: " + jsonPath);

        return items;
    }

    /// <summary>
    /// Gets the full path of a file relative to the git root directory.
    /// </summary>
    public static string GetFullPath(string path)
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
