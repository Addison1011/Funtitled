using UnityEngine;
using System.IO;
using System;

public class FbxBlobConverter : MonoBehaviour
{
    public string fbxFilePath = null;

    // Converts FBX file to byte[] (BLOB)
    public static byte[] ConvertFbxFileToBlob(string fbxFilePath)
    {
        if (!File.Exists(fbxFilePath))
        {
            System.Console.WriteLine($"Error: FBX file not found at {fbxFilePath}");
            return null;
        }

        try
        {
            // Read all bytes from the FBX file into a byte array
            byte[] fbxData = File.ReadAllBytes(fbxFilePath);
            return fbxData;
        }
        catch (IOException ex)
        {
            System.Console.WriteLine($"Error reading FBX file: {ex.Message}");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Console.WriteLine($"Error: Access denied to FBX file: {ex.Message}");
            return null;
        }
    }

    // Converts byte[] (BLOB) back to FBX file
    public static bool ConvertBlobToFbx(byte[] blobData, string outputFilePath)
    {
        if (blobData == null || blobData.Length == 0)
        {
            Debug.LogError("Error: Empty or null BLOB data.");
            return false;
        }

        try
        {
            File.WriteAllBytes(outputFilePath, blobData);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing FBX file: {ex.Message}");
            return false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        byte[] fbxBlob = ConvertFbxFileToBlob(fbxFilePath);

        if (fbxBlob != null)
        {
            Debug.Log($"Successfully converted FBX to BLOB. Size: {fbxBlob.Length} bytes.");

            ConvertBlobToFbx(fbxBlob, fbxFilePath + "_copy.fbx");
        }
        else
        {
            Debug.Log("FBX to BLOB conversion failed.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
