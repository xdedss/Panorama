using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CSPanorama
{
    class PanoramaController : MonoBehaviour
    {
        public bool showUI = false;
        public bool showErr = false;
        public string Errmsg = "";
        public float j = 0;

        bool test = false;
        string tests = "";

        Texture2D[] texs;
        Texture2D mergeTex;

        string ssFolder;
        string settingFile;
        string dateFormat;
        string div = "\\";

        string i_step = "100";
        string i_pass = "180";
        string i_filename = "myScreenshot";
        int windowWidth = 200;
        int windowHeight = 400;
        Texture2D m_samptex;
        Rect windowpos;
        Rect windowposErr;
        bool fullViewIn = false;
        bool fullViewFinished = false;

        bool k_shift = false;
        bool k_ctrl = true;
        bool k_alt = true;
        KeyCode k_ = KeyCode.P;

        private void Start()
        {
            ssFolder = Environment.CurrentDirectory + div + "ScreenShotP" + div;
            windowpos = new Rect(20, 20, windowWidth, windowHeight);
            windowposErr = new Rect(20, 20, windowWidth, windowHeight);

            CheckFolder();
            StartCoroutine(RefreshSamp());
        }

        private void Update()
        {
            if((shift || !k_shift)&&(ctrl || !k_ctrl)&&(alt || !k_alt)&& Input.GetKeyDown(k_))
            {
                showUI = !showUI;
            }

            if (fullViewFinished)
            {
                fullViewFinished = false;
                FileStream swr = File.Create(ssFolder + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + i_filename + ".png");
                byte[] imgbyte = mergeTex.EncodeToPNG();
                swr.Write(imgbyte, 0, imgbyte.Length);
                swr.Close();
            }
        }

        private void OnGUI()
        {
            if (!fullViewIn && showUI)
            {
                windowpos = GUI.Window(38291, windowpos, MakeWindow, "Panorama Settings");
            }
            if (!fullViewIn && showErr)
            {
                windowposErr = GUI.Window(38292, windowposErr, MakeWindowErr, "Panorama Error");
            }
        }
        private void MakeWindowErr(int id)
        {
            int y = 10;

            GUI.DragWindow(new Rect(0, y, windowWidth, 20));
            y += 20;

            GUI.Label(new Rect(0, y, windowWidth, 90), Errmsg);
            y += 90;

            if(GUI.Button(new Rect(0, y, windowWidth, 20), "Close"))
            {
                showErr = false;
            }
        }
        private void MakeWindow(int id)
        {
            int y = 10;

            try
            {
                GUI.DragWindow(new Rect(0, y, windowWidth, 20));
                y += 20;

                GUI.Label(new Rect(10, y, windowWidth - 20, 25), "step:                pixels");
                i_step = GUI.TextField(new Rect(45, y, 50, 20), i_step, 4);
                y += 20;
                i_step = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(10, y, windowWidth - 20, 10), int.Parse(i_step), 10, Screen.width)).ToString();
                y += 10;
                GUI.Label(new Rect(10, y, windowWidth - 20, 45), "Width of each shot.\nlower = better quality");
                y += 45;

                GUI.Label(new Rect(10, y, windowWidth - 20, 25), "width:                degrees");
                i_pass = GUI.TextField(new Rect(50, y, 50, 20), i_pass, 3);
                y += 20;
                i_pass = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(10, y, windowWidth - 20, 10), float.Parse(i_pass), 60, 360)).ToString();
                y += 10;

                GUI.Label(new Rect(10, y, windowWidth - 20, 25), "Total Width.(60°~360°)");
                y += 20;

                GUI.Label(new Rect(10, y, windowWidth - 20, 45), "camera:" + Camera.main.transform.position.ToString() + "\nstart angle:" + Camera.main.transform.eulerAngles.y + "°");
                y += 45;

                GUI.Label(new Rect(10, y, windowWidth - 20, 25), "Preview:");
                y += 20;
                GUI.Label(new Rect(10, y, windowWidth - 20, windowWidth - 20), m_samptex);
                y += windowWidth - 20;

                GUI.Label(new Rect(10, y, windowWidth - 20, 25), "file name:");
                y += 20;
                i_filename = GUI.TextField(new Rect(10, y, windowWidth - 20, 25), i_filename);
                y += 30;
                if (!IsFileNameValid(i_filename))
                {
                    throw new FormatException("File name is invalid:  " + i_filename);
                }

                //test = GUI.Toggle(new Rect(10, y, windowWidth - 20, 25), test, "test");
                //y += 20;

                //tests = GUI.TextArea(new Rect(10, y, windowWidth - 20, 100), tests);
                //y += 100;

                if (GUI.Button(new Rect(10, y, windowWidth - 20, 20), "Open Folder"))
                {
                    System.Diagnostics.Process.Start("explorer.exe", ssFolder);
                    //Debug.Log(Environment.CurrentDirectory);
                }
                y += 30;

                if (GUI.Button(new Rect(10, y, windowWidth - 20, 20), "Start"))
                {
                    if (!fullViewIn)
                    {
                        StartCoroutine(FullView(Camera.main.transform.eulerAngles.y, int.Parse(i_pass), int.Parse(i_step)));
                    }
                }
                y += 30;
            }
            catch (Exception e)
            {
                //Debug.Log(e);
                GUI.Label(new Rect(10, y, windowWidth - 20, 25), "WRONG FORMAT!");
                y += 25;
            }
            windowpos.height = y;
        }

        void CheckFolder()
        {
            if (!Directory.Exists(ssFolder))
            {
                Directory.CreateDirectory(ssFolder);
            }
            if (!File.Exists(ssFolder + "hotkey.txt"))
            {
                StreamWriter optfile = File.CreateText(ssFolder + "hotkey.txt");
                optfile.Write((k_ctrl ? "control+" : "") + (k_alt ? "alt+" : "") + (k_shift ? "shift+" : "") + k_.ToString());
                optfile.Close();
            }
            else
            {
                StreamReader optfile = File.OpenText(ssFolder + "hotkey.txt");
                string[] info = optfile.ReadLine().Split('+');
                optfile.Close();
                k_shift = false;
                k_ctrl = false;
                k_alt = false;
                for(int infoi = 0;infoi < info.Length; infoi++)
                {
                    string thiskey = info[infoi];
                    if(thiskey == "shift")
                    {
                        k_shift = true;
                    }
                    else if(thiskey == "control")
                    {
                        k_ctrl = true;
                    }
                    else if(thiskey == "alt")
                    {
                        k_alt = true;
                    }
                    else
                    {
                        bool kcfound = false;
                        foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode))){
                            if(thiskey == kc.ToString())
                            {
                                k_ = kc;
                                kcfound = true;
                                break;
                            }
                        }
                        if (!kcfound)
                        {
                            showErr = true;
                            Errmsg = "Can't find key:\n" + thiskey + "\nReset to default:\nalt+control+P";

                        }
                    }
                }
            }
        }

        bool shift
        {
            get
            {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }
        bool ctrl
        {
            get
            {
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            }
        }
        bool alt
        {
            get
            {
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            }
        }

        IEnumerator FullView(float startAngle, float passAngle, int step)
        {
            fullViewIn = true;

            TextureFormat texf = TextureFormat.RGB24;

            Transform camt = Camera.main.transform;
            camt.eulerAngles = new Vector3(0, startAngle, 0);

            int sch = Screen.height;
            int halfStep = step / 2;
            int halfWidth = Screen.width / 2;
            float passRad = passAngle * Mathf.Deg2Rad;
            float halfFOV = Camera.main.fieldOfView / 2;
            float dist = (float)sch / 2 / Mathf.Tan(halfFOV * Mathf.Deg2Rad);
            float halfRad = Mathf.Atan((float)halfStep / dist);
            int move = Mathf.CeilToInt(passRad / (halfRad * 2));
            int lastPxl = Mathf.CeilToInt(Mathf.Tan(passRad % (halfRad * 2) - halfRad) * dist + (float)halfStep);
            
            texs = new Texture2D[move];
            mergeTex = new Texture2D((move - 1) * halfStep * 2 + lastPxl, sch, texf, false);

            Camera.main.GetComponent<CameraController>().enabled = false;
            //Camera m_ori_cam = Camera.main;
            //Camera m_alt_cam = Instantiate(m_ori_cam.gameObject).GetComponent<Camera>();
            //Destroy(m_alt_cam.GetComponent<CameraController>());
            //StreamWriter dbgsw = File.CreateText(ssFolder + "dbg.txt");
            //if (test)
            //{
            //Component[] mb = m_alt_cam.GetComponents(typeof(MonoBehaviour));
            //foreach (Component tmb in mb)
            //{
            //    if (tests.Contains(tmb.ToString()))
            //    {
            //        Destroy(tmb);
            //    }
            //    else
            //    {
            //        dbgsw.WriteLine(tmb.ToString());
            //    }
            //}
            //}
            //dbgsw.Close();
            //int m_ori_disp = m_ori_cam.targetDisplay;
            //m_ori_cam.targetDisplay = 1;
            //m_alt_cam.targetDisplay = 0;
            //m_alt_cam.gameObject.tag = "Untagged";
            for (int movei = 0; movei < move - 1; movei++)
            {
                yield return new WaitForEndOfFrame();

                Vector3 ceu = camt.eulerAngles;
                ceu.y += halfRad * 2 * Mathf.Rad2Deg;
                camt.eulerAngles = ceu;
                //Vector3 ceu = m_alt_cam.transform.eulerAngles;
                //ceu.y += halfRad * 2 * Mathf.Rad2Deg;
                //m_alt_cam.transform.eulerAngles = ceu;

                texs[movei] = new Texture2D(halfStep * 2, sch, texf, false);
                texs[movei].ReadPixels(new Rect(halfWidth - halfStep, 0, halfStep * 2, sch), 0, 0);
                texs[movei].Apply();
                mergeTex.SetPixels(movei * halfStep * 2, 0, halfStep * 2, sch, texs[movei].GetPixels());
                mergeTex.Apply();
            }
            yield return new WaitForEndOfFrame();
            texs[move - 1] = new Texture2D(lastPxl, sch);
            texs[move - 1].ReadPixels(new Rect(halfWidth - halfStep + 1, 0, lastPxl, sch), 0, 0);
            texs[move - 1].Apply();
            mergeTex.SetPixels((move - 1) * halfStep * 2, 0, lastPxl, sch, texs[move - 1].GetPixels());
            yield return 0;
            mergeTex.Apply();

            Vector3 ceu_ = camt.eulerAngles;
            ceu_.y = startAngle;
            camt.eulerAngles = ceu_;

            //Destroy(m_alt_cam);
            //m_ori_cam.targetDisplay = m_ori_disp;

            Camera.main.GetComponent<CameraController>().enabled = true;

            fullViewIn = false;
            fullViewFinished = true;
        }

        IEnumerator RefreshSamp()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                if (showUI)
                {
                    try
                    {
                        m_samptex = Samptex(windowWidth - 20);
                    }
                    catch (Exception e)
                    {
                        //Debug.Log(e);
                    }
                }
            }
        }
        private Texture2D Samptex(int d)
        {
            int sch = Screen.height;
            int halfStep = int.Parse(i_step) / 2;
            int halfWidth = Screen.width / 2;
            float passRad = int.Parse(i_pass) * Mathf.Deg2Rad;
            float halfFOV = Camera.main.fieldOfView / 2;
            float dist = (float)sch / 2 / Mathf.Tan(halfFOV * Mathf.Deg2Rad);
            float halfRad = Mathf.Atan((float)halfStep / dist);
            int move = Mathf.CeilToInt(passRad / (halfRad * 2));
            int lastPxl = Mathf.CeilToInt(Mathf.Tan(passRad % (halfRad * 2) - halfRad) * dist + (float)halfStep);
            float r = (float)d / 2;
            float ind = (float)d / 8 * 3;

            Texture2D samptex = new Texture2D(d, d);
            ClearTex(samptex, new Color(1, 1, 1, 0));
            float startRad = 0;
            float startX = r;
            float startY = r + ind;
            float endRad = startRad;
            float endX = startX;
            float endY = startY;
            for (int movei = 0; movei < move; movei++)
            {
                if (movei != 0)
                {
                    endRad = halfRad * 2 * movei;
                    endX = r + Mathf.Sin(endRad) * ind;
                    endY = r + Mathf.Cos(endRad) * ind;
                    DrawLine(samptex, startX, startY, endX, endY, 0.3f, new Color(0.9f, 0.9f, 0.9f, 1));
                    DrawLine(samptex, startX, startY, r, r, 0.3f, new Color(0.9f, 0.9f, 0.9f, 0.5f));
                    Plot(samptex, Mathf.RoundToInt(startX), Mathf.RoundToInt(startY), 2, Color.white);
                    startRad = endRad;
                    startX = endX;
                    startY = endY;
                }
                else
                {
                    startRad = halfRad * 2 * movei;
                    startX = r + Mathf.Sin(startRad) * ind;
                    startY = r + Mathf.Cos(startRad) * ind;
                }
            }
            float lastRad = halfRad * 2 * move;
            float lastX = r + Mathf.Sin(lastRad) * ind;
            float lastY = r + Mathf.Cos(lastRad) * ind;
            float perc = (float)lastPxl / (halfStep * 2);
            lastX = endX * (1 - perc) + lastX * perc;
            lastY = endY * (1 - perc) + lastY * perc;
            DrawLine(samptex, lastX, lastY, endX, endY, 0.3f, new Color(0.9f, 0.9f, 0.9f, 1));
            DrawLine(samptex, lastX, lastY, r, r, 0.3f, new Color(0.9f, 0.9f, 0.9f, 0.5f));
            DrawLine(samptex, endX, endY, r, r, 0.3f, new Color(0.9f, 0.9f, 0.9f, 0.5f));
            Plot(samptex, Mathf.RoundToInt(endX), Mathf.RoundToInt(endY), 2, Color.white);

            samptex.Apply();
            return samptex;
        }
        private void ClearTex(Texture2D tex, Color c)
        {
            for (int xi = 0; xi < tex.width; xi++)
            {
                for (int yi = 0; yi < tex.height; yi++)
                {
                    tex.SetPixel(xi, yi, c);
                }
            }
        }
        private void Plot(Texture2D tex, int x, int y, int d, Color c)
        {
            for (int xi = x - d + 1; xi <= x + d; xi++)
            {
                for (int yi = y - d + 1; yi <= y + d; yi++)
                {
                    tex.SetPixel(xi, yi, c);
                }
            }
        }
        private void DrawLine(Texture2D tex, float x1, float y1, float x2, float y2, float step, Color c)
        {
            if (x1 > x2)
            {
                x1 += x2;
                x2 = x1 - x2;
                x1 -= x2;
                y1 += y2;
                y2 = y1 - y2;
                y1 -= y2;
            }
            if (x1 != x2)
            {
                for (float x = x1; x <= x2; x += step)
                {
                    tex.SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt((x - x1) / (x2 - x1) * (y2 - y1) + y1), c);
                }
            }
            if (y1 > y2)
            {
                x1 += x2;
                x2 = x1 - x2;
                x1 -= x2;
                y1 += y2;
                y2 = y1 - y2;
                y1 -= y2;
            }
            if (y1 != y2)
            {
                for (float y = y1; y <= y2; y += step)
                {
                    tex.SetPixel(Mathf.RoundToInt((y - y1) / (y2 - y1) * (x2 - x1) + x1), Mathf.RoundToInt(y), c);
                }
            }
        }

        private bool IsFileNameValid(string name)
        {
            bool res = true;
            string[] errorStr = new string[] { "/", "\\", ":", ",", "*", "?", "\"", "<", ">", "|" };

            if (string.IsNullOrEmpty(name))
            {
                res = false;
            }
            else
            {
                for (int i = 0; i < errorStr.Length; i++)
                {
                    if (name.Contains(errorStr[i]))
                    {
                        res = false;
                        break;
                    }
                }
            }
            return res;
        }

    }
}
