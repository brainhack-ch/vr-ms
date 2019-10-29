using Hover.Core.Items.Types;
using Hover.Core.Renderers.Items.Buttons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class InteractionsScript : MonoBehaviour {

    public SteamVR_TrackedControllerModified RightController;
    public SteamVR_TrackedControllerModified LeftController;

    public GameObject RightHandFingerOculus;
    public GameObject RightHandFingerVive;
    public GameObject LeftHandHoverCastOculus;
    public GameObject LeftHandHoverCastVive;

    //[HideInInspector]
    public GameObject Finger;
    public GameObject HoverCast;

    [HideInInspector]
    public bool RayCast = false;
    [HideInInspector]
    public bool RayCast2 = false;

    [HideInInspector]
    public Vector3 RayCastStartPosition;
    [HideInInspector]
    public Vector3 RayCastEndPosition;

    public HoverItemDataSlider MarkersRedSlider;
    public HoverItemDataSlider MarkersGreenSlider;
    public HoverItemDataSlider MarkersBlueSlider;
    public HoverItemDataSlider MarkersSizeSlider;
    public HoverItemDataSlider FullModelRedSlider;
    public HoverItemDataSlider FullModelGreenSlider;
    public HoverItemDataSlider FullModelBlueSlider;

    private RaycastHit hit;
    private LineRenderer line;
    private bool reset = true;
    private bool reset2 = true;

    [HideInInspector]
    public string objName;
    [HideInInspector]
    public float[] MarkersRGB = new float[3];
    [HideInInspector]
    public float MarkersSize = 255; // alpha
    [HideInInspector]
    public float[] FullModelRGB = new float[3];
    [HideInInspector]
    public float[] FullModelRGBBase = new float[3];
    [HideInInspector]
    public float[] FullModelRGBSave = new float[3];
    [HideInInspector]
    public float[] FullModelRGBCurrent = new float[3];
    [HideInInspector]
    public float RatioDistanceRayCast2 = 0.05f;


    public GameObject steamVRObject;

    public int Hardware = -1;

    public bool LeftController_ispadPressed;
    public bool LeftController_istriggerPressed;

    // Use this for initialization
    void Start () {
        // Add a Line Renderer to the GameObject
        line = this.gameObject.AddComponent<LineRenderer>();
        // Set the width of the Line Renderer
        line.SetWidth(0.001f, 0.001f);
        // Set the number of vertex fo the Line Renderer
        line.SetVertexCount(2);

        this.gameObject.GetComponent<LineRenderer>().enabled = false;

        MarkersRGB[0] = 255;
        MarkersRGB[1] = 0;
        MarkersRGB[2] = 0;
        MarkersSize = 255;
        FullModelRGB[0] = 0;
        FullModelRGB[1] = 0;
        FullModelRGB[2] = 0;
        FullModelRGBBase[0] = 0;
        FullModelRGBBase[1] = 0;
        FullModelRGBBase[2] = 0;
        FullModelRGBSave[0] = 0;
        FullModelRGBSave[1] = 0;
        FullModelRGBSave[2] = 0;          
    }

    // Update is called once per frame
    void Update () {


        if (Hardware == -1)
        {
            var vr = SteamVR.instance;
            string TrackingSystemName = "";
            if (vr != null)
                TrackingSystemName = vr.hmd_TrackingSystemName;

            if (TrackingSystemName.Contains("oculus"))
            {
                Hardware = 0;
                Finger.transform.position = RightHandFingerOculus.transform.position;
                Finger.transform.rotation = RightHandFingerOculus.transform.rotation;
                HoverCast.transform.position = LeftHandHoverCastOculus.transform.position;
                HoverCast.transform.rotation = LeftHandHoverCastOculus.transform.rotation;
            }
            else if (TrackingSystemName.Contains("lighthouse"))
            {
                Hardware = 1;
                Finger.transform.position = RightHandFingerVive.transform.position;
                Finger.transform.rotation = RightHandFingerVive.transform.rotation;
                HoverCast.transform.position = LeftHandHoverCastVive.transform.position;
                HoverCast.transform.rotation = LeftHandHoverCastVive.transform.rotation;
            }
        }


        if (Hardware == 0)
        {
            if (RayCast)
            {
                Physics.Raycast(Finger.transform.position, Finger.transform.forward, out hit, Mathf.Infinity);

                objName = hit.transform.gameObject.name;


                RayCastStartPosition = Finger.transform.position;
                RayCastEndPosition = hit.point;

                line.SetPosition(0, RayCastStartPosition);
                line.SetPosition(1, RayCastEndPosition);

                if (RightController.menuAPressed)
                {
                    if (reset == true)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.localScale = new Vector3(MarkersSize, MarkersSize, MarkersSize);
                        sphere.transform.position = RayCastEndPosition;

                        sphere.GetComponent<Renderer>().material.color = new Color(MarkersRGB[0], MarkersRGB[1], MarkersRGB[2]);
                    }
                    reset = false;
                }
                else
                {
                    reset = true;
                }

                if (RightController.menuPressed)
                {
                    if (reset2 == true)
                    {
                        if (hit.transform.gameObject.name == "Sphere")
                        {
                            Destroy(hit.transform.gameObject);
                        }
                    }
                    reset2 = false;
                }
                else
                {
                    reset2 = true;
                }
            }

            if (RayCast2)
            {
                RayCastStartPosition = Finger.transform.position;
                RayCastEndPosition = Finger.transform.position + Finger.transform.forward * RatioDistanceRayCast2;

                line.SetPosition(0, RayCastStartPosition);
                line.SetPosition(1, RayCastEndPosition);

                if (RightController.menuAPressed)
                {
                    //if (reset == true)
                    {
                        /*GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.localScale = new Vector3(MarkersSize, MarkersSize, MarkersSize);
                        sphere.transform.position = RayCastEndPosition;

                        sphere.GetComponent<Renderer>().material.color = new Color(MarkersRGB[0], MarkersRGB[1], MarkersRGB[2]);*/
                        GameObject.Find("_VolumetricDataRendering/Pointer").transform.position = RayCastEndPosition;
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().try_add_cube();
                    }
                    //reset = false;
                }
                else
                {
                    reset = true;
                }

                if (RightController.menuPressed)
                {
                    //if (reset2 == true)
                    {
                        /*if (Physics.Raycast(RayCastStartPosition, Finger.transform.forward, out hit, Vector3.Distance(RayCastStartPosition, RayCastEndPosition)))
                        {
                            if (hit.transform.gameObject.name == "Sphere")
                            {
                                Destroy(hit.transform.gameObject);
                            }
                        }*/
                        GameObject.Find("_VolumetricDataRendering/Pointer").transform.position = RayCastEndPosition;
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().try_remove_cube();

                    }
                    //reset2 = false;
                }
                else
                {
                    reset2 = true;
                }

                if (LeftController.menuAPressed)
                {
                    GameObject.Find("_VolumetricDataRendering/Pointer_2_choose_slices").transform.position = LeftController.transform.position;
                    GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().update_current_slices();
                }

                if ((LeftController.padPressed) && (LeftController_ispadPressed == false))
                {
                    LeftController_ispadPressed = true;

                    float angle_1 = Vector3.Angle(LeftController.transform.GetChild(1).transform.forward, transform.up);
                    float angle_2 = Vector3.Angle(LeftController.transform.GetChild(1).transform.forward, -transform.up);
                    float angle_3 = Vector3.Angle(LeftController.transform.GetChild(1).transform.forward, transform.right);
                    float angle_4 = Vector3.Angle(LeftController.transform.GetChild(1).transform.forward, -transform.right);
                    float angle_5 = Vector3.Angle(LeftController.transform.GetChild(1).transform.forward, transform.forward);
                    float angle_6 = Vector3.Angle(LeftController.transform.GetChild(1).transform.forward, -transform.forward);

                    float min_angle = Mathf.Min(angle_1, angle_2, angle_3, angle_4, angle_5, angle_6);

                    if (min_angle == angle_1)
                    {
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().i_curr_active_1st_slice += 1;
                    }
                    else if (min_angle == angle_2)
                    {
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().i_curr_active_1st_slice -= 1;
                    }
                    else if (min_angle == angle_3)
                    {
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().i_curr_active_2nd_slice += 1;
                    }
                    else if (min_angle == angle_4)
                    {
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().i_curr_active_2nd_slice -= 1;
                    }
                    else if (min_angle == angle_5)
                    {
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().i_curr_active_3rd_slice += 1;
                    }
                    else if (min_angle == angle_6)
                    {
                        GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().i_curr_active_3rd_slice -= 1;
                    }
                }
                if (!LeftController.padPressed)
                {
                    LeftController_ispadPressed = false;
                }

                if ((LeftController.triggerPressed) && (LeftController_istriggerPressed == false))
                {
                    LeftController_istriggerPressed = true;
                    GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().b_slice_alpha_one_slice_per_axis = !GameObject.Find("_DICOM_Manager").GetComponent<_DicomManager>().b_slice_alpha_one_slice_per_axis;
                }
                else if (!LeftController.triggerPressed)
                {
                    LeftController_istriggerPressed = false;
                }
            }
        }

        if (Hardware == 1)
        {
            if (RayCast)
            {
                Physics.Raycast(Finger.transform.position, Finger.transform.forward, out hit, Mathf.Infinity);

                objName = hit.transform.gameObject.name;


                RayCastStartPosition = Finger.transform.position;
                RayCastEndPosition = hit.point;

                line.SetPosition(0, RayCastStartPosition);
                line.SetPosition(1, RayCastEndPosition);

                if (RightController.triggerPressed)
                {
                    if (reset == true)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.localScale = new Vector3(MarkersSize, MarkersSize, MarkersSize);
                        sphere.transform.position = RayCastEndPosition;

                        sphere.GetComponent<Renderer>().material.color = new Color(MarkersRGB[0], MarkersRGB[1], MarkersRGB[2]);
                    }
                    reset = false;
                }
                else
                {
                    reset = true;
                }

                if (RightController.gripped)
                {
                    if (reset2 == true)
                    {
                        if (hit.transform.gameObject.name == "Sphere")
                        {
                            Destroy(hit.transform.gameObject);
                        }
                    }
                    reset2 = false;
                }
                else
                {
                    reset2 = true;
                }
            }

            if (RayCast2)
            {
                RayCastStartPosition = Finger.transform.position;
                RayCastEndPosition = Finger.transform.position + Finger.transform.forward * RatioDistanceRayCast2;

                line.SetPosition(0, RayCastStartPosition);
                line.SetPosition(1, RayCastEndPosition);

                if (RightController.triggerPressed)
                {
                    if (reset == true)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.localScale = new Vector3(MarkersSize, MarkersSize, MarkersSize);
                        sphere.transform.position = RayCastEndPosition;

                        sphere.GetComponent<Renderer>().material.color = new Color(MarkersRGB[0], MarkersRGB[1], MarkersRGB[2]);
                    }
                    reset = false;
                }
                else
                {
                    reset = true;
                }

                if (RightController.gripped)
                {
                    if (reset2 == true)
                    {
                        if (Physics.Raycast(RayCastStartPosition, Finger.transform.forward, out hit, Vector3.Distance(RayCastStartPosition, RayCastEndPosition)))
                        {
                            if (hit.transform.gameObject.name == "Sphere")
                            {
                                Destroy(hit.transform.gameObject);
                            }
                        }
                    }
                    reset2 = false;
                }
                else
                {
                    reset2 = true;
                }
            }
        }

            
    }



    public void ChangeModeToHandsMode()
    {
        RayCast = false;
        RayCast2 = false;
        this.gameObject.GetComponent<LineRenderer>().enabled = false;
    }
    public void ChangeModeToMarkersMode()
    {
        RayCast = true;
        RayCast2 = false;
        this.gameObject.GetComponent<LineRenderer>().enabled = true;
    }
    public void ChangeModeToMarkers3DMode()
    {
        RayCast = false;
        RayCast2 = true;
        this.gameObject.GetComponent<LineRenderer>().enabled = true;
    }

    public void UpdateMarkersR()
    {
        MarkersRGB[0] = MarkersRedSlider.Value;
    }
    public void UpdateMarkersG()
    {
        MarkersRGB[1] = MarkersGreenSlider.Value;
    }
    public void UpdateMarkersB()
    {
        MarkersRGB[2] = MarkersBlueSlider.Value;
    }
    public void UpdateMarkersSize()
    {
        MarkersSize = MarkersSizeSlider.Value;
    }

    public void SaveFullModelColor()
    {
        FullModelRGBSave[0] = FullModelRedSlider.Value;
        FullModelRGBSave[1] = FullModelGreenSlider.Value;
        FullModelRGBSave[2] = FullModelBlueSlider.Value;
    }
    public void RestoreFullModelColor()
    {
        FullModelRedSlider.Value = FullModelRGBSave[0];
        FullModelGreenSlider.Value = FullModelRGBSave[1];
        FullModelBlueSlider.Value = FullModelRGBSave[2];
    }
    public void ResetFullModelColor()
    {
        FullModelRedSlider.Value = FullModelRGBBase[0];
        FullModelGreenSlider.Value = FullModelRGBBase[1];
        FullModelBlueSlider.Value = FullModelRGBBase[2];
    }
}
