// Landscape Builder. Copyright (c) 2016-2022 SCSM Pty Ltd. All rights reserved.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LandscapeBuilder
{
    /// <summary>
    /// General purpose utiliites used in Landscape Builder
    /// </summary>
    public class LBUtils
    {
        #region Json Methods

        /// <summary>
        /// Save a list to Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson<T>(List<T> list)
        {
            ListWrapper<T> listWrapper = new ListWrapper<T>();
            listWrapper.itemList = list;
            return JsonUtility.ToJson(listWrapper);
        }

        /// <summary>
        /// Convert json into a List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static List<T> FromJson<T>(string jsonText)
        {
            ListWrapper<T> listWrapper = JsonUtility.FromJson<ListWrapper<T>>(jsonText);
            return listWrapper.itemList;
        }

        #endregion

    }

    #region List Wrapper Class
    /// <summary>
    /// Used with JsonUtility to convert a list to/from json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ListWrapper<T>
    {
        public List<T> itemList;
    }
    #endregion
}