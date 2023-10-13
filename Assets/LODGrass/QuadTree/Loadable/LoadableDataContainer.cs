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
    private TData? data;
    public TData? Data 
    {
        get 
        {
            return data;
        }
        set
        {
            data = value;
        }
    }

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
    /// Saves any data in this instanced container to disk
    /// </summary>
    /// <param name="folderPath"></param>
    public virtual async Task SaveData(string folderPath)
    {
        // Serialize the struct to a byte array.
        byte[] dataBytes = StructureToByteArray(this.Data.Value);

        // Create the file path.
        string filePath = Path.Combine(folderPath, this.FileName);

        // Write the byte array to the file.
        try
        {
            await File.WriteAllBytesAsync(filePath, dataBytes);
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving data: " + e.Message);
        }
    }

    // Helper method to convert a struct to a byte array.
    private static byte[] StructureToByteArray(TData data)
    {
        int size = Marshal.SizeOf(data);
        byte[] byteArray = new byte[size];

        GCHandle handle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(data, handle.AddrOfPinnedObject(), false);
        }
        finally
        {
            handle.Free();
        }

        return byteArray;
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

    public virtual void UnloadData()
    {
        this.Data = null;
        this.IsLoaded = false;
        Resources.UnloadUnusedAssets(); // Probably a bad idea if multiple are unloaded
    }
}

public interface ILoadableStructContainer
{
    public Task LoadData(string folderPath);
    public void UnloadData();
    public Task SaveData(string folderPath);
}