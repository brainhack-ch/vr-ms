using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _SmallCubeManager : MonoBehaviour {

    public string s_GO_cube_t3d_small_name;
    public string s_GO_cube_t3d_name;

    public Vector3 v3_local_position;
    public Vector3 v3_local_position_modified;
    public Vector3 v3_local_position_modified_constrained;
    Vector3 v3_parent_local_scale;
    Vector3 v3_main_cube_local_scale;
    GameObject GO_cube_t3d_small;
    GameObject GO_cube_t3d;
    public Vector3 i_nb_pixels_small_cube = new Vector3(5, 5, 5);
    Vector3 v3_GO_cube_t3d_small_init_local_position;

    // Use this for initialization
    void Start () {
        GO_cube_t3d_small = GameObject.Find(s_GO_cube_t3d_small_name);
        v3_GO_cube_t3d_small_init_local_position = GO_cube_t3d_small.transform.localPosition;
        GO_cube_t3d = GameObject.Find(s_GO_cube_t3d_name);
        v3_local_position = this.transform.localPosition;
    }
	
	// Update is called once per frame
	void Update () {
        if (this.gameObject.transform.localPosition != v3_local_position)
        {
            update_small_cube();
        }
    }

    public void update_small_cube()
    {
        v3_parent_local_scale = this.gameObject.transform.parent.transform.localScale;
        v3_local_position = this.gameObject.transform.localPosition;
        v3_local_position_modified = Vector3.Scale(v3_local_position, new Vector3(1 / v3_parent_local_scale.x, 1 / v3_parent_local_scale.y, 1 / v3_parent_local_scale.z));
        v3_local_position_modified_constrained = new Vector3(Mathf.Clamp(v3_local_position_modified.x, -0.5f, 0.5f), Mathf.Clamp(v3_local_position_modified.y, -0.5f, 0.5f), Mathf.Clamp(v3_local_position_modified.z, -0.5f, 0.5f));

        // Update small cube shader parameters
        float f_scale_1px = GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().f_scale_1px_public;
        /*GO_cube_t3d_small.transform.localScale = GO_cube_t3d.transform.localScale;*/
        Material material_small = GO_cube_t3d_small.GetComponent<Renderer>().material;

        v3_main_cube_local_scale = GO_cube_t3d.transform.localScale;

        Vector3 v3_pixel_nb_XYZ = new Vector3(v3_main_cube_local_scale.x / f_scale_1px, v3_main_cube_local_scale.y / f_scale_1px, v3_main_cube_local_scale.z / f_scale_1px); // pixel numbers on all 3 axes

        float f_SliceAxis1Min = Mathf.Clamp((Mathf.Round((v3_local_position_modified_constrained.x + 0.5f) * v3_pixel_nb_XYZ.x) / v3_pixel_nb_XYZ.x - i_nb_pixels_small_cube.x / v3_pixel_nb_XYZ.x), 0, 1); // * v3_pixel_nb_XYZ.x
        float f_SliceAxis1Max = Mathf.Clamp((Mathf.Round((v3_local_position_modified_constrained.x + 0.5f) * v3_pixel_nb_XYZ.x) / v3_pixel_nb_XYZ.x + i_nb_pixels_small_cube.x / v3_pixel_nb_XYZ.x), 0, 1); // * v3_pixel_nb_XYZ.x;
        float f_SliceAxis2Min = Mathf.Clamp((Mathf.Round((v3_local_position_modified_constrained.y + 0.5f) * v3_pixel_nb_XYZ.y) / v3_pixel_nb_XYZ.y - i_nb_pixels_small_cube.y / v3_pixel_nb_XYZ.y), 0, 1); // * v3_pixel_nb_XYZ.x
        float f_SliceAxis2Max = Mathf.Clamp((Mathf.Round((v3_local_position_modified_constrained.y + 0.5f) * v3_pixel_nb_XYZ.y) / v3_pixel_nb_XYZ.y + i_nb_pixels_small_cube.y / v3_pixel_nb_XYZ.y), 0, 1); // * v3_pixel_nb_XYZ.x;
        float f_SliceAxis3Min = Mathf.Clamp((Mathf.Round((v3_local_position_modified_constrained.z + 0.5f) * v3_pixel_nb_XYZ.z) / v3_pixel_nb_XYZ.z - i_nb_pixels_small_cube.z / v3_pixel_nb_XYZ.z), 0, 1); // * v3_pixel_nb_XYZ.x
        float f_SliceAxis3Max = Mathf.Clamp((Mathf.Round((v3_local_position_modified_constrained.z + 0.5f) * v3_pixel_nb_XYZ.z) / v3_pixel_nb_XYZ.z + i_nb_pixels_small_cube.z / v3_pixel_nb_XYZ.z), 0, 1); // * v3_pixel_nb_XYZ.x;

        material_small.SetFloat("_SliceAxis1Min", f_SliceAxis1Min);
        material_small.SetFloat("_SliceAxis1Max", f_SliceAxis1Max);
        material_small.SetFloat("_SliceAxis2Min", f_SliceAxis3Min);
        material_small.SetFloat("_SliceAxis2Max", f_SliceAxis3Max);
        material_small.SetFloat("_SliceAxis3Min", f_SliceAxis2Min);
        material_small.SetFloat("_SliceAxis3Max", f_SliceAxis2Max);

        //GO_cube_t3d_small.transform.localPosition = v3_GO_cube_t3d_small_init_local_position - new Vector3((Mathf.Round((v3_local_position_modified_constrained.y) * v3_pixel_nb_XYZ.y) / v3_pixel_nb_XYZ.y) * GO_cube_t3d_small.transform.localScale.y, -(Mathf.Round((v3_local_position_modified_constrained.x) * v3_pixel_nb_XYZ.x) / v3_pixel_nb_XYZ.x) * GO_cube_t3d_small.transform.localScale.x, (Mathf.Round((v3_local_position_modified_constrained.z) * v3_pixel_nb_XYZ.z) / v3_pixel_nb_XYZ.z) * GO_cube_t3d_small.transform.localScale.z);
        GO_cube_t3d_small.transform.localPosition = v3_GO_cube_t3d_small_init_local_position - new Vector3((Mathf.Round((v3_local_position_modified_constrained.x) * v3_pixel_nb_XYZ.x) / v3_pixel_nb_XYZ.x) * GO_cube_t3d_small.transform.localScale.x, (Mathf.Round((v3_local_position_modified_constrained.y) * v3_pixel_nb_XYZ.y) / v3_pixel_nb_XYZ.y) * GO_cube_t3d_small.transform.localScale.y, (Mathf.Round((v3_local_position_modified_constrained.z) * v3_pixel_nb_XYZ.z) / v3_pixel_nb_XYZ.z) * GO_cube_t3d_small.transform.localScale.z);

        float f_factor_scale = Mathf.Max(v3_pixel_nb_XYZ.x, v3_pixel_nb_XYZ.y, v3_pixel_nb_XYZ.z);
        this.transform.GetChild(0).localScale = new Vector3(f_scale_1px * i_nb_pixels_small_cube.x * 2 / v3_main_cube_local_scale.x, f_scale_1px * i_nb_pixels_small_cube.y * 2 / v3_main_cube_local_scale.y, f_scale_1px * i_nb_pixels_small_cube.z * 2 / v3_main_cube_local_scale.z);

        Debug.Log(GO_cube_t3d.transform.localScale.x / v3_pixel_nb_XYZ.x + " - " + GO_cube_t3d.transform.localScale.y / v3_pixel_nb_XYZ.y + " - " + GO_cube_t3d.transform.localScale.z / v3_pixel_nb_XYZ.z);
    }
}
