using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowLoad 
{
    public static ArrowLoad Instance()
    {
        return instance;
    }
    private static ArrowLoad instance;
    public ArrowLoad()
    {
        instance=this;
    }
    public class ArrowDara
    {
        public GameObject go;
        public bool beUsing;
    }
    private List<ArrowDara> arrowContainer;
    /// <summary>
    /// 生成箭矢
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    public void creatArrow(Object prefab,int count)
    {
        arrowContainer = new List<ArrowDara>();
        for(int i=0;i<count;i++)
        {
            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            go.SetActive(false); 
            ArrowDara data = new ArrowDara();
            data.beUsing = false;
            data.go = go;
            arrowContainer.Add(data);
        }
    }
    /// <summary>
    /// 使用箭矢
    /// </summary>
    /// <returns></returns>
    public GameObject LoadArrow()
    {
        int count = arrowContainer.Count;
        GameObject rgo = null;
        for(int i=0;i<count;i++)
        {
            if(arrowContainer[i].go.activeSelf==false)
            {
                rgo = arrowContainer[i].go;
                break;
            }
        }
        return rgo;
    }
}
