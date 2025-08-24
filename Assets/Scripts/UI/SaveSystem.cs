using UnityEngine;

public static class SaveSystem
{
    private const string SaveExistsKey = "SAVE_EXISTS";

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
    }

    public static void CreateNewSave()
    {
        // TODO: initialize your actual save data here
        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.Save();
    }

    public static void LoadLatest()
    {
        // TODO: load your actual save data here
        // For a stub, nothing to do—just trust that data will be loaded in your Game scene
    }

    public static void WipeSaveForTesting()
    {
        PlayerPrefs.DeleteKey(SaveExistsKey);
        PlayerPrefs.Save();
    }
}
