<?php
	header("content-type:text/html;charset=utf-8");

	$dir = 'C:\\wwwroot\\152.136.93.19\\unity_homework';
	$netdir = 'http:\\\\47.94.42.94:80\\unity_homework';
	
	if(is_dir($dir) == false) mkdir($dir);
	
	if($_POST["mode"] == "query")
	{
		if(is_dir($dir))
		{
			$arr1 = scandir($dir);
		}
		foreach($arr1 as $item) 
		{
			if(pathinfo($item, PATHINFO_EXTENSION) != "unitypackage") continue;
			echo $item ."|";
		}
	}
	
	if($_POST["mode"] == "getDescribe")
	{
		$path = $dir."\\".$_POST["filename"];
		$path_without_extension = pathinfo($path, PATHINFO_DIRNAME)."\\".pathinfo($path, PATHINFO_FILENAME);
		$path_describe = $path_without_extension.".txt";
		
		if(is_file($path_describe))
		{
			$content = file_get_contents($path_describe);
			echo $content;
		}
	}
	
	if($_POST["mode"] == "setDescribe")
	{
		$path = $dir."\\".$_POST["filename"];
		$path_without_extension = pathinfo($path, PATHINFO_DIRNAME)."\\".pathinfo($path, PATHINFO_FILENAME);
		$path_describe = $path_without_extension.".txt";
		
		if(file_exists($path_describe)) unlink($path_describe);
		
		file_put_contents($path_describe, $_POST["describe"]);
		echo $path_describe;
	}
	
	if($_POST["mode"] == "download")
	{
		$path = $netdir."\\".$_POST["filename"];
		echo $path;
	}
	
	if($_POST["mode"] == "upload")
	{
		$fixedName = iconv_mime_decode($_FILES["file"]["name"]);
		$extension = pathinfo($fixedName, PATHINFO_EXTENSION);
		$fullpath = $dir."/".$fixedName;
		
		if($extension != "unitypackage") return;
		
		// if(file_exists($fullpath)) unlink($fullpath);
		if(file_exists($fullpath)) return;	// 禁止替换任何文件
		
		move_uploaded_file($_FILES["file"]["tmp_name"], $fullpath);
	}

?>