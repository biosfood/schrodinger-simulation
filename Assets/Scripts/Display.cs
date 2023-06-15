using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Display : MonoBehaviour
{
    public ComputeShader schrodinger, visualization;
    public RenderTexture texture;
    public ComputeBuffer psi, potential;
    public int resolution = 512;
    public int speed = 100;
    public Text time;
    public int elapsedTime = 0;
    public bool showTime = true;

    private void Start() {
        psi = new ComputeBuffer(resolution * resolution * 8, 8);
        // GC.KeepAlive(psiOld);
        potential = new ComputeBuffer(resolution * resolution * 4, 4);

        texture = new RenderTexture(resolution, resolution, 2);
        texture.enableRandomWrite = true;
        texture.Create();
        
        int kernel = schrodinger.FindKernel("initialize");
        schrodinger.SetInt("resolution", resolution);
        visualization.SetInt("resolution", resolution);
        schrodinger.SetBuffer(kernel, "psi", psi);
        schrodinger.SetBuffer(kernel, "potential", potential);
        schrodinger.Dispatch(kernel, resolution/32, resolution/32, 1);
    }

    private void Update() {
        int kernel = schrodinger.FindKernel("process");
        schrodinger.SetInt("speed", speed);
        schrodinger.SetBuffer(kernel, "psi", psi);
        schrodinger.SetBuffer(kernel, "potential", potential);
        for (int i = 0; i < speed; i++) {
            schrodinger.Dispatch(kernel, resolution/32, resolution/32, 1);
        }
        elapsedTime += speed;
        visualization.SetBuffer(0, "psiIn", psi);
        visualization.SetTexture(0, "result", texture);
        visualization.Dispatch(0, resolution/32, resolution/32, 1);
        if (showTime) {
            time.text = string.Format("time: {0} us", (float)elapsedTime / 200);
        } else {
            time.text = "";
        }
    }

    private void OnRenderImage(RenderTexture before, RenderTexture destination) {
        Graphics.Blit(texture, destination);        
    }
}
