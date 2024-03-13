using System;

[Serializable]
public class TargetData
{
    public static TargetData instance = new TargetData();

    public string urlMarker;
    public string urlModel;

    public void SetData(string marker, string model)
    {
        urlMarker = marker;
        urlModel = model;
    }
}
