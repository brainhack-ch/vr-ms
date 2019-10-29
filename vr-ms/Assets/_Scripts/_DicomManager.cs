using EvilDICOM.Core.Element;
using EvilDICOM.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class _DicomManager : MonoBehaviour {

    [Serializable]
    public class DICOM_meta_data
    {
        public Byte[] byte_raw_data;
        public Color[] color_pixel_data;
        public Texture2D t2d_color_pixel_data;
        public ushort ushort_rows;
        public ushort ushort_columns;
        public string s_firstname;
        public string s_lastname;
        public string s_age;
        public double d_sliceThickness;
        public double d_pixelSpacing;
        public double d_instanceNumber;
    }

    public class DICOM_datas_textures
    {
        public Texture2D[] t2d_color_pixel_data_1st_axis;
        public Texture2D[] t2d_color_pixel_data_2nd_axis;
        public Texture2D[] t2d_color_pixel_data_3rd_axis;
    }

    public DICOM_datas_textures current_image;

    List<DICOM_meta_data> a_DICOM_List_accessible;
    GameObject GO_cube_t3d;
    GameObject GO_cube_t3d_small;

    [HideInInspector]
    public float f_scale_1px_public;
    [HideInInspector]
    public GameObject GO_pointer;

    [Header("Public parameters")]
    public Color c_marks = new Color(1, 0, 0, 1); // color markers
    public float f_small_cube_scale = 1; // scale of the small zoom cube
    [Header("Public advanced parameters")]
    public double d_custom_DICOM_WINDOW_WIDTH = -1;
    public double d_custom_DICOM_WINDOW_CENTER = -1;

    public bool[,,] b_is_cube_present;
    public GameObject[,,] GO_cube_present;

    //GameObject GO_cube_t3d_mask;
    //GameObject GO_cube_t3d_small_mask;

    Quaternion quaternion_cube;
    Quaternion quaternion_cube_small;

    GameObject Pointer_2_choose_slices;



    string s_folder_name;


    // Use this for initialization
    void Start()
    {
        GO_pointer = GameObject.Find("_VolumetricDataRendering/Pointer");
        //Pointer_2_choose_slices = GameObject.Find("_VolumetricDataRendering/Pointer_2_choose_slices");

        GO_cube_t3d = GameObject.Find("_VolumetricDataRendering/_DICOM_3D_Texture_Displayer");
        GO_cube_t3d_small = GameObject.Find("_VolumetricDataRendering/_DICOM_3D_Texture_Displayer_small");

        quaternion_cube = GO_cube_t3d.transform.rotation;
        quaternion_cube_small = GO_cube_t3d_small.transform.rotation;
        //GO_cube_t3d_mask = GameObject.Find("_DICOM_3D_Texture_Displayer_mask");
        //GO_cube_t3d_small_mask = GameObject.Find("_DICOM_3D_Texture_Displayer_small_mask");


        ////var source = new StreamReader(Application.dataPath + "/.." + "/_Ressources/" + "config.txt");
        var source = new StreamReader("Assets/_Ressources/" + "config.txt");
        var fileContents = source.ReadToEnd();
        source.Close();
        var lines = fileContents.Split("\n"[0]);

        d_custom_DICOM_WINDOW_WIDTH = double.Parse(lines[1].Replace("\r", "").Replace("\n", ""));
        d_custom_DICOM_WINDOW_CENTER = double.Parse(lines[2].Replace("\r", "").Replace("\n", ""));

        s_folder_name = (string)lines[0].Replace("\r", "").Replace("\n", "");
        ////string s_folder_path = Application.dataPath + "/.." + "/_Ressources/" + (string)lines[0].Replace("\r", "").Replace("\n", "") + "/"; // add file folder selector
        string s_folder_path = "Assets/_Ressources/" + (string)lines[0].Replace("\r", "").Replace("\n", "") + "/"; ;
        LoadDatas(s_folder_path);
        //GO_cube_t3d.transform.GetChild(0).GetComponent<_SmallCubeManager>().update_small_cube();

        l_GO_slices_all_axis_full_model = new List<GameObject>();
        l_GO_slices_one_slice_per_axis = new List<GameObject>();

        DICOM_datas_textures DICOM_datas_textures_test = DICOM_data_textures_load_data_texture(a_DICOM_List_accessible);

        List<GameObject> l_Create_texture_2D_3D_all_axis_all_axis_full_model = Create_texture_2D_3D_all_axis(DICOM_datas_textures_test, "GO_slices_along_all_axis_full_model", f_slice_alpha_all_axis_full_model);
        GameObject GO_slices_along_all_axis_full_model = l_Create_texture_2D_3D_all_axis_all_axis_full_model[l_Create_texture_2D_3D_all_axis_all_axis_full_model.Count - 1];
        GO_slices_along_all_axis_full_model.transform.position = new UnityEngine.Vector3(-1, 1.25f, 0.5f);
        l_Create_texture_2D_3D_all_axis_all_axis_full_model.RemoveAt(l_Create_texture_2D_3D_all_axis_all_axis_full_model.Count - 1);
        l_GO_slices_all_axis_full_model = l_Create_texture_2D_3D_all_axis_all_axis_full_model;
        //duplicate_GO_slices_along_all_axes("GO_slices_along_all_axis_full_model", "GO_slices_along_all_axis_one_slice_at_a_time");
        List<GameObject> l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time = Create_texture_2D_3D_all_axis(DICOM_datas_textures_test, "GO_slices_along_all_axis_one_slice_at_a_time", f_slice_alpha_one_slice_per_axis);
        GameObject GO_slices_along_all_axis_one_slice_at_a_time = l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time[l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time.Count - 1];
        GO_slices_along_all_axis_one_slice_at_a_time.transform.position = new UnityEngine.Vector3(0, 1.25f, 0);
        l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time.RemoveAt(l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time.Count - 1);
        l_GO_slices_one_slice_per_axis = l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time;

        activate_only_specific_slices_one_slice_per_axis(0, 0, 0);
        b_is_cube_present = new bool[texture3D_tex3d.width, texture3D_tex3d.height, texture3D_tex3d.depth];
        GO_cube_present = new GameObject[texture3D_tex3d.width, texture3D_tex3d.height, texture3D_tex3d.depth];

        update_current_slices();

        GameObject.Find("GO_slices_along_all_axis_full_model").SetActive(false);
        GameObject.Find("GO_slices_along_all_axis_one_slice_at_a_time").transform.localScale *= f_ratio_size;
        GameObject.Find("_VolumetricDataRendering").transform.localScale *= f_ratio_size;

        debug_function();
    }


    void debug_function()
    {
        /*Debug.Log("w" + GO_cube_t3d.transform.localScale.x / texture3D_tex3d.width);
        Debug.Log("h" + GO_cube_t3d.transform.localScale.x / texture3D_tex3d.height);
        Debug.Log("d" + GO_cube_t3d.transform.localScale.x / texture3D_tex3d.depth);*/
        GameObject Pointer = GameObject.Find("_VolumetricDataRendering/Pointer");
        Debug.Log(Pointer.transform.localPosition);
    }


    public void try_add_cube()
    {
        GameObject Pointer = GameObject.Find("_VolumetricDataRendering/Pointer");
        GameObject ref_cube = GameObject.Find("_VolumetricDataRendering");
        if ((GO_pointer.transform.localPosition.x > -0.5) && (GO_pointer.transform.localPosition.x < 0.5))
        {
            if ((GO_pointer.transform.localPosition.y > -0.5) && (GO_pointer.transform.localPosition.y < 0.5))
            {
                if ((GO_pointer.transform.localPosition.z > -0.5) && (GO_pointer.transform.localPosition.z < 0.5))
                {
                    float f_GO_cube_scale = GO_cube_t3d.transform.localScale.x / texture3D_tex3d.width;

                    Debug.Log((int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale));
                    Debug.Log((int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale));
                    Debug.Log((int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale));
                    Debug.Log(b_is_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)]);

                    if (b_is_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)] == false)
                    {
                        GameObject GO_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GO_cube.transform.parent = ref_cube.transform;

                        GO_cube.transform.localScale = new UnityEngine.Vector3(f_GO_cube_scale, f_GO_cube_scale, f_GO_cube_scale);
                        UnityEngine.Vector3 adjusted_pointer_position = new UnityEngine.Vector3(0, 0, 0);
                        adjusted_pointer_position.x = Mathf.Round(Pointer.transform.localPosition.x / f_GO_cube_scale) * f_GO_cube_scale;
                        adjusted_pointer_position.y = Mathf.Round(Pointer.transform.localPosition.y / f_GO_cube_scale) * f_GO_cube_scale;
                        adjusted_pointer_position.z = Mathf.Round(Pointer.transform.localPosition.z / f_GO_cube_scale) * f_GO_cube_scale;
                        GO_cube.transform.localPosition = adjusted_pointer_position;

                        Material myNewMaterial = new Material(Shader.Find("Standard"));
                        Color col = new Color(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[0] / 255, GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[1] / 255, GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[2] / 255, GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersSize / 255);
                        myNewMaterial.SetColor("_Color", col);
                        GO_cube.GetComponent<Renderer>().material = myNewMaterial;

                        b_is_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)] = true;
                        GO_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)] = GO_cube;
                    }
                }
            }
        }
    }

    public void try_remove_cube()
    {
        GameObject Pointer = GameObject.Find("_VolumetricDataRendering/Pointer");
        GameObject ref_cube = GameObject.Find("_VolumetricDataRendering");
        if ((GO_pointer.transform.localPosition.x > -0.5) && (GO_pointer.transform.localPosition.x < 0.5))
        {
            if ((GO_pointer.transform.localPosition.y > -0.5) && (GO_pointer.transform.localPosition.y < 0.5))
            {
                if ((GO_pointer.transform.localPosition.z > -0.5) && (GO_pointer.transform.localPosition.z < 0.5))
                {
                    float f_GO_cube_scale = GO_cube_t3d.transform.localScale.x / texture3D_tex3d.width;

                    if (b_is_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)] == true)
                    {
                        /*GameObject GO_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GO_cube.transform.parent = ref_cube.transform;

                        GO_cube.transform.localScale = new UnityEngine.Vector3(f_GO_cube_scale, f_GO_cube_scale, f_GO_cube_scale);
                        UnityEngine.Vector3 adjusted_pointer_position = new UnityEngine.Vector3(0, 0, 0);
                        adjusted_pointer_position.x = Mathf.Round(Pointer.transform.localPosition.x / f_GO_cube_scale) * f_GO_cube_scale;
                        adjusted_pointer_position.y = Mathf.Round(Pointer.transform.localPosition.y / f_GO_cube_scale) * f_GO_cube_scale;
                        adjusted_pointer_position.z = Mathf.Round(Pointer.transform.localPosition.z / f_GO_cube_scale) * f_GO_cube_scale;
                        GO_cube.transform.localPosition = adjusted_pointer_position;*/

                        b_is_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)] = false;
                        Destroy(GO_cube_present[(int)Mathf.Round((Pointer.transform.localPosition.x + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.y + 0.5f) / f_GO_cube_scale), (int)Mathf.Round((Pointer.transform.localPosition.z + 0.5f) / f_GO_cube_scale)]);
                    }
                }
            }
        }
    }



    public void update_current_slices()
    {
        GameObject ref_cube = GameObject.Find("_VolumetricDataRendering");
        GameObject Pointer_2_choose_slices = GameObject.Find("_VolumetricDataRendering/Pointer_2_choose_slices");
        //GameObject Slices = GameObject.Find("GO_slices_along_all_axis_one_slice_at_a_time");

        if ((Pointer_2_choose_slices.transform.localPosition.x > -0.5 * ref_cube.transform.GetChild(0).transform.localScale.x) && (Pointer_2_choose_slices.transform.localPosition.x < 0.5 * ref_cube.transform.GetChild(0).transform.localScale.x))
        {
            if ((Pointer_2_choose_slices.transform.localPosition.y > -0.5 * ref_cube.transform.GetChild(0).transform.localScale.y) && (Pointer_2_choose_slices.transform.localPosition.y < 0.5 * ref_cube.transform.GetChild(0).transform.localScale.y))
            {
                if ((Pointer_2_choose_slices.transform.localPosition.z > -0.5 * ref_cube.transform.GetChild(0).transform.localScale.z) && (Pointer_2_choose_slices.transform.localPosition.z < 0.5 * ref_cube.transform.GetChild(0).transform.localScale.z))
                {
                    float f_GO_cube_scale = GO_cube_t3d.transform.localScale.x / texture3D_tex3d.width;

                    i_curr_active_2nd_slice = (int)Mathf.Round((Pointer_2_choose_slices.transform.localPosition.x + 0.5f * ref_cube.transform.GetChild(0).transform.localScale.x) / f_GO_cube_scale);
                    i_curr_active_1st_slice = (int)Mathf.Round((Pointer_2_choose_slices.transform.localPosition.y + 0.5f * ref_cube.transform.GetChild(0).transform.localScale.y) / f_GO_cube_scale);
                    i_curr_active_3rd_slice = (int)Mathf.Round((Pointer_2_choose_slices.transform.localPosition.z + 0.5f * ref_cube.transform.GetChild(0).transform.localScale.z) / f_GO_cube_scale);
                    //Debug.Log(Pointer_2_choose_slices.transform.localPosition.x + 0.5f + " " + Pointer_2_choose_slices.transform.localPosition.y + 0.5f + " " + Pointer_2_choose_slices.transform.localPosition.z + 0.5f);
                }
            }
        }        
    }



    /*void color_pixel_on_pointer()
    {
        Material material_small = GO_cube_t3d_small.GetComponent<Renderer>().material;

        //color_pixels_3D_Texture(texture3D_tex3d, texture3D_tex3d.depth - (int)(f_4+1), (int)(f_2), (int)(f_6), c_marks);

        float pointer_x = GO_pointer.transform.position.x;
        float pointer_y = GO_pointer.transform.position.y;
        float pointer_z = GO_pointer.transform.position.z;
        float y_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.y) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.y)) - pointer_y);
        float x_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.x) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.x)) - pointer_x);
        float z_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.z) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.z)) + pointer_z);

        float f_ratio_scale = Mathf.Max(GO_cube_t3d_small.transform.localScale.x, GO_cube_t3d_small.transform.localScale.y, GO_cube_t3d_small.transform.localScale.z);
        f_1 = y_small_cube;
        f_2 = y_small_cube / (f_scale_1px_public * f_ratio_scale);
        f_3 = x_small_cube;
        f_4 = x_small_cube / (f_scale_1px_public * f_ratio_scale);
        f_5 = z_small_cube;
        f_6 = z_small_cube / (f_scale_1px_public * f_ratio_scale);



        c_marks = new Color((float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[0]), (float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[1]), (float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[2]), (float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersSize));

        color_pixels_3D_Texture(texture3D_tex3d, texture3D_tex3d.depth - (int)(f_2 + 1), texture3D_tex3d.width - (int)(f_4 + 1), (int)(f_6), c_marks);
    }*/






    public float f_ratio_size = 2;







    //public GameObject Pointer_2_choose_slices;




    void try_remove_cube(UnityEngine.Vector3 position)
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.L))
        {
            var source = new StreamReader("Assets/_Ressources/" + "config.txt");
            var fileContents = source.ReadToEnd();
            source.Close();
            var lines = fileContents.Split("\n"[0]);

            d_custom_DICOM_WINDOW_WIDTH = double.Parse(lines[1].Replace("\r", "").Replace("\n", ""));
            d_custom_DICOM_WINDOW_CENTER = double.Parse(lines[2].Replace("\r", "").Replace("\n", ""));

            s_folder_name = (string)lines[0].Replace("\r", "").Replace("\n", "");
            string s_folder_path = "Assets/_Ressources/" + (string)lines[0].Replace("\r", "").Replace("\n", "") + "/"; // add file folder selector
            LoadDatas(s_folder_path);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            var source = new StreamReader("Assets/_Ressources/" + "config.txt");
            var fileContents = source.ReadToEnd();
            source.Close();
            var lines = fileContents.Split("\n"[0]);

            d_custom_DICOM_WINDOW_WIDTH = double.Parse(lines[1].Replace("\r", "").Replace("\n", ""));
            d_custom_DICOM_WINDOW_CENTER = double.Parse(lines[2].Replace("\r", "").Replace("\n", ""));

            s_folder_name = (string)lines[0].Replace("\r", "").Replace("\n", "");
            string s_folder_path = "Assets/_Ressources/" + (string)lines[0].Replace("\r", "").Replace("\n", "") + "/"; // add file folder selector
            LoadDatas(s_folder_path);


            DICOM_datas_textures DICOM_datas_textures_test = DICOM_data_textures_load_data_texture(a_DICOM_List_accessible);

            List<GameObject> l_Create_texture_2D_3D_all_axis_all_axis_full_model = Create_texture_2D_3D_all_axis(DICOM_datas_textures_test, "GO_slices_along_all_axis_full_model", f_slice_alpha_all_axis_full_model);
            GameObject GO_slices_along_all_axis_full_model = l_Create_texture_2D_3D_all_axis_all_axis_full_model[l_Create_texture_2D_3D_all_axis_all_axis_full_model.Count - 1];
            GO_slices_along_all_axis_full_model.transform.position = new UnityEngine.Vector3(-1, 1.25f, 0.5f);
            l_Create_texture_2D_3D_all_axis_all_axis_full_model.RemoveAt(l_Create_texture_2D_3D_all_axis_all_axis_full_model.Count - 1);
            l_GO_slices_all_axis_full_model = l_Create_texture_2D_3D_all_axis_all_axis_full_model;
            //duplicate_GO_slices_along_all_axes("GO_slices_along_all_axis_full_model", "GO_slices_along_all_axis_one_slice_at_a_time");
            List<GameObject> l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time = Create_texture_2D_3D_all_axis(DICOM_datas_textures_test, "GO_slices_along_all_axis_one_slice_at_a_time", f_slice_alpha_one_slice_per_axis);
            GameObject GO_slices_along_all_axis_one_slice_at_a_time = l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time[l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time.Count - 1];
            GO_slices_along_all_axis_one_slice_at_a_time.transform.position = new UnityEngine.Vector3(0, 1.25f, 0);
            l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time.RemoveAt(l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time.Count - 1);
            l_GO_slices_one_slice_per_axis = l_Create_texture_2D_3D_all_axis_all_axis_one_slice_at_a_time;

            activate_only_specific_slices_one_slice_per_axis(0,0,0);
            b_is_cube_present = new bool[texture3D_tex3d.width, texture3D_tex3d.height, texture3D_tex3d.depth];
            GO_cube_present = new GameObject[texture3D_tex3d.width, texture3D_tex3d.height, texture3D_tex3d.depth];

            update_current_slices();

            GameObject.Find("GO_slices_along_all_axis_full_model").SetActive(false);
            GameObject.Find("GO_slices_along_all_axis_one_slice_at_a_time").transform.localScale *= f_ratio_size;
            GameObject.Find("_VolumetricDataRendering").transform.localScale *= f_ratio_size;

            debug_function();

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {

            //m_MyTexture = GameObject.Find("_DICOM_2D_Texture_Displayer_Z").GetComponent<Renderer>().material.mainTexture;
            m_MyTexture = GameObject.Find("_DICOM_3D_Texture_Displayer").GetComponent<Renderer>().material.GetTexture("_Data");
            //Switch the Texture's Filter Mode
            m_MyTexture.filterMode = SwitchFilterModes();
            //Output the current Filter Mode to the console
            Debug.Log("Filter mode : " + m_MyTexture.filterMode);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            try_add_cube();
        }
        if (Input.GetKey(KeyCode.G))
        {
            try_remove_cube();
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            i_curr_active_1st_slice += 1;
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            i_curr_active_1st_slice -= 1;
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            i_curr_active_2nd_slice += 1;
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            i_curr_active_2nd_slice -= 1;
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            i_curr_active_3rd_slice += 1;
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            i_curr_active_3rd_slice -= 1;
        }
        */

        // on alpha change all slices full model
        if ((f_slice_alpha_all_axis_full_model != f_slice_alpha_all_axis_full_model_old) && (f_slice_alpha_all_axis_full_model > 0) && (f_slice_alpha_all_axis_full_model <= 1))
        {
            on_slice_alpha_changed(f_slice_alpha_all_axis_full_model, f_slice_alpha_all_axis_full_model_old, l_GO_slices_all_axis_full_model);
            f_slice_alpha_all_axis_full_model_old = f_slice_alpha_all_axis_full_model;
        }
        // on alpha change one slice per axis model
        if ((f_slice_alpha_one_slice_per_axis != f_slice_alpha_one_slice_per_axis_old) && (f_slice_alpha_one_slice_per_axis > 0) && (f_slice_alpha_one_slice_per_axis <= 1))
        {
            on_slice_alpha_changed(f_slice_alpha_one_slice_per_axis, f_slice_alpha_one_slice_per_axis_old, l_GO_slices_one_slice_per_axis);
            f_slice_alpha_one_slice_per_axis_old = f_slice_alpha_one_slice_per_axis;
        }

        // on change actives slices on slice per axis model
        if ((i_curr_active_1st_slice != i_curr_active_1st_slice_old) && (i_curr_active_1st_slice < texture3D_tex3d.depth) && (i_curr_active_1st_slice >= 0))
        {
            activate_only_specific_slices_one_slice_per_axis(i_curr_active_1st_slice, i_curr_active_2nd_slice, i_curr_active_3rd_slice);
            i_curr_active_1st_slice_old = i_curr_active_1st_slice;
        }
        if ((i_curr_active_2nd_slice != i_curr_active_2nd_slice_old) && (i_curr_active_2nd_slice < texture3D_tex3d.width) && (i_curr_active_2nd_slice >= 0))
        {
            activate_only_specific_slices_one_slice_per_axis(i_curr_active_1st_slice, i_curr_active_2nd_slice, i_curr_active_3rd_slice);
            i_curr_active_2nd_slice_old = i_curr_active_2nd_slice;
        }
        if ((i_curr_active_3rd_slice != i_curr_active_3rd_slice_old) && (i_curr_active_3rd_slice < texture3D_tex3d.height) && (i_curr_active_3rd_slice >= 0))
        {
            activate_only_specific_slices_one_slice_per_axis(i_curr_active_1st_slice, i_curr_active_2nd_slice, i_curr_active_3rd_slice);
            i_curr_active_3rd_slice_old = i_curr_active_3rd_slice;
        }

        // on alpha active cahnge
        if ((b_slice_alpha_all_axis_full_model != b_slice_alpha_all_axis_full_model_old))
        {
            on_alpha_active_changed(b_slice_alpha_all_axis_full_model, b_slice_alpha_all_axis_full_model_old, l_GO_slices_all_axis_full_model);
            b_slice_alpha_all_axis_full_model_old = b_slice_alpha_all_axis_full_model;
        }
        if ((b_slice_alpha_one_slice_per_axis != b_slice_alpha_one_slice_per_axis_old))
        {
            on_alpha_active_changed(b_slice_alpha_one_slice_per_axis, b_slice_alpha_one_slice_per_axis_old, l_GO_slices_one_slice_per_axis);
            b_slice_alpha_one_slice_per_axis_old = b_slice_alpha_one_slice_per_axis;
        }       
    }


    public int i_curr_active_1st_slice = 0;
    public int i_curr_active_2nd_slice = 0;
    public int i_curr_active_3rd_slice = 0;
    int i_curr_active_1st_slice_old = 0;
    int i_curr_active_2nd_slice_old = 0;
    int i_curr_active_3rd_slice_old = 0;

    public void activate_only_specific_slices_one_slice_per_axis(int i_active_1st_slice, int i_active_2nd_slice, int i_active_3rd_slice)
    {
        for (int i = 0; i < l_GO_slices_one_slice_per_axis.Count(); i++)
        {
            l_GO_slices_one_slice_per_axis[i].gameObject.SetActive(false);
        }
        l_GO_slices_one_slice_per_axis[i_active_1st_slice].gameObject.SetActive(true);
        l_GO_slices_one_slice_per_axis[i_active_2nd_slice + texture3D_tex3d.depth - 1].gameObject.SetActive(true);
        l_GO_slices_one_slice_per_axis[i_active_3rd_slice + texture3D_tex3d.depth + texture3D_tex3d.width - 1].gameObject.SetActive(true);

    }

    public void color_pixel_on_pointer()
    {
        if (isPointerInsideSmallCube())
        {
            colorPixelOnPointer_o();
        }
    }

    public void decolor_pixel_on_pointer()
    {
        if (isPointerInsideSmallCube())
        {
            decolorPixelOnPointer();
        }
    }

    void colorPixelOnPointer_o()
    {
        Material material_small = GO_cube_t3d_small.GetComponent<Renderer>().material;

        float pointer_x = GO_pointer.transform.position.x;
        float pointer_y = GO_pointer.transform.position.y;
        float pointer_z = GO_pointer.transform.position.z;
        float y_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.y) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.y)) - pointer_y);
        float x_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.x) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.x)) - pointer_x);
        float z_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.z) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.z)) + pointer_z);

        float f_ratio_scale = Mathf.Max(GO_cube_t3d_small.transform.localScale.x, GO_cube_t3d_small.transform.localScale.y, GO_cube_t3d_small.transform.localScale.z);
        f_1 = y_small_cube;
        f_2 = y_small_cube / (f_scale_1px_public * f_ratio_scale);
        f_3 = x_small_cube;
        f_4 = x_small_cube / (f_scale_1px_public * f_ratio_scale);
        f_5 = z_small_cube;
        f_6 = z_small_cube / (f_scale_1px_public * f_ratio_scale);


        c_marks = new Color((float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[0]), (float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[1]), (float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersRGB[2]), (float)(GameObject.Find("_InteractionsScipts").GetComponent<InteractionsScript>().MarkersSize)); 

        color_pixels_3D_Texture(texture3D_tex3d, texture3D_tex3d.depth - (int)(f_2 + 1), texture3D_tex3d.width - (int)(f_4 + 1), (int)(f_6), c_marks);
    }

    float f_1;
    float f_2;
    float f_3;
    float f_4;
    float f_5;
    float f_6;

    void decolorPixelOnPointer()
    {
        Material material_small = GO_cube_t3d_small.GetComponent<Renderer>().material;

        float pointer_x = GO_pointer.transform.position.x;
        float pointer_y = GO_pointer.transform.position.y;
        float pointer_z = GO_pointer.transform.position.z;
        float y_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.y) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.y)) - pointer_y);
        float x_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.x) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.x)) - pointer_x);
        float z_small_cube = Mathf.Abs((float)(Mathf.Abs(GO_cube_t3d_small.transform.position.z) + Mathf.Abs((0.5f) * GO_cube_t3d_small.transform.localScale.z)) + pointer_z);

        float f_ratio_scale = Mathf.Max(GO_cube_t3d_small.transform.localScale.x, GO_cube_t3d_small.transform.localScale.y, GO_cube_t3d_small.transform.localScale.z);
        f_1 = y_small_cube;
        f_2 = y_small_cube / (f_scale_1px_public * f_ratio_scale);
        f_3 = x_small_cube;
        f_4 = x_small_cube / (f_scale_1px_public * f_ratio_scale);
        f_5 = z_small_cube;
        f_6 = z_small_cube / (f_scale_1px_public * f_ratio_scale);

        decolor_pixels_3D_Texture(texture3D_tex3d, texture3D_tex3d.depth - (int)(f_2 + 1), texture3D_tex3d.width - (int)(f_4 + 1), (int)(f_6));
    }

    bool isPointerInsideSmallCube()
    {
        Material material_small = GO_cube_t3d_small.GetComponent<Renderer>().material;

        float f_SliceAxis1Min = material_small.GetFloat("_SliceAxis1Min");
        float f_SliceAxis1Max = material_small.GetFloat("_SliceAxis1Max");
        float f_SliceAxis2Min = material_small.GetFloat("_SliceAxis2Min");
        float f_SliceAxis2Max = material_small.GetFloat("_SliceAxis2Max");
        float f_SliceAxis3Min = material_small.GetFloat("_SliceAxis3Min");
        float f_SliceAxis3Max = material_small.GetFloat("_SliceAxis3Max");

        float pointer_x = GO_pointer.transform.position.x;
        float small_x_min = (float)(GO_cube_t3d_small.transform.position.x - (0.5 - f_SliceAxis1Min) * GO_cube_t3d_small.transform.localScale.x);
        float small_x_max = (float)(GO_cube_t3d_small.transform.position.x - (0.5 - f_SliceAxis1Max) * GO_cube_t3d_small.transform.localScale.x);
        float pointer_y = GO_pointer.transform.position.y;
        float small_y_min = (float)(GO_cube_t3d_small.transform.position.y - (0.5 - (1 - f_SliceAxis3Max)) * GO_cube_t3d_small.transform.localScale.y);
        float small_y_max = (float)(GO_cube_t3d_small.transform.position.y - (0.5 - (1 - f_SliceAxis3Min)) * GO_cube_t3d_small.transform.localScale.y);
        float pointer_z = GO_pointer.transform.position.z;
        float small_z_min = (float)(GO_cube_t3d_small.transform.position.z - (0.5 - f_SliceAxis2Min) * GO_cube_t3d_small.transform.localScale.z);
        float small_z_max = (float)(GO_cube_t3d_small.transform.position.z - (0.5 - f_SliceAxis2Max) * GO_cube_t3d_small.transform.localScale.z);

        if (pointer_x > small_x_min)
        {
            if (pointer_x < small_x_max)
            {
                if (pointer_y > small_y_min)
                {
                    if (pointer_y < small_y_max)
                    {
                        if (pointer_z > small_z_min)
                        {
                            if (pointer_z < small_z_max)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool b;

    Texture3D texture3D_tex3d;
    Texture3D texture3D_tex3d_mask;
    Color[] volumeColors;
    Color[] volumeColors_save;

    void Create_texture_2D_3D_1st_axis(List<DICOM_meta_data> l_DICOM_meta_data_in)
    {
        var GO_slice_Y = new GameObject();
        GO_slice_Y.name = "GO_slice_Y";

        for (int i=0; i<l_DICOM_meta_data_in.Count; i++)
        {
            // use a bunch of memory!
            Texture2D texture2D_tex_2d = new Texture2D(l_DICOM_meta_data_in[i].t2d_color_pixel_data.width, l_DICOM_meta_data_in[i].t2d_color_pixel_data.height, TextureFormat.RGBA32, false);
            texture2D_tex_2d = l_DICOM_meta_data_in[i].t2d_color_pixel_data;
            texture2D_tex_2d.Apply();

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.parent = GO_slice_Y.transform;
            plane.name = i + "_slice_Y";
            Material myNewMaterial = new Material(Shader.Find("Unlit/AlphaSelfIllum"));
            plane.GetComponent<MeshRenderer>().material = myNewMaterial;

            float f_scale_1px = 1.0f / (Mathf.Max(l_DICOM_meta_data_in[i].t2d_color_pixel_data.width, l_DICOM_meta_data_in[i].t2d_color_pixel_data.height));
            plane.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, 1, texture2D_tex_2d.height) * f_scale_1px;
            plane.transform.localPosition = new UnityEngine.Vector3(0, i * (float)l_DICOM_meta_data_in[i].d_sliceThickness * f_scale_1px * 10, 0);
            Material material = plane.GetComponent<Renderer>().material;
            /*material.SetFloat("_Mode", 3);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;*/
            material.color = new Color(material.color.r, material.color.g, material.color.b, material.color.a*0.25f);
            material.SetTexture("_MainTex", texture2D_tex_2d);

            plane.GetComponent<Renderer>().material.GetTexture("_MainTex").filterMode = FilterMode.Point;
        }
    }

    void set_texture_2D_Z(int i_slice, List<DICOM_meta_data> l_DICOM_meta_data_in)
    {
        if ((i_slice < 0) || (i_slice >= l_DICOM_meta_data_in.Count))
        {
            Debug.LogError("slice nb doesn't exist (<0 || >max) : " + i_slice);
            return;
        }

        // use a bunch of memory!
        Texture2D texture2D_tex_2d = new Texture2D(l_DICOM_meta_data_in[i_slice].t2d_color_pixel_data.width, l_DICOM_meta_data_in[i_slice].t2d_color_pixel_data.height, TextureFormat.RGBA32, false);
        texture2D_tex_2d = l_DICOM_meta_data_in[i_slice].t2d_color_pixel_data;
        texture2D_tex_2d.Apply();

        Color channelsIntensity = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        GameObject GO_cube_t2d_Z = GameObject.Find("_DICOM_2D_Texture_Displayer_Z");
        float f_scale_1px = 1.0f / (Mathf.Max(l_DICOM_meta_data_in[i_slice].t2d_color_pixel_data.width, l_DICOM_meta_data_in[i_slice].t2d_color_pixel_data.height));
        Material material = GO_cube_t2d_Z.GetComponent<Renderer>().material;
        material.SetTexture("_MainTex", texture2D_tex_2d);
    }

    void LoadDatas(string s_folder_path)
    {
        List<DICOM_meta_data> a_DICOM_List = new List<DICOM_meta_data>();
        List<Texture2D> a_DICOM_List_t2d = new List<Texture2D>();

        // Fill the list will all DICOM images filename
        DirectoryInfo dir = new DirectoryInfo(s_folder_path);
        FileInfo[] info = dir.GetFiles("*.dcm");

        List<string> list_DICOM_filenames_string = new List<string>();
        foreach (FileInfo s_file_name in info)
        {
            list_DICOM_filenames_string.Add(s_file_name.ToString());
        }
        
        // Sort the list to get images in right order
        list_DICOM_filenames_string.Sort();
        
        // Fill arrays with datas corresponding to DICOM images
        foreach (string s_file_name in list_DICOM_filenames_string)
        {
            //Debug.Log(s_file_name.ToString());
            DICOM_meta_data current_DICOM_image = DICOM_meta_data_load_EvilDICOM(s_file_name.ToString());
            a_DICOM_List.Add(current_DICOM_image);
        }

        // Sort the list by instance number to get images in proper order
        a_DICOM_List = a_DICOM_List.OrderBy(o => o.d_instanceNumber).ToList();

        foreach (DICOM_meta_data current_DICOM_meta_data in a_DICOM_List)
        {
            a_DICOM_List_t2d.Add(current_DICOM_meta_data.t2d_color_pixel_data);
        }

        // make list available in other functions
        a_DICOM_List_accessible = a_DICOM_List;


        //list_DICOM_filenames_string.Sort();

        // Veify Dataset
        bool b_is_DICOM_dataset_valid = true;

        if (b_is_DICOM_dataset_valid)
        {
            // Create the 3D texture and apply it
            texture3D_tex3d = GenerateVolumeTexture(a_DICOM_List_t2d);

            // Apply the 3D texture to a cube
            Color channelsIntensity = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            float f_scale_1px = 1.0f / (Mathf.Max(texture3D_tex3d.width, texture3D_tex3d.depth * (float)a_DICOM_List[0].d_sliceThickness / (float)a_DICOM_List[0].d_pixelSpacing, texture3D_tex3d.height));
            f_scale_1px_public = f_scale_1px;
            GO_cube_t3d.transform.localScale = new UnityEngine.Vector3(texture3D_tex3d.width, texture3D_tex3d.depth * (float)a_DICOM_List[0].d_sliceThickness / (float)a_DICOM_List[0].d_pixelSpacing, texture3D_tex3d.height) * f_scale_1px;
            Material material = GO_cube_t3d.GetComponent<Renderer>().material;
            material.SetTexture("_Data", texture3D_tex3d);
            material.SetVector("_DataChannel", channelsIntensity); // intensity

            GO_cube_t3d_small.transform.localScale = GO_cube_t3d.transform.localScale * f_small_cube_scale;
            Material material_small = GO_cube_t3d_small.GetComponent<Renderer>().material;
            material_small.SetTexture("_Data", texture3D_tex3d);
            //material_small.SetVector("_DataChannel", channelsIntensity); // intensity
            material_small.GetTexture("_Data").filterMode = FilterMode.Point;
        }
    }

    private Texture3D GenerateVolumeTexture(List<Texture2D> t2d_slices)
    {
        int volumeHeight = t2d_slices[0].height;
        int volumeWidth = t2d_slices[0].width;
        int volumeDepth = t2d_slices.Count;

        // use a bunch of memory!
        Texture3D _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.RGBA32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;

        // skip some slices if we can't fit it all in
        var countOffset = (t2d_slices.Count - 1) / (float)d;

        //var volumeColors = new Color[w * h * d];
        volumeColors = new Color[w * h * d];
        volumeColors_save = new Color[w * h * d];

        var sliceCount = 0;
        var sliceCountFloat = 0f;

        /////
        /*int[] histogram = new int[255];
        string s_histo = s_folder_name;
        s_histo += "\n";
        s_histo += "0";
        for (int i = 1; i < 255; i++)
        {
            s_histo += " " + i.ToString();
        }*/

        /////
        for (int z = 0; z < d; z++)
        {
            sliceCountFloat += countOffset;
            sliceCount = Mathf.FloorToInt(sliceCountFloat);

            ///
            //List<int> histogram = new List<int>();
            /*for (int i=0; i<255; i++)
            {
                histogram[i] = 0;
            }*/
            ///

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var idx = x + (y * w) + (z * (w * h));

                    Color c = t2d_slices[sliceCount].GetPixelBilinear(x / (float)w, y / (float)h);
                    volumeColors[idx] = new Color(c.r, c.g, c.b, c.a);
                    volumeColors_save[idx] = new Color(c.r, c.g, c.b, c.a);

                    ///
                    /*histogram[(int)Mathf.Round(c.r * 254)] += 1;*/
                    ///
                }
            }

            /////
            /*s_histo += "\n";
            s_histo += histogram[0].ToString();
            for (int i = 1; i < 255; i++)
            {
                s_histo += " " + histogram[i].ToString();
            }*/
            ////
        }

        ///File.WriteAllText("Assets/_Ressources/Debug/" + s_folder_name + "-histogram255" + ".txt", s_histo);

        _volumeBuffer.SetPixels(volumeColors);
        _volumeBuffer.Apply();

        return _volumeBuffer;
    }


    /*private Texture3D GenerateEmptyVolumeTexture(List<Texture2D> t2d_slices)
    {
        int volumeHeight = t2d_slices[0].height;
        int volumeWidth = t2d_slices[0].width;
        int volumeDepth = t2d_slices.Count;

        // use a bunch of memory!
        Texture3D _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.RGBA32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;

        // skip some slices if we can't fit it all in
        var countOffset = (t2d_slices.Count - 1) / (float)d;

        //var volumeColors = new Color[w * h * d];
        volumeColors = new Color[w * h * d];

        var sliceCount = 0;
        var sliceCountFloat = 0f;
        for (int z = 0; z < d; z++)
        {
            sliceCountFloat += countOffset;
            sliceCount = Mathf.FloorToInt(sliceCountFloat);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var idx = x + (y * w) + (z * (w * h));

                    //Color c = t2d_slices[sliceCount].GetPixelBilinear(x / (float)w, y / (float)h);
                    Color c = new Color(0, 0, 0, 0);
                    volumeColors[idx] = new Color(c.r, c.g, c.b, c.a);
                }
            }
        }

        _volumeBuffer.SetPixels(volumeColors);
        _volumeBuffer.Apply();

        return _volumeBuffer;
    }*/


    void color_pixels_3D_Texture(Texture3D t3d_in, int z, int x, int y, Color c)
    {
        int d = t3d_in.depth;
        int w = t3d_in.width;
        int h = t3d_in.height;

        var idx = x + (y * w) + (z * (w * h));

        volumeColors[idx]= c;

        t3d_in.SetPixels(volumeColors);
        t3d_in.Apply();
    }


    void decolor_pixels_3D_Texture(Texture3D t3d_in, int z, int x, int y)
    {
        int d = t3d_in.depth;
        int w = t3d_in.width;
        int h = t3d_in.height;

        var idx = x + (y * w) + (z * (w * h));

        volumeColors[idx] = volumeColors_save[idx];

        t3d_in.SetPixels(volumeColors);
        t3d_in.Apply();
    }



    bool is_DICOM_dataset_valid(List<DICOM_meta_data> a_DICOM_List)
    {
        int i_nb_slices = 0;
        int i_nb_rows = 0;
        int i_nb_columns = 0;
        if (a_DICOM_List.Count > 0)
        {
            ushort i_a_DICOM_List_0_rows = a_DICOM_List[0].ushort_rows;
            ushort i_a_DICOM_List_0_columns = a_DICOM_List[0].ushort_columns;
            i_nb_slices++;
            if (a_DICOM_List.Count > 1)
            {
                for (int i = 1; i < a_DICOM_List.Count; i++)
                {
                    if ((a_DICOM_List[i].ushort_rows == i_a_DICOM_List_0_rows) && (a_DICOM_List[i].ushort_columns == i_a_DICOM_List_0_columns))
                    {

                    }
                    else
                    {
                        i_nb_rows = -1;
                        i_nb_columns = -1;
                        Debug.LogError("inconsistances in rows and columns detected among slices");
                        return false;
                    }

                    i_nb_slices++;
                }
            }
            if ((i_nb_rows == 0) && (i_nb_columns == 0))
            {
                i_nb_rows = i_a_DICOM_List_0_rows;
                i_nb_columns = i_a_DICOM_List_0_columns;
            }
        }
        else
        {
            Debug.LogError("No data in 'a_DICOM_List' container");
            return false;
        }
        return true;
    }



    public DICOM_meta_data DICOM_meta_data_load_EvilDICOM(string s_file_name)
    {
        var dcm = EvilDICOM.Core.DICOMObject.Read(s_file_name);

        string photo = dcm.FindFirst(TagHelper.PHOTOMETRIC_INTERPRETATION).DData.ToString();
        ushort bitsAllocated = (ushort)dcm.FindFirst(TagHelper.BITS_ALLOCATED).DData;
        ushort highBit = (ushort)dcm.FindFirst(TagHelper.HIGH_BIT).DData;
        ushort bitsStored = (ushort)dcm.FindFirst(TagHelper.BITS_STORED).DData;
        //double intercept = (double)dcm.FindFirst(TagHelper.RESCALE_INTERCEPT).DData;
        //double slope = (double)dcm.FindFirst(TagHelper.RESCALE_SLOPE).DData;
        ushort pixelRepresentation = (ushort)dcm.FindFirst(TagHelper.PIXEL_REPRESENTATION).DData;
        List<byte> pixelData = (List<byte>)dcm.FindFirst(TagHelper.PIXEL_DATA).DData_;
        double window;
        double level;

        /*try
        {
            window = (double)dcm.FindFirst(TagHelper.WINDOW_WIDTH).DData;
            level = (double)dcm.FindFirst(TagHelper.WINDOW_CENTER).DData;
        }
        catch (Exception ex)
        {
            window = 10 * Mathf.Max((ushort)dcm.FindFirst(TagHelper.ROWS).DData, (ushort)dcm.FindFirst(TagHelper.COLUMNS).DData);
            level = window / 2;

            if (d_custom_DICOM_WINDOW_WIDTH >= 0)
            {
                window = d_custom_DICOM_WINDOW_WIDTH;
            }
            if (d_custom_DICOM_WINDOW_CENTER >= 0)
            {
                level = d_custom_DICOM_WINDOW_CENTER;
            }
        }*/

        window = d_custom_DICOM_WINDOW_WIDTH;
        level = d_custom_DICOM_WINDOW_CENTER;

        ushort rows = (ushort)dcm.FindFirst(TagHelper.ROWS).DData;
        ushort columns = (ushort)dcm.FindFirst(TagHelper.COLUMNS).DData;

        /*Debug.Log(rows);
        Debug.Log(columns);
        Debug.Log(pixelData.Count);*/

        /*if (!photo.Contains("MONOCHROME")) //just works for gray images
            return new DICOM_meta_data();*/

        int index = 0;
        byte[] outPixelData = new byte[rows * columns * 4]; //rgba
        Color[] outPixelColors = new Color[rows * columns]; //rgba
        int i_color_index = 0;
        ushort mask = (ushort)(ushort.MaxValue >> (bitsAllocated - bitsStored));
        /***double maxval = Math.Pow(2, bitsStored);***/

        ////
        //string s_histo_base = "";
        //int[] a_histo_base = new int[pixelData.Count];
        ////

        double[] direct_intensity = new double[pixelData.Count];
        double[] color_direct_intensity = new double[pixelData.Count];


        for (int i = 0; i < pixelData.Count; i += 2)
        {
            ushort gray = (ushort)((ushort)(pixelData[i]) + (ushort)(pixelData[i + 1] << 8));
            double valgray = gray & mask; //remove not used bits

            ///
            //s_histo_base += valgray.ToString() + " ";
            ///

            /***if (pixelRepresentation == 1) // the last bit is the sign, apply a2 complement
            {
                if (valgray > (maxval / 2))
                {
                    valgray = (valgray - maxval);
                }
            }***/

            //valgray = slope * valgray + intercept;//modality lut

            //This is  the window level algorithm
            /***double half = ((window - 1) / 2.0) - 0.5;

            if (valgray <= level - half)
                valgray = 0;
            else if (valgray >= level + half)
                valgray = 255;
            else
                valgray = ((valgray - (level - 0.5)) / (window - 1) + 0.5) * 255;***/

            double f_min = window;
            double f_max = level;

            if (valgray < f_min)
            {
                valgray = f_min;
            }
            else if (valgray > f_max)
            {
                valgray = f_max;
            }

            valgray = (valgray - f_min) / (f_max - f_min);

            valgray = valgray * 255;

            outPixelData[index] = (byte)valgray;
            outPixelData[index + 1] = (byte)valgray;
            outPixelData[index + 2] = (byte)valgray;
            outPixelData[index + 3] = 255;

            /////////////////////////////////////////
            outPixelData[index] = (byte)valgray;
            outPixelData[index + 1] = (byte)valgray;
            outPixelData[index + 2] = (byte)valgray;
            outPixelData[index + 3] = 255;
            /////////////////////////////////////////



            outPixelColors[i_color_index] = new Color((float)valgray / 255, (float)valgray / 255, (float)valgray / 255, 255 / 255);
            i_color_index++;

            index += 4;
        }

        // retrieve some data
        DICOM_meta_data DICOM_current_image = new DICOM_meta_data();
        DICOM_current_image.ushort_rows = (ushort)dcm.FindFirst(TagHelper.ROWS).DData;
        DICOM_current_image.ushort_columns = (ushort)dcm.FindFirst(TagHelper.COLUMNS).DData;
        DICOM_current_image.byte_raw_data = outPixelData;
        DICOM_current_image.color_pixel_data = outPixelColors;

        // retrieve some other data
        var strongName = dcm.FindFirst(TagHelper.PATIENT_NAME) as PersonName;
        DICOM_current_image.s_firstname = strongName.FirstName;
        DICOM_current_image.s_lastname = strongName.LastName;
        var age = dcm.FindFirst(TagHelper.PATIENT_AGE) as AgeString;
        DICOM_current_image.s_age = age.Data;
        //Debug.Log(DICOM_current_image.s_firstname + " " + DICOM_current_image.s_lastname + " " + DICOM_current_image.s_age);

        double sliceThickness = (double)dcm.FindFirst(TagHelper.SLICE_THICKNESS).DData;
        DICOM_current_image.d_sliceThickness = sliceThickness;
        double pixelSpacing = (double)dcm.FindFirst(TagHelper.PIXEL_SPACING).DData;
        DICOM_current_image.d_pixelSpacing = pixelSpacing;
        int instanceNumber = (int)dcm.FindFirst(TagHelper.INSTANCE_NUMBER).DData;
        DICOM_current_image.d_instanceNumber = instanceNumber;


        ///
        ///File.WriteAllText("Assets/_Ressources/Debug/" + s_folder_name + "-histogram_base_" + instanceNumber + ".txt", s_histo_base);
        ///


        // Create a columns*rows texture with PVRTC RGBA32 format
        // and fill it with raw PVRTC bytes.
        Texture2D tex = new Texture2D(columns, rows, TextureFormat.RGBA32, false);
        
        // Load data into the texture and upload it to the GPU.
        tex.LoadRawTextureData(outPixelData);

        // Set alpha value of the texture ((r+g+b)/3)
        int w = tex.width;
        int h = tex.height;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Color c = tex.GetPixel(x, y);
                c = new Color(c.r, c.g, c.b, (c.r + c.g + c.b) / 3);
                tex.SetPixel(x, y, c);
            }
        }       

        // Set the DICOM texture2D meta data
        DICOM_current_image.t2d_color_pixel_data = tex;

        // apply pixels change
        tex.Apply();

        return DICOM_current_image;
    }

    //a_DICOM_List_accessible
    public DICOM_datas_textures DICOM_data_textures_load_data_texture(List<DICOM_meta_data> l_DICOM_meta_data_in)
    {
        DICOM_datas_textures DICOM_datas_textures_out = new DICOM_datas_textures();

        int i_width = l_DICOM_meta_data_in[0].t2d_color_pixel_data.width;
        int i_height = l_DICOM_meta_data_in[0].t2d_color_pixel_data.height;
        int i_depth = l_DICOM_meta_data_in.Count - 1;

        float[,,] color_pixel_data_3D = new float[i_depth, i_height, i_width];

        for (int i = 0; i < i_depth; i++)
        {
            for (int j = 0; j < i_height; j++)
            {
                for (int k = 0; k < i_width; k++)
                {
                    color_pixel_data_3D[i, j, k] = l_DICOM_meta_data_in[i].t2d_color_pixel_data.GetPixel(k, j).a;
                }
            }
        }

        DICOM_datas_textures_out.t2d_color_pixel_data_1st_axis = new Texture2D[i_depth];
        for (int i = 0; i < i_depth; i++)
        {
            // Create a columns*rows texture with PVRTC RGBA32 format
            // and fill it with raw PVRTC bytes.
            Texture2D tex = new Texture2D(i_width, i_height, TextureFormat.RGBA32, false);
            for (int k = 0; k < i_width; k++)
            {
                for (int j = 0; j < i_height; j++)
                {
                    Color c = new Color(color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k]);
                    tex.SetPixel(k, j, c);
                }
            }
            tex.Apply();
            DICOM_datas_textures_out.t2d_color_pixel_data_1st_axis[i] = tex;
        }

        DICOM_datas_textures_out.t2d_color_pixel_data_2nd_axis = new Texture2D[i_width];
        for (int k = 0; k < i_width; k++)
        {
            // Create a columns*rows texture with PVRTC RGBA32 format
            // and fill it with raw PVRTC bytes.
            Texture2D tex = new Texture2D(i_depth, i_height, TextureFormat.RGBA32, false);
            for (int i = 0; i < i_depth; i++)
            {
                for (int j = 0; j < i_height; j++)
                {
                    Color c = new Color(color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k]);
                    tex.SetPixel(i, j, c);
                }
            }
            tex.Apply();
            DICOM_datas_textures_out.t2d_color_pixel_data_2nd_axis[k] = tex;
        }

        DICOM_datas_textures_out.t2d_color_pixel_data_3rd_axis = new Texture2D[i_height];
        for (int j = 0; j < i_height; j++)
        {
            // Create a columns*rows texture with PVRTC RGBA32 format
            // and fill it with raw PVRTC bytes.
            Texture2D tex = new Texture2D(i_width, i_depth, TextureFormat.RGBA32, false);
            for (int k = 0; k < i_width; k++)
            {
                for (int i = 0; i < i_depth; i++)
                {
                    Color c = new Color(color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k], color_pixel_data_3D[i, j, k]);
                    tex.SetPixel(k, i, c);
                }
            }
            tex.Apply();
            DICOM_datas_textures_out.t2d_color_pixel_data_3rd_axis[j] = tex;
        }

        return DICOM_datas_textures_out;
    }



    List<GameObject> l_GO_slices_all_axis_full_model = new List<GameObject>();
    List<GameObject> l_GO_slices_one_slice_per_axis = new List<GameObject>();

    public float f_slice_alpha_all_axis_full_model = 0.15f;
    float f_slice_alpha_all_axis_full_model_old = 0.15f;

    public float f_slice_alpha_one_slice_per_axis = 0.5f;
    float f_slice_alpha_one_slice_per_axis_old = 0.5f;

    void on_slice_alpha_changed(float f_slice_alpha_in, float f_slice_alpha_old_in, List<GameObject> l_GO_slices)
    {
        for (int i = 0; i < l_GO_slices.Count; i++)
        {
            Color col = l_GO_slices[i].GetComponent<Renderer>().material.GetColor("_Color");
            if ((f_slice_alpha_in > 0) && (f_slice_alpha_in <= 1))
            {
                col.a = col.a / f_slice_alpha_old_in * f_slice_alpha_in;
            }
            l_GO_slices[i].GetComponent<Renderer>().material.SetColor("_Color", col);
        }
    }




    public bool b_slice_alpha_all_axis_full_model = false;
    bool b_slice_alpha_all_axis_full_model_old = false;

    public bool b_slice_alpha_one_slice_per_axis = false;
    bool b_slice_alpha_one_slice_per_axis_old = false;

    void on_alpha_active_changed(bool b_slice_alpha_in, bool b_slice_alpha_old_in, List<GameObject> l_GO_slices_in)
    {
        string s_shader_to_use_name = "";
        if (b_slice_alpha_in == true)
        {
            s_shader_to_use_name = "Unlit/AlphaSelfIllum_alpha_1_forced_1";
        }
        else if (b_slice_alpha_in == false)
        {
            s_shader_to_use_name = "Unlit/AlphaSelfIllum";
        }

        for (int i = 0; i < l_GO_slices_in.Count; i++)
        {
            l_GO_slices_in[i].GetComponent<Renderer>().material.shader = Shader.Find(s_shader_to_use_name);
        }
    }





    List<GameObject> Create_texture_2D_3D_all_axis(DICOM_datas_textures DICOM_datas_textures_in, string s_GO_slices_along_all_axes_name, float f_slice_alpha)
    {
        List<GameObject> l_GO_slices = new List<GameObject>();

        var GO_slices_along_all_axes = new GameObject();
        GO_slices_along_all_axes.name = s_GO_slices_along_all_axes_name;
        {
            var GO_slice_Y = new GameObject();
            GO_slice_Y.name = "GO_slice_Y";
            GO_slice_Y.transform.parent = GO_slices_along_all_axes.transform;

            for (int i = 0; i < DICOM_datas_textures_in.t2d_color_pixel_data_1st_axis.Length; i++)
            {
                // use a bunch of memory!
                Texture2D texture2D_tex_2d = DICOM_datas_textures_in.t2d_color_pixel_data_1st_axis[i];
                texture2D_tex_2d.Apply();

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.parent = GO_slice_Y.transform;
                plane.name = i + "_slice_Y";
                Material myNewMaterial = new Material(Shader.Find("Unlit/AlphaSelfIllum"));
                plane.GetComponent<MeshRenderer>().material = myNewMaterial;

                //Color channelsIntensity = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                ///float f_scale_1px = 1.0f / (Mathf.Max(DICOM_datas_textures_in.t2d_color_pixel_data_1st_axis[i].width, DICOM_datas_textures_in.t2d_color_pixel_data_1st_axis[i].height));
                ///plane.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, 1, texture2D_tex_2d.height) * f_scale_1px;
                ///plane.transform.localPosition = new UnityEngine.Vector3(-plane.transform.localScale.x / 2, i * (float)a_DICOM_List_accessible[0].d_sliceThickness * f_scale_1px * 10 - (0.5f * (DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis.Length - 1) * (float)a_DICOM_List_accessible[0].d_sliceThickness * f_scale_1px * 10), -plane.transform.localScale.z / 2);
                plane.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, 1, texture2D_tex_2d.height);
                plane.transform.localPosition = new UnityEngine.Vector3(0, 10 * i /*(float)a_DICOM_List_accessible[0].d_sliceThickness * */  /* * 10*/ - 10 * (0.5f * (DICOM_datas_textures_in.t2d_color_pixel_data_1st_axis.Length-1)), 0);
                //GO_cube_t2d_Z.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, (float)l_DICOM_meta_data_in[i_slice].d_sliceThickness / (float)l_DICOM_meta_data_in[i_slice].d_pixelSpacing, texture2D_tex_2d.height) * f_scale_1px;
                Material material = plane.GetComponent<Renderer>().material;
                /*material.SetFloat("_Mode", 3);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;*/
                material.color = new Color(material.color.r, material.color.g, material.color.b, material.color.a * f_slice_alpha);
                material.SetTexture("_MainTex", texture2D_tex_2d);

                plane.GetComponent<Renderer>().material.GetTexture("_MainTex").filterMode = FilterMode.Point;

                l_GO_slices.Add(plane);
            }

            GO_slice_Y.transform.RotateAround(this.transform.position, this.transform.up, 180);
        }
        {
            var GO_slice_X = new GameObject();
            GO_slice_X.name = "GO_slice_X";
            GO_slice_X.transform.parent = GO_slices_along_all_axes.transform;

            for (int i = 0; i < DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis.Length; i++)
            {
                // use a bunch of memory!
                Texture2D texture2D_tex_2d = DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis[i];
                texture2D_tex_2d.Apply();

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.parent = GO_slice_X.transform;
                plane.name = i + "_slice_X";
                Material myNewMaterial = new Material(Shader.Find("Unlit/AlphaSelfIllum"));
                plane.GetComponent<MeshRenderer>().material = myNewMaterial;

                //Color channelsIntensity = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                ///float f_scale_1px = 1.0f / (Mathf.Max(DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis[i].width, DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis[i].height));
                ///plane.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, 1, texture2D_tex_2d.height) * f_scale_1px;
                ///plane.transform.localPosition = new UnityEngine.Vector3(-plane.transform.localScale.x / 2, i * (float)a_DICOM_List_accessible[0].d_sliceThickness * f_scale_1px * 10 - (0.5f * (DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis.Length - 1) * (float)a_DICOM_List_accessible[0].d_sliceThickness * f_scale_1px * 10), -plane.transform.localScale.z / 2);
                plane.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, 1, texture2D_tex_2d.height);
                plane.transform.localPosition = new UnityEngine.Vector3(-5, 10 * i /*(float)a_DICOM_List_accessible[0].d_sliceThickness * */  /* * 10*/ - 10 * (0.5f * (DICOM_datas_textures_in.t2d_color_pixel_data_2nd_axis.Length - 1)) + 5, 0);
                //GO_cube_t2d_Z.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, (float)l_DICOM_meta_data_in[i_slice].d_sliceThickness / (float)l_DICOM_meta_data_in[i_slice].d_pixelSpacing, texture2D_tex_2d.height) * f_scale_1px;
                Material material = plane.GetComponent<Renderer>().material;
                /*material.SetFloat("_Mode", 3);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;*/
                material.color = new Color(material.color.r, material.color.g, material.color.b, material.color.a * f_slice_alpha);
                material.SetTexture("_MainTex", texture2D_tex_2d);

                plane.GetComponent<Renderer>().material.GetTexture("_MainTex").filterMode = FilterMode.Point;

                l_GO_slices.Add(plane);
            }

            GO_slice_X.transform.RotateAround(this.transform.position, this.transform.up, 180);
            GO_slice_X.transform.RotateAround(this.transform.position, this.transform.forward, 90);

            GO_slice_X.transform.localScale = new UnityEngine.Vector3(1, -1, 1);
        }
        {
            var GO_slice_Z = new GameObject();
            GO_slice_Z.name = "GO_slice_Z";
            GO_slice_Z.transform.parent = GO_slices_along_all_axes.transform;

            for (int i = 0; i < DICOM_datas_textures_in.t2d_color_pixel_data_3rd_axis.Length; i++)
            {
                // use a bunch of memory!
                Texture2D texture2D_tex_2d = DICOM_datas_textures_in.t2d_color_pixel_data_3rd_axis[i];
                texture2D_tex_2d.Apply();

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.parent = GO_slice_Z.transform;
                plane.name = i + "_slice_Z";
                Material myNewMaterial = new Material(Shader.Find("Unlit/AlphaSelfIllum"));
                plane.GetComponent<MeshRenderer>().material = myNewMaterial;

                //Color channelsIntensity = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                //float f_scale_1px = 1.0f / (Mathf.Max(DICOM_datas_textures_in.t2d_color_pixel_data_3rd_axis[i].width, DICOM_datas_textures_in.t2d_color_pixel_data_3rd_axis[i].height));
                plane.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, 1, texture2D_tex_2d.height);
                plane.transform.localPosition = new UnityEngine.Vector3(0, 10 * i /*(float)a_DICOM_List_accessible[0].d_sliceThickness * */  /* * 10*/ - 10 * (0.5f * (DICOM_datas_textures_in.t2d_color_pixel_data_3rd_axis.Length - 1)) - 5, -5);
                //GO_cube_t2d_Z.transform.localScale = new UnityEngine.Vector3(texture2D_tex_2d.width, (float)l_DICOM_meta_data_in[i_slice].d_sliceThickness / (float)l_DICOM_meta_data_in[i_slice].d_pixelSpacing, texture2D_tex_2d.height) * f_scale_1px;
                Material material = plane.GetComponent<Renderer>().material;
                /*material.SetFloat("_Mode", 3);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;*/
                material.color = new Color(material.color.r, material.color.g, material.color.b, material.color.a * f_slice_alpha);
                material.SetTexture("_MainTex", texture2D_tex_2d);

                plane.GetComponent<Renderer>().material.GetTexture("_MainTex").filterMode = FilterMode.Point;

                l_GO_slices.Add(plane);
            }
            GO_slice_Z.transform.RotateAround(this.transform.position, this.transform.right, 90);
            GO_slice_Z.transform.localScale = new UnityEngine.Vector3(-1, 1, 1);
        }

        GO_slices_along_all_axes.transform.localScale = new UnityEngine.Vector3(0.00046f, 0.00046f, 0.00046f);
        //GO_slices_along_all_axes.transform.position = new UnityEngine.Vector3(-1, 1.25f, 0);

        l_GO_slices.Add(GO_slices_along_all_axes);

        return l_GO_slices;
    }


    void duplicate_GO(string s_GO_name_in, string s_GO_name_duplicate)
    {
        GameObject GO_duplicate_GO_slices_along_all_axes = Instantiate(GameObject.Find(s_GO_name_in));
        GO_duplicate_GO_slices_along_all_axes.transform.name = s_GO_name_duplicate;
        GO_duplicate_GO_slices_along_all_axes.transform.position = new UnityEngine.Vector3(0,1.25f,0);
    }
}