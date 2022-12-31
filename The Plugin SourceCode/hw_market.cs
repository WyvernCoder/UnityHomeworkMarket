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

public class hw_market : EditorWindow
{
    static EditorWindow main_window;
    static List<string> homework_list = new List<string>();

    static string savePath;
    const string postURL = "http://127.0.0.1/homework_request.php";       // 服务器URL
    [SerializeField] public Sprite headerImage_market;



    [MenuItem("HomeworkMarket/Open")]
    public static void OpenMarket()
    {
        main_window = GetWindow<hw_market>();     // 创建窗口
        main_window.ShowNotification(new GUIContent("作业问题本插件不负责。"));
        main_window.titleContent = new GUIContent("Homework Market");
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
        savePath = Path.Combine(Application.dataPath, "Editor", "HomeworkMarket", "VaultCache");    // 保存位置
        UpdateHomeworkList();
        
        var upsideSplitView = new UnityEngine.UIElements.TwoPaneSplitView(0, 135, TwoPaneSplitViewOrientation.Vertical);

        var titleImage = new Image();
        if (headerImage_market != null)
        {
            titleImage.image = headerImage_market.texture;
            titleImage.scaleMode = ScaleMode.ScaleAndCrop;
            upsideSplitView.Add(titleImage);
        }

        var splitView = new UnityEngine.UIElements.TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        upsideSplitView.Add(splitView);

        var leftView = new UnityEngine.UIElements.ListView();
        splitView.Add(leftView);
        var rightView = new UnityEngine.UIElements.ListView();
        splitView.Add(rightView);

        rootVisualElement.Add(upsideSplitView);

        leftView.makeItem = () => { return new Label(); };
        leftView.bindItem = (item, index) => { (item as Label).text = Path.GetFileNameWithoutExtension(homework_list[index]); };
        leftView.itemsSource = homework_list;

        leftView.onSelectionChange += selectedItem =>
        {
            rightView.hierarchy.Clear();
            string describe = "作业介绍：";
            describe += GetDescribeByFilename((string)selectedItem.First());
            describe = describe.Replace('|', '\n');
            rightView.hierarchy.Add(new Label(describe));

            var button = new Button();
            if (FileExist((string)selectedItem.First()))
            {
                button.hierarchy.Add(new Label("删除"));
                button.clicked += () =>
                {
                    FileDeleteByName(selectedItem.First() as string);
                    rightView.hierarchy.Clear();
                };

                var importButton = new Button();
                importButton.hierarchy.Add(new Label("安装"));
                importButton.clicked += () =>
                {
                    ImportPackageByFileName(selectedItem.First() as string);
                };
                rightView.hierarchy.Add(importButton);
            }
            else
            {
                button.hierarchy.Add(new Label("下载"));
                button.clicked += () =>
                {
                    DownloadByFileName(selectedItem.First() as string);
                    rightView.hierarchy.Clear();
                };
            }

            rightView.hierarchy.Add(button);
        };
    }

    static List<string> UpdateHomeworkList()
    {

        var form = new WWWForm();
        form.AddField("mode", "query");
        form.AddField("platform", "unityeditor");
        form.AddField("filename", "NULL");

        var www = UnityWebRequest.Post(postURL, form);
        www.SendWebRequest();
        while (www.isDone == false) { }

        homework_list = new List<string>(www.downloadHandler.text.Split('|'));
        homework_list.Remove(".");
        homework_list.Remove("..");
        homework_list.Remove("");

        www.Dispose();
        return homework_list;
    }

    static string GetDescribeByFilename(string FileName)
    {
        var form = new WWWForm();
        form.AddField("mode", "getDescribe");
        form.AddField("platform", "unityeditor");
        form.AddField("filename", FileName);

        var www = UnityWebRequest.Post(postURL, form);
        www.SendWebRequest();
        while (www.isDone == false) { }
        var result = www.downloadHandler.text;
        www.Dispose();
        return result;
    }

    static string GetDownloadURLByFilename(string FileName)
    {
        var form = new WWWForm();
        form.AddField("mode", "download");
        form.AddField("platform", "unityeditor");
        form.AddField("filename", FileName);

        var www = UnityWebRequest.Post(postURL, form);
        www.SendWebRequest();
        while (www.isDone == false) { }
        var result = www.downloadHandler.text;
        www.Dispose();
        return result.Replace('\\','/');
    }

    static bool FileExist(string FileName)
    {
        return File.Exists(Path.Combine(savePath, FileName));
    }

    static void FileDeleteByName(string FileName)
    {
        if(FileExist(FileName)) File.Delete(Path.Combine(savePath, FileName));
        FileName = FileName + ".meta";
        if(FileExist(FileName)) File.Delete(Path.Combine(savePath, FileName));
        Debug.Log(FileName + "已删除。");
    }

    static void ImportPackageByFileName(string FileName)
    {
        if(File.Exists(Path.Combine(savePath, FileName)))
            AssetDatabase.ImportPackage(Path.Combine(savePath, FileName), true);
    }

    static void DownloadByFileName(string FileName)
    {
        var www = UnityWebRequest.Get(GetDownloadURLByFilename(FileName));
        www.SendWebRequest();
        while (www.isDone == false) { }

        byte[] results = www.downloadHandler.data;
        if(Directory.Exists(savePath) == false) Directory.CreateDirectory(savePath);
        var fileInfo = new FileInfo(Path.Combine(savePath, FileName));
        var fs = fileInfo.Create();
        fs.Write(results, 0, results.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        www.Dispose();
        Debug.Log(FileName + "已下载完成。");
    }

    static void UploadFile(string FullPath)
    {
        var form = new WWWForm();
        form.AddField("mode", "upload");
        form.AddField("platform", "unityeditor");
        form.AddField("filename", Path.GetFileName(FullPath));
        form.AddBinaryData("file", File.ReadAllBytes(FullPath), Path.GetFileName(FullPath), "application");

        var www = UnityWebRequest.Post(postURL, form);
        www.SendWebRequest();
        while (www.isDone == false) { }

        if(www.downloadHandler.text != "") Debug.LogError(www.downloadHandler.text);
        Debug.Log(Path.GetFileName(FullPath) + "上传成功。");
        www.Dispose();

    }

}
