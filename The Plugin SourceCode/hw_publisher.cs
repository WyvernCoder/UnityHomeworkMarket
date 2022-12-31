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
    const string postURL = "http://127.0.0.1/homework_request.php";       // ������URL
    static string uploadPath = "";
    [SerializeField] public Sprite headerImage_publisher;


    [MenuItem("HomeworkMarket/Publisher")]
    public static void OpenPublisher()
    {
        main_window = GetWindow<hw_publisher>();     // ��������
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

        var title_InputField = new UnityEngine.UIElements.TextField("���⣺");
        title_InputField.value = Path.GetFileNameWithoutExtension(uploadPath);
        title_InputField.SetEnabled(false);
        downView.Add(title_InputField);

        var desc_InputField = new UnityEngine.UIElements.TextField("���ܣ�");
        downView.Add(desc_InputField);

        var auth_InputField = new UnityEngine.UIElements.TextField("���ߣ�");
        auth_InputField.value = System.Environment.UserName;
        auth_InputField.SetEnabled(false);
        downView.Add(auth_InputField);

        var file_button = new UnityEngine.UIElements.Button();
        var file_btn_label = new UnityEngine.UIElements.Label("ѡ���ļ�");
        file_button.hierarchy.Add(file_btn_label);
        file_button.clicked += () =>
        {
            uploadPath = EditorUtility.OpenFilePanelWithFilters("ѡ��Ҫ�ϴ����ļ�", Application.dataPath, new string[] { "Unity Package", "unitypackage" });
            title_InputField.value = Path.GetFileNameWithoutExtension(uploadPath);
            if (uploadPath == "") file_btn_label.text = "ѡ���ļ�";
            else file_btn_label.text = "��ѡ�ļ���" + Path.GetFileName(uploadPath) + " (" + GetFileSize(uploadPath) + " MB )";
        };
        downView.Add(file_button);

        var upd_button = new UnityEngine.UIElements.Button();
        var upd_label = new Label("ȷ���ϴ�");
        upd_button.hierarchy.Add(upd_label);
        upd_button.clicked += () =>
        {
            if (uploadPath == "")
            {
                Debug.LogError("ֻ�����ǲŻ������ɣ�");
                return;
            }

            if (GetFileSize(uploadPath) > 1023f)
            {
                Debug.LogWarning("�ļ������޷��ϴ���");
                return;
            }

            var desc = desc_InputField.text;
            if (desc == "") desc = "�޽��ܡ�";
            UploadFile(uploadPath);
            SetDescribeByFilename(Path.GetFileNameWithoutExtension(uploadPath), "" + desc + "|���ߣ�" + auth_InputField.text + "|�ļ���С��" + GetFileSize(uploadPath) + "MB"+"|Ԥ������ʱ�䣺"+ GetDownloadTime(uploadPath)+"����");
            upd_label.text = "�ϴ����";
        };
        downView.Add(upd_button);

        downView.Add(new Label("ע��һ���ϴ���ֻ�����޸Ľ��ܣ������޸��ļ���"));
        downView.Add(new Label("�ϴ����ƣ��� 20 ���������ϴ������� 1024MB ���ļ��������������Ƶ�����һ�־��ᵼ���ϴ�ʧ�ܡ�"));
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
