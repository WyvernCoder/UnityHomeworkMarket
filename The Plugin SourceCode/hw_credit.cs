using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class hw_credit : EditorWindow
{
    static EditorWindow main_window;
    public Sprite thx;


    [MenuItem("HomeworkMarket/Credit")]
    public static void OpenCredit()
    {
        main_window = GetWindow<hw_credit>();     // ´´½¨´°¿Ú
        main_window.titleContent = new GUIContent("Plugin Credit");
        var currentRect = new Rect();
        currentRect.x = Screen.resolutions[0].width / 2f;
        currentRect.y = Screen.resolutions[0].height / 2f;
        currentRect.height = 516;
        currentRect.width = 1006;
        main_window.position = currentRect;
        main_window.Show();
    }

    private void CreateGUI()
    {
        var panel = new UnityEngine.UIElements.Box();
        rootVisualElement.Add(panel);

        var titleImage = new Image();
        titleImage.image = thx.texture;
        titleImage.scaleMode = ScaleMode.ScaleAndCrop;
        panel.Add(titleImage);
    }

}
