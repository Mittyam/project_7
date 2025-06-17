using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventEntity
{
    public int sectionID;
    public string text = "";
    public int voiceIndex = -1;
    public int seIndex = -1;
    public int bgmIndex = -1;
    public string imagePath;
    public string isImageFade = "";

    public string live2DModelID = "";                   // モデルのリソースパス 
    public float live2DScale = 1.0f;                    // 表示倍率
    public string live2DPosition;  // 位置（UI座標系）

    public string live2DAnimTrigger;
    public string live2DExpTrigger;

    // パラメータIDをカンマ区切り文字列として保存
    // 例: "n1,n2,n3"
    public string live2DParamIDs;

    // パラメータ値をカンマ区切り文字列として保存
    // 例: "0.5,1.0,0.75" 
    public string live2DParamValues;
}
