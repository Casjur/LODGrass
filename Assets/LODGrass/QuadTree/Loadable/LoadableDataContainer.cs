using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

// Improve:
// 1. Dont know if it even works
// 2. Having to pass the folder path to the container to load its data is weird

public class LoadableStructContainer<TData> : ILoadableStructContainer // Maybe rename to `LoadableStructContainer`
    where TData : struct
{
    public string FileName { get; protected set; }
    public TData? Data { get; protected set; }
    public bool IsLoaded { get; protected set; } = false;

    public LoadableStructContainer(string fileName)
    {
        this.FileName = fileName;
        this.IsLoaded = false;
    }

    public LoadableStructContainer(string fileName, TData data)
    {
        this.FileName = fileName;
        this.Data = data;
        this.IsLoaded = true;
    }

    // !!! NOT FULLY IMPLEMENTED !!! (Might be a bad idea)
    public virtual bool SetFileName(string fileName)
    {
        this.FileName = fileName;
        return true;
    }

    /// <summary>
    /// !!! NOT IMPLEMENTED !!!
    /// Saves any data in this instanced container to disk
    /// </summary>
    /// <param name="folderPath"></param>
    public virtual void SaveData(string folderPath)
    {

    }

    //public virtual IEnumerator LoadData(string folderPath)
    //{
    //    yield return null; // Yield control back to the main thread.

    //    if (this.IsLoaded)
    //    {
    //        // Data is already loaded.
    //        yield break;
    //    }

    //    UnityWebRequest www = UnityWebRequest.Get("file://" + folderPath + this.FileName); // Dont know if `this` can be used in a coroutine that was started from another class

    //    yield return www.SendWebRequest();

    //    if (www.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError("Error loading file: " + www.error);
    //        yield break;
    //    }

    //    byte[] dataBytes = www.downloadHandler.data;

    //    try
    //    {
    //        // Deserialize the struct.
    //        this.Data = ByteArrayToStructure<TData>(dataBytes);
    //        this.IsLoaded = true;
    //        OnDataLoaded.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        // Handle any exceptions that may occur during deserialization.
    //        Debug.LogError($"Error deserializing data: {e.Message}");
    //    }
    //}

    public async Task<string> Bla()
    {
        string foo = "bar";
        await Task.Delay(10);
        return foo;
    }

    public virtual async Task LoadData(string folderPath)
    {
        //yield return null; // Yield control back to the main thread.

        if (this.IsLoaded)
        {
            // Data is already loaded.
            //yield break;
            return;
        }

        UnityWebRequest www = UnityWebRequest.Get("file://" + folderPath + this.FileName);

        AsyncOperation asyncOp = www.SendWebRequest();

        while (!asyncOp.isDone)
        {
            await Task.Yield(); // Yield control back to the main thread.
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading file: " + www.error);
            return;
            //yield break;
        }

        byte[] dataBytes = www.downloadHandler.data;

        try
        {
            // Deserialize the struct.
            this.Data = ByteArrayToStructure<TData>(dataBytes);
            this.IsLoaded = true;
        }
        catch (Exception e)
        {
            // Handle any exceptions that may occur during deserialization.
            Debug.LogError($"Error deserializing data: {e.Message}");
        }
    }

    // Load data from the specified file.
    //public virtual bool LoadData(string folderPath)
    //{
    //    string filePath = folderPath + this.FileName;
    //    if (this.IsLoaded)
    //    {
    //        // Data is already loaded.
    //        return true;
    //    }

    //    try
    //    {
    //        if (File.Exists(this.FileName))
    //        {
    //            // Read data from the file and load it into the container.
    //            byte[] dataBytes = File.ReadAllBytes(filePath);
    //            this.Data = ByteArrayToStructure<TData>(dataBytes);
    //            this.IsLoaded = true;
    //            return true;
    //        }
    //        else
    //        {
    //            // File not found.
    //            return false;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        // Handle any exceptions that may occur during data loading.
    //        Console.WriteLine($"Error loading data: {e.Message}");
    //        return false;
    //    }
    //}

    public virtual void UnloadData()
    {
        this.Data = null;
        Resources.UnloadUnusedAssets(); // Probably a bad idea if multiple are unloaded
    }

    // Helper method to convert a byte array to a struct.
    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }
    }
}

public interface ILoadableStructContainer
{
    public Task LoadData(string folderPath);
    public void UnloadData();
    public void SaveData(string folderPath);
}