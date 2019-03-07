using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonPost
{
    public string name;
    public float width;
    public string image;
    public bool active_flag;
    public string application_metadata;

    public JsonPost(string nameString, float widthNum, string imageString, string metaString)
    {
        name = nameString;
        width = widthNum;
        image = imageString;
        active_flag = true;
        application_metadata = metaString;
    }

    public JsonPost(string nameString, float widthNum, string imageString, bool activeBool, string metaString)
    {
        name = nameString;
        width = widthNum;
        image = imageString;
        active_flag = activeBool;
        application_metadata = metaString;
    }
}
