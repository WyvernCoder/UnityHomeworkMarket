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

public class hw_publisher : EditorWindow
{
    static EditorWindow main_window;
    const string postURL = "http://127.0.0.1/homework_request.php";       // 服务器URL
    static string uploadPath = "";
    [SerializeField] public Sprite headerImage_publisher;


    [MenuItem("HomeworkMarket/Publisher")]
    public static void OpenPublisher()
    {
        main_window = GetWindow<hw_publisher>();     // 创建窗口
        //main_window.ShowNotification(new GUIContent(""));
        main_window.titleContent = new GUIContent("Homework Publisher");
        var currentRect = new Rect();
        currentRect.x = Screen.resolutions[0].width / 2f;
        currentRect.y = Screen.resolutions[0].height / 2f;
        currentRect.height = 500;
        currentRect.width = 700;
        main_window.position = currentRect;
        main_window.Show();
    }

    private void CreateGUI()
    {
        var upsideSplitView = new UnityEngine.UIElements.TwoPaneSplitView(0, 135, TwoPaneSplitViewOrientation.Vertical);
        rootVisualElement.Add(upsideSplitView);

        var titleImage = new Image();
        if (headerImage_publisher != null)
        {
            titleImage.image = headerImage_publisher.texture;
            titleImage.scaleMode = ScaleMode.ScaleAndCrop;
            upsideSplitView.Add(titleImage);
        }

        var downView = new Box();
        upsideSplitView.Add(downView);

        var title_InputField = new UnityEngine.UIElements.TextField("标题：");
        title_InputField.value = Path.GetFileNameWithoutExtension(uploadPath);
        title_InputField.SetEnabled(false);
        downView.Add(title_InputField);

        var desc_InputField = new UnityEngine.UIElements.TextField("介绍：");
        downView.Add(desc_InputField);

        var auth_InputField = new UnityEngine.UIElements.TextField("作者：");
        auth_InputField.value = System.Environment.UserName;
        auth_InputField.SetEnabled(false);
        downView.Add(auth_InputField);

        var file_button = new UnityEngine.UIElements.Button();
        var file_btn_label = new UnityEngine.UIElements.Label("选择文件");
        file_button.hierarchy.Add(file_btn_label);
        file_button.clicked += () =>
        {
            uploadPath = EditorUtility.OpenFilePanelWithFilters("选择要上传的文件", Application.dataPath, new string[] { "Unity Package", "unitypackage" });
            title_InputField.value = Path.GetFileNameWithoutExtension(uploadPath);
            if (uploadPath == "") file_btn_label.text = "选择文件";
            else file_btn_label.text = "已选文件：" + Path.GetFileName(uploadPath) + " (" + GetFileSize(uploadPath) + " MB )";
        };
        downView.Add(file_button);

        var upd_button = new UnityEngine.UIElements.Button();
        var upd_label = new Label("确认上传");
        upd_button.hierarchy.Add(upd_label);
        upd_button.clicked += () =>
        {
            if (uploadPath == "")
            {
                Debug.LogError("只有弱智才会这样干！");
                return;
            }

            if (GetFileSize(uploadPath) > 1023f)
            {
                Debug.LogWarning("文件过大，无法上传。");
                return;
            }

            var desc = desc_InputField.text;
            if (desc == "") desc = "无介绍。";
            UploadFile(uploadPath);
            SetDescribeByFilename(Path.GetFileNameWithoutExtension(uploadPath), "" + desc + "|作者：" + auth_InputField.text + "|文件大小：" + GetFileSize(uploadPath) + "MB"+"|预估下载时间："+ GetDownloadTime(uploadPath)+"分钟");
            upd_label.text = "上传完成";
        };
        downView.Add(upd_button);

        downView.Add(new Label("注：一旦上传，只可以修改介绍，不能修改文件。"));
        downView.Add(new Label("上传限制：在 20 分钟以内上传不大于 1024MB 的文件，超过两个限制的任意一种均会导致上传失败。"));
    }
    static void SetDescribeByFilename(string FileName, string Describe)
    {
        var form = new WWWForm();
        form.AddField("mode", "setDescribe");
        form.AddField("platform", "unityeditor");
        form.AddField("filename", FileName);
        form.AddField("describe", Describe);

        var www = UnityWebRequest.Post(postURL, form);
        www.SendWebRequest();
        while (www.isDone == false) { }
        www.Dispose();
    }

    static void UploadFile(string FullPath)
    {
        var form = new WWWForm();
        form.AddField("mode", "upload");
        form.AddField("platform", "unityeditor");
        form.AddField("filename", Path.GetFileName(FullPath));

        var fs = new FileStream(FullPath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        form.AddBinaryData("file", buffer, Path.GetFileName(FullPath), "application");
        fs.Close();
        fs.Dispose();

        try
        {
            var www = UnityWebRequest.Post(postURL, form);
            www.SendWebRequest();
            while (www.isDone == false) { }
            if (www.downloadHandler.text != "") Debug.LogError(www.downloadHandler.text);
            www.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    static float GetFileSize(string FullPath)
    {   // mb
        var fi = new FileInfo(FullPath);
        return fi.Length / 1000000f;
    }
    
    static float GetDownloadTime(string FullPath)
    {   // seconds
        return GetFileSize(FullPath) / 10.24f;
    }

}
