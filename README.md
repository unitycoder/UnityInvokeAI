# UnityInvokeAI
simple Unity editor UI for calling InvokeAI (stable diffusion) web interface (locally)

## Important Note
- This currently supports only invokeAI version v1.14.1

#### Installation : InvokeAI v1.14.1)
- install anaconda https://docs.anaconda.com/anaconda/install/windows/
- NOTE can also follow old install instructions at https://github.com/invoke-ai/InvokeAI/tree/9b28c65e4b0526956fd90ad86e2118536e4eaa9b
- Download https://github.com/invoke-ai/InvokeAI/releases/tag/release-1.14.1 (source code zip)
- unzip it
- run anaconda3 prompt (from windows start menu)
- Go to your invokeAI download folder: cd C:\Users\USERNAME\Downloads\InvokeAI-release-1.14.1\InvokeAI-release-1.14.1\ (main.py is in this folder)
- conda activate ldm 
- download models: https://huggingface.co/CompVis/stable-diffusion-v-1-4-original (you'll need to signup first and accept license agreement)
- download "sd-v1-4.ckpt"
- create folder(s) \InvokeAI-release-1.14.1\models\ldm\stable-diffusion-v1
- place "sd-v1-4.ckpt" file there, rename it as "model.ckpt"
- python scripts\preload_models.py
- python scripts\dream.py --web
- test in browser: http://127.0.0.1:9090/

### Setup Unity plugin
- open window Tools/StableUI
- in the settings view, paste in your installation folder (example): C:\Users\USERNAME\Downloads\InvokeAI-release-1.14.1\InvokeAI-release-1.14.1\
- click save
- test image creation by adding some prompt and press "Generate"
