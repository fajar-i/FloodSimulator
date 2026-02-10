using UnityEngine;
using System.Collections.Generic;

public class RenderBatch
{
    public Mesh mesh;
    public Material material;
    
    // Kita simpan dalam potongan-potongan (Chunks)
    public List<List<Matrix4x4>> chunks = new List<List<Matrix4x4>>();

    public RenderBatch(Mesh m, Material mat)
    {
        this.mesh = m;
        this.material = mat;
        // Siapkan chunk pertama
        chunks.Add(new List<Matrix4x4>());
    }

    // Fungsi pintar untuk menambah data
    public void AddInstance(Matrix4x4 mat)
    {
        // Ambil chunk terakhir
        var lastChunk = chunks[chunks.Count - 1];

        // Jika sudah penuh (1023), buat chunk baru
        if (lastChunk.Count >= 1023)
        {
            lastChunk = new List<Matrix4x4>();
            chunks.Add(lastChunk);
        }

        lastChunk.Add(mat);
    }

    public void Clear()
    {
        chunks.Clear();
        chunks.Add(new List<Matrix4x4>());
    }
}