using Newtonsoft.Json;
using shared;

namespace Json;

public static class JsonReader
{
    /// <summary>
    /// Loads and deserializes a JSON file into an object of type T.
    /// </summary>
    public static T LoadData<T>(string filename)
    {
        // Read the json file into a string
        string jsonString = LoadJsonData(filename);
        return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new Exception("Failed to load data from: ");
    }

    /// <summary>
    /// Loads and deserializes a JSON file into a list of objects of type T using a wrapper type and factory function.
    /// </summary>
    public static List<T> LoadListedData<T, TWrapper>(string filename, Func<TWrapper, T> factory)
    {
        // Read the json file into a string
        string jsonString = LoadJsonData(filename);
        // Load data in wrapper list
        List<TWrapper> wrappers = JsonConvert.DeserializeObject<List<TWrapper>>(jsonString) ?? throw new Exception("Failed to load data from: " + filename);

        // Check if actual data is loaded
        if (wrappers == null || wrappers.Count == 0)
            throw new Exception("Failed to load station from: " + filename);

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
    public static List<T> LoadNestedListData<TRoot, T>(string filename, Func<TRoot, List<T>> extractList)
    {
        // Read the json file into a string
        string jsonString = LoadJsonData(filename);

        // Deserialize the root wrapper
        TRoot root = JsonConvert.DeserializeObject<TRoot>(jsonString) ?? throw new Exception("Failed to load data from: " + filename);
        List<T> items = extractList(root);

        if (items == null || items.Count == 0)
            throw new Exception("No nested list items found in: " + filename);

        return items;
    }

    /// <summary>
    /// Loads and reads embedded json file from shared project.
    /// </summary>
    private static string LoadJsonData(string filename)
    {
        var assembly = typeof(WebSocketMessage).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(string.Format("shared.jsondata.{0}", filename)) ?? throw new Exception("Failed to find file: " + filename);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
