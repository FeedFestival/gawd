using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHexes : MonoBehaviour
{
    public GameObject Hex;
    public GameObject HexDown;

    void Start()
    {
        //var go = Instantiate(Hex);
        //go.name = "2";

        //go.transform.position = new Vector3(1.5f, 0, .866f);

        //combineMesh(go);

        //Destroy(go);
    }

    private void combineMesh(GameObject go)
    {
        var mainMeshFilter = Hex.GetComponent<MeshFilter>();

        var materialsList = new List<Material[]>();
        var meshFilters = new MeshFilter[] {
            go.GetComponent<MeshFilter>(),
            HexDown.GetComponent<MeshFilter>(),
            mainMeshFilter
        };
        var combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        foreach (var meshF in meshFilters)
        {
            var materials = meshF.gameObject.GetComponent<MeshRenderer>().materials;
            materialsList.Add(materials);

            combine[i].mesh = meshF.sharedMesh;
            combine[i].transform = meshF.transform.localToWorldMatrix;
            meshF.gameObject.SetActive(false);
            i++;
        }

        mainMeshFilter.mesh = new Mesh();
        mainMeshFilter.mesh.CombineMeshes(combine);
        Hex.transform.gameObject.SetActive(true);

        var mainMeshRenderer = Hex.GetComponent<MeshRenderer>();
        var materialsToAdd = new List<Material>();
        foreach (var materials in materialsList)
        {
            materialsToAdd.AddRange(materials);
        }
        mainMeshRenderer.materials = materialsToAdd.ToArray();
    }
}
