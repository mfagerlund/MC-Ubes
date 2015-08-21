using UnityEngine;
using System.Collections;
using MarchingCubes;

[RequireComponent(typeof(MeshGenerator))]
public class RegenerateOnClick : MonoBehaviour
{
    private MeshGenerator _meshGenerator;

	public void Start ()
	{
	    _meshGenerator = GetComponent<MeshGenerator>();
	}

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _meshGenerator.GenerateMesh();            
        }
	}
}
