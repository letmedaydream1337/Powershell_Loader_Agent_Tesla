# Powershell_Loader_Agent_Tesla
It's the example code I made, when research the malware
Visit for more information  

[Research Note — Agent Tesla (1)](https://medium.com/@letmedaydream.sparrow/research-note-agent-tesla-1-afe9eb7bece5)  

[Research Note — Agent Tesla (2)](https://medium.com/@letmedaydream.sparrow/research-note-agent-tesla-2-b3eca46def80)  

## How to use

### Compile
Just Create a Visual project and put code in
![image](https://github.com/letmedaydream1337/Powershell_Loader_Agent_Tesla/blob/main/VS.png)

### Append dotNet Module to JPG
cmd
```
.\AppendModuleToJPG.exe "<dll_path>" "<image_path>" "<malicious_image_output_path>"
```
![image](https://github.com/letmedaydream1337/Powershell_Loader_Agent_Tesla/blob/main/AppendModuleToJPG.png)

### powershell loader
cmd
```
.\dotNetLoader_local.ps1 "<image_path>" "<namespace>.<className>" "<method>" "<args>"
```
![image](https://github.com/letmedaydream1337/Powershell_Loader_Agent_Tesla/blob/main/executeModule.png)
