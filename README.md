# UnityInvokeAI
simple Unity editor UI for calling InvokeAI (stable diffusion) web interface (locally)

## Important Note
- This currently supports only invokeAI version v1.14.1
- Unity version is 2019.4.25f1 (but should work in newer releases too)

#### Installation : InvokeAI v1.14.1)
- install anaconda https://docs.anaconda.com/anaconda/install/windows/
- NOTE can also follow old install instructions at https://github.com/invoke-ai/InvokeAI/tree/9b28c65e4b0526956fd90ad86e2118536e4eaa9b
- Download https://github.com/invoke-ai/InvokeAI/releases/tag/release-1.14.1 (source code zip)
- unzip it
- run Anaconda3 Prompt (from windows start menu)
- Go to your invokeAI download folder: ```cd C:\Users\USERNAME\Downloads\InvokeAI-release-1.14.1\InvokeAI-release-1.14.1\``` (main.py is in this folder)
- ```conda activate ldm```
- download models from: https://huggingface.co/CompVis/stable-diffusion-v-1-4-original (you'll need to signup first and accept license agreement)
- download **"sd-v1-4.ckpt"**
- create folder(s): **\InvokeAI-release-1.14.1\models\ldm\stable-diffusion-v1**
- place **"sd-v1-4.ckpt"** file there, rename it as **"model.ckpt"**
- From anaconda3 prompt, run: ```python scripts\preload_models.py```
- From anaconda3 prompt, run: ```python scripts\dream.py --web```
- test in browser: http://127.0.0.1:9090/

### Setup Unity plugin
- open window Tools/StableUI
- in the settings view, paste in your installation folder (example): **"C:\Users\USERNAME\Downloads\InvokeAI-release-1.14.1\InvokeAI-release-1.14.1\"**
- click save
- test image creation by adding some prompt and press "Generate"

### Info
- See https://github.com/unitycoder/UnityInvokeAI/wiki

### Images
![image](https://user-images.githubusercontent.com/5438317/200028080-b592525d-5db1-4bc3-acdd-cb40de51a187.png)

### Website
- https://unitycoder.com/blog/2022/11/04/unity-stable-diffusion-plugin/
