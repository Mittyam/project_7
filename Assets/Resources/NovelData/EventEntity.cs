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

    public string live2DModelID = "";                   // ���f���̃��\�[�X�p�X 
    public float live2DScale = 1.0f;                    // �\���{��
    public string live2DPosition;  // �ʒu�iUI���W�n�j

    public string live2DAnimTrigger;
    public string live2DExpTrigger;

    // �p�����[�^ID���J���}��؂蕶����Ƃ��ĕۑ�
    // ��: "n1,n2,n3"
    public string live2DParamIDs;

    // �p�����[�^�l���J���}��؂蕶����Ƃ��ĕۑ�
    // ��: "0.5,1.0,0.75" 
    public string live2DParamValues;
}
