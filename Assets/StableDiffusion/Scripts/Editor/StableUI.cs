// InvokeAI (stable diffusion) unity editor ui
// https://github.com/unitycoder/UnityInvokeAI

using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary
{
    public class StableUI : EditorWindow
    {
        string url = "http://127.0.0.1:9090/";
        string installationFolder = @"";
        string importFolder = "Assets/Textures";

        string prefix = "StableUI_";
        bool isSettingsOpen = false;

        string lastImagePath = null;
        Texture2D resultsTexture;
        string[] sizeOptions = { "64", "128", "256", "512", "1024", "2048" };
        int[] sizeOptionsInt = { 64, 128, 256, 512, 1024, 2048 };

        string[] samplerOptions = { "ddim", "plms", "k_lms", "k_dpm_2", "k_dpm_2_a", "k_euler", "k_euler_a", "k_heun" };

        string prompt = "empty";
        int iterations = 1;
        int steps = 20;
        float cfg_scale = 7.5f;
        int samplerIndex = 2;
        int sizeIndex = 3;
        string seed = "-1";
        bool seamless = false;
        float variation_amount = 0;
        string with_variations = "";
        Texture2D initimg = null;
        float strength = 0.75f; // Img2Img
        bool fit = true;
        float gfpgan_strength = 0.8f;
        string upscale_level = "";
        float upscale_strength = 0.75f;
        string initimg_name = "";

        // www
        HttpWebResponse response;
        StreamReader reader;
        string fullResponce = "";
        long lastSeed = -1;

        // UI
        float progressValue = 0;
        int progressMax = 0;
        int panelWidth = 333;


        [MenuItem("Tools/StableUI")]
        public static void Init()
        {
            var window = GetWindow(typeof(StableUI));
            window.titleContent = new GUIContent("StableUI");
            window.minSize = new Vector2(1000, 512);
        }

        void OnGUI()
        {
            if (isSettingsOpen==true || installationFolder=="")
            {
                if (isSettingsOpen == false) isSettingsOpen = true;
                
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                
                url = EditorGUILayout.TextField("URL", url);
                installationFolder = EditorGUILayout.TextField("Installation folder", installationFolder);
                importFolder = EditorGUILayout.TextField("Import folder", importFolder);
                
                EditorGUILayout.Space(20);
                if (GUILayout.Button(new GUIContent("Save", "Save settings"), GUILayout.Width(250), GUILayout.Height(24)))
                {
                    isSettingsOpen = false;
                }
                return;
            }

            // main panel
            EditorGUILayout.BeginHorizontal("box", GUILayout.Width(EditorGUIUtility.currentViewWidth - 10), GUILayout.Height(position.height - 10));

            // settings panel
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(panelWidth));

            EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
            EditorStyles.textArea.wordWrap = true;

            prompt = EditorGUILayout.TextArea(prompt, EditorStyles.textArea, GUILayout.Height(80));

            if (GUILayout.Button("Generate", GUILayout.Height(44))) Generate();

            // progress bar
            Rect rect = EditorGUILayout.BeginVertical();
            GUILayout.Button("dummy", GUILayout.Height(1));
            EditorGUILayout.EndVertical();
            //This box will cover all controls between the former BeginVertical() & EndVertical()
            EditorGUI.DrawRect(rect, Color.black);
            rect.width = rect.width * (progressValue / (float)(progressMax + 1));
            EditorGUI.DrawRect(rect, Color.green);

            EditorGUILayout.Space(10);

            // --------------- SETTINGS ----------------
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            steps = EditorGUILayout.IntField("Steps", steps);
            sizeIndex = EditorGUILayout.Popup("Size", sizeIndex, sizeOptions);
            samplerIndex = EditorGUILayout.Popup("Sampler", samplerIndex, samplerOptions);

            EditorGUILayout.BeginHorizontal();
            seed = EditorGUILayout.TextField("Seed", seed);
            if (GUILayout.Button("x", GUILayout.Width(32))) seed = "-1";
            EditorGUILayout.EndHorizontal();

            seamless = EditorGUILayout.Toggle("Seamless", seamless);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Init image");
            initimg = (Texture2D)EditorGUILayout.ObjectField(initimg, typeof(Texture2D), true, GUILayout.Width(64), GUILayout.Height(64));
            // unity 2020.1+ can read camera icon with EditorGUIUtility.IconContent("Camera Gizmo") // https://github.com/halak/unity-editor-icons
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(new GUIContent("Clr", "Clear image"), GUILayout.Width(38))) initimg = null;
            if (GUILayout.Button(new GUIContent("Grab", "Screenshot from MainCamera"), GUILayout.Width(38))) TakeScreenshot();
            if (GUILayout.Button(new GUIContent("<<<", "Use latest generated image"), GUILayout.Width(38))) TakeResult();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            //strength = EditorGUILayout.FloatField("Img2Img Strength", strength);
            // slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Img2Img Strength");
            strength = EditorGUILayout.Slider(strength, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // --------------- TOOLS ------------------

            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Import", GUILayout.Height(24))) ImportTexture();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Last seed: " + lastSeed);
            if (GUILayout.Button("Copy last seed")) seed = lastSeed.ToString();
            EditorGUILayout.EndHorizontal();

            // settings button
            if (GUILayout.Button(new GUIContent("@", "Open settings"), GUILayout.Width(24), GUILayout.Height(24)))
            {
                // clear selection, otherwise URL field is bugged
                GUI.FocusControl(null);
                
                // open new overlay panel
                isSettingsOpen = true;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            // ------------------ RESULTS ------------------
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperLeft;
            var tpos = new Rect(panelWidth + 10, 0, EditorGUIUtility.currentViewWidth - panelWidth - 14, 500);
            if (resultsTexture != null) GUI.DrawTexture(tpos, resultsTexture, ScaleMode.ScaleToFit);

            EditorGUILayout.EndHorizontal();
        }


        private void TakeResult()
        {
            if (lastImagePath != null)
            {
                initimg = new Texture2D(2, 2);
                initimg.LoadImage(File.ReadAllBytes(lastImagePath));
            }
        }

        // take screenshot from main camera
        void TakeScreenshot()
        {
            var camera = Camera.main;

            // clamp max size to 1024x1024, TODO test if any sizes work, or needs POT or div by 2?
            var w = camera.pixelWidth;
            var h = camera.pixelHeight;
            //w = Mathf.Min(w, 1024);
            //h = Mathf.Min(h, 1024);

            var cameraTexture = new RenderTexture(w, h, 24);
            camera.targetTexture = cameraTexture;
            camera.Render();
            RenderTexture.active = cameraTexture;
            var gameViewTexture = new Texture2D(cameraTexture.width, cameraTexture.height);
            gameViewTexture.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
            gameViewTexture.Apply();
            initimg = gameViewTexture;
            camera.targetTexture = null;
        }

        void Generate()
        {
            Debug.Log("Generate..");
            SavePrefs();

            lastImagePath = null;

            string fitStr = fit ? "on" : "off";
            string seamlessStr = seamless ? "'seamless': 'on'," : "";
            int width = sizeOptionsInt[sizeIndex];
            string sampler_name = samplerOptions[samplerIndex];
            int height = width;
            string initImgObj = "";
            // convert texture2d into base64 jpeg
            if (initimg == null)
            {
                // send null
                Object temp = null;
                initImgObj = SimpleJsonConverter.Serialize(temp);
            }
            else
            {
                // if image is not readable, need to generate copy
                if (initimg.isReadable == false)
                {
                    initimg = DuplicateTexture(initimg);
                }

                byte[] bytes = initimg.EncodeToJPG();
                string base64 = System.Convert.ToBase64String(bytes);
                initImgObj = "\"data:image/jpeg;base64," + base64 + "\"";
            }

            // clamp img2img strength
            var strengthClamped = Mathf.Clamp(strength, 0, 0.9999999f);

            string postData = $"{{'prompt':'{prompt}','iterations':'{iterations}','steps':'{steps}','cfg_scale':'{cfg_scale}','sampler_name':'{sampler_name}','width':'{width}','height':'{height}'," + seamlessStr + $"'seed':'{seed}','variation_amount':'{variation_amount}','with_variations':'{with_variations}','initimg':{initImgObj},'strength':'{strengthClamped}','fit':'{fitStr}','gfpgan_strength':'{gfpgan_strength}','upscale_level':'{upscale_level}','upscale_strength':'{upscale_strength}','initimg_name':'{initimg_name}'}}";
            postData = postData.Replace("'", "\"");
            Debug.Log(postData);

            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.ASCII.GetBytes(postData);
            request.KeepAlive = true;
            request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            // reset old values
            fullResponce = "";
            progressValue = 0;
            progressMax = steps;

            EditorApplication.update += EditorUpdate;

            response = (HttpWebResponse)request.GetResponse();
            reader = new StreamReader(response.GetResponseStream());
        }

        void EditorUpdate()
        {
            
            var responseString = reader.ReadLine();
            if (string.IsNullOrEmpty(responseString) == false)
            {
                //   Debug.Log("responseString:" + responseString);
                fullResponce += responseString + "\n";
                // FIXME doesnt work without this?
                Debug.Log("progress: " + ((progressValue++) + "/" + steps));
                Repaint();
            }

            if (reader.EndOfStream)
            {
                EditorApplication.update -= EditorUpdate;
                reader.Close();
                response.Close();

                Debug.Log("fullResponce:" + fullResponce);

                var jsonRows = fullResponce.Split('\n');
                // NOTE last row is empty
                var lastRow = jsonRows[jsonRows.Length - 2];
                var deserialize = SimpleJsonConverter.Deserialize<Root>(lastRow);
                //Debug.Log("url: " + deserialize.url);

                lastSeed = deserialize.seed;

                // load local image
                Debug.Log("Load image..");
                lastImagePath = Path.Combine(installationFolder, deserialize.url);
                //Debug.Log("imgPath: " + imgPath);

                // load texture from file
                resultsTexture = new Texture2D(2, 2);
                ImageConversion.LoadImage(resultsTexture, File.ReadAllBytes(lastImagePath));
            }
        }

        void OnEnable()
        {
            LoadPrefs();
            // just in case
            EditorApplication.update -= EditorUpdate;
        }

        void OnDisable()
        {
            SavePrefs();
        }

        void ImportTexture()
        {
            // copy to assets
            if (File.Exists(lastImagePath))
            {
                if (Directory.Exists(importFolder) == false)
                {
                    Directory.CreateDirectory(importFolder);
                }
                var targetImagePath = Path.Combine(importFolder, Path.GetFileName(lastImagePath));
                File.Copy(lastImagePath, targetImagePath, true);
                AssetDatabase.Refresh();
                var lastImageAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(targetImagePath);
                EditorGUIUtility.PingObject(lastImageAsset);
            }
        }

        void LoadPrefs()
        {
            url = EditorPrefs.GetString(prefix + "url", url);
            installationFolder = EditorPrefs.GetString(prefix + "installationFolder", installationFolder);
            importFolder = EditorPrefs.GetString(prefix + "importFolder", importFolder);
            prompt = EditorPrefs.GetString(prefix + "prompt", prompt);
            iterations = EditorPrefs.GetInt(prefix + "iterations", iterations);
            steps = EditorPrefs.GetInt(prefix + "steps", steps);
            cfg_scale = EditorPrefs.GetFloat(prefix + "cfg_scale", cfg_scale);
            //sampler_name = EditorPrefs.GetString(prefix + "sampler_name", sampler_name);
            //size = EditorPrefs.GetInt(prefix + "width", size);
            sizeIndex = EditorPrefs.GetInt(prefix + "sizeIndex", sizeIndex);
            samplerIndex = EditorPrefs.GetInt(prefix + "samplerIndex", samplerIndex);
            //width = EditorPrefs.GetInt(prefix + "width", width);
            //height = EditorPrefs.GetInt(prefix + "height", height);
            seed = EditorPrefs.GetString(prefix + "seed", seed);
            seamless = EditorPrefs.GetBool(prefix + "seamless", seamless);
            variation_amount = EditorPrefs.GetFloat(prefix + "variation_amount", variation_amount);
            with_variations = EditorPrefs.GetString(prefix + "with_variations", with_variations);
            initimg_name = EditorPrefs.GetString(prefix + "initimg_name", initimg_name);
            strength = EditorPrefs.GetFloat(prefix + "strength", strength);
            fit = EditorPrefs.GetBool(prefix + "fit", fit);
            gfpgan_strength = EditorPrefs.GetFloat(prefix + "gfpgan_strength", gfpgan_strength);
            upscale_level = EditorPrefs.GetString(prefix + "upscale_level", upscale_level);
            upscale_strength = EditorPrefs.GetFloat(prefix + "upscale_strength", upscale_strength);
        }

        void SavePrefs()
        {
            EditorPrefs.SetString(prefix + "url", url);
            EditorPrefs.SetString(prefix + "installationFolder", installationFolder);
            EditorPrefs.SetString(prefix + "importFolder", importFolder);
            EditorPrefs.SetString(prefix + "prompt", prompt);
            EditorPrefs.SetInt(prefix + "iterations", iterations);
            EditorPrefs.SetInt(prefix + "steps", steps);
            EditorPrefs.SetFloat(prefix + "cfg_scale", cfg_scale);
            //EditorPrefs.SetString(prefix + "sampler_name", sampler_name);
            //EditorPrefs.SetInt(prefix + "size", size);
            EditorPrefs.SetInt(prefix + "sizeIndex", sizeIndex);
            EditorPrefs.SetInt(prefix + "samplerIndex", samplerIndex);
            //EditorPrefs.SetInt(prefix + "width", width);
            //EditorPrefs.SetInt(prefix + "height", height);
            EditorPrefs.SetString(prefix + "seed", seed);
            EditorPrefs.SetBool(prefix + "seamless", seamless);
            EditorPrefs.SetFloat(prefix + "variation_amount", variation_amount);
            EditorPrefs.SetString(prefix + "with_variations", with_variations);
            EditorPrefs.SetString(prefix + "initimg_name", initimg_name);
            EditorPrefs.SetFloat(prefix + "strength", strength);
            EditorPrefs.SetBool(prefix + "fit", fit);
            EditorPrefs.SetFloat(prefix + "gfpgan_strength", gfpgan_strength);
            EditorPrefs.SetString(prefix + "upscale_level", upscale_level);
            EditorPrefs.SetFloat(prefix + "upscale_strength", upscale_strength);
        }

        // http://answers.unity.com/answers/1708382/view.html
        Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
