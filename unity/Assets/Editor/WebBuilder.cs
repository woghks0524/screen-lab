using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

// 장면(바닥·벽·공·별·상자 탑·경사로·카메라)을 코드로 만들고 웹(WebGL)으로 내보내요.
// 실행: Unity -batchmode -projectPath unity -executeMethod WebBuilder.Build -out ../unity-play -quit
public static class WebBuilder
{
    static Material Mat(string name, Color c, float gloss = 0.3f)
    {
        string path = "Assets/Materials/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, path); }
        m.color = c;
        m.SetFloat("_Glossiness", gloss);
        return m;
    }

    static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, float rotX = 0f)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = size;
        g.transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
        g.GetComponent<Renderer>().sharedMaterial = mat;
        return g;
    }

    [MenuItem("Lab/Build Web")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        var ground = Mat("Ground", new Color(0.56f, 0.80f, 0.56f), 0.1f);
        var wall = Mat("Wall", new Color(0.93f, 0.90f, 0.84f), 0.1f);
        var ball = Mat("Ball", new Color(0.16f, 0.44f, 0.86f), 0.7f);
        var star = Mat("Star", new Color(1f, 0.78f, 0.15f), 0.6f);
        var crate = Mat("Crate", new Color(0.89f, 0.34f, 0.12f), 0.2f);
        var ramp = Mat("Ramp", new Color(0.47f, 0.33f, 0.78f), 0.2f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 바닥과 벽
        Box("Ground", new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 24f), ground);
        Box("Wall N", new Vector3(0f, 0.5f, 12.25f), new Vector3(24.5f, 1f, 0.5f), wall);
        Box("Wall S", new Vector3(0f, 0.5f, -12.25f), new Vector3(24.5f, 1f, 0.5f), wall);
        Box("Wall E", new Vector3(12.25f, 0.5f, 0f), new Vector3(0.5f, 1f, 24.5f), wall);
        Box("Wall W", new Vector3(-12.25f, 0.5f, 0f), new Vector3(0.5f, 1f, 24.5f), wall);

        // 공(플레이어): Rigidbody가 있어야 굴러가고 부딪혀요
        var player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 0.5f, -8f);
        player.GetComponent<Renderer>().sharedMaterial = ball;
        player.AddComponent<Rigidbody>().angularDamping = 0.3f;
        player.AddComponent<PlayerController>();

        // 별 8개: Collider를 '트리거'로 두면 부딪히지 않고 닿았는지만 알려 줘요
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI * 2f / 8f;
            var s = Box("Star " + (i + 1), new Vector3(Mathf.Cos(a) * 7f, 0.7f, Mathf.Sin(a) * 7f), Vector3.one * 0.6f, star);
            s.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            s.GetComponent<Collider>().isTrigger = true;
            s.AddComponent<Pickup>();
        }

        // 무너뜨릴 상자 탑: 3-2-1 피라미드, 상자마다 Rigidbody
        for (int row = 0; row < 3; row++)
            for (int k = 0; k < 3 - row; k++)
            {
                var c = Box("Crate", new Vector3((k - (2 - row) / 2f) * 1.0f, 0.45f + row * 0.9f, 2f), Vector3.one * 0.9f, crate);
                c.AddComponent<Rigidbody>().mass = 0.4f;
            }

        // 점프대(경사로)
        Box("Ramp", new Vector3(-6f, 0.45f, -4f), new Vector3(3f, 0.3f, 5f), ramp, -14f);

        // 카메라가 공을 따라가게
        var cam = Camera.main;
        cam.transform.position = new Vector3(0f, 9f, -18f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.62f, 0.80f, 0.95f);
        cam.gameObject.AddComponent<CameraFollow>().target = player.transform;

        new GameObject("Hud").AddComponent<Hud>();

        EditorSceneManager.SaveScene(scene, "Assets/Main.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Main.unity", true) };

        PlayerSettings.companyName = "screen-lab";
        PlayerSettings.productName = "Roll Lab";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "PROJECT:Lab";
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);

        string outDir = "../unity-play";
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-out") outDir = args[i + 1];

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Main.unity" },
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Debug.Log("WEB BUILD: " + report.summary.result + " " + report.summary.totalSize + " bytes");
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
