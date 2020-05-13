# Getting started with an experiment using virtual reality

This repository contains code to run virtual reality experiments using Unity, under continuous control of and communication with the experimental control software MonkeyLogic2. Communication between these programs, behavioural recording devices, and electrophysiological devices is handled by [Lab Streaming Layer](https://github.com/sccn/labstreaminglayer). 

See the [Wiki page](https://github.com/JMTNeuroLab/MTLab_UnitySource/wiki) for detailed instructions, and a lot of really useful detail on MonkeyLogic, Lab Streaming Layer, Unity, C#, and more. If you have trouble installing or running any of this software, you can also check out the [Wiki Troubleshooting section](https://github.com/JMTNeuroLab/MTLab_UnitySource/wiki/5.-Misc-and-Troubleshooting). 

This project was originally built by [Guillaume Doucet](https://www.github.com/Doug1983/).

## Requirements & Dependencies
* PC running Windows 10
* [git](https://git-scm.com/download/win)
* [Matlab](http://www.mathworks.com) (tested with R2019b)
* [Unity](https://unity3d.com/get-unity/download), one of the tested versions: 2019.3.0b3, 2019.3.0f6. 
* Lab Streaming Layer: 
    * [Binaries v. 1.13.1](https://github.com/sccn/liblsl/releases/tag/1.13.1) 
    * [LSL for Matlab](https://github.com/labstreaminglayer/liblsl-Matlab/releases/tag/1.13.0-b13-matlab2019b) (These files are already included in the MTLab_ML_UnityTask repository)
* [Customized EyeLink SDK](https://github.com/JMTNeuroLab/EyeLink_SDK_for_Unity)

While this is not required, Unity uses the C# language which has very good integration with [Microsoft Visual Studio](https://visualstudio.microsoft.com/). Just make sure you install the **Game development with Unity** package when you install it. This will allow you to have code completion and debugger access. If you already have Visual Studio but want to add the game development package, open Visual Studio, got to: `Tools/Get Tools and Features...` and install the package from there. 


**MonkeyLogic2 resources** <br>  
* [NIMH MonkeyLogic2](https://monkeylogic.nimh.nih.gov/download.html)
* [Customisted task for MonkeyLogic2](https://github.com/JMTNeuroLab/MTLab_ML_UnityTask.git)
	* This task contains a subfolder named `libLSL`. This subfolder contains all of the code for [Lab Streaming Layer version 1.13.0](https://github.com/labstreaminglayer/liblsl-Matlab/releases), which is compatible with Matlab 2019b. 
	
**Unity resources** <br>
* Unity-MonkeyLogic project source code (this repository)
* [Customized Unity task](https://github.com/JMTNeuroLab/MTLab_ML_UnityTask.git), with built-in controllers for communication with MonkeyLogic2, eye tracking, and other experimental equipment.


## Installation
Refer to the [wiki page](https://github.com/JMTNeuroLab/MTLab_UnitySource/wiki/1.-Installation) for complete details. 


## Getting started

To test that your installation is working, you can try to run a copy of the associative memory task used in:
* [Gulli <em>et al.</em> 2020, <em>Nature Neuroscience</em>](https://www.nature.com/articles/s41593-019-0548-3).
* [Doucet <em>et al.</em> 2019, <em>Hippocampus</em>](https://onlinelibrary.wiley.com/doi/10.1002/hipo.23140).
* [Corrigan <em>et al.</em> 2017, <em>Journal of Vision</em>](http://jov.arvojournals.org/article.aspx?articleid=2659575).

This task is built in a virtual environment called the X-Maze. 
> Note, this is not an exact copy of the task used in these articles. The published version relies on a now-deprecated version of Unreal Engine. The task was re-created exactly using the Unity engine. 

### Download the X-Maze task
* In a terminal window, navigate to the Tasks folder of your local MonkeyLogic-Unity Source repo
  `$ cd <yourLocalSourceRepoPath>/Assets/Tasks/`
* Clone the example task repo called `Temp` to this folder 
  `$ git clone https://github.com/Doug1983/MTLab_UnityExampleTask.git`
* Move all of the files now in `<yourLocalSourceRepoPath>/Assets/Tasks/MTLab_UnityExampleTask/` to `<yourLocalSourceRepoPath>/Assets/Tasks/`
* In Unity Hub, add your local repository as a new project. 
* Open the project using Unity v2019.3.0**
     * If you're having trouble opening Unity, check out the [Wiki section on Troubleshooting](https://github.com/Doug1983/MTLab_UnitySource/wiki/5.-Misc-and-Troubleshooting).

> Note, you may get a pop-up asking: 
>> 
>> Do you want to upgrade the project to use Asset Database Version 2? <br>
>> Note: Version 1 is deprecated from 2019.3. If you upgrade to version 2, the project will be re-imported. <br> 
>> You can always change back to version 1 in the project settings. <br>
>> 
> Select "No". 

### Running the task
#### Unity
 * Launch Unity-MonkeyLogic2 project
 * Load the X-Maze End scene
 * Ensure that there are no errors in the console
 * Hit "Play" (ctrl+P)

#### MonkeyLogic2 
* In Matlab, run: <br>
  `addpath(genpath(C:\MonkeyLogic\))`<br>
  `>> monkeylogic()`
  > Note, on first call, you may need to approve some permissions requests. 
* In the MonkeyLogic2 dialog box, load `C:\MonkeyLogic\task\UnityVR.txt`
* Hit &#x2622;**RUN**
