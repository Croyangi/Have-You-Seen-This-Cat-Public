using System.IO;
using UnityEngine;

public static class TextFileReader
{
    public static string GetRandomLine(string path, string textFileName)
    {
        string myPath = Path.Combine(path, textFileName);

        if (!File.Exists(myPath))
        {
            Debug.LogError("File not found at: " + myPath);
            return string.Empty;
        }

        string[] lines = File.ReadAllLines(myPath);

        if (lines.Length == 0) return string.Empty;

        return lines[Random.Range(0, lines.Length)];
    }
}