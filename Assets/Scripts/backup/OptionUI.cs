using CubeWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public GameObject optionPanel;

    public Toggle gridToggle;
    public Toggle lightToggle;

    public Button importButton;
    // Start is called before the first frame update

    // Update is called once per frame

    private void Start()
    {
        ActivateUI(true);
        Controller2.instance.inputManager.Player.Esc.performed += val =>
        {
            ToggleUI();
        };
        

        Controller2.instance.inputManager.Player.Grid.performed += val =>
        {
            gridToggle.isOn = !gridToggle.isOn;
        };
        Controller2.instance.inputManager.Player.LightShadow.performed += val =>
        {
            lightToggle.isOn = !lightToggle.isOn;
        };
#if UNITY_IOS
        //Controller3.instance.SetGamepadMode(true);
        importButton.gameObject.SetActive(true);
        importButton.onClick.AddListener(() =>
            {
                string[] extensionList = {"public.plain-text"};
                NativeFilePicker.PickFile((path) =>
                {
                    if(File.Exists(path))
                    {
                        byte[] data = File.ReadAllBytes(path);
                        if (data != null)
                        {
                            File.WriteAllBytes(Application.persistentDataPath + "/" + Path.GetFileName(path), data);
                        }
                    }
            }, extensionList);

    });
#endif
    }

    public void ToggleUI()
    {
        ActivateUI(!optionPanel.activeSelf);
    }

    void ActivateUI(bool activate)
    {
        optionPanel.SetActive(activate);
        //Cursor.visible = activate;
        //Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;

        if (activate)
        {
            List<ScrollData> list = ScrollUI.MakeDataList(new[] { "*.pa" }, (filePath) =>
            {
                PointArr pointArr = FileManager<PointArr>.OpenFile(filePath);

                Vector3Int mapSize = new Vector3Int
                {
                    x = pointArr.sizeX,
                    y = pointArr.sizeY,
                    z = pointArr.sizeZ
                };
                Vector4[] mapData = new Vector4[mapSize.x * mapSize.y * mapSize.z];
                for (int i = 0; i < pointArr.data.Count; i++)
                {
                    float r = pointArr.data[i].r / 256f;
                    float g = pointArr.data[i].g / 256f;
                    float b = pointArr.data[i].b / 256f;
                    mapData[pointArr.data[i].index] = new Vector4(r, g, b, 1);
                }
                Controller2.instance.StartRendering(mapSize, mapData);
                ActivateUI(false);
            });
            ScrollUI.instance.EnableUI(list);
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void SetWindowModeScreen()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        Controller2.instance.SetTexture();
    }

    public void SetFullModeScreen()
    {
        Screen.SetResolution(Screen.currentResolution.width,Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        Controller2.instance.SetTexture();
    }
}
